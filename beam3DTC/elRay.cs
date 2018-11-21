using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/******************************************************************************
 *
 * Copyright (c) 2004-2005, Samuli Laine
 * All rights reserved.
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
