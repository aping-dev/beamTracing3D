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
    class OrientedPoint
    {
        private Vector3 m_position;
        private string m_name;

        public OrientedPoint()
        {
            m_position = new Vector3(0, 0, 0);
        }

        public OrientedPoint(ref OrientedPoint s)
        {
            m_position	= new Vector3(ref s.m_position);
	        m_name		= s.m_name;
        }

        public OrientedPoint(OrientedPoint s)
        {
            m_position = new Vector3(ref s.m_position);
            m_name = s.m_name;
        }

        public void opAssign(ref OrientedPoint s)
        {
            m_position = new Vector3(ref s.m_position);
	        m_name = s.m_name;
        }


	    public Vector3	getPosition		() 					        { return m_position; }	
	    public void		setPosition		(Vector3 position)		{ m_position = new Vector3(ref position); }
	    public string	getName			()      					{ return m_name; }
	    public void		setName			(string name)				{ m_name = name; }
    }
}
