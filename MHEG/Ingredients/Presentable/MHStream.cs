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
    class MHStream : MHPresentable
    {
        protected MHSequence<MHPresentable> m_Multiplex;
        protected int m_nStorage;
        protected int m_nLooping;

        public MHStream()
        {
            m_Multiplex = new MHSequence<MHPresentable>();
            m_nStorage = ST_Stream;
            m_nLooping = 0; // Infinity
        }

        public override string ClassName() 
        {
            return "Stream"; 
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            MHParseNode pMultiplex = p.GetNamedArg(ASN1Codes.C_MULTIPLEX);
            for (int i = 0; i < pMultiplex.GetArgCount(); i++) {
                MHParseNode pItem = pMultiplex.GetArgN(i);
                if (pItem.GetTagNo() == ASN1Codes.C_AUDIO) {
                    MHAudio pAudio = new MHAudio();
                    m_Multiplex.Append(pAudio);
                    pAudio.Initialise(pItem, engine);
                }
                else if (pItem.GetTagNo() == ASN1Codes.C_VIDEO) {
                    MHVideo pVideo = new MHVideo();
                    m_Multiplex.Append(pVideo);
                    pVideo.Initialise(pItem, engine);
                }
                else if (pItem.GetTagNo() == ASN1Codes.C_RTGRAPHICS) {
                    MHRTGraphics pRtGraph = new MHRTGraphics();
                    m_Multiplex.Append(pRtGraph);
                    pRtGraph.Initialise(pItem, engine);
                }
                // Ignore unknown items
            }
            MHParseNode pStorage = p.GetNamedArg(ASN1Codes.C_STORAGE);
            if (pStorage != null) m_nStorage = pStorage.GetArgN(0).GetEnumValue();
            MHParseNode pLooping = p.GetNamedArg(ASN1Codes.C_LOOPING);
            if (pLooping != null) m_nLooping = pLooping.GetArgN(0).GetIntValue();
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs); writer.Write("{:Stream ");
            base.Print(writer, nTabs + 1);
            Logging.PrintTabs(writer, nTabs + 1); writer.Write(":Multiplex (\n");
            for (int i = 0; i < m_Multiplex.Size; i++)
            {
                m_Multiplex.GetAt(i).Print(writer, nTabs + 2);
            }
            Logging.PrintTabs(writer, nTabs + 1); writer.Write(" )\n");
            if (m_nStorage != ST_Stream) 
            {
                Logging.PrintTabs(writer, nTabs + 1); writer.Write(":Storage memory\n"); 
            }
            if (m_nLooping != 0) 
            {
                Logging.PrintTabs(writer, nTabs + 1); writer.Write(":Looping {0}\n", m_nLooping); 
            }
            Logging.PrintTabs(writer, nTabs); writer.Write("}\n");
        }

        public override void Preparation(MHEngine engine)
        {
            if (m_fAvailable) return; // Already prepared
            for (int i = 0; i < m_Multiplex.Size; i++) 
            {
                MHPresentable pItem = m_Multiplex.GetAt(i);
                if (pItem.InitiallyActive()) 
                {
                    pItem.Activation(engine); // N.B.  This will also call Preparation for the components.
                }
            }
            base.Preparation(engine);        
        }

        public override void Destruction(MHEngine engine)
        {
            // Apply Destruction in reverse order.
            for (int j = m_Multiplex.Size; j > 0; j--)
            {
                m_Multiplex.GetAt(j - 1).Destruction(engine);
            }
            base.Destruction(engine);        
        }

        public override void Activation(MHEngine engine)
        {
            if (m_fRunning) return;
            base.Activation(engine);
            // Start playing all active stream components.
            for (int i = 0; i < m_Multiplex.Size; i++)
            {
                m_Multiplex.GetAt(i).BeginPlaying(engine);
            }
            m_fRunning = true;
            engine.EventTriggered(this, EventIsRunning);        
        }

        public override void Deactivation(MHEngine engine)
        {
            if (! m_fRunning) return;
            // Stop playing all active Stream components
            for (int i = 0; i < m_Multiplex.Size; i++)
            {
                m_Multiplex.GetAt(i).StopPlaying(engine);
            }
            base.Deactivation(engine);        
        }

        // The MHEG corrigendum allows SetData to be targeted to a stream so
        // the content ref could change while the stream is playing.
        // Not currently handled.
        public override void ContentPreparation(MHEngine engine)
        {
            engine.EventTriggered(this, EventContentAvailable); // Perhaps test for the streams being available?
            for (int i = 0; i < m_Multiplex.Size; i++)
            {
                m_Multiplex.GetAt(i).SetStreamRef(m_ContentRef);
            }
        }
        
        // Return an object if there is a matching component.
        public override MHRoot FindByObjectNo(int n)
        {
            if (n == m_ObjectIdentifier.ObjectNo) return this;
            for (int i = m_Multiplex.Size; i > 0; i--)
            {
                MHRoot pResult = m_Multiplex.GetAt(i - 1).FindByObjectNo(n);
                if (pResult != null) return pResult;
            }
            return null;
        }
    
        public const int ST_Mem = 1;
        public const int ST_Stream = 2;
    }
}
