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

namespace MHEG.Ingredients.Presentable
{
    class MHTokenGroupItem
    {
        protected MHObjectRef m_Object;
        protected MHSequence<MHActionSequence> m_ActionSlots;

        public MHTokenGroupItem()
        {
            m_Object = new MHObjectRef();
            m_ActionSlots = new MHSequence<MHActionSequence>();
        }

        public MHObjectRef Object
        {
            get
            {
                return m_Object;
            }
        }

        public MHSequence<MHActionSequence> ActionSlots
        {
            get
            {
                return m_ActionSlots;
            }
        }

        public void Initialise(MHParseNode p, MHEngine engine)
        {
            // A pair consisting of an object reference and an optional action slot sequence.
            m_Object.Initialise(p.GetSeqN(0), engine);
            if (p.GetSeqCount() > 1)
            {
                MHParseNode pSlots = p.GetSeqN(1);
                for (int i = 0; i < pSlots.GetSeqCount(); i++)
                {
                    MHParseNode pAct = pSlots.GetSeqN(i);
                    MHActionSequence pActions = new MHActionSequence();
                    m_ActionSlots.Append(pActions);
                    // The action slot entry may be NULL.
                    if (pAct.NodeType != MHParseNode.PNNull) pActions.Initialise(pAct, engine);
                }
            }
        }

        public void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs); writer.Write("( ");
            m_Object.Print(writer, nTabs + 1); writer.Write("\n");
            if (m_ActionSlots.Size != 0)
            {
                Logging.PrintTabs(writer, nTabs + 1); writer.Write(":ActionSlots (\n");
                for (int i = 0; i < m_ActionSlots.Size; i++)
                {
                    Logging.PrintTabs(writer, nTabs + 2); writer.Write("(\n");
                    MHActionSequence pActions = m_ActionSlots.GetAt(i);
                    if (pActions.Size == 0) writer.Write("NULL\n");
                    else pActions.Print(writer, nTabs + 2);
                    Logging.PrintTabs(writer, nTabs + 2); writer.Write(")\n");
                }
                Logging.PrintTabs(writer, nTabs + 1); writer.Write(")\n");
            }
            Logging.PrintTabs(writer, nTabs); writer.Write(")\n");
        }

    }
}
