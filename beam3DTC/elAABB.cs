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
    class AABB
    {
        public Vector3 m_mn;
        public Vector3 m_mx;

        public AABB() { m_mn = new Vector3(0, 0, 0); m_mx = new Vector3(0, 0, 0); }
        public AABB(ref Vector3 mn, ref Vector3 mx) { m_mn = new Vector3(ref mn); m_mx = new Vector3(ref mx); }
        public AABB(ref AABB aabb) { m_mn = new Vector3(ref aabb.m_mn); m_mx = new Vector3(ref aabb.m_mx); }
        public AABB(AABB aabb) { m_mn = new Vector3(ref aabb.m_mn); m_mx = new Vector3(ref aabb.m_mx); }
        public void opAssign(ref AABB aabb) { m_mn = aabb.m_mn; m_mx = aabb.m_mx; }

        public void grow(Vector3 p)
        {
            for (int j = 0; j < 3; j++)
            {
                if (p[j] < m_mn[j]) m_mn[j] = p[j];
                if (p[j] > m_mx[j]) m_mx[j] = p[j];
            }
        }

        public bool overlaps(ref AABB o)
        {
            return (m_mn.x < o.m_mx.x && m_mx.x > o.m_mn.x &&
                m_mn.y < o.m_mx.y && m_mx.y > o.m_mn.y &&
                m_mn.z < o.m_mx.z && m_mx.z > o.m_mn.z);
        }

        public bool contains(ref Vector3 p)
        {
            return (p.x > m_mn.x && p.x < m_mx.x &&
                p.y > m_mn.y && p.y < m_mx.y &&
                p.z > m_mn.z && p.z < m_mx.z);
        }
    };
}
