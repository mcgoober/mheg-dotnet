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
    class MHSetFontAttributes : MHElemAction
    {
        protected MHGenericOctetString m_FontAttrs; // New font attributes.

        public MHSetFontAttributes()
            : base(":SetFontAttributes")
        {
            m_FontAttrs = new MHGenericOctetString();
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine); // Target
            m_FontAttrs.Initialise(p.GetArgN(1), engine); // New font attrs
        }

        public override void Perform(MHEngine engine)
        {
            // Get the new font attributes.
            MHOctetString newAttrs = new MHOctetString();
            m_FontAttrs.GetValue(newAttrs, engine);
            Target(engine).SetFontAttributes(newAttrs, engine);
        }

        protected override void PrintArgs(TextWriter writer, int nTabs)
        {
            m_FontAttrs.Print(writer, 0);
        }
    }
}
