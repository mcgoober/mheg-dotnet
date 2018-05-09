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
    class MHBitmap : MHVisible
    {
        protected bool m_fTiling;
        protected int m_nOrigTransparency;

        // Internal attributes
        protected int m_nTransparency;
        // Added in UK MHEG
        protected int m_nXDecodeOffset, m_nYDecodeOffset;

        protected IMHBitmapDisplay m_pContent; // current image if any.

        public MHBitmap()
        {
            m_fTiling = false;
            m_nOrigTransparency = 0;
            m_nXDecodeOffset = 0;
            m_nYDecodeOffset = 0;
            m_pContent = null;           
        }

        public MHBitmap(MHBitmap reference)
        {
            m_fTiling = reference.m_fTiling;
            m_nOrigTransparency = reference.m_nOrigTransparency;
            m_nXDecodeOffset = 0;
            m_nYDecodeOffset = 0;
            m_pContent = null;
        }

        public override string ClassName()
        {
            return "Bitmap";
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            // Tiling - optional
            MHParseNode pTiling = p.GetNamedArg(ASN1Codes.C_TILING);
            if (pTiling != null) m_fTiling = pTiling.GetArgN(0).GetBoolValue();
            // Transparency - optional
            MHParseNode pTransparency = p.GetNamedArg(ASN1Codes.C_ORIGINAL_TRANSPARENCY);
            if (pTransparency != null) m_nOrigTransparency = pTransparency.GetArgN(0).GetIntValue();
            m_pContent = engine.GetContext().CreateBitmap(m_fTiling);
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs); writer.Write("{:Bitmap ");
            base.Print(writer, nTabs+1);
            if (m_fTiling) { Logging.PrintTabs(writer, nTabs + 1); writer.Write(":Tiling true\n"); }
            if (m_nOrigTransparency != 0) { Logging.PrintTabs(writer, nTabs + 1); writer.Write(":OrigTransparency {0}\n", m_nOrigTransparency); }
            Logging.PrintTabs(writer, nTabs); writer.Write("}\n");
        }

        public override void Preparation(MHEngine engine)
        {
            if (AvailabilityStatus) return;
            m_nTransparency = m_nOrigTransparency; // Transparency isn't used in UK MHEG
            base.Preparation(engine);
        }

        public override void ContentPreparation(MHEngine engine)
        {
            base.ContentPreparation(engine);
            Logging.Assert(m_ContentType != IN_NoContent);
            Logging.Assert(m_ContentType != IN_IncludedContent); // We can't handle included content at the moment.
        //  if (m_ContentType == IN_IncludedContent) CreateContent(m_IncludedContent.Bytes(), m_IncludedContent.Size());
        }

        public override void ContentArrived(byte[] data, MHEngine engine)
        {
            Region updateArea = GetVisibleArea(); // If there's any content already we have to redraw it.
            if (m_pContent == null) return; // Shouldn't happen.

            int nCHook = m_nContentHook;
            if (nCHook == 0) nCHook = engine.GetDefaultBitmapCHook();

            // TODO: What if we can't convert it?
            if (nCHook == 4)
            { // PNG.
                m_pContent.CreateFromPNG(data);
            }
            else if (nCHook == 2)
            { // MPEG I-frame.
                m_pContent.CreateFromMPEG(data);
            }
            else throw new MHEGException("Unknown bitmap content hook " + nCHook);

            updateArea.Union(GetVisibleArea()); // Redraw this bitmap.
            engine.Redraw(updateArea); // Mark for redrawing

            // Now signal that the content is available.
            engine.EventTriggered(this, EventContentAvailable);
        }

        public override void SetTransparency(int nTransPerCent, MHEngine engine)
        {
            // The object transparency isn't actually used in UK MHEG.
            // We want a value between 0 and 255
            Logging.Assert(nTransPerCent >= 0 && nTransPerCent <= 100); // This should really be a check.
            m_nTransparency = ((nTransPerCent * 255) + 50) / 100;
        }

        public override void ScaleBitmap(int xScale, int yScale, MHEngine engine)
        {
            Region updateArea = GetVisibleArea(); // If there's any content already we have to redraw it.
            m_pContent.ScaleImage(xScale, yScale);
            updateArea.Union(GetVisibleArea()); // Redraw this bitmap.
            engine.Redraw(updateArea); // Mark for redrawing
        }

        public override void SetBitmapDecodeOffset(int newXOffset, int newYOffset, MHEngine engine)
        {
            Region updateArea = GetVisibleArea(); // Redraw the area before the offset
            m_nXDecodeOffset = newXOffset;
            m_nYDecodeOffset = newYOffset;
            updateArea.Union(GetVisibleArea()); // Redraw this bitmap.
            engine.Redraw(updateArea); // Mark for redrawing

        }

        public override void GetBitmapDecodeOffset(MHRoot pXOffset, MHRoot pYOffset)
        {
            pXOffset.SetVariableValue(new MHUnion(m_nXDecodeOffset));
            pYOffset.SetVariableValue(new MHUnion(m_nYDecodeOffset));
        }

        public override MHIngredient Clone(MHEngine engine)
        {
            return new MHBitmap(this);
        }

        public override void Display(MHEngine engine)
        {
            if (!m_fRunning || m_pContent == null || m_nBoxWidth == 0 || m_nBoxHeight == 0) return; // Can't draw zero sized boxes.

            m_pContent.Draw(m_nPosX + m_nXDecodeOffset, m_nPosY + m_nYDecodeOffset,
                new Rectangle(m_nPosX, m_nPosY, m_nBoxWidth, m_nBoxHeight), m_fTiling);
        }

        public override Region GetVisibleArea()
        {
            if (!m_fRunning || m_pContent == null)
            {
                Region r = new Region();
                r.MakeEmpty();
                return r;
            }
            // The visible area is the intersection of the containing box with the, possibly offset,
            // bitmap.
            Size imageSize = m_pContent.GetSize();
            Region boxRegion = new Region(new Rectangle(m_nPosX, m_nPosY, m_nBoxWidth, m_nBoxHeight));
            Region bitmapRegion = new Region(new Rectangle(m_nPosX + m_nXDecodeOffset, m_nPosY + m_nYDecodeOffset,
                                        imageSize.Width, imageSize.Height));
            boxRegion.Intersect(bitmapRegion);
            return boxRegion;
        }

        public override Region GetOpaqueArea()
        {
            // The area is empty unless the bitmap is opaque.
            if (!m_fRunning || m_pContent == null || !m_pContent.IsOpaque())
            {
                Region r = new Region();
                r.MakeEmpty();
                return r;
            }
            else return GetVisibleArea();
        }
    }
}
