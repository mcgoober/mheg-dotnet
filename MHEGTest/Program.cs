/* 
 *  Copyright (C) 2008 Jason Leonard
 * 
 *  Work based on libmythfreemheg part of mythtv (www.mythtv.org)
 *  Copyright (C) 2004 David C. J. Matthews
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public License
 *  as published by the Free Software Foundation; either version 2
 *  of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 *  Or, point your browser to http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.Text;
using MHEG;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;

namespace MHEGTest
{
    public class MHEGForm : Form
    {
        private Queue<int> buttonQueue;
        private Timer timer;
        public MHEGForm()
        {
            buttonQueue = new Queue<int>();
            timer = new Timer();
            timer.Interval = 100;
            timer.Tick += new EventHandler(tmrTimer_Tick);
            timer.Start();

            InitialiseComponents();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.UserPaint | 
                ControlStyles.DoubleBuffer, true);
        }

        private void tmrTimer_Tick(object sender, System.EventArgs e)
        {
            processEngine();
        }

        private void processEngine()
        {
            if (Program.resetengine)
            {
                Program.mhegengine.SetBooting();
                Program.resetengine = false;
            }
            if (System.Console.KeyAvailable)
            {
                int key = System.Console.In.Read();
                char ch = (char)key;
                if (ch == 'a')
                {
                    Program.bitmap.Save("..\\..\\..\\mhegShot.png");
                }
                if (ch == 'b')
                {
                    Program.mhegengine.PrintCurrentApp(Console.Out);
                }
            }
            if (buttonQueue.Count > 0)
            {
                Program.mhegengine.GenerateUserAction(buttonQueue.Dequeue());
            }
            int toWait = Program.mhegengine.RunAll();
            if (toWait < 0)
            {
                timer.Stop();
                toWait = 100;
            }
            else if (toWait == 0)
            {
                toWait = 1;
            }
            timer.Interval = toWait;
            //Console.Out.WriteLine("Wait: " + toWait);
            //Invalidate();

        }

        private void form_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // Create a local version of the graphics object for the PictureBox.
            Graphics g = e.Graphics;
            g.DrawImageUnscaled(Properties.Resources.Buttons, 0, 0);
            g.DrawImage(Program.bitmap, 306, 0, 720, 576);
        }

        private void on_click(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < (buttonCoords.Length / 6); i++)
            {
                if (e.X > buttonCoords[i, 1] && e.X < buttonCoords[i, 3] &&
                    e.Y > buttonCoords[i, 2] && e.Y < buttonCoords[i, 4])
                {
                    int code = buttonCoords[i, 0];
                    if (code == 1000)
                    {
                        Logging.SetLoggingLevel(Logging.MHLogAll);
                    }
                    else if (code == 1001)
                    {
                        Logging.SetLoggingLevel(Logging.MHLogError | Logging.MHLogWarning);
                    }
                    else if (code == 1002)
                    {
                        FolderBrowserDialog dialog = new FolderBrowserDialog();
                        DialogResult result = dialog.ShowDialog();
                        if (result.ToString().Equals("OK"))
                        {
                            Program.directory = dialog.SelectedPath;
                            Program.resetengine = true;
                            timer.Start();
                        }
                    }
                    else
                    {
                        int reg = buttonCoords[i, 5];
                        if ((Program.inputRegister == 5 && (reg == 3 && reg == 5)) ||
                                (Program.inputRegister == 3 && reg == 3) ||
                                Program.inputRegister == 4)
                        {
                            buttonQueue.Enqueue(code);
                            timer.Interval = 1;
                        }
                    }
                }
            }
        }

        public void InitialiseComponents()
        {
            this.Paint += new PaintEventHandler(this.form_Paint);
            this.MouseClick += new MouseEventHandler(this.on_click);
        }

        public static int[,] buttonCoords =
        {
            // Code, x1, y1, x2, y2, register
            {1, 189, 220, 239, 251, 5}, // Up
            {2, 189, 289, 239, 318, 5}, // Down
            {3, 137, 254, 186, 285, 5}, // Left
            {4, 242, 254, 292, 285, 5}, // Right

            {6, 136, 31, 186, 62, 4}, // 1
            {7, 190, 31, 239, 62, 4}, // 2
            {8, 242, 31, 292, 62, 4}, // 3
            {9, 136, 65, 186, 96, 4}, // 4
            {10, 190, 65, 239, 96, 4}, // 5
            {11, 242, 65, 292, 96, 4}, // 6
            {12, 136, 99, 186, 129, 4}, // 7
            {13, 190, 99, 239, 129, 4}, // 8
            {14, 242, 99, 292, 129, 4}, // 9
            {5, 190, 133, 239, 163, 4}, // 0
            
            {15, 189, 254, 239, 287, 5}, // Select
            {16, 31, 376, 80, 406, 3}, // Cancel

            {100, 84, 413, 113, 443, 3}, // Red
            {101, 136, 413, 186, 443, 3}, // Green
            {102, 190, 413, 240, 443, 3}, // Yellow
            {103, 243, 413, 292, 443, 3}, // Blue

            {104, 31, 413, 80, 443, 3}, // Text

            // Other controls not part of the UK Profile.
            {1000, 5, 458, 72, 488, 0}, // Debug On
            {1001, 81, 458, 152, 488, 0}, // Debug Off
            {1002, 255, 457, 299, 488, 0}, // Root
        };
    }

    public class Program
    {
        public static Bitmap bitmap;
        public static Graphics graphics;
        public static IMHEG mhegengine;
        public static MHEGForm form;
        public static int inputRegister;
        public static string directory;
        public static bool resetengine;
        public static string fontname;

        [STAThread]
        public static void Main(string[] args)
        {
            Console.WriteLine("MHEG implementation test harness");
            Console.WriteLine("Type a<enter> to create mhegShot.png");

            Logging.Initialise();
            Logging.SetLoggingLevel(Logging.MHLogError | Logging.MHLogWarning);
            //Logging.SetLoggingLevel(Logging.MHLogAll);

            bitmap = new Bitmap(720, 576);
            bitmap.SetResolution(58, 72);
            graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Black);

            if (HasFont("TiresiasScreenfont"))
            {
                fontname = "TiresiasScreenfont";
            }
            else if (HasFont("Tioga"))
            {
                fontname = "Tioga";
            }
            else
            {
                Logging.Log(Logging.MHLogWarning, "WARNING: Unable to find Tiresias compatiable font");
                fontname = FontFamily.GenericSansSerif.Name;
            }
            Logging.Log(Logging.MHLogDetail, "Using font '" + fontname + "'");


            inputRegister = 0;
            resetengine = false;
            directory = "..\\..\\examples\\BBCONE";
            mhegengine = MHEGEngineFactory.CreateEngine(new TestContext());            
            mhegengine.SetBooting();

            form = new MHEGForm();
            form.ClientSize = new Size(720 + 306, 576);
            form.Show();
            form.Focus();
            Application.Run(form);

            Logging.Close();
            Console.WriteLine("Goodbye");
        }

        public static bool HasFont(string font)
        {
            foreach (FontFamily f in FontFamily.Families)
            {
                if (f.Name.Equals(font))
                {
                    return true;
                }
                
            }
            return false;
        }
    }
    
    class TestContext : IMHContext
    {
        // Test for an object in the carousel.  Returns true if the object is present and
        // so a call to GetCarouselData will not block and will return the data.
        // Returns false if the object is not currently present because it has not
        // yet appeared and also if it is not present in the containing directory.
        public bool CheckCarouselObject(string objectPath)
        {
            return File.Exists(Program.directory + "\\" + objectPath);
        }

        public bool GetCarouselData(string objectPath, out byte[] result)
        {
            if (objectPath[0] == '~')
            {
                objectPath = objectPath.Substring(2);
            }
            objectPath.Replace('/', '\\');
            try
            {
                BinaryReader br = new BinaryReader(File.Open(Program.directory + "\\" + objectPath, FileMode.Open, FileAccess.Read, FileShare.Read));
                byte[] data = br.ReadBytes((int)br.BaseStream.Length);
                br.Close();
                result = data;
                Logging.Log(Logging.MHLogDetail, "Context - GetCarouselData: " + objectPath);
            }
            catch (FileNotFoundException)
            {
                result = null;
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                result = null;
                return false;
            }
            return true;
        }

        public void SetInputRegister(int nReg)
        {
            Logging.Log(Logging.MHLogDetail, "Context - SetInputRegister: nReg = " + nReg);
            Program.inputRegister = nReg;
        }

        public void RequireRedraw(Region region)
        {
            Program.graphics.Clear(Color.Black);
            Program.mhegengine.DrawDisplay(new Region(new Rectangle(0, 0, 768, 576)));
            Program.form.Invalidate();
        }


        public IMHDLADisplay CreateDynamicLineArt(bool isBoxed, MHRgba lineColour, MHRgba fillColour)
        {
            return new TestDLADisplay();
        }

        public IMHTextDisplay CreateText()
        {
            return new TestTextDisplay();
        }

        public IMHBitmapDisplay CreateBitmap(bool tiled)
        {
            return new TestBitmapDisplay();
        }

        public void DrawRect(int xPos, int yPos, int width, int height, MHRgba colour)
        {
            Color c = colour.ToColor();
            Logging.Log(Logging.MHLogDetail, "Context - DrawRect: xPos = " + xPos +
                ", yPos = " + yPos + ", width = " + width + ", height = " + height + ", colour = " + c.ToString());
            Brush brush = new Pen(c).Brush;
            Rectangle rectangle = new Rectangle(xPos, yPos, width, height);
            Program.graphics.FillRectangle(brush, rectangle);
/*
            Brush b = new Pen(Color.Red).Brush;
            Program.graphics.FillRectangle(b, xPos, yPos, 1, 1);
            Program.graphics.FillRectangle(b, xPos, yPos + height, 1, 1);
            Program.graphics.FillRectangle(b, xPos + width, yPos, 1, 1);
            Program.graphics.FillRectangle(b, xPos + width, yPos + height, 1, 1);
 */
        }

        public void DrawVideo(Rectangle videoRect, Rectangle displayRect)
        {
            Logging.Log(Logging.MHLogDetail, "Context - DrawVideo:");
        }

        public void DrawBackground(Region reg)
        {
            Logging.Log(Logging.MHLogDetail, "Context - DrawBackground:");
           // graphics.Clear(Color.Black);
        }

        public int GetChannelIndex(string str)
        {
            return 0;
        }

        public bool TuneTo(int channel)
        {
            MessageBox.Show("Currently unsupported request to tune to " + channel + "\nThe engine may lockup");
            Program.resetengine = true;
            return false;
        }

        public bool CheckStop()
        {
            return false;
        }

        public bool BeginAudio(string stream, int tag)
        {
            return false;
        }

        public void StopAudio()
        {

        }

        public bool BeginVideo(string stream, int tag)
        {
            return false;
        }

        public void StopVideo()
        {

        }

        public string GetReceiverId()
        {
            return "TST001001";
        }

        public string GetDSMCCId()
        {
            return "DSMTST001";
        }


    }

    class TestDLADisplay : IMHDLADisplay
    {
        public void Draw(int x, int y)
        {
            Logging.Log(Logging.MHLogDetail, "DLA - Draw: x = " + x + ", y = " + y);
        }
        // Set the box size.  Also clears the drawing.
        public void SetSize(int width, int height)
        {
            Logging.Log(Logging.MHLogDetail, "DLA - SetSize: width = " + width + ", height = " + height);
        }

        public void SetLineSize(int width)
        {
            Logging.Log(Logging.MHLogDetail, "DLA - SetLineSize: width = " + width);
        }

        public void SetLineColour(MHRgba colour)
        {
            Logging.Log(Logging.MHLogDetail, "DLA - SetLineColour:");
        }
        public void SetFillColour(MHRgba colour)
        {
            Logging.Log(Logging.MHLogDetail, "DLA - SetFillColour:");
        }

        // Clear the drawing
        public void Clear()
        {
            Logging.Log(Logging.MHLogDetail, "DLA - Clear:");
        }

        // Operations to add items to the drawing.
        public void DrawLine(int x1, int y1, int x2, int y2)
        {
            Logging.Log(Logging.MHLogDetail, "DLA - DrawLine: x1 = " + x1 + ", y1 = " + y1 + "x2 = " + x2 + ", y2 = " + y2);
        }

        public void DrawBorderedRectangle(int x, int y, int width, int height)
        {
            Logging.Log(Logging.MHLogDetail, "DLA - DrawBorderedRectangle: x = " + x + ", y = " + y + "width = " + width + ", height = " + height);
        }

        public void DrawOval(int x, int y, int width, int height)
        {
            Logging.Log(Logging.MHLogDetail, "DLA - DrawOval: x = " + x + ", y = " + y + "width = " + width + ", height = " + height);
        }

        public void DrawArcSector(int x, int y, int width, int height, int start, int arc, bool isSector)
        {
            Logging.Log(Logging.MHLogDetail, "DLA - DrawArcSector: x = " + x + ", y = " + y + "width = " + width + ", height = " + height +
                "start = " + start + ", arc = " + arc + "isSector = " + isSector);
        }

        public void DrawPoly(bool isFilled, Point[] points)
        {
            Logging.Log(Logging.MHLogDetail, "DLA - DrawPoly:");
        }

    }

    class TestBitmapDisplay : IMHBitmapDisplay
    {
        private Bitmap blitbox;
        private Graphics graphics;
        private Bitmap bitmap;

        public void Draw(int x, int y, Rectangle rect, bool tiled)
        {
            Logging.Log(Logging.MHLogDetail, "Bitmap - Draw: x = " + x + ", y = " + y);
            blitbox = new Bitmap(rect.Width, rect.Height);
            graphics = Graphics.FromImage(blitbox);
            if (tiled)
            {
                for (int i = 0; i < rect.Width; i += bitmap.Width)
                {
                    for (int j = 0; j < rect.Height; j += bitmap.Height)
                    {
                        graphics.DrawImage(bitmap, i, j, bitmap.Width, bitmap.Height);
                    }
                }
            }
            else
            {
                graphics.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
            }
            Program.graphics.DrawImage(blitbox, x, y, rect.Width, rect.Height);
/*
            Brush b = new Pen(Color.LightGreen).Brush;
            Program.graphics.DrawRectangle(new Pen(Color.LightGreen), new Rectangle(x, y, rect.Width, rect.Height));
            Program.graphics.FillRectangle(b, x, y, 1, 1);
            Program.graphics.FillRectangle(b, x, y + rect.Height, 1, 1);
            Program.graphics.FillRectangle(b, x + rect.Width, y, 1, 1);
            Program.graphics.FillRectangle(b, x + rect.Width, y + rect.Height, 1, 1);
*/
        }

        public void CreateFromPNG(byte[] data)
        {
            Logging.Log(Logging.MHLogDetail, "Bitmap - CreateFromPNG:");
            MemoryStream ms = new MemoryStream(data);
            bitmap = new Bitmap(ms);
        }

        public void CreateFromMPEG(byte[] data)
        {
            Logging.Log(Logging.MHLogDetail, "Bitmap - CreateFromMPEG:");
        }

        // Scale the bitmap.  Only used for image derived from MPEG I-frames.
        public void ScaleImage(int newWidth, int newHeight)
        {
            Logging.Log(Logging.MHLogDetail, "Bitmap - ScaleImage: newWidth = " + newWidth + ", newHeight = " + newHeight);
        }

        // Information about the image.
        public Size GetSize()
        {
            Logging.Log(Logging.MHLogDetail, "Bitmap - GetSize:");
            if (bitmap != null)
            {
                return bitmap.Size;
            }
            else
            {
                return new Size(0, 0);
            }
        }

        // True only if the visible area is fully opaque
        public bool IsOpaque()
        {
            Logging.Log(Logging.MHLogDetail, "Bitmap - IsOpaque:");
            return true;
        }
    }

    class TestTextDisplay : IMHTextDisplay
    {
        private Font font;
        private int xPos;
        private int yPos;
        private int width;
        private int height;
        private Bitmap bitmap;
        private Graphics graphics;

        public TestTextDisplay()
        {
            bitmap = null;
            graphics = null;
        }

        

        public void SetSize(int width, int height)
        {
            Logging.Log(Logging.MHLogDetail, "Text - SetSize: width = " + width + ", height = " + height);
            this.height = height;
            this.width = width;
            if (!(width == 0 || height == 0))
            {
                bitmap = new Bitmap(width, height);
                bitmap.SetResolution((float)57.857, (float)72.0);
                //bitmap.SetResolution((float)48, (float)72.0);
                graphics = Graphics.FromImage(bitmap);
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            }
        }

        public void Draw(int x, int y)
        {
            Logging.Log(Logging.MHLogDetail, "Text - Draw: x = " + x + ", y = " + y);
//            Program.graphics.DrawImage(bitmap, x, y, width, height);
            Program.graphics.DrawImage(bitmap, x, y, width, (int)(height * 1.241));  //*1.241
//            Program.graphics.DrawRectangle(new Pen(Color.Coral), new Rectangle(x, y, width, (int)(height)));
        }

        public void SetFont(int size, bool isBold, bool isItalic)
        {
            Logging.Log(Logging.MHLogDetail, "Text - SetFont: size = " + size + ", isBold = " + isBold + ", isItalic = " + isItalic);
            FontStyle style = FontStyle.Regular;
            if (isBold)
            {
                style = FontStyle.Bold;
            }
            else if (isItalic)
            {
                style = FontStyle.Italic;
            }
            font = new Font(Program.fontname, size, style, GraphicsUnit.Point);
            
        }

        // Get the size of a piece of text.  If maxSize is >= 0 it sets strLen to the number
        // of characters that will fit in that number of bits.
        public Rectangle GetBounds(string str, ref int strLen, int maxSize)
        {
            Logging.Log(Logging.MHLogDetail, "Text - GetBounds: str = " + str + ", maxSize = " + maxSize);
            int charsFitted, linesFitted;
            StringFormat sf = StringFormat.GenericTypographic;
            sf.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox | StringFormatFlags.LineLimit;
            SizeF size = graphics.MeasureString(str, font, new SizeF(maxSize, height), sf, out charsFitted, out linesFitted);
            if (charsFitted < strLen && maxSize >= 0)
            {
                strLen = charsFitted;
            }
            Rectangle rectangle = new Rectangle(0, 0, (int)size.Width, (int)(size.Height * 1.241));
            Logging.Log(Logging.MHLogDetail, "Text - GetBounds returns: width = " + rectangle.Width + ", height = " + rectangle.Height);
            return rectangle;
        }

        public void Clear()
        {
            Logging.Log(Logging.MHLogDetail, "Text - Clear:");
            graphics.Clear(Color.Transparent);
        }

        public void AddText(int x, int y, String str, MHRgba colour)
        {
            Logging.Log(Logging.MHLogDetail, "Text - AddText: x = " + x + ", y = " + y + ", str = " + str);
            Brush brush = new Pen(colour.ToColor()).Brush;
            StringFormat sformat = StringFormat.GenericTypographic;
            RectangleF rect = new RectangleF((float)x, (float)(y / 1.241), (float)width, (float)(height / 1.241));
            graphics.DrawString(str, font, brush, rect, sformat);
            //graphics.DrawRectangle(new Pen(Color.Aqua), (float)x, (float)(y / 1.241), (float)width, (float)(height / 1.241));
        }
    }
}


