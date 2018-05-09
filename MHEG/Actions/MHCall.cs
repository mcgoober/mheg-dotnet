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
    class MHCall: MHElemAction
    {
        private bool m_bIsFork;
        private MHObjectRef m_Succeeded; // Boolean variable set to call result
        private MHSequence<MHParameter> m_Parameters; // Arguments.

        public MHCall(string name, bool bIsFork)
            : base(name) 
        {
            m_Succeeded = new MHObjectRef();
            m_Parameters = new MHSequence<MHParameter>();
            m_bIsFork = bIsFork;
        }
        
        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine); // Target
            m_Succeeded.Initialise(p.GetArgN(1), engine); // Call/fork succeeded flag
            // Arguments.
            MHParseNode args = p.GetArgN(2);
            for (int i = 0; i < args.GetSeqCount(); i++) 
            {
                MHParameter pParm = new MHParameter();
                m_Parameters.Append(pParm);
                pParm.Initialise(args.GetSeqN(i), engine);
            }
        }

        protected override void PrintArgs(TextWriter writer, int nTabs)
        {
            m_Succeeded.Print(writer, nTabs);
            writer.Write(" ( ");
            for (int i = 0; i < m_Parameters.Size; i++) m_Parameters.GetAt(i).Print(writer, 0);
            writer.Write(" )\n");
        }

        public override void Perform(MHEngine engine)
        {
            // Output parameters are handled by IndirectRefs so we don't evaluate the parameters here.
            Target(engine).CallProgram(m_bIsFork, m_Succeeded, m_Parameters, engine);
        }
    }
}
