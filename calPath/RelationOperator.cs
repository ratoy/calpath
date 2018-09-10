using System;
using System.Collections.Generic;
using System.Text;

namespace GenPath
{
    ///<summary>
    /// * Copyright(c)  GeoStar Soft Group
    /// * Description   : 
    /// * CreateDate    : 2014/12/17 12:09:59
    /// * Creator       : wuhao
    /// * LastChangeDate: 
    /// * LastChanger   : 
    /// * Version Info  : 1.0
    ///</summary>
    public class RelationOperator
    {

        #region 点在多边形内
        // cn_PnPoly(): crossing number test for a point in a polygon
        //      Input:   P = a point,
        //               V[] = vertex points of a polygon V[n+1] with V[n]=V[0]
        //      Return:  0 = outside, 1 = inside
        // This code is patterned after [Franklin, 2000]
        public bool PointInPolygon(Point P, Point[] V)
        {
            int cn = 0, n = V.Length-1;    // the  crossing number counter

            // loop through all edges of the polygon
            for (int i = 0; i < n; i++)
            {    // edge from V[i]  to V[i+1]
                if (((V[i].Y <= P.Y) && (V[i + 1].Y > P.Y))     // an upward crossing
                 || ((V[i].Y > P.Y) && (V[i + 1].Y <= P.Y)))
                { // a downward crossing
                    // compute  the actual edge-raY intersect X-coordinate
                    double vt = (P.Y - V[i].Y) / (V[i + 1].Y - V[i].Y);
                    if (P.X < V[i].X + vt * (V[i + 1].X - V[i].X)) // P.X < intersect
                        ++cn;   // a valid crossing of y=P.y right of P.x
                }
            }
            return (cn & 1) == 1;    // 0 if even (out), and 1 if  odd (in)
        }
        #endregion

        #region 线与多边形相交
        // intersect2D_SegPoly(): intersect a 2D segment with a convex polygon
        //    Input:  S = 2D segment to intersect with the convex polygon V[]
        //            n = number of 2D points in the polygon
        //            V[] = array of n+1 vertex points with V[n] = V[0]
        //      Note: The polygon MUST be convex and
        //                have vertices oriented counterclockwise (ccw).
        //            This code does not check for and verify these conditions.
        //    Output: *IS = the intersection segment (when it exists)
        //    Return: FALSE = no intersection
        //            TRUE  = a valid intersection segment exists
        public bool LineInsectArea(Point P0, Point P1, Point[] V)
        {
            int n = V.Length-1;
            double SMALL_NUM = 0.00000001;
            if (P0 == P1)
            {         // the segment S is a single point
                // test for inclusion of S.P0 in the polygon
                return PointInPolygon(P0, V);
            }

            double tE = 0;              // the maximum entering segment parameter
            double tL = 1;              // the minimum leaving segment parameter
            double t, N, D;             // intersect parameter t = N / D
            Point dS = P1 - P0;     // the  segment direction vector
            Point e;                   // edge vector
            // Vector ne;               // edge outward normal (not explicit in code)

            for (int i = 0; i < n; i++)   // process polygon edge V[i]V[i+1]
            {
                e = V[i + 1] - V[i];
                N = Point.Perp2(e, P0 - V[i]); // = -dot(ne, S.P0 - V[i])
                D = -Point.Perp2(e, dS);       // = dot(ne, dS)
                if (Math.Abs(D) < SMALL_NUM)
                {  // S is nearly parallel to this edge
                    if (N < 0)              // P0 is outside this edge, so
                        return false;      // S is outside the polygon
                    else                    // S cannot cross this edge, so
                        continue;          // ignore this edge
                }

                t = N / D;
                if (D < 0)
                {            // segment S is entering across this edge
                    if (t > tE)
                    {       // new max tE
                        tE = t;
                        if (tE > tL)   // S enters after leaving polygon
                            return false;
                    }
                }
                else
                {                  // segment S is leaving across this edge
                    if (t < tL)
                    {       // new min tL
                        tL = t;
                        if (tL < tE)   // S leaves before entering polygon
                            return false;
                    }
                }
            }

            return true;
        }

