using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/******************************************************************************

 *

 * Copyright (c) 2004-2005, Samuli Laine
 * 
   Copyright (c) 2018-2019, 尹静萍
 * 
 * All rights reserved.

 *

 * Redistribution and use in source and binary forms, with or without modification,

 * are permitted provided that the following conditions are met:

 *

 *  - Redistributions of source code must retain the above copyright notice,

 *    this list of conditions and the following disclaimer.

 *  - Redistributions in binary form must reproduce the above copyright notice,

 *    this list of conditions and the following disclaimer in the documentation

 *    and/or other materials provided with the distribution.

 *  - Neither the name of the copyright holder nor the names of its contributors

 *    may be used to endorse or promote products derived from this software

 *    without specific prior written permission.

 *

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND

 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED

 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.

 * IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,

 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT

 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,

 * OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,

 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)

 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE

 * POSSIBILITY OF SUCH DAMAGE.

 */

namespace Wellcomm.BLL.beam
{
    class Beam
    {
        private Vector3 m_top;
        private Polygon m_polygon;
        private List<Vector4> m_pleqs;

        public Beam()
        {
            m_top = new Vector3();
            m_polygon = new Polygon();
            m_pleqs = new List<Vector4>();
        }

        public Beam(ref Vector3 top, ref Polygon polygon)
        {
            m_top = new Vector3(ref top);
            m_polygon = new Polygon(ref polygon);

            calculatePleqs();  // 得到 beam 的各面
        }

        public Beam(ref Beam beam)
        {
            m_top = new Vector3(ref beam.m_top);     // 源点的镜像
            m_polygon = new Polygon(ref beam.m_polygon); // 障碍物面
            m_pleqs = new List<Vector4>(beam.m_pleqs); // 各面的面方程
        }

        public Beam(Beam beam)
        {
            m_top = new Vector3(ref beam.m_top);     // 源点的镜像
            m_polygon = new Polygon(ref beam.m_polygon); // 障碍物面
            m_pleqs = new List<Vector4>(beam.m_pleqs); // 各面的面方程
        }

        public void opAssign(ref Beam beam)
        {
            m_top = new Vector3(ref beam.m_top);     // 源点的镜像
            m_polygon = new Polygon(ref beam.m_polygon); // 障碍物面
            m_pleqs = new List<Vector4>(beam.m_pleqs); // 各面的面方程
        }

        public Vector3 getTop() { return m_top; }
        public Polygon getPolygon() { return m_polygon; }
        public int numPleqs() { return (int)m_pleqs.Count(); }
        public Vector4 getPleq(int i) { return m_pleqs[i]; }

        public bool contains(ref Vector3 p)
        {
            for (int i = 0; i < numPleqs(); i++)
                if (Vector4.dot(ref p, getPleq(i)) < 0)
                    return false;
            return true;
        }
        //------------------------------------------------------------------------

        // 得到 beam 的各面，各面法向量向外
        public void calculatePleqs()
        {
            int n = m_polygon.numPoints();

            m_pleqs = new List<Vector4>(new Vector4[n + 1]);
            Vector3 p1 = m_polygon[n - 1];

            float sign = Vector4.dot(ref m_top, m_polygon.getPleq()) > 0 ? -1 : 1;  // -1: 虚拟点位于障碍物法向量一侧

            for (int i = 0; i < n; i++)
            {
                Vector3 p0 = p1;
                p1 = m_polygon[i];

                Vector4 plane = Polygon.getPlaneEquation(ref m_top, ref p0, ref p1);
                plane.normalize();
                m_pleqs[i + 1] = sign * plane;
            }
            m_pleqs[0] = sign * m_polygon.getPleq(); // 第一个面是障碍物面，法向量与障碍物面相反
        }
    }
}
