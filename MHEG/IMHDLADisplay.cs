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
    /// Encapsulates a Dynamic Line Art object
    /// </summary>
    public interface IMHDLADisplay
    {
        /// <summary>
        /// Draws the Line Art image
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void Draw(int x, int y);

        /// <summary>
        /// Set the box size.  Also clears the drawing. 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void SetSize(int width, int height);

        /// <summary>
        /// Sets the size used to draw the line.
        /// </summary>
        /// <param name="width"></param>
        void SetLineSize(int width);

        /// <summary>
        /// Sets the colour line to draw the lines of the object
        /// </summary>
        /// <param name="colour"></param>
        void SetLineColour(MHRgba colour);

        /// <summary>
        /// Sets the color used to fill the object
        /// </summary>
        /// <param name="colour"></param>
        void SetFillColour(MHRgba colour);
        
        /// <summary>
        /// Clear the drawing
        /// </summary>
        void Clear();

        /// <summary>
        /// Draws a line with the given parameters
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        void DrawLine(int x1, int y1, int x2, int y2);

        /// <summary>
        /// Draws a bordered rectangle with the given parameters
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void DrawBorderedRectangle(int x, int y, int width, int height);

        /// <summary>
        /// Draws an Oval with the given parameters
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void DrawOval(int x, int y, int width, int height);

        /// <summary>
        /// Draws a sector of an Arc with the given parameters
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="start"></param>
        /// <param name="arc"></param>
        /// <param name="isSector"></param>
        void DrawArcSector(int x, int y, int width, int height, int start, int arc, bool isSector);

        /// <summary>
        /// Draws a arbituarypolygon
        /// </summary>
        /// <param name="isFilled"></param>
        /// <param name="points"></param>
        void DrawPoly(bool isFilled, Point[] points);
    }
}
