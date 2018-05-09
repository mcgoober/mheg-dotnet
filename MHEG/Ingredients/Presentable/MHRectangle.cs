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
using System.Drawing;
using System.IO;

namespace MHEG.Ingredients.Presentable
{
    class MHRectangle : MHLineArt
    {
        public MHRectangle() 
        {
        }

        MHRectangle(MHRectangle reference): base(reference) 
        {
        }

        public override string ClassName() 
        { 
            return "Rectangle"; 
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs); writer.Write("{:Rectangle ");
            base.Print(writer, nTabs+1);
            Logging.PrintTabs(writer, nTabs); writer.Write("}\n");
        }

        // Display function.
        public override void Display(MHEngine engine)
        {
            if (!RunningStatus) return;
            if (m_nBoxWidth == 0 || m_nBoxHeight == 0) return; // Can't draw zero sized boxes.
            // The bounding box is assumed always to be True.

            MHRgba lineColour = GetColour(m_LineColour);
            MHRgba fillColour = GetColour(m_FillColour);
            IMHContext d = engine.GetContext();
            // Fill the centre.
            if (m_nBoxHeight < m_nLineWidth * 2 || m_nBoxWidth < m_nLineWidth * 2)
            {
                // If the area is very small but non-empty fill it with the line colour
                d.DrawRect(m_nPosX, m_nPosY, m_nBoxWidth, m_nBoxHeight, lineColour);
            }
            else
            {
                d.DrawRect(m_nPosX + m_nLineWidth, m_nPosY + m_nLineWidth,
                        m_nBoxWidth - m_nLineWidth * 2, m_nBoxHeight - m_nLineWidth * 2, fillColour);
                // Draw the lines round the outside.  UK MHEG allows us to treat all line styles as solid.
                // It isn't clear when we draw dashed and dotted lines what colour to put in the spaces.
                d.DrawRect(m_nPosX, m_nPosY, m_nBoxWidth, m_nLineWidth, lineColour);
                d.DrawRect(m_nPosX, m_nPosY + m_nBoxHeight - m_nLineWidth, m_nBoxWidth, m_nLineWidth, lineColour);
                d.DrawRect(m_nPosX, m_nPosY + m_nLineWidth, m_nLineWidth, m_nBoxHeight - m_nLineWidth * 2, lineColour);
                d.DrawRect(m_nPosX + m_nBoxWidth - m_nLineWidth, m_nPosY + m_nLineWidth,
                    m_nLineWidth, m_nBoxHeight - m_nLineWidth * 2, lineColour);
            }
        }


        public override Region GetOpaqueArea()
        {
            if (!RunningStatus) return createEmptyRegion();
            MHRgba lineColour = GetColour(m_LineColour);
            MHRgba fillColour = GetColour(m_FillColour);
            // If the fill is transparent or semi-transparent we return an empty region and
            // ignore the special case where the surrounding box is opaque.
            if (fillColour.Alpha != 255) return createEmptyRegion();
            if (lineColour.Alpha == 255 || m_nLineWidth == 0) return new Region(new Rectangle(m_nPosX, m_nPosY, m_nBoxWidth, m_nBoxHeight));
            if (m_nBoxWidth <= 2 * m_nLineWidth || m_nBoxHeight <= 2 * m_nLineWidth) return createEmptyRegion();
            else return new Region(new Rectangle(m_nPosX + m_nLineWidth, m_nPosY + m_nLineWidth,
                        m_nBoxWidth - m_nLineWidth*2, m_nBoxHeight - m_nLineWidth*2));        
        }


        public override MHIngredient Clone(MHEngine engine) 
        { 
            return new MHRectangle(this); 
        } // Create a clone of this ingredient.
    }
}
