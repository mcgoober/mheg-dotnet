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

namespace MHEG.Ingredients
{
    abstract class MHVariable : MHIngredient
    {
        public MHVariable()
        {

        }

        public override void Activation(MHEngine engine)
        {
            if (RunningStatus) return;
            base.Activation(engine);
            m_fRunning = true;
            engine.EventTriggered(this, EventIsRunning);
        }

        protected string TestToString(int tc)
        {
            switch (tc) 
            {
                case TC_Equal: return "Equal";
                case TC_NotEqual: return "NotEqual";
                case TC_Less: return "Less";
                case TC_LessOrEqual: return "LessOrEqual";
                case TC_Greater: return "Greater";
                case TC_GreaterOrEqual: return "GreaterOrEqual";
            }
            return null; // To keep the compiler happy
        }

        public const int TC_Equal = 1;
        public const int TC_NotEqual = 2;
        public const int TC_Less = 3;
        public const int TC_LessOrEqual = 4;
        public const int TC_Greater = 5;
        public const int TC_GreaterOrEqual = 6;

    }
}
