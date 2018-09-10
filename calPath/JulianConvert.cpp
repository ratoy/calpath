#include "JulianConvert.h"
#include <cmath>

const double EPOCH_JAN1_12H_2000 = 2451545.0;
const double SEC_PER_DAY = 86400.0;   // Seconds per day (solar)
const double OMEGA_E = 1.00273790934; // Earth rotation per sidereal day
const double PI = 3.141592653589793;
const double F = 1.0 / 298.26;
const double XKMPER = 6378.135;

double JulianConvert::ToGMST(DateTime Dt)
{
    Dt = Dt.ToUniversalTime();
    double lmst = JulianDate(Dt.getYear(), Dt.getMonth(), Dt.getDay(), Dt.getHour(), Dt.getMinute(), Dt.getSecond());
    return ConvertToGMST(lmst);
}

double JulianConvert::JulianDate(int year,               // i.e., 2004
                                 int mon,                // 1..12
                                 int day,                // 1..31
                                 int hour,               // 0..23
                                 int min,                // 0..59
                                 double sec /* = 0.0 */) // 0..(59.999999...)
{
    // Calculate N, the day of the year (1..366)
    int N;
    int F1 = (int)((275.0 * mon) / 9.0);
    int F2 = (int)((mon + 9.0) / 12.0);

    if (IsLeapYear(year))
    {
        // Leap year
        N = F1 - F2 + day - 30;
    }
    else
    {
        // Common year
        N = F1 - (2 * F2) + day - 30;
    }

    double dblDay = N + (hour + (min + (sec / 60.0)) / 60.0) / 24.0;

    // Now calculate Julian date
    year--;
    // Centuries are not leap years unless they divide by 400
    int A = (year / 100);
    int B = 2 - A + (A / 4);

    double NewYears = (int)(365.25 * year) +
                      (int)(30.6001 * 14) +
                      1720994.5 + B; // 1720994.5 = Oct 30, year -1

    double m_Date = NewYears + dblDay;
    return m_Date;
}

double JulianConvert::ConvertToGMST(double m_Date)
{
    const double UT = fmod(m_Date + 0.5, 1.0);
    double TU = (m_Date - EPOCH_JAN1_12H_2000 - UT) / 36525.0;

    double GMST = 24110.54841 + TU *
                                    (8640184.812866 + TU * (0.093104 - TU * 6.2e-06));

    GMST = fmod(GMST + SEC_PER_DAY * OMEGA_E * UT, SEC_PER_DAY);

    if (GMST < 0.0)
    {
        GMST += SEC_PER_DAY; // "wrap" negative modulo value
    }

    return (2 * PI * (GMST / SEC_PER_DAY));
}