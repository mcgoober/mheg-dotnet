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
using MHEG.Ingredients;
using MHEG.Ingredients.Presentable;

namespace MHEG.Parser
{
    /*
    Parser for the textual form of MHEG5.
    This is very basic and is only there to enable some test programs to be run.
    */
    class MHParseText : IMHParser
    {
        private int m_lineCount;
        private int m_ch;
        private int m_nTag;
        private int m_nInt;
        private bool m_fBool;
        private string m_String;
        private int m_nStringLength;

        private int m_p; // Count of bytes read
        private byte[] m_data;

        private int m_nType;

        public MHParseText(byte[] program)
        {
            m_data = program;
            m_lineCount = 1;
            m_String = "";
            m_p = 0;
        }

        public MHParseNode Parse()
        {
            GetNextChar(); // Initialise m_ch
            NextSym(); // Initialise the symbol values.
            return DoParse();
        }

        private void GetNextChar()
        {
            if (m_p >= m_data.Length)
                m_ch = -1;
            else m_ch = m_data[m_p++];
        }

        private void NextSym()
        {
            while (true) 
            {
                switch (m_ch) 
                {
                case '\n': 
                    m_lineCount++; // And drop to next
                    goto case ' ';
                case ' ': case '\r': case '\t': case '\f':
                    // Skip white space.
                    GetNextChar();
                    continue;

                case '/':
                    { // Comment.
                        GetNextChar();
                        if (m_ch != '/') Error("Malformed comment");
                        do { GetNextChar(); } while (m_ch != '\n' && m_ch != '\f' && m_ch != '\r');
                        continue; // Next symbol
                    }

                case ':': // Start of a tag
                    {
                        m_nType = PTTag;
                        char[] buff = new char[MAX_TAG_LENGTH+1];
                        int p = 0;
                        do {
                            buff[p++] = (char)m_ch;
                            GetNextChar();
                            if (p == MAX_TAG_LENGTH) break;
                        } while ((m_ch >= 'a' && m_ch <= 'z') || (m_ch >= 'A' && m_ch <= 'Z'));                        

                        // Look it up and return it if it's found.
                        m_nTag = FindTag(new string(buff, 0, p));
                        if (m_nTag >= 0) return;
                        // Unrecognised tag.
                        Error("Unrecognised tag");
                        break;
                    }

                case '"': // Start of a string
                    {
                        m_nType = PTString;
                        // MHEG strings can include NULLs.  For the moment we pass back the length and also
                        // null-terminate the strings.
                        StringBuilder sb = new StringBuilder();
                        while (true) 
                        {
                            GetNextChar();
                            if (m_ch == '"') break; // Finished the string.
                            if (m_ch == '\\') GetNextChar(); // Escape character. Include the next char in the string.
                            if (m_ch == '\n' || m_ch == '\r') Error("Unterminated string");

                            sb.Append((char)m_ch);
                        }
                        GetNextChar(); // Skip the closing quote
                        m_String = sb.ToString();
                        m_nStringLength = sb.Length;
                        return;
                    }

                case '\'': // Start of a string using quoted printable
                    {
                        m_nType = PTString;
                        StringBuilder sb = new StringBuilder();
                        // Quotable printable strings contain escape sequences beginning with the
                        // escape character '='.  The strings can span lines but each line must
                        // end with an equal sign.
                        while (true) 
                        {
                            GetNextChar();
                            if (m_ch == '\'') break;
                            if (m_ch == '\n') Error("Unterminated string");
                            if (m_ch == '=') { // Special code in quoted-printable.
                                // Should be followed by two hex digits or by white space and a newline.
                                GetNextChar();
                                if (m_ch == ' ' || m_ch == '\t' || m_ch == '\r' || m_ch == '\n') 
                                {
                                    // White space.  Remove everything up to the newline.
                                    while (m_ch != '\n') 
                                    {
                                        if (! (m_ch == ' ' || m_ch == '\t' || m_ch == '\r')) 
                                        {
                                            Error("Malformed quoted printable string");
                                        }
                                        GetNextChar();
                                    }
                                    continue; // continue with the first character on the next line
                                }
                                else 
                                {
                                    int nByte = 0;
                                    if (m_ch >= '0' && m_ch <= '9') nByte = m_ch - '0';
                                    else if (m_ch >= 'A' && m_ch <= 'F') nByte = m_ch - 'A' + 10;
                                    else if (m_ch >= 'a' && m_ch <= 'f') nByte = m_ch - 'a' + 10;
                                    else Error("Malformed quoted printable string");
                                    nByte *= 16;
                                    GetNextChar();
                                    if (m_ch >= '0' && m_ch <= '9') nByte += m_ch - '0';
                                    else if (m_ch >= 'A' && m_ch <= 'F') nByte += m_ch - 'A' + 10;
                                    else if (m_ch >= 'a' && m_ch <= 'f') nByte += m_ch - 'a' + 10;
                                    else Error("Malformed quoted printable string");
                                    m_ch = nByte; // Put this into the string.
                                }
                            }
                            // We grow the buffer to the largest string in the input.
                            
                            sb.Append((char)m_ch);
                        }
                        GetNextChar(); // Skip the closing quote
                        m_String = sb.ToString();
                        m_nStringLength = sb.Length;
                        return;
                    }

                case '`': // Start of a string using base 64
                    // These can, presumably span lines.
                     Logging.Assert(false); // TODO
                    break;

                case '#': // Start of 3-byte hex constant.
                    Logging.Assert(false); // TODO
                    break;

                case '-': case '0': case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8': case '9':
                    {
                        m_nType = PTInt;
                        bool negative = m_ch == '-';
                        if (negative) 
                        {
                            GetNextChar();
                            if (m_ch < '0' || m_ch > '9') Error("Expected digit after '-'");
                        }
                        // Start of a number.  Hex can be represented as 0xn.
                        // Strictly speaking hex values cannot be preceded by a minus sign.
                        m_nInt = m_ch - '0';
                        GetNextChar();
                        if (m_nInt == 0 && (m_ch == 'x' || m_ch == 'X')) 
                        {
                            throw new MHEGException("TODO");
//                            ASSERT(FALSE); // TODO
                        }
                        while (m_ch >= '0' && m_ch <= '9') 
                        {
                            m_nInt = m_nInt * 10 + m_ch - '0';
                            // TODO: What about overflow?
                            GetNextChar();
                        }
                        if (negative) m_nInt = -m_nInt;
                        return;
                    }

                case 'a': case 'b': case 'c': case 'd': case 'e': case 'f': case 'g': case 'h': case 'i': case 'j':
                case 'k': case 'l': case 'm': case 'n': case 'o': case 'p': case 'q': case 'r': case 's': case 't':
                case 'u': case 'v': case 'w': case 'x': case 'y': case 'z': 
                case 'A': case 'B': case 'C': case 'D': case 'E': case 'F': case 'G': case 'H': case 'I': case 'J':
                case 'K': case 'L': case 'M': case 'N': case 'O': case 'P': case 'Q': case 'R': case 'S': case 'T':
                case 'U': case 'V': case 'W': case 'X': case 'Y': case 'Z':
                    { // Start of an enumerated type.
                        m_nType = PTEnum;
                        char[] buff = new char[MAX_ENUM+1];
                        int p = 0;
                        do {
                            buff[p++] = (char)m_ch;
                            GetNextChar();
                            if (p == MAX_ENUM) break;
                        }
                        while ((m_ch >= 'a' && m_ch <= 'z') || (m_ch >= 'A' && m_ch <= 'Z') || m_ch == '-');
                        string b = new string(buff, 0, p);
                        if (b.Equals("NULL")) { m_nType = PTNull; return; }
                        if (b.Equals("true")) { m_nType = PTBool; m_fBool = true; return; }
                        if (b.Equals("false")) { m_nType = PTBool; m_fBool = false; return; }
                        // Look up the tag in all the tables.  Fortunately all the enumerations
                        // are distinct so we don't need to know the context.
 
                        m_nInt = MHLink.GetEventType(b);
                        if (m_nInt > 0) return;
    
                        m_nInt = MHText.GetJustification(b);
                        if (m_nInt > 0) return;
                        m_nInt = MHText.GetLineOrientation(b);
                        if (m_nInt > 0) return;
                        m_nInt = MHText.GetStartCorner(b);
                        if (m_nInt > 0) return;


                        // Check the colour table.  If it's there generate a string containing the colour info.
                        for (int i = 0; i < colourTable.Length; i++) 
                        {
                         
                            if (b.ToLower().Equals(colourTable[i].name)) 
                            {
                                m_nType = PTString;
                                string str = new string((char)colourTable[i].red, 1);
                                str += (char)colourTable[i].green;
                                str += (char)colourTable[i].blue;
                                str += (char)colourTable[i].alpha;
                                m_String = str;
                                m_nStringLength = 4;
                                return;
                            }
                        }

                        Error("Unrecognised enumeration");
                        break;
                    }

                case '{': // Start of a "section".
                    // The standard indicates that open brace followed by a tag should be written
                    // as a single word.  We'll be more lenient and allow spaces or comments between them.
                    m_nType = PTStartSection;
                    GetNextChar();
                    return;

                case '}': // End of a "section".
                    m_nType = PTEndSection;
                    GetNextChar();
                    return;

                case '(': // Start of a sequence.
                    m_nType = PTStartSeq;
                    GetNextChar();
                    return;

                case ')': // End of a sequence.
                    m_nType = PTEndSeq;
                    GetNextChar();
                    return;

                case -1:
                    m_nType = PTEOF;
                    return;

                default:
                    Error("Unknown character");
                    GetNextChar();
                    break;
                }
            }
 
        }

