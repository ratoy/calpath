#include <iostream>
#include <cstdlib>
#include "MyPoint.h"

using namespace std;

const double ZERO = 1e-6;
MyPoint::MyPoint(double x, double y)
{
    m_x = x;
    m_y = y;
    m_z = 0;
}
MyPoint::MyPoint(double x, double y, double z)
{
    m_x = x;
    m_y = y;
    m_z = z;
}

bool MyPoint::operator==(const MyPoint &p)
{
    return abs(m_x - p.m_x) < ZERO && abs(m_y - p.m_y) < ZERO && abs(m_z - p.m_z) < ZERO;
}