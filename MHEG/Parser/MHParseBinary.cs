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
    class MHParseBinary : IMHParser
    {
        private int m_p; // Count of bytes read
        private byte[] m_data;
        
        public MHParseBinary(byte[] program)
        {
            m_data = program;
            m_p = 0;
        }

        public MHParseNode Parse()
        {
            return DoParse();
        }

        // Get the next byte.  In most all cases it's an error if we reach end-of-file
        // and we throw an exception.
        private byte GetNextChar()
        {
            if (m_p >= (int)m_data.Length) throw new MHEGException("Unexpected end of file");
            return m_data[m_p++];
        }

        // Parse a string argument.  ASN1 strings can include nulls as valid characters.
        private void ParseString(int endStr, MHOctetString str)
        {
            // TODO: Don't deal with indefinite length at the moment.
            Logging.Assert(endStr != INDEFINITE_LENGTH); 

            int nLength = endStr - m_p;
            char[] stringValue = new char[nLength];
            int p = 0;
            while (m_p < endStr) stringValue[p++] = (char)GetNextChar();
            str.Copy(new MHOctetString(new string(stringValue, 0, p)));
        }

        // Parse an integer argument.  Also used for bool and enum.
        private int ParseInt(int endInt)
        {
            int intVal = 0;
            bool firstByte = true;
            
            // TODO: Don't deal with indefinite length at the moment.
            Logging.Assert(endInt != INDEFINITE_LENGTH); 
            
            while (m_p < endInt) 
            {
                byte ch = GetNextChar();
                // Integer values are signed so if the top bit is set in the first byte
                // we need to set the sign bit.
                if (firstByte && ch >= 128) intVal = -1;
                firstByte = false;
                intVal = (intVal << 8) | ch;
            }
            return intVal;
        }

        public const int Universal = 0;
        public const int Context = 1;
        public const int Pseudo = 2;

        //  Simple recursive parser for ASN1 BER.
        private MHParseNode DoParse()
        {
            byte ch;
            // Tag class
            int tagClass = Universal;
            // Byte count of end of this item.  Set to INDEFINITE_LENGTH if the length is Indefinite.
            int endOfItem;
            int tagNumber = 0;

            // Read the first character.
            ch = GetNextChar();

            // ASN1 Coding rules: Top two bits (0 and 1) indicate the tag class.
            // 0x00 - Universal,  0x40 - Application, 0x80 - Context-specific, 0xC0 - Private
            // We only use Universal and Context.
            switch (ch & 0xC0) 
            {
            case 0x00: // Universal
                tagClass = Universal;
                break;
            case 0x80:
                tagClass = Context;
                break;
            default:
                throw new MHEGException("Invalid tag class = " + ch);
            }
            // Bit 2 indicates whether it is a simple or compound type.  Not used.
            // Lower bits are the tag number.
            tagNumber = ch & 0x1f;
            if (tagNumber == 0x1f) { // Except that if it is 0x1F then the tag is encoded in the following bytes.
                tagNumber = 0;
                do {
                    ch = GetNextChar();
                    tagNumber = (tagNumber << 7) | (ch & 0x7f);
                } while ((ch & 0x80) != 0); // Top bit set means there's more to come.
            }

            // Next byte is the length.  If it is less than 128 it is the actual length, otherwise it
            // gives the number of bytes containing the length, except that if this is zero the item
            // has an "indefinite" length and is terminated by two zero bytes.
            ch = GetNextChar();
            if ((ch & 0x80) != 0) {
                int lengthOfLength = ch & 0x7f;
                if (lengthOfLength == 0) endOfItem = INDEFINITE_LENGTH;
                else {
                    endOfItem = 0;
                    while ((lengthOfLength--) != 0) {
                        ch = GetNextChar();
                        endOfItem = (endOfItem << 8) | ch;
                    }
                    endOfItem += m_p;
                }
            }
            else endOfItem = ch + m_p;

            if (tagClass == Context) {
                MHPTagged pNode = new MHPTagged(tagNumber);
                // The argument here depends on the particular tag we're processing.
                switch (tagNumber) {
                case ASN1Codes.C_MULTIPLE_SELECTION:
                case ASN1Codes.C_OBSCURED_INPUT:
                case ASN1Codes.C_INITIALLY_AVAILABLE:
                case ASN1Codes.C_WRAP_AROUND:
                case ASN1Codes.C_TEXT_WRAPPING:
                case ASN1Codes.C_INITIALLY_ACTIVE:
                case ASN1Codes.C_MOVING_CURSOR:
                case ASN1Codes.C_SHARED:
                case ASN1Codes.C_ENGINE_RESP:
                case ASN1Codes.C_TILING:
                case ASN1Codes.C_BORDERED_BOUNDING_BOX:
                    { // BOOL
                        // If there is no argument we need to indicate that so that it gets
                        // the correct default value.
                        if (m_p != endOfItem){
                            int intVal = ParseInt(endOfItem); // May raise an exception
                            pNode.AddArg(new MHPBool(intVal != 0));
                        }
                        break;
                    }

                case ASN1Codes.C_INPUT_TYPE:
                case ASN1Codes.C_SLIDER_STYLE:
                case ASN1Codes.C_TERMINATION:
                case ASN1Codes.C_ORIENTATION:
                case ASN1Codes.C_HORIZONTAL_JUSTIFICATION:
                case ASN1Codes.C_BUTTON_STYLE:
                case ASN1Codes.C_START_CORNER:
                case ASN1Codes.C_LINE_ORIENTATION:
                case ASN1Codes.C_VERTICAL_JUSTIFICATION:
                case ASN1Codes.C_STORAGE:
                    { // ENUM
                        if (m_p != endOfItem){
                            int intVal = ParseInt(endOfItem); // May raise an exception
                            pNode.AddArg(new MHPEnum(intVal));
                        }
                        break;
                    }

                case ASN1Codes.C_INITIAL_PORTION:
                case ASN1Codes.C_STEP_SIZE:
                case ASN1Codes.C_INPUT_EVENT_REGISTER:
                case ASN1Codes.C_INITIAL_VALUE:
                case ASN1Codes.C_IP_CONTENT_HOOK:
                case ASN1Codes.C_MAX_VALUE:
                case ASN1Codes.C_MIN_VALUE:
                case ASN1Codes.C_LINE_ART_CONTENT_HOOK:
                case ASN1Codes.C_BITMAP_CONTENT_HOOK:
                case ASN1Codes.C_TEXT_CONTENT_HOOK:
                case ASN1Codes.C_STREAM_CONTENT_HOOK:
                case ASN1Codes.C_MAX_LENGTH:
                case ASN1Codes.C_CHARACTER_SET:
                case ASN1Codes.C_ORIGINAL_TRANSPARENCY:
                case ASN1Codes.C_ORIGINAL_GC_PRIORITY:
                case ASN1Codes.C_LOOPING:
                case ASN1Codes.C_ORIGINAL_LINE_STYLE:
                case ASN1Codes.C_STANDARD_VERSION:
                case ASN1Codes.C_ORIGINAL_LINE_WIDTH:
                case ASN1Codes.C_CONTENT_HOOK:
                case ASN1Codes.C_CONTENT_CACHE_PRIORITY:
                case ASN1Codes.C_COMPONENT_TAG:
                case ASN1Codes.C_ORIGINAL_VOLUME:
                case ASN1Codes.C_PROGRAM_CONNECTION_TAG:
                case ASN1Codes.C_CONTENT_SIZE:
                    { // INT
                        if (m_p != endOfItem){
                            int intVal = ParseInt(endOfItem); // May raise an exception
                            pNode.AddArg(new MHPInt(intVal));
                        }
                        break;
                    }

                case ASN1Codes.C_OBJECT_INFORMATION:
                case ASN1Codes.C_CONTENT_REFERENCE:
                case ASN1Codes.C_FONT_ATTRIBUTES:
                case ASN1Codes.C_CHAR_LIST:
                case ASN1Codes.C_NAME:
                case ASN1Codes.C_ORIGINAL_LABEL:
                    { // STRING
                        // Unlike INT, BOOL and ENUM we can't distinguish an empty string
                        // from a missing string.
                        MHOctetString str = new MHOctetString();
                        ParseString(endOfItem, str);
                        pNode.AddArg(new MHPString(str));
                        break;
                    }

                default:
                    {
                        // Everything else has either no argument or is self-describing
                        // TODO: Handle indefinite length.
                        Logging.Assert(endOfItem != INDEFINITE_LENGTH); // For the moment.
                        while (m_p < endOfItem) {
                            pNode.AddArg(DoParse());
                        }
                        break;
                    }
                }                
                return pNode;
            }
            else { // Universal - i.e. a primitive type.
                // Tag values

                switch (tagNumber) 
                {
                case ASN1Codes.U_BOOL: // Boolean
                    {
                        int intVal = ParseInt(endOfItem);
                        return new MHPBool(intVal != 0);     
                    }
                case ASN1Codes.U_INT: // Integer
                    {
                        int intVal = ParseInt(endOfItem);
                        return new MHPInt(intVal);
                    }
                case ASN1Codes.U_ENUM: // ENUM
                    {
                        int intVal = ParseInt(endOfItem);
                        return new MHPEnum(intVal);
                    }
                case ASN1Codes.U_STRING: // String
                    {
                        MHOctetString str = new MHOctetString();
                        ParseString(endOfItem, str);
                        return new MHPString(str);
                    }
                case ASN1Codes.U_NULL: // ASN1 NULL
                    {
                        return new MHPNull();
                    }
                case ASN1Codes.U_SEQUENCE: // Sequence
                    {
                        MHParseSequence pNode = new MHParseSequence();
                        Logging.Assert(endOfItem != INDEFINITE_LENGTH); // TODO: Implement this.
                        while (m_p < endOfItem) 
                        {
                            pNode.Append(DoParse());
                        }
                        Logging.Assert(m_p == endOfItem);
                        return pNode;
                    }
                default:
                    Logging.Assert(false);
                    throw new MHEGException("Unknown universal");
                }
            }
        }

        public const int INDEFINITE_LENGTH = -1;
    }
}
