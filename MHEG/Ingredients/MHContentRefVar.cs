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
    class MHContentRefVar : MHVariable
    {
        protected MHContentRef m_OriginalValue, m_Value;

        public MHContentRefVar() 
        {
            m_OriginalValue = new MHContentRef();
            m_Value = new MHContentRef();
        }

        public override string ClassName() 
        { 
            return "ContentRefVariable"; 
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            // Original value should be an Content reference.
            MHParseNode pInitial = p.GetNamedArg(ASN1Codes.C_ORIGINAL_VALUE);
            // and this should be a ObjRef node.
            MHParseNode pArg = pInitial.GetNamedArg(ASN1Codes.C_CONTENT_REFERENCE);
            m_OriginalValue.Initialise(pArg.GetArgN(0), engine);
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs); writer.Write("{:ContentRefVar");
            base.Print(writer, nTabs+1);
            Logging.PrintTabs(writer, nTabs+1); writer.Write(":OrigValue "); m_OriginalValue.Print(writer, nTabs+1); writer.Write("\n");
            Logging.PrintTabs(writer, nTabs); writer.Write("}\n");
        }

        public virtual void Prepare() 
        { 
            m_Value.Copy(m_OriginalValue); 
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
            parm.CheckType(MHUnion.U_ContentRef);
            bool fRes = false;
            switch (nOp)
            {
                case TC_Equal: fRes = m_Value.Equal(parm.ContentRef, engine); break;
                case TC_NotEqual: fRes = ! m_Value.Equal(parm.ContentRef, engine); break;
                default: throw new MHEGException("Invalid comparison for Content ref");
            }
            engine.EventTriggered(this, EventTestEvent, new MHUnion(fRes));
        }

        public override void GetVariableValue(MHUnion value, MHEngine engine)
        {
            value.Type = MHUnion.U_ContentRef;
            value.ContentRef.Copy(m_Value);
        }

        public override void SetVariableValue(MHUnion value)
        {
            value.CheckType(MHUnion.U_ContentRef);
            m_Value.Copy(value.ContentRef);
            Logging.Log(Logging.MHLogDetail, "Update " + m_ObjectIdentifier.Printable() + " := " + m_Value.Printable());
        }       
    }
}
