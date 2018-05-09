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

namespace MHEG.Ingredients.Presentable
{
    class MHVideo : MHVisible
    {
        protected int m_nComponentTag;
        protected int m_Termination;
        // Added in UK MHEG
        protected int m_nXDecodeOffset;
        protected int m_nYDecodeOffset;
        protected int m_nDecodeWidth;
        protected int m_nDecodeHeight;
        protected bool m_fStreamPlaying;
        protected MHContentRef m_streamContentRef;

        public MHVideo()
        {
            m_Termination = VI_Disappear;
            m_nXDecodeOffset = 0;
            m_nYDecodeOffset = 0;
            m_nDecodeWidth = 0;
            m_nDecodeHeight = 0;
            m_streamContentRef = new MHContentRef();
        }

        public override string ClassName() 
        {
            return "Video"; 
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            m_nComponentTag = p.GetNamedArg(ASN1Codes.C_COMPONENT_TAG).GetArgN(0).GetIntValue();
            MHParseNode pTerm = p.GetNamedArg(ASN1Codes.C_TERMINATION);
            if (pTerm != null) m_Termination = pTerm.GetEnumValue();
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs); writer.Write("{:Video ");
            base.Print(writer, nTabs + 1);
            Logging.PrintTabs(writer, nTabs + 1); writer.Write(":ComponentTag {0}\n", m_nComponentTag);
            if (m_Termination != VI_Disappear) 
            {
                Logging.PrintTabs(writer, nTabs + 1); writer.Write("Termination freeze "); 
            }
            Logging.PrintTabs(writer, nTabs); writer.Write("}\n");
        }

        public override void Preparation(MHEngine engine)
        {
            if (m_fAvailable) return; // Already prepared
            base.Preparation(engine); // Prepare the base class.
            // Set up the internal attributes after MHVisible.Preparation.
            m_nDecodeWidth = m_nBoxWidth;
            m_nDecodeHeight = m_nBoxHeight;
        }

        public override void ContentPreparation(MHEngine engine)
        {
            // Pretend it's available.
            engine.EventTriggered(this, EventContentAvailable);
        }

        public override void Activation(MHEngine engine)
        {
            if (m_fRunning) return;
            base.Activation(engine);
            if (m_fStreamPlaying && m_streamContentRef.IsSet())
            {
                string stream = "";
                MHOctetString str = m_streamContentRef.ContentRef;
                if (str.Size != 0) stream = str.ToString();
                engine.GetContext().BeginVideo(stream, m_nComponentTag);
            }
        }

        public override void Deactivation(MHEngine engine)
        {
            if (! m_fRunning) return;
            base.Deactivation(engine);
            if (m_fStreamPlaying) engine.GetContext().StopVideo();
        }

        public override void Display(MHEngine engine)
        {
            if (!m_fRunning) return;
            if (m_nBoxWidth == 0 || m_nBoxHeight == 0) return; // Can't draw zero sized boxes.
            // The bounding box is assumed always to be True.
            // The full screen video is displayed in this rectangle.  It is therefore scaled to
            // m_nDecodeWidth/720 by m_nDecodeHeight/576.
            Rectangle videoRect = new Rectangle(m_nPosX + m_nXDecodeOffset, m_nPosY + m_nYDecodeOffset,
                                        m_nDecodeWidth, m_nDecodeHeight);
            Rectangle displayRect = videoRect;
            displayRect.Intersect(new Rectangle(m_nPosX, m_nPosY, m_nBoxWidth, m_nBoxHeight));
            engine.GetContext().DrawVideo(videoRect, displayRect);
        }

        public override Region GetVisibleArea()
        {
            if (!m_fRunning)
            {
                Region r = new Region();
                r.MakeEmpty();
                return r;
            }
            // The visible area is the intersection of the containing box with the, possibly offset,
            // video.
            Region boxRegion = new Region(new Rectangle(m_nPosX, m_nPosY, m_nBoxWidth, m_nBoxHeight));
            Region videoRegion = new Region(new Rectangle(m_nPosX + m_nXDecodeOffset, m_nPosY + m_nYDecodeOffset,
                                        m_nDecodeWidth, m_nDecodeHeight));
            boxRegion.Intersect(videoRegion);
            return boxRegion;
        }

        public override Region GetOpaqueArea()
        {
            return GetVisibleArea();
        }

        public override void ScaleVideo(int xScale, int yScale, MHEngine engine) 
        {
            if (xScale == m_nDecodeWidth && yScale == m_nDecodeHeight) return;
            Region updateArea = GetVisibleArea(); // Redraw the area before the offset
            m_nDecodeWidth = xScale;
            m_nDecodeHeight = yScale;
            updateArea.Union(GetVisibleArea()); // Redraw this bitmap.
            engine.Redraw(updateArea); // Mark for redrawing 
        }

        public override void SetVideoDecodeOffset(int newXOffset, int newYOffset, MHEngine engine)
        {
            Region updateArea = GetVisibleArea(); // Redraw the area before the offset
            m_nXDecodeOffset = newXOffset;
            m_nYDecodeOffset = newYOffset;
            updateArea.Union(GetVisibleArea()); // Redraw the resulting area.
            engine.Redraw(updateArea); // Mark for redrawing            
        }

        public override void GetVideoDecodeOffset(MHRoot pXOffset, MHRoot pYOffset, MHEngine engine) 
        {
            pXOffset.SetVariableValue(new MHUnion(m_nXDecodeOffset));
            pYOffset.SetVariableValue(new MHUnion(m_nYDecodeOffset));            
        }
        
        public override void SetStreamRef(MHContentRef contentRef)
        {
            m_streamContentRef.Copy(contentRef);
        }

        public override void BeginPlaying(MHEngine engine)
        {
            m_fStreamPlaying = true;
            if (m_fRunning && m_streamContentRef.IsSet())
            {
                string stream = "";
                MHOctetString str = m_streamContentRef.ContentRef;
                if (str.Size != 0) stream = str.ToString();
                engine.GetContext().BeginVideo(stream, m_nComponentTag);
            }
        }

        public override void StopPlaying(MHEngine engine)
        {
            m_fStreamPlaying = false;
            if (m_fRunning) engine.GetContext().StopVideo();
        }

        public const int VI_Freeze = 1;
        public const int VI_Disappear = 2;
    }
}