        // Search for a tag and return it if it exists.  Returns -1 if it isn't found.
        private int FindTag(string p)
        {
            for (int i = 0; i < rchTagNames.Length; i++) {
                if (rchTagNames[i] == p) return i;
            }
            return -1;
        }

        private MHParseNode DoParse()
        {
            MHParseNode pRes = null;
            switch (m_nType) 
            {
            case PTStartSection: // Open curly bracket
                {
                    NextSym();
                    // Should be followed by a tag.
                    if (m_nType != PTTag) Error("Expected ':' after '{'");
                    MHPTagged pTag = new MHPTagged(m_nTag);
                    pRes = pTag;
                    NextSym();
                    while (m_nType != PTEndSection) {
                        pTag.AddArg(DoParse());
                    }
                    NextSym(); // Remove the close curly bracket.
                    break;
                }

            case PTTag: // Tag on its own.
                {
                    int nTag = m_nTag;
                    MHPTagged pTag = new MHPTagged(nTag);
                    pRes = pTag;
                    NextSym();
                    switch (nTag) {
                    case ASN1Codes.C_ITEMS:
                    case ASN1Codes.C_LINK_EFFECT:
                    case ASN1Codes.C_ACTIVATE:
                    case ASN1Codes.C_ADD:
                    case ASN1Codes.C_ADD_ITEM:
                    case ASN1Codes.C_APPEND:
                    case ASN1Codes.C_BRING_TO_FRONT:
                    case ASN1Codes.C_CALL:
                    case ASN1Codes.C_CALL_ACTION_SLOT:
                    case ASN1Codes.C_CLEAR:
                    case ASN1Codes.C_CLONE:
                    case ASN1Codes.C_CLOSE_CONNECTION:
                    case ASN1Codes.C_DEACTIVATE:
                    case ASN1Codes.C_DEL_ITEM:
                    case ASN1Codes.C_DESELECT:
                    case ASN1Codes.C_DESELECT_ITEM:
                    case ASN1Codes.C_DIVIDE:
                    case ASN1Codes.C_DRAW_ARC:
                    case ASN1Codes.C_DRAW_LINE:
                    case ASN1Codes.C_DRAW_OVAL:
                    case ASN1Codes.C_DRAW_POLYGON:
                    case ASN1Codes.C_DRAW_POLYLINE:
                    case ASN1Codes.C_DRAW_RECTANGLE:
                    case ASN1Codes.C_DRAW_SECTOR:
                    case ASN1Codes.C_FORK:
                    case ASN1Codes.C_GET_AVAILABILITY_STATUS:
                    case ASN1Codes.C_GET_BOX_SIZE:
                    case ASN1Codes.C_GET_CELL_ITEM:
                    case ASN1Codes.C_GET_CURSOR_POSITION:
                    case ASN1Codes.C_GET_ENGINE_SUPPORT:
                    case ASN1Codes.C_GET_ENTRY_POINT:
                    case ASN1Codes.C_GET_FILL_COLOUR:
                    case ASN1Codes.C_GET_FIRST_ITEM:
                    case ASN1Codes.C_GET_HIGHLIGHT_STATUS:
                    case ASN1Codes.C_GET_INTERACTION_STATUS:
                    case ASN1Codes.C_GET_ITEM_STATUS:
                    case ASN1Codes.C_GET_LABEL:
                    case ASN1Codes.C_GET_LAST_ANCHOR_FIRED:
                    case ASN1Codes.C_GET_LINE_COLOUR:
                    case ASN1Codes.C_GET_LINE_STYLE:
                    case ASN1Codes.C_GET_LINE_WIDTH:
                    case ASN1Codes.C_GET_LIST_ITEM:
                    case ASN1Codes.C_GET_LIST_SIZE:
                    case ASN1Codes.C_GET_OVERWRITE_MODE:
                    case ASN1Codes.C_GET_PORTION:
                    case ASN1Codes.C_GET_POSITION:
                    case ASN1Codes.C_GET_RUNNING_STATUS:
                    case ASN1Codes.C_GET_SELECTION_STATUS:
                    case ASN1Codes.C_GET_SLIDER_VALUE:
                    case ASN1Codes.C_GET_TEXT_CONTENT:
                    case ASN1Codes.C_GET_TEXT_DATA:
                    case ASN1Codes.C_GET_TOKEN_POSITION:
                    case ASN1Codes.C_GET_VOLUME:
                    case ASN1Codes.C_LAUNCH:
                    case ASN1Codes.C_LOCK_SCREEN:
                    case ASN1Codes.C_MODULO:
                    case ASN1Codes.C_MOVE:
                    case ASN1Codes.C_MOVE_TO:
                    case ASN1Codes.C_MULTIPLY:
                    case ASN1Codes.C_OPEN_CONNECTION:
                    case ASN1Codes.C_PRELOAD:
                    case ASN1Codes.C_PUT_BEFORE:
                    case ASN1Codes.C_PUT_BEHIND:
                    case ASN1Codes.C_QUIT:
                    case ASN1Codes.C_READ_PERSISTENT:
                    case ASN1Codes.C_RUN:
                    case ASN1Codes.C_SCALE_BITMAP:
                    case ASN1Codes.C_SCALE_VIDEO:
                    case ASN1Codes.C_SCROLL_ITEMS:
                    case ASN1Codes.C_SELECT:
                    case ASN1Codes.C_SELECT_ITEM:
                    case ASN1Codes.C_SEND_EVENT:
                    case ASN1Codes.C_SEND_TO_BACK:
                    case ASN1Codes.C_SET_BOX_SIZE:
                    case ASN1Codes.C_SET_CACHE_PRIORITY:
                    case ASN1Codes.C_SET_COUNTER_END_POSITION:
                    case ASN1Codes.C_SET_COUNTER_POSITION:
                    case ASN1Codes.C_SET_COUNTER_TRIGGER:
                    case ASN1Codes.C_SET_CURSOR_POSITION:
                    case ASN1Codes.C_SET_CURSOR_SHAPE:
                    case ASN1Codes.C_SET_DATA:
                    case ASN1Codes.C_SET_ENTRY_POINT:
                    case ASN1Codes.C_SET_FILL_COLOUR:
                    case ASN1Codes.C_SET_FIRST_ITEM:
                    case ASN1Codes.C_SET_FONT_REF:
                    case ASN1Codes.C_SET_HIGHLIGHT_STATUS:
                    case ASN1Codes.C_SET_INTERACTION_STATUS:
                    case ASN1Codes.C_SET_LABEL:
                    case ASN1Codes.C_SET_LINE_COLOUR:
                    case ASN1Codes.C_SET_LINE_STYLE:
                    case ASN1Codes.C_SET_LINE_WIDTH:
                    case ASN1Codes.C_SET_OVERWRITE_MODE:
                    case ASN1Codes.C_SET_PALETTE_REF:
                    case ASN1Codes.C_SET_PORTION:
                    case ASN1Codes.C_SET_POSITION:
                    case ASN1Codes.C_SET_SLIDER_VALUE:
                    case ASN1Codes.C_SET_SPEED:
                    case ASN1Codes.C_SET_TIMER:
                    case ASN1Codes.C_SET_TRANSPARENCY:
                    case ASN1Codes.C_SET_VARIABLE:
                    case ASN1Codes.C_SET_VOLUME:
                    case ASN1Codes.C_SPAWN:
                    case ASN1Codes.C_STEP:
                    case ASN1Codes.C_STOP:
                    case ASN1Codes.C_STORE_PERSISTENT:
                    case ASN1Codes.C_SUBTRACT:
                    case ASN1Codes.C_TEST_VARIABLE:
                    case ASN1Codes.C_TOGGLE:
                    case ASN1Codes.C_TOGGLE_ITEM:
                    case ASN1Codes.C_TRANSITION_TO:
                    case ASN1Codes.C_UNLOAD:
                    case ASN1Codes.C_UNLOCK_SCREEN:
                    case ASN1Codes.C_CONTENT_REFERENCE:
                    case ASN1Codes.C_TOKEN_GROUP_ITEMS:
                    case ASN1Codes.C_POSITIONS:
                    case ASN1Codes.C_MULTIPLEX:
                        {   // These are parenthesised in the text form.  We have to remove the
                            // parentheses otherwise we will return a sequence which will not be
                            // be compatible with the binary form.
                            if (m_nType != PTStartSeq) Error("Expected '('");
                            NextSym();
                            while (m_nType != PTEndSeq) pTag.AddArg(DoParse());
                            NextSym(); // Remove the close parenthesis.
                            break;
                        }
                    case ASN1Codes.C_ORIGINAL_CONTENT:
                    case ASN1Codes.C_NEW_GENERIC_BOOLEAN:
                    case ASN1Codes.C_NEW_GENERIC_INTEGER:
                    case ASN1Codes.C_NEW_GENERIC_OCTETSTRING:
                    case ASN1Codes.C_NEW_GENERIC_OBJECT_REF:
                    case ASN1Codes.C_NEW_GENERIC_CONTENT_REF:
                    case ASN1Codes.C_ORIGINAL_VALUE:
                    case ASN1Codes.C_INDIRECTREFERENCE:
                        // These always have an argument which may be a tagged item.
                        {
                            // Is it always the case that there is at least one argument so if we haven't
                            // had any arguments yet we should always process a tag as an argument?
                            pTag.AddArg(DoParse());
                            break;
                        }
                    default:
                        // This can be followed by an int, etc but a new tag is dealt with by the caller.
                        while (m_nType == PTBool ||m_nType == PTInt || m_nType == PTString || m_nType == PTEnum || m_nType == PTStartSeq)
                        {
                            pTag.AddArg(DoParse());
                        }
                        break;

                    }
                    break;
                }

            case PTInt:
                {
                    pRes = new MHPInt(m_nInt);
                    NextSym();
                    break;
                }

            case PTBool:
                {
                    pRes = new MHPBool(m_fBool);
                    NextSym();
                    break;
                }

            case PTString:
                {
                    MHOctetString str = new MHOctetString();
//                    str.Copy(MHOctetString((const char *)m_String, m_nStringLength));
                    str.Copy(m_String);
                    pRes = new MHPString(str);
                    NextSym();
                    break;
                }

            case PTEnum:
                {
                    pRes = new MHPEnum(m_nInt); 
//                    ASSERT(m_nInt > 0);
                    NextSym();
                    break;
                }

            case PTNull:
                {
                    pRes = new MHPNull();
                    NextSym();
                    break;
                }

            case PTStartSeq: // Open parenthesis.
                {
                    MHParseSequence pSeq = new MHParseSequence();
                    pRes = pSeq;
                    NextSym();
                    while (m_nType != PTEndSeq) pSeq.Append(DoParse());
                    NextSym(); // Remove the close parenthesis.
                    break;
                }

            default:
                Error("Unexpected symbol");
                break;
            }
            return pRes;
        }

