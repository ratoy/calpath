using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Zeptomoby.OrbitTools;

namespace GenPath
{
    ///<summary>
    /// * Copyright(c)  GeoStar Soft Group
    /// * Description   : 
    /// * CreateDate    : 2014/12/17 16:28:32
    /// * Creator       : wuhao
    /// * LastChangeDate: 
    /// * LastChanger   : 
    /// * Version Info  : 1.0
    ///</summary>
    public class SatTLE : Satellite
    {
        public struct strTLE
        {
            public string line0;
            public string line1;
            public string line2;
        }
        string m_DbFullName = "";// Application.StartupPath;
        strTLE m_CurTLE;

        public SatTLE(string myname)
            : base(myname)
        {

        }

        public void SetCurTLE(string line0, string line1, string line2)
        {
            m_CurTLE.line0 = line0;
            m_CurTLE.line1 = line1;
            m_CurTLE.line2 = line2;
        }

        public override Point GetSatPosGeo(DateTime Dt)
        {
            Eci eci = GetSatPosEci(Dt, m_CurTLE);
            if (eci == null)
            {
                return null;
            }
            else
            {
                double lon, lat, alt;
                //换成经纬度
                CoordGeo geo = eci.toGeo();
                //把经度坐标换成-180到180之间
                lon = (geo.Longitude) * 180 / System.Math.PI;
                lon = lon > 180 ? lon - 360 : lon;
                lat = (geo.Latitude) * 180 / System.Math.PI;
                alt = geo.Altitude;

                return new Point(lon, lat, alt);
            }
        }

        public override Point GetSatPosEci(DateTime Dt)
        {
            Eci eci = GetSatPosEci(Dt,m_CurTLE);
            if (eci ==null )
            {
                return null;
            }
            else
            {
            return new Point(eci.Position.X, eci.Position.Y, eci.Position.Z);
            }
        }

        public void GetSatPosEciFull(DateTime Dt,out Point pos,out Point vel)
        {
            pos = null;
            vel = null;
            Eci eci = GetSatPosEci(Dt, m_CurTLE);
            if (eci == null)
            {
                return ;
            }
            else
            {
               pos= new Point(eci.Position.X, eci.Position.Y, eci.Position.Z);
               vel= new Point(eci.Velocity.X, eci.Velocity.Y, eci.Velocity.Z);
            }
        }

        Eci GetSatPosEci(DateTime Dt,strTLE sat_data)
        {
            //开始计算,准备TLE数据
            if (sat_data.line1.Length == 0)
            {
                m_LastError = "TLE数据读取出错！";
                return null;
            }

            Tle sattle = new Tle(sat_data.line0, sat_data.line1, sat_data.line2);
            Orbit satorbit = new Orbit(sattle);

            //准备计算时刻
            DateTime tmpTime = Dt;

            double mins;

            //换算成绝对时间
            Julian jul = new Julian(tmpTime.ToUniversalTime());
            //计算时间差
            mins = (jul.Date - satorbit.Epoch.Date) * 1440;
            //计算得出xyz坐标
            Eci eci = satorbit.getPosition(mins);

            return eci;
        }

        protected override double GetHeight()
        {
           Point p= GetSatPosGeo(DateTime.Now);
           if (p ==null )
           {
               return -1;
           }
           else
           {
               return p.Z;
           }
        }

        public override DataTable ComputeTrack(DateTime StartTime, DateTime EndTime, TimeSpan StepTime)
        {
            DataTable dtEPM = dtTrackResult.Clone();

            //开始计算,准备TLE数据
            strTLE sat_data = m_CurTLE;

            if (sat_data.line1.Length == 0)
            {
                m_LastError = "TLE数据读取出错！";
                return null;
            }

            Tle sattle = new Tle(sat_data.line0, sat_data.line1, sat_data.line2);
            Orbit satorbit = new Orbit(sattle);

            //准备计算时刻
            DateTime tmpTime = StartTime;

            while (tmpTime <= EndTime)
            {
                double lat, lon, alt, mins;

                //换算成绝对时间
                Julian jul = new Julian(tmpTime.ToUniversalTime());
                //计算时间差
                mins = (jul.Date - satorbit.Epoch.Date) * 1440;
                //计算得出xyz坐标
                Eci eci = satorbit.getPosition(mins);

                //换成经纬度
                CoordGeo geo = eci.toGeo();
                //把经度坐标换成-180到180之间
                lon = (geo.Longitude) * 180 / System.Math.PI;
                lon = lon > 180 ? lon - 360 : lon;
                lat = (geo.Latitude) * 180 / System.Math.PI;
                alt = geo.Altitude;

                //tmpTime加上下一时长
                tmpTime = tmpTime.Add(StepTime);

                DataRow dr = dtEPM.NewRow();
                dr["Time"] = tmpTime;
                dr["X"] = eci.Position.X;
                dr["Y"] = eci.Position.Y;
                dr["Z"] = eci.Position.Z;
                dr["Lon"] = lon;
                dr["Lat"] = lat;
                dr["Alt"] = alt;

                dtEPM.Rows.Add(dr);
            }

            return dtEPM;

        }

        public override List<TrackPoint> ComputeTrack2(DateTime StartTime, DateTime EndTime, TimeSpan StepTime)
        {
            double R = 6378.15;
            List<TrackPoint> TpList = new List<TrackPoint>();

            //开始计算,准备TLE数据
            strTLE sat_data = m_CurTLE;

            if (sat_data.line1.Length == 0)
            {
                m_LastError = "TLE数据读取出错！";
                return null;
            }

            Tle sattle = new Tle(sat_data.line0, sat_data.line1, sat_data.line2);
            Orbit satorbit = new Orbit(sattle);

            //准备计算时刻
            DateTime tmpTime = StartTime;

            while (tmpTime <= EndTime)
            {
                double lat, lon, alt, mins;

                //换算成绝对时间
                Julian jul = new Julian(tmpTime.ToUniversalTime());
                //计算时间差
                mins = (jul.Date - satorbit.Epoch.Date) * 1440;
                //计算得出xyz坐标
                Eci eci = satorbit.getPosition(mins);

                //换成经纬度
                CoordGeo geo = eci.toGeo();
                //把经度坐标换成-180到180之间
                lon = (geo.Longitude) * 180 / System.Math.PI;
                lon = lon > 180 ? lon - 360 : lon;
                lat = (geo.Latitude) * 180 / System.Math.PI;
                alt = geo.Altitude;

                //tmpTime加上下一时长
                tmpTime = tmpTime.Add(StepTime);

                TrackPoint tp = new TrackPoint(tmpTime, lon, lat, alt, eci.Position.X / R, eci.Position.Y / R,
                    eci.Position.Z / R, eci.Velocity.X / R, eci.Velocity.Y / R, eci.Velocity.Z / R);
                TpList.Add(tp);
            }

            return TpList;
        }
    }
}
