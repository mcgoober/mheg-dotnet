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


namespace MHEG.Ingredients
{
    class MHLink : MHIngredient
    {
        protected MHObjectRef m_EventSource;
        protected int m_nEventType;
        protected MHUnion m_EventData;
        protected MHActionSequence m_LinkEffect;

        public MHLink()
        {
            m_EventSource = new MHObjectRef();
            m_EventData = new MHUnion();
            m_LinkEffect = new MHActionSequence();
        }
        
        public override string ClassName()
        { 
            return "Link";
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs);
            writer.Write( "{:Link"); base.Print(writer, nTabs+1);
            Logging.PrintTabs(writer, nTabs+1); writer.Write( ":EventSource "); m_EventSource.Print(writer, nTabs+1); writer.Write( "\n");
            Logging.Assert(m_nEventType > 0 && m_nEventType <= rchEventType.Length);
            Logging.PrintTabs(writer, nTabs+1); writer.Write( ":EventType {0}\n", rchEventType[m_nEventType-1]);
            // The event data is optional and its format depends on the event type.
            switch (m_EventData.Type) 
            {
            case MHUnion.U_Bool: Logging.PrintTabs(writer, nTabs+1); writer.Write( ":EventData {0}\n", m_EventData.Bool ? "true" : "false"); break;
            case MHUnion.U_Int: Logging.PrintTabs(writer, nTabs+1); writer.Write( ":EventData {0}\n", m_EventData.Int); break;
            case MHUnion.U_String: Logging.PrintTabs(writer, nTabs+1); writer.Write( ":EventData"); m_EventData.String.Print(writer, nTabs); writer.Write( "\n"); break;
            default: break; // None and others 
            }
            Logging.PrintTabs(writer, nTabs+1); writer.Write( ":LinkEffect (\n");
            m_LinkEffect.Print(writer, nTabs+2);
            Logging.PrintTabs(writer, nTabs+1); writer.Write( ")\n");
            Logging.PrintTabs(writer, nTabs); writer.Write( "}\n");
        }

        // Set this up from the parse tree.
        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            // The link condition is encoded differently in the binary and text representations.
            MHParseNode pLinkCond = p.GetNamedArg(ASN1Codes.C_LINK_CONDITION);
            if (pLinkCond != null) 
            { // Only in binary.
                m_EventSource.Initialise(pLinkCond.GetArgN(0), engine); // Event source
                m_nEventType = pLinkCond.GetArgN(1).GetEnumValue(); // Event type
                // The event data is optional and type-dependent.
                if (pLinkCond.GetArgCount() >= 3) 
                {
                    MHParseNode pEventData = pLinkCond.GetArgN(2);
                    switch (pEventData.NodeType) 
                    {
                        case MHParseNode.PNBool: m_EventData.Bool = pEventData.GetBoolValue(); m_EventData.Type = MHUnion.U_Bool; break;
                        case MHParseNode.PNInt: m_EventData.Int = pEventData.GetIntValue(); m_EventData.Type = MHUnion.U_Int; break;
                        case MHParseNode.PNString: pEventData.GetStringValue(m_EventData.String); m_EventData.Type = MHUnion.U_String; break;
                        default: pEventData.Failure("Unknown type of event data"); break;
                    }
                }
            }
            else 
            { // Only in text.
                MHParseNode pEventSource = p.GetNamedArg(ASN1Codes.P_EVENT_SOURCE); // Event source
                if (pEventSource == null) p.Failure("Missing :EventSource");
                m_EventSource.Initialise(pEventSource.GetArgN(0), engine);
                MHParseNode pEventType = p.GetNamedArg(ASN1Codes.P_EVENT_TYPE); // Event type
                if (pEventType == null) p.Failure("Missing :EventType");
                m_nEventType = pEventType.GetArgN(0).GetEnumValue();
                MHParseNode pEventData = p.GetNamedArg(ASN1Codes.P_EVENT_DATA); // Event data - optional
                if (pEventData != null) 
                {
                    MHParseNode pEventDataArg = pEventData.GetArgN(0);
                    switch (pEventDataArg.NodeType) 
                    {
                        case MHParseNode.PNBool: m_EventData.Bool = pEventDataArg.GetBoolValue(); m_EventData.Type = MHUnion.U_Bool; break;
                        case MHParseNode.PNInt: m_EventData.Int = pEventDataArg.GetIntValue(); m_EventData.Type = MHUnion.U_Int; break;
                        case MHParseNode.PNString: pEventDataArg.GetStringValue(m_EventData.String); m_EventData.Type = MHUnion.U_String; break;
                        default: pEventDataArg.Failure("Unknown type of event data"); break;
                    }
                }
            }

