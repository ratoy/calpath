#include "SatTLE.h"
#include <vector>
#include <string>
#include "DateTime.h"
using namespace std;

void SatTLE::ComputeTrack(DateTime StartTime, DateTime EndTime, int StepTimeInSec,TrackPoint res[])
{
    double R = 6378.15;

    //开始计算,准备TLE数据
    if (m_line1.empty())
    {
        m_LastError = "TLE数据读取出错！";
        return ;
    }

    cTle sattle(m_line0, m_line1, m_line2);
    cOrbit satorbit(sattle);

    //准备计算时刻
    DateTime tmpTime = StartTime;


for(int i = 0; i < 25921; i++)
{
        double lat, lon, alt, mins;

        //换算成绝对时间
        cJulian jul(tmpTime.ToTime_t());
        //计算时间差
        mins = (jul.Date() - satorbit.Epoch().Date()) * 1440;
        //计算得出xyz坐标
        cEciTime eci = satorbit.PositionEci(mins);

        //换成经纬度
        cGeoTime geo(eci);
        //把经度坐标换成-180到180之间
        lon = geo.LongitudeDeg();
        lon = lon > 180 ? lon - 360 : lon;
        lat = geo.LatitudeDeg();
        alt = geo.AltitudeKm();

        TrackPoint tp(tmpTime, lon, lat, alt, eci.Position().m_x / R, eci.Position().m_y / R,
                      eci.Position().m_z / R, eci.Velocity().m_x / R, eci.Velocity().m_y / R,
                      eci.Velocity().m_z / R);

        res[i]=tp;
        //tmpTime加上下一时长
        tmpTime = tmpTime.AddSec(StepTimeInSec);
    }
}

vector<TrackPoint> SatTLE::ComputeTrack2(DateTime StartTime, DateTime EndTime, int StepTimeInSec)
{
    double R = 6378.15;
    vector<TrackPoint> TpList;

    //开始计算,准备TLE数据
    if (m_line1.empty())
    {
        m_LastError = "TLE数据读取出错！";
        return TpList;
    }

    cTle sattle(m_line0, m_line1, m_line2);
    cOrbit satorbit(sattle);

    //准备计算时刻
    DateTime tmpTime = StartTime;

    while (tmpTime <= EndTime)
    {
        double lat, lon, alt, mins;

        //换算成绝对时间
        cJulian jul(tmpTime.ToTime_t());
        //计算时间差
        mins = (jul.Date() - satorbit.Epoch().Date()) * 1440;
        //计算得出xyz坐标
        cEciTime eci = satorbit.PositionEci(mins);

        //换成经纬度
        cGeoTime geo(eci);
        //把经度坐标换成-180到180之间
        lon = geo.LongitudeDeg();
        lon = lon > 180 ? lon - 360 : lon;
        lat = geo.LatitudeDeg();
        alt = geo.AltitudeKm();

        TrackPoint tp(tmpTime, lon, lat, alt, eci.Position().m_x / R, eci.Position().m_y / R,
                      eci.Position().m_z / R, eci.Velocity().m_x / R, eci.Velocity().m_y / R,
                      eci.Velocity().m_z / R);

        TpList.push_back(tp);
        //tmpTime加上下一时长
        tmpTime = tmpTime.AddSec(StepTimeInSec);
    }

    return TpList;
}