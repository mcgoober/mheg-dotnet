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

namespace MHEG.Ingredients.Presentable
{
    class MHTextItem
    {
        private MHOctetString m_Text; // UTF-8 text
        private string m_Unicode; // Unicode text
        private int m_nUnicode; // Number of characters in it
        private int m_Width; // Size of this block
        private MHRgba m_Colour; // Colour of the text
        private int m_nTabCount; // Number of tabs immediately before this (usually zero)

        public MHTextItem()
        {
            m_nUnicode = 0;
            m_Width = 0; // Size of this block
            m_Colour = new MHRgba(0, 0, 0, 255);
            m_nTabCount = 0;

            m_Text = new MHOctetString();
            m_Colour = new MHRgba();
        }

        public MHOctetString Text
        {
            get { return m_Text; }
        }

        public string Unicode
        {
            get { return m_Unicode; }
            set { m_Unicode = value; }
        }

        public int UnicodeLength
        {
            get { return m_nUnicode; }
            set { m_nUnicode = value; }
        }

        public int Width
        {
            get { return m_Width; }
            set { m_Width = value; }
        }

        public MHRgba Colour
        {
            get { return m_Colour; }
            set { m_Colour = value; }
        }

        public int TabCount
        {
            get { return m_nTabCount; }
            set { m_nTabCount = value; }
        }


        // Generate new items inheriting properties from the previous
        public MHTextItem NewItem()
        {
            MHTextItem pItem = new MHTextItem();
            pItem.m_Colour = m_Colour;
            return pItem;
        }
    }
}
