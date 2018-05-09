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
using System.IO;
using MHEG.Actions;
using MHEG.Parser;

namespace MHEG
{
    interface IMHSequence<T>
    {
        void Append(T b);
        T GetAt(int i);
        void InsertAt(T b, int n);
        void RemoveAt(int i);
        int Size { get; }
    }

    class MHSequence<T> : IMHSequence<T>
    {
        private List<T> m_Values = new List<T>();       

        public int Size
        {
            get { return m_Values.Count; }
        }

        public T GetAt(int i)
        {
            return m_Values[i];
        }

        public void InsertAt(T b, int n)
        {
            m_Values.Insert(n, b);
        }

        public void Append(T b)
        {
            m_Values.Add(b);
        }

        public void RemoveAt(int i)
        {
            m_Values.RemoveAt(i);
        }
    }

    class MHActionSequence : MHSequence<MHElemAction>
    {
        public MHActionSequence()
        {

        }

        public void Initialise(MHParseNode p, MHEngine engine)
        {
            // Depending on the caller we may have a tagged argument list or a sequence.
            for (int i = 0; i < p.GetArgCount(); i++) 
            {
                MHParseNode pElemAction = p.GetArgN(i);
                MHElemAction pAction = null;
                switch (pElemAction.GetTagNo()) 
                {
                case ASN1Codes.C_ACTIVATE: pAction = new MHActivate(":Activate", true); break;
                case ASN1Codes.C_ADD: pAction = new MHAdd(); break;
                case ASN1Codes.C_ADD_ITEM: pAction = new MHAddItem(); break;
                case ASN1Codes.C_APPEND: pAction = new MHAppend(); break;
                case ASN1Codes.C_BRING_TO_FRONT: pAction = new MHBringToFront(); break;
                case ASN1Codes.C_CALL: pAction = new MHCall(":Call", false); break;
                case ASN1Codes.C_CALL_ACTION_SLOT: pAction = new MHCallActionSlot(); break;
                case ASN1Codes.C_CLEAR: pAction = new MHClear(); break;
                case ASN1Codes.C_CLONE: pAction = new MHClone(); break;
                case ASN1Codes.C_CLOSE_CONNECTION: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // ??
                case ASN1Codes.C_DEACTIVATE: pAction = new MHActivate(":Deactivate", false); break;
                case ASN1Codes.C_DEL_ITEM: pAction = new MHDelItem(); break;
                case ASN1Codes.C_DESELECT: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // Button
                case ASN1Codes.C_DESELECT_ITEM: pAction = new MHDeselectItem(); break;
                case ASN1Codes.C_DIVIDE: pAction = new MHDivide(); break;
                case ASN1Codes.C_DRAW_ARC: pAction = new MHDrawArcSector(":DrawArc", false); break;
                case ASN1Codes.C_DRAW_LINE: pAction = new MHDrawLine(); break;
                case ASN1Codes.C_DRAW_OVAL: pAction = new MHDrawOval(); break;
                case ASN1Codes.C_DRAW_POLYGON: pAction = new MHDrawPoly(":DrawPolygon", true); break;
                case ASN1Codes.C_DRAW_POLYLINE: pAction = new MHDrawPoly(":DrawPolyline", false); break;
                case ASN1Codes.C_DRAW_RECTANGLE: pAction = new MHDrawRectangle(); break;
                case ASN1Codes.C_DRAW_SECTOR: pAction = new MHDrawArcSector(":DrawSector", true); break;
                case ASN1Codes.C_FORK: pAction = new MHCall(":Fork", true); break;
                case ASN1Codes.C_GET_AVAILABILITY_STATUS: pAction = new MHGetAvailabilityStatus(); break;
                case ASN1Codes.C_GET_BOX_SIZE: pAction = new MHGetBoxSize(); break;
                case ASN1Codes.C_GET_CELL_ITEM: pAction = new MHGetCellItem(); break;
                case ASN1Codes.C_GET_CURSOR_POSITION: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // ?
                case ASN1Codes.C_GET_ENGINE_SUPPORT: pAction = new MHGetEngineSupport(); break;
                case ASN1Codes.C_GET_ENTRY_POINT: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // EntryField
                case ASN1Codes.C_GET_FILL_COLOUR: pAction = new MHGetFillColour(); break;
                case ASN1Codes.C_GET_FIRST_ITEM: pAction = new MHGetFirstItem(); break;
                case ASN1Codes.C_GET_HIGHLIGHT_STATUS: pAction = new MHTemporary(pElemAction.GetTagNo()); break;// ?
                case ASN1Codes.C_GET_INTERACTION_STATUS: pAction = new MHTemporary(pElemAction.GetTagNo()); break;// ?
                case ASN1Codes.C_GET_ITEM_STATUS: pAction = new MHGetItemStatus(); break;
                case ASN1Codes.C_GET_LABEL: pAction = new MHTemporary(pElemAction.GetTagNo()); break;// PushButton
                case ASN1Codes.C_GET_LAST_ANCHOR_FIRED: pAction = new MHTemporary(pElemAction.GetTagNo()); break;// HyperText
                case ASN1Codes.C_GET_LINE_COLOUR: pAction = new MHGetLineColour(); break;
                case ASN1Codes.C_GET_LINE_STYLE: pAction = new MHGetLineStyle(); break;
                case ASN1Codes.C_GET_LINE_WIDTH: pAction = new MHGetLineWidth(); break;
                case ASN1Codes.C_GET_LIST_ITEM: pAction = new MHGetListItem(); break;
                case ASN1Codes.C_GET_LIST_SIZE: pAction = new MHGetListSize(); break;
                case ASN1Codes.C_GET_OVERWRITE_MODE: pAction = new MHTemporary(pElemAction.GetTagNo()); break;// ?
                case ASN1Codes.C_GET_PORTION: pAction = new MHTemporary(pElemAction.GetTagNo()); break;// Slider
                case ASN1Codes.C_GET_POSITION: pAction = new MHGetPosition(); break;
                case ASN1Codes.C_GET_RUNNING_STATUS: pAction = new MHGetRunningStatus(); break;
                case ASN1Codes.C_GET_SELECTION_STATUS: pAction = new MHTemporary(pElemAction.GetTagNo()); break;// ?
                case ASN1Codes.C_GET_SLIDER_VALUE: pAction = new MHTemporary(pElemAction.GetTagNo()); break;// Slider
                case ASN1Codes.C_GET_TEXT_CONTENT: pAction = new MHTemporary(pElemAction.GetTagNo()); break;// Text
                case ASN1Codes.C_GET_TEXT_DATA: pAction = new MHGetTextData(); break;
                case ASN1Codes.C_GET_TOKEN_POSITION: pAction = new MHGetTokenPosition(); break;
                case ASN1Codes.C_GET_VOLUME: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // ?
                case ASN1Codes.C_LAUNCH: pAction = new MHLaunch(); break;
                case ASN1Codes.C_LOCK_SCREEN: pAction = new MHLockScreen(); break;
                case ASN1Codes.C_MODULO: pAction = new MHModulo(); break;
                case ASN1Codes.C_MOVE: pAction = new MHMove(); break;
                case ASN1Codes.C_MOVE_TO: pAction = new MHMoveTo(); break;
                case ASN1Codes.C_MULTIPLY: pAction = new MHMultiply(); break;
                case ASN1Codes.C_OPEN_CONNECTION: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // ?
                case ASN1Codes.C_PRELOAD: pAction = new MHPreload(); break;
                case ASN1Codes.C_PUT_BEFORE: pAction = new MHPutBefore(); break;
                case ASN1Codes.C_PUT_BEHIND: pAction = new MHPutBehind(); break;
                case ASN1Codes.C_QUIT: pAction = new MHQuit(); break;
                case ASN1Codes.C_READ_PERSISTENT: pAction = new MHPersistent(":ReadPersistent", true); break;
                case ASN1Codes.C_RUN: pAction = new MHRun(); break;
                case ASN1Codes.C_SCALE_BITMAP: pAction = new MHScaleBitmap(); break;
                case ASN1Codes.C_SCALE_VIDEO: pAction = new MHScaleVideo(); break;
                case ASN1Codes.C_SCROLL_ITEMS: pAction = new MHScrollItems(); break;
                case ASN1Codes.C_SELECT: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // Button
                case ASN1Codes.C_SELECT_ITEM: pAction = new MHSelectItem(); break;
                case ASN1Codes.C_SEND_EVENT: pAction = new MHSendEvent(); break;
                case ASN1Codes.C_SEND_TO_BACK: pAction = new MHSendToBack(); break;
                case ASN1Codes.C_SET_BOX_SIZE: pAction = new MHSetBoxSize(); break;
                case ASN1Codes.C_SET_CACHE_PRIORITY: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // ?
                case ASN1Codes.C_SET_COUNTER_END_POSITION: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // Stream
                case ASN1Codes.C_SET_COUNTER_POSITION: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // Stream
                case ASN1Codes.C_SET_COUNTER_TRIGGER: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // Stream
                case ASN1Codes.C_SET_CURSOR_POSITION: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // ?
                case ASN1Codes.C_SET_CURSOR_SHAPE: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // ?
                case ASN1Codes.C_SET_DATA: pAction = new MHSetData(); break;
                case ASN1Codes.C_SET_ENTRY_POINT: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // EntryField
                case ASN1Codes.C_SET_FILL_COLOUR: pAction = new MHSetFillColour(); break;
                case ASN1Codes.C_SET_FIRST_ITEM: pAction = new MHSetFirstItem(); break;
                case ASN1Codes.C_SET_FONT_REF: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // Text
                case ASN1Codes.C_SET_HIGHLIGHT_STATUS: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // ?
                case ASN1Codes.C_SET_INTERACTION_STATUS: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // ?
                case ASN1Codes.C_SET_LABEL: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // PushButton
                case ASN1Codes.C_SET_LINE_COLOUR: pAction = new MHSetLineColour(); break;
                case ASN1Codes.C_SET_LINE_STYLE: pAction = new MHSetLineStyle(); break;
                case ASN1Codes.C_SET_LINE_WIDTH: pAction = new MHSetLineWidth(); break;
                case ASN1Codes.C_SET_OVERWRITE_MODE: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // EntryField
                case ASN1Codes.C_SET_PALETTE_REF: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // Visible
                case ASN1Codes.C_SET_PORTION: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // Slider
                case ASN1Codes.C_SET_POSITION: pAction = new MHSetPosition(); break;
                case ASN1Codes.C_SET_SLIDER_VALUE: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // Slider
                case ASN1Codes.C_SET_SPEED: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // ?
                case ASN1Codes.C_SET_TIMER: pAction = new MHSetTimer(); break;
                case ASN1Codes.C_SET_TRANSPARENCY: pAction = new MHSetTransparency(); break;
                case ASN1Codes.C_SET_VARIABLE: pAction = new MHSetVariable(); break;
                case ASN1Codes.C_SET_VOLUME: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // ?
                case ASN1Codes.C_SPAWN: pAction = new MHSpawn(); break;
                case ASN1Codes.C_STEP: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // Slider
                case ASN1Codes.C_STOP: pAction = new MHStop(); break;
                case ASN1Codes.C_STORE_PERSISTENT: pAction = new MHPersistent(":StorePersistent", false); break;
                case ASN1Codes.C_SUBTRACT: pAction = new MHSubtract(); break;
                case ASN1Codes.C_TEST_VARIABLE: pAction = new MHTestVariable(); break;
                case ASN1Codes.C_TOGGLE: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // Button
                case ASN1Codes.C_TOGGLE_ITEM: pAction = new MHToggleItem(); break;
                case ASN1Codes.C_TRANSITION_TO: pAction = new MHTransitionTo(); break;
                case ASN1Codes.C_UNLOAD: pAction = new MHUnload(); break;
                case ASN1Codes.C_UNLOCK_SCREEN: pAction = new MHUnlockScreen(); break;
                // UK MHEG added actions.
                case ASN1Codes.C_SET_BACKGROUND_COLOUR: pAction = new MHSetBackgroundColour(); break;
                case ASN1Codes.C_SET_CELL_POSITION: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // ?
                case ASN1Codes.C_SET_INPUT_REGISTER: pAction = new MHSetInputRegister(); break;
                case ASN1Codes.C_SET_TEXT_COLOUR: pAction = new MHSetTextColour(); break;
                case ASN1Codes.C_SET_FONT_ATTRIBUTES: pAction = new MHSetFontAttributes(); break;
                case ASN1Codes.C_SET_VIDEO_DECODE_OFFSET: pAction = new MHSetVideoDecodeOffset(); break;
                case ASN1Codes.C_GET_VIDEO_DECODE_OFFSET: pAction = new MHGetVideoDecodeOffset(); break;
                case ASN1Codes.C_GET_FOCUS_POSITION: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // HyperText
                case ASN1Codes.C_SET_FOCUS_POSITION: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // HyperText
                case ASN1Codes.C_SET_BITMAP_DECODE_OFFSET: pAction = new MHSetBitmapDecodeOffset(); break;
                case ASN1Codes.C_GET_BITMAP_DECODE_OFFSET: pAction = new MHGetBitmapDecodeOffset(); break;
                case ASN1Codes.C_SET_SLIDER_PARAMETERS: pAction = new MHTemporary(pElemAction.GetTagNo()); break; // ?

                default:
                    Logging.Log(Logging.MHLogWarning, "Action " + pElemAction.GetTagNo() + " not implemented");
                    Logging.Assert(false); // So we find out about these when debugging.
                    // Future proofing: ignore any actions that we don't know about.
                    // Obviously these can only arise in the binary coding.
                    pAction = null;
                    break;
                }
                if (pAction != null) 
                {
                    Append(pAction); // Add to the sequence.
                    pAction.Initialise(pElemAction, engine);
                }
            }
        }

        public void Print(TextWriter writer, int nTabs)
        {
            for (int i = 0; i < Size; i++)
            {
                GetAt(i).Print(writer, nTabs);
            }
        }
    }
}
