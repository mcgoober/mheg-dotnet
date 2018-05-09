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
    class ASN1Codes
    {
        // Universal codes _ standard across all ASN1 definitions
        public const int U_BOOL     = 1;
        public const int U_INT      = 2;
        public const int U_STRING   = 4;
        public const int U_NULL     = 5;
        public const int U_ENUM     = 10;
        public const int U_SEQUENCE = 16;

        // Context_specific codes _ defined for MHEG 5
        public const int C_APPLICATION               =0  ;
        public const int C_SCENE                     =1  ;
        public const int C_STANDARD_IDENTIFIER       =2  ;
        public const int C_STANDARD_VERSION          =3  ;
        public const int C_OBJECT_INFORMATION        =4  ;
        public const int C_ON_START_UP               =5  ;
        public const int C_ON_CLOSE_DOWN             =6  ;
        public const int C_ORIGINAL_GC_PRIORITY      =7  ;
        public const int C_ITEMS                     =8  ;
        public const int C_RESIDENT_PROGRAM          =9  ;
        public const int C_REMOTE_PROGRAM            =10 ;
        public const int C_INTERCHANGED_PROGRAM      =11 ;
        public const int C_PALETTE                   =12 ;
        public const int C_FONT                      =13 ;
        public const int C_CURSOR_SHAPE              =14 ;
        public const int C_BOOLEAN_VARIABLE          =15 ;
        public const int C_INTEGER_VARIABLE          =16 ;
        public const int C_OCTET_STRING_VARIABLE     =17 ;
        public const int C_OBJECT_REF_VARIABLE       =18 ;
        public const int C_CONTENT_REF_VARIABLE      =19 ;
        public const int C_LINK                      =20 ;
        public const int C_STREAM                    =21 ;
        public const int C_BITMAP                    =22 ;
        public const int C_LINE_ART                  =23 ;
        public const int C_DYNAMIC_LINE_ART          =24 ;
        public const int C_RECTANGLE                 =25 ;
        public const int C_HOTSPOT                   =26 ;
        public const int C_SWITCH_BUTTON             =27 ;
        public const int C_PUSH_BUTTON               =28 ;
        public const int C_TEXT                      =29 ;
        public const int C_ENTRY_FIELD               =30 ;
        public const int C_HYPER_TEXT                =31 ;
        public const int C_SLIDER                    =32 ;
        public const int C_TOKEN_GROUP               =33 ;
        public const int C_LIST_GROUP                =34 ;
        public const int C_ON_SPAWN_CLOSE_DOWN       =35 ;
        public const int C_ON_RESTART                =36 ;
        public const int C_DEFAULT_ATTRIBUTES        =37 ;
        public const int C_CHARACTER_SET             =38 ;
        public const int C_BACKGROUND_COLOUR         =39 ;
        public const int C_TEXT_CONTENT_HOOK         =40 ;
        public const int C_TEXT_COLOUR               =41 ;
        public const int C_FONT2                     =42 ;
        public const int C_FONT_ATTRIBUTES           =43 ;
        public const int C_IP_CONTENT_HOOK           =44 ;
        public const int C_STREAM_CONTENT_HOOK       =45 ;
        public const int C_BITMAP_CONTENT_HOOK       =46 ;
        public const int C_LINE_ART_CONTENT_HOOK     =47 ;
        public const int C_BUTTON_REF_COLOUR         =48 ;
        public const int C_HIGHLIGHT_REF_COLOUR      =49 ;
        public const int C_SLIDER_REF_COLOUR         =50 ;
        public const int C_INPUT_EVENT_REGISTER      =51 ;
        public const int C_SCENE_COORDINATE_SYSTEM   =52 ;
        public const int C_ASPECT_RATIO              =53 ;
        public const int C_MOVING_CURSOR             =54 ;
        public const int C_NEXT_SCENES               =55 ;
        public const int C_INITIALLY_ACTIVE          =56 ;
        public const int C_CONTENT_HOOK              =57 ;
        public const int C_ORIGINAL_CONTENT          =58 ;
        public const int C_SHARED                    =59 ;
        public const int C_CONTENT_SIZE              =60 ;
        public const int C_CONTENT_CACHE_PRIORITY    =61 ;
        public const int C_LINK_CONDITION            =62 ;
        public const int C_LINK_EFFECT               =63 ;
        public const int C_NAME                      =64 ;
        public const int C_INITIALLY_AVAILABLE       =65 ;
        public const int C_PROGRAM_CONNECTION_TAG    =66 ;
        public const int C_ORIGINAL_VALUE            =67 ;
        public const int C_OBJECT_REFERENCE          =68 ;
        public const int C_CONTENT_REFERENCE         =69 ;
        public const int C_MOVEMENT_TABLE            =70 ;
        public const int C_TOKEN_GROUP_ITEMS         =71 ;
        public const int C_NO_TOKEN_ACTION_SLOTS     =72 ;
        public const int C_POSITIONS                 =73 ;
        public const int C_WRAP_AROUND               =74 ;
        public const int C_MULTIPLE_SELECTION        =75 ;
        public const int C_ORIGINAL_BOX_SIZE         =76 ;
        public const int C_ORIGINAL_POSITION         =77 ;
        public const int C_ORIGINAL_PALETTE_REF      =78 ;
        public const int C_TILING                    =79 ;
        public const int C_ORIGINAL_TRANSPARENCY     =80 ;
        public const int C_BORDERED_BOUNDING_BOX     =81 ;
        public const int C_ORIGINAL_LINE_WIDTH       =82 ;
        public const int C_ORIGINAL_LINE_STYLE       =83 ;
        public const int C_ORIGINAL_REF_LINE_COLOUR  =84 ;
        public const int C_ORIGINAL_REF_FILL_COLOUR  =85 ;
        public const int C_ORIGINAL_FONT             =86 ;
        public const int C_HORIZONTAL_JUSTIFICATION  =87 ;
        public const int C_VERTICAL_JUSTIFICATION    =88 ;
        public const int C_LINE_ORIENTATION          =89 ;
        public const int C_START_CORNER              =90 ;
        public const int C_TEXT_WRAPPING             =91 ;
        public const int C_MULTIPLEX                 =92 ;
        public const int C_STORAGE                   =93 ;
        public const int C_LOOPING                   =94 ;
        public const int C_AUDIO                     =95 ;
        public const int C_VIDEO                     =96 ;
        public const int C_RTGRAPHICS                =97 ;
        public const int C_COMPONENT_TAG             =98 ;
        public const int C_ORIGINAL_VOLUME           =99 ;
        public const int C_TERMINATION               =100;
        public const int C_ENGINE_RESP               =101;
        public const int C_ORIENTATION               =102;
        public const int C_MAX_VALUE                 =103;
        public const int C_MIN_VALUE                 =104;
        public const int C_INITIAL_VALUE             =105;
        public const int C_INITIAL_PORTION           =106;
        public const int C_STEP_SIZE                 =107;
        public const int C_SLIDER_STYLE              =108;
        public const int C_INPUT_TYPE                =109;
        public const int C_CHAR_LIST                 =110;
        public const int C_OBSCURED_INPUT            =111;
        public const int C_MAX_LENGTH                =112;
        public const int C_ORIGINAL_LABEL            =113;
        public const int C_BUTTON_STYLE              =114;
        public const int C_ACTIVATE                  =115;
        public const int C_ADD                       =116;
        public const int C_ADD_ITEM                  =117;
        public const int C_APPEND                    =118;
        public const int C_BRING_TO_FRONT            =119;
        public const int C_CALL                      =120;
        public const int C_CALL_ACTION_SLOT          =121;
        public const int C_CLEAR                     =122;
        public const int C_CLONE                     =123;
        public const int C_CLOSE_CONNECTION          =124;
        public const int C_DEACTIVATE                =125;
        public const int C_DEL_ITEM                  =126;
        public const int C_DESELECT                  =127;
        public const int C_DESELECT_ITEM             =128;
        public const int C_DIVIDE                    =129;
        public const int C_DRAW_ARC                  =130;
        public const int C_DRAW_LINE                 =131;
        public const int C_DRAW_OVAL                 =132;
        public const int C_DRAW_POLYGON              =133;
        public const int C_DRAW_POLYLINE             =134;
        public const int C_DRAW_RECTANGLE            =135;
        public const int C_DRAW_SECTOR               =136;
        public const int C_FORK                      =137;
        public const int C_GET_AVAILABILITY_STATUS   =138;
        public const int C_GET_BOX_SIZE              =139;
        public const int C_GET_CELL_ITEM             =140;
        public const int C_GET_CURSOR_POSITION       =141;
        public const int C_GET_ENGINE_SUPPORT        =142;
        public const int C_GET_ENTRY_POINT           =143;
        public const int C_GET_FILL_COLOUR           =144;
        public const int C_GET_FIRST_ITEM            =145;
        public const int C_GET_HIGHLIGHT_STATUS      =146;
        public const int C_GET_INTERACTION_STATUS    =147;
        public const int C_GET_ITEM_STATUS           =148;
        public const int C_GET_LABEL                 =149;
        public const int C_GET_LAST_ANCHOR_FIRED     =150;
        public const int C_GET_LINE_COLOUR           =151;
        public const int C_GET_LINE_STYLE            =152;
        public const int C_GET_LINE_WIDTH            =153;
        public const int C_GET_LIST_ITEM             =154;
        public const int C_GET_LIST_SIZE             =155;
        public const int C_GET_OVERWRITE_MODE        =156;
        public const int C_GET_PORTION               =157;
        public const int C_GET_POSITION              =158;
        public const int C_GET_RUNNING_STATUS        =159;
        public const int C_GET_SELECTION_STATUS      =160;
        public const int C_GET_SLIDER_VALUE          =161;
        public const int C_GET_TEXT_CONTENT          =162;
        public const int C_GET_TEXT_DATA             =163;
        public const int C_GET_TOKEN_POSITION        =164;
        public const int C_GET_VOLUME                =165;
        public const int C_LAUNCH                    =166;
        public const int C_LOCK_SCREEN               =167;
        public const int C_MODULO                    =168;
        public const int C_MOVE                      =169;
        public const int C_MOVE_TO                   =170;
        public const int C_MULTIPLY                  =171;
        public const int C_OPEN_CONNECTION           =172;
        public const int C_PRELOAD                   =173;
        public const int C_PUT_BEFORE                =174;
        public const int C_PUT_BEHIND                =175;
        public const int C_QUIT                      =176;
        public const int C_READ_PERSISTENT           =177;
        public const int C_RUN                       =178;
        public const int C_SCALE_BITMAP              =179;
        public const int C_SCALE_VIDEO               =180;
        public const int C_SCROLL_ITEMS              =181;
        public const int C_SELECT                    =182;
        public const int C_SELECT_ITEM               =183;
        public const int C_SEND_EVENT                =184;
        public const int C_SEND_TO_BACK              =185;
        public const int C_SET_BOX_SIZE              =186;
        public const int C_SET_CACHE_PRIORITY        =187;
        public const int C_SET_COUNTER_END_POSITION  =188;
        public const int C_SET_COUNTER_POSITION      =189;
        public const int C_SET_COUNTER_TRIGGER       =190;
        public const int C_SET_CURSOR_POSITION       =191;
        public const int C_SET_CURSOR_SHAPE          =192;
        public const int C_SET_DATA                  =193;
        public const int C_SET_ENTRY_POINT           =194;
        public const int C_SET_FILL_COLOUR           =195;
        public const int C_SET_FIRST_ITEM            =196;
        public const int C_SET_FONT_REF              =197;
        public const int C_SET_HIGHLIGHT_STATUS      =198;
        public const int C_SET_INTERACTION_STATUS    =199;
        public const int C_SET_LABEL                 =200;
        public const int C_SET_LINE_COLOUR           =201;
        public const int C_SET_LINE_STYLE            =202;
        public const int C_SET_LINE_WIDTH            =203;
        public const int C_SET_OVERWRITE_MODE        =204;
        public const int C_SET_PALETTE_REF           =205;
        public const int C_SET_PORTION               =206;
        public const int C_SET_POSITION              =207;
        public const int C_SET_SLIDER_VALUE          =208;
        public const int C_SET_SPEED                 =209;
        public const int C_SET_TIMER                 =210;
        public const int C_SET_TRANSPARENCY          =211;
        public const int C_SET_VARIABLE              =212;
        public const int C_SET_VOLUME                =213;
        public const int C_SPAWN                     =214;
        public const int C_STEP                      =215;
        public const int C_STOP                      =216;
        public const int C_STORE_PERSISTENT          =217;
        public const int C_SUBTRACT                  =218;
        public const int C_TEST_VARIABLE             =219;
        public const int C_TOGGLE                    =220;
        public const int C_TOGGLE_ITEM               =221;
        public const int C_TRANSITION_TO             =222;
        public const int C_UNLOAD                    =223;
        public const int C_UNLOCK_SCREEN             =224;
        public const int C_NEW_GENERIC_BOOLEAN       =225;
        public const int C_NEW_GENERIC_INTEGER       =226;
        public const int C_NEW_GENERIC_OCTETSTRING   =227;
        public const int C_NEW_GENERIC_OBJECT_REF    =228;
        public const int C_NEW_GENERIC_CONTENT_REF   =229;
        public const int C_NEW_COLOUR_INDEX          =230;
        public const int C_NEW_ABSOLUTE_COLOUR       =231;
        public const int C_NEW_FONT_NAME             =232;
        public const int C_NEW_FONT_REFERENCE        =233;
        public const int C_NEW_CONTENT_SIZE          =234;
        public const int C_NEW_CONTENT_CACHE_PRIO    =235;
        public const int C_INDIRECTREFERENCE         =236;
        /* UK MHEG */                                    
        public const int C_SET_BACKGROUND_COLOUR     =237;
        public const int C_SET_CELL_POSITION         =238;
        public const int C_SET_INPUT_REGISTER        =239;
        public const int C_SET_TEXT_COLOUR           =240;
        public const int C_SET_FONT_ATTRIBUTES       =241;
        public const int C_SET_VIDEO_DECODE_OFFSET   =242;
        public const int C_GET_VIDEO_DECODE_OFFSET   =243;
        public const int C_GET_FOCUS_POSITION        =244;
        public const int C_SET_FOCUS_POSITION        =245;
        public const int C_SET_BITMAP_DECODE_OFFSET  =246;
        public const int C_GET_BITMAP_DECODE_OFFSET  =247;
        public const int C_SET_SLIDER_PARAMETERS     =248;
                                                         
        // Pseudo-codes.  These are encoded into the link condition in binary but it's convenient
        // to give them codes here since that way we can include them in the same lookup table.
        public const int P_EVENT_SOURCE              =249;
        public const int P_EVENT_TYPE                =250;
        public const int P_EVENT_DATA                =251;
        // The :ActionSlots tag appears in the textual form but not in binary.
        public const int P_ACTION_SLOTS              =252;
                                                         

    }
}
