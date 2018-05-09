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

namespace MHEG.Actions
{
    class MHSetTimer : MHElemAction
    {
        protected MHGenericInteger m_TimerId;
        // A new timer may not be specified in which case this cancels the timer.
        // If the timer is specified the "absolute" flag is optional.
        protected int m_TimerType;
        protected MHGenericInteger m_TimerValue;
        protected MHGenericBoolean m_AbsFlag;

        public MHSetTimer()
            : base(":SetTimer")
        {
            m_TimerId = new MHGenericInteger();
            m_TimerType = ST_NoNewTimer;
            m_TimerValue = new MHGenericInteger();
            m_AbsFlag = new MHGenericBoolean();
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            m_TimerId.Initialise(p.GetArgN(1), engine); // The timer id
            if (p.GetArgCount() > 2) {
                MHParseNode pNewTimer = p.GetArgN(2);
                m_TimerValue.Initialise(pNewTimer.GetSeqN(0), engine);
                if (pNewTimer.GetSeqCount() > 1) 
                {
                    m_TimerType = ST_TimerAbsolute; // May be absolute - depends on the value.
                    m_AbsFlag.Initialise(pNewTimer.GetSeqN(1), engine);
                }
                else m_TimerType = ST_TimerRelative;
            }
        }

        public override void Perform(MHEngine engine)
        {
            int nTimerId = m_TimerId.GetValue(engine);
            bool fAbsolute = false; // Defaults to relative time.
            int newTime = -1;
            switch (m_TimerType)
            {
                case ST_NoNewTimer:
                    fAbsolute = true; // We treat an absolute time of -1 as "cancel"
                    newTime = -1; 
                    break; 
                case ST_TimerAbsolute: 
                    fAbsolute = m_AbsFlag.GetValue(engine);
                    newTime = m_TimerValue.GetValue(engine);
                    break;
                case ST_TimerRelative: 
                    newTime = m_TimerValue.GetValue(engine);
                    break;
            }
            Target(engine).SetTimer(nTimerId, fAbsolute, newTime, engine);
        }

        protected override void PrintArgs(TextWriter writer, int nTabs)
        {
            m_TimerId.Print(writer, 0);
            if (m_TimerType != ST_NoNewTimer)
            {
                writer.Write("( ");
                m_TimerValue.Print(writer, 0);
                if (m_TimerType == ST_TimerAbsolute) m_AbsFlag.Print(writer, 0);
                writer.Write(") ");
            }
        }
        
        public const int ST_NoNewTimer = 0;
        public const int ST_TimerAbsolute = 1;
        public const int ST_TimerRelative = 2;
    }
}
