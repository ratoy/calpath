#include "CRegion.h"
#include "DateTime.h"
#include "Color.h"
CRegion::CRegion()
{
    Init();
}
CRegion::~CRegion()
{
    delete m_StartTime;
    delete m_StopTime;
    delete m_RegionColor;
}
void CRegion::Init()
{
    m_SatName = "";
    m_SenName = "";
    m_StartTime = &DateTime::Today();
    m_StopTime = &DateTime::Today();
    m_Width = -1;      //幅宽
    m_Resolution = -1; //分辨率
    m_SideAngle = 0;
    m_SunAngle = 0; //侧摆角和太阳高度角

    m_PathId = "-1";
    m_AreaId = "-1"; //region的ID

    m_pGeometry = vector<MyPoint>(0);

    m_Checked = false;
    int m_CircleCount = -1;

    m_RegionColor = new Color(0, 0, 0);
}