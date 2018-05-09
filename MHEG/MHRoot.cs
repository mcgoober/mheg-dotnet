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
using System.Drawing;
using MHEG.Parser;
using MHEG.Ingredients;

namespace MHEG
{
    abstract class MHRoot
    {
        protected bool m_fAvailable; // Set once Preparation has completed.
        protected bool m_fRunning; // Set once Activation has completed.
        protected MHObjectRef m_ObjectIdentifier; // Identifier of this object.

        public MHRoot()
        {
            m_fAvailable = false;
            m_fRunning = false;
            m_ObjectIdentifier = new MHObjectRef();
        }

        public MHObjectRef ObjectIdentifier
        {
            get { return m_ObjectIdentifier; }
        }

        public virtual void Initialise(MHParseNode p, MHEngine engine)
        {
            // The first argument should be present.
            MHParseNode arg = p.GetArgN(0); 

            // Extract the field. 
            m_ObjectIdentifier.Initialise(arg, engine);
        }

        public virtual void Print(TextWriter writer, int nTabs)
        {
            m_ObjectIdentifier.Print(writer, nTabs);
            writer.Write("\n");
        }

        public virtual bool IsShared() 
        { 
            return false; 
        }
 
        // Attributes on Root class
        public virtual bool AvailabilityStatus
        {
            get { return m_fAvailable; }
        }

        public virtual bool RunningStatus
        {
            get { return m_fRunning; }
        }

        // MHEG Behaviours
        // Preparation - sets up the run-time representation.  Sets m_fAvailable and generates IsAvailable event.
        public virtual void Preparation(MHEngine engine)
        {
            if (m_fAvailable) return; // Already prepared
            
            // Retrieve object           

            // Init internal attributes of the object.

            // Set AvailabilityStatus
            m_fAvailable = true;

            // Generate an IsAvailable event
            engine.EventTriggered(this, EventIsAvailable);

            // When the content becomes available generate EventContentAvailable.  This is not
            // generated if an object has no Content.
            ContentPreparation(engine);
        }

        // Activation - starts running the object.  Sets m_fRunning and generates IsRunning event.
        public virtual void Activation(MHEngine engine)
        {
            if (m_fRunning) return; // Already running.
            if (!m_fAvailable) Preparation(engine); // Prepare it if that hasn't already been done.
            // The subclasses are responsible for setting m_fRunning and generating IsRunning.
        }

        // Deactivation - stops running the object.  Clears m_fRunning
        public virtual void Deactivation(MHEngine engine)
        {
            if (!m_fRunning) return; // Already stopped.
            m_fRunning = false;
            engine.EventTriggered(this, EventIsStopped);
        }

        // Destruction - deletes the run-time representation.  Clears m_fAvailable.
        public virtual void Destruction(MHEngine engine)
        {
            if (!m_fAvailable) return; // Already destroyed or never prepared.
            if (m_fRunning) Deactivation(engine); // Deactivate it if it's still running.
            // We're supposed to wait until it's stopped here.
            m_fAvailable = false;
            engine.EventTriggered(this, EventIsDeleted);
        }

        // ContentPreparation - This behaviour is added in COR1.
        public virtual void ContentPreparation(MHEngine engine)
        {
            // This behaviour performs no action in the Root class
        }

        // Return an object with a given object number.  In the root class this returns this object
        // if it matches.  Group and Stream classes also search their components.
        public virtual MHRoot FindByObjectNo(int n)
        {
            if (n == m_ObjectIdentifier.ObjectNo) return this;
            else return null;
        }

        // Actions.  The default behaviour if a sub-class doesn't override them is to fail.


        // Actions on Groups
        public virtual void SetTimer(int nTimerId, bool fAbsolute, int nMilliSecs, MHEngine engine) 
        { 
            InvalidAction("SetTimer"); 
        }
        
        // This isn't an MHEG action as such but is used as part of the implementation of "Clone"
        public virtual void MakeClone(MHRoot target, MHRoot reference, MHEngine engine) 
        { 
            InvalidAction("MakeClone"); 
        }

        public virtual void SetInputRegister(int nReg, MHEngine engine) 
        { 
            InvalidAction("SetInputRegister"); 
        }

        // Actions on Ingredients.
        public virtual void SetData(MHOctetString included, MHEngine engine)
        { 
            InvalidAction("SetData"); 
        }

        public virtual void SetData(MHContentRef referenced, bool fSizeGiven, int size, bool fCCGiven, int cc, MHEngine engine)
        { 
            InvalidAction("SetData"); 
        }

        public virtual void Preload(MHEngine engine) 
        { 
            InvalidAction("Preload"); 
        }

        public virtual void Unload(MHEngine engine) 
        { 
            InvalidAction("Unload"); 
        }
        
        // The UK MHEG profile only requires cloning for Text, Bitmap and Rectangle.
        public virtual MHIngredient Clone(MHEngine engine) 
        { 
            InvalidAction("Clone"); 
            return null; 
        }

