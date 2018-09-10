#ifndef SATTLE_H_
#define SATTLE_H_
#include "Satellite.h"
#include "DateTime.h"
#include "MyPoint.h"
#include "TrackPoint.h"
#include <vector>
#include <tuple>
#include "coreLib.h"
#include "orbitLib.h"

using namespace std;
class SatTLE : public Satellite
{
private:
  string m_line0;
  string m_line1;
  string m_line2;
  string m_DbFullName;
  cEci GetSatPosEci(DateTime Dt, string line0, string line1, string line2);

protected:
  double GetHeight(){return 0.0;};

public:
  void SetCurTLE(string line0, string line1, string line2)
  {
    m_line0 = line0;
    m_line1 = line1;
    m_line2 = line2;
  }

  MyPoint GetSatPosGeo(DateTime Dt);

  MyPoint GetSatPosEci(DateTime Dt);

  tuple<MyPoint, MyPoint> GetSatPosEciFull(DateTime Dt);
  vector<TrackPoint> ComputeTrack2(DateTime StartTime, DateTime EndTime, int StepTimeInSec);
  SatTLE(string myname) : Satellite("", myname){}
};
#endif
