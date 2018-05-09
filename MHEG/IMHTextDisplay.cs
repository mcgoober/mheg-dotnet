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
using System.Drawing;
using System.Text;

namespace MHEG
{
    /// <summary>
    /// Encapsulates a Text object
    /// </summary>
    public interface IMHTextDisplay
    {
        /// <summary>
        /// Draw the text onto the display.  x and y give the position of the image
        /// relative to the screen.  rect gives the bounding box for the image, again relative to
        /// the screen.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void Draw(int x, int y);

        /// <summary>
        /// Sets the width and height of the box the text is to be drawn in
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void SetSize(int width, int height);

        /// <summary>
        /// Sets the font styles
        /// </summary>
        /// <param name="size"></param>
        /// <param name="isBold"></param>
        /// <param name="isItalic"></param>
        void SetFont(int size, bool isBold, bool isItalic);
        
        /// <summary>
        /// Get the size of a piece of text.  If maxSize is >= 0 it sets strLen to the number
        /// of characters that will fit in that number of bits.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="strLen"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        Rectangle GetBounds(string str, ref int strLen, int maxSize);
        
        /// <summary>
        /// Clears the text box
        /// </summary>
        void Clear();
        
        /// <summary>
        /// Adds text to the text box at the given location and in the given colour
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="str"></param>
        /// <param name="colour"></param>
        void AddText(int x, int y, String str, MHRgba colour);
    }
}
