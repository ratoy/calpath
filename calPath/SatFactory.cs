using System;
using System.Collections.Generic;
using System.Text;

namespace GenPath
{
    ///<summary>
    /// * Copyright(c)  GeoStar Soft Group
    /// * Description   : 
    /// * CreateDate    : 2014/12/19 14:02:33
    /// * Creator       : wuhao
    /// * LastChangeDate: 
    /// * LastChanger   : 
    /// * Version Info  : 1.0
    ///</summary>
    public class SatFactory
    {
        public enum SatType
        {
            TLE, NORMAL
        }
        public Satellite CreateSatellite(string SatID, SatType CustomSatType)
        {
            Satellite sat = null;
            switch (CustomSatType)
            {
                case SatType.TLE:
                    sat = new SatTLE(SatID);
                    break;
                case SatType.NORMAL:
                    break;
                default:
                    break;
            }

            return sat;
        }
    }
}