        // Actions on Presentables.  The Run/Stop actions on presentables and the Activate/Deactivate actions
        // on Links have identical effects.  They could be merged.
        public virtual void Run(MHEngine engine) 
        { 
            InvalidAction("Run"); 
        }

        public virtual void Stop(MHEngine engine) 
        { 
            InvalidAction("Stop"); 
        }

        // Actions on variables.
        public virtual void TestVariable(int nOp, MHUnion parm, MHEngine engine)
        { 
            InvalidAction("TestVariable"); 
        }

        public virtual void GetVariableValue(MHUnion value, MHEngine engine) 
        { 
            InvalidAction("GetVariableValue"); 
        }

        public virtual void SetVariableValue(MHUnion value)
        { 
            InvalidAction("SetVariableValue"); 
        }

        // Actions on Text objects
        public virtual void GetTextData(MHRoot pDestination, MHEngine engine) 
        { 
            InvalidAction("GetTextData"); 
        }

        public virtual void SetBackgroundColour(MHColour colour, MHEngine engine) 
        { 
            InvalidAction("SetBackgroundColour"); 
        }

        public virtual void SetTextColour(MHColour colour, MHEngine engine) 
        { 
            InvalidAction("SetTextColour"); 
        }

        public virtual void SetFontAttributes(MHOctetString fontAttrs, MHEngine engine) 
        { 
            InvalidAction("SetFontAttributes"); 
        }

        // Actions on Links
        // Activate/Deactivate
        public virtual void Activate(bool f, MHEngine engine) 
        { 
            InvalidAction("Activate/Deactivate"); 
        } 

        // Actions on Programs.
        public virtual void CallProgram(bool fIsFork, MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine) 
        { 
            InvalidAction("CallProgram"); 
        }

            // Actions on TokenGroups
        public virtual void CallActionSlot(int n, MHEngine engine) 
        { 
            InvalidAction("CallActionSlot"); 
        }

        public virtual void Move(int n, MHEngine engine) 
        { 
            InvalidAction("Move"); 
        }

        public virtual void MoveTo(int n, MHEngine engine) 
        { 
            InvalidAction("MoveTo"); 
        }

        public virtual void GetTokenPosition(MHRoot pResult, MHEngine engine)
        { 
            InvalidAction("GetTokenPosition"); 
        }

        // Actions on ListGroups
        public virtual void AddItem(int nIndex, MHRoot pItem, MHEngine engine) { InvalidAction("GetCellItem"); }
        public virtual void DelItem(MHRoot pItem, MHEngine engine) { InvalidAction("GetCellItem"); }
        public virtual void GetCellItem(int nCell, MHObjectRef itemDest, MHEngine engine) { InvalidAction("GetCellItem"); }
        public virtual void GetListItem(int nCell, MHObjectRef itemDest, MHEngine engine) { InvalidAction("GetCellItem"); }
        public virtual void GetItemStatus(int nCell, MHObjectRef itemDest, MHEngine engine) { InvalidAction("GetItemStatus"); }
        public virtual void SelectItem(int nCell, MHEngine engine) { InvalidAction("SelectItem"); }
        public virtual void DeselectItem(int nCell, MHEngine engine) { InvalidAction("DeselectItem"); }
        public virtual void ToggleItem(int nCell, MHEngine engine) { InvalidAction("ToggleItem"); }
        public virtual void ScrollItems(int nCell, MHEngine engine) { InvalidAction("ScrollItems"); }
        public virtual void SetFirstItem(int nCell, MHEngine engine) { InvalidAction("SetFirstItem"); }
        public virtual void GetFirstItem(MHRoot pResult, MHEngine engine) { InvalidAction("GetFirstItem"); }
        public virtual void GetListSize(MHRoot pResult, MHEngine engine) { InvalidAction("GetListSize"); }

        // Actions on Visibles.
        public virtual void SetPosition(int nXPosition, int nYPosition, MHEngine engine) { InvalidAction("SetPosition"); }
        public virtual void GetPosition(MHRoot pXPosN, MHRoot pYPosN) { InvalidAction("GetPosition"); }
        public virtual void SetBoxSize(int nWidth, int nHeight, MHEngine engine) { InvalidAction("SetBoxSize"); }
        public virtual void GetBoxSize(MHRoot pWidthDest, MHRoot HeightDest) { InvalidAction("GetBoxSize"); }
        public virtual void SetPaletteRef(MHObjectRef newPalette, MHEngine engine) { InvalidAction("SetPaletteRef"); }
        public virtual void BringToFront(MHEngine engine) { InvalidAction("BringToFront"); }
        public virtual void SendToBack(MHEngine engine) { InvalidAction("SendToBack"); }
        public virtual void PutBefore(MHRoot pRef, MHEngine engine) { InvalidAction("PutBefore"); }
        public virtual void PutBehind(MHRoot pRef, MHEngine engine) { InvalidAction("PutBehind"); }
        public virtual void ResetPosition() { InvalidAction("ResetPosition"); } // Used internally by ListGroup

