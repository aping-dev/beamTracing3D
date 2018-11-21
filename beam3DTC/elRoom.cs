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
    class Room
    {
        private List<Polygon>	m_elements;
	    private List<Source>	m_sources;
	    private List<Listener>	m_listeners;
	    private BSP				m_bsp;

        public int			numElements			() 	    { return m_elements.Count; }
	    public Polygon		getElement			(int i)	{ return m_elements[i]; }


	    public int			numSources			()      	    { return m_sources.Count; }
	    public void			addSource			(ref Source  s) { m_sources.Add(s); }
	    public Source		getSource			(int i)     	{ return m_sources[i]; }

	    public int			numListeners		()          	{ return m_listeners.Count; }
	    public void			addListener			(ref Listener l){ m_listeners.Add(l); }
	    public Listener		getListener			(int i)         { return m_listeners[i]; }
	
        public BSP			getBSP				()          	{ return m_bsp; }


        public Room()
        {
            m_elements = new List<Polygon>();
	        m_sources = new List<Source>();
	        m_listeners = new List<Listener>();
            m_bsp = null;
        }

        //------------------------------------------------------------------------

        public void addPolygon(ref Polygon poly)
        {
            m_elements.Add(new Polygon(ref poly));
        }

        public void constructBSP()
        {
	        // 构建 BSP
	        List<Polygon> polygons = new List<Polygon>();
	        for (int i=0; i < numElements(); i++)
		        polygons.Add(m_elements[i]);
	        m_bsp = new BSP();
	        m_bsp.constructHierarchy(ref polygons, polygons.Count);
        }

        //------------------------------------------------------------------------

        public void getBoundingBox(ref Vector3 mn, ref Vector3 mx) 
        {
	        if (numElements() == 0)
	        {
		        mn.set(0, 0, 0);
		        mx = mn;
		        return;
	        }

            mn = new Vector3(m_elements[0][0]);
            mx = new Vector3(m_elements[0][0]);

            for (int i = 0; i < numElements(); i++)
            {
                for (int j = 0; j < m_elements[i].numPoints(); j++)
                {
                    Vector3 p = getElement(i)[j];
                    for (int k = 0; k < 3; k++)
                    {
                        mn[k] = Math.Min(mn[k], p[k]);
                        mx[k] = Math.Max(mx[k], p[k]);
                    }
                }
            }
        }

        public float getMaxLength() 
        {
	        Vector3 mn = new Vector3(), mx = new Vector3();
	        getBoundingBox(ref mn, ref mx);

	        mx -= mn;
	        float len = mx.x;
	        len = Math.Max(len, mx.y);
            len = Math.Max(len, mx.z);

	        return len;
        }

        public Vector3 getCenter() 
        {
	        Vector3 mn = new Vector3(), mx = new Vector3();
	        getBoundingBox(ref mn, ref mx);
	        return .5f*(mn+mx);
        }

    }
}