            MHParseNode pLinkEffect = p.GetNamedArg(ASN1Codes.C_LINK_EFFECT);
            m_LinkEffect.Initialise(pLinkEffect, engine);
        }


        public static int GetEventType(string str)
        {
            for (int i = 0; i < rchEventType.Length; i++)
            {
                if (str.Equals(rchEventType[i])) return (i + 1); // Numbered from 1
            }
            return 0;
        }

        public static string EventTypeToString(int ev)
        {
            if (ev > 0 && ev <= rchEventType.Length)
            {
                return rchEventType[ev - 1]; // Numbered from 1
            }
            else
            {
                return "Unknown event " + ev;
            }
        }

        public static string[] rchEventType =
        {
            "IsAvailable",
            "ContentAvailable",
            "IsDeleted",
            "IsRunning",
            "IsStopped",
            "UserInput",
            "AnchorFired",
            "TimerFired",
            "AsyncStopped",
            "InteractionCompleted",
            "TokenMovedFrom",
            "TokenMovedTo",
            "StreamEvent",
            "StreamPlaying",
            "StreamStopped",
            "CounterTrigger",
            "HighlightOn",
            "HighlightOff",
            "CursorEnter",
            "CursorLeave",
            "IsSelected",
            "IsDeselected",
            "TestEvent",
            "FirstItemPresented",
            "LastItemPresented",
            "HeadItems",
            "TailItems",
            "ItemSelected",
            "ItemDeselected",
            "EntryFieldFull",
            "EngineEvent",
            "FocusMoved",
            "SliderValueChanged"
        };

        // Activation.
        public override void Activation(MHEngine engine)
        {
            if (RunningStatus) return;
            base.Activation(engine);
            m_fRunning = true;
            engine.AddLink(this);
            engine.EventTriggered(this, EventIsRunning);
        }

        public override void Deactivation(MHEngine engine)
        {
            if (!RunningStatus) return;
            engine.RemoveLink(this);
            base.Deactivation(engine);
        }

        // Activate or deactivate the link.
        public override void Activate(bool fActivate, MHEngine engine)
        {
            if (fActivate) {
                if (!RunningStatus) Activation(engine);
            }
            else {
                if (RunningStatus) Deactivation(engine);
            }
        }

        // Check this link to see if the event matches the requirements.  If the link does not specify
        // any event data the link fires whatever the value of the data.
        public void MatchEvent(MHObjectRef sourceRefRef, int ev, MHUnion evData, MHEngine engine)
        {
            Logging.Assert(RunningStatus); // Should now be true if we call this.
            if (RunningStatus && m_nEventType == ev && sourceRefRef.Equal(m_EventSource, engine))
            { // Source and event type match.
                bool fMatch = false;
                switch (m_EventData.Type) {
                case MHUnion.U_None: fMatch = true; break; // No data specified - always matches.
                case MHUnion.U_Bool: evData.CheckType(MHUnion.U_Bool);
                    fMatch = evData.Bool == m_EventData.Bool; break;
                case MHUnion.U_Int: evData.CheckType(MHUnion.U_Int); fMatch = evData.Int == m_EventData.Int; break;
                case MHUnion.U_String: evData.CheckType(MHUnion.U_String); fMatch = evData.String.Equal(m_EventData.String); break;
                default: Logging.Log(Logging.MHLogWarning, "Unmatched Event: " + m_EventData.Type); Logging.Assert(false); break; // Should only be the above types.
                }
                // Fire the link
                if (fMatch) {
                    Logging.Log(Logging.MHLogLinks, "Link fired - " + m_ObjectIdentifier.Printable());
                    engine.AddActions(m_LinkEffect);
                }
            }
        }
    }

}
