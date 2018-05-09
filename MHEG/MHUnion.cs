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

namespace MHEG
{
    class MHUnion
    {
        private int m_Type;

        private int m_nIntVal;
        private bool m_fBoolVal;
        private MHOctetString m_StrVal;
        private MHObjectRef m_ObjRefVal;
        private MHContentRef m_ContentRefVal;

        public MHUnion() 
        { 
            m_Type = U_None;
            m_StrVal = new MHOctetString();
            m_ObjRefVal = new MHObjectRef();
            m_ContentRefVal = new MHContentRef();
        }

        public MHUnion(int nVal)
        {
            m_Type = U_Int; 
            m_nIntVal = nVal;
        }

        public MHUnion(bool fVal)
        {
            m_Type = U_Bool; 
            m_fBoolVal = fVal; 
        }
        
        public MHUnion(MHOctetString strVal)
        {
            m_Type = U_String; 
            m_StrVal = new MHOctetString();
            m_StrVal.Copy(strVal); 
        }
        
        public MHUnion(MHObjectRef objVal)
        {
            m_Type = U_ObjRef;
            m_ObjRefVal = new MHObjectRef();
            m_ObjRefVal.Copy(objVal); 
        }

        public MHUnion(MHContentRef cnVal)
        {
            m_Type = U_ContentRef;
            m_ContentRefVal = new MHContentRef();
            m_ContentRefVal.Copy(cnVal); 
        }

        public int Type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        public const int U_Int = 0;
        public const int U_Bool = 1;
        public const int U_String = 2;
        public const int U_ObjRef = 3;
        public const int U_ContentRef = 4;
        public const int U_None = 5;

         // Copies the argument, getting the value of an indirect args.
        public void GetValueFrom(MHParameter value, MHEngine engine)
        {
            switch (value.Type) 
            {
            case MHParameter.P_Int: m_Type = U_Int; m_nIntVal = value.Int.GetValue(engine); break;
            case MHParameter.P_Bool: m_Type = U_Bool; m_fBoolVal = value.Bool.GetValue(engine); break;
            case MHParameter.P_String: m_Type = U_String; value.String.GetValue(m_StrVal, engine); break;
            case MHParameter.P_ObjRef: m_Type = U_ObjRef; value.ObjRef.GetValue(m_ObjRefVal, engine); break;
            case MHParameter.P_ContentRef: m_Type = U_ContentRef; value.ContentRef.GetValue(m_ContentRefVal, engine); break;
            case MHParameter.P_Null: m_Type = U_None; break;
            }
        }
        // Check a type and fail if it doesn't match. 
        public void CheckType (int unionType) 
        {
            if (m_Type != unionType)
            {
                throw new MHEGException("Type mismatch - expected " + GetAsString(m_Type) + " found " + GetAsString(unionType));
            }
        }

        static string GetAsString(int unionType)
        {
            switch (unionType)
            {
                case U_Int: return "int";
                case U_Bool: return "bool";
                case U_String: return "string";
                case U_ObjRef: return "objref";
                case U_ContentRef: return "contentref";
                case U_None: return "none";
            }
            return ""; // Not reached.
        }

        public bool Bool
        {
            get { return m_fBoolVal; }
            set { m_fBoolVal = value; }
        }

        public int Int
        {
            get { return m_nIntVal; }
            set { m_nIntVal = value; }
        }

        public MHOctetString String
        {
            get { return m_StrVal; }
            set { m_StrVal = value; }
        }

        public MHContentRef ContentRef
        {
            get { return m_ContentRefVal; }
            set { m_ContentRefVal = value; }
        }

        public MHObjectRef ObjRef
        {
            get { return m_ObjRefVal; }
            set { m_ObjRefVal = value; }
        }
    }
}
