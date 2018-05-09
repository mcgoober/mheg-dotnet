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
using System.Drawing;

namespace MHEG
{
    /// <summary>
    /// Represents a Color specified by the MHEG engine for use 
    /// by the the context.
    /// </summary>
    public class MHRgba
    {
        private int red;
        private int green;
        private int blue;
        private int alpha;

        /// <summary>
        /// Constructs a representation of black
        /// </summary>
        public MHRgba()
        {
            red = green = blue = alpha = 0;
        }

        /// <summary>
        /// Constructs a specified color
        /// </summary>
        /// <param name="red">Amount of Red</param>
        /// <param name="green">Amount of Green</param>
        /// <param name="blue">Amount of Blue</param>
        /// <param name="alpha">Amount of Alpha</param>
        public MHRgba(int red, int green, int blue, int alpha)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
            this.alpha = alpha;
        }

        /// <summary>
        /// Converts the object to a System.Drawing.Color object
        /// </summary>
        /// <returns>a System.Drawing.Color representation of this object</returns>
        public Color ToColor()
        {
            return Color.FromArgb(Alpha, Red, Green, Blue);
        }

        /// <summary>
        /// Amount of Red
        /// </summary>
        public int Red
        {
            get { return red; }
        }

        /// <summary>
        /// Amount of Green
        /// </summary>
        public int Green
        {
            get { return green; }
        }

        /// <summary>
        /// Amount of Blue
        /// </summary>
        public int Blue
        {
            get { return blue; }
        }

        /// <summary>
        /// Amount of Alpha
        /// </summary>
        public int Alpha
        {
            get { return alpha; }
        }
    }
}
