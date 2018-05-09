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
    class MHAppend : MHElemAction
    {
        protected MHGenericOctetString m_Operand;

        public MHAppend()
            : base(":Append")
        {
            m_Operand = new MHGenericOctetString();            
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine); // Target
            m_Operand.Initialise(p.GetArgN(1), engine); // Operand to append
        }

        public override void Perform(MHEngine engine)
        {
            MHUnion targetVal = new MHUnion();
            // Find the target and get its current value.  The target can be an indirect reference.
            MHObjectRef parm = new MHObjectRef();
            m_Target.GetValue(parm, engine);
            MHRoot pTarget = engine.FindObject(parm);
            pTarget.GetVariableValue(targetVal, engine);
            targetVal.CheckType(MHUnion.U_String);
            // Get the string to append.
            MHOctetString toAppend = new MHOctetString();
            m_Operand.GetValue(toAppend, engine);
            targetVal.String.Append(toAppend); // Add it on the end
            pTarget.SetVariableValue(targetVal); // Set the target to the result.
        }

        protected override void PrintArgs(TextWriter writer, int nTabs)
        {
            m_Operand.Print(writer, 0);
        }
    }
}
