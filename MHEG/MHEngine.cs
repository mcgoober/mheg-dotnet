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
using System.Drawing;
using System.Text;
using System.IO;
using MHEG.Groups;
using MHEG.Actions;
using MHEG.Ingredients;
using MHEG.Ingredients.Presentable;
using MHEG.Parser;

namespace MHEG
{
    class MHEngine : IMHEG
    {
        Region m_redrawRegion = new Region(); // The accumulation of repaints when the screen is locked.

        // Application stack and functions to get the current application and scene.
        protected Stack<MHApplication> m_ApplicationStack;

        // Action stack.  Actions may generate synchronous events which fire links and add
        // new actions.  These new actions have to be processed before we continue with other
        // actions.
        protected Stack<MHElemAction> m_ActionStack;

        // Asynchronous event queue.  Asynchronous events are added to this queue and handled
        // once the action stack is empty.
        protected List<MHAsynchEvent> m_EventQueue;

        // Active Link set.  Active links are included in this table.
        protected List<MHLink> m_LinkTable;

        // Pending external content.  If we have requested external content that has not yet arrived
        // we make an entry in this table.
        protected List<MHExternContent> m_ExternContentTable;

        protected MHSequence<MHPSEntry> m_PersistentStore;
        
        // If we get a TransitionTo, Quit etc during OnStartUp and OnCloseDown we ignore them.
        protected bool m_fInTransition; 

        // To canonicalise the object ids we set this to the group id of the current scene or app
        // and use that wherever we get an object id without a group id.
        protected MHOctetString m_CurrentGroupId;
        
        // Reference to the context providing drawing and other operations
        protected IMHContext m_Context;
        protected bool m_fBooting;

        public MHEngine(IMHContext context)
        {
            m_Context = context;
            m_fInTransition = false;
            m_fBooting = true;

            m_redrawRegion.MakeEmpty();

            m_ApplicationStack = new Stack<MHApplication>();
            m_ActionStack = new Stack<MHElemAction>();
            m_EventQueue = new List<MHAsynchEvent>();
            m_LinkTable = new List<MHLink>();
            m_ExternContentTable = new List<MHExternContent>();
            m_PersistentStore = new MHSequence<MHPSEntry>();
            m_CurrentGroupId = new MHOctetString();
        }

        public void SetBooting()
        {
            m_fBooting = true;
        }

        public void PrintCurrentApp(TextWriter writer)
        {
            MHApplication application = CurrentApp();
            application.Print(writer, 0);
        }

        public void DrawDisplay(Region toDraw)
        {
            if (m_fBooting) return;
            int nTopStack = CurrentApp() == null ? -1 : CurrentApp().DisplayStack.Size - 1;
            DrawRegion(toDraw, nTopStack);
        }

        public void BootApplication(string fileName)
        {
            // no definition provided during port
        }

        public void TransitionToScene(MHObjectRef target)
        {
            int i;
            if (m_fInTransition)
            {
                // TransitionTo is not allowed in OnStartUp or OnCloseDown actions. 
                Logging.Log(Logging.MHLogWarning, "TransitionTo during transition - ignoring");
                return;
            }
            if (target.GroupId.Size == 0) return; // No file name.
            string csPath = GetPathName(target.GroupId);

            // Check that the file exists before we commit to the transition.
            byte[] text;
            if (!m_Context.GetCarouselData(csPath, out text)) return;

            // Parse and run the file.
            MHGroup pProgram = ParseProgram(text);
            if (pProgram.IsApp) throw new MHEGException("Expected a scene");
            // Clear the action queue of anything pending.
            m_ActionStack.Clear();

            // At this point we have managed to load the scene.
            // Deactivate any non-shared ingredients in the application.
            MHApplication pApp = CurrentApp();
            for (i = pApp.Items.Size; i > 0; i--)
            {
                MHIngredient pItem = pApp.Items.GetAt(i - 1);
                if (!pItem.IsShared()) pItem.Deactivation(this); // This does not remove them from the display stack.
            }
            m_fInTransition = true; // TransitionTo etc are not allowed.
            if (pApp.CurrentScene != null)
            {
                pApp.CurrentScene.Deactivation(this); // This may involve a call to RunActions
                pApp.CurrentScene.Destruction(this);
            }
            // Everything that belongs to the previous scene should have been removed from the display stack.

            // At this point we may have added actions to the queue as a result of synchronous
            // events during the deactivation.

            // Remove any events from the asynch event queue unless they derive from
            // the application itself or a shared ingredient.
            List<MHAsynchEvent> removeEvents = new List<MHAsynchEvent>();
            foreach(MHAsynchEvent e in m_EventQueue)
            {
                if (!e.EventSource.IsShared())
                {
                    removeEvents.Add(e);
                }
            }
            foreach(MHAsynchEvent e in removeEvents)
            {
                m_EventQueue.Remove(e);
            }            

            // Can now actually delete the old scene.
            if (pApp.CurrentScene != null)
            {
                pApp.CurrentScene = null;
            }
            
            // Switch to the new scene.
            CurrentApp().CurrentScene = (MHScene)pProgram;
            SetInputRegister(CurrentScene().EventReg);
            
            m_redrawRegion = new Region(new Rectangle(0, 0, CurrentScene().SceneCoordX, CurrentScene().SceneCoordY)); // Redraw the whole screen

            if ((Logging.GetLoggingLevel() & Logging.MHLogScenes) != 0)
            { // Print it so we know what's going on.
                pProgram.Print(Logging.GetLoggingStream(), 0);
            }

            pProgram.Preparation(this);
            pProgram.Activation(this);
            m_fInTransition = false; // The transition is complete
        }

