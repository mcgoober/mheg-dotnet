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
using System.Drawing;
using System.IO;
using MHEG.Parser;

namespace MHEG.Ingredients.Presentable
{
    class MHListGroup : MHTokenGroup
    {
        protected MHSequence<Point> m_Positions;
        protected bool m_fWrapAround, m_fMultipleSelection;
        //Internal attributes
        protected List<MHListItem> m_ItemList; // Items found by looking up the object refs
        protected int m_nFirstItem; // First item displayed - N.B. MHEG indexes from 1.
        protected bool m_fFirstItemDisplayed, m_fLastItemDisplayed;
        protected int m_nLastCount, m_nLastFirstItem;

        public MHListGroup() 
        {
            m_Positions = new MHSequence<Point>();
            m_ItemList = new List<MHListItem>();
            m_fWrapAround = false;
            m_fMultipleSelection = false;
            m_nFirstItem = 1;            
            m_nLastFirstItem = m_nFirstItem;
            m_nLastCount = 0; 
        }

        public override string ClassName() 
        {
            return "ListGroup"; 
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            MHParseNode pPositions = p.GetNamedArg(ASN1Codes.C_POSITIONS);
            for (int i = 0; i < pPositions.GetArgCount(); i++) 
            {
                MHParseNode pPos = pPositions.GetArgN(i);
                Point pos = new Point(pPos.GetSeqN(0).GetIntValue(), pPos.GetSeqN(1).GetIntValue());
                m_Positions.Append(pos);
            }
            MHParseNode pWrap = p.GetNamedArg(ASN1Codes.C_WRAP_AROUND);
            if (pWrap != null) m_fWrapAround = pWrap.GetArgN(0).GetBoolValue();
            MHParseNode pMultiple = p.GetNamedArg(ASN1Codes.C_MULTIPLE_SELECTION);
            if (pMultiple != null) m_fMultipleSelection = pMultiple.GetArgN(0).GetBoolValue();
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs); writer.Write("{:ListGroup ");
            base.PrintContents(writer, nTabs);
            Logging.PrintTabs(writer, nTabs + 1); writer.Write(":Positions (");
            for (int i = 0; i < m_Positions.Size; i++) {
                writer.Write(" ( {0} {1} )", m_Positions.GetAt(i).X, m_Positions.GetAt(i).Y);
            }
            writer.Write(")\n");
            if (m_fWrapAround) { Logging.PrintTabs(writer, nTabs + 1); writer.Write(":WrapAround true\n"); }
            if (m_fMultipleSelection) { Logging.PrintTabs(writer, nTabs + 1); writer.Write(":MultipleSelection true\n"); }
            Logging.PrintTabs(writer, nTabs); writer.Write("}\n");
        }

        public override void Preparation(MHEngine engine)
        {
            base.Preparation(engine);
            for (int i = 0; i < m_TokenGrpItems.Size; i++) 
            {
                // Find the item and add it to the list if it isn't already there.
                MHRoot pItem = engine.FindObject(m_TokenGrpItems.GetAt(i).Object);
                bool bFound = false;
                foreach(MHListItem p in m_ItemList)
                {
                    if (p.Visible == pItem)
                    {
                        bFound = true;
                    }
                }
                if (!bFound)
                {
                    m_ItemList.Add(new MHListItem(pItem));
                }
            }
        }

        public override void Destruction(MHEngine engine)
        {
             // Reset the positions of the visibles.
            foreach(MHListItem p in m_ItemList)
            {
                p.Visible.ResetPosition();
            }
            base.Destruction(engine); 
        }

        public override void Activation(MHEngine engine)
        {
            m_fFirstItemDisplayed = m_fLastItemDisplayed = false;
            base.Activation(engine);
            Update(engine);
        }

        public override void Deactivation(MHEngine engine)
        {
            // Deactivate the visibles.
            foreach(MHListItem p in m_ItemList)
            {
                p.Visible.Deactivation(engine);
            }
            base.Deactivation(engine);
        }

