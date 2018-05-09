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
using System.Drawing;

namespace MHEG.Ingredients.Presentable
{
    abstract class MHVisible : MHPresentable
    {
        // Exchange attributes
        protected int m_nOriginalBoxWidth, m_nOriginalBoxHeight;
        protected int m_nOriginalPosX, m_nOriginalPosY;
        protected MHObjectRef m_OriginalPaletteRef; // Optional palette ref
        // Internal attributes
        protected int m_nBoxWidth, m_nBoxHeight;
        protected int m_nPosX, m_nPosY;
        protected MHObjectRef m_PaletteRef;

        public MHVisible()
        {
            m_OriginalPaletteRef = new MHObjectRef();
            m_PaletteRef = new MHObjectRef();
            m_nOriginalBoxWidth = m_nOriginalBoxHeight = -1; // Should always be specified.
            m_nOriginalPosX = m_nOriginalPosY = 0; // Default values.
        }

        public MHVisible(MHVisible reference) : base(reference)
        {
            m_OriginalPaletteRef = new MHObjectRef();
            m_PaletteRef = new MHObjectRef();
            m_nOriginalBoxWidth = reference.m_nOriginalBoxWidth;
            m_nOriginalBoxHeight = reference.m_nOriginalBoxHeight;
            m_nOriginalPosX = reference.m_nOriginalPosX;
            m_nOriginalPosY = reference.m_nOriginalPosY;
            m_OriginalPaletteRef.Copy(reference.m_OriginalPaletteRef);
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            // Original box size - two integer arguments.
            MHParseNode pOriginalBox = p.GetNamedArg(ASN1Codes.C_ORIGINAL_BOX_SIZE);
            if (pOriginalBox == null) p.Failure("OriginalBoxSize missing");
            m_nOriginalBoxWidth = pOriginalBox.GetArgN(0).GetIntValue();
            m_nOriginalBoxHeight = pOriginalBox.GetArgN(1).GetIntValue();

            // Original position - two integer arguments.  Optional
            MHParseNode pOriginalPos = p.GetNamedArg(ASN1Codes.C_ORIGINAL_POSITION);
            if (pOriginalPos != null) 
            {
                m_nOriginalPosX = pOriginalPos.GetArgN(0).GetIntValue();
                m_nOriginalPosY = pOriginalPos.GetArgN(1).GetIntValue();
            }

            // OriginalPalette ref - optional. 
            MHParseNode pOriginalPaletteRef = p.GetNamedArg(ASN1Codes.C_ORIGINAL_PALETTE_REF);
            if (pOriginalPaletteRef != null) m_OriginalPaletteRef.Initialise(pOriginalPaletteRef.GetArgN(0), engine);
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            base.Print(writer, nTabs);
            Logging.PrintTabs(writer, nTabs); writer.Write(":OrigBoxSize {0} {1}\n", m_nOriginalBoxWidth, m_nOriginalBoxHeight);
            if (m_nOriginalPosX != 0 || m_nOriginalPosY != 0) {
                Logging.PrintTabs(writer, nTabs); writer.Write(":OrigPosition {0} {1}\n", m_nOriginalPosX, m_nOriginalPosY);
            }
            if (m_OriginalPaletteRef.IsSet()) {
                Logging.PrintTabs(writer, nTabs); writer.Write(":OrigPaletteRef"); m_OriginalPaletteRef.Print(writer, nTabs + 1); writer.Write("\n");
            }     
        }

        public override void Preparation(MHEngine engine)
        {
            if (AvailabilityStatus) return; // Already prepared
            m_nBoxWidth = m_nOriginalBoxWidth;
            m_nBoxHeight = m_nOriginalBoxHeight;
            m_nPosX = m_nOriginalPosX;
            m_nPosY = m_nOriginalPosY;
            m_PaletteRef.Copy(m_OriginalPaletteRef);
            // Add a reference to this to the display stack.
            engine.AddToDisplayStack(this);
            base.Preparation(engine);
        }

        public override void Destruction(MHEngine engine)
        {
            engine.RemoveFromDisplayStack(this);
            base.Destruction(engine);
        }

        public override void Activation(MHEngine engine)
        {
            if (RunningStatus) return;
            base.Activation(engine);
            m_fRunning = true;
            engine.Redraw(GetVisibleArea()); // Display the visible.
            engine.EventTriggered(this, EventIsRunning);

        }