        public bool Launch(MHObjectRef target, bool fIsSpawn)
        {
            string csPath = GetPathName(target.GroupId); // Get path relative to root.
            if (csPath.Length == 0) return false; // No file name.
            if (m_fInTransition) 
            {
                Logging.Log(Logging.MHLogWarning, "Launch during transition - ignoring");
                return false;
            }
            // Check that the file exists before we commit to the transition.
            // This may block if we cannot be sure whether the object is present.
            byte[] text;
            if (!m_Context.GetCarouselData(csPath, out text)) return false;

            m_fInTransition = true; // Starting a transition
            try 
            {
                if (CurrentApp() != null) 
                {                    
                    if (fIsSpawn) 
                    {
                        // Run the CloseDown actions.
                        AddActions(CurrentApp().CloseDown);
                        RunActions();
                    }
                    if (CurrentScene() != null) CurrentScene().Destruction(this);
                    CurrentApp().Destruction(this);
                    if (! fIsSpawn) m_ApplicationStack.Pop(); // Pop and delete the current app.
                }

                MHApplication pProgram = (MHApplication)ParseProgram(text);

                if ((Logging.GetLoggingLevel() & Logging.MHLogScenes) != 0) { // Print it so we know what's going on.
                    pProgram.Print(Logging.GetLoggingStream(), 0);
                }

                if (! pProgram.IsApp) throw new MHEGException("Expected an application");

                // Save the path we use for this app.
                pProgram.Path = csPath; // Record the path
                int nPos = pProgram.Path.LastIndexOf('/');
                if (nPos < 0) pProgram.Path = "";
                else pProgram.Path = pProgram.Path.Substring(0, nPos);
               // Have now got the application.
               m_ApplicationStack.Push(pProgram);

               // This isn't in the standard as far as I can tell but we have to do this because
               // we may have events referring to the old application.
               m_EventQueue.Clear();

               // Activate the application. ....
               CurrentApp().Activation(this);
               m_fInTransition = false; // The transition is complete
               return true;
            }
            catch (MHEGException) 
            {
                m_fInTransition = false; // The transition is complete
                return false;
            }
        }

        public bool Launch(MHObjectRef target)
        {
            return Launch(target, false);
        }

        public void Spawn(MHObjectRef target) 
        { 
            Launch(target, true); 
        }

        public void Quit()
        {
            if (m_fInTransition)
            {
                Logging.Log(Logging.MHLogWarning, "Quit during transition - ignoring");
                return;
            }
            m_fInTransition = true; // Starting a transition
            if (CurrentScene() != null) CurrentScene().Destruction(this);
            CurrentApp().Destruction(this);
            // This isn't in the standard as far as I can tell but we have to do this because
            // we may have events referring to the old application.
            m_EventQueue.Clear();

            m_ApplicationStack.Pop();
            // If the stack is now empty we return to boot mode.
            if (m_ApplicationStack.Count == 0)
            {
                m_fBooting = true;
            }
            else
            {
                CurrentApp().Restarting = true;
                CurrentApp().Activation(this); // This will do any OnRestart actions.
                // Note - this doesn't activate the previously active scene.
            }
            m_fInTransition = false; // The transition is complete
        }
 
        // Look up an object by its object reference.  In nearly all cases we want to throw
        // an exception if it isn't found.  In a very few cases where we don't fail this
        // returns NULL if it isn't there.
        public MHRoot FindObject(MHObjectRef objr)
        {
            return FindObject(objr, true);
        }

