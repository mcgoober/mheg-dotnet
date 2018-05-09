/* 
 *  MHEG-5 Engine (ISO-13522-5)
 *  Copyright (C) 2008 Jason Leonard
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

namespace MHEG.Ingredients
{
    class MHResidentProgram : MHProgram
    {
        public MHResidentProgram()
        {

        }

        public override string ClassName()
        {
            return "ResidentProgram";
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            Logging.PrintTabs(writer, nTabs); writer.Write("{:ResidentPrg ");
            base.Print(writer, nTabs+1);
            Logging.PrintTabs(writer, nTabs); writer.Write("}\n");
        }

        public override void CallProgram(bool fIsFork, MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            if (!m_fAvailable) Preparation(engine);
            if (m_fRunning) return; // Strictly speaking there should be only one instance of a program running at a time. 
            Activation(engine);
            Logging.Log(Logging.MHLogDetail, "Calling program " + m_Name.Printable());
            try 
            {
                // Run the code.
                if (m_Name.ToString().Equals("GCD")) 
                { 
                    // GetCurrentDate - returns local time.
                    GetCurrentDate(success, args, engine);
                }
                else if (m_Name.ToString().Equals("FDa"))
                {
                    // FormatDate
                    FormatDate(success, args, engine);
                }
                else if (m_Name.ToString().Equals("GDW"))
                { 
                    // GetDayOfWeek - returns the day of week that the date occurred on.
                    GetDayOfWeek(success, args, engine);
                }

                else if (m_Name.ToString().Equals("Rnd"))
                { 
                    // Random
                    Random(success, args, engine);
                }

                else if (m_Name.ToString().Equals("CTC"))
                { 
                    // CastToContentRef
                    CastToContentRef(success, args, engine);
                }

                else if (m_Name.ToString().Equals("CTO"))
                { 
                    // CastToObjectRef
                    CastToObjectRef(success, args, engine);
                }

                else if (m_Name.ToString().Equals("GSL"))
                { 
                    // GetStringLength
                    GetStringLength(success, args, engine);
                }

                else if (m_Name.ToString().Equals("GSS"))
                { 
                    // GetSubString
                    GetSubString(success, args, engine);
                }

                else if (m_Name.ToString().Equals("SSS"))
                { 
                    // SearchSubString
                    SearchSubString(success, args, engine);
                }

                else if (m_Name.ToString().Equals("SES"))
                { 
                    // SearchAndExtractSubString
                    SearchAndExtractSubString(success, args, engine);
                }

                else if (m_Name.ToString().Equals("GSI"))
                { 
                    // SI_GetServiceIndex
                    SI_GetServiceIndex(success, args, engine);
                }

                else if (m_Name.ToString().Equals("TIn"))
                { 
                    // SI_TuneIndex - Fork not allowed
                    SI_TuneIndex(success, args, engine);
                }
                else if (m_Name.ToString().Equals("TII"))
                { 
                    // SI_TuneIndexInfo
                    SI_TuneIndexInfo(success, args, engine);
                }
                else if (m_Name.ToString().Equals("BSI"))
                { 
                    // SI_GetBasicSI
                    SI_GetBasicSI(success, args, engine);
                }
                else if (m_Name.ToString().Equals("GBI"))
                { 
                    // GetBootInfo
                    GetBootInfo(success, args, engine);                    
                }
                else if (m_Name.ToString().Equals("CCR"))
                { 
                    // CheckContentRef
                    CheckContentRef(success, args, engine);
                }
                else if (m_Name.ToString().Equals("CGR"))
                { 
                    // CheckGroupIDRef
                    CheckGroupIDRef(success, args, engine);
                }
                else if (m_Name.ToString().Equals("VTG"))
                { 
                    // VideoToGraphics
                    VideoToGraphics(success, args, engine);
                }
                else if (m_Name.ToString().Equals("SWA"))
                { 
                    // SetWidescreenAlignment
                    SetWidescreenAlignment(success, args, engine);
                }
                else if (m_Name.ToString().Equals("GDA"))
                { 
                    // GetDisplayAspectRatio
                    GetDisplayAspectRatio(success, args, engine);                    
                }
                else if (m_Name.ToString().Equals("CIS"))
                { 
                    // CI_SendMessage
                    CI_SendMessage(success, args, engine);                    
                }
                else if (m_Name.ToString().Equals("SSM"))
                { 
                    // SetSubtitleMode
                    SetSubtitleMode(success, args, engine);                    
                }

                else if (m_Name.ToString().Equals("WAI"))
                { 
                    // WhoAmI
                    WhoAmI(success, args, engine);
                }

                else if (m_Name.ToString().Equals("DBG"))
                { 
                    // Debug - optional
                    Debug(success, args, engine);
                }
                else
                {
                    Logging.Assert(false);
                }
            }
            catch (MHEGException) 
            {
                // If something went wrong set the succeeded flag to false
                SetSuccessFlag(success, false, engine);
                // And continue on.  In particular we need to deactivate.
            }
            Deactivation(engine);
            // At the moment we always treat Fork as Call.  If we do get a Fork we should signal that we're done. 
            if (fIsFork) engine.EventTriggered(this, EventAsyncStopped);

        }

        public virtual void GetCurrentDate(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            if (args.Size == 2)
            {
                DateTime dt = new DateTime(1858, 11, 17);
                DateTime now = DateTime.Now;
                int nModJulianDate = now.Subtract(dt).Days;

                int nTimeAsSecs = (int)now.TimeOfDay.TotalSeconds;

                engine.FindObject(args.GetAt(0).GetReference()).SetVariableValue(new MHUnion(nModJulianDate));
                engine.FindObject(args.GetAt(1).GetReference()).SetVariableValue(new MHUnion(nTimeAsSecs));
                SetSuccessFlag(success, true, engine);
            }
            else
            {
                SetSuccessFlag(success, false, engine);
            }
        }

        public virtual void FormatDate(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            if (args.Size == 4) 
            {
                // This is a bit like strftime but not quite.
                MHOctetString format = new MHOctetString();
                GetString(args.GetAt(0), format, engine);
                int date = GetInt(args.GetAt(1), engine); // As produced in GCD
                int time = GetInt(args.GetAt(2), engine);

                string result = "";

                DateTime dt = new DateTime(1858, 11, 17);
                dt = dt.AddDays(date).AddSeconds(time);

                for (int i = 0; i < format.Size; i++) 
                {
                    char ch = format.GetAt(i);
                    string part; // Largest text is 4 chars for a year + null terminator
                    if (ch == '%') 
                    {
                        i++;
                        if (i == format.Size) break;
                        ch = format.GetAt(i);
                        switch (ch)
                        {
                        case 'Y': part = dt.ToString("yyyy"); break;
                        case 'y': part = dt.ToString("yy"); break;
                        case 'X': part = dt.ToString("MM"); break;
                        case 'x': part = dt.ToString("%M"); break;
                        case 'D': part = dt.ToString("dd"); break;
                        case 'd': part = dt.ToString("%d"); break;
                        case 'H': part = dt.ToString("HH"); break;
                        case 'h': part = dt.ToString("%H"); break;
                        case 'I': part = dt.ToString("hh");break;
                        case 'i': part = dt.ToString("%h");break;
                        case 'M': part = dt.ToString("mm"); break;
                        case 'm': part = dt.ToString("%m"); break;
                        case 'S': part = dt.ToString("ss"); break;
                        case 's': part = dt.ToString("%s"); break;
                            // TODO: These really should be localised.
                        case 'A': part = dt.ToString("tt"); break;
                        case 'a': part = dt.ToString("tt").ToLower(); break;
                        default: part = ""; break;
                        }
                        result += part;
                    }
                    else 
                    {
                        result += ch;
                    }
                }
                MHOctetString theResult = new MHOctetString(result);
                MHParameter pResString = args.GetAt(3);
                engine.FindObject(pResString.GetReference()).SetVariableValue(new MHUnion(theResult));
                SetSuccessFlag(success, true, engine);
            }
            else 
            {
                SetSuccessFlag(success, false, engine);
            }
        }

        public virtual void GetDayOfWeek(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            if (args.Size == 2)
            {
                int date = GetInt(args.GetAt(0), engine); // Date as produced in GCD
                DateTime dt = new DateTime(1858, 11, 17);
                int nDayOfWeek = (int)dt.AddDays(date).DayOfWeek;

                engine.FindObject(args.GetAt(1).GetReference()).SetVariableValue(new MHUnion(nDayOfWeek));
                SetSuccessFlag(success, true, engine);
            }
            else
            {
                SetSuccessFlag(success, false, engine);
            }

        }

        public virtual void Random(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            if (args.Size == 2) 
            {
                int nLimit = GetInt(args.GetAt(0), engine);
                MHParameter pResInt = args.GetAt(1);
                Random randomGenerator = new Random();
                engine.FindObject((pResInt.GetReference())).SetVariableValue(new MHUnion(randomGenerator.Next(nLimit) + 1));
                SetSuccessFlag(success, true, engine);
            }
            else SetSuccessFlag(success, false, engine);
        }

        public virtual void CastToContentRef(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            // Converts a string to a ContentRef.
            if (args.Size == 2) 
            {
                MHOctetString str = new MHOctetString();
                GetString(args.GetAt(0), str, engine);
                MHContentRef result = new MHContentRef();
                result.ContentRef.Copy(str);
                engine.FindObject(args.GetAt(1).GetReference()).SetVariableValue(new MHUnion(result));
                SetSuccessFlag(success, true, engine);
            }
            else 
            {
                SetSuccessFlag(success, false, engine);
            }
        }

        public virtual void CastToObjectRef(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            // Converts a string and an integer to an ObjectRef.
            if (args.Size == 3)
            {
                MHObjectRef result = new MHObjectRef();
                GetString(args.GetAt(0), result.GroupId, engine);
                result.ObjectNo = GetInt(args.GetAt(1), engine);
                engine.FindObject(args.GetAt(2).GetReference()).SetVariableValue(new MHUnion(result));
                SetSuccessFlag(success, true, engine);
            }
            else
            {
                SetSuccessFlag(success, false, engine);
            }
        }

        public virtual void GetStringLength(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            if (args.Size == 2)
            {
                // Find a substring within a string and return an index to the position.
                MHOctetString str = new MHOctetString();
                GetString(args.GetAt(0), str, engine);
                MHParameter pResInt = args.GetAt(1);
                SetSuccessFlag(success, true, engine);
                engine.FindObject(pResInt.GetReference()).SetVariableValue(new MHUnion(str.Size));
            }
            else
            {
                SetSuccessFlag(success, false, engine);
            }
        }

        public virtual void GetSubString(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            if (args.Size == 4)
            {
                // Extract a sub-string from a string.
                MHOctetString str = new MHOctetString();
                GetString(args.GetAt(0), str, engine);
                int nBeginExtract = GetInt(args.GetAt(1), engine);
                int nEndExtract = GetInt(args.GetAt(2), engine);
                if (nBeginExtract < 1) nBeginExtract = 1;
                if (nBeginExtract > str.Size) nBeginExtract = str.Size;
                if (nEndExtract < 1) nEndExtract = 1;
                if (nEndExtract > str.Size) nEndExtract = str.Size;
                MHParameter pResString = args.GetAt(3);
                // Returns beginExtract to endExtract inclusive.
                engine.FindObject(pResString.GetReference()).SetVariableValue(
                    new MHUnion(new MHOctetString(str, nBeginExtract - 1, nEndExtract - nBeginExtract + 1)));
                SetSuccessFlag(success, true, engine);
            }
            else
            {
                SetSuccessFlag(success, false, engine);
            }
        }

        public virtual void SearchSubString(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            if (args.Size == 4)
            {
                // Find a substring within a string and return an index to the position.
                MHOctetString str = new MHOctetString();
                MHOctetString searchString = new MHOctetString();
                GetString(args.GetAt(0), str, engine);
                int nStart = GetInt(args.GetAt(1), engine);
                if (nStart < 1) nStart = 1;
                GetString(args.GetAt(2), searchString, engine);
                // Strings are indexed from one.
                int nPos;
                for (nPos = nStart - 1; nPos <= str.Size - searchString.Size; nPos++)
                {
                    int i;
                    for (i = 0; i < searchString.Size; i++)
                    {
                        if (searchString.GetAt(i) != str.GetAt(i + nPos)) break;
                    }
                    if (i == searchString.Size) break; // Found a match.
                }
                // Set the result.
                MHParameter pResInt = args.GetAt(3);
                SetSuccessFlag(success, true, engine); // Set this first.
                if (nPos <= str.Size - searchString.Size)
                { // Found
                    // Set the index to the position of the string, counting from 1.
                    engine.FindObject(pResInt.GetReference()).SetVariableValue(new MHUnion(nPos + 1));
                }
                else
                { // Not found.  Set the result index to -1
                    engine.FindObject(pResInt.GetReference()).SetVariableValue(new MHUnion(-1));
                }
            }
            else
            {
                SetSuccessFlag(success, false, engine);
            }
        }

        public virtual void SearchAndExtractSubString(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            if (args.Size == 5)
            {
                // Find a substring within a string and return an index to the position
                // and the prefix to the substring.
                MHOctetString str = new MHOctetString();
                MHOctetString searchString = new MHOctetString();
                GetString(args.GetAt(0), str, engine);
                int nStart = GetInt(args.GetAt(1), engine);
                if (nStart < 1) nStart = 1;
                GetString(args.GetAt(2), searchString, engine);
                // Strings are indexed from one.
                int nPos;
                for (nPos = nStart - 1; nPos <= str.Size - searchString.Size; nPos++)
                {
                    int i;
                    for (i = 0; i < searchString.Size; i++)
                    {
                        if (searchString.GetAt(i) != str.GetAt(i + nPos)) break; // Doesn't match
                    }
                    if (i == searchString.Size) break; // Found a match.
                }
                // Set the results.
                MHParameter pResString = args.GetAt(3);
                MHParameter pResInt = args.GetAt(4);
                SetSuccessFlag(success, true, engine); // Set this first.
                if (nPos <= str.Size - searchString.Size)
                {
                    // Found
                    // Set the index to the position AFTER the string, counting from 1.
                    engine.FindObject(pResInt.GetReference()).SetVariableValue(new MHUnion(nPos + 1 + searchString.Size));
                    // Return the sequence from nStart - 1 of length nPos - nStart + 1
                    MHOctetString resultString = new MHOctetString(str, nStart - 1, nPos - nStart + 1);
                    engine.FindObject(pResString.GetReference()).SetVariableValue(new MHUnion(resultString));
                }
                else
                {
                    // Not found.  Set the result string to empty and the result index to -1
                    engine.FindObject(pResInt.GetReference()).SetVariableValue(new MHUnion(-1));
                    engine.FindObject(pResString.GetReference()).SetVariableValue(new MHUnion(new MHOctetString("")));
                }
            }
            else
            {
                SetSuccessFlag(success, false, engine);
            }
        }

        public virtual void SI_GetServiceIndex(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            // Returns an index indicating the service
            if (args.Size == 2) 
            {
                MHOctetString str = new MHOctetString();
                GetString(args.GetAt(0), str, engine);
                MHParameter pResInt = args.GetAt(1);
                // The format of the service is dvb://netID.[transPortID].serviceID
                // where the IDs are in hex.
                // or rec://svc/lcn/N where N is the "logical channel number" i.e. the Freeview channel.
                int nResult = engine.GetContext().GetChannelIndex(str.ToString());
                engine.FindObject(pResInt.GetReference()).SetVariableValue(new MHUnion(nResult));
                Logging.Log(Logging.MHLogDetail, "Get service index for " + str.Printable() + " - result " + nResult);
                SetSuccessFlag(success, true, engine);
            }
            else 
            {
                SetSuccessFlag(success, false, engine);
            }
        }

        public virtual void SI_TuneIndex(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            // Tunes to an index returned by GSI
            if (args.Size == 1)
            {
                int nChannel = GetInt(args.GetAt(0), engine);
                bool res = engine.GetContext().TuneTo(nChannel);
                SetSuccessFlag(success, res, engine);
            }
            else
            {
                SetSuccessFlag(success, false, engine);
            }
        }

        public virtual void SI_TuneIndexInfo(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            // Indicates whether to perform a subsequent TIn quietly or normally. 
            Logging.Assert(false);
        }

        public virtual void SI_GetBasicSI(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            // Returns basic SI information about the service indicated by an index
            // returned by GSI.
            // Returns networkID, origNetworkID, transportStreamID, serviceID
            Logging.Assert(false);
        }

        public virtual void GetBootInfo(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            // Gets the NB_info field.
            Logging.Assert(false);
        }

        public virtual void CheckContentRef(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            // Sees if an item with a particular content reference is available
            // in the carousel.
            Logging.Assert(false);
        }

        public virtual void CheckGroupIDRef(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            // Sees if an application or scene with a particular group id
            // is available in the carousel.
            Logging.Assert(false);
        }

        public virtual void VideoToGraphics(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            // Video to graphics transformation.
            Logging.Assert(false);
        }

        public virtual void SetWidescreenAlignment(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            // Sets either LetterBox or Centre-cut-out mode.
            // Seems to be concerned with aligning a 4:3 scene with an underlying 16:9 video
            Logging.Assert(false);
        }

        public virtual void GetDisplayAspectRatio(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            // Returns the aspcet ratio.  4:3 => 1, 16:9 => 2
            Logging.Assert(false);
        }

        public virtual void CI_SendMessage(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            // Sends a message to a DVB CI application
            Logging.Assert(false);
        }

        public virtual void SetSubtitleMode(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            // Enable or disable subtitles in addition to MHEG.
            Logging.Assert(false);
        }

        public virtual void WhoAmI(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            // Return a concatenation of the strings we respond to in
            // GetEngineSupport(UKEngineProfile(X))

            if (args.Size == 1)
            {
                MHOctetString result = new MHOctetString();
                result.Copy(MHEngine.MHEGEngineProviderIdString);
                result.Append(" ");
                result.Append(engine.GetContext().GetReceiverId());
                result.Append(" ");
                result.Append(engine.GetContext().GetDSMCCId());
                engine.FindObject((args.GetAt(0).GetReference())).SetVariableValue(new MHUnion(result));
                SetSuccessFlag(success, true, engine);
            }
            else SetSuccessFlag(success, false, engine);
        }

        public virtual void Debug(MHObjectRef success, MHSequence<MHParameter> args, MHEngine engine)
        {
            Logging.Assert(false);
        }
        
    }
}
