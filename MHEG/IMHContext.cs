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
    /// Implemented by the application using the MHEG library.
    /// </summary>
    public interface IMHContext
    {
        /// <summary>
        /// Test for an object in the carousel.  Returns true if the object is present and
        /// so a call to GetCarouselData will not block and will return the data.
        /// Returns false if the object is not currently present because it has not
        /// yet appeared and also if it is not present in the containing directory.
        /// </summary>
        /// <param name="objectPath">A DSM-CC path to the object being tested for</param>
        /// <returns>Returns true only if the object exists and is ready</returns>
        bool CheckCarouselObject(string objectPath);

        /// <summary>
        /// Get an object from the carousel.  Returns true and sets the data if
        /// it was able to retrieve the named object.  Blocks if the object seems
        /// to be present but has not yet appeared.  Returns false if the object
        /// cannot be retrieved.
        /// </summary>
        /// <param name="objectPath">A DSM-CC path to the object</param>
        /// <param name="result">A byte array that receives the data</param>
        /// <returns>Returns true if the object exists and has been read successfully</returns>
        bool GetCarouselData(string objectPath, out byte[] result);

        /// <summary>
        /// Set the input register.  
        /// This sets the group of keys that are to be handled by MHEG.  
        /// Calling this method will flush the current key queue.
        /// </summary>
        /// <param name="nReg">The new value of the InputRegister</param>
        void SetInputRegister(int nReg);

        /// <summary>
        /// The specified area of the screen needs to be redrawn
        /// because an event has caused an object to become invalid.
        /// </summary>
        /// <param name="region">The region that needs to be redrawn</param>
        void RequireRedraw(Region region);

        /// <summary>
        /// Creates an object that implements IMHDLADisplay that handles Dynamic Line Art.
        /// </summary>
        /// <param name="isBoxed"></param>
        /// <param name="lineColour">The colour to use for drawing lines</param>
        /// <param name="fillColour">The colour to fill the object</param>
        /// <returns>An object implementing IMHDLADisplay</returns>
        IMHDLADisplay CreateDynamicLineArt(bool isBoxed, MHRgba lineColour, MHRgba fillColour);

        /// <summary>
        /// Creates an object that implements IMHTextDisplay that handles a text box
        /// </summary>
        /// <returns>An object implementing IMHTextDisplay</returns>
        IMHTextDisplay CreateText();
        
        /// <summary>
        /// Creates an object that implements IMHBitmapDisplay that handles a bitmap image
        /// </summary>
        /// <param name="tiled">Whether this bitmap should be tiled</param>
        /// <returns>An object implementing IMHBitmapDisplay</returns>
        IMHBitmapDisplay CreateBitmap(bool tiled);
        
        // Additional drawing functions.
        // Draw a rectangle in the specified colour/transparency.
        /// <summary>
        /// Draw a rectangle on the drawing surface with the given parameters
        /// </summary>
        /// <param name="xPos">Top-left X co-ordinate</param>
        /// <param name="yPos">Top-left Y co-ordinate</param>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="height">Height of the rectangle</param>
        /// <param name="colour">The color to draw and fill the rectangle</param>
        void DrawRect(int xPos, int yPos, int width, int height, MHRgba colour);

        /// <summary>
        /// Draw a video stream
        /// </summary>
        /// <param name="videoRect"></param>
        /// <param name="displayRect"></param>
        void DrawVideo(Rectangle videoRect, Rectangle displayRect);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reg"></param>
        void DrawBackground(Region reg);

        /// <summary>
        /// Tuning.  Get the index corresponding to a given channel.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>The channel index as an integer</returns>
        int GetChannelIndex(string channelName);
        
        /// <summary>
        /// Tune to an index returned by GetChannelIndex
        /// </summary>
        /// <param name="channel">channel to tune to</param>
        /// <returns>true if the channel tune was successful</returns>
        bool TuneTo(int channel);

        /// <summary>
        /// Check whether we have requested a stop.  Returns true and signals
        /// the m_stopped condition if we have.
        /// </summary>
        /// <returns>true if the stop condition has been raised</returns>
        bool CheckStop();

        /// <summary>
        /// Begin playing audio from the specified stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        bool BeginAudio(string stream, int tag);

        /// <summary>
        /// Stop playing audio 
        /// </summary>
        void StopAudio();

        /// <summary>
        /// Begin displaying video from the specified stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        bool BeginVideo(string stream, int tag);

        /// <summary>
        /// Stop displaying video 
        /// </summary>
        void StopVideo();

        /// <summary>
        /// Get the id string for the user implemented receiver
        /// </summary>
        /// <returns>Receiver ID as a string</returns>
        string GetReceiverId();

        /// <summary>
        /// Get the id string for the user implemented DSM-CC decoder
        /// </summary>
        /// <returns>DSM-CC decoder ID as a string</returns>
        string GetDSMCCId();       
    }
}
