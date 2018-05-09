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
    class MHSetData : MHElemAction
    {
        protected bool m_fIsIncluded, m_fSizePresent, m_fCCPriorityPresent;
        protected MHGenericOctetString m_Included;
        protected MHGenericContentRef m_Referenced;
        protected MHGenericInteger m_ContentSize;
        protected MHGenericInteger m_CCPriority;

        public MHSetData()
            : base(":SetData")
        {
            m_Included = new MHGenericOctetString();
            m_Referenced = new MHGenericContentRef();
            m_ContentSize = new MHGenericInteger();
            m_CCPriority = new MHGenericInteger();
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine); // Target
            MHParseNode pContent = p.GetArgN(1);
            if (pContent.NodeType == MHParseNode.PNSeq) {
                // Referenced content.
                m_fIsIncluded = false;
                m_fSizePresent = m_fCCPriorityPresent = false;
                m_Referenced.Initialise(pContent.GetSeqN(0), engine);

                if (pContent.GetSeqCount() > 1) {
                    MHParseNode pArg = pContent.GetSeqN(1);
                    if (pArg.NodeType == MHParseNode.PNTagged && pArg.GetTagNo() == ASN1Codes.C_NEW_CONTENT_SIZE) {
                        MHParseNode pVal = pArg.GetArgN(0);
                        // It may be NULL as a place-holder
                        if (pVal.NodeType == MHParseNode.PNInt) {
                            m_fSizePresent = true;
                            m_ContentSize.Initialise(pVal, engine);
                        }
                    }
                }

                if (pContent.GetSeqCount() > 2) {
                    MHParseNode pArg = pContent.GetSeqN(2);
                    if (pArg.NodeType == MHParseNode.PNTagged && pArg.GetTagNo() == ASN1Codes.C_NEW_CONTENT_CACHE_PRIO) {
                        MHParseNode pVal = pArg.GetArgN(0);
                        if (pVal.NodeType == MHParseNode.PNInt) {
                            m_fCCPriorityPresent = true;
                            m_CCPriority.Initialise(pVal, engine);
                        }
                    }
                }
            }
            else {
                m_Included.Initialise(pContent, engine);
                m_fIsIncluded = true;
            }
        }

        public override void Perform(MHEngine engine)
        {
            MHObjectRef target = new MHObjectRef();
            m_Target.GetValue(target, engine); // Get the target
            if (m_fIsIncluded)
            { // Included content
                MHOctetString included = new MHOctetString();
                m_Included.GetValue(included, engine);
                engine.FindObject(target).SetData(included, engine);
            }
            else
            {
                MHContentRef referenced = new MHContentRef();
                int size, cc;
                m_Referenced.GetValue(referenced, engine);
                if (m_fSizePresent) size = m_ContentSize.GetValue(engine); else size = 0;
                if (m_fCCPriorityPresent) cc = m_CCPriority.GetValue(engine); else cc = 0;
                engine.FindObject(target).SetData(referenced, m_fSizePresent, size, m_fCCPriorityPresent, cc, engine);
            }
        }

        protected override void PrintArgs(TextWriter writer, int nTabs)
        {
            if (m_fIsIncluded) m_Included.Print(writer, 0);
            else
            {
                m_Referenced.Print(writer, 0);
                if (m_fSizePresent)
                {
                    writer.Write(" :NewContentSize ");
                    m_ContentSize.Print(writer, 0);
                }
                if (m_fCCPriorityPresent)
                {
                    writer.Write(" :NewCCPriority ");
                    m_CCPriority.Print(writer, 0);
                }
            }
        }
    }
}
