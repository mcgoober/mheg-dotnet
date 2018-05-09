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
    abstract class MHAction4Int: MHElemAction
    {
        protected MHGenericInteger m_Argument1;
        protected MHGenericInteger m_Argument2;
        protected MHGenericInteger m_Argument3;
        protected MHGenericInteger m_Argument4;

        public MHAction4Int(string name)
            : base(name)
        {
            m_Argument1 = new MHGenericInteger();
            m_Argument2 = new MHGenericInteger();
            m_Argument3 = new MHGenericInteger();
            m_Argument4 = new MHGenericInteger();
        }

        public abstract void CallAction(MHEngine engine, MHRoot pTarget, int nArg1, int nArg2, int nArg3, int nArg4);

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            m_Argument1.Initialise(p.GetArgN(1), engine);
            m_Argument2.Initialise(p.GetArgN(2), engine);
            m_Argument3.Initialise(p.GetArgN(3), engine);
            m_Argument4.Initialise(p.GetArgN(4), engine);
        }

        public override void Perform(MHEngine engine)
        {
            CallAction(engine, Target(engine), m_Argument1.GetValue(engine), m_Argument2.GetValue(engine), m_Argument3.GetValue(engine), m_Argument4.GetValue(engine));
        }

        protected override void PrintArgs(TextWriter writer, int nTabs)
        {
            m_Argument1.Print(writer, 0);
            m_Argument2.Print(writer, 0);
            m_Argument3.Print(writer, 0);
            m_Argument4.Print(writer, 0);
        }
    }
}
