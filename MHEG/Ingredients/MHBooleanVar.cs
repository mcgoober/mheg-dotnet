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
    class MHBooleanVar : MHVariable
    {
        protected bool m_fOriginalValue, m_fValue;

        public MHBooleanVar()
        {

        }

        public override string ClassName()
        {
            return "BooleanVariable";
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            // Original value should be a bool.
            MHParseNode pInitial = p.GetNamedArg(ASN1Codes.C_ORIGINAL_VALUE);
            m_fOriginalValue = pInitial.GetArgN(0).GetBoolValue();
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs); writer.Write("{:BooleanVar");
            base.Print(writer, nTabs+1);
            Logging.PrintTabs(writer, nTabs+1); writer.Write(":OrigValue {0}\n", m_fOriginalValue ? "true" : "false");
            Logging.PrintTabs(writer, nTabs); writer.Write("}\n");
        }

        public override void Preparation(MHEngine engine)
        {
            if (m_fAvailable) return;
            m_fValue = m_fOriginalValue;
            base.Preparation(engine);
        }

        public override void TestVariable(int nOp, MHUnion parm, MHEngine engine)
        {
            parm.CheckType(MHUnion.U_Bool);
            bool fRes = false;
            switch (nOp) 
            {
                case TC_Equal: fRes = m_fValue == parm.Bool; break;
                case TC_NotEqual: fRes = m_fValue != parm.Bool; break;
                default: throw new MHEGException("Invalid comparison for bool");
            }
            Logging.Log(Logging.MHLogDetail, "Comparison " + TestToString(nOp) + " between " + (m_fValue ? "true" : "false")
                + " and " + (parm.Bool ? "true" : "false") + " => " + (fRes ? "true" : "false"));
            engine.EventTriggered(this, EventTestEvent, new MHUnion(fRes));
        }

        public override void GetVariableValue(MHUnion value, MHEngine engine)
        {
            value.Type = MHUnion.U_Bool;
            value.Bool = m_fValue;
        }

        public override void SetVariableValue(MHUnion value)
        {
            value.CheckType(MHUnion.U_Bool);
            m_fValue = value.Bool;
            Logging.Log(Logging.MHLogDetail, "Update " + ObjectIdentifier.Printable() + " := " + (m_fValue ? "true": "false"));
        }



    }
}