        public bool LineInsectEnvelope(Point StartPoint, Point EndPoint, double Xmax, double Xmin, double Ymax, double Ymin)
        {
            double startX, startY, endX, endY;
            startX = StartPoint.X;
            startY = StartPoint.Y;
            endX = EndPoint.X;
            endY = EndPoint.Y;

            //判断是否过180度经线
            if (Math.Abs(startX - endX) > 180)
            {
                //与-180度经线和180度经线的交点
                Point leftp, rightp;
                GetEdgePoints(StartPoint, EndPoint, out leftp, out rightp);
                Point StartNext = startX > 0 ? rightp : leftp;
                Point EndNext = endX > 0 ? rightp : leftp;
                bool insect = LineInsectEnvelope(StartPoint, StartNext, Xmax, Xmin, Ymax, Ymin);
                insect |= LineInsectEnvelope(EndPoint, EndNext, Xmax, Xmin, Ymax, Ymin);

                return insect;
            }
            else
            {
                if ((startX < Xmin) && (endX < Xmin) || ((startX > Xmax) && (endX > Xmax)) ||
                    ((startY < Ymin) && (endY < Ymin)) || ((startY > Ymax) && (endY > Ymax)))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// 计算与180度经线的交点
        /// </summary>
        /// <param name="p1">起始点</param>
        /// <param name="p2">终止点</param>
        /// <param name="lftp">与-180度的交点</param>
        /// <param name="rgt">与180度的交点</param>
        protected void GetEdgePoints(Point p1, Point p2, out Point lftp, out Point rgtp)
        {
            if (p1.X - p2.X < -180)
            {
                //过-180度经线
                double k = Math.Atan((p2.Y - p1.Y) / (p2.X - p1.X - 360));
                lftp = new Point(-180, p1.Y - k * (p1.X + 180));
                rgtp = new Point(180, lftp.Y);
            }
            else if (p1.X - p2.X > 180)
            {
                //过180度经线
                double k = Math.Atan((p2.Y - p1.Y) / (p2.X - p1.X + 360));

                lftp = new Point(-180, p1.Y - k * (p1.X - 180));
                rgtp = new Point(180, lftp.Y);
            }
            else
            {
                lftp = null;
                rgtp = null;
            }
        }
        #endregion

        #region 多边形与多边形相交
        /// <summary>
        /// 目标区域是否相交
        /// </summary>
        /// <param name="AreaPoints">目标区域的顶点，顺时针排列</param>
        /// <param name="Area2">第二个目标区域</param>
        /// <returns></returns>
        public bool AreaInsectArea(List<Point> Area1, List<Point> Area2)
        {
            if (Area1 == null || Area1.Count < 3 || Area2 == null || Area2.Count < 3)
            {
                return false;
            }
            //判断边线是否相交
            int Area1Count = Area1.Count;
            Point[] Area2Array = Area2.ToArray();
            bool Intersected = false;
            for (int i = 0; i < Area1.Count - 1; i++)
            {
                Intersected = LineInsectArea(Area1[i], Area1[i + 1], Area2Array);
                if (Intersected)
                {
                    //边线相交，返回true
                    return true;
                }
            }

            //如果边线不相交，判断顶点
            return PointInPolygon(Area1[0], Area2Array) ||
                PointInPolygon(Area2[0], Area1.ToArray());
        }
        #endregion
    }

    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Point()
        { X = 0; Y = 0; Z = 0; }
        public Point(double x, double y)
        { X = x; Y = y; Z = 0; }
        public Point(double x, double y, double z)
        { X = x; Y = y; Z = z; }

        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }

        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        /// <summary>
        /// 二维叉乘
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double Perp2(Point p1, Point p2)
        {
            return p1.X * p2.Y - p1.Y * p2.X;
        }
        /// <summary>
        /// 二维点乘
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double DotPlus2(Point p1, Point p2)
        {
            return (p1.X * p2.X + p1.Y * p2.Y);
        }
    }
}
