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
    class SolutionNode
    {
        public int				m_parent;
	    public Polygon      	m_polygon;

        public SolutionNode()
        {
            m_polygon = new Polygon();
        }
    };

    class Path
    {
	    public int				m_order;
	    public List<Vector3>    m_points;
	    public List<Polygon>	m_polygons;

        public Path()
        {
            m_points = new List<Vector3>();
            m_polygons = new List<Polygon>();
        }
    };

    class PathSolution
    {
        private Room				m_room;
	    private Source				m_source;
	    private Listener			m_listener;
	    private int					m_maximumOrder;

	    private List<Polygon>	    m_polygonCache;
	    private List<Vector3>		m_validateCache;
	    private Dictionary<float, List<int>> m_pathFirstSet;

	    private List<SolutionNode>	m_solutionNodes;
	    private List<Vector4>		m_failPlanes;
	    private List<Vector4>		m_distanceSkipCache;
	    private Vector3				m_cachedSource;

	    private List<Path>			m_paths;

        public int					numPaths		() 			{ return m_paths.Count; }
	    public Path 				getPath			(int i) 	{ return m_paths[i]; }

        //------------------------------------------------------------------------

        float EPS_SIMILAR_PATHS			= 1;
        float EPS_DEGENERATE_POLYGON_AREA	= 1;
        int   DISTANCE_SKIP_BUCKET_SIZE	= 16;

        //------------------------------------------------------------------------

        //------------------------------------------------------------------------

        public PathSolution(ref Room	room,
						   ref Source	source,
						   ref Listener	listener,
						   int			maximumOrder)
        {
        	m_room			 = room;
	        m_source		 = source;
	        m_listener		 = listener;
	        m_maximumOrder	 = maximumOrder;

            m_pathFirstSet = new Dictionary<float, List<int>>();
	        m_polygonCache = new List<Polygon>(new Polygon[maximumOrder]);
	        m_validateCache = new List<Vector3>(new Vector3[maximumOrder*2]);
            m_paths = new List<Path>();
            m_solutionNodes = new List<SolutionNode>();
            m_failPlanes = new List<Vector4>();
	        m_distanceSkipCache = new List<Vector4>();
	        m_cachedSource = new Vector3();
        }

        //------------------------------------------------------------------------

        public void clearCache()
        {
	        m_solutionNodes.Clear();
	        m_failPlanes.Clear();
        }

        //------------------------------------------------------------------------

        public void update()
        {
	        int numProc   = 0;
	        int numTested = 0;

	        Vector3 source = m_source.getPosition();
	        Vector3 target = m_listener.getPosition();

	        m_paths.Clear();

	        if (m_solutionNodes.Count==0 || m_cachedSource != source)  // target 改变后不影响
	        {
		        clearCache();  

		        SolutionNode root = new SolutionNode();
		        root.m_polygon = null;
		        root.m_parent  = -1;
		        m_solutionNodes.Add(root);  // 记录障碍物面及父 beam id

                Beam beam = new Beam();
		        solveRecursive(ref source, ref target, ref beam, 0, 0);  // 得到 m_solutionNodes，m_failPlanes

		        m_cachedSource = source;

		        // 设置桶的数量
		        // numBuckets <= m_solutionNodes.size()
		        int numBuckets = (m_solutionNodes.Count + DISTANCE_SKIP_BUCKET_SIZE - 1) / DISTANCE_SKIP_BUCKET_SIZE;
		        m_distanceSkipCache = new List<Vector4>();
		        for (int i=0; i < numBuckets; i++)
			        m_distanceSkipCache.Add(new Vector4(0,0,0,0));
	        }

	        int n  = m_solutionNodes.Count;
	        int nb = (m_solutionNodes.Count + DISTANCE_SKIP_BUCKET_SIZE - 1) / DISTANCE_SKIP_BUCKET_SIZE;  // numBuckets
	        List<Vector4> skipSphere = m_distanceSkipCache;
	        for (int b=0; b < nb; b++)
	        {
		        Vector4 fc = skipSphere[b];
		        Vector3 r = target - new Vector3(fc.x, fc.y, fc.z);  // 与障碍物的距离
		        if (r.lengthSqr() < fc.w)    // 接收点位于忽略球内，忽略所有路径测试。
			        continue;

		        float maxdot = 0;
		        numProc++;

		        int imn = b * DISTANCE_SKIP_BUCKET_SIZE;   // 第 b 个桶
		        int imx = imn + DISTANCE_SKIP_BUCKET_SIZE;
		        if (imx > n)  
			        imx = n;
		        for (int i=imn; i < imx; i++)
		        {
			        float d = Vector4.dot(ref target, m_failPlanes[i]);
			        if (d >= 0)  // 在失败面的前面，也就是在 beam 面的后面，可能有合法路径
			        {
				        validatePath(ref source, ref target, i, m_failPlanes[i]);  // 验证路径，更新 m_failPlanes[i]，m_path
				
				        numTested++;
			        }
			        if (i == imn || d > maxdot)
				        maxdot = d;
		        }

		        if (maxdot < 0)   // 桶中的所有节点都失败了，便可计算失败面到接收点的最短距离，作为半径，而接收点为球心
			        m_distanceSkipCache[b].set(target.x, target.y, target.z, maxdot*maxdot);
	        }

	        m_pathFirstSet.Clear();

	        //printf("paths: %d (proc %d = %.2f %%, tested %d, valid %d)\n", m_solutionNodes.size(), numProc * DISTANCE_SKIP_BUCKET_SIZE, (float)numProc/nb*100.f, numTested, m_paths.size());
        }

        // 找到最有可能失败的面
        // 平面已经被归一化
        // 取离该点最近的面
        public Vector4 getFailPlane(ref Beam beam, ref Vector3 target)
        {
	        Vector4 failPlane = new Vector4(0, 0, 0, 1);
	        if (beam.numPleqs() > 0)   
		        failPlane = beam.getPleq(0);

	        for (int i=1; i < beam.numPleqs(); i++)   
		        if (Vector4.dot(ref target, beam.getPleq(i)) < Vector4.dot(ref target, ref failPlane)) 
			        failPlane = beam.getPleq(i);

	        return failPlane;
        }

        // failPlane 是输出结果
        public void validatePath(ref Vector3 source,
                                ref Vector3 target,
                                int nodeIndex,
                                ref Vector4 failPlane)
        {
            validatePath(ref source, ref target, nodeIndex, failPlane);
        }
        public void validatePath(ref Vector3 source,
								ref Vector3 target,
								int nodeIndex,
								Vector4 failPlane)
        {
	        // 收集多边形
	        int order = 0;
	        // 只有靠近发射源的面 nodeIndex == 0
	        while(nodeIndex != 0)  // m_polygonCache 中，第一个面是靠近目标的面，最后一个面是靠近发射源的面
	        {
		        m_polygonCache[order++] = m_solutionNodes[nodeIndex].m_polygon;
		        nodeIndex = m_solutionNodes[nodeIndex].m_parent;
	        }

	        // 重建虚拟源
	        Vector3 imgSource = source;
	        for (int i=order-1; i >= 0; i--)  // 从发射源开始重建虚拟源
		        imgSource = Vector4.mirror(ref imgSource, m_polygonCache[i].getPleq());

            // 失败面测试
	        Vector3 s = imgSource;
	        Vector3 t = target;

	        bool		   missed	 = false;
	        int			   missOrder = -1;
	        Polygon missPoly  = null;
	        Ray			   missRay = new Ray(new Vector3(0,0,0), new Vector3(0,0,0));
	        bool		   missSide  = false;

	        for (int i=0; i < order; i++)  // 从靠近目标的面开始
	        {
		        Polygon poly = m_polygonCache[i];
		        Vector4 pleq = poly.getPleq();
		        Ray ray = new Ray(ref s, ref t); 

		        // 射线完全位于障碍物的一边，不可能发射反射
		        if (Vector4.dot(ref s, ref pleq) * Vector4.dot(ref t, ref pleq) > 0)  
		        {
			        missed	  = true;
			        missSide  = true;
			        missOrder = i;
			        missPoly  = poly;
			        missRay	  = ray;
			        break;
		        }
		
		        // 射线没有与障碍物产生交点，不可能发生反射
		        if (!ray.intersectExt(ref poly))
		        {
			        missed	  = true;
			        missSide  = false;
			        missOrder = i;
			        missPoly  = poly;
			        missRay   = ray;
			        break;
		        }

		        // 射线与障碍物的交点
		        Vector3 isect = Ray.intersect(ref ray, ref pleq);
		        s = Vector4.mirror(ref s, ref pleq);  // 新的虚拟源
		        t = isect;   

		        m_validateCache[i*2] = isect;
		        m_validateCache[i*2+1] = s;
	        }

	        // 传播失败面
	        if (missed)
	        {
		        Vector4 missPlane = new Vector4(0, 0, 0, 0);
		        if (missSide)
		        {
                    // 根据面方程重建
			        missPlane = missPoly.getPleq();
			        if (Vector4.dot(ref missRay.m_a, ref missPlane) > 0)
				        missPlane.opNegative();
		        } 
		        else
		        {
                    // 根据失败的 beam 边重建
			        Beam beam = new Beam(ref missRay.m_a, ref missPoly);
			        missPlane = beam.getPleq(1);
			        for (int i=2; i < beam.numPleqs(); i++)
				        if (Vector4.dot(ref missRay.m_b, beam.getPleq(i)) < Vector4.dot(ref missRay.m_b, ref missPlane))
					        missPlane = beam.getPleq(i);
		        }
		
		        // 传播失败面
		        for (int i=missOrder-1; i >= 0; i--)  // 从当前面到接近目标的面
			        missPlane = Vector3.mirror(ref missPlane, m_polygonCache[i].getPleq());

		        // 由于浮点精度，可能出错，重新计算
		        if (Vector4.dot(ref target, ref missPlane) > 0)
		        {
                    // 从失败面重建 beam
			        Beam beam = new Beam();
			        imgSource = source;
			        for (int i=order-1; i >= 0; i--)
			        {
				        Polygon poly = m_polygonCache[i];
				        poly.clip(ref beam);

				        imgSource = Vector4.mirror(ref imgSource, poly.getPleq());
				        beam = new Beam(ref imgSource, ref poly);
			        }

			        // 更新失败面
			        missPlane = getFailPlane(ref beam, ref target);
		        }

		        // 归一化
		        missPlane.normalize();
                failPlane = missPlane;
		        return;
	        }

	        // 检测路径是否合法
	        t = target;
	        for (int i=0; i < order; i++)  // 从接收点开始检测
	        {
		        Vector3 isect = m_validateCache[i*2];
                Ray ray = new Ray(ref isect, ref t);
		        if (m_room.getBSP().rayCastAny(ref ray))
			        return;

		        t = isect;
	        }
            Ray ray1 = new Ray(ref source, ref t);
	        if (m_room.getBSP().rayCastAny(ref ray1))  // 检测到发射源的路径是否合法
		        return;

	        // 将合法路径加入结果
	        Path path = new Path();
	        path.m_order = order;
	        path.m_points = new List<Vector3>(new Vector3[order+2]);   
	        path.m_polygons = new List<Polygon>(new Polygon[order]);

	        t = target;
	        for (int i=0; i < order; i++)
	        {
		        path.m_points[order-i+1] = t;
		        path.m_polygons[order-i-1] = m_polygonCache[i];

		        t = m_validateCache[i*2];
	        }

	        path.m_points[0] = source;
	        path.m_points[1] = t;


	        // 将相似的路径移除
	        float fval = Vector3.dot(path.m_points[1], new Vector3(1, 1, 1));  // 
	        float fmin = fval - 2*EPS_SIMILAR_PATHS;
	        float fmax = fval + 2*EPS_SIMILAR_PATHS;

            foreach(List<int> paths in m_pathFirstSet.Values)
	        {
		        //List<int> paths = m_pathFirstSet[j];

                for(int i=0; i<paths.Count; i++)
                {
		            Path p = m_paths[paths[i]];
		            if (p.m_order != order)
			            continue;
		            bool safe = false;
		            for (int k=1; k < (int)p.m_points.Count-1; k++)
		            {
			            if ((p.m_points[k] - path.m_points[k]).lengthSqr() > EPS_SIMILAR_PATHS * EPS_SIMILAR_PATHS)
			            {
				            safe = true;
				            break;
			            }
		            }
		            if (!safe)
			            return;
	            }
            }

            if(m_pathFirstSet.Keys.Contains(fval))
            {
                m_pathFirstSet[fval].Add(m_paths.Count);
            }
            else
            {
                m_pathFirstSet[fval] = new List<int>();
                m_pathFirstSet[fval].Add(m_paths.Count);
            }
	        m_paths.Add(path);
        }

        // 建立 beam 树，得到 m_solutionNodes，不考虑遮挡
        // 存储每个 beam 节点中离目标点最近的 beam 侧面
        public void solveRecursive(ref Vector3 source,ref Vector3 target,ref Beam beam, int order,int parentIndex)
        {
	        m_failPlanes.Add(new Vector4(getFailPlane(ref beam, ref target)));  // 离目标点最近的面

	        if (order >= m_maximumOrder)
		          return;

	        List<Polygon> polygons = new List<Polygon>();
	        m_room.getBSP().beamCast(ref beam, ref polygons);
	        for (int i=(int)polygons.Count-1; i >= 0; i--)  // 当前 BSP 节点中包含的所有多边形
	        {
		        Polygon orig = polygons[i];
		        Vector3 imgSource = Vector4.mirror(ref source, orig.getPleq());

		        if (parentIndex > 0)  // 跳过一些特殊情况
		        {
			        Polygon ppoly = m_solutionNodes[parentIndex].m_polygon;
			        if (orig.m_id == ppoly.m_id)  // 如果与上一个面是同一个面，跳过
				        continue;

			        Vector3 testSource = Vector4.mirror(ref imgSource, ppoly.getPleq());
			        if ((source-testSource).length() < EPS_SIMILAR_PATHS)  // 如果与上一个源相距太近
				        continue;
		        }

		
		        Polygon poly = orig;
		        if (poly.clip(ref beam) == Polygon.ClipResult.CLIP_VANISHED)
			        continue;

		        if (poly.getArea() < EPS_DEGENERATE_POLYGON_AREA)
			        continue;

		        Beam b = new Beam(ref imgSource, ref poly);

		        SolutionNode node = new SolutionNode();
		        node.m_polygon = orig;
		        node.m_parent  = parentIndex;
		        m_solutionNodes.Add(node);

		        solveRecursive(ref imgSource, ref target, ref b, order+1, m_solutionNodes.Count-1);

		        if (order==0)
			        Console.WriteLine("building beam tree.. {0}% ({1})\r", 100-(float)i/(float)polygons.Count()*100, m_solutionNodes.Count);
	        }

	        if (order==0)
		       Console.WriteLine();
        }

    }
}
