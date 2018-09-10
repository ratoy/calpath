#ifndef CREGION_H_
#define CREGION_H_
#include "MyPoint.h"
#include "DateTime.h"
#include "Color.h"
#include <vector>
#include <string>
using namespace std;
class CRegion
{
  private:
    string m_SatName, m_SenName;
    DateTime *m_StartTime, *m_StopTime;
    double m_Width;                 //幅宽
    double m_Resolution;            //分辨率
    double m_SideAngle, m_SunAngle; //侧摆角和太阳高度角

    string m_PathId, m_AreaId; //region的ID

    vector<MyPoint> m_pGeometry;

    bool m_Checked ;
    int m_CircleCount ;

    Color *m_RegionColor;
    void Init();

  public:
    CRegion();
    ~CRegion();

    Color *getRegionColor() const { return m_RegionColor; }
    void setRegionColor(Color *c) { m_RegionColor = c; }

    string getAreaId() const { return m_AreaId; }
    void setAreaId(string s) { m_AreaId = s; }

    double getSunAngle() const { return m_SunAngle; }
    void setSunAngle(double s) { m_SunAngle = s; }

    vector<MyPoint> getpGeometry() const { return m_pGeometry; }
    void setpGeometry(vector<MyPoint> s) { m_pGeometry = s; }

    double getChecked() const { return m_Checked; }
    void setChecked(double s) { m_Checked = s; }

    string getPathId() const { return m_PathId; }
    void setPathId(string s) { m_PathId = s; }

    int getCircleCount() const { return m_CircleCount; }
    void setCircleCount(int s) { m_CircleCount = s; }

    string getSatName() const { return m_SatName; }
    void setSatName(string s) { m_SatName = s; }

    string getSenName() const { return m_SenName; }
    void setSenName(string s) { m_SenName = s; }

    DateTime *getStartTime() const { return m_StartTime; }
    void setStartTime(DateTime *c) { m_StartTime = c; }

    DateTime *getStopTime() const { return m_StopTime; }
    void setStopTime(DateTime *c) { m_StopTime = c; }

    double getWidth() const { return m_Width; }
    void setWidth(double s) { m_Width = s; }

    double getResolution() const { return m_Resolution; }
    void setResolution(double s) { m_Resolution = s; }

    double getSideAngle() const { return m_SideAngle; }
    void setSideAngle(double s) { m_SideAngle = s; }
};

#endif