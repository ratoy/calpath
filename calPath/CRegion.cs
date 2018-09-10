using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace GenPath
{
    ///<summary>
    /// * Copyright(c)  GeoStar Soft Group
    /// * Description   : 
    /// * CreateDate    : 2014/12/16 14:10:12
    /// * Creator       : wuhao
    /// * LastChangeDate: 
    /// * LastChanger   : 
    /// * Version Info  : 1.0
    ///</summary>
    [Serializable()]
    public class CRegion
    {
        string _satName = "", _senName = "";
        DateTime _startTime = DateTime.Today, _endTime = DateTime.Today;
        double _width = -1; //幅宽
        double _resolution = -1; //分辨率
        double _sideAngle = 0, _sunAngle = 0;//侧摆角和太阳高度角

        string _id = "-1", _AreaId = "-1";//region的ID

        List<Point> m_pGeometry = new List<Point>();

        bool _checked = false;
        int m_CircleCount = -1;

        System.Drawing.Color m_RegionColor = System.Drawing.Color.Black;
        //System.Drawing.Color m_RegionColor =System.Drawing.Color.FromArgb(50,230,0,0);

        #region 属性
        [Browsable(false)]
        public string AreaID
        {
            get { return _AreaId; }
            set { _AreaId=value; }
        }
        [Browsable(false)]
        public System.Drawing.Color RegionColor
        {
            get { return m_RegionColor; }
            set { m_RegionColor = value; }
        }
        [Browsable(false)]
        public List<Point> pGeometry
        {
            get { return m_pGeometry; }
            set { m_pGeometry = value; }
        }
        [Browsable(false)]
        public double SunAngle
        {
            get { return _sunAngle; }
            set { _sunAngle = value; }
        }
        [Browsable(false)]
        public bool Checked
        {
            get { return _checked; }
            set { _checked = value; }
        }
        [Browsable(false)]
        public string PathID
        { get { return _id; } set { _id = value; } }

        public int CircleCount
        {
            get { return m_CircleCount; }
            set { m_CircleCount = value; }
        }

        [CategoryAttribute("影像属性")]
        public string SatName
        {
            get
            {
                return _satName;
            }
            set
            {
                _satName = value;
            }
        }

        [CategoryAttribute("影像属性")]
        public string SenName
        {
            get
            {
                return _senName;
            }
            set
            {
                _senName = value;
            }
        }

        [CategoryAttribute("影像属性")]
        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                _startTime = value;
            }
        }

        [CategoryAttribute("影像属性")]
        public DateTime EndTime
        {
            get
            {
                return _endTime;
            }
            set
            {
                _endTime = value;
            }
        }

        [CategoryAttribute("影像属性")]
        public double Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
            }
        }

        [CategoryAttribute("影像属性")]
        public double resolution
        {
            get
            {
                return _resolution;
            }
            set
            {
                _resolution = value;
            }
        }

        [CategoryAttribute("影像属性")]
        public double SideAngle
        {
            get
            {
                return _sideAngle;
            }
            set
            {
                _sideAngle = value;
            }
        }
        #endregion

        public CRegion Clone()
        {
            //由于ipoint等对象不能序列化，因此只有手动的一个个复制
            CRegion cloneregion = new CRegion();
            cloneregion._id = this._id;
            cloneregion.resolution = this._resolution;
            cloneregion.SatName = this._satName;
            cloneregion.SenName = this._senName;
            cloneregion.SideAngle = this._sideAngle;
            cloneregion.StartTime = this._startTime;
            cloneregion.Width = this._width;
            cloneregion.EndTime = this._endTime;
            cloneregion.Checked = this._checked;
            cloneregion.SunAngle = this._sunAngle;


            return cloneregion;
        }

        public CRegion()
        {
        }

    }

}