        public MHRoot FindObject(MHObjectRef oRef, bool failOnNotFound)
        {
            // It should match either the application or the scene.
            MHGroup pSearch = null;
            MHGroup pScene = CurrentScene();
            MHGroup pApp = CurrentApp();
            if (pScene != null && GetPathName(pScene.ObjectIdentifier.GroupId) == GetPathName(oRef.GroupId)) pSearch = pScene;
            else if (pApp != null && GetPathName(pApp.ObjectIdentifier.GroupId) == GetPathName(oRef.GroupId)) pSearch = pApp;
            if (pSearch != null) 
            {
                MHRoot pItem = pSearch.FindByObjectNo(oRef.ObjectNo);
                if (pItem != null) return pItem;
            }
            if (failOnNotFound)
            {
                // I've seen at least one case where MHEG code has quite deliberately referred to
                // an object that may or may not exist at a particular time.
                // Another case was a call to CallActionSlot with an object reference variable
                // that had been initialised to zero.
                Logging.Log(Logging.MHLogWarning, "Reference " + oRef.ObjectNo + " not found");
                throw new MHEGException("FindObject failed");
            }
            return null; // If we don't generate an error.
        }

        // Called when an event is triggered.  Either queues the event or finds a link that matches.
        public void EventTriggered(MHRoot pSource, int eventType)
        {
            EventTriggered(pSource, eventType, new MHUnion()); 
        }

        public void EventTriggered(MHRoot pSource, int ev, MHUnion evData)
        {
            Logging.Log(Logging.MHLogLinks, "Event - " + MHLink.EventTypeToString(ev) + " from " + pSource.ObjectIdentifier.Printable());

            switch (ev) 
            {
            case MHRoot.EventFirstItemPresented:
            case MHRoot.EventHeadItems:
            case MHRoot.EventHighlightOff:
            case MHRoot.EventHighlightOn:
            case MHRoot.EventIsAvailable:
            case MHRoot.EventIsDeleted:
            case MHRoot.EventIsDeselected:
            case MHRoot.EventIsRunning:
            case MHRoot.EventIsSelected:
            case MHRoot.EventIsStopped:
            case MHRoot.EventItemDeselected:
            case MHRoot.EventItemSelected:
            case MHRoot.EventLastItemPresented:
            case MHRoot.EventTailItems:
            case MHRoot.EventTestEvent:
            case MHRoot.EventTokenMovedFrom:
            case MHRoot.EventTokenMovedTo:
                // Synchronous events.  Fire any links that are waiting.
                // The UK MHEG describes this as the preferred interpretation.  We are checking the link
                // at the time we generate the event rather than queuing the synchronous events until
                // this elementary action is complete.  That matters if we are processing an elementary action
                // which will activate or deactivate links.
                CheckLinks(pSource.ObjectIdentifier, ev, evData);
                break;
            case MHRoot.EventAnchorFired:
            case MHRoot.EventAsyncStopped:
            case MHRoot.EventContentAvailable:
            case MHRoot.EventCounterTrigger:
            case MHRoot.EventCursorEnter:
            case MHRoot.EventCursorLeave:
            case MHRoot.EventEngineEvent:
            case MHRoot.EventEntryFieldFull:
            case MHRoot.EventInteractionCompleted:
            case MHRoot.EventStreamEvent:
            case MHRoot.EventStreamPlaying:
            case MHRoot.EventStreamStopped:
            case MHRoot.EventTimerFired:
            case MHRoot.EventUserInput:
            case MHRoot.EventFocusMoved: // UK MHEG.  Generated by HyperText class
            case MHRoot.EventSliderValueChanged: // UK MHEG.  Generated by Slider class
                {
                    // Asynchronous events.  Add to the event queue.
                    MHAsynchEvent pEvent = new MHAsynchEvent();
                    pEvent.EventSource = pSource;
                    pEvent.EventType = ev;
                    pEvent.EventData = evData;
                    m_EventQueue.Add(pEvent);
                } break;
            }            
        }

        // Called when a link fires to add the actions to the action stack.
        public void AddActions(MHActionSequence actions)
        {
            // Put them on the stack in reverse order so that we will pop the first.
            for (int i = actions.Size; i > 0; i--) m_ActionStack.Push(actions.GetAt(i - 1));
        }

        // Display stack and draw functions.
        public void AddToDisplayStack(MHVisible pVis)
        {
            if (CurrentApp().FindOnStack(pVis) != -1) return; // Return if it's already there.
            CurrentApp().DisplayStack.Append(pVis);
            Redraw(pVis.GetVisibleArea()); // Request a redraw
        }

