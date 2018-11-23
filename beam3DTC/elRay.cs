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
    class Ray
    {
        public Vector3 m_a;   // 起点
        public Vector3 m_b;   // 终点

        public Ray() { }
        public Ray(ref Vector3 a, ref Vector3 b) { m_a = new Vector3(ref a); m_b = new Vector3(ref b); }
        public Ray(Vector3 a, Vector3 b) { m_a = new Vector3(ref a); m_b = new Vector3(ref b); }
        public Ray(ref Ray ray) { m_a = new Vector3(ref ray.m_a); m_b = new Vector3(ref ray.m_b); }
        public void opAssign(ref Ray ray) { m_a = new Vector3(ref ray.m_a); m_b = new Vector3(ref ray.m_b); }


        public bool intersect(ref Polygon polygon)
        {
            // 判断点与面的位置
            float s0 = Vector4.dot(ref m_a, polygon.getPleq());
            float s1 = Vector4.dot(ref m_b, polygon.getPleq());

            if (s0 * s1 >= 0)  // 如果在面的同一侧，不可能有交点
                return false;

            int n = polygon.numPoints();

            Vector3 dir = m_b - m_a;
            Vector3 eb = polygon[n - 1] - m_a;
            float sign = 0;
            for (int i = 0; i < n; i++)
            {
                Vector3 ea = new Vector3(ref eb);
                eb = polygon[i] - m_a;

                float det = Vector3.dot(ref dir, Vector3.cross(ref ea, ref eb));

                if (sign == 0)  // 第一次
                    sign = det;
                else if (det * sign < 0)  // 不可能碰撞
                    return false;
            }

            return (sign != 0);
        }

        public bool intersectExt(ref Polygon polygon)
        {
            int n = polygon.numPoints();

            Vector3 dir = m_b - m_a;
            Vector3 eb = polygon[n - 1] - m_a;
            float sign = 0;
            for (int i = 0; i < n; i++)
            {
                Vector3 ea = new Vector3(ref eb);
                eb = polygon[i] - m_a;

                float det = Vector3.dot(ref dir, Vector3.cross(ref ea, ref eb));

                if (sign == 0)
                    sign = det;
                else if (det * sign < 0)
                    return false;
            }

            return (sign != 0);
        }

        public static Vector3 intersect(ref Ray ray, ref Vector4 pleq)
        {
            float s0 = Vector4.dot(ref ray.m_a, ref pleq);
            float s1 = Vector4.dot(ref ray.m_b, ref pleq);

            return ray.m_a + (s0 / (s0 - s1)) * (ray.m_b - ray.m_a);
        }
    }
}
