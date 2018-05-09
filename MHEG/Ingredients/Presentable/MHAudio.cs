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
    class MHAudio : MHPresentable
    {
        protected int m_nComponentTag;
        protected int m_nOriginalVol;
        protected bool m_fStreamPlaying;
        protected MHContentRef m_streamContentRef;

        public MHAudio()
        {
            m_streamContentRef = new MHContentRef();
            m_nOriginalVol = 0;
            m_fStreamPlaying = false;
        }

        public override string ClassName() 
        {
            return "Audio"; 
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            m_nComponentTag = p.GetNamedArg(ASN1Codes.C_COMPONENT_TAG).GetArgN(0).GetIntValue();
            MHParseNode pOrigVol = p.GetNamedArg(ASN1Codes.C_ORIGINAL_VOLUME);
            if (pOrigVol != null) m_nOriginalVol = pOrigVol.GetIntValue();
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs); writer.Write("{:Audio ");
            base.Print(writer, nTabs+1);
            Logging.PrintTabs(writer, nTabs+1); writer.Write(":ComponentTag {0}\n", m_nComponentTag);
            if (m_nOriginalVol != 0) { Logging.PrintTabs(writer, nTabs+1); writer.Write("OriginalVolume {0} ", m_nOriginalVol); }
            Logging.PrintTabs(writer, nTabs); writer.Write("}\n");
        }

        // Activation for Audio is defined in the corrigendum
        public override void Activation(MHEngine engine)
        {
            if (m_fRunning) return;
            base.Activation(engine);
            // Beginning presentation is started by the Stream object.
            m_fRunning = true;
            engine.EventTriggered(this, EventIsRunning);

            if (m_fStreamPlaying && m_streamContentRef.IsSet())
            {
                string stream = "";
                MHOctetString str = m_streamContentRef.ContentRef;
                if (str.Size != 0) stream = str.ToString();
                engine.GetContext().BeginAudio(stream, m_nComponentTag);
            }
        }

        // Deactivation for Audio is defined in the corrigendum
        public override void Deactivation(MHEngine engine)
        {
            if (! m_fRunning) return;
            m_fRunning = false;

            // Stop presenting the audio
            if (m_fStreamPlaying) engine.GetContext().StopAudio();

            base.Deactivation(engine);
        }

        public override void SetStreamRef(MHContentRef contentRef)
        {
            m_streamContentRef.Copy(contentRef);
        }

        public override void BeginPlaying(MHEngine engine)
        {
            m_fStreamPlaying = true;
            if (m_fRunning && m_streamContentRef.IsSet())
            {
                string stream = "";
                MHOctetString str = m_streamContentRef.ContentRef;
                if (str.Size != 0) stream = str.ToString();
                engine.GetContext().BeginAudio(stream, m_nComponentTag);
            }
        }

        public override void StopPlaying(MHEngine engine)
        {
            m_fStreamPlaying = false;
            if (m_fRunning) engine.GetContext().StopAudio();
        }
    }
}
