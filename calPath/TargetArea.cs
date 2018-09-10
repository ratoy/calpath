using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GenPath
{
    ///<summary>
    /// * Copyright(c)  GeoStar Soft Group
    /// * Description   : 
    /// * CreateDate    : 2014/12/16 10:10:34
    /// * Creator       : wuhao
    /// * LastChangeDate: 
    /// * LastChanger   : 
    /// * Version Info  : 1.0
    ///</summary>
    public class TargetArea
    {
        string m_Name = "", m_ID = "";
        List<Point> m_Geometry = null;
        Color m_AreaColor = Color.Black;
        Color m_OutLineColor = Color.Black;
        double m_OutLineWidth = 1.0;
        bool m_Checked = false;

        public bool Checked
        {
            get { return m_Checked; }
            set { m_Checked = value; }
        }

        public Color OutLineColor
        {
            get { return m_OutLineColor; }
            set { m_OutLineColor = value; }
        }

        public double OutLineWidth
        {
            get { return m_OutLineWidth; }
            set { m_OutLineWidth = value; }
        }

        public Color AreaColor
        {
            get { return m_AreaColor; }
            set { m_AreaColor = value; }
        }

        public string ID
        {
            get { return m_ID; }
        }

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public List<Point> Geometry
        {
            get { return m_Geometry; }
            set { m_Geometry = value; }
        }

        public TargetArea(string ID, string Name, List<Point> Geometry)
        {
            m_Name = Name;
            m_ID = ID;
            m_Geometry = Geometry;
            DefaultStyleInit();
        }

        public TargetArea(string ID,string Name)
        {
            m_Name = Name;
            m_ID = ID;
            DefaultStyleInit();
        }

        public TargetArea(string ID)
        {
            m_Name = m_ID = ID;
            DefaultStyleInit();
        }

        void DefaultStyleInit()
        { 
            m_AreaColor = GetRandomColor(0,255,150);
            m_OutLineColor = GetRandomColor(0,100);
            m_OutLineWidth = 2;
        }

        Color GetRandomColor(int Min=0, int Max=255,int Alpha=255)
        {
            int MinValue = Math.Max(0, Min);
            int MaxValue = Math.Min(255, Max);
            Random rd = new Random();
            Color pColor = Color.FromArgb(Alpha ,rd.Next(MinValue,MaxValue),
                rd.Next(MinValue),rd.Next(MinValue)) ;

            return pColor;
        }
    }
}
