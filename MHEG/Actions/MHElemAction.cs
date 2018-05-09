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
    abstract class MHElemAction
    {
        protected string m_ActionName;
        protected MHGenericObjectRef m_Target;

        public MHElemAction(string name)
        {
            m_ActionName = name;
            m_Target = new MHGenericObjectRef();
        }

        public virtual void Initialise(MHParseNode p, MHEngine engine)
        {
            m_Target.Initialise(p.GetArgN(0), engine);
        }

        public virtual void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs);
            writer.Write("{0} (", m_ActionName);
            m_Target.Print(writer, nTabs + 1);
            PrintArgs(writer, nTabs + 1); // Any other arguments must be handled by the subclass.
            writer.Write(")\n");
        }

        protected virtual void PrintArgs(TextWriter writer, int nTabs)
        {
            // Default is no action
        }

        // Perform the action.
        public abstract void Perform(MHEngine engine); 

        // Look up the target
        protected MHRoot Target(MHEngine engine)
        {
            MHObjectRef target = new MHObjectRef();
            m_Target.GetValue(target, engine);
            return engine.FindObject(target);
        }

    }
}
