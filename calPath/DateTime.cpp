#include "DateTime.h"
#include <ctime>

using namespace std;
DateTime::DateTime()
{
    Init(1970, 1, 1, 0, 0, 0, 0);
}
DateTime::DateTime(int year, int month, int day)
{
    Init(year, month, day, 0, 0, 0, 0);
}
DateTime::DateTime(int year, int month, int day, int hour, int min, int sec)
{
    Init(year, month, day, hour, min, sec, 0);
}
DateTime::DateTime(int year, int month, int day, int hour, int min, int sec, int millisec)
{
    Init(year, month, day, hour, min, sec, millisec);
}
void DateTime::Init(int year, int month, int day, int hour, int min, int sec, int millisec)
{
    m_year = year;
    m_month = month;
    m_day = day;
    m_hour = hour;
    m_min = min;
    m_sec = sec;
    m_millisec = millisec;
}
DateTime &DateTime::getNow(bool useLocalTime)
{
    time_t ttNow = time(0);
    tm *ptmNow;

    if (useLocalTime)
    {
        ptmNow = localtime(&ttNow);
    }
    else
    {
        ptmNow = gmtime(&ttNow);
    }
    int Year = ptmNow->tm_year + 1900;
    int Month = ptmNow->tm_mon + 1;
    int Day = ptmNow->tm_mday;

    int Hour = ptmNow->tm_hour;
    int Min = ptmNow->tm_min;
    int Sec = ptmNow->tm_sec;

    static DateTime now(Year, Month, Day, Hour, Min, Sec);
    return now;
}
DateTime &DateTime::getToday(bool useLocalTime)
{
    time_t ttNow = time(0);
    tm *ptmNow;

    if (useLocalTime)
    {
        ptmNow = localtime(&ttNow);
    }
    else
    {
        ptmNow = gmtime(&ttNow);
    }
    int Year = ptmNow->tm_year + 1900;
    int Month = ptmNow->tm_mon + 1;
    int Day = ptmNow->tm_mday;

    int Hour = ptmNow->tm_hour;
    int Min = ptmNow->tm_min;
    int Sec = ptmNow->tm_sec;

    static DateTime now(Year, Month, Day, Hour, Min, Sec);
    return now;
}
DateTime &DateTime::UtcNow()
{
    return getNow(false);
}
DateTime &DateTime::Now()
{
    return getNow(true);
}
DateTime &DateTime::Today()
{
    return getToday(true);
}
DateTime &DateTime::UtcToday()
{
    return getToday(false);
}
string DateTime::toString()
{
    return to_string(m_year) + "-" + to_string(m_month) + "-" + to_string(m_day) +
           " " + to_string(m_hour) + ":" + to_string(m_min) + ":" + to_string(m_sec);
}
DateTime DateTime::ToUniversalTime()
{
    //convert to timestamp
    /*
    tm t;
    t.tm_year = m_year - 1900;
    t.tm_mon = m_month - 1;
    t.tm_mday = m_day;
    t.tm_hour = m_hour;
    t.tm_min = m_min;
    t.tm_sec = m_sec;
    t.tm_isdst = 0;
    */
    tm t = ToTm();

    //substract hours*timezone
    t.tm_hour -= m_timezone;
    time_t time_t_after = mktime(&t);
    tm tm_after = *localtime(&time_t_after);

    DateTime dt(tm_after.tm_year + 1900, tm_after.tm_mon + 1,
                tm_after.tm_mday, tm_after.tm_hour, tm_after.tm_min, tm_after.tm_sec, m_millisec);
    return dt;
}

long DateTime::ToTimestamp()
{
    tm Tm = ToTm();
    time_t t = mktime(&Tm);
    long now = static_cast<long int>(t);
    return now;
}

tm DateTime::ToTm()
{
    tm t;
    t.tm_year = m_year - 1900;
    t.tm_mon = m_month - 1;
    t.tm_mday = m_day;
    t.tm_hour = m_hour;
    t.tm_min = m_min;
    t.tm_sec = m_sec;
    t.tm_isdst = 0;
    return t;
}

bool DateTime::operator<=(DateTime &dt)
{
    return ToTimestamp() <= dt.ToTimestamp();
}
time_t DateTime::ToTime_t()
{
    tm Tm = ToTm();
    time_t t = mktime(&Tm);
    return t;
}

DateTime DateTime::AddSec(int seconds)
{
    tm t = ToTm();
    t.tm_sec += seconds;
    time_t time_t_after = mktime(&t);
    tm tm_after = *localtime(&time_t_after);

    DateTime dt(tm_after.tm_year + 1900, tm_after.tm_mon + 1,
                tm_after.tm_mday, tm_after.tm_hour, tm_after.tm_min, tm_after.tm_sec, m_millisec);
    return dt;
}