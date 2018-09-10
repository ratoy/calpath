#ifndef STRIPUNIT_H_
#define STRIPUNIT_H_
#include "MyPoint.h"
#include "DateTime.h"
#include "Color.h"
#include <vector>
#include <string>
using namespace std;
class StripUnit
{
  private:
    //星下点轨迹的两个点
    MyPoint *m_FirstPoint, *m_LastPoint;
    //扫描带，顺时针排列
    vector<MyPoint> m_StripPoints;
    DateTime *m_StartTime;
    DateTime *m_StopTime;
    string m_SenName, m_SatName;
    Color *m_RegionColor;

  public:
    Color *getRegionColor() const { return m_RegionColor; }
    void setRegionColor(Color *c) { m_RegionColor = c; }

    string getSatName() const { return m_SatName; }
    void setSatName(string s) { m_SatName = s; }

    string getSenName() const { return m_SenName; }
    void setSenName(string s) { m_SenName = s; }

    MyPoint *getLastMyPoint() const { return m_LastPoint; }
    void setLastMyPoint(MyPoint *c) { m_LastPoint = c; }

    MyPoint *getFirstMyPoint() const { return m_FirstPoint; }
    void setFirstMyPoint(MyPoint *c) { m_FirstPoint = c; }

    vector<MyPoint> getStripPoints() const { return m_StripPoints; }
    void setStripPoints(vector<MyPoint> c) { m_StripPoints = c; }

    DateTime *getStartTime() const { return m_StartTime; }
    void setStartTime(DateTime *c) { m_StartTime = c; }

    DateTime *getStopTime() const { return m_StopTime; }
    void setStopTime(DateTime *c) { m_StopTime = c; }

    StripUnit();
    ~StripUnit();
};
#endif