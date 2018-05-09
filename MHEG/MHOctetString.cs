/* 
 *  MHEG-5 Engine (ISO-13522-5)
 *  Copyright (C) 2008 Jason Leonard
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

namespace MHEG
{
    class MHOctetString
    {
        protected string m_String;

        public MHOctetString()
        {
            m_String = "";
        }

        public MHOctetString(string str)
        {
            Copy(str);
        }

        public MHOctetString(MHOctetString str, int nOffset, int nLen)
        {
            if (nLen < 0) nLen = 0;
            if (nLen > str.Size) nLen = str.Size;
            m_String = str.m_String.Substring(nOffset, nLen);            
        }

        public MHOctetString(byte[] bytes)
        {
            m_String = new ASCIIEncoding().GetString(bytes);
        }

        public byte[] Bytes
        {
            get
            {
                return (new ASCIIEncoding()).GetBytes(m_String);
            }
        }

        public void Copy(MHOctetString str)
        {
            m_String = (string)str.m_String.Clone();
        }

        public void Copy(string str)
        {
            m_String = (string)str.Clone();
        }

        public int Size
        {
            get { return m_String.Length; }
        }

        public int Compare(MHOctetString str)
        {
            return m_String.CompareTo(str.m_String);
        }

        public bool Equal(MHOctetString str)
        {
            return m_String.Equals(str.m_String);
        }

        public char GetAt(int i)
        {
            return m_String[i];
        }

        public void Append(MHOctetString str)
        {
            m_String = m_String + str.m_String;
        }

        public void Append(string str)
        {
            m_String = m_String + str;
        }

        public string Printable()
        {
            return ToString();            
        }

        public override string ToString()
        {
            return m_String;
        }

        public void Print(TextWriter writer, int nTabs)
        {
            writer.Write("'");
            for (int i = 0; i < m_String.Length; i++) 
            {
                char c = m_String[i];
                // Escape a non-printable character or an equal sign or a quote.
                if (c == '=' || c == '\'' || c < ' ' || c >= 127) writer.Write("={0:X2}", (int)c);
                else writer.Write(c);
            }
            writer.Write("'");
        }

        public void PrintAsHex(TextWriter writer, int nTabs)
        {
            writer.Write("'");
            for (int i = 0; i < m_String.Length; i++)
            {
                char c = m_String[i];
                writer.Write("={0:X2}", (int)c);                
            }
            writer.Write("'");
        }  
    }
}
