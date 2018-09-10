#ifndef RELATION_OPERATOR_H_
#define RELATION_OPERATOR_H_
#include "MyPoint.h"
#include <vector>
#include <tuple>
using namespace std;

class RelationOperator
{
  public:
    // 点在多边形内
    bool PointInPolygon(MyPoint* P, vector<MyPoint> V);
    // 线与多边形相交
    bool LineInsectArea(MyPoint* P0, MyPoint* P1, vector<MyPoint> V);

    bool LineInsectEnvelope(MyPoint* StartMyPoint, MyPoint* EndMyPoint, double Xmax, double Xmin, double Ymax, double Ymin);

    // 多边形与多边形相交
    bool AreaInsectArea(vector<MyPoint> Area1, vector<MyPoint> Area2);

  private:
    tuple<MyPoint*,MyPoint*> GetEdgePoints(MyPoint* p1, MyPoint* p2);
};

#endif