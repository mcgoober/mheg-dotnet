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
    abstract class MHIngredient : MHRoot
    {
        protected bool m_fInitiallyActive;
        protected int m_nContentHook;
        protected bool m_fShared;
        // Original content.  The original included content and the other fields are
        // mutually exclusive.
        protected MHOctetString m_OrigIncludedContent;
        protected MHContentRef m_OrigContentRef;
        protected int m_nOrigContentSize;
        protected int m_nOrigCCPrio;
        // Internal attributes
        protected MHOctetString m_IncludedContent;
        protected MHContentRef m_ContentRef;
        protected int m_nContentSize;
        protected int m_nCCPrio;
        protected int m_ContentType;

        public MHIngredient()
        {
            m_fInitiallyActive = true; // Default is true
            m_nContentHook = 0; // Need to choose a value that isn't otherwise used
            m_fShared = false;
            m_nOrigContentSize = 0;
            m_nOrigCCPrio = 127; // Default.
            m_ContentType = IN_NoContent;
            m_OrigIncludedContent = new MHOctetString();
            m_OrigContentRef = new MHContentRef();
            m_IncludedContent = new MHOctetString();
            m_ContentRef = new MHContentRef();
        }

        public MHContentRef ContentRef
        {
            get { return m_ContentRef; }
        }

        public MHIngredient(MHIngredient reference)
        {
            // Don't copy the object reference since that's set separately.
            m_fInitiallyActive = reference.m_fInitiallyActive;
            m_nContentHook = reference.m_nContentHook;
            m_ContentType = reference.m_ContentType;
            m_OrigIncludedContent.Copy(reference.m_OrigIncludedContent);
            m_OrigContentRef.Copy(reference.m_OrigContentRef);
            m_nOrigContentSize = reference.m_nOrigContentSize;
            m_nOrigCCPrio = reference.m_nOrigCCPrio;
            m_fShared = reference.m_fShared;

        }

        // Set this up from the parse tree.
        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            Logging.Assert(m_ObjectIdentifier.ObjectNo > 0);
            MHParseNode pIA = p.GetNamedArg(ASN1Codes.C_INITIALLY_ACTIVE);
            if (pIA != null) m_fInitiallyActive = pIA.GetArgN(0).GetBoolValue();

            MHParseNode pCHook = p.GetNamedArg(ASN1Codes.C_CONTENT_HOOK);
            if (pCHook != null) m_nContentHook = pCHook.GetArgN(0).GetIntValue();

            MHParseNode pOrigContent = p.GetNamedArg(ASN1Codes.C_ORIGINAL_CONTENT);
            if (pOrigContent != null) 
            {
                MHParseNode pArg = pOrigContent.GetArgN(0);
                // Either a string - included content.
                if (pArg.NodeType == MHParseNode.PNString) 
                {
                    m_ContentType = IN_IncludedContent;
                    pArg.GetStringValue(m_OrigIncludedContent);
                }
                else 
                { // or a sequence - referenced content.
                    // In the text version this is tagged with :ContentRef
                    m_ContentType = IN_ReferencedContent;
                    m_OrigContentRef.Initialise(pArg.GetArgN(0), engine); 
                    MHParseNode pContentSize = pArg.GetNamedArg(ASN1Codes.C_CONTENT_SIZE);
                    if (pContentSize != null) m_nOrigContentSize = pContentSize.GetArgN(0).GetIntValue();
                    MHParseNode pCCPrio = pArg.GetNamedArg(ASN1Codes.C_CONTENT_CACHE_PRIORITY);
                    if (pCCPrio != null) m_nOrigCCPrio = pCCPrio.GetArgN(0).GetIntValue();
                }
            }

            MHParseNode pShared = p.GetNamedArg(ASN1Codes.C_SHARED);
            if (pShared != null) m_fShared = pShared.GetArgN(0).GetBoolValue();
        }

        public override void Print(TextWriter writer, int nTabs)
        {
            base.Print(writer, nTabs);
            if (!m_fInitiallyActive) { Logging.PrintTabs(writer, nTabs); writer.Write( ":InitiallyActive false\n"); }
            if (m_nContentHook != 0) { Logging.PrintTabs(writer, nTabs); writer.Write( ":CHook {0}\n", m_nContentHook); }
            // Original content
            if (m_ContentType == IN_IncludedContent)
            {
                Logging.PrintTabs(writer, nTabs);
                writer.Write( ":OrigContent ");
                m_OrigIncludedContent.Print(writer, nTabs + 1);
                writer.Write( "\n");
            }
            else if (m_ContentType == IN_ReferencedContent)
            {
                Logging.PrintTabs(writer, nTabs);
                writer.Write( ":OrigContent (");
                m_OrigContentRef.Print(writer, nTabs + 1);
                if (m_nOrigContentSize > 0) writer.Write( " :ContentSize {0}", m_nOrigContentSize);
                if (m_nOrigCCPrio != 127) writer.Write( " :CCPriority {0}", m_nOrigCCPrio);
                writer.Write( " )\n");
            }
            if (m_fShared) { Logging.PrintTabs(writer, nTabs); writer.Write( ":Shared true\n"); }
        }
    
        public virtual bool InitiallyActive() 
        { 
            return m_fInitiallyActive; 
        }
        
        // Used for programs only.
        public virtual bool InitiallyAvailable() 
        { 
            return false; 
        } 

        public override bool IsShared() 
        { 
            return m_fShared; 
        }

        // Internal behaviours.
        public override void Preparation(MHEngine engine)
        {
            if (AvailabilityStatus) return;
            // Initialise the content information if any.
            m_IncludedContent.Copy(m_OrigIncludedContent);
            m_ContentRef.Copy(m_OrigContentRef);
            m_nContentSize = m_nOrigContentSize;
            m_nCCPrio = m_nOrigCCPrio;
            // Prepare the base class.
            base.Preparation(engine);
        }

        public override void Destruction(MHEngine engine)
        {
            engine.CancelExternalContentRequest(this);
            base.Destruction(engine);
        }

        public override void ContentPreparation(MHEngine engine)
        {
            if (m_ContentType == IN_IncludedContent)
            {
                // Included content is there - generate ContentAvailable.
                engine.EventTriggered(this, EventContentAvailable);
            }
            else if (m_ContentType == IN_ReferencedContent)
            {
                // We are requesting external content
                engine.CancelExternalContentRequest(this);
                engine.RequestExternalContent(this);
            }
        }

        // Actions.
        public override void SetData(MHOctetString included, MHEngine engine)
        {
            // If the content is currently Included then the data should be Included
            // and similarly for Referenced content.  I've seen cases where SetData
            // with included content has been used erroneously with the intention that
            // this should be the file name for referenced content.
            if (m_ContentType == IN_ReferencedContent) {
                m_ContentRef.ContentRef.Copy(included);
            }
            else {
                Logging.Assert(m_ContentType == IN_IncludedContent);
                m_IncludedContent.Copy(included);
                
            }
            ContentPreparation(engine);
        }

        public override void SetData(MHContentRef referenced, bool fSizeGiven, int size, bool fCCGiven, int cc, MHEngine engine)
        {
            Logging.Assert(m_ContentType == IN_ReferencedContent);
            m_ContentRef.Copy(referenced);
            m_nContentSize = size;
            if (fCCGiven) m_nCCPrio = m_nOrigCCPrio;
            ContentPreparation(engine);
        }

        public override void Preload(MHEngine engine) 
        { 
            Preparation(engine); 
        }
 
        public override void Unload(MHEngine engine) 
        { 
            Destruction(engine); 
        }

        // Called by the engine to deliver external content.
        public virtual void ContentArrived(byte[] data, MHEngine engine) 
        {
            Logging.Assert(false); 
        }

        public const int IN_NoContent = 0;
        public const int IN_IncludedContent = 1;
        public const int IN_ReferencedContent = 2;
    }
}