        private void Error(string str)
        {
            throw new MHEGException(str + " at line " + m_lineCount);
        }

        DefaultColour[] colourTable =
        {
            new DefaultColour("black", 0, 0, 0, 0),
            new DefaultColour("transparent", 0, 0, 0, 255),
            new DefaultColour("gray", 128,128,128,0),
            new DefaultColour("darkgray", 192,192,192,0),
            new DefaultColour("red", 255,0,  0,  0),
            new DefaultColour("darkred", 128,0,  0,  0),
            new DefaultColour("blue", 0,  0,  255,0),
            new DefaultColour("darkblue", 0,  0,  128,0 ),
            new DefaultColour("green", 0,  255,0,  0 ),
            new DefaultColour("darkgreen", 0,  128,0,  0),
            new DefaultColour("yellow", 255,255,0,  0),
            new DefaultColour("cyan", 0,  255,255,0),
            new DefaultColour("magenta", 255,0,  255,0)            

        };

        string[] rchTagNames =
        {
            ":Application",
            ":Scene",
            ":StdID",
            ":StdVersion",
            ":ObjectInfo",
            ":OnStartUp",
            ":OnCloseDown",
            ":OrigGCPriority",
            ":Items",
            ":ResidentPrg",
            ":RemotePrg",
            ":InterchgPrg",
            ":Palette",
            ":Font",  // Occurs twice.
            ":CursorShape",
            ":BooleanVar",
            ":IntegerVar",
            ":OStringVar",
            ":ObjectRefVar",
            ":ContentRefVar",
            ":Link",
            ":Stream",
            ":Bitmap",
            ":LineArt",
            ":DynamicLineArt",
            ":Rectangle",
            ":Hotspot",
            ":SwitchButton",
            ":PushButton",
            ":Text",
            ":EntryField",
            ":HyperText",
            ":Slider",
            ":TokenGroup",
            ":ListGroup",
            ":OnSpawnCloseDown",
            ":OnRestart",
            "", // Default attributes - encoded as a group in binary
            ":CharacterSet",
            ":BackgroundColour",
            ":TextCHook",
            ":TextColour",
            ":Font",
            ":FontAttributes",
            ":InterchgPrgCHook",
            ":StreamCHook",
            ":BitmapCHook",
            ":LineArtCHook",
            ":ButtonRefColour",
            ":HighlightRefColour",
            ":SliderRefColour",
            ":InputEventReg",
            ":SceneCS",
            ":AspectRatio",
            ":MovingCursor",
            ":NextScenes",
            ":InitiallyActive",
            ":CHook",
            ":OrigContent",
            ":Shared",
            ":ContentSize",
            ":CCPriority",
            "" , // Link condition - always replaced by EventSource, EventType and EventData
            ":LinkEffect",
            ":Name",
            ":InitiallyAvailable",
            ":ProgramConnectionTag",
            ":OrigValue",
            ":ObjectRef",
            ":ContentRef",
            ":MovementTable",
            ":TokenGroupItems",
            ":NoTokenActionSlots",
            ":Positions",
            ":WrapAround",
            ":MultipleSelection",
            ":OrigBoxSize",
            ":OrigPosition",
            ":OrigPaletteRef",
            ":Tiling",
            ":OrigTransparency",
            ":BBBox",
            ":OrigLineWidth",
            ":OrigLineStyle",
            ":OrigRefLineColour",
            ":OrigRefFillColour",
            ":OrigFont",
            ":HJustification",
            ":VJustification",
            ":LineOrientation",
            ":StartCorner",
            ":TextWrapping",
            ":Multiplex",
            ":Storage",
            ":Looping",
            ":Audio",
            ":Video",
            ":RTGraphics",
            ":ComponentTag",
            ":OrigVolume",
            ":Termination",
            ":EngineResp",
            ":Orientation",
            ":MaxValue",
            ":MinValue",
            ":InitialValue",
            ":InitialPortion",
            ":StepSize",
            ":SliderStyle",
            ":InputType",
            ":CharList",
            ":ObscuredInput",
            ":MaxLength",
            ":OrigLabel",
            ":ButtonStyle",
            ":Activate",
            ":Add",
            ":AddItem",
            ":Append",
            ":BringToFront",
            ":Call",
            ":CallActionSlot",
            ":Clear",
            ":Clone",
            ":CloseConnection",
            ":Deactivate",
            ":DelItem",
            ":Deselect",
            ":DeselectItem",
            ":Divide",
            ":DrawArc",
            ":DrawLine",
            ":DrawOval",
            ":DrawPolygon",
            ":DrawPolyline",
            ":DrawRectangle",
            ":DrawSector",
            ":Fork",
            ":GetAvailabilityStatus",
            ":GetBoxSize",
            ":GetCellItem",
            ":GetCursorPosition",
            ":GetEngineSupport",
            ":GetEntryPoint",
            ":GetFillColour",
            ":GetFirstItem",
            ":GetHighlightStatus",
            ":GetInteractionStatus",
            ":GetItemStatus",
            ":GetLabel",
            ":GetLastAnchorFired",
            ":GetLineColour",
            ":GetLineStyle",
            ":GetLineWidth",
            ":GetListItem",
            ":GetListSize",
            ":GetOverwriteMode",
            ":GetPortion",
            ":GetPosition",
            ":GetRunningStatus",
            ":GetSelectionStatus",
            ":GetSliderValue",
            ":GetTextContent",
            ":GetTextData",
            ":GetTokenPosition",
            ":GetVolume",
            ":Launch",
            ":LockScreen",
            ":Modulo",
            ":Move",
            ":MoveTo",
            ":Multiply",
            ":OpenConnection",
            ":Preload",
            ":PutBefore",
            ":PutBehind",
            ":Quit",
            ":ReadPersistent",
            ":Run",
            ":ScaleBitmap",
            ":ScaleVideo",
            ":ScrollItems",
            ":Select",
            ":SelectItem",
            ":SendEvent",
            ":SendToBack",
            ":SetBoxSize",
            ":SetCachePriority",
            ":SetCounterEndPosition",
            ":SetCounterPosition",
            ":SetCounterTrigger",
            ":SetCursorPosition",
            ":SetCursorShape",
            ":SetData",
            ":SetEntryPoint",
            ":SetFillColour",
            ":SetFirstItem",
            ":SetFontRef",
            ":SetHighlightStatus",
            ":SetInteractionStatus",
            ":SetLabel",
            ":SetLineColour",
            ":SetLineStyle",
            ":SetLineWidth",
            ":SetOverwriteMode",
            ":SetPaletteRef",
            ":SetPortion",
            ":SetPosition",
            ":SetSliderValue",
            ":SetSpeed",
            ":SetTimer",
            ":SetTransparency",
            ":SetVariable",
            ":SetVolume",
            ":Spawn",
            ":Step",
            ":Stop",
            ":StorePersistent",
            ":Subtract",
            ":TestVariable",
            ":Toggle",
            ":ToggleItem",
            ":TransitionTo",
            ":Unload",
            ":UnlockScreen",
            ":GBoolean",
            ":GInteger",
            ":GOctetString",
            ":GObjectRef",
            ":GContentRef",
            ":NewColourIndex",
            ":NewAbsoluteColour",
            ":NewFontName",
            ":NewFontRef",
            ":NewContentSize",
            ":NewCCPriority",
            ":IndirectRef",
        /* UK MHEG */
            ":SetBackgroundColour",
            ":SetCellPosition",
            ":SetInputRegister",
            ":SetTextColour",
            ":SetFontAttributes",
            ":SetVideoDecodeOffset",
            ":GetVideoDecodeOffset",
            ":GetFocusPosition",
            ":SetFocusPosition",
            ":SetBitmapDecodeOffset",
            ":GetBitmapDecodeOffset",
            ":SetSliderParameters",
        /* Pseudo-operations.  These are encoded as LinkCondition in binary. */
            ":EventSource",
            ":EventType",
            ":EventData",
            ":ActionSlots"
        };


        public const int PTTag = 0; 
        public const int PTInt = 1; 
        public const int PTString = 2; 
        public const int PTEnum = 3; 
        public const int PTStartSection = 4;
        public const int PTEndSection = 5; 
        public const int PTStartSeq = 6; 
        public const int PTEndSeq = 7; 
        public const int PTNull = 8; 
        public const int PTEOF = 9;
        public const int PTBool = 10;

        public const int MAX_ENUM = 30;
        public const int MAX_TAG_LENGTH = 30;
        
    }

    class DefaultColour
    {
        public string name;
        public int red;
        public int green;
        public int blue;
        public int alpha;

        public DefaultColour(string name, int red, int green, int blue, int alpha)
        {
            this.name = name;
            this.red = red;
            this.green = green;
            this.blue = blue;
            this.alpha = alpha;
        }

    }


}
