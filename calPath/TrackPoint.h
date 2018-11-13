#ifndef TRACKPOINT_H_
#define TRACKPOINT_H_
#include <iostream>
#include "MyPoint.h"
#include "DateTime.h"

using namespace std;

//Class Point represents points in the Cartesian coordinate
class TrackPoint
{
private:
  DateTime m_Time;
  MyPoint m_EciPoint;
  MyPoint m_BlhPoint;
  MyPoint m_Vel;

public:
  MyPoint getVel() const { return m_Vel; }
  void setVel(MyPoint v) { m_Vel = v; }

  MyPoint getEciPoint() const { return m_EciPoint; }
  void setEciPoint(MyPoint p) { m_EciPoint = p; }

  MyPoint getBlhPoint() const { return m_BlhPoint; }
  void setBlhPoint(MyPoint p) { m_BlhPoint = p; }

  DateTime getTime() { return m_Time; }
  void setTime(DateTime t) { m_Time = t; }

  TrackPoint();
  TrackPoint(DateTime t, double lon, double lat, double alt);
  TrackPoint(DateTime t, double lon, double lat, double alt, double eci_x, double eci_y, double eci_z, double vx, double vy, double vz);
  ~TrackPoint();
};

#endif