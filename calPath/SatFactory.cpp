#include "SatFactory.h"
#include "SatTLE.h"

Satellite *SatFactory::CreateSatellite(string SatID, SatType CustomSatType)
{
    Satellite *sat = new SatTLE("");
    switch (CustomSatType)
    {
    case TLE:
        sat = new SatTLE(SatID);
        break;
    case NORMAL:
        break;
    default:
        break;
    }

    return sat;
}