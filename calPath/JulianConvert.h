#ifndef JULIAN_CONVERT_H_
#define JULIAN_CONVERT_H_
#include "DateTime.h"
class JulianConvert
{
  public:
    double ToGMST(DateTime Dt);
    bool IsLeapYear(int y)
    {
        return (y % 4 == 0 && y % 100 != 0) || (y % 400 == 0);
    }

    double JulianDate(int year, int mon, int day, int hour, int min, double sec);
    double ConvertToGMST(double m_Date);
};
#endif