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
    class MHColour
    {
        private MHOctetString m_ColStr;
        private int m_nColIndex;

        public MHColour()
        {
            m_nColIndex = -1;
            m_ColStr = new MHOctetString();
        }

        public MHOctetString ColStr     
        {
            get { return m_ColStr; }
        }

        public int ColIndex
        {
            get { return m_nColIndex; }
        }

        public void Initialise(MHParseNode p, MHEngine engine)
        {
            if (p.NodeType == MHParseNode.PNInt) m_nColIndex = p.GetIntValue();
            else p.GetStringValue(m_ColStr);
        }

        public void Print(TextWriter writer, int nTabs)
        {
            if (m_nColIndex >= 0) writer.Write(" {0} ", m_nColIndex);
            else m_ColStr.PrintAsHex(writer, nTabs);
        }   
 
        public bool IsSet()
        { 
            return m_nColIndex >= 0 || m_ColStr.Size != 0; 
        }
        
        public void SetFromString(string str)
        {
            m_nColIndex = -1;
            m_ColStr.Copy(new MHOctetString(str));
        }

        public void SetFromIndex(int index)
        {
            m_nColIndex = index;
            m_ColStr = new MHOctetString();
        }

        public void Copy(MHColour col)
        {
            m_nColIndex = col.m_nColIndex;
            m_ColStr.Copy(col.m_ColStr);
        }
    }
}
