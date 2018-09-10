using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GenPath
{
    ///<summary>
    /// * Copyright(c)  GeoStar Soft Group
    /// * Description   : 
    /// * CreateDate    : 2014/12/17 16:41:33
    /// * Creator       : wuhao
    /// * LastChangeDate: 
    /// * LastChanger   : 
    /// * Version Info  : 1.0
    ///</summary>
    class StripUnit
    {
        //星下点轨迹的两个点
        Point m_FirstPoint = null, m_LastPoint = null;
        //扫描带，顺时针排列
        List<Point> m_StripPoints = new List<Point>();
        DateTime m_StartTime = new DateTime(1900, 1, 1);
        DateTime m_StopTime = new DateTime(1900, 1, 1);
        string m_SenName = "", m_SatName = "";
        Color m_RegionColor = Color.Black;

        public Color RegionColor
        {
            get { return m_RegionColor; }
            set { m_RegionColor = value; }
        }

        public string SatName
        {
            get { return m_SatName; }
            set { m_SatName = value; }
        }

        public string SenName
        {
            get { return m_SenName; }
            set { m_SenName = value; }
        }

        public Point LastPoint
        {
            get { return m_LastPoint; }
            set { m_LastPoint = value; }
        }

        public Point FirstPoint
        {
            get { return m_FirstPoint; }
            set { m_FirstPoint = value; }
        }

        public List<Point> StripPoints
        {
            get { return m_StripPoints; }
            set { m_StripPoints = value; }
        }

        public DateTime StartTime
        {
            get { return m_StartTime; }
            set { m_StartTime = value; }
        }

        public DateTime StopTime
        {
            get { return m_StopTime; }
            set { m_StopTime = value; }
        }
    }
}
