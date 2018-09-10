#include "RelationOperator.h"
#include <cmath>
#include <tuple>
using namespace std;
// 点在多边形内
bool RelationOperator::PointInPolygon(MyPoint* P, vector<MyPoint> V)
{
    int cn = 0, n = V.size() - 1; // the  crossing number counter

    // loop through all edges of the polygon
    for (int i = 0; i < n; i++)
    {                                                                   // edge from V[i]  to V[i+1]
        if (((V[i].getY() <= P->getY()) && (V[i + 1].getY() > P->getY())) // an upward crossing
            || ((V[i].getY() > P->getY()) && (V[i + 1].getY() <= P->getY())))
        { // a downward crossing
            // compute  the actual edge-ragetY() intersect getX()-coordinate
            double vt = (P->getY() - V[i].getY()) / (V[i + 1].getY() - V[i].getY());
            if (P->getX() < V[i].getX() + vt * (V[i + 1].getX() - V[i].getX())) // P->getX() < intersect
                ++cn;                                                          // a valid crossing of y=P.y right of P.x
        }
    }
    return (cn & 1) == 1; // 0 if even (out), and 1 if  odd (in)
}
// 线与多边形相交
bool RelationOperator::LineInsectArea(MyPoint* P0, MyPoint* P1, vector<MyPoint> V)
{
    int n = V.size() - 1;
    double SMALL_NUM = 0.00000001;
    if (P0 == P1)
    { // the segment S is a single point
        // test for inclusion of S.P0 in the polygon
        return PointInPolygon(P0, V);
    }

    double tE = 0;        // the maximum entering segment parameter
    double tL = 1;        // the minimum leaving segment parameter
    double t, N, D;       // intersect parameter t = N / D
    MyPoint dS = *P1 - *P0; // the  segment direction vector
    MyPoint e;            // edge vector
    // Vector ne;               // edge outward normal (not explicit in code)

    for (int i = 0; i < n; i++) // process polygon edge V[i]V[i+1]
    {
        e = V[i + 1] - V[i];
        N = MyPoint::Perp2(e, *P0 - V[i]); // = -dot(ne, S.P0 - V[i])
        D = -MyPoint::Perp2(e, dS);       // = dot(ne, dS)
        if (abs(D) < SMALL_NUM)
        {                     // S is nearly parallel to this edge
            if (N < 0)        // P0 is outside this edge, so
                return false; // S is outside the polygon
            else              // S cannot cross this edge, so
                continue;     // ignore this edge
        }

        t = N / D;
        if (D < 0)
        { // segment S is entering across this edge
            if (t > tE)
            { // new max tE
                tE = t;
                if (tE > tL) // S enters after leaving polygon
                    return false;
            }
        }
        else
        { // segment S is leaving across this edge
            if (t < tL)
            { // new min tL
                tL = t;
                if (tL < tE) // S leaves before entering polygon
                    return false;
            }
        }
    }

    return true;
}

bool RelationOperator::LineInsectEnvelope(MyPoint* StartPoint, MyPoint* EndPoint,
                                          double Xmax, double Xmin, double Ymax, double Ymin)
{
    double startX, startY, endX, endY;
    startX = StartPoint->getX();
    startY = StartPoint->getY();
    endX = EndPoint->getX();
    endY = EndPoint->getY();

    //判断是否过180度经线
    if (abs(startX - endX) > 180)
    {
        //与-180度经线和180度经线的交点
        MyPoint* leftp;
         MyPoint* rightp;
        tuple<MyPoint*, MyPoint*> tuplePoints = GetEdgePoints(StartPoint, EndPoint);
        leftp = get<0>(tuplePoints);
        rightp = get<1>(tuplePoints);

        MyPoint* StartNext = startX > 0 ? rightp : leftp;
        MyPoint* EndNext = endX > 0 ? rightp : leftp;
        bool insect = LineInsectEnvelope(StartPoint, StartNext, Xmax, Xmin, Ymax, Ymin);
        insect |= LineInsectEnvelope(EndPoint, EndNext, Xmax, Xmin, Ymax, Ymin);

        return insect;
    }
    else
    {
        if (((startX < Xmin) && (endX < Xmin)) || ((startX > Xmax) && (endX > Xmax)) ||
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

// 多边形与多边形相交
bool RelationOperator::AreaInsectArea(vector<MyPoint> Area1, vector<MyPoint> Area2)
{
    if (Area1.empty() || Area1.size() < 3 || Area2.empty() || Area2.size() < 3)
    {
        return false;
    }
    //判断边线是否相交
    bool Intersected = false;
    for (int i = 0; i < Area1.size() - 1; i++)
    {
        Intersected = LineInsectArea(&Area1[i], &Area1[i + 1], Area2);
        if (Intersected)
        {
            //边线相交，返回true
            return true;
        }
    }

    //如果边线不相交，判断顶点
    return PointInPolygon(&Area1[0], Area2) ||
           PointInPolygon(&Area2[0], Area1);
}

tuple<MyPoint*, MyPoint*> RelationOperator::GetEdgePoints(MyPoint* p1, MyPoint* p2)
{
    MyPoint* lftp;
    MyPoint* rgtp;
    if (p1->getX() - p2->getX() < -180)
    {
        //过-180度经线
        double k = atan((p2->getY() - p1->getY()) / (p2->getX() - p1->getX() - 360));
        lftp = new MyPoint(-180, p1->getY() - k * (p1->getX() + 180));
        rgtp = new MyPoint(180, lftp->getY());
    }
    else if (p1->getX() - p2->getX() > 180)
    {
        //过180度经线
        double k = atan((p2->getY() - p1->getY()) / (p2->getX() - p1->getX() + 360));

        lftp = new MyPoint(-180, p1->getY() - k * (p1->getX() - 180));
        rgtp = new MyPoint(180, lftp->getY());
    }
    auto t = make_tuple(lftp,rgtp);
    return t;
}