        public void RemoveFromDisplayStack(MHVisible pVis)
        {
            int nPos = CurrentApp().FindOnStack(pVis);
            if (nPos == -1) return;
            CurrentApp().DisplayStack.RemoveAt(nPos);
            Redraw(pVis.GetVisibleArea()); // Request a redraw
        }

        // Request a redraw.
        public void Redraw(Region region)
        {
            m_redrawRegion.Union(region);
        }
 
        // Functions to alter the Z-order.
        public void BringToFront(MHRoot p)
        {
            int nPos = CurrentApp().FindOnStack(p);
            if (nPos == -1) return; // If it's not there do nothing
            MHVisible pVis = (MHVisible)p; // Can now safely cast it.
            CurrentApp().DisplayStack.RemoveAt(nPos); // Remove it from its present posn
            CurrentApp().DisplayStack.Append((MHVisible)pVis); // Push it on the top.
            Redraw(pVis.GetVisibleArea()); // Request a redraw       
        }

        public void SendToBack(MHRoot p)
        {
            int nPos = CurrentApp().FindOnStack(p);
            if (nPos == -1) return; // If it's not there do nothing
            MHVisible pVis = (MHVisible)p; // Can now safely cast it.
            CurrentApp().DisplayStack.RemoveAt(nPos); // Remove it from its present posn
            CurrentApp().DisplayStack.InsertAt(pVis, 0); // Put it on the bottom.
            Redraw(pVis.GetVisibleArea()); // Request a redraw
        }
        
        public void PutBefore(MHRoot p, MHRoot pRef)
        {
            int nPos = CurrentApp().FindOnStack(p);
            if (nPos == -1) return; // If it's not there do nothing
            MHVisible pVis = (MHVisible)p; // Can now safely cast it.
            int nRef = CurrentApp().FindOnStack(pRef);
            if (nRef == -1) return; // If the reference visible isn't there do nothing.
            CurrentApp().DisplayStack.RemoveAt(nPos);
            if (nRef >= nPos) nRef--; // The position of the reference may have shifted
            CurrentApp().DisplayStack.InsertAt(pVis, nRef + 1);
            // Redraw the area occupied by the moved item.  We might be able to reduce
            // the area to be redrawn by looking at the way it is affected by other items
            // in the stack.  We could also see whether it's currently active.
            Redraw(pVis.GetVisibleArea()); // Request a redraw
        }

        public void PutBehind(MHRoot p, MHRoot pRef)
        {
            int nPos = CurrentApp().FindOnStack(p);
            if (nPos == -1) return; // If it's not there do nothing
            int nRef = CurrentApp().FindOnStack(pRef);
            if (nRef == -1) return; // If the reference visible isn't there do nothing.
            MHVisible pVis = (MHVisible)p; // Can now safely cast it.
            CurrentApp().DisplayStack.RemoveAt(nPos);
            if (nRef >= nPos) nRef--; // The position of the reference may have shifted
            CurrentApp().DisplayStack.InsertAt((MHVisible)pVis, nRef); // Shift the reference and anything above up.
            Redraw(pVis.GetVisibleArea()); // Request a redraw
        }

        public void LockScreen() 
        { 
            CurrentApp().LockCount++; 
        }

        public void UnlockScreen()
        {
            if (CurrentApp().LockCount > 0) CurrentApp().LockCount--;
        }

