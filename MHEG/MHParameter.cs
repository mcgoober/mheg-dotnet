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

namespace MHEG
{
    class MHParameter
    {
        private int m_Type;

        private MHGenericInteger m_IntVal;
        private MHGenericBoolean m_BoolVal;
        private MHGenericOctetString m_StrVal;
        private MHGenericObjectRef m_ObjRefVal;
        private MHGenericContentRef m_ContentRefVal;

        public MHParameter()
        {
            m_Type = P_Null;
            m_IntVal = new MHGenericInteger();
            m_BoolVal = new MHGenericBoolean();
            m_StrVal = new MHGenericOctetString();
            m_ObjRefVal = new MHGenericObjectRef();
            m_ContentRefVal = new MHGenericContentRef();
        }

        public const int P_Int = 0;
        public const int P_Bool = 1;
        public const int P_String = 2;
        public const int P_ObjRef = 3;
        public const int P_ContentRef = 4;
        public const int P_Null = 5;
        
        public void Initialise(MHParseNode p, MHEngine engine)
        {
            switch (p.GetTagNo())
            {
                case ASN1Codes.C_NEW_GENERIC_BOOLEAN: m_Type = P_Bool; m_BoolVal.Initialise(p.GetArgN(0), engine); break;
                case ASN1Codes.C_NEW_GENERIC_INTEGER: m_Type = P_Int; m_IntVal.Initialise(p.GetArgN(0), engine); break;
                case ASN1Codes.C_NEW_GENERIC_OCTETSTRING: m_Type = P_String; m_StrVal.Initialise(p.GetArgN(0), engine); break;
                case ASN1Codes.C_NEW_GENERIC_OBJECT_REF: m_Type = P_ObjRef; m_ObjRefVal.Initialise(p.GetArgN(0), engine); break;
                case ASN1Codes.C_NEW_GENERIC_CONTENT_REF: m_Type = P_ContentRef; m_ContentRefVal.Initialise(p.GetArgN(0), engine); break;
                default: p.Failure("Expected generic"); break;
            }
        }

