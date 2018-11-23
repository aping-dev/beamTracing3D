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
    class Program
    {
        static void Main(string[] args)
        {
            Room room = new Room();
    
	        Vector3[] a1 = {new Vector3(0,0,0), new Vector3(0,1000,0), new Vector3(1000,1000,0), new Vector3(1000,0,0)};
	        Vector3[] a2 = {new Vector3(0,0,0), new Vector3(0,1000,0), new Vector3(0,1000,1000), new Vector3(0,0,1000)};
	        Vector3[] a3 = {new Vector3(0,0,0), new Vector3(1000,0,0), new Vector3(1000,0,1000), new Vector3(0,0,1000)};
	        Vector3[] a4 = {new Vector3(0,0,1000), new Vector3(0,1000,1000), new Vector3(1000,1000,1000), new Vector3(1000,0,1000)};
	        Vector3[] a5 = {new Vector3(0,1000,1000), new Vector3(0,1000,0), new Vector3(1000,1000,0), new Vector3(1000,1000,1000)};
	        Vector3[] a6 = {new Vector3(1000,0,1000), new Vector3(1000,1000,1000), new Vector3(1000,1000,0), new Vector3(1000,0,0)};
            Vector3[] a7 = { new Vector3(1000, 0, 1000), new Vector3(1000, 1000, 1000), new Vector3(1000, 1000, 0), new Vector3(500, 0, 0) };
            Vector3[] a8 = { new Vector3(1000, 0, 1000), new Vector3(1000, 1000, 1000), new Vector3(1000, 500, 0), new Vector3(500, 0, 0) };
            Vector3[] a9 = { new Vector3(1000, 0, 1000), new Vector3(1000, 1000, 1000), new Vector3(500, 500, 0), new Vector3(500, 0, 0) };
	        Polygon face1poly = new Polygon(a1, 4, 1);
            Polygon face2poly = new Polygon(a2, 4, 2);
            Polygon face3poly = new Polygon(a3, 4, 3);
            Polygon face4poly = new Polygon(a4, 4, 4);
            Polygon face5poly = new Polygon(a5, 4, 5);
            Polygon face6poly = new Polygon(a6, 4, 6);
            Polygon face7poly = new Polygon(a7, 4, 7);
            Polygon face8poly = new Polygon(a8, 4, 8);
            Polygon face9poly = new Polygon(a9, 4, 9);

            room.addPolygon(ref face1poly);
            room.addPolygon(ref face2poly);
            room.addPolygon(ref face3poly);
            room.addPolygon(ref face4poly);
            room.addPolygon(ref face5poly);
            room.addPolygon(ref face6poly);
            room.addPolygon(ref face7poly);
            room.addPolygon(ref face8poly);
            room.addPolygon(ref face9poly);

            room.constructBSP();

            Vector3 center = room.getCenter();
            Console.WriteLine("Room maximum length: {0}", room.getMaxLength());
            Console.WriteLine("Room center: x={0}, y={1}, z={2}", center.x, center.y, center.z);
            Console.WriteLine("Number of elements: {0}", room.numElements());
            //printf("Number of convex elements: %d", room.numConvexElements());
    
            // Create source localized in room
            Source src1 = new Source();
            src1.setPosition(new Vector3(750,750,750));
            //src1.setOrientation(Matrix3(0,0,1,
            //                            1,0,0,
            //                            0,1,0));
            src1.setName("Src1");
            room.addSource(ref src1);
            Console.WriteLine("Number of sources: {0}", room.numSources());
    
            // Create listener localized in room
            Listener list1 = new Listener();
            list1.setPosition(new Vector3(250, 250, 500));
            //list1.setOrientation(Matrix3(0,0,-1,
            //                             -1,0,0,
            //                             0,1,0));
            list1.setName("Lst1");
            room.addListener(ref list1);

            for(int s=0; s<room.numSources(); s++)
	        {
                for(int l=0; l<room.numListeners(); l++)
		        {
                    Source src = room.getSource(s);
                    Listener lst = room.getListener(l);
    
                    Console.WriteLine("-----------------------------------");
                    //printf("From source %s to listener %s",src.getName(), lst.getName());
                    Console.WriteLine("-----------------------------------");
            
                    // Calculate paths
                    int maximumOrder = 4;
                    PathSolution solution = new PathSolution(ref room, ref src, ref lst, maximumOrder);
                    solution.update();
                    Console.WriteLine("Number of paths calculated: {0}", solution.numPaths());
            
                    // 分析路径
                    int minPathLength = 0;
                    int maxPathLength = 0;
                    for(int i=0; i<solution.numPaths(); i++)
			        {
                        Path path = solution.getPath(i);
                
                        // 计算路径长度
                        int pathLength = 0;
                        Vector3 lastPt = path.m_points[0];
                        for(int j=1; j<path.m_points.Count; j++)
				        {
					        Vector3 pt = path.m_points[j];
                            pathLength += (int)Math.Sqrt(Math.Pow(lastPt.x - pt.x, 2) + 
                                            Math.Pow(lastPt.y - pt.y,2) +
                                            Math.Pow(lastPt.z - pt.z, 2));
                            lastPt = pt;
				        }
                
                        if( pathLength > maxPathLength)
                            maxPathLength = pathLength;
                        if( pathLength < minPathLength || minPathLength == 0)
                            minPathLength = pathLength;
			        }
                    Console.WriteLine("Minimum path length: {0}", minPathLength);
                    Console.WriteLine("Maximum path length: {0}", maxPathLength);
			
		        }
		
	        }
            Console.ReadLine();
        }
    }
}
