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

namespace MHEG.Ingredients
{
    abstract class MHProgram : MHIngredient
    {
        protected MHOctetString m_Name; // Name of the program
        protected bool m_fInitiallyAvailable;
        
        public MHProgram()
        {
            m_Name = new MHOctetString();
            m_fInitiallyAvailable = true; // Default true
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            p.GetNamedArg(ASN1Codes.C_NAME).GetArgN(0).GetStringValue(m_Name); // Program name
            MHParseNode pAvail = p.GetNamedArg(ASN1Codes.C_INITIALLY_AVAILABLE);
            if (pAvail != null) m_fInitiallyAvailable = pAvail.GetArgN(0).GetBoolValue();
            // The MHEG Standard says that InitiallyAvailable is mandatory and should be false.
            // That doesn't seem to be the case in some MHEG programs so we force it here.
            m_fInitiallyActive = false;
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            base.Print(writer, nTabs);
            Logging.PrintTabs(writer, nTabs); 
            writer.Write(":Name "); 
            m_Name.Print(writer, 0); 
            writer.Write("\n");
            if (! m_fInitiallyAvailable) 
            {
                Logging.PrintTabs(writer, nTabs); 
                writer.Write(":InitiallyAvailable false"); 
                writer.Write("\n"); 
            }
        }

        public override bool InitiallyAvailable() 
        { 
            return m_fInitiallyAvailable; 
        }

        public override void Activation(MHEngine engine)
        {
            if (m_fRunning) return;
            base.Activation(engine);
            m_fRunning = true;
            engine.EventTriggered(this, EventIsRunning);
        }

        public override void Deactivation(MHEngine engine)
        {
            if (! m_fRunning) return;
            // TODO: Stop the forked program.
            base.Deactivation(engine);
        }

        // Action - Stop can be used to stop the code.
        public override void Stop(MHEngine engine) 
        {
            Deactivation(engine); 
        }

        protected void SetSuccessFlag(MHObjectRef success, bool result, MHEngine engine)
        {
            engine.FindObject(success).SetVariableValue(new MHUnion(result));
        }

        protected void GetString(MHParameter parm, MHOctetString str, MHEngine engine)
        {
            MHUnion un = new MHUnion();
            un.GetValueFrom(parm, engine);
            un.CheckType(MHUnion.U_String);
            str.Copy(un.String);
        }

        protected int GetInt(MHParameter parm, MHEngine engine)
        {
            MHUnion un = new MHUnion();
            un.GetValueFrom(parm, engine);
            un.CheckType(MHUnion.U_Int);
            return un.Int;
        }
    }
}
