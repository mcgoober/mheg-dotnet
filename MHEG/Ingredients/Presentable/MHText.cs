/* 
 *  MHEG-5 Engine (ISO-13522-5)
 *  Copyright (C) 2007 Jason Leonard
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
using System.Drawing;
using System.Text;
using System.IO;
using MHEG.Parser;

namespace MHEG.Ingredients.Presentable
{
    class MHText : MHVisible
    {
        protected MHFontBody      m_OrigFont;
        protected MHOctetString   m_OriginalFontAttrs;
        protected MHColour        m_OriginalTextColour, m_OriginalBgColour;
        protected int             m_nCharSet;
        protected bool            m_fTextWrap;
        // Internal attributes.  The font colour, background colour and font attributes are
        // internal attributes in UK MHEG.
    //  MHFontBody      m_Font;
        protected MHColour        m_textColour, m_bgColour;
        protected MHOctetString   m_fontAttrs;
        protected MHOctetString   m_Content; // The content as an octet string

        protected IMHTextDisplay   m_pDisplay; // Pointer to the display object.
        protected bool            m_NeedsRedraw;

        protected int m_HorizJ, m_VertJ;
        protected int m_LineOrientation;
        protected int m_StartCorner;

        public MHText()
        {
            m_nCharSet = -1;
            m_HorizJ = m_VertJ = Start;
            m_LineOrientation = Horizontal;
            m_StartCorner = UpperLeft;
            m_fTextWrap = false;
            m_pDisplay = null;
            m_OrigFont = new MHFontBody();
            m_OriginalFontAttrs = new MHOctetString();
            m_OriginalTextColour = new MHColour();
            m_OriginalBgColour = new MHColour();
            m_textColour = new MHColour();
            m_bgColour = new MHColour();
            m_fontAttrs = new MHOctetString();
            m_Content = new MHOctetString();            
        }

        public MHText(MHText reference) : base(reference)
        {
            m_OrigFont.Copy(reference.m_OrigFont);
            m_OriginalFontAttrs.Copy(reference.m_OriginalFontAttrs);
            m_OriginalTextColour.Copy(reference.m_OriginalTextColour);
            m_OriginalBgColour.Copy(reference.m_OriginalBgColour);
            m_nCharSet = reference.m_nCharSet;
            m_HorizJ = reference.m_HorizJ;
            m_VertJ = reference.m_VertJ;
            m_LineOrientation = reference.m_LineOrientation;
            m_StartCorner = reference.m_StartCorner;
            m_fTextWrap = reference.m_fTextWrap;
            m_pDisplay = null;
        }
        
        public override string ClassName() 
        { 
            return "Text"; 
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs); writer.Write( "{:Text ");
            base.Print(writer, nTabs+1);
            if (m_OrigFont.IsSet()) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":OrigFont "); m_OrigFont.Print(writer, nTabs+1); writer.Write( "\n"); }
            if (m_OriginalFontAttrs.Size > 0) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":FontAttributes "); m_OriginalFontAttrs.Print(writer, nTabs+1); writer.Write( "\n"); }
            if (m_OriginalTextColour.IsSet())  { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":TextColour "); m_OriginalTextColour.Print(writer, nTabs+1); writer.Write( "\n"); }
            if (m_OriginalBgColour.IsSet())  { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":BackgroundColour "); m_OriginalBgColour.Print(writer, nTabs+1); writer.Write( "\n"); }
            if (m_nCharSet >= 0)  { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":CharacterSet {0}\n", m_nCharSet); }
            if (m_HorizJ != Start) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":HJustification {0}\n", rchJustification[m_HorizJ-1]); }
            if (m_VertJ != Start) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":VJustification {0}\n", rchJustification[m_VertJ-1]); }
            if (m_LineOrientation != Horizontal) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":LineOrientation {0}\n", rchlineOrientation[m_LineOrientation-1]); }
            if (m_StartCorner != UpperLeft) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":StartCorner {0}\n", rchStartCorner[m_StartCorner-1]); }
            if (m_fTextWrap) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":TextWrapping true\n"); }
            Logging.PrintTabs(writer, nTabs);writer.Write( "}\n");
        }


        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            // Font and attributes.
            MHParseNode pFontBody = p.GetNamedArg(ASN1Codes.C_ORIGINAL_FONT);
            if (pFontBody != null) m_OrigFont.Initialise(pFontBody.GetArgN(0), engine);
            MHParseNode pFontAttrs = p.GetNamedArg(ASN1Codes.C_FONT_ATTRIBUTES);
            if (pFontAttrs != null) pFontAttrs.GetArgN(0).GetStringValue(m_OriginalFontAttrs);
            // Colours
            MHParseNode pTextColour = p.GetNamedArg(ASN1Codes.C_TEXT_COLOUR);
            if (pTextColour != null) m_OriginalTextColour.Initialise(pTextColour.GetArgN(0), engine);
            MHParseNode pBGColour = p.GetNamedArg(ASN1Codes.C_BACKGROUND_COLOUR);
            if (pBGColour != null) m_OriginalBgColour.Initialise(pBGColour.GetArgN(0), engine);
            // Character set
            MHParseNode pChset = p.GetNamedArg(ASN1Codes.C_CHARACTER_SET);
            if (pChset != null) m_nCharSet = pChset.GetArgN(0).GetIntValue();
            // Justification
            MHParseNode pHJust = p.GetNamedArg(ASN1Codes.C_HORIZONTAL_JUSTIFICATION);
            if (pHJust != null) m_HorizJ = pHJust.GetArgN(0).GetEnumValue();
            MHParseNode pVJust = p.GetNamedArg(ASN1Codes.C_VERTICAL_JUSTIFICATION);
            if (pVJust != null) m_VertJ = pVJust.GetArgN(0).GetEnumValue();
            // Line orientation
            MHParseNode pLineO = p.GetNamedArg(ASN1Codes.C_LINE_ORIENTATION);
            if (pLineO != null) m_LineOrientation = pLineO.GetArgN(0).GetEnumValue();
            // Start corner
            MHParseNode pStartC = p.GetNamedArg(ASN1Codes.C_START_CORNER);
            if (pStartC != null) m_StartCorner = pStartC.GetArgN(0).GetEnumValue();
            // Text wrapping
            MHParseNode pTextWrap = p.GetNamedArg(ASN1Codes.C_TEXT_WRAPPING);
            if (pTextWrap != null) m_fTextWrap = pTextWrap.GetArgN(0).GetBoolValue();

            m_pDisplay = engine.GetContext().CreateText();
            m_NeedsRedraw = true;
        }

        public override void Preparation(MHEngine engine)
        {
            if (AvailabilityStatus) return;
            // Set the colours and font up from the originals if specified otherwise use the application defaults.
        //  if (m_OrigFont.IsSet()) m_Font.Copy(m_OrigFont);
        //  else m_Font.Copy(engine->m_DefaultFont);
            if (m_OriginalTextColour.IsSet()) m_textColour.Copy(m_OriginalTextColour);
            else engine.GetDefaultTextColour(m_textColour);
            Logging.Assert(m_textColour.IsSet());
            if (m_OriginalBgColour.IsSet()) m_bgColour.Copy(m_OriginalBgColour);
            else engine.GetDefaultBGColour(m_bgColour);
            Logging.Assert(m_bgColour.IsSet());
            if (m_OriginalFontAttrs.Size > 0) m_fontAttrs.Copy(m_OriginalFontAttrs);
            else engine.GetDefaultFontAttrs(m_fontAttrs);
            base.Preparation(engine);

            m_pDisplay.SetSize(m_nBoxWidth, m_nBoxHeight);
            m_NeedsRedraw = true;
        }

        public override void ContentPreparation(MHEngine engine)
        {
            base.ContentPreparation(engine);
            Logging.Assert(m_ContentType != IN_NoContent);
            if (m_ContentType == IN_IncludedContent) CreateContent(m_IncludedContent.Bytes, engine);
        }

        public override void ContentArrived(byte[] data, MHEngine engine)
        {
            CreateContent(data, engine);
            // Now signal that the content is available.
            engine.EventTriggered(this, EventContentAvailable);
            m_NeedsRedraw = true;
        }

        // Actions.
        // Extract the text from an object.  This can be used to load content from a file.
        public override void GetTextData(MHRoot pDestination, MHEngine engine) 
        { 
            pDestination.SetVariableValue(new MHUnion(m_Content)); 
        }

        // Create a clone of this ingredient.
        public override MHIngredient Clone(MHEngine engine) 
        { 
            return new MHText(this); 
        } 

        public override void SetBackgroundColour(MHColour colour, MHEngine engine)
        {
            m_bgColour.Copy(colour);
            // Setting the background colour doesn't affect the text image but we have to
            // redraw it onto the display.
            engine.Redraw(GetVisibleArea());
        }

        public override void SetTextColour(MHColour colour, MHEngine engine)
        {
            m_textColour.Copy(colour);
            m_NeedsRedraw = true;
            engine.Redraw(GetVisibleArea());
        }

        public override void SetFontAttributes(MHOctetString fontAttrs, MHEngine engine)
        {
            m_fontAttrs.Copy(fontAttrs);
            m_NeedsRedraw = true;
            engine.Redraw(GetVisibleArea());
        }

        public static string[] rchJustification =
        {
            "start", // 1
            "end",
            "centre",
            "justified" // 4
        };

        // Enumerated type lookup functions for the text parser.
        public static int GetJustification(string str)
        {
            for (int i = 0; i < rchJustification.Length; i++) 
            {
                if (str.Equals(rchJustification[i])) return (i+1); // Numbered from 1
            }
            return 0;
        }

        public static string[] rchlineOrientation =
        {
            "vertical", // 1
            "horizontal"
        };

        public static int GetLineOrientation(string str)
        {
            for (int i = 0; i < rchlineOrientation.Length; i++) 
            {
                if (str.Equals(rchlineOrientation[i])) return (i + 1);
            }
            return 0;
        }
        
        public static string[] rchStartCorner =
        {
            "upper-left", // 1
            "upper-right",
            "lower-left",
            "lower-right" // 4
        };

        public static int GetStartCorner(string str)
        {
            for (int i = 0; i < rchStartCorner.Length; i++) {
                if (str.Equals(rchStartCorner[i])) return (i + 1);
            }
            return 0;
        }

        // Display function.
        public override void Display(MHEngine engine)
        {
            if (!RunningStatus || m_pDisplay == null || m_nBoxWidth == 0 || m_nBoxHeight == 0) return; // Can't draw zero sized boxes.
            // We only need to recreate the display if something has changed.
            if (m_NeedsRedraw)
            {
                Redraw();
                m_NeedsRedraw = false;
            }
            // Draw the background first, then the text.
            engine.GetContext().DrawRect(m_nPosX, m_nPosY, m_nBoxWidth, m_nBoxHeight, GetColour(m_bgColour));
            m_pDisplay.Draw(m_nPosX, m_nPosY);
        }


        public override Region GetOpaqueArea()
        {
            if (!RunningStatus || (GetColour(m_bgColour)).Alpha != 255)
            {
                Region r = new Region();
                r.MakeEmpty();
                return r;
            }
            else return new Region(new Rectangle(m_nPosX, m_nPosY, m_nBoxWidth, m_nBoxHeight));
        }

        // UK MHEG specifies the use of the Tiresias font and broadcasters appear to
        // assume that all MHEG applications will lay the text out in the same way.

        // Recreate the image.
        protected void Redraw()
        {

            if (!RunningStatus || m_pDisplay == null) return;
            if (m_nBoxWidth == 0 || m_nBoxHeight == 0) return; // Can't draw zero sized boxes.

            m_pDisplay.SetSize(m_nBoxWidth, m_nBoxHeight);
            m_pDisplay.Clear();

            MHRgba textColour = GetColour(m_textColour);  
            // Process any escapes in the text and construct the text arrays.
            MHSequence <MHTextLine> theText = new MHSequence<MHTextLine>();
            // Set up the first item on the first line.
            MHTextItem pCurrItem = new MHTextItem();
            MHTextLine pCurrLine = new MHTextLine();
            pCurrLine.Items.Append(pCurrItem);
            theText.Append(pCurrLine);
            Stack <MHRgba> m_ColourStack = new Stack<MHRgba>(); // Stack to handle nested colour codes.
            m_ColourStack.Push(textColour);
            pCurrItem.Colour = textColour;

            int i = 0;
            while (i < m_Content.Size) {
                char ch = m_Content.GetAt(i++);

                if (ch == '\t') { // Tab - start a new item if we have any text in the existing one.
                    if (pCurrItem.Text.Size != 0) 
                    { 
                        pCurrItem = pCurrItem.NewItem(); 
                        pCurrLine.Items.Append(pCurrItem); 
                    }
                    pCurrItem.TabCount++;
                }

                else if (ch == '\r') { // CR - line break.
                    // TODO: Two CRs next to one another are treated as </P> rather than <BR><BR>
                    // This should also include the sequence CRLFCRLF.
                    pCurrLine = new MHTextLine();
                    theText.Append(pCurrLine);
                    pCurrItem = pCurrItem.NewItem();
                    pCurrLine.Items.Append(pCurrItem);
                }

                else if (ch == 0x1b) { // Escape - special codes.
                    if (i == m_Content.Size) break;
                    char code = m_Content.GetAt(i);
                    // The only codes we are interested in are the start and end of colour.
                    // TODO: We may also need "bold" and some hypertext colours.

                    if (code >= 0x40 && code <= 0x5e) { // Start code
                        // Start codes are followed by a parameter count and a number of parameter bytes.
                        if (++i == m_Content.Size) break;
                        char paramCount = m_Content.GetAt(i);
                        i++;
                        if (code == 0x43 && paramCount == 4 && i+paramCount <= m_Content.Size) {
                            // Start of colour.
                            if (pCurrItem.Text.Size != 0) {
                                pCurrItem = pCurrItem.NewItem(); pCurrLine.Items.Append(pCurrItem);
                            }
                            pCurrItem.Colour = new MHRgba(m_Content.GetAt(i), m_Content.GetAt(i+1),
                                                        m_Content.GetAt(i+2), 255-m_Content.GetAt(i+3));
                            // Push this colour onto the colour stack.
                            m_ColourStack.Push(pCurrItem.Colour);
                        }
                        else Logging.Log(Logging.MHLogWarning, "Unknown text escape code " + code);
                        i += paramCount; // Skip the parameters
                    }
                    else if (code >= 0x60 && code <= 0x7e) { // End code.
                        i++;
                        if (code == 0x63) {
                            if (m_ColourStack.Count > 1) {
                                m_ColourStack.Pop();
                                // Start a new item since we're using a new colour.
                                if (pCurrItem.Text.Size != 0) {
                                    pCurrItem = pCurrItem.NewItem();
                                    pCurrLine.Items.Append(pCurrItem);
                                }
                                // Set the subsequent text in the colour we're using now.
                                pCurrItem.Colour = m_ColourStack.Peek();
                            }
                        }
                    }
                }

                else if (ch <= 0x1f) {
                    // Certain characters including LF and the marker codes between 0x1c and 0x1f are
                    // explicitly intended to be ignored.  Include all the other codes.
                }

                else { // Add to the current text.
                    int nStart = i-1;
                    while (i < m_Content.Size && m_Content.GetAt(i) >= 0x20) i++;
                    pCurrItem.Text.Append(new MHOctetString(m_Content, nStart, i-nStart));
                }
            }
        

            // Set up the initial attributes.
            int style, size, lineSpace, letterSpace;
            InterpretAttributes(m_fontAttrs, out style, out size, out lineSpace, out letterSpace);
            // Create a font with this information.
            m_pDisplay.SetFont(size, (style & 2) != 0, (style & 1) != 0);

            // Calculate the layout of each section.
            for (i = 0; i < theText.Size; i++) {
                MHTextLine pLine = theText.GetAt(i);
                pLine.LineWidth = 0;
                for (int j = 0; j < pLine.Items.Size; j++) 
                {
                    MHTextItem pItem = pLine.Items.GetAt(j);
                    // Set any tabs.
                    for (int k = 0; k < pItem.TabCount; k++) pLine.LineWidth += TABSTOP - pLine.LineWidth % TABSTOP;

                    if (pItem.UnicodeLength == 0) 
                    { // Convert UTF-8 to Unicode.
                        int s = pItem.Text.Size;
                        pItem.Unicode = pItem.Text.ToString();
                        pItem.UnicodeLength = pItem.Unicode.Length;
                    }
                    // Fit the text onto the line.
                    int nFullText = pItem.UnicodeLength;
                    // Get the box size and update pItem.m_nUnicode to the number that will fit.
                    Rectangle rect = m_pDisplay.GetBounds(pItem.Unicode, ref nFullText, m_nBoxWidth - pLine.LineWidth);
                    if (nFullText == pItem.UnicodeLength || ! m_fTextWrap) 
                    {
                        // All the characters fit or we're not wrapping.
                        pItem.Width = rect.Width;
                        pLine.LineWidth += rect.Width;
                    }
 /*                   else if (m_fTextWrap) 
                    { // No, we have to word-wrap.
                        int nTruncated = pItem.UnicodeLength; // Just in case.
                        // Now remove characters until we find a word-break character.
                        while (pItem.UnicodeLength > 0 && pItem.Unicode[pItem.UnicodeLength] != ' ') pItem.UnicodeLength--;
                        // If there are now word-break characters we truncate the text.
                        if (pItem.UnicodeLength == 0) pItem.UnicodeLength = nTruncated;
                        // Special case to avoid infinite loop if the box is very narrow.
                        if (pItem.UnicodeLength == 0) pItem.UnicodeLength = 1;

                        // We need to move the text we've cut off this line into a new line.
                        int nNewWidth = nFullText - pItem.UnicodeLength;
                        int nNewStart = pItem.UnicodeLength;
                        // Remove any spaces at the start of the new section.
                        while (nNewWidth != 0 && pItem.Unicode[nNewStart] == ' ') { nNewStart++; nNewWidth--; }
                        if (nNewWidth != 0) {
                            // Create a new line from the extra text.
                            MHTextLine pNewLine = new MHTextLine();
                            theText.InsertAt(pNewLine, i+1);
                            // The first item on the new line is the rest of the text.
                            MHTextItem pNewItem = pItem.NewItem();
                            pNewLine.Items.Append(pNewItem);
                            pNewItem.Unicode = pItem.Unicode.Substring(nNewStart, nNewWidth);
                            pNewItem.UnicodeLength = nNewWidth;
                        }
                        // Remove any spaces at the end of the old section.  If we don't do that and
                        // we are centering or right aligning the text we'll get it wrong.
                        while (pItem.UnicodeLength > 1 && pItem.Unicode[pItem.UnicodeLength - 1] == ' ') pItem.UnicodeLength--;
                        int uniLength = pItem.UnicodeLength;
                        rect = m_pDisplay.GetBounds(pItem.Unicode, ref uniLength, 0);
                        pItem.Width = rect.Width;
                        pLine.LineWidth += rect.Width;
                    }
*/                }
            }

            // Now output the text.
            int yOffset = 0;
            // If there isn't space for all the lines we should drop extra lines.
            int nNumLines = theText.Size;
            do 
            {
                if (m_VertJ == End) yOffset = m_nBoxHeight - nNumLines * lineSpace;
                else if (m_VertJ == Centre) yOffset = (m_nBoxHeight - nNumLines * lineSpace)/2;
                if (yOffset < 0) nNumLines--;
            } while (yOffset < 0);

            for (i = 0; i < nNumLines; i++) 
            {
                MHTextLine pLine = theText.GetAt(i);
                int xOffset = 0;
                if (m_HorizJ == End) xOffset = m_nBoxWidth - pLine.LineWidth;
                else if (m_HorizJ == Centre) xOffset = (m_nBoxWidth - pLine.LineWidth)/2;
                //ASSERT(xOffset >= 0);

                for (int j = 0; j < pLine.Items.Size; j++) {
                    MHTextItem pItem = pLine.Items.GetAt(j);
                    // Tab across if necessary.
                    for (int k = 0; k < pItem.TabCount; k++) xOffset += TABSTOP - xOffset % TABSTOP;
                    if (pItem.Unicode.Length != 0) { // We may have blank lines.
                        m_pDisplay.AddText(xOffset, yOffset, // Jas removed this cos it doesn't make sense yOffset + lineSpace
                            pItem.Unicode.Substring(0, pItem.UnicodeLength), pItem.Colour);
                    }
                    xOffset += pItem.Width;
 
                }
                yOffset += lineSpace;
                if (yOffset + lineSpace > m_nBoxHeight) break;
            }
        }

        // UK MHEG. Interpret the font attributes.
        protected void InterpretAttributes(MHOctetString attrs, out int style, out int size, out int lineSpace, out int letterSpace)
        {
            // Set the defaults.
            style = 0; size = 0x18; lineSpace = 0x18; letterSpace = 0;
            if (attrs.Size == 5) { // Short form.
                style = attrs.GetAt(0); // Only the bottom nibble is significant.
                size = attrs.GetAt(1);
                lineSpace = attrs.GetAt(2);
                // Is this big-endian or little-endian?  Assume big.
                letterSpace = attrs.GetAt(3) * 256 + attrs.GetAt(4);
                if (letterSpace > 32767) letterSpace -= 65536; // Signed.
            }
            else { // Textual form.
                String str = attrs.ToString() + ".";
                Logging.Assert(str != null);                
                int q = str.IndexOf('.'); // Find the terminating dot
                if (q != -1) { // plain, italic etc.                    
                    string type = str.Substring(0, q);
                    str = str.Substring(q + 1);
                    if (type.Equals("italic")) style = 1;
                    else if (type.Equals("bold")) style = 2;
                    else if (type.Equals("bold-italic")) style = 3;
                    // else it's plain.
                    q = str.IndexOf('.');
                }
                if (q != -1) { // Size
                    string s = str.Substring(0, q);
                    str = str.Substring(q + 1);
                    size = Convert.ToInt32(s);
                    if (size == 0) size = 0x18;

                    q = str.IndexOf('.'); // Find the next dot.
                }
                if (q != -1) { // lineSpacing
                    string ls = str.Substring(0, q);
                    str = str.Substring(q + 1);
                    lineSpace = Convert.ToInt32(ls);
                    if (lineSpace == 0) size = 0x18;

                    q = str.IndexOf('.'); // Find the next dot.
                }
                if (q != -1) { // letter spacing.  May be zero or negative
                    string ls = str.Substring(0, q);
                    letterSpace = Convert.ToInt32(ls);
                }
            }

        }

        // Create the Unicode content from the character input.
        protected void CreateContent(byte[] p, MHEngine engine)
        {
            m_Content.Copy(new MHOctetString(p));
            engine.Redraw(GetVisibleArea()); // Have to redraw if the content has changed.
            m_NeedsRedraw = true;
        //  fprintf(fd, "Text content is now "); m_Content.PrintMe(0); fprintf(fd, "\n");
        }

        public const int TABSTOP = 45;

        public const int Start = 1;
        public const int End = 2;
        public const int Centre = 3;
        public const int Justified = 4;

        public const int Vertical = 1;
        public const int Horizontal = 2;
        
        public const int UpperLeft = 1;
        public const int UpperRight = 2;
        public const int LowerLeft = 3;
        public const int LowerRight = 4;

    }
}
