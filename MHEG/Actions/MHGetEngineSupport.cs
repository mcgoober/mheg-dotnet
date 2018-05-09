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
    class MHGetEngineSupport : MHElemAction
    {
        protected MHGenericOctetString m_Feature;
        protected MHObjectRef m_Answer;

        public MHGetEngineSupport()
            : base(":GetEngineSupport")
        {
            m_Feature = new MHGenericOctetString();
            m_Answer = new MHObjectRef();
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            m_Feature.Initialise(p.GetArgN(1), engine);
            m_Answer.Initialise(p.GetArgN(2), engine);
        }

        public override void Perform(MHEngine engine)
        {
            // Ignore the target which isn't used.
            MHOctetString feature = new MHOctetString();
            m_Feature.GetValue(feature, engine);
            engine.FindObject(m_Answer).SetVariableValue(new MHUnion(engine.GetEngineSupport(feature)));
        }

        protected override void PrintArgs(TextWriter writer, int nTabs)
        {
            m_Feature.Print(writer, 0); 
            m_Answer.Print(writer, 0);
        }
    }
}
