/* 
 *  Copyright (C) 2007 Jason Leonard
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

namespace MHEG
{
    public class Logging
    {
        public static TextWriter tw;
        public static bool bCanClose;
        public static int nLevel;

        public static void Log(int level, string message)
        {
            if ((nLevel & level) != 0)
            {
                tw.WriteLine(message);
            }
        }

        public static void Assert(bool test)
        {
            if (!test)
            {
                tw.WriteLine("Assertion Failure");
                tw.WriteLine(Environment.StackTrace);
            }
        }

        public static void Initialise(string filename)
        {
            tw = new StreamWriter(filename);
            bCanClose = true;
        }

        public static void Initialise()
        {
            bCanClose = false;
            tw = System.Console.Out;
        }

        public const int MHLogError = 1;         // Log errors - these are errors that need to be reported to the user.
        public const int MHLogWarning = 2;       // Log warnings - typically bad MHEG which might be an error in this program
        public const int MHLogNotifications = 4; // General things to log.
        public const int MHLogScenes = 8;        // Print each application and scene
        public const int MHLogActions = 16;      // Print each action before it is run.
        public const int MHLogLinks = 32;        // Print each link when it is fired and each event as it is queued
        public const int MHLogDetail = 64;        // Detailed evaluation of each action.

        public const int MHLogAll = (MHLogError|MHLogWarning|MHLogNotifications|MHLogScenes|MHLogActions|MHLogLinks|MHLogDetail);
    
        public static void Close()
        {
            if (bCanClose) tw.Close();
        }

        public static void PrintTabs(TextWriter writer, int nTabs)
        {
            for (int i = 0; i < nTabs; i++) writer.Write("    ");
        }

        public static void SetLoggingLevel(int level)
        {
            nLevel = level;
        }

        public static int GetLoggingLevel()
        {
            return nLevel;
        }
        
        public static TextWriter GetLoggingStream()
        {
            return tw;
        }
    }
}
