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

namespace MHEG.Actions
{
    class MHDrawPoly : MHElemAction
    {
        protected bool m_fIsPolygon;
        protected MHSequence<MHPointArg> m_Points; // List of points

        public MHDrawPoly(string name, bool fIsPolygon)
            : base(name) 
        {
            m_fIsPolygon = fIsPolygon;
            m_Points = new MHSequence<MHPointArg>();
        }

        public override void Initialise(MHParseNode p, MHEngine engine)
        {
            base.Initialise(p, engine);
            MHParseNode args = p.GetArgN(1);
            for (int i = 0; i < args.GetSeqCount(); i++) 
            {
                MHPointArg pPoint = new MHPointArg();
                m_Points.Append(pPoint);
                pPoint.Initialise(args.GetSeqN(i), engine);
            }
        }

        public override void Perform(MHEngine engine)
        {
            Logging.Log(Logging.MHLogError, "MHDrawPoly::Perform - Unimplemented");
            Logging.Assert(false);
/*
            QPointArray points(m_Points.Size());
            for (int i = 0; i < m_Points.Size(); i++)
            {
                MHPointArg *pPoint = m_Points[i];
                points.setPoint(i, pPoint->x.GetValue(engine), pPoint->y.GetValue(engine));
            }
            Target(engine)->DrawPoly(m_fIsPolygon, points, engine);
*/
        }

        protected override void PrintArgs(TextWriter writer, int nTabs)
        {
            writer.Write(" ( ");
            for (int i = 0; i < m_Points.Size; i++) m_Points.GetAt(i).Print(writer, 0);
            writer.Write(" )\n");
        }
    }
}
