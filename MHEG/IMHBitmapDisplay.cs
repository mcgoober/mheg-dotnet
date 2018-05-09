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
    /// Encapsulates a bitmap object that the MHEG engine can manipulate.
    /// </summary>
    public interface IMHBitmapDisplay
    {
        /// <summary>
        /// Draw the completed drawing onto the display.  x and y give the position of the image
        /// relative to the screen.  rect gives the bounding box for the image, again relative to
        /// the screen.
        /// </summary>
        /// <param name="x">Top-left X co-ordinate</param>
        /// <param name="y">Top-left Y co-ordinate</param>
        /// <param name="rect">Bounding box for the image</param>
        /// <param name="tiled">Tile the image</param>
        void Draw(int x, int y, Rectangle rect, bool tiled);
        
        /// <summary>
        /// Creates an image from the given PNG data. 
        /// </summary>
        /// <param name="data"></param>
        void CreateFromPNG(byte[] data);
        
        /// <summary>
        /// Creates an image from the given MPEG I-frame data
        /// </summary>
        /// <param name="data"></param>
        void CreateFromMPEG(byte[] data);

        /// <summary>
        /// Scale the bitmap.  Only used for image derived from MPEG I-frames. 
        /// </summary>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        void ScaleImage(int newWidth, int newHeight);
        
        /// <summary>
        /// Calculates the dimensions of the image. 
        /// </summary>
        /// <returns>A Size structure filled with the dimensions of the image</returns>
        Size GetSize();

        /// <summary>
        /// Determines if the visible area is fully opaque
        /// </summary>
        /// <returns>true if the visiable area is fully opaque</returns>
        bool IsOpaque();
    }
}
