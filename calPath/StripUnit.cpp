#include "StripUnit.h"
StripUnit::StripUnit()
{
    m_FirstPoint = new MyPoint(0, 0);
    m_LastPoint = new MyPoint(0, 0);
    //扫描带，顺时针排列
    m_StripPoints = vector<MyPoint>(0);
    m_StartTime = new DateTime(1900, 1, 1);
    m_StopTime = new DateTime(1900, 1, 1);
    m_SenName = "";
    m_SatName = "";
    m_RegionColor = new Color(0, 0, 0);
}
StripUnit::~StripUnit()
{
    delete m_FirstPoint;
    delete m_LastPoint;
    delete m_StartTime;
    delete m_StopTime;
    delete m_RegionColor;
}