        public int RunAll()
        {
            // Request to boot or reboot
            if (m_fBooting) 
            {
                // Reset everything
                m_ApplicationStack.Clear();
                m_EventQueue.Clear();
                m_ExternContentTable.Clear();
                m_LinkTable.Clear();

                // UK MHEG applications boot from ~//a or ~//startup.  Actually the initial
                // object can also be explicitly given in the 
                MHObjectRef startObj = new MHObjectRef();
                startObj.ObjectNo = 0;
                startObj.GroupId.Copy(new MHOctetString("~//a"));
                // Launch will block until either it finds the appropriate object and
                // begins the application or discovers that the file definitely isn't
                // present in the carousel.  It is possible that the object might appear
                // if one of the containing directories is updated.
                if (! Launch(startObj))
                {
                     startObj.GroupId.Copy(new MHOctetString("~//startup"));
                     if (! Launch(startObj))
                     {
                         //Logging.Log(Logging.MHLogError, "Unable to launch application");
                         return -1;
                     }
                }
                m_fBooting = false;
            }

            int nNextTime = 0;
            do 
            {
                // Check to see if we need to close.
                if (m_Context.CheckStop()) return 0;

                // Run queued actions.
                RunActions();
                // Now the action stack is empty process the next asynchronous event.
                // Processing one event may affect how subsequent events are handled.

                // Check to see if some files we're waiting for have arrived.
                // This could result in ContentAvailable events.
                CheckContentRequests();

                // Check the timers.  This may result in timer events being raised.
                if (CurrentScene() != null) 
                {
                    int next = CurrentScene().CheckTimers(this);
                    if (nNextTime == 0 || nNextTime > next) nNextTime = next;
                }
                if (CurrentApp() != null) 
                {
                    // The UK MHEG profile allows applications to have timers.
                    int nAppTime = CurrentApp().CheckTimers(this);
                    if (nAppTime != 0 && (nNextTime == 0 || nAppTime < nNextTime)) nNextTime = nAppTime;
                }
                if (m_ExternContentTable.Count != 0) 
                {
                    // If we have an outstanding request for external content we need to set a timer.
                    if (nNextTime == 0 || nNextTime > CONTENT_CHECK_TIME) nNextTime = CONTENT_CHECK_TIME;
                }

                if (m_EventQueue.Count != 0) 
                {
                    MHAsynchEvent pEvent = m_EventQueue[0];
                    Logging.Log(Logging.MHLogLinks, "Asynchronous event dequeued - " + MHLink.EventTypeToString(pEvent.EventType)
                        + " from " + pEvent.EventSource.ObjectIdentifier.Printable());
                    CheckLinks(pEvent.EventSource.ObjectIdentifier, pEvent.EventType, pEvent.EventData);
                    m_EventQueue.Remove(pEvent);
                }
            } while (m_EventQueue.Count != 0 || m_ActionStack.Count != 0);

            // Redraw the display if necessary.
            if (!IsRegionEmpty(m_redrawRegion))
            {
                m_Context.RequireRedraw(m_redrawRegion);
                m_redrawRegion = new Region();
                m_redrawRegion.MakeEmpty();
            }

            return nNextTime;
        }

        // Run synchronous actions.
        public void RunActions()
        {
            while (m_ActionStack.Count != 0) 
            {
                // Remove the first action.
                MHElemAction pAction = m_ActionStack.Pop();
                
                // Output debug information
                if ((Logging.GetLoggingLevel() & Logging.MHLogActions) != 0)
                {
                    Logging.GetLoggingStream().Write("Action - ");
                    pAction.Print(Logging.GetLoggingStream(), 0);
                }

                // Run it.  If it fails and throws an exception catch it and continue with the next.
                try
                {
                    pAction.Perform(this);
                }
                catch (MHEGException e)
                {
                    Logging.GetLoggingStream().WriteLine(e.Message);
                    Logging.GetLoggingStream().WriteLine(e.StackTrace);
                }
            }
        }


        public void GenerateUserAction(int nCode)
        {
            MHScene pScene = CurrentScene();
            if (pScene == null) return;
            EventTriggered(pScene, MHRoot.EventUserInput, new MHUnion(nCode));
        }

        // Called from an ingredient to request a load of external content.
        public void RequestExternalContent(MHIngredient pRequester)
        {
            // It seems that some MHEG applications contain active ingredients with empty contents
            // This isn't correct but we simply ignore that.
            if (! pRequester.ContentRef.IsSet()) return;
            // Remove any existing content requests for this ingredient.
            CancelExternalContentRequest(pRequester);
            string csPath = GetPathName(pRequester.ContentRef.ContentRef);
            // Is this actually a carousel object?  It could be a stream.  We should deal
            // with that separately.
            if (csPath.Length == 0)
                return;
            byte[] text;
            if (m_Context.CheckCarouselObject(csPath) && m_Context.GetCarouselData(csPath, out text)) {
                // Available now - pass it to the ingredient.
                pRequester.ContentArrived(text, this);
            }
            else {
                // Need to record this and check later.
                MHExternContent pContent = new MHExternContent();
                pContent.FileName = csPath;
                pContent.Requester = pRequester;
                m_ExternContentTable.Add(pContent);
            }
        }

        public void CancelExternalContentRequest(MHIngredient pRequester)
        {
            foreach(MHExternContent content in m_ExternContentTable)
            {
                if (content.Requester == pRequester)
                {
                    m_ExternContentTable.Remove(content);
                    return;
                }
            }
        }

        // Load from or store to the persistent store.
        public bool LoadStorePersistent(bool fIsLoad, MHOctetString fileName, MHSequence<MHObjectRef> variables)
        {
            return false;
        }

        // Add and remove links to and from the active link table.
        public void AddLink(MHLink pLink)
        {
#if DEBUG
            // Should not be there already.
            for (int i = 0; i < m_LinkTable.Count; i++)
            {
                Logging.Assert(pLink != m_LinkTable[i]);
            }
#endif
            m_LinkTable.Add(pLink);
        }

