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
using System.Text;
using System.IO;
using System.Drawing;
using MHEG.Parser;

namespace MHEG.Ingredients.Presentable
{
    class MHDynamicLineArt : MHLineArt
    {
        protected IMHDLADisplay m_picture; // The sequence of drawing actions.

        public MHDynamicLineArt()
        {
            m_picture = null;
        }

        public override string ClassName()
        {
            return "DynamicLineArt";
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
             base.Initialise(p, engine);
             m_picture = engine.GetContext().CreateDynamicLineArt(m_fBorderedBBox, GetColour(m_OrigLineColour), GetColour(m_OrigFillColour));
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs); writer.Write("{:DynamicLineArt ");
            base.Print(writer, nTabs + 1);
            Logging.PrintTabs(writer, nTabs); writer.Write("}\n");
        }

        public override void Preparation(MHEngine engine)
        {
            base.Preparation(engine);
            m_picture.SetSize(m_nBoxWidth, m_nBoxHeight);
            m_picture.SetLineSize(m_nLineWidth);
            m_picture.SetLineColour(GetColour(m_LineColour));
            m_picture.SetFillColour(GetColour(m_FillColour));
        }

        public override void Display(MHEngine engine)
        {
            m_picture.Draw(m_nPosX, m_nPosY);
        }

        // Get the opaque area.  This is only opaque if the background is.
        public override Region GetOpaqueArea()
        {
            if ((GetColour(m_OrigFillColour)).Alpha == 255) return GetVisibleArea();
            else
            {
                Region region = new Region();
                region.MakeEmpty();
                return region;
            }
        }

        public override void SetBoxSize(int nWidth, int nHeight, MHEngine engine)
        {
            base.SetBoxSize(nWidth, nHeight, engine);
            m_picture.SetSize(nWidth, nHeight);
            Clear();
        }

        public override void SetPosition(int nXPosition, int nYPosition, MHEngine engine)
        {
            base.SetPosition(nXPosition, nYPosition, engine);
            Clear();
        }

        public override void BringToFront(MHEngine engine)
        {
            base.BringToFront(engine);
            Clear();
        }

        public override void SendToBack(MHEngine engine)
        {
            base.SendToBack(engine);
            Clear();
        }

        public override void PutBefore(MHRoot pRef, MHEngine engine)
        {
            base.PutBefore(pRef, engine);
            Clear();
        }

        public override void PutBehind(MHRoot pRef, MHEngine engine)
        {
            base.PutBehind(pRef, engine);
            Clear();
        }

        public override void Clear()
        {
            m_picture.Clear();
        }

        // Actions on LineArt
        public override void SetFillColour(MHColour colour, MHEngine engine)
        {
            m_FillColour.Copy(colour);
            m_picture.SetFillColour(GetColour(m_FillColour));
        }

        public override void SetLineColour(MHColour colour, MHEngine engine)
        {
            m_LineColour.Copy(colour);
            m_picture.SetLineColour(GetColour(m_LineColour));
        }

        public override void SetLineWidth(int nWidth, MHEngine engine)
        {
            m_nLineWidth = nWidth;
            m_picture.SetLineSize(m_nLineWidth);
        }

        public override void SetLineStyle(int nStyle, MHEngine engine)
        {
            m_LineStyle = nStyle;
        }

        public override void GetLineWidth(MHRoot pResult)
        {
            pResult.SetVariableValue(new MHUnion(m_nLineWidth));
        }

        public override void GetLineStyle(MHRoot pResult)
        {
            pResult.SetVariableValue(new MHUnion(m_LineStyle));
        }

        public override void GetLineColour(MHRoot pResult)
        {
            // Returns the palette index as an integer if it is an index or the colour as a string if not.
            if (m_LineColour.ColIndex >= 0)
            {
                pResult.SetVariableValue(new MHUnion(m_LineColour.ColIndex));
            }
            else
            {
                pResult.SetVariableValue(new MHUnion(m_LineColour.ColStr));
            }
        }

        public override void GetFillColour(MHRoot pResult)
        {
            if (m_FillColour.ColIndex >= 0)
            {
                pResult.SetVariableValue(new MHUnion(m_FillColour.ColIndex));
            }
            else
            {
                pResult.SetVariableValue(new MHUnion(m_FillColour.ColStr));
            }
        }

        public override void DrawArcSector(bool fIsSector, int x, int y, int width, int height, int start, int arc, MHEngine engine)
        {
            m_picture.DrawArcSector(x, y, width, height, start, arc, fIsSector);
            engine.Redraw(GetVisibleArea()); 
        }

        public override void DrawLine(int x1, int y1, int x2, int y2, MHEngine engine)
        {
            m_picture.DrawLine(x1, y1, x2, y2);
            engine.Redraw(GetVisibleArea());
        }

        public override void DrawOval(int x, int y, int width, int height, MHEngine engine)
        {
            m_picture.DrawOval(x, y, width, height);
            engine.Redraw(GetVisibleArea());
        }

        public override void DrawRectangle(int x1, int y1, int x2, int y2, MHEngine engine)
        {
            m_picture.DrawBorderedRectangle(x1, y1, x2 - x1, y2 - y1);
            engine.Redraw(GetVisibleArea());
        }

        public override void DrawPoly(bool fIsPolygon, System.Drawing.Point[] points, MHEngine engine)
        {
            m_picture.DrawPoly(fIsPolygon, points);
            engine.Redraw(GetVisibleArea());
        }
    }
}