        public override void Deactivation(MHEngine engine)
        {
            if (!RunningStatus) return;
            // Stop displaying.  We need to save the area before we turn off m_fRunning but
            // only call Redraw once this is no longer visible so that the area beneath will be drawn.
            Region region = GetVisibleArea();
            base.Deactivation(engine);
            engine.Redraw(region); // Draw underlying object.

        }

        // Actions.
        public override void SetPosition(int nXPosition, int nYPosition, MHEngine engine)
        {
            // When we move a visible we have to redraw both the old area and the new.
            // In some cases, such as moving an opaque rectangle we might be able to reduce
            // this if there is some overlap.
            Region drawRegion = GetVisibleArea();
            m_nPosX = nXPosition;
            m_nPosY = nYPosition;
            drawRegion.Union(GetVisibleArea());
            engine.Redraw(drawRegion);
        }

        public override void GetPosition(MHRoot pXPosN, MHRoot pYPosN)
        {
            pXPosN.SetVariableValue(new MHUnion(m_nPosX));
            pYPosN.SetVariableValue(new MHUnion(m_nPosY));
        }

        public override void SetBoxSize(int nWidth, int nHeight, MHEngine engine)
        {
            Region drawRegion = GetVisibleArea();
            m_nBoxWidth = nWidth;
            m_nBoxHeight = nHeight;
            drawRegion.Union(GetVisibleArea());
            engine.Redraw(drawRegion);
        }

        public override void GetBoxSize(MHRoot pWidthDest, MHRoot pHeightDest)
        {
            pWidthDest.SetVariableValue(new MHUnion(m_nBoxWidth));
            pHeightDest.SetVariableValue(new MHUnion(m_nBoxHeight));
        }

        public override void SetPaletteRef(MHObjectRef newPalette, MHEngine engine)
        {
            m_PaletteRef.Copy(newPalette);
            engine.Redraw(GetVisibleArea());
        }

        public override void BringToFront(MHEngine engine)
        {
            engine.BringToFront(this);
        }

        public override void SendToBack(MHEngine engine)
        {
            engine.SendToBack(this);
        }

        public override void PutBefore(MHRoot pRef, MHEngine engine)
        {
            engine.PutBefore(this, pRef);
        }

        public override void PutBehind(MHRoot pRef, MHEngine engine)
        {
            engine.PutBehind(this, pRef);
        }

        // Display function.
        public abstract void Display(MHEngine engine);

        // Get the visible region of this visible.  This is the area that needs to be drawn.
        public virtual Region GetVisibleArea()
        {
            if (!RunningStatus)
            {
                Region r = new Region(); // Not visible at all.
                r.MakeEmpty();
                return r;
            }
            else return new Region(new Rectangle(m_nPosX, m_nPosY, m_nBoxWidth, m_nBoxHeight));
        }

        // Get the opaque area.  This is the area that this visible completely obscures and
        // is empty if the visible is drawn in a transparent or semi-transparent colour.
        public virtual Region GetOpaqueArea() 
        {
            Region r = new Region(); // Not visible at all.
            r.MakeEmpty();
            return r;
        }

        // Reset the position - used by ListGroup.
        public override void ResetPosition() 
        { 
            m_nPosX = m_nOriginalPosX; 
            m_nPosY = m_nOriginalPosY; 
        }

        // Return the colour, looking up in the palette if necessary.
        protected MHRgba GetColour(MHColour colour)
        {
            Logging.Assert(colour.ColIndex < 0); // We don't support palettes.
            int red = 0, green = 0, blue = 0, alpha = 0;
            int cSize = colour.ColStr.Size;
            if (cSize != 4) Logging.Log(Logging.MHLogWarning, "Colour string has length " + cSize + " not 4.");
            // Just in case the length is short we handle those properly.
            if (cSize > 0) red = colour.ColStr.GetAt(0);
            if (cSize > 1) green = colour.ColStr.GetAt(1);
            if (cSize > 2) blue = colour.ColStr.GetAt(2);
            if (cSize > 3) alpha = 255 - colour.ColStr.GetAt(3); // Convert transparency to alpha
            return new MHRgba(red, green, blue, alpha);
        }

        protected Region createEmptyRegion()
        {
            Region r = new Region();
            r.MakeEmpty();
            return r;
        }
    }
}