        public void RemoveLink(MHLink pLink)
        {
            bool fRes = m_LinkTable.Remove(pLink);
            Logging.Assert(fRes); // The link should have been there.
        }

        public bool InTransition() 
        { 
            return m_fInTransition; 
        }

        public bool GetEngineSupport(MHOctetString feature)
        {
            string csFeat = feature.ToString();
            string[] strings = csFeat.Split(new char[] {'(', ')', ','});

            if (strings[0] == "ApplicationStacking" || strings[0] == "ASt") return true;
            // We're required to support cloning for Text, Bitmap and Rectangle.
            if (strings[0] == "Cloning" || strings[0] == "Clo") return true; 
            if (strings[0] == "SceneCoordinateSystem" || strings[0] == "SCS") {
                if (strings.Length >= 3 && strings[1] == "720" && strings[2] == "576")
                    return true;
                else return false;
             // I've also seen SceneCoordinateSystem(1,1)
            }
            if (strings[0] == "MultipleAudioStreams" || strings[0] == "MAS") {
                if (strings.Length >= 2 && (strings[1] == "0" || strings[1] == "1"))
                    return true;
                else return false;
            }
            if (strings[0] == "MultipleVideoStreams" || strings[0] == "MVS") {
                if (strings.Length >= 2 && (strings[1] == "0" || strings[1] == "1"))
                    return true;
                else return false;
            }
            // We're supposed to return true for all values of N
            if (strings[0] == "OverlappingVisibles" || strings[0] == "OvV") return true;

            if (strings[0] == "SceneAspectRatio" || strings[0] == "SAR") {
                if (strings.Length < 3) return false;
                else if ((strings[1] == "4" && strings[2] == "3") || (strings[1] == "16" && strings[2] == "9"))
                    return true;
                else return false;
            }

            // We're supposed to support these at least.  May also support(10,1440,1152)
            if (strings[0] == "VideoScaling" || strings[0] == "VSc") {
                if (strings.Length < 4 || strings[1] != "10") return false;
                else if ((strings[2] == "720" && strings[3] == "576") || (strings[2] == "360" && strings[3] == "288"))
                    return true;
                else return false;
            }
            if (strings[0] == "BitmapScaling" || strings[0] == "BSc") {
                if (strings.Length < 4 || strings[1] != "2") return false;
                else if ((strings[2] == "720" && strings[3] == "576") || (strings[2] == "360" && strings[3] == "288"))
                    return true;
                else return false;
            }

            // I think we only support the video fully on screen
            if (strings[0] == "VideoDecodeOffset" || strings[0] == "VDO") {
                if (strings.Length >= 3 && strings[1] == "10" && strings[1] == "0") return true;
                else return false;
            }
            // We support bitmaps that are partially off screen (don't we?)
            if (strings[0] == "BitmapDecodeOffset" || strings[0] == "BDO") {
                if (strings.Length >= 3 && strings[1] == "10" && (strings[2] == "0" || strings[2] == "1"))
                    return true;
                else return false;
            }

            if (strings[0] == "UKEngineProfile" || strings[0] == "UEP") {
                if (strings.Length < 2) return false;
                if (strings[1] == MHEGEngineProviderIdString)
                    return true;
                if (strings[1] == m_Context.GetReceiverId())
                    return true;
                if (strings[1] == m_Context.GetDSMCCId())
                    return true;
                // The UK profile 1.06 seems a bit confused on this point.  It is not clear whether
                // we are supposed to return true for UKEngineProfile(2) or not.
                if (strings[1] == "2")
                    return true;
                else return false;
            }
            // Otherwise return false.
            return false;
        }

        // Get the various defaults.  These are extracted from the current app or the (UK) MHEG defaults.
        public int GetDefaultCharSet()
        {
            MHApplication pApp = CurrentApp();
            if (pApp != null && pApp.CharSet > 0) return pApp.CharSet;
            else return 10; // UK MHEG default.        
        }

        public void GetDefaultBGColour(MHColour colour)
        {
            MHApplication pApp = CurrentApp();
            if (pApp != null && pApp.BGColour.IsSet()) colour.Copy(pApp.BGColour);
            else colour.SetFromString("\u0000\u0000\u0000\u00FF"); // '=00=00=00=FF' Default - transparent
        }

        public void GetDefaultTextColour(MHColour colour)
        {
            MHApplication pApp = CurrentApp();
            if (pApp != null && pApp.TextColour.IsSet()) colour.Copy(pApp.TextColour);
            else colour.SetFromString("\u00FF\u00FF\u00FF\u0000"); // '=FF=FF=FF=00' UK MHEG Default - white
        }