        public void Update(MHEngine engine)
        {
            if (m_ItemList.Count == 0) 
            { // Special cases when the list becomes empty
                if (m_fFirstItemDisplayed) 
                {
                    m_fFirstItemDisplayed = false;
                    engine.EventTriggered(this, EventFirstItemPresented, new MHUnion(false));
                }
                if (m_fLastItemDisplayed) 
                {
                    m_fLastItemDisplayed = false;
                    engine.EventTriggered(this, EventLastItemPresented, new MHUnion(false));
                }
            }
            else 
            { // Usual case.
                int i = -1;
                foreach(MHListItem p in m_ItemList)
                {
                    i++;
                    MHRoot pVis = p.Visible;
                    int nCell = i + 1 - m_nFirstItem; // Which cell does this item map onto?
                    if (nCell >= 0 && nCell < m_Positions.Size) 
                    {
                        if (i == 0 && ! m_fFirstItemDisplayed) 
                        {
                            m_fFirstItemDisplayed = true;
                            engine.EventTriggered(this, EventFirstItemPresented, new MHUnion(true));
                        }
                        if (i == (int)m_ItemList.Count - 1 && ! m_fLastItemDisplayed) 
                        {
                            m_fLastItemDisplayed = true;
                            engine.EventTriggered(this, EventLastItemPresented, new MHUnion(true));
                        }
                        pVis.SetPosition(m_Positions.GetAt(i - m_nFirstItem+1).X, m_Positions.GetAt(i - m_nFirstItem + 1).Y, engine);
                        if (!pVis.RunningStatus) 
                        {
                            pVis.Activation(engine);
                        }
                    }
                    else 
                    {
                        if (i == 0 && m_fFirstItemDisplayed) 
                        {
                            m_fFirstItemDisplayed = false;
                            engine.EventTriggered(this, EventFirstItemPresented, new MHUnion(false));
                        }
                        if (i == (int)m_ItemList.Count - 1 && m_fLastItemDisplayed) 
                        {
                            m_fLastItemDisplayed = false;
                            engine.EventTriggered(this, EventLastItemPresented, new MHUnion(false));
                        }
                        if (pVis.RunningStatus) 
                        { 
                            pVis.Deactivation(engine); pVis.ResetPosition(); 
                        }
                    }
                }
            }
            // Generate the HeadItems and TailItems events.  Even in the MHEG corrigendum this is unclear.
            // I'm not at all sure this is right.
            if (m_nLastFirstItem != m_nFirstItem) 
            {
                engine.EventTriggered(this, EventHeadItems, new MHUnion(m_nFirstItem));
            }
            if (m_nLastCount - m_nLastFirstItem != (int)m_ItemList.Count - m_nFirstItem) 
            {
                engine.EventTriggered(this, EventTailItems, new MHUnion(m_ItemList.Count - m_nFirstItem));
            }
            m_nLastCount = m_ItemList.Count;
            m_nLastFirstItem = m_nFirstItem;
        }

        public override void AddItem(int nIndex, MHRoot pItem, MHEngine engine)
        { 
             // See if the item is already there and ignore this if it is.
            foreach (MHListItem p in m_ItemList)
            {
                if (p.Visible == pItem) return;
            }
            // Ignore this if the index is out of range
            if (nIndex < 1 || nIndex > (int)m_ItemList.Count + 1) return;
            // Insert it at the appropriate position (MHEG indexes count from 1).
            m_ItemList.Insert(nIndex - 1, new MHListItem(pItem));
            if (nIndex <= m_nFirstItem && m_nFirstItem < (int)m_ItemList.Count) m_nFirstItem++;
            Update(engine); // Apply the update behaviour
        }

        public override void DelItem(MHRoot pItem, MHEngine engine) 
        {
            // See if the item is already there and ignore this if it is.
            for (int i = 0; i < (int)m_ItemList.Count; i++)
            {
                if (m_ItemList[i].Visible == pItem)
                { // Found it - remove it from the list and reset the posn.
                    m_ItemList.RemoveAt(i);
                    pItem.ResetPosition();
                    if (i + 1 < m_nFirstItem && m_nFirstItem > 1)
                    {
                        m_nFirstItem--;
                    }
                    return;
                }
            }
        }
        public void Select(int nIndex, MHEngine engine)
        {
            MHListItem pListItem = m_ItemList[nIndex - 1];
            if (pListItem == null || pListItem.Selected) return; // Ignore if already selected.
            if (!m_fMultipleSelection)
            {
                // Deselect any existing selections.
                for (int i = 0; i < (int)m_ItemList.Count; i++)
                {
                    if (m_ItemList[i].Selected) Deselect(i + 1, engine);
                }
            }
            pListItem.Selected = true;
            engine.EventTriggered(this, EventItemSelected, new MHUnion(nIndex));
        }

