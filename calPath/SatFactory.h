#ifndef SATFACTORY_H_
#define SATFACTORY_H_
#include <string>
#include "Satellite.h"

using namespace std;
enum SatType
{
    TLE,
    NORMAL
};
class SatFactory
{
  public:
    Satellite *CreateSatellite(string SatID, SatType CustomSatType);
};
#endif