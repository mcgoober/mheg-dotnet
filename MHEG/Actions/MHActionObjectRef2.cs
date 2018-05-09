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
    abstract class MHActionObjectRef2 : MHElemAction
    {
        protected MHObjectRef m_ResultVar1;
        protected MHObjectRef m_ResultVar2;

        public MHActionObjectRef2(string name)
            : base(name)
        {
            m_ResultVar1 = new MHObjectRef();
            m_ResultVar2 = new MHObjectRef();
        }

        public abstract void CallAction(MHEngine engine, MHRoot pTarget, MHRoot pArg1, MHRoot pArg2);

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            m_ResultVar1.Initialise(p.GetArgN(1), engine);
            m_ResultVar2.Initialise(p.GetArgN(2), engine);
        }

        public override void Perform(MHEngine engine)
        {
            CallAction(engine, Target(engine), engine.FindObject(m_ResultVar1), engine.FindObject(m_ResultVar2));
        }

        protected override void PrintArgs(TextWriter writer, int nTabs)
        {
            m_ResultVar1.Print(writer, 0);
            m_ResultVar2.Print(writer, 0);
        }
    }
}