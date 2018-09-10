using System;
using System.Collections.Generic;
using System.Text;

namespace GenPath
{
    ///<summary>
    /// * Copyright(c)  GeoStar Soft Group
    /// * Description   : 
    /// * CreateDate    : 2014/12/17 16:36:46
    /// * Creator       : wuhao
    /// * LastChangeDate: 
    /// * LastChanger   : 
    /// * Version Info  : 1.0
    ///</summary>
    public class TrackPoint
    {
        DateTime m_Time = new DateTime(1900, 1, 1);
        Point m_EciPoint = null;
        Point m_BlhPoint = null;
        Point m_Vel = null;

        public Point Vel
        {
            get { return m_Vel; }
            set { m_Vel = value; }
        }

        public Point EciPoint
        {
            get { return m_EciPoint; }
            set { m_EciPoint = value; }
        }

        public Point BlhPoint
        {
            get { return m_BlhPoint; }
            set { m_BlhPoint = value; }
        }

        public DateTime Time
        {
            get { return m_Time; }
            set { m_Time = value; }
        }

        public TrackPoint(DateTime dt, double lon, double lat, double alt)
        {
            m_Time = dt;
            m_BlhPoint = new Point(lon, lat, alt);
        }

        public TrackPoint(DateTime dt, double lon, double lat, double alt, double eci_x, double eci_y, double eci_z, double vx, double vy, double vz)
        {
            m_Time = dt;
            m_BlhPoint = new Point(lon, lat, alt);
            m_EciPoint = new Point(eci_x, eci_y, eci_z);
            m_Vel = new Point(vx, vy, vz);
        }
    }
}
