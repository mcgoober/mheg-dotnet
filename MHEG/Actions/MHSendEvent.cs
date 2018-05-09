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
using MHEG.Ingredients;

namespace MHEG.Actions
{
    class MHSendEvent: MHElemAction
    {
        MHGenericObjectRef m_EventSource; // Event source
        int m_EventType; // Event type
        MHParameter m_EventData; // Optional - Null means not specified.  Can only be bool, int or string.

        public MHSendEvent()
            : base(":SendEvent")
        {
            m_EventSource = new MHGenericObjectRef();
            m_EventType = 0;
            m_EventData = new MHParameter();
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine); // Target
            m_EventSource.Initialise(p.GetArgN(1), engine);
            m_EventType = p.GetArgN(2).GetEnumValue();
            if (p.GetArgCount() >= 4) {
                // TODO: We could check here that we only have bool, int or string and not object ref or content ref.
                m_EventData.Initialise(p.GetArgN(3), engine);
            }
        }

        public override void Perform(MHEngine engine)
        {
            // The target is always the current scene so we ignore it here.
            MHObjectRef target = new MHObjectRef();
            MHObjectRef source = new MHObjectRef();
            m_Target.GetValue(target, engine); // TODO: Check this is the scene?
            m_EventSource.GetValue(source, engine);
            // Generate the event.
            if (m_EventData.Type == MHParameter.P_Null)
            {
                engine.EventTriggered(engine.FindObject(source), m_EventType);
            }
            else 
            {
                MHUnion data = new MHUnion();
                data.GetValueFrom(m_EventData, engine);
                engine.EventTriggered(engine.FindObject(source), m_EventType, data);
            }
        }

        protected override void PrintArgs(TextWriter writer, int nTabs)
        {
            m_EventSource.Print(writer, 0);
            writer.Write(MHLink.EventTypeToString(m_EventType));
            writer.Write(" ");
            if (m_EventData.Type != MHParameter.P_Null) m_EventData.Print(writer, 0);
        }
    }
}
