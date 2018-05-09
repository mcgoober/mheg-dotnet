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
using MHEG.Ingredients;
using MHEG.Parser;
using MHEG.Ingredients.Presentable;

namespace MHEG.Groups
{
    abstract class MHGroup : MHRoot
    {
        // Standard ID, Standard version, Object information aren't recorded.
        protected int m_nOrigGroupCachePriority;
        protected MHActionSequence m_StartUp, m_CloseDown;
        MHSequence <MHIngredient> m_Items; // Array of items.
        protected bool m_fIsApp;
 
        // Timers are an attribute of the scene class in the standard but have been moved
        // to the group in UK MHEG.  We record the time that the group starts running so
        // we know how to calculate absolute times.
        TimeSpan m_StartTime;
        protected List<MHTimer> m_Timers = new List<MHTimer>();

        protected int m_nLastId; // Highest numbered ingredient.  Used to make new ids for clones.

        public MHGroup()
        {
            m_Items = new MHSequence<MHIngredient>();
            m_StartUp = new MHActionSequence();
            m_CloseDown = new MHActionSequence();
            m_Timers = new List<MHTimer>();
        }

        public MHActionSequence CloseDown
        {
            get { return m_CloseDown; }
        }

        public MHSequence<MHIngredient> Items
        {
            get { return m_Items; }
        }

        public bool IsApp
        {
            get { return m_fIsApp; }
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            base.Print(writer, nTabs);
            if (m_StartUp.Size != 0) {
                Logging.PrintTabs(writer, nTabs+1); writer.Write( ":OnStartUp (\n");
                m_StartUp.Print(writer, nTabs+2);
                Logging.PrintTabs(writer, nTabs+2); writer.Write( ")\n");
            }
            if (m_CloseDown.Size != 0) {
                Logging.PrintTabs(writer, nTabs+1); writer.Write( ":OnCloseDown (\n");
                m_CloseDown.Print(writer, nTabs+2);
                Logging.PrintTabs(writer, nTabs+2); writer.Write( ")\n");
            }
            if (m_nOrigGroupCachePriority != 127) { Logging.PrintTabs(writer, nTabs + 1); writer.Write(":OrigGCPriority {0}\n", m_nOrigGroupCachePriority); }
            Logging.PrintTabs(writer, nTabs+1); writer.Write( ":Items ( \n");
            for (int i = 0; i < m_Items.Size; i++) m_Items.GetAt(i).Print(writer, nTabs+2);
            Logging.PrintTabs(writer, nTabs+1); writer.Write(")\n");
        }

        public virtual int GetOriginalGroupCachePriority()
        {
            return m_nOrigGroupCachePriority;
        }

        public override void Preparation(MHEngine engine)
        {
            // Prepare the ingredients first if they are initially active or are initially available programs. 
            for (int i = 0; i < m_Items.Size; i++) 
            {
                MHIngredient pIngredient = m_Items.GetAt(i);
                if (pIngredient.InitiallyActive() || pIngredient.InitiallyAvailable()) 
                {
                    pIngredient.Preparation(engine);
                }
            }
            base.Preparation(engine); // Prepare the root object and send the IsAvailable event.
        }

        public override void Activation(MHEngine engine)
        {
            if (RunningStatus) return;
            base.Activation(engine);
            // Run any start-up actions.
            engine.AddActions(m_StartUp);
            engine.RunActions();
            // Activate the ingredients in order.
            for (int i = 0; i < m_Items.Size; i++) 
            {
                MHIngredient pIngredient = m_Items.GetAt(i);
                if (pIngredient.InitiallyActive()) pIngredient.Activation(engine);
            }
            m_fRunning = true;
            // Record the time here.  This is the basis for absolute times.
            m_StartTime = MHTimer.getCurrentTimeSpan();
            // Don't generate IsRunning here - that's done by the sub-classes.
        }

        public override void Deactivation(MHEngine engine)
        {
            if (!RunningStatus) return;
            // Run any close-down actions.
            engine.AddActions(m_CloseDown);
            engine.RunActions();
            base.Deactivation(engine);
        }

        public override void Destruction(MHEngine engine)
        {
            for (int i = m_Items.Size; i > 0; i--)
            {
                m_Items.GetAt(i - 1).Destruction(engine);
            }
            base.Destruction(engine);
        }

        public override MHRoot FindByObjectNo(int n)
        {
            if (n == m_ObjectIdentifier.ObjectNo) return this;
            for (int i = m_Items.Size; i > 0; i--)
            {
                MHRoot pResult = m_Items.GetAt(i-1).FindByObjectNo(n);
                if (pResult != null)
                {
                    return pResult;
                }
            }
            return null;
        }

