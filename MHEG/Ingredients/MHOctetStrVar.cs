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

namespace MHEG.Ingredients
{
    class MHOctetStrVar : MHVariable
    {
        protected MHOctetString m_OriginalValue, m_Value;

        public MHOctetStrVar() 
        {
            m_OriginalValue = new MHOctetString();
            m_Value = new MHOctetString();
        }

        public override string ClassName() 
        {
            return "OctetStringVariable"; 
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            // Original value should be an int.
            MHParseNode pInitial = p.GetNamedArg(ASN1Codes.C_ORIGINAL_VALUE);
            pInitial.GetArgN(0).GetStringValue(m_OriginalValue);
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs); writer.Write("{:OStringVar");
            base.Print(writer, nTabs+1);
            Logging.PrintTabs(writer, nTabs+1); writer.Write(":OrigValue "); m_OriginalValue.Print(writer, nTabs); writer.Write("\n");
            Logging.PrintTabs(writer, nTabs); writer.Write("}\n"); 
        }

        // Internal behaviours.
        public override void Preparation(MHEngine engine)
        {
            if (AvailabilityStatus) return;
            m_Value.Copy(m_OriginalValue);
            base.Preparation(engine);
        }

        // Actions implemented
        public override void TestVariable(int nOp, MHUnion parm, MHEngine engine)
        {
            parm.CheckType(MHUnion.U_String);
            int nRes = m_Value.Compare(parm.String);
            bool fRes = false;
            switch (nOp) 
            {
                case TC_Equal: fRes = (nRes == 0); break;
                case TC_NotEqual: fRes = (nRes != 0); break;
/*              case TC_Less: fRes = (m_nValue < parm.Int); break;
                case TC_LessOrEqual: fRes = (m_nValue <= parm.Int); break;
                case TC_Greater: fRes = (m_nValue > parm.Int); break;
                case TC_GreaterOrEqual: fRes = (m_nValue >= parm.Int); break;*/
                default: throw new MHEGException("Invalid comparison for string"); // Shouldn't ever happen
            }
            MHOctetString sample1 = new MHOctetString(m_Value, 0, 10);
            MHOctetString sample2 = new MHOctetString(parm.String, 0, 10);
            Logging.Log(Logging.MHLogDetail, "Comparison " + TestToString(nOp) + " between " + sample1.Printable()
                + " and " + sample2.Printable() + " => " + (fRes ? "true" : "false"));
            engine.EventTriggered(this, EventTestEvent, new MHUnion(fRes));
        }

        public override void GetVariableValue(MHUnion value, MHEngine engine)
        {
            value.Type = MHUnion.U_String;
            value.String.Copy(m_Value);
        }

        public override void SetVariableValue(MHUnion value)
        {
            if (value.Type == MHUnion.U_Int) 
            {
                // Implicit conversion of int to string.                
                m_Value.Copy(Convert.ToString(value.Int));
            }
            else 
            {
                value.CheckType(MHUnion.U_String);
                m_Value.Copy(value.String);
            }
            MHOctetString sample = new MHOctetString(m_Value, 0, 10);
            Logging.Log(Logging.MHLogDetail, "Update " + m_ObjectIdentifier.Printable() + " := " + sample.Printable());
        }       
    }
}