        public void GetDefaultButtonRefColour(MHColour colour)
        {
            MHApplication pApp = CurrentApp();
            if (pApp != null && pApp.ButtonRefColour.IsSet()) colour.Copy(pApp.ButtonRefColour);
            else colour.SetFromString("\u00FF\u00FF\u00FF\u0000"); // '=FF=FF=FF=00' ??? Not specified in UK MHEG       
        }

        public void GetDefaultHighlightRefColour(MHColour colour)
        {
            MHApplication pApp = CurrentApp();
            if (pApp != null && pApp.HighlightRefColour.IsSet()) colour.Copy(pApp.HighlightRefColour);
            else colour.SetFromString("\u00FF\u00FF\u00FF\u0000"); // '=FF=FF=FF=00' UK MHEG Default - white
        }
        public void GetDefaultSliderRefColour(MHColour colour)
        {
            MHApplication pApp = CurrentApp();
            if (pApp != null && pApp.SliderRefColour.IsSet()) colour.Copy(pApp.SliderRefColour);
            else colour.SetFromString("\u00FF\u00FF\u00FF\u0000"); // '=FF=FF=FF=00' UK MHEG Default - white
        }
        public int GetDefaultTextCHook()
        {
            MHApplication pApp = CurrentApp();
            if (pApp != null && pApp.TextCHook > 0) return pApp.TextCHook;
            else return 10; // UK MHEG default.        
        }
        public int GetDefaultStreamCHook()
        {
            MHApplication pApp = CurrentApp();
            if (pApp != null && pApp.StrCHook > 0) return pApp.StrCHook;
            else return 10; // UK MHEG default.
        }
        public int GetDefaultBitmapCHook()
        {
            MHApplication pApp = CurrentApp();
            if (pApp != null && pApp.BitmapCHook > 0) return pApp.BitmapCHook;
            else return 4; // UK MHEG default - PNG bitmap
        }
    //  void GetDefaultFont(MHFontBody &font); // Not currently implemented
        public void GetDefaultFontAttrs(MHOctetString str)
        {
            MHApplication pApp = CurrentApp();
            if (pApp != null && pApp.FontAttrs.Size > 0) str.Copy(pApp.FontAttrs);
            else str.Copy("plain.24.24.0"); // TODO: Check this.
        }
        public void SetInputRegister(int nReg)
        {
            m_Context.SetInputRegister(nReg); // Enable the appropriate buttons
        }

        public MHOctetString GetGroupId() 
        { 
            return m_CurrentGroupId; 
        }
        
        public IMHContext GetContext() 
        { 
            return m_Context; 
        }
        
        // Return a path relative to the home directory
        public string GetPathName(MHOctetString str)
        {
            string csPath = "";
            if (str.Size != 0) csPath = str.ToString();
            if (csPath.StartsWith("DSM:")) csPath = csPath.Substring(4); // Remove DSM:
            // If it has any other prefix this isn't a request for a carousel object.
            int firstColon = csPath.IndexOf(':'), firstSlash = csPath.IndexOf('/');
            if (firstColon > 0 && firstSlash > 0 && firstColon < firstSlash)
                return "";

            if (csPath.StartsWith("~")) csPath = csPath.Substring(1); // Remove ~
            // Ignore "CI://"
            if (!csPath.StartsWith("//")) 
            { // 
                // Add the current application's path name
                if (CurrentApp() != null) csPath = CurrentApp().Path + csPath;
            }
            // Remove any occurrences of x/../
            int nPos;
            while ((nPos = csPath.IndexOf("/../")) >= 0) 
            {
                Logging.Log(Logging.MHLogWarning, "/../ found in path " + csPath); // To check.                
                int nEnd = nPos+4;
                while (nPos >= 1 && csPath[nPos-1] != '/') nPos--;
                csPath = csPath.Substring(0, nPos) + csPath.Substring(nEnd);
            }
            return csPath;
        }

        protected void CheckLinks(MHObjectRef sourceRef, int eventType, MHUnion un)
        {
            for (int i = 0; i < (int)m_LinkTable.Count; i++)
            {
                m_LinkTable[i].MatchEvent(sourceRef, eventType, un, this);
            }
        }

