using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace GenPath
{
    ///<summary>
    /// * Copyright(c)  GeoStar Soft Group
    /// * Description   : 
    /// * CreateDate    : 2014/12/12 18:00:08
    /// * Creator       : wuhao
    /// * LastChangeDate: 
    /// * LastChanger   : 
    /// * Version Info  : 1.0
    ///</summary>
    public class Sensor
    {
        private string m_senID = "", m_satID = "", m_senName = "", m_satName = "";

        private string m_countryName = "";
        private double m_width = -1;
        private double m_obsAngle = -1;//观测角
        private System.Drawing.Color m_senColor;
        private double m_curSideAngle = 0; //当前的侧摆角
        private double m_resolution = 0;
        private bool m_isChecked = false; //指示treeview上的选中状态
        private double m_rightsideangle = 0; //向右侧摆的角度
        private double m_leftsideangle = 0; //向左侧摆的角度
        double m_initangle = 0;//载荷安装角

        public Sensor(string satid, string sensorid)
        {
            m_senID = sensorid;
            m_satID = satid;
        }

        #region 属性
        [CategoryAttribute("载荷属性")]
        public string SatID
        {
            get { return m_satID; }
        }

        [CategoryAttribute("载荷属性")]
        public string SenID
        {
            get { return m_senID; }
        }
        [CategoryAttribute("载荷属性")]
        public double Resolution
        {
            get
            {
                return m_resolution;
            }
            set
            {
                m_resolution = value;
            }
        }

        [CategoryAttribute("载荷属性")]
        public double ObserveAngle
        {
            get
            {
                return m_obsAngle;
            }
            set
            {
                m_obsAngle = value;
            }
        }

        [Browsable(false)]
        public bool Checked
        {
            get
            {
                return m_isChecked;
            }
            set
            {
                m_isChecked = value;
            }
        }

        [Browsable(false)]
        public double LeftSideAngle
        {
            get
            {
                return m_leftsideangle;
            }
            set
            {
                m_leftsideangle = value;
            }
        }

        [Browsable(false)]
        public double RightSideAngle
        {
            get
            {
                return m_rightsideangle;
            }
            set
            {
                m_rightsideangle = value;
            }
        }

        [CategoryAttribute("载荷属性")]
        public string SenName
        {
            get
            {
                return m_senName;
            }
            set
            {
                m_senName = value;
            }
        }

        [CategoryAttribute("载荷属性")]
        public string SatName
        {
            get
            {
                return m_satName;
            }
            set
            {
                m_satName = value;
            }
        }

        [CategoryAttribute("载荷属性")]
        public string CountryName
        {
            get
            {
                return m_countryName;
            }
            set
            {
                m_countryName = value;
            }
        }

        [CategoryAttribute("载荷属性")]
        public double Width
        {
            get
            {
                return m_width;
            }
            set
            {
                m_width = value;
            }
        }


        [CategoryAttribute("载荷属性")]
        public System.Drawing.Color SenColor
        {
            get
            {
                return m_senColor;
            }
            set
            {
                m_senColor = value;
            }
        }

        [CategoryAttribute("载荷属性")]
        public double CurSideAngle
        {
            get
            {
                return m_curSideAngle;
            }
            set
            {
                m_curSideAngle = value;
            }
        }

        [CategoryAttribute("载荷属性")]
        public double InitAngle
        {
            get
            {
                return m_initangle;
            }
            set
            {
                m_initangle = value;
            }
        }
        #endregion

    }
}
