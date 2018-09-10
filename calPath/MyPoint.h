#ifndef MYPOINT_H_
#define MYPOINT_H_
#include <iostream>
#include <cmath>

using namespace std;
//Class Point represents points in the Cartesian coordinate
class MyPoint
{
private:
  double m_x, m_y, m_z; // coordinates of the point

public:
  MyPoint() { m_x = m_y = m_z = 0; };
  MyPoint(double x, double y);
  MyPoint(double x, double y, double z);
  void setX(double x) { m_x = x; }
  void setY(double y) { m_y = y; }
  void setZ(double z) { m_z = z; }
  double getX() { return m_x; }
  double getY() { return m_y; }
  double getZ() { return m_z; }
  static double Perp2(MyPoint p1, MyPoint p2) { return p1.m_x * p2.m_y - p1.m_y * p2.m_x; }
  static double DotPlus2(MyPoint p1, MyPoint p2) { return (p1.m_x * p2.m_x + p1.m_y * p2.m_y); }
  bool operator==(const MyPoint &p);

  const MyPoint operator+(const MyPoint &p) const
  {
    return MyPoint(m_x + p.m_x, m_y + p.m_y, m_z + p.m_z);
  }
  const MyPoint operator-(const MyPoint &p) const
  {
    return MyPoint(m_x - p.m_x, m_y - p.m_y, m_z - p.m_z);
  }
};
#endif