        // Actions.
        public override void SetTimer(int nTimerId, bool bAbsolute, int nMilliSecs, MHEngine engine)
        {
            // First find any existing timer with the same Id.
            for (int i = 0; i < m_Timers.Count; i++) 
            {
                MHTimer timer = m_Timers[i];
                if (timer.Identifier == nTimerId) 
                {
                    // If the Position wasn't given then we must remove the timer
                    // if it exists, otherwise update the Position
//                    if (nMilliSecs == -1)
//                    {
                        m_Timers.Remove(timer);
//                    }
//                    else
//                    {
//                        timer.Position = nMilliSecs;
//                    }
                    break;
                }
            }

            // If the time has passed we don't set up a timer.
            TimeSpan currentTime = MHTimer.getCurrentTimeSpan();
            if (nMilliSecs < 0 || (bAbsolute && m_StartTime.Add(MHTimer.getMillisecondsTimeSpan(nMilliSecs)) < currentTime)) return;

            long nActualMillis;

            // Adjust the millisec accordingly           
            if (bAbsolute)
            {
                TimeSpan tsMillis = MHTimer.getMillisecondsTimeSpan(nMilliSecs);
                TimeSpan tsResult = m_StartTime.Add(tsMillis);
                nActualMillis = tsResult.Ticks;
            }
            else
            {
                TimeSpan tsMillis = MHTimer.getMillisecondsTimeSpan(nMilliSecs);
                TimeSpan tsResult = currentTime.Add(tsMillis);
                nActualMillis = (long)tsResult.TotalMilliseconds;
            }

            // Create and add the timer
            MHTimer pTimer = new MHTimer(nTimerId, nActualMillis, bAbsolute);
            m_Timers.Add(pTimer);
        }

        // This isn't an MHEG action as such but is used as part of the implementation of "Clone"
        public override void MakeClone(MHRoot pTarget, MHRoot pRef, MHEngine engine)
        {
            MHIngredient pClone = pTarget.Clone(engine); // Clone it.
            pClone.ObjectIdentifier.GroupId.Copy(m_ObjectIdentifier.GroupId); // Group id is the same as this.
            pClone.ObjectIdentifier.ObjectNo = ++m_nLastId; // Create a new object id.
            m_Items.Append(pClone);
            // Set the object reference result to the newly constructed ref.
            pRef.SetVariableValue(new MHUnion(pClone.ObjectIdentifier));
            pClone.Preparation(engine); // Prepare the clone.
        }
        
        // Set this up from the parse tree.
        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            // Set to empty before we start (just in case).
            engine.GetGroupId().Copy(""); 

            base.Initialise(p, engine);

            // Must be an external reference with an object number of zero.
            Logging.Assert(m_ObjectIdentifier.ObjectNo == 0 && m_ObjectIdentifier.GroupId.Size != 0);

            // Set the group id for the rest of the group to this.
            engine.GetGroupId().Copy(m_ObjectIdentifier.GroupId);

            // Some of the information is irrelevant.
        //  MHParseNode pStdId = p.GetNamedArg(C_STANDARD_IDENTIFIER);
        //  MHParseNode pStdVersion = p.GetNamedArg(C_STANDARD_VERSION);
        //  MHParseNode pObjectInfo = p.GetNamedArg(C_OBJECT_INFORMATION);

            MHParseNode pOnStartUp = p.GetNamedArg(ASN1Codes.C_ON_START_UP);
            if (pOnStartUp != null) m_StartUp.Initialise(pOnStartUp, engine);
            MHParseNode pOnCloseDown = p.GetNamedArg(ASN1Codes.C_ON_CLOSE_DOWN);
            if (pOnCloseDown != null) m_CloseDown.Initialise(pOnCloseDown, engine);
            MHParseNode pOriginalGCPrio = p.GetNamedArg(ASN1Codes.C_ORIGINAL_GC_PRIORITY);
            if (pOriginalGCPrio != null) m_nOrigGroupCachePriority = pOriginalGCPrio.GetArgN(0).GetIntValue();

