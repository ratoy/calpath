#include "TrackPoint.h"
#include "DateTime.h"
using namespace std;

TrackPoint::TrackPoint()
{
    m_Time = DateTime();
    m_BlhPoint = MyPoint(0,0,0);
    m_EciPoint = MyPoint(0, 0, 0);
    m_Vel = MyPoint(0, 0, 0);
}
TrackPoint::TrackPoint(DateTime t, double lon, double lat, double alt)
{
    m_Time = t;
    m_BlhPoint = MyPoint(lon, lat, alt);
    m_EciPoint = MyPoint(0, 0, 0);
    m_Vel = MyPoint(0, 0, 0);
}
TrackPoint::TrackPoint(DateTime t, double lon, double lat, double alt, 
double eci_x, double eci_y, double eci_z, double vx, double vy, double vz)
{
    m_Time = t;
    m_BlhPoint = MyPoint(lon, lat, alt);
    m_EciPoint = MyPoint(eci_x, eci_y, eci_z);
    m_Vel = MyPoint(vx, vy, vz);
    //cout <<"input: "<<eci_x <<","<<eci_y<<","<<eci_z<<","<<vx<<","<<vy<<","<<vz<<endl;
}
TrackPoint::~TrackPoint()
{
}