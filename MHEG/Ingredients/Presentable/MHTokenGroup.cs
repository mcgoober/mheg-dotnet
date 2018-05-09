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

namespace MHEG.Ingredients.Presentable
{
    class MHTokenGroup : MHPresentable
    {
        protected MHSequence<MHMovement> m_MovementTable;
        protected MHSequence<MHTokenGroupItem> m_TokenGrpItems;
        protected MHSequence<MHActionSequence> m_NoTokenActionSlots;
        protected int m_nTokenPosition;
        
        public MHTokenGroup() 
        {
            m_MovementTable = new MHSequence<MHMovement>();
            m_TokenGrpItems = new MHSequence<MHTokenGroupItem>();
            m_NoTokenActionSlots = new MHSequence<MHActionSequence>();
            m_nTokenPosition = 1;
        }

        public override string ClassName() 
        {
            return "TokenGroup"; 
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            MHParseNode pMovements = p.GetNamedArg(ASN1Codes.C_MOVEMENT_TABLE);
            if (pMovements != null) 
            {
                for (int i = 0; i < pMovements.GetArgCount(); i++) 
                {
                    MHMovement pMove = new MHMovement();
                    m_MovementTable.Append(pMove);
                    pMove.Initialise(pMovements.GetArgN(i), engine);
                }
            }
            MHParseNode pTokenGrp = p.GetNamedArg(ASN1Codes.C_TOKEN_GROUP_ITEMS);
            if (pTokenGrp != null) 
            {
                for (int i = 0; i < pTokenGrp.GetArgCount(); i++) 
                {
                    MHTokenGroupItem pToken = new MHTokenGroupItem();
                    m_TokenGrpItems.Append(pToken);
                    pToken.Initialise(pTokenGrp.GetArgN(i), engine);
                }
            }
            MHParseNode pNoToken = p.GetNamedArg(ASN1Codes.C_NO_TOKEN_ACTION_SLOTS);
            if (pNoToken != null) 
            {
                for (int i = 0; i < pNoToken.GetArgCount(); i++) 
                {
                    MHParseNode pAct = pNoToken.GetArgN(i);
                    MHActionSequence pActions = new MHActionSequence();
                    m_NoTokenActionSlots.Append(pActions);
                    // The action slot entry may be NULL.
                    if (pAct.NodeType != MHParseNode.PNNull) pActions.Initialise(pAct, engine);
                }
            }
        }

        protected void PrintContents(TextWriter writer, int nTabs)
        {
            base.Print(writer, nTabs + 1);
            if (m_MovementTable.Size != 0) 
            {
                Logging.PrintTabs(writer, nTabs + 1); writer.Write(":MovementTable (\n");
                for (int i = 0; i < m_MovementTable.Size; i++) 
                {
                    m_MovementTable.GetAt(i).Print(writer, nTabs + 2);
                }
                Logging.PrintTabs(writer, nTabs + 1); writer.Write(")\n");
            }
            if (m_TokenGrpItems.Size != 0) 
            {
                Logging.PrintTabs(writer, nTabs + 1); writer.Write(":TokenGroupItems (\n");
                for (int i = 0; i < m_TokenGrpItems.Size; i++) 
                {
                    m_TokenGrpItems.GetAt(i).Print(writer, nTabs + 2);
                }
                Logging.PrintTabs(writer, nTabs + 1); writer.Write(")\n");
            }
            if (m_NoTokenActionSlots.Size != 0) 
            {
                Logging.PrintTabs(writer, nTabs + 1); writer.Write(":NoTokenActionSlots (\n");
                for (int i = 0; i < m_NoTokenActionSlots.Size; i++) 
                {
                    MHActionSequence pActions = m_NoTokenActionSlots.GetAt(i);
                    if (pActions.Size == 0) { Logging.PrintTabs(writer, nTabs + 2); writer.Write("NULL "); }
                    else pActions.Print(writer, nTabs + 2);
                }
                Logging.PrintTabs(writer, nTabs + 1); writer.Write(")\n");
            }
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs); writer.Write("{:TokenGroup ");
            PrintContents(writer, nTabs);
            Logging.PrintTabs(writer, nTabs); writer.Write("}\n");
        }

        public override void Activation(MHEngine engine)
        {
            if (m_fRunning) return;
            base.Activation(engine);
            // We're supposed to apply Activation to each of the "items" but it isn't clear
            // exactly what that means.  Assume it means each of the visibles.
            for (int i = 0; i < m_TokenGrpItems.Size; i++) 
            {
                MHObjectRef pObject = m_TokenGrpItems.GetAt(i).Object;
                // The object reference may be the null reference.
                // Worse: it seems that sometimes in BBC's MHEG the reference simply doesn't exist.
                if (pObject.IsSet())
                {
                    try 
                    {
                        engine.FindObject(m_TokenGrpItems.GetAt(i).Object).Activation(engine);
                    } catch (MHEGException) {}
                }
            }
            engine.EventTriggered(this, EventTokenMovedTo, new MHUnion(m_nTokenPosition));
            m_fRunning = true;
            engine.EventTriggered(this, EventIsRunning);
        }

        public override void Deactivation(MHEngine engine)
        {
            if (!RunningStatus) return;
            engine.EventTriggered(this, EventTokenMovedFrom, new MHUnion(m_nTokenPosition));
            base.Deactivation(engine);
        }

        protected void TransferToken(int newPos, MHEngine engine)
        {
            if (newPos != m_nTokenPosition)
            {
                engine.EventTriggered(this, EventTokenMovedFrom, new MHUnion(m_nTokenPosition));
                m_nTokenPosition = newPos;
                engine.EventTriggered(this, EventTokenMovedTo, new MHUnion(m_nTokenPosition));
            }
        }

        public override void CallActionSlot(int n, MHEngine engine)
        {
            if (m_nTokenPosition == 0)
            { // No slot has the token.
                if (n > 0 && n <= m_NoTokenActionSlots.Size)
                {
                    engine.AddActions(m_NoTokenActionSlots.GetAt(n - 1));
                }
            }
            else
            {
                if (m_nTokenPosition > 0 && m_nTokenPosition <= m_TokenGrpItems.Size)
                {
                    MHTokenGroupItem pGroup = m_TokenGrpItems.GetAt(m_nTokenPosition - 1);
                    if (n > 0 && n <= pGroup.ActionSlots.Size)
                    {
                        engine.AddActions(pGroup.ActionSlots.GetAt(n - 1));
                    }
                }
            }
        }

        public override void Move(int n, MHEngine engine)
        {
            if (m_nTokenPosition == 0 || n < 1 || n > m_MovementTable.Size)
            {
                TransferToken(0, engine); // Not in the standard
            }
            else
            {
                TransferToken(m_MovementTable.GetAt(n - 1).Movement.GetAt(m_nTokenPosition - 1), engine);
            }
        }

        public override void MoveTo(int n, MHEngine engine)
        {
            TransferToken(n, engine);
        }

        public override void GetTokenPosition(MHRoot pResult, MHEngine engine)
        {
            pResult.SetVariableValue(new MHUnion(m_nTokenPosition));
        }
    }
}