        protected MHGroup ParseProgram(byte[] text)
        {
            if (text.Length == 0) return null;
            
            IMHParser parser = null;
            MHParseNode pTree = null;
            MHGroup pRes = null;

            // Look at the first byte to decide whether this is text or binary.  Binary
            // files will begin with 0xA0 or 0xA1, text files with white space, comment ('/')
            // or curly bracket.
            // This is only there for testing: all downloaded objects will be in ASN1
            byte ch = text[0];
            if (ch >= 128) parser = new MHParseBinary(text);
            else parser = new MHParseText(text);            

            // Parse the binary or text.
            pTree = parser.Parse();

            switch (pTree.GetTagNo()) 
            { // The parse node should be a tagged item.
                case ASN1Codes.C_APPLICATION: pRes = new MHApplication(); break;
                case ASN1Codes.C_SCENE: pRes = new MHScene(); break;
                default: pTree.Failure("Expected Application or Scene"); break; // throws exception.
            }
            pRes.Initialise(pTree, this); // Convert the parse tree.

            return pRes;
        }

        protected void DrawRegion(Region toDraw, int nStackPos)
        {
            if (IsRegionEmpty(toDraw)) return;

            while (nStackPos >= 0)
            {
                MHVisible pItem = CurrentApp().DisplayStack.GetAt(nStackPos);
                // Work out how much of the area we want to draw is included in this visible.
                // The visible area will be empty if the item is transparent or not active.
                Region drawArea = pItem.GetVisibleArea().Clone();
                drawArea.Intersect(toDraw);

                if (!IsRegionEmpty(drawArea))
                { // It contributes something.
                    // Remove the opaque area of this item from the region we have left.
                    // If this item is (semi-)transparent this will not remove anything.
                    Region newDraw = toDraw.Clone();
                    newDraw.Exclude(pItem.GetOpaqueArea());
                    DrawRegion(newDraw, nStackPos - 1); // Do the items further down if any.
                    // Now we've drawn anything below this we can draw this item on top.
                    pItem.Display(this);
                    return;
                }
                nStackPos--;
            }
            // We've drawn all the visibles and there's still some undrawn area.
            // Fill it with black.
            m_Context.DrawBackground(toDraw);
        }


        protected void CheckContentRequests()
        {
            List<MHExternContent> removeList = new List<MHExternContent>();
            foreach (MHExternContent content in m_ExternContentTable) 
            {
                byte[] text;
                if (m_Context.CheckCarouselObject(content.FileName) && m_Context.GetCarouselData(content.FileName, out text)) 
                {
                    content.Requester.ContentArrived(text, this);
                    // Remove from the list.
                    removeList.Add(content);                    
                }                
            }
            foreach (MHExternContent content in removeList)
            {
                m_ExternContentTable.Remove(content);
            }
        }

        protected bool IsRegionEmpty(Region region)
        {
            if (CurrentScene() != null)
            {
                return !region.IsVisible(new Rectangle(0, 0, CurrentScene().SceneCoordX, CurrentScene().SceneCoordY));
            }
            return region.IsVisible(0, 0);
        }

        protected MHApplication CurrentApp() 
        {
            if (m_ApplicationStack.Count == 0)
            {
                return null;
            }
            else
            {
                return m_ApplicationStack.Peek();
            }
        }

        protected MHScene CurrentScene() 
        { 
            return CurrentApp() == null ? null : CurrentApp().CurrentScene; 
        }

        // Check for external content every 2 seconds.
        public const int CONTENT_CHECK_TIME = 2000;

        // An identifier string required by the UK profile.  The "manufacturer" is GNU.
        public const String MHEGEngineProviderIdString = "MHGGNU001";
    }

    class MHAsynchEvent 
    {
        private MHRoot eventSource;
        private int eventType;
        private MHUnion eventData;

        public MHRoot EventSource
        {
            get { return eventSource; }
            set { eventSource = value; }
        }

        public int EventType
        {
            get { return eventType; }
            set { eventType = value; }
        }

        public MHUnion EventData
        {
            get { return eventData; }
            set { eventData = value; }
        }
    }

    class MHPSEntry 
    {
        public MHPSEntry() 
        {
            m_FileName = new MHOctetString();
        }

        private MHOctetString m_FileName;
        private MHSequence <MHUnion> m_Data;

        public MHOctetString FileName
        {
            get { return m_FileName; }
            set { m_FileName = value; }
        }

        public MHSequence<MHUnion> Data
        {
            get { return m_Data; }
            set { m_Data = value; }
        }
    }

    class MHExternContent 
    {
        private string m_FileName;
        private MHIngredient m_Requester; 

        public string FileName
        {
            get 
            { 
                return m_FileName; 
            }
            set
            {
                m_FileName = value; 
            }

        }

        public MHIngredient Requester
        {
            get
            {
                return m_Requester;
            }
            set
            {
                m_Requester = value;
            }
        }

    }
}
