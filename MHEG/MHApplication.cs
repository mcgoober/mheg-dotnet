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
using MHEG.Ingredients.Presentable;
using MHEG.Parser;

namespace MHEG.Groups
{
    class MHApplication : MHGroup
    {
        protected MHActionSequence m_OnSpawnCloseDown, m_OnRestart;
        // Default attributes.
        protected int m_nCharSet;
        protected MHColour m_BGColour, m_TextColour, m_ButtonRefColour, m_HighlightRefColour, m_SliderRefColour;
        protected int m_nTextCHook, m_nIPCHook, m_nStrCHook, m_nBitmapCHook, m_nLineArtCHook;
        protected MHFontBody m_Font;
        protected MHOctetString m_FontAttrs;

        // Internal attributes and additional state
        protected int m_nLockCount; // Count for locking the screen
        // Display stack.  Visible items with the lowest item in the stack first.
        // Later items may obscure earlier.
        protected MHSequence<MHVisible> m_DisplayStack;

        protected MHScene m_pCurrentScene;
        protected bool m_fRestarting;
        protected string m_Path; // Path from the root directory to this application.  Either the null string or
                        // a string of the form /a/b/c .

        public MHApplication()
        {
            m_fIsApp = true;
            m_nCharSet = 0;
            m_nTextCHook = 0;
            m_nIPCHook = 0;
            m_nStrCHook = 0;
            m_nBitmapCHook = 0;
            m_nLineArtCHook = 0;

            m_pCurrentScene = null;
            m_nLockCount = 0;
            m_fRestarting = false;
            m_OnSpawnCloseDown = new MHActionSequence();
            m_OnRestart = new MHActionSequence();
            m_BGColour = new MHColour();
            m_TextColour = new MHColour();
            m_ButtonRefColour = new MHColour();
            m_HighlightRefColour = new MHColour();
            m_SliderRefColour = new MHColour();
            m_Font = new MHFontBody();
            m_FontAttrs = new MHOctetString();
            m_DisplayStack = new MHSequence<MHVisible>();
        }

        public MHOctetString FontAttrs
        {
            get { return m_FontAttrs; }
        }

        public int TextCHook
        {
            get {return m_nTextCHook;}
        }

        public int IPCHook
        {
            get {return m_nIPCHook;}
        }

        public int StrCHook
        {
            get {return m_nStrCHook;}
        }

        public int BitmapCHook
        {
            get {return m_nBitmapCHook;}
        }

        public int LineArtCHook
        {
            get {return m_nLineArtCHook;}
        }

        public MHColour BGColour
        {
            get { return m_BGColour; }
        }

        public MHColour TextColour
        {
            get { return m_TextColour; }
        }

        public MHColour ButtonRefColour
        {
            get { return m_ButtonRefColour; }
        }

        public MHColour HighlightRefColour
        {
            get { return m_HighlightRefColour; }
        }

        public MHColour SliderRefColour
        {
            get { return m_SliderRefColour; }
        }

        public int CharSet
        {
            get { return m_nCharSet; }
        }

        public MHSequence<MHVisible> DisplayStack
        {
            get { return m_DisplayStack; }
        }

        public int LockCount
        {
            get { return m_nLockCount; }
            set { m_nLockCount = value; }
        }

        public bool Restarting
        {
            get { return m_fRestarting; }
            set { m_fRestarting = value; }
        }

        public string Path
        {
            get { return m_Path; }
            set { m_Path = value; }
        }

        public MHScene CurrentScene
        {
            get { return m_pCurrentScene; }
            set { m_pCurrentScene = value; }
        }