        // Actions on LineArt
        public virtual void SetFillColour(MHColour colour, MHEngine engine) { InvalidAction("SetFillColour"); }
        public virtual void SetLineColour(MHColour colour, MHEngine engine) { InvalidAction("SetLineColour"); }
        public virtual void SetLineWidth(int nWidth, MHEngine engine) { InvalidAction("SetLineWidth"); }
        public virtual void SetLineStyle(int nStyle, MHEngine engine) { InvalidAction("SetLineStyle"); }

        // Actions on Bitmaps
        public virtual void SetTransparency(int nTransPerCent, MHEngine engine) { InvalidAction("SetTransparency"); }
        public virtual void ScaleBitmap(int xScale, int yScale, MHEngine engine) { InvalidAction("ScaleBitmap"); }
        public virtual void SetBitmapDecodeOffset(int newXOffset, int newYOffset, MHEngine engine) { InvalidAction("SetBitmapDecodeOffset"); }
        public virtual void GetBitmapDecodeOffset(MHRoot pXOffset, MHRoot pYOffset) { InvalidAction("GetBitmapDecodeOffset"); }

        // Actions on Dynamic Line Art
        public virtual void Clear() { InvalidAction(""); }
        public virtual void GetLineWidth(MHRoot pResult) { InvalidAction("GetLineWidth"); }
        public virtual void GetLineStyle(MHRoot pResult) { InvalidAction("GetLineStyle"); }
        public virtual void GetLineColour(MHRoot pResult) { InvalidAction("GetLineColour"); }
        public virtual void GetFillColour(MHRoot pResult) { InvalidAction("GetFillColour"); }
        public virtual void DrawArcSector(bool fIsSector, int x, int y, int width, int height, int start, int arc, MHEngine engine) { InvalidAction("DrawArc/Sector"); }
        public virtual void DrawLine(int x1, int y1, int x2, int y2, MHEngine engine) { InvalidAction("DrawLine"); }
        public virtual void DrawOval(int x1, int y1, int width, int height, MHEngine engine) { InvalidAction("DrawOval"); }
        public virtual void DrawRectangle(int x1, int y1, int x2, int y2, MHEngine engine) { InvalidAction("DrawRectangle"); }
        public virtual void DrawPoly(bool fIsPolygon, Point[] points, MHEngine engine ) { InvalidAction("DrawPoly(gon/line)"); }

        // Actions on Video streams.
        public virtual void ScaleVideo(int xScale, int yScale, MHEngine engine) { InvalidAction("ScaleVideo"); }
        public virtual void SetVideoDecodeOffset(int newXOffset, int newYOffset, MHEngine engine) { InvalidAction("SetVideoDecodeOffset"); }
        public virtual void GetVideoDecodeOffset(MHRoot pXOffset, MHRoot pYOffset, MHEngine engine) { InvalidAction("GetVideoDecodeOffset"); }

        protected void InvalidAction(string actionName)
        {
            Console.WriteLine("Action \"" + actionName + "\" is not understood by class \"" + ClassName() + "\"");
        }

        public abstract string ClassName(); // For debugging messages.

        public const int EventIsAvailable = 1;
        public const int EventContentAvailable = 2;
        public const int EventIsDeleted = 3; 
        public const int EventIsRunning = 4; 
        public const int EventIsStopped = 5; 
        public const int EventUserInput = 6; 
        public const int EventAnchorFired = 7; 
        public const int EventTimerFired = 8; 
        public const int EventAsyncStopped = 9; 
        public const int EventInteractionCompleted = 10; 
        public const int EventTokenMovedFrom = 11; 
        public const int EventTokenMovedTo = 12; 
        public const int EventStreamEvent = 13; 
        public const int EventStreamPlaying = 14; 
        public const int EventStreamStopped = 15; 
        public const int EventCounterTrigger = 16; 
        public const int EventHighlightOn = 17; 
        public const int EventHighlightOff = 18; 
        public const int EventCursorEnter = 19; 
        public const int EventCursorLeave = 20; 
        public const int EventIsSelected = 21; 
        public const int EventIsDeselected = 22; 
        public const int EventTestEvent = 23; 
        public const int EventFirstItemPresented = 24; 
        public const int EventLastItemPresented = 25; 
        public const int EventHeadItems = 26; 
        public const int EventTailItems = 27; 
        public const int EventItemSelected = 28; 
        public const int EventItemDeselected = 29; 
        public const int EventEntryFieldFull = 30; 
        public const int EventEngineEvent = 31; 
        
        // The next two events are added in UK MHEG.
        public const int EventFocusMoved = 32; 
        public const int EventSliderValueChanged = 33;
    }
}