        public void Deselect(int nIndex, MHEngine engine)
        {
            MHListItem pListItem = m_ItemList[nIndex - 1];
            if (pListItem == null || !pListItem.Selected) return; // Ignore if not selected.
            pListItem.Selected = false;
            engine.EventTriggered(this, EventItemDeselected, new MHUnion(nIndex));
        }

        public override void GetCellItem(int nCell, MHObjectRef itemDest, MHEngine engine) 
        { 
            if (nCell < 1) nCell = 1; // First cell
            if (nCell > m_Positions.Size) nCell = m_Positions.Size; // Last cell.
            int nVisIndex = nCell + m_nFirstItem - 2;
            if (nVisIndex >= 0 && nVisIndex < (int)m_ItemList.Count) {
                MHRoot pVis = m_ItemList[nVisIndex].Visible;
                engine.FindObject(itemDest).SetVariableValue(new MHUnion(pVis.ObjectIdentifier));
            }
            else engine.FindObject(itemDest).SetVariableValue(new MHUnion(MHObjectRef.Null));
        }

        public int AdjustIndex(int nIndex) // Added in the MHEG corrigendum
        {
            int nItems = m_ItemList.Count;
            if (nItems == 0) return 1;
            if (nIndex > nItems) return ((nIndex-1) % nItems) + 1;
            else if (nIndex < 0) return nItems - ((-nIndex) % nItems);
            else return nIndex;
        }

        public override void GetListItem(int nCell, MHObjectRef itemDest, MHEngine engine) 
        {
            if (m_fWrapAround) nCell = AdjustIndex(nCell);
            if (nCell < 1 || nCell > m_ItemList.Count) return; // Ignore it if it's out of range and not wrapping
            engine.FindObject(itemDest).SetVariableValue(new MHUnion(m_ItemList[nCell - 1].Visible.ObjectIdentifier));
        }

        public override void GetItemStatus(int nCell, MHObjectRef itemDest, MHEngine engine) 
        {
            if (m_fWrapAround) nCell = AdjustIndex(nCell);
            if (nCell < 1 || nCell > (int)m_ItemList.Count) return;
            engine.FindObject(itemDest).SetVariableValue(new MHUnion(m_ItemList[nCell - 1].Selected));       
        }

        public override void SelectItem(int nCell, MHEngine engine) 
        {
            if (m_fWrapAround) nCell = AdjustIndex(nCell);
            if (nCell < 1 || nCell > m_ItemList.Count) return;
            Select(nCell, engine);
        }

        public override void DeselectItem(int nCell, MHEngine engine) 
        {
            if (m_fWrapAround) nCell = AdjustIndex(nCell);
            if (nCell < 1 || nCell > m_ItemList.Count) return;
            Deselect(nCell, engine);
        }

        public override void ToggleItem(int nCell, MHEngine engine)
        {
            if (m_fWrapAround) nCell = AdjustIndex(nCell);
            if (nCell < 1 || nCell > m_ItemList.Count) return;
            if (m_ItemList[nCell - 1].Selected) Deselect(nCell, engine); else Select(nCell, engine);
        }

        public override void ScrollItems(int nCell, MHEngine engine)
        {
            nCell += m_nFirstItem;
            if (m_fWrapAround) nCell = AdjustIndex(nCell);
            if (nCell < 1 || nCell > (int)m_ItemList.Count) return;
            m_nFirstItem = nCell;
            Update(engine);
        }

        public override void SetFirstItem(int nCell, MHEngine engine)
        {
            if (m_fWrapAround) nCell = AdjustIndex(nCell);
            if (nCell < 1 || nCell > m_ItemList.Count) return;
            m_nFirstItem = nCell;
            Update(engine);
        }

        public override void GetFirstItem(MHRoot pResult, MHEngine engine) 
        {
            pResult.SetVariableValue(new MHUnion(m_nFirstItem));
        }

        public override void GetListSize(MHRoot pResult, MHEngine engine) 
        {
            pResult.SetVariableValue(new MHUnion(m_ItemList.Count));
        }
    }
}
