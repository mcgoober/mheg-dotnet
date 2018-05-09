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
using MHEG.Parser;

namespace MHEG.Ingredients.Presentable
{
    class MHLineArt : MHVisible
    {
        // Exchanged attributes,
        protected bool m_fBorderedBBox; // Does it have lines round or not?
        protected int m_nOriginalLineWidth;
        protected int m_OriginalLineStyle;

        protected MHColour m_OrigLineColour, m_OrigFillColour;
        // Internal attributes
        protected int m_nLineWidth;
        protected int m_LineStyle;
        protected MHColour m_LineColour, m_FillColour;

        public MHLineArt()
        {
            m_OrigLineColour = new MHColour();
            m_OrigFillColour = new MHColour();
            m_LineColour = new MHColour();
            m_FillColour = new MHColour();
            m_fBorderedBBox = true;
            m_nOriginalLineWidth = 1;
            m_OriginalLineStyle = LineStyleSolid;
            // Colour defaults to empty.
        }

        public MHLineArt(MHLineArt reference) : base(reference)
        {
            m_OrigLineColour = new MHColour();
            m_OrigFillColour = new MHColour();
            m_LineColour = new MHColour();
            m_FillColour = new MHColour();
            m_fBorderedBBox = reference.m_fBorderedBBox;
            m_nOriginalLineWidth = reference.m_nOriginalLineWidth;
            m_OriginalLineStyle = reference.m_OriginalLineStyle;
            m_OrigLineColour = reference.m_OrigLineColour;
            m_OrigFillColour = reference.m_OrigFillColour;
        }

        public override string ClassName() 
        {
            return "LineArt"; 
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            base.Print(writer, nTabs);
            if (! m_fBorderedBBox) { Logging.PrintTabs(writer, nTabs); writer.Write( ":BBBox false\n"); }
            if (m_nOriginalLineWidth != 1) { Logging.PrintTabs(writer, nTabs); writer.Write( ":OrigLineWidth {0}\n", m_nOriginalLineWidth); }
            if (m_OriginalLineStyle != LineStyleSolid) {
                Logging.PrintTabs(writer, nTabs); writer.Write( ":OrigLineStyle {0}\n", m_OriginalLineStyle);
            }
            if (m_OrigLineColour.IsSet()) {
                Logging.PrintTabs(writer, nTabs); writer.Write( ":OrigRefLineColour "); m_OrigLineColour.Print(writer, nTabs+1); writer.Write( "\n");
            }
            if (m_OrigFillColour.IsSet()) {
                Logging.PrintTabs(writer, nTabs); writer.Write( ":OrigRefFillColour "); m_OrigFillColour.Print(writer, nTabs+1); writer.Write( "\n");
            }       
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            // Bordered bounding box - optional
            MHParseNode pBBBox = p.GetNamedArg(ASN1Codes.C_BORDERED_BOUNDING_BOX);
            if (pBBBox != null) m_fBorderedBBox = pBBBox.GetArgN(0).GetBoolValue();
            // Original line width
            MHParseNode pOlw = p.GetNamedArg(ASN1Codes.C_ORIGINAL_LINE_WIDTH);
            if (pOlw != null) m_nOriginalLineWidth = pOlw.GetArgN(0).GetIntValue();
            // Original line style.  This is an integer not an enum.
            MHParseNode pOls = p.GetNamedArg(ASN1Codes.C_ORIGINAL_LINE_STYLE);
            if (pOls != null) m_OriginalLineStyle = pOls.GetArgN(0).GetIntValue();
            // Line colour.
            MHParseNode pOrlc = p.GetNamedArg(ASN1Codes.C_ORIGINAL_REF_LINE_COLOUR);
            if (pOrlc != null) m_OrigLineColour.Initialise(pOrlc.GetArgN(0), engine);
            // Fill colour
            MHParseNode pOrfc = p.GetNamedArg(ASN1Codes.C_ORIGINAL_REF_FILL_COLOUR);
            if (pOrfc != null) m_OrigFillColour.Initialise(pOrfc.GetArgN(0), engine);
        }

        /*
        public virtual void PrintMe(FILE *fd, int nTabs) const;
         */

        public override void Display(MHEngine engine) 
        {
            // Only DynamicLineArt and Rectangle are supported
        } 

        public override void Preparation(MHEngine engine)
        {
            if (AvailabilityStatus) return; // Already prepared
            // Set up the internal attributes.
            m_nLineWidth = m_nOriginalLineWidth;
            m_LineStyle = m_OriginalLineStyle;
            if (m_OrigLineColour.IsSet()) m_LineColour.Copy(m_OrigLineColour);
            else m_LineColour.SetFromString("\u0000\u0000\u0000\u0000"); // Default is black
            if (m_OrigFillColour.IsSet()) m_FillColour.Copy(m_OrigFillColour);
            else m_FillColour.SetFromString("\u0000\u0000\u0000\u00FF"); // Default is transparent

            base.Preparation(engine); // Prepare the base class.
        }

        // Actions on LineArt
        public override void SetFillColour(MHColour colour, MHEngine engine)
        {
            m_FillColour.Copy(colour);
            engine.Redraw(GetVisibleArea());
        }

        public override void SetLineColour(MHColour colour, MHEngine engine)
        {
            m_LineColour.Copy(colour);
            engine.Redraw(GetVisibleArea());
        }

        public override void SetLineWidth(int nWidth, MHEngine engine)
        {
            m_nLineWidth = nWidth;
            engine.Redraw(GetVisibleArea());
        }

        public override void SetLineStyle(int nStyle, MHEngine engine)
        {
            m_LineStyle = nStyle;
            engine.Redraw(GetVisibleArea());
        }


        public const int LineStyleSolid = 1;
        public const int LineStyleDashed = 2;
        public const int LineStyleDotted = 3;


    }
}