        public void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs);
            switch (m_Type)
            {
                // Direct values.
                case P_Int: writer.Write(":GInteger "); m_IntVal.Print(writer, 0); break;
                case P_Bool: writer.Write(":GBoolean "); m_BoolVal.Print(writer, 0); break;
                case P_String: writer.Write(":GOctetString "); m_StrVal.Print(writer, 0); break;
                case P_ObjRef: writer.Write(":GObjectRef "); m_ObjRefVal.Print(writer, 0); break;
                case P_ContentRef: writer.Write(":GObjectRef "); m_ContentRefVal.Print(writer, 0); break;
                case P_Null: break;
            }
        }

        // Get an indirect reference.
        public MHObjectRef GetReference()
        {
            switch (m_Type)
            {
                case P_Int: return m_IntVal.GetReference();
                case P_Bool: return m_BoolVal.GetReference();
                case P_String: return m_StrVal.GetReference();
                case P_ObjRef: return m_ObjRefVal.GetReference();
                case P_ContentRef: return m_ContentRefVal.GetReference();
                case P_Null: return null;
            }
            return null; // To keep compiler happy
        }

        public int Type
        {
            get { return m_Type; }
        }

        public MHGenericBoolean Bool
        {
            get { return m_BoolVal; }
        }

        public MHGenericInteger Int
        {
            get { return m_IntVal; }
        }

        public MHGenericOctetString String
        {
            get { return m_StrVal; }
        }

        public MHGenericContentRef ContentRef
        {
            get { return m_ContentRefVal; }
        }

        public MHGenericObjectRef ObjRef
        {
            get { return m_ObjRefVal; }
        }
    }

    class MHGenericBase
    {
        protected bool m_fIsDirect;
        protected MHObjectRef m_Indirect;

        public MHGenericBase()
        {
            m_Indirect = new MHObjectRef();
        }

        public MHObjectRef GetReference()
        {
            if (m_fIsDirect)
            {
                throw new MHEGException("Expected indirect reference");
            }
            return m_Indirect;
        }
    }

    class MHGenericBoolean : MHGenericBase
    {
        protected bool m_fDirect;

        public MHGenericBoolean() : base()
        {
            m_fDirect = false;
        }

        public void Initialise(MHParseNode arg, MHEngine engine)
        {
            if (arg.NodeType == MHParseNode.PNTagged && arg.GetTagNo() == ASN1Codes.C_INDIRECTREFERENCE) 
            {
                // Indirect reference.
                m_fIsDirect = false;
                m_Indirect.Initialise(arg.GetArgN(0), engine);
            }
            else 
            { // Simple integer value.
                m_fIsDirect = true;
                m_fDirect = arg.GetBoolValue();
            }
        }

        public void Print(TextWriter writer, int nTabs)
        {
            if (m_fIsDirect) writer.Write("{0} ", m_fDirect ? "true" : "false");
            else { writer.Write(":IndirectRef "); m_Indirect.Print(writer, nTabs + 1); }
        }

        // Return the value, looking up any indirect ref.
        public bool GetValue(MHEngine engine)
        {
            if (m_fIsDirect) return m_fDirect;
            else 
            {
                MHUnion result = new MHUnion();
                MHRoot pBase = engine.FindObject(m_Indirect);
                pBase.GetVariableValue(result, engine);
                result.CheckType(MHUnion.U_Bool);
                return result.Bool;
            }
        }
    }

    class MHGenericInteger : MHGenericBase
    {
        protected int m_nDirect;

        public MHGenericInteger() : base()
        {
            m_nDirect = 0;
        }

        public void Initialise(MHParseNode arg, MHEngine engine)
        {
            if (arg.NodeType == MHParseNode.PNTagged && arg.GetTagNo() == ASN1Codes.C_INDIRECTREFERENCE)
            {
                // Indirect reference.
                m_fIsDirect = false;
                m_Indirect.Initialise(arg.GetArgN(0), engine);
            }
            else
            { // Simple integer value.
                m_fIsDirect = true;
                m_nDirect = arg.GetIntValue();
            }
        }

        public void Print(TextWriter writer, int nTabs)
        {
            if (m_fIsDirect) writer.Write("{0} ", m_nDirect);
            else { writer.Write(":IndirectRef "); m_Indirect.Print(writer, nTabs + 1); }
        }

        // Return the value, looking up any indirect ref.
        public int GetValue(MHEngine engine)
        {
            if (m_fIsDirect) return m_nDirect;
            else 
            {
                MHUnion result = new MHUnion();
                MHRoot pBase = engine.FindObject(m_Indirect);
                pBase.GetVariableValue(result, engine);
                // From my reading of the MHEG documents implicit conversion is only
                // performed when assigning variables.  Nevertheless the Channel 4
                // Teletext assumes that implicit conversion takes place here as well.
                if (result.Type == MHUnion.U_String) 
                {
                    // Implicit conversion of string to integer.
                    return Convert.ToInt32(result.String.ToString());
                }
                else 
                {
                    result.CheckType(MHUnion.U_Int);
                    return result.Int;
                }
            }
        }
    }
    class MHGenericOctetString : MHGenericBase
    {
        protected MHOctetString m_Direct;

        public MHGenericOctetString() : base()
        {
            m_Direct = new MHOctetString();
        }

        public void Initialise(MHParseNode arg, MHEngine engine)
        {
            if (arg.NodeType == MHParseNode.PNTagged && arg.GetTagNo() == ASN1Codes.C_INDIRECTREFERENCE)
            {
                // Indirect reference.
                m_fIsDirect = false;
                m_Indirect.Initialise(arg.GetArgN(0), engine);
            }
            else
            { // Simple integer value.
                m_fIsDirect = true;
                arg.GetStringValue(m_Direct);
            }
        }

        public void Print(TextWriter writer, int nTabs)
        {
            if (m_fIsDirect) m_Direct.Print(writer, 0);
            else { writer.Write(":IndirectRef "); m_Indirect.Print(writer, nTabs + 1); }
        }

        // Return the value, looking up any indirect ref.
        public void GetValue(MHOctetString str, MHEngine engine)
        {
            if (m_fIsDirect) str.Copy(m_Direct);
            else
            {
                MHUnion result = new MHUnion();
                MHRoot pBase = engine.FindObject(m_Indirect);
                pBase.GetVariableValue(result, engine);
                // From my reading of the MHEG documents implicit conversion is only
                // performed when assigning variables.  Nevertheless the Channel 4
                // Teletext assumes that implicit conversion takes place here as well.
                if (result.Type == MHUnion.U_Int)
                {
                    // Implicit conversion of string to integer.
                    MHOctetString s = new MHOctetString("" + result.Int);
                    str.Copy(s);
                }
                else
                {
                    result.CheckType(MHUnion.U_String);
                    str.Copy(result.String);
                }
            }
        }
    }

    class MHGenericObjectRef : MHGenericBase
    {
        protected MHObjectRef m_ObjRef;

        public MHGenericObjectRef() : base()
        {
            m_ObjRef = new MHObjectRef();
        }

        public void Initialise(MHParseNode arg, MHEngine engine)
        {
            if (arg.NodeType == MHParseNode.PNTagged && arg.GetTagNo() == ASN1Codes.C_INDIRECTREFERENCE)
            {
                // Indirect reference.
                m_fIsDirect = false;
                m_Indirect.Initialise(arg.GetArgN(0), engine);
            }
            else
            { // Simple integer value.
                m_fIsDirect = true;
                m_ObjRef.Initialise(arg, engine);
            }
        }

        public void Print(TextWriter writer, int nTabs)
        {
            if (m_fIsDirect) m_ObjRef.Print(writer, 0);
            else { writer.Write(":IndirectRef "); m_Indirect.Print(writer, nTabs + 1); }
        }

        // Return the value, looking up any indirect ref.
        public void GetValue(MHObjectRef reference, MHEngine engine)
        {
            if (m_fIsDirect) reference.Copy(m_ObjRef);
            else {
                MHUnion result = new MHUnion();
                MHRoot pBase = engine.FindObject(m_Indirect);
                pBase.GetVariableValue(result, engine);
                result.CheckType(MHUnion.U_ObjRef);
                reference.Copy(result.ObjRef);
            }
        }
    }
    class MHGenericContentRef : MHGenericBase
    {
        protected MHContentRef m_Direct;

        public MHGenericContentRef() : base()
        {
            m_Direct = new MHContentRef();
        }

        public void Initialise(MHParseNode arg, MHEngine engine)
        {
            if (arg.GetTagNo() == ASN1Codes.C_INDIRECTREFERENCE)
            {
                // Indirect reference.
                m_fIsDirect = false;
                m_Indirect.Initialise(arg.GetArgN(0), engine);
            }
            else if (arg.GetTagNo() == ASN1Codes.C_CONTENT_REFERENCE)
            { // Simple integer value.
                m_fIsDirect = true;
                m_Direct.Initialise(arg.GetArgN(0), engine);
            }
            else
            {
                throw new MHEGException("Expected direct or indirect content reference");
            }
        }

        public void Print(TextWriter writer, int nTabs)
        {
            if (m_fIsDirect) m_Direct.Print(writer, 0);
            else { writer.Write(":IndirectRef "); m_Indirect.Print(writer, nTabs + 1); }
        }
        
        // Return the value, looking up any indirect ref.
        public void GetValue(MHContentRef reference, MHEngine engine)
        {
            if (m_fIsDirect) reference.Copy(m_Direct);
            else {
                MHUnion result = new MHUnion();
                MHRoot pBase = engine.FindObject(m_Indirect);
                pBase.GetVariableValue(result, engine);
                result.CheckType(MHUnion.U_ContentRef);
                reference.Copy(result.ContentRef);
            }
        }
    }
}
