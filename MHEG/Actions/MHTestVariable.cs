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
    class MHTestVariable : MHElemAction
    {
        protected MHParameter m_Comparison; // Value to compare with.
        int m_nOperator;

        public MHTestVariable()
            : base(":TestVariable")
        {
            m_Comparison = new MHParameter();
            m_nOperator = 0;
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine); // Target
            m_nOperator = p.GetArgN(1).GetIntValue(); // Test to perform
            m_Comparison.Initialise(p.GetArgN(2), engine); // Value to compare against
        }

        public override void Perform(MHEngine engine)
        {
            MHObjectRef target = new MHObjectRef();
            m_Target.GetValue(target, engine); // Get the target
            MHUnion testValue = new MHUnion();
            testValue.GetValueFrom(m_Comparison, engine); // Get the actual value to compare.
            engine.FindObject(target).TestVariable(m_nOperator, testValue, engine); // Do the test.
        }

        protected override void PrintArgs(TextWriter writer, int nTabs)
        {
            writer.Write(" {0} ", m_nOperator);
            m_Comparison.Print(writer, 0);
        }
    }
}
