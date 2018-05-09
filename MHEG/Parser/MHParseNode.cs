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

namespace MHEG.Parser
{
    class MHParseNode
    {
        private int m_nNodeType;

        public MHParseNode(int type)
        {
            m_nNodeType = type;
        }

        public void Failure(string p)
        {
            throw new MHEGException(p);
        }
        
        public int GetTagNo()
        {
            if (m_nNodeType != PNTagged) Failure("Expected tagged value");
            return ((MHPTagged)this).TagNo;
        }

        // Return the number of items in the sequence.
        public int GetArgCount()
        {
            if (m_nNodeType == PNTagged) {
                MHPTagged pTag = (MHPTagged)this;
                return pTag.Args.Size;
            }
            else if (m_nNodeType == PNSeq) {
                MHParseSequence pSeq = (MHParseSequence)this;
                return pSeq.Size;
            }
            else Failure("Expected tagged value");
            return 0; // To keep the compiler happy
        }

        // Get the Nth entry.
        public MHParseNode GetArgN(int n)
        {
            if (m_nNodeType == PNTagged) {
                MHPTagged pTag = (MHPTagged)this;
                if (n < 0 || n >= pTag.Args.Size) Failure("Argument not found");
                return pTag.Args.GetAt(n);
            }
            else if (m_nNodeType == PNSeq) {
                MHParseSequence pSeq = (MHParseSequence)this;
                if (n < 0 || n >= pSeq.Size) Failure("Argument not found");
                return pSeq.GetAt(n);
            }
            else Failure("Expected tagged value");
            return null; // To keep the compiler happy
        }

        // Get an argument with a specific tag.  Returns NULL if it doesn't exist.
        // There is a defined order of tags for both the binary and textual representations.
        // Unfortunately they're not the same.
        public MHParseNode GetNamedArg(int nTag)
        {
            MHParseSequence pArgs = null;
            if (m_nNodeType == PNTagged) pArgs = ((MHPTagged)this).Args;
            else if (m_nNodeType == PNSeq) pArgs = (MHParseSequence)this;
            else Failure("Expected tagged value or sequence");
            for (int i = 0; i < pArgs.Size; i++) {
                MHParseNode p = pArgs.GetAt(i);
                if (p != null && p.NodeType == PNTagged && ((MHPTagged)p).TagNo == nTag) return p;
            }
            return null;
        }


        // Sequence.
        public int GetSeqCount()
        {
            if (m_nNodeType != PNSeq) Failure("Expected sequence");
            MHParseSequence pSeq = (MHParseSequence)this;
            return pSeq.Size;
        }

        public MHParseNode GetSeqN(int n)
        {
            if (m_nNodeType != PNSeq) Failure("Expected sequence");
            MHParseSequence pSeq = (MHParseSequence)this;
            if (n < 0 || n >= pSeq.Size) Failure("Argument not found");
            return pSeq.GetAt(n);
        }


        public int NodeType
        {
            get { return m_nNodeType; }
        }

        public int GetIntValue()
        {
            if (m_nNodeType != PNInt) Failure("Expected integer");
            return ((MHPInt)this).Value;
        }

        public int GetEnumValue()
        {
            if (m_nNodeType != PNEnum) Failure("Expected enumerated type");
            return ((MHPEnum)this).Value;
        }

        public bool GetBoolValue()
        {
            if (m_nNodeType != PNBool) Failure("Expected boolean");
            return ((MHPBool)this).Value;
        }

        public void GetStringValue(MHOctetString str)
        {
            if (m_nNodeType != PNString) Failure("Expected string");
            str.Copy(((MHPString)this).Value);
        }

        public const int PNTagged = 0;
        public const int PNBool = 1;
        public const int PNInt = 2;
        public const int PNEnum = 3;
        public const int PNString = 4;
        public const int PNNull = 5;
        public const int PNSeq = 6;
    }

    class MHParseSequence : MHParseNode, IMHSequence<MHParseNode>
    {
        public MHParseSequence()
            : base(PNSeq)
        {
            m_Values = new List<MHParseNode>();
        }

        private List<MHParseNode> m_Values;

        public int Size
        {
            get { return m_Values.Count; }
        }

        public MHParseNode GetAt(int i)
        {
            return m_Values[i];
        }

        public void InsertAt(MHParseNode b, int n)
        {
            m_Values.Insert(n, b);
        }

        public void Append(MHParseNode b)
        {
            m_Values.Add(b);
        }

        public void RemoveAt(int i)
        {
            m_Values.RemoveAt(i);
        }
     }

    class MHPTagged : MHParseNode
    {
        private int m_TagNo;
        private MHParseSequence m_Args;

        public MHPTagged(int nTag)
            : base(PNTagged)
        {
            m_Args = new MHParseSequence();
            m_TagNo = nTag;
        }

        public int TagNo
        {
            get { return m_TagNo; }
        }

        public MHParseSequence Args
        {
            get { return m_Args; }
        }

        // Add an argument to the argument sequence.
        public void AddArg(MHParseNode node)
        {
            m_Args.Append(node);
        }
    }

    class MHPInt : MHParseNode
    {
        private int m_Value;

        public MHPInt(int v) : base(PNInt)
        {            
            m_Value = v;
        }

        public int Value
        {
            get { return m_Value; }
        }
    }

    class MHPEnum : MHParseNode
    {
        private int m_Value;

        public MHPEnum(int v) : base(PNEnum)
        {
            m_Value = v;
        }

        public int Value
        {
            get { return m_Value; }
        }
    }

    class MHPBool : MHParseNode
    {
        private bool m_Value;

        public MHPBool(bool v) : base(PNBool)
        {
            m_Value = v;
        }

        public bool Value
        {
            get { return m_Value; }
        }
    }

    class MHPString : MHParseNode
    {
        private MHOctetString m_Value;

        public MHPString(MHOctetString v)
            : base(PNString)
        {
            m_Value = v;
        }

        public MHOctetString Value
        {
            get { return m_Value; }
        }
    }

    class MHPNull : MHParseNode
    {
        public MHPNull() : base(PNNull)
        {
        }
    }
}
