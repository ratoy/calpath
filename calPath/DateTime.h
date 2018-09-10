#ifndef DATETIME_H_
#define DATETIME_H_
#include <string>
using namespace std;
class DateTime
{
private:
  int m_year;
  int m_month;
  int m_day;
  int m_hour;
  int m_min;
  int m_sec;
  int m_millisec;
  int m_timezone;

  void Init(int year, int month, int day, int hour, int min, int sec, int millisec);

  static DateTime &getNow(bool useLocalTime);
  static DateTime &getToday(bool useLocalTime);

  tm ToTm();

public:
  static DateTime &UtcNow();
  static DateTime &Now();
  static DateTime &Today();
  static DateTime &UtcToday();
  int getYear() { return m_year; }
  int getMonth() { return m_month; }
  int getDay() { return m_day; }
  int getHour() { return m_hour; }
  int getMinute() { return m_min; }
  int getSecond() { return m_sec; }
  int getMillisec() { return m_millisec; }
  int getTimezone() { return m_timezone; }
  void setTimezone(int timezone) { m_timezone = timezone; }
  DateTime ToUniversalTime();
  long ToTimestamp();
  time_t ToTime_t();
  string toString();
  DateTime AddSec(int seconds);

  DateTime();
  DateTime(int year, int month, int day);
  DateTime(int year, int month, int day, int hour, int min, int sec);
  DateTime(int year, int month, int day, int hour, int min, int sec, int millisec);

  bool operator<=(DateTime &dt);
};
#endif