            // Ignore the other stuff at the moment.
            MHParseNode pItems = p.GetNamedArg(ASN1Codes.C_ITEMS);
            if (pItems == null) p.Failure("Missing :Items block");
            for (int i = 0; i < pItems.GetArgCount(); i++) 
            {
                MHParseNode pItem = pItems.GetArgN(i);
                MHIngredient pIngredient = null;

                // Generate the particular kind of ingredient.
                switch (pItem.GetTagNo()) 
                {
                    case ASN1Codes.C_RESIDENT_PROGRAM: pIngredient = new MHResidentProgram(); break;
//  NOT UK                  case ASN1Codes.C_REMOTE_PROGRAM: pIngredient = new MHRemoteProgram(); break;
//  NOT UK                  case ASN1Codes.C_INTERCHANGED_PROGRAM: pIngredient = new MHInterChgProgram(); break;
//  NOT UK                  case ASN1Codes.C_PALETTE: pIngredient = new MHPalette(); break;
//  NOT UK                  case ASN1Codes.C_FONT: pIngredient = new MHFont(); break;
//  NOT UK                  case ASN1Codes.C_CURSOR_SHAPE: pIngredient = new MHCursorShape(); break;
                    case ASN1Codes.C_BOOLEAN_VARIABLE: pIngredient = new MHBooleanVar(); break;
                    case ASN1Codes.C_INTEGER_VARIABLE: pIngredient = new MHIntegerVar(); break;
                    case ASN1Codes.C_OCTET_STRING_VARIABLE: pIngredient = new MHOctetStrVar(); break;
                    case ASN1Codes.C_OBJECT_REF_VARIABLE: pIngredient = new MHObjectRefVar(); break;
                    case ASN1Codes.C_CONTENT_REF_VARIABLE: pIngredient = new MHContentRefVar(); break;
                    case ASN1Codes.C_LINK: pIngredient = new MHLink(); break;
                    case ASN1Codes.C_STREAM: pIngredient = new MHStream(); break;
                    case ASN1Codes.C_BITMAP: pIngredient = new MHBitmap(); break;
                    case ASN1Codes.C_LINE_ART: pIngredient = new MHLineArt(); break;
                    case ASN1Codes.C_DYNAMIC_LINE_ART: pIngredient = new MHDynamicLineArt(); break;
                    case ASN1Codes.C_RECTANGLE: pIngredient = new MHRectangle(); break;
// NOT UK                   case ASN1Codes.C_HOTSPOT: pIngredient = new MHHotSpot(); break;
// NOT UK                   case ASN1Codes.C_SWITCH_BUTTON: pIngredient = new MHSwitchButton(); break;
// NOT UK                   case ASN1Codes.C_PUSH_BUTTON: pIngredient = new MHPushButton(); break;
                    case ASN1Codes.C_TEXT: pIngredient = new MHText(); break;
                    case ASN1Codes.C_ENTRY_FIELD: pIngredient = new MHEntryField(); break;
                    case ASN1Codes.C_HYPER_TEXT: pIngredient = new MHHyperText(); break;
                    case ASN1Codes.C_SLIDER: pIngredient = new MHSlider(); break;
                    case ASN1Codes.C_TOKEN_GROUP: pIngredient = new MHTokenGroup(); break;
                    case ASN1Codes.C_LIST_GROUP: pIngredient = new MHListGroup(); break;
                default:
                    // So we find out about these when debugging.
                    Logging.Log(Logging.MHLogError, "'" + pItem.GetTagNo() + "' tag not in switch");
                    Logging.Assert(false);
                    
                    // Future proofing: ignore any ingredients that we don't know about.
                    // Obviously these can only arise in the binary coding.
                    break;
                }
                if (pIngredient != null) 
                {
                    // Initialise it from its argments.
                    pIngredient.Initialise(pItem, engine);
                    // Remember the highest numbered ingredient
                    if (pIngredient.ObjectIdentifier.ObjectNo > m_nLastId)
                        m_nLastId = pIngredient.ObjectIdentifier.ObjectNo;
                    // Add it to the ingedients of this group.
                    m_Items.Append(pIngredient);
                }
            }
        }

        // Checks the timers and fires any relevant events.  Returns the millisecs to the
        // next event or zero if there aren't any.
        public int CheckTimers(MHEngine engine)
        {
            TimeSpan currentTime = MHTimer.getCurrentTimeSpan(); // Get current time
            int nMSecs = 0;

            List<MHTimer> executedTimers = new List<MHTimer>();

            for (int i = 0; i < m_Timers.Count; i++) 
            {
                MHTimer timer = m_Timers[i];

                // Use <= rather than < here so we fire timers with zero time immediately.
                if (timer.PositionTimeSpan <= currentTime)
                { 
                    // If the time has passed trigger the event and remove the timer from the queue.
                    engine.EventTriggered(this, EventTimerFired, new MHUnion(timer.Identifier));
                    executedTimers.Add(timer); // Add to list of executed timers which are removed later
                }
                else 
                {
                    // This has not yet expired.  Set "nMSecs" to the earliest time we have.
                    int nMSecsToGo = timer.PositionTimeSpan.Subtract(currentTime).Milliseconds;
                    if (nMSecs == 0 || nMSecsToGo < nMSecs) nMSecs = nMSecsToGo;                    
                }
            }

            // Remove all executed timers
            for (int i = 0; i < executedTimers.Count; i++)
            {
                MHTimer timer = executedTimers[i];
                m_Timers.Remove(timer);
            }

            return nMSecs;
        }
    }
}
