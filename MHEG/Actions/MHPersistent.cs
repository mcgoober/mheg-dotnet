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
    class MHPersistent: MHElemAction
    {
        protected bool m_bIsLoad;
        protected MHObjectRef m_Succeeded;
        protected MHSequence<MHObjectRef> m_Variables;
        protected MHGenericOctetString m_FileName;

        public MHPersistent(string name, bool bIsLoad)
            : base(name)
        {
            m_bIsLoad = bIsLoad;
            m_Succeeded = new MHObjectRef();
            m_Variables = new MHSequence<MHObjectRef>();
            m_FileName = new MHGenericOctetString();
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine); // Target
            m_Succeeded.Initialise(p.GetArgN(1), engine);
            MHParseNode pVarSeq = p.GetArgN(2);
            for (int i = 0; i < pVarSeq.GetSeqCount(); i++) 
            {
                MHObjectRef pVar = new MHObjectRef();
                m_Variables.Append(pVar);
                pVar.Initialise(pVarSeq.GetSeqN(i), engine);
            }
            m_FileName.Initialise(p.GetArgN(3), engine); 
        }

        public override void Perform(MHEngine engine)
        {
            MHObjectRef target = new MHObjectRef();
            m_Target.GetValue(target, engine); // Get the target - this should always be the application
            MHOctetString fileName = new MHOctetString() ;
            m_FileName.GetValue(fileName, engine);
            bool fResult = engine.LoadStorePersistent(m_bIsLoad, fileName, m_Variables);
            engine.FindObject(m_Succeeded).SetVariableValue(new MHUnion(fResult));
        }

        protected override void PrintArgs(TextWriter writer, int nTabs)
        {
            m_Succeeded.Print(writer, nTabs);
            writer.Write(" ( ");
            for (int i = 0; i < m_Variables.Size; i++) m_Variables.GetAt(i).Print(writer, 0);
            writer.Write(" ) ");
            m_FileName.Print(writer, nTabs);
        }
    }
}