        public override string ClassName() 
        { 
            return "Application"; 
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            // OnSpawnCloseDown
            MHParseNode pOnSpawn = p.GetNamedArg(ASN1Codes.C_ON_SPAWN_CLOSE_DOWN);
            if (pOnSpawn != null) m_OnSpawnCloseDown.Initialise(pOnSpawn, engine);
            // OnRestart
            MHParseNode pOnRestart = p.GetNamedArg(ASN1Codes.C_ON_RESTART);
            if (pOnRestart != null) m_OnRestart.Initialise(pOnRestart, engine);
            // Default attributes.  These are encoded in a group in binary.
            MHParseNode pDefattrs = p.GetNamedArg(ASN1Codes.C_DEFAULT_ATTRIBUTES);
            // but in the text form they're encoded in the Application block.
            if (pDefattrs == null) pDefattrs = p;
            MHParseNode pCharSet = pDefattrs.GetNamedArg(ASN1Codes.C_CHARACTER_SET);
            if (pCharSet != null) m_nCharSet = pCharSet.GetArgN(0).GetIntValue();
            // Colours
            MHParseNode pBGColour = pDefattrs.GetNamedArg(ASN1Codes.C_BACKGROUND_COLOUR);
            if (pBGColour != null) m_BGColour.Initialise(pBGColour.GetArgN(0), engine);
            MHParseNode pTextColour = pDefattrs.GetNamedArg(ASN1Codes.C_TEXT_COLOUR);
            if (pTextColour != null) m_TextColour.Initialise(pTextColour.GetArgN(0), engine);
            MHParseNode pButtonRefColour = pDefattrs.GetNamedArg(ASN1Codes.C_BUTTON_REF_COLOUR);
            if (pButtonRefColour != null) m_ButtonRefColour.Initialise(pButtonRefColour.GetArgN(0), engine);
            MHParseNode pHighlightRefColour = pDefattrs.GetNamedArg(ASN1Codes.C_HIGHLIGHT_REF_COLOUR);
            if (pHighlightRefColour != null) m_HighlightRefColour.Initialise(pHighlightRefColour.GetArgN(0), engine);
            MHParseNode pSliderRefColour = pDefattrs.GetNamedArg(ASN1Codes.C_SLIDER_REF_COLOUR);
            if (pSliderRefColour != null) m_SliderRefColour.Initialise(pSliderRefColour.GetArgN(0), engine);
            // Content hooks
            MHParseNode pTextCHook = pDefattrs.GetNamedArg(ASN1Codes.C_TEXT_CONTENT_HOOK);
            if (pTextCHook != null) m_nTextCHook = pTextCHook.GetArgN(0).GetIntValue();
            MHParseNode pIPCHook = pDefattrs.GetNamedArg(ASN1Codes.C_IP_CONTENT_HOOK);
            if (pIPCHook != null) m_nIPCHook = pIPCHook.GetArgN(0).GetIntValue();
            MHParseNode pStrCHook = pDefattrs.GetNamedArg(ASN1Codes.C_STREAM_CONTENT_HOOK);
            if (pStrCHook != null) m_nStrCHook = pStrCHook.GetArgN(0).GetIntValue();
            MHParseNode pBitmapCHook = pDefattrs.GetNamedArg(ASN1Codes.C_BITMAP_CONTENT_HOOK);
            if (pBitmapCHook != null) m_nBitmapCHook = pBitmapCHook.GetArgN(0).GetIntValue();
            MHParseNode pLineArtCHook = pDefattrs.GetNamedArg(ASN1Codes.C_LINE_ART_CONTENT_HOOK);
            if (pLineArtCHook != null) m_nLineArtCHook = pLineArtCHook.GetArgN(0).GetIntValue();
            // Font.  This is a little tricky.  There are two attributes both called Font.
            // In the binary notation the font here is encoded as 42 whereas the text form
            // finds the first occurrence of :Font in the table and returns 13.
            MHParseNode pFont = pDefattrs.GetNamedArg(ASN1Codes.C_FONT2);
            if (pFont == null) pFont = pDefattrs.GetNamedArg(ASN1Codes.C_FONT);
            if (pFont != null) m_Font.Initialise(pFont.GetArgN(0), engine);
            // Font attributes.
            MHParseNode pFontAttrs = pDefattrs.GetNamedArg(ASN1Codes.C_FONT_ATTRIBUTES);
            if (pFontAttrs != null) pFontAttrs.GetArgN(0).GetStringValue(m_FontAttrs);
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs);
            writer.Write("{:Application ");
            base.Print(writer, nTabs);
            if (m_OnSpawnCloseDown.Size != 0) {
                Logging.PrintTabs(writer, nTabs+1); writer.Write( ":OnSpawnCloseDown");
                m_OnSpawnCloseDown.Print(writer, nTabs+1); writer.Write( "\n");
            }
            if (m_OnRestart.Size != 0)  {
                Logging.PrintTabs(writer, nTabs+1); writer.Write( ":OnRestart");
                m_OnRestart.Print(writer, nTabs+1); writer.Write( "\n");
            }
            if (m_nCharSet > 0) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":CharacterSet {0}\n", m_nCharSet); }
            if (m_BGColour.IsSet()) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":BackgroundColour "); m_BGColour.Print(writer, nTabs+1); writer.Write( "\n"); }
            if (m_nTextCHook > 0) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":TextCHook {0}\n", m_nTextCHook); }
            if (m_TextColour.IsSet()) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":TextColour "); m_TextColour.Print(writer, nTabs+1); writer.Write( "\n"); }
            if (m_Font.IsSet()) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":Font "); m_Font.Print(writer, nTabs+1); writer.Write( "\n"); }
            if (m_FontAttrs.Size > 0) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":FontAttributes "); m_FontAttrs.Print(writer, nTabs+1); writer.Write( "\n"); }
            if (m_nIPCHook > 0) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":InterchgPrgCHook {0}\n", m_nIPCHook); }
            if (m_nStrCHook > 0) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":StreamCHook {0}\n", m_nStrCHook); }
            if (m_nBitmapCHook > 0) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":BitmapCHook {0}\n", m_nBitmapCHook); }
            if (m_nLineArtCHook > 0) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":LineArtCHook {0}\n", m_nLineArtCHook); }
            if (m_ButtonRefColour.IsSet()) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":ButtonRefColour "); m_ButtonRefColour.Print(writer, nTabs+1); writer.Write( "\n"); }
            if (m_HighlightRefColour.IsSet()) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":HighlightRefColour "); m_HighlightRefColour.Print(writer, nTabs+1); writer.Write( "\n"); }
            if (m_SliderRefColour.IsSet()) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":SliderRefColour "); m_SliderRefColour.Print(writer, nTabs+1); writer.Write( "\n"); }
            writer.Write( "}\n");
        }        
        // The application is "shared".
        public override bool IsShared() 
        { 
            return true; 
        }
        public override void Activation(MHEngine engine)
        {
            if (RunningStatus) return;
            base.Activation(engine);
            if (m_fRestarting) { // Set by Quit
                engine.AddActions(m_OnRestart);
                engine.RunActions();
            }
            engine.EventTriggered(this, EventIsRunning);
        }

        
        // Returns the index on the stack or -1 if it's not there.
        public int FindOnStack(MHRoot pVis)
        {
            for (int i = 0; i < m_DisplayStack.Size; i++)
            {
                if (m_DisplayStack.GetAt(i).Equals(pVis)) return i;
            }
            return -1; // Not there
        }

    }
}
