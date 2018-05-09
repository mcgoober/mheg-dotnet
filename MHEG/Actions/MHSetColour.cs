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

namespace MHEG.Actions
{
    abstract class MHSetColour : MHElemAction
    {
        int m_ColourType;
        protected MHGenericInteger m_Indexed;
        protected MHGenericOctetString m_Absolute;

        public MHSetColour(string name)
            : base(name)
        {
            m_Indexed = new MHGenericInteger();
            m_Absolute = new MHGenericOctetString();
            m_ColourType = 0;
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            if (p.GetArgCount() > 1) 
            {
                MHParseNode pIndexed = p.GetNamedArg(ASN1Codes.C_NEW_COLOUR_INDEX);
                MHParseNode pAbsolute = p.GetNamedArg(ASN1Codes.C_NEW_ABSOLUTE_COLOUR);
                if (pIndexed != null) 
                { 
                    m_ColourType = CT_Indexed; 
                    m_Indexed.Initialise(pIndexed.GetArgN(0), engine); 
                }
                else if (pAbsolute != null) 
                { 
                    m_ColourType = CT_Absolute; 
                    m_Absolute.Initialise(pAbsolute.GetArgN(0), engine); 
                }
            }
        }

        public abstract void SetColour(MHColour colour, MHEngine engine);


        public override void Perform(MHEngine engine)
        {
            MHObjectRef target = new MHObjectRef();
            m_Target.GetValue(target, engine); // Get the item to set.
            MHColour newColour = new MHColour();
            switch (m_ColourType)
            {
            case CT_None:
                {
                    // If the colour is not specified use "transparent".
                    newColour.SetFromString("\u0000\u0000\u0000\u00FF");
                    break;
                }
            case CT_Absolute:
                {
                    MHOctetString colour = new MHOctetString();
                    m_Absolute.GetValue(colour, engine);
                    newColour.ColStr.Copy(colour);
                    break;
                }
            case CT_Indexed:
                newColour.SetFromIndex(m_Indexed.GetValue(engine));
                break;
            }
            SetColour(newColour, engine); // Set the colour of the appropriate portion of the visible
        }

        protected override void PrintArgs(TextWriter writer, int nTabs)
        {
            if (m_ColourType == CT_Indexed) 
            { 
                writer.Write(":NewColourIndex "); 
                m_Indexed.Print(writer, 0); 
            }
            else if (m_ColourType == CT_Absolute) 
            {
                writer.Write(":NewAbsoluteColour ");
                m_Absolute.Print(writer, 0); 
            }
        }

        public const int CT_None = 0;
        public const int CT_Indexed = 1;
        public const int CT_Absolute = 2;
    }
}
