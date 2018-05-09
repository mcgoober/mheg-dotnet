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

namespace MHEG
{
    // An object reference is used to identify and refer to an object.
    // Internal objects have the m_GroupId field empty.
    class MHObjectRef
    {
        private int m_nObjectNo;
        private MHOctetString m_GroupId;

        public MHObjectRef()
        {
            m_nObjectNo = 0;
            m_GroupId = new MHOctetString();
        }

        public int ObjectNo
        {
            get { return m_nObjectNo; }
            set { m_nObjectNo = value; }
        }

        public MHOctetString GroupId
        {
            get { return m_GroupId; }
        }

        public void Initialise(MHParseNode p, MHEngine engine)
        {
            if (p.NodeType == MHParseNode.PNInt) 
            {
                m_nObjectNo = p.GetIntValue();
                // Set the group id to the id of this group.
                m_GroupId.Copy(engine.GetGroupId());
            }
            else if (p.NodeType == MHParseNode.PNSeq) 
            {
                MHParseNode pFirst = p.GetSeqN(0);
                MHOctetString groupId = new MHOctetString();
                pFirst.GetStringValue(m_GroupId);
                m_nObjectNo = p.GetSeqN(1).GetIntValue();
            }
            else p.Failure("ObjectRef: Argument is not int or sequence");            
        }

        public void Copy(MHObjectRef objr)
        {
            m_nObjectNo = objr.m_nObjectNo;
            m_GroupId.Copy(objr.m_GroupId);
        }

        public static MHObjectRef Null = new MHObjectRef();

        public bool IsSet()
        {
            return (m_nObjectNo != 0 || m_GroupId.Size != 0);
        }

        public void Print(TextWriter writer, int nTabs)
        {
            if (m_GroupId.Size == 0)
            {
                writer.Write(" " + m_nObjectNo + " ");
            }
            else
            {
                writer.Write(" ( ");
                m_GroupId.Print(writer, nTabs);
                writer.Write(" " + m_nObjectNo + " ) ");
            }
        }
      
        public bool Equal(MHObjectRef objr, MHEngine engine)
        {
            return m_nObjectNo == objr.m_nObjectNo && engine.GetPathName(m_GroupId) == engine.GetPathName(objr.m_GroupId);
        }

        public string Printable()
        {
            if (m_GroupId.Size == 0) return " " + m_nObjectNo + " ";
            else return " ( " + m_GroupId.Printable() + " " + m_nObjectNo + " ) ";
        }
    }
}
