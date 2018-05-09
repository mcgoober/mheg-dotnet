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

namespace MHEG.Groups
{
    class MHScene : MHGroup
    {
        protected int m_nEventReg;
        protected int m_nSceneCoordX;
        protected int m_nSceneCoordY;
        protected int m_nAspectRatioW;
        protected int m_nAspectRatioH;
        protected bool m_fMovingCursor;
        // We don't use the Next-Scenes info at the moment.
        //  MHSceneSeq m_NextScenes; // Preload info for next scenes.

        public MHScene()
        {
            m_fIsApp = false;
            // TODO: In UK MHEG 1.06 the aspect ratio is optional and if not specified "the
            // scene has no aspect ratio".
            m_nAspectRatioW = 4; m_nAspectRatioH = 3;
            m_fMovingCursor = false;
        }

        public int EventReg
        {
            get { return m_nEventReg; }
        }

        public int SceneCoordX
        {
            get { return m_nSceneCoordX; }
        }

        public int SceneCoordY
        {
            get { return m_nSceneCoordY; }
        }

        public int AspectRatioW
        {
            get { return m_nAspectRatioW; }
        }

        public int AspectRatioH
        {
            get { return m_nAspectRatioH; }
        }

        public bool MovingCursor
        {
            get { return m_fMovingCursor; }
        }

        // Set this up from the parse tree.
        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            // Event register.
            MHParseNode pInputEventReg = p.GetNamedArg(ASN1Codes.C_INPUT_EVENT_REGISTER);
            m_nEventReg = pInputEventReg.GetArgN(0).GetIntValue();
            // Co-ordinate system
            MHParseNode pSceneCoords = p.GetNamedArg(ASN1Codes.C_SCENE_COORDINATE_SYSTEM);
            m_nSceneCoordX = pSceneCoords.GetArgN(0).GetIntValue();
            m_nSceneCoordY = pSceneCoords.GetArgN(1).GetIntValue();
            // Aspect ratio
            MHParseNode pAspectRatio = p.GetNamedArg(ASN1Codes.C_ASPECT_RATIO);
            if (pAspectRatio != null) {
                // Is the binary encoded as a sequence or a pair of arguments?
                m_nAspectRatioW = pAspectRatio.GetArgN(0).GetIntValue();
                m_nAspectRatioH = pAspectRatio.GetArgN(1).GetIntValue();
            }
            // Moving cursor
            MHParseNode pMovingCursor = p.GetNamedArg(ASN1Codes.C_MOVING_CURSOR);
            if (pMovingCursor != null) pMovingCursor.GetArgN(0).GetBoolValue();
            // Next scene sequence.
            MHParseNode pNextScenes = p.GetNamedArg(ASN1Codes.C_NEXT_SCENES);
            if (pNextScenes != null) 
            {
                // TODO:
                Logging.Assert(false); 
            } 
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs);
            writer.Write( "{:Scene ");
            base.Print(writer, nTabs);
            Logging.PrintTabs(writer, nTabs+1); writer.Write( ":InputEventReg {0}\n", m_nEventReg);
            Logging.PrintTabs(writer, nTabs+1); writer.Write( ":SceneCS {0} {1}\n", m_nSceneCoordX, m_nSceneCoordY);
            if (m_nAspectRatioW != 4 || m_nAspectRatioH != 3) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":AspectRatio {0} {1}\n", m_nAspectRatioW, m_nAspectRatioH); }
            if (m_fMovingCursor) { Logging.PrintTabs(writer, nTabs+1); writer.Write( ":MovingCursor true\n"); }
            writer.Write( "}\n");
        }

        public override void Activation(MHEngine engine)
        {
            if (RunningStatus) return;
            base.Activation(engine);
            engine.EventTriggered(this, EventIsRunning);                
        }

        // Actions.
        public override void SetInputRegister(int nReg, MHEngine engine)
        {
            m_nEventReg = nReg;
            engine.SetInputRegister(nReg);
        }


        public override string ClassName() 
        { 
            return "Scene"; 
        }

    }
}
