using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.ComponentModel;
//using SharpMap.Geometries;

namespace GenPath
{
    public class Satellite
    {
        protected const double R=6378.15;
        //YYYY-MM-DD HH24:MI:SS
        protected const string BJTimeFormate = "yyyy-MM-dd HH:mm:ss";//北京时间的格式
        protected TimeSpan m_satTraceSpan;//计算轨迹的时间间隔，在计算时由外部传入
        public static double m_MarginTimeMin = 2;//扫描带的时间裕量，前后各加上该时间，单位为分钟
        protected string m_satName = "";

        protected System.Drawing.Color m_displayColor;
        private double m_satHeight = 0;

        protected bool m_isChecked = false; //指示treeview上的选中状态
        //时间间隔
        protected TimeSpan StepTime = new TimeSpan(0, 0, 20);//正常步长,20s
        protected TimeSpan LongStepTime = new TimeSpan(0, 2, 0);//长步长,2min
        protected TimeSpan ShortStepTime = new TimeSpan(0, 0, 1);//短步长,1s
        protected TimeSpan TinyStepTime = new TimeSpan(0, 0, 0, 0, 1);//超短步长,1ms

        protected RelationOperator m_RelOpera = new RelationOperator();
        //卫星上的载荷
        protected List<Sensor> m_sensorlist = new List<Sensor>();

        //存储轨道计算结果的表结构
        protected static DataTable dtTrackResult = new DataTable();
        //存储载荷观测范围的表结构
        protected static DataTable dtPathResult = new DataTable();

        protected string m_NoardID;
        protected string m_LastError = "";

        TimeSpan m_TrackTp = new TimeSpan(0, 0, 20);

        //属性
        Dictionary<string, object> m_DictProperties = new Dictionary<string, object>();

        public enum PathMode
        {
            Day, Night, Both
        }

        public Satellite(string satid, string satname = "")
        {
            m_NoardID = satid;
            if (satname.Length == 0)
            {
                m_satName = satid;
            }

            if (dtTrackResult.Columns.Count == 0)
            {
                dtTrackResult.Columns.Add("Time", typeof(DateTime));
                dtTrackResult.Columns.Add("Lon", typeof(double));
                dtTrackResult.Columns.Add("Lat", typeof(double));
                dtTrackResult.Columns.Add("Alt", typeof(double));
                dtTrackResult.Columns.Add("X", typeof(double));
                dtTrackResult.Columns.Add("Y", typeof(double));
                dtTrackResult.Columns.Add("Z", typeof(double));

                dtPathResult.Columns.Add("SenName", typeof(string));
                dtPathResult.Columns.Add("Time", typeof(DateTime));
                dtPathResult.Columns.Add("LeftLon", typeof(double));
                dtPathResult.Columns.Add("LeftLat", typeof(double));
                dtPathResult.Columns.Add("RightLon", typeof(double));
                dtPathResult.Columns.Add("RightLat", typeof(double));
            }
        }

        T GetFromDic<T>(Dictionary<string, object> Dict, string KeyName, T vDefault)
        {
            object obj = null;
            Dict.TryGetValue(KeyName, out obj);

            if (obj == null)
                return vDefault;
            Type t = typeof(T);
            try
            {
                if (t.IsEnum)
                    return (T)Enum.Parse(t, Convert.ToString(obj));

                return (T)Convert.ChangeType(obj, t);
            }
            catch
            {

            }
            return vDefault;
        }

        public Sensor GetSensor(string SenID)
        {
            foreach (Sensor  sen in m_sensorlist )
            {
                if (string.Compare(SenID,sen.SenID,true)==0 ||
                    string.Compare(SenID,sen.SenName,true)==0)
                {
                    return sen;
                }
            }
            return null;
        }

        #region 属性
        [Browsable(false)]
        public Dictionary<string, object> DictProperties
        {
            set { m_DictProperties = value; }
        }

        [Browsable(false)]
        public string LastError
        {
            get { return m_LastError; }
        }

        [CategoryAttribute("卫星属性")]
        public double OrbitHeight
        {
            get
            {
                return GetSatHeight();
            }
        }

        [CategoryAttribute("卫星属性")]
        public string NoardID
        {
            get
            {
                return m_NoardID;
            }
        }

        [CategoryAttribute("卫星属性")]
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

        [CategoryAttribute("卫星属性")]
        public string CountryName
        {
            get
            {
                if (m_DictProperties.ContainsKey("CountryName"))
                {
                    return Convert.ToString(m_DictProperties["CountryName"]);
                }
                return "";
            }
            set
            {
                m_DictProperties["CountryName"] = value;
            }
        }

        [CategoryAttribute("卫星属性")]
        public System.Drawing.Color DisplayColor
        {
            get
            {
                return m_displayColor;
            }
            set
            {
                m_displayColor = value;
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
        public List<Sensor> SensorList
        {
            get
            {
                return m_sensorlist;
            }
        }
        #endregion

        public virtual Point GetSatPosGeo(DateTime Dt)
        {
            return null;
        }

        public virtual Point GetSatPosEci(DateTime Dt)
        {
            return null;
        }

        protected virtual double GetHeight()
        {
            return -1;
        }

        public double GetSenWidth(double ObsAngleDeg)
        {
            //compute height
            double height = GetHeight();
            if (height < 0)
            {
                return -1;
            }
            ObsAngleDeg = 0.5 * ObsAngleDeg * Math.PI / 180;
            return 2 * R * (Math.Asin(((R + height) / R) * Math.Sin(ObsAngleDeg)) - ObsAngleDeg);
        }

        public double GetSenObsAngle(double Width)
        {
            //compute height
            double height = GetHeight();
            if (height < 0)
            {
                return -1;
            }

            double alpha = 0.5 * Width / R;
            double ObsAngle=2 * Math.Atan(Math.Sin(alpha) / ((R + height) / R - Math.Cos(alpha)));
            ObsAngle *= 180 / Math.PI;

            return Math.Round(ObsAngle, 3);
        }

        /// <summary>
        /// 计算某时间段的数据,存放在临时数据集合中
        /// </summary>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="StepTime"></param>
        public virtual DataTable ComputeTrack(DateTime StartTime, DateTime EndTime, TimeSpan StepTime) { return null; }

        public virtual List<TrackPoint> ComputeTrack2(DateTime StartTime, DateTime EndTime, TimeSpan StepTime) { return null; }

        public virtual int GetCircleCount(DateTime dt)
        {
            return -1;
        }

        #region 载荷对地观测范围
        /// <summary>
        /// 计算载荷的观测范围
        /// </summary>
        /// <param name="sen">载荷</param>
        /// <param name="dt">待计算的时刻</param>
        /// <param name="rx">卫星位置X分量,单位为地球半径</param>
        /// <param name="ry">Y分量</param>
        /// <param name="rz">Z分量</param>
        /// <param name="vx">卫星速度X分量,单位为km/s</param>
        /// <param name="vy">Y分量</param>
        /// <param name="vz">Z分量</param>
        /// <returns>左侧经度,纬度,右侧经度,纬度排列,单位为度</returns>
        public double[] GetSensorPointsBLH(Sensor sen, DateTime dt, double rx, double ry, double rz,
            double vx, double vy, double vz)
        {
            //计算载荷观测范围的ECI坐标
            double[] seneci = GetSensorPointsECI(sen, rx, ry, rz, vx, vy, vz);

            //分离出左右侧点
            double[] pleci = new double[3] { seneci[0], seneci[1], seneci[2] };
            double[] preci = new double[3] { seneci[3], seneci[4], seneci[5] };
            //根据时间将ECI转换成ECR
            double thetaG = TheltaG(dt);
            double[] plecr = RotateZ(pleci[0], pleci[1], pleci[2], -thetaG);
            double[] precr = RotateZ(preci[0], preci[1], preci[2], -thetaG);

            //ECR转换成BLH
            double[] left = ECRtoBL(plecr);
            double[] right = ECRtoBL(precr);

            double[] points = new double[4] { left[0], left[1], right[0], right[1] };
            return points;
        }

        /// <summary>
        /// 计算载荷对地观测左右点的ECI坐标
        /// </summary>
        /// <param name="sen">待计算的载荷</param>
        /// <param name="rx">卫星位置x轴分量,单位为地球半径</param>
        /// <param name="ry">y轴分量</param>
        /// <param name="rz">z轴分量</param>
        /// <param name="vx">卫星速度x轴分量</param>
        /// <param name="vy">y轴分量</param>
        /// <param name="vz">z轴分量</param>
        /// <returns>四个点,顺时针排列,单位为地球半径</returns>
        public double[] GetSensorPointsECI(Sensor sen, double rx, double ry, double rz,
             double vx, double vy, double vz)
        {
            //传感器坐标下的Z轴向量
            //（，，）
            //根据传感器的安装参数和当前侧摆角将Z轴坐标转换成卫星本体坐标系
            //只考虑侧摆方向的安装角，绕X轴旋转，向右为正
            //临时修改，因安装角与计算结果正好相反，故加了个负号
            double[] satz = RotateX(0, 0, 1, -sen.InitAngle * Math.PI / 180);
            //double[] satz = RotateX(0, 0, 1, 0);

            //一些角度
            double SenCurAngle = sen.CurSideAngle * Math.PI / 180;
            //粗略计算载荷的观测角
            double ObsAngle = sen.ObserveAngle > 0 ? sen.ObserveAngle : Math.Atan(sen.Width / (GetSatHeight() * 2));
            double SenHalfAngle = (ObsAngle / 2) * Math.PI / 180;
            //SenHalfAngle = 10 * Math.PI / 180;
            //SenCurAngle = 20 * Math.PI / 180;

            //根据卫星的姿态将卫星本体坐标系转换成轨道坐标系
            //暂时只考虑侧摆，绕X轴旋转，向右侧摆为负
            //载荷Z轴的旋转,向右为负
            double[] satsidez = RotateX(satz[0], satz[1], satz[2], -SenCurAngle);
            //将载荷的Z轴向左右旋转，得出观测的左右侧向量
            double[] sl = RotateX(satsidez[0], satsidez[1], satsidez[2], SenHalfAngle);

            double[] sr = RotateX(satsidez[0], satsidez[1], satsidez[2], -SenHalfAngle);

            //根据卫星的位置和速度将轨道坐标系转换成ECI
            //计算转换矩阵Reo
            double[,] Reo = ComputeReo(rx, ry, rz, vx, vy, vz);

            //用Reo将sl[]和sr[]转换至ECI
            double[] sleci = MulMatrix3p1(Reo, sl);
            double[] sreci = MulMatrix3p1(Reo, sr);

            //观测向量与地球求交
            double[] pleci = IntersectSolution(sleci[0], sleci[1], sleci[2], rx, ry, rz);
            double[] preci = IntersectSolution(sreci[0], sreci[1], sreci[2], rx, ry, rz);

            //设定返回值
            double[] retvalue = new double[6] { pleci[0], pleci[1], pleci[2], preci[0], preci[1], preci[2] };
            return retvalue;
        }

        private double GetSatHeight()
        {
            return this.m_satHeight;
        }

        /// <summary>
        /// ECR坐标转换成BLH坐标
        /// </summary>
        /// <param name="ecr">ECR坐标,单位为地球半径</param>
        /// <returns>经度,纬度,单位为度</returns>
        double[] ECRtoBL(double[] ecr)
        {
            //ECR转换成BLH
            double lon_left = Math.Atan(ecr[1] / ecr[0]) * 180 / Math.PI;
            double lat_left = Math.Atan(ecr[2] / (Math.Sqrt(ecr[0] * ecr[0] + ecr[1] * ecr[1]))) * 180 / Math.PI;

            //经度范围在-180~180之间，而atan的值域为-90~90，因此需要对经度进行调整
            if (ecr[0] < 0)
            {
                lon_left = ecr[1] < 0 ? lon_left - 180 : lon_left + 180;
            }

            double[] blh = new double[2] { lon_left, lat_left };
            return blh;
        }

        /// <summary>
        /// BL坐标转ECR
        /// </summary>
        /// <param name="lon">经度，单位为弧度</param>
        /// <param name="lat">纬度，单位为弧度</param>
        /// <returns>ECR坐标，单位为地球半径</returns>
        double[] BLtoECR(double lon, double lat)
        {
            double x, y, xoy, z;
            z = Math.Sin(lat);
            xoy = Math.Cos(lat);
            x = xoy * Math.Cos(lon);
            y = xoy * Math.Sin(lon);

            double[] result = new double[3] { x, y, z };
            return result;
        }

        /// <summary>
        /// BLH坐标转ECR
        /// </summary>
        /// <param name="lon">经度，单位为度</param>
        /// <param name="lat">纬度，单位为度</param>
        /// <param name="alt">高度，单位为KM</param>
        /// <returns>x,y,z坐标，单位为地球半径</returns>
        double[] BLHtoECR(double lon, double lat, double alt)
        {
            double[] result = BLtoECR(lon * Math.PI / 180, lat * Math.PI / 180);
            double R = 6378.15, scale = 1 + alt / R;
            result[0] *= scale;
            result[1] *= scale;
            result[2] *= scale;
            return result;
        }

        double TheltaG(DateTime dt)
        {
            JulianConvert jul = new JulianConvert();
            return jul.ToGMST(dt.ToUniversalTime());
        }

        /// <summary>
        /// 计算轨道坐标向ECI的转换矩阵
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="ry"></param>
        /// <param name="rz"></param>
        /// <param name="vx"></param>
        /// <param name="vy"></param>
        /// <param name="vz"></param>
        /// <returns></returns>
        double[,] ComputeReo(double rx, double ry, double rz, double vx, double vy, double vz)
        {
            //单位化
            double modr = Math.Sqrt(rx * rx + ry * ry + rz * rz);
            double modv = Math.Sqrt(vx * vx + vy * vy + vz * vz);
            rx /= modr; ry /= modr; rz /= modr;
            vx /= modv; vy /= modv; vz /= modv;

            double[] r = new double[3] { rx, ry, rz };
            double[] v = new double[3] { vx, vy, vz };
            //求H
            double[] h = ChaCheng(r, v);

            //Roe[0]
            double[] Roe0 = ChaCheng(h, r);

            double[,] Roe = new double[3, 3] { {Roe0[0],Roe0[1],Roe0[2]},
                                                {-h[0],-h[1],-h[2]},
                                                {-r[0],-r[1],-r[2]}};
            //求逆
            double[,] Reo = MatrixReverse(Roe);
            return Reo;
        }

        /// <summary>
        /// 求直线与球体的交点
        /// </summary>
        /// <param name="vx">直线的方向向量</param>
        /// <param name="vy"></param>
        /// <param name="vz"></param>
        /// <param name="rx">直线上的一点,单位为球体半径</param>
        /// <param name="ry"></param>
        /// <param name="rz"></param>
        /// <returns></returns>
        double[] IntersectSolution(double vx, double vy, double vz, double rx, double ry, double rz)
        {
            //圆的方程
            //x^2+y^2+z^2=1
            //直线方程
            //x-rx   y-ry   z-rz
            //---- = ---- = ---- = k
            // vx     vy     vz
            //联立之后得如下方程：
            //(vx^2+vy^2+vz^2)k^2 + 2(vx*rx+vy*ry+vz*rz)k + (rx^2+r^2+rz^2-1)=0
            //A=(vx^2+vy^2+vz^2); B=2(vx*rx+vy*ry+vz*rz); C=(rx^2+r^2+rz^2-1)

            double A = vx * vx + vy * vy + vz * vz, B = 2 * (vx * rx + vy * ry + vz * rz),
                C = rx * rx + ry * ry + rz * rz - 1;
            double k, delta = B * B - 4 * A * C;
            if (delta < 0)
            {
                Console.WriteLine("交线方程无解！");
                return null;
            }
            //由于要求出球面上距(rx,ry,rz)点较近的点，因此如果B>0，则解取+号，否则取-号
            if (B > 0)
            {
                k = (-B + Math.Sqrt(delta)) / (2 * A);
                //Console.WriteLine("k取了加号");
            }
            else
            {
                k = (-B - Math.Sqrt(delta)) / (2 * A);
                //Console.WriteLine("k取了减号");
            }

            double[] res = new double[3] { rx + k * vx, ry + k * vy, rz + k * vz };
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sen"></param>
        /// <param name="startpoint">BLH坐标</param>
        /// <param name="endpoint"></param>
        /// <param name="sideangle">单位为度</param>
        /// <param name="Lpoint">BLH坐标</param>
        /// <param name="Rpoint"></param>
        public void ComputeSide(Sensor sen, Point startpoint, Point endpoint, ref Point Lpoint, ref Point Rpoint)
        {
            //根据观测角计算载荷观测范围
            List<List<double>> senpoints = ComputeSenCoverage(sen, startpoint.X, startpoint.Y, startpoint.Z, endpoint.X, endpoint.Y, endpoint.Z);
            Lpoint.X = senpoints[0][0];
            Lpoint.Y = senpoints[0][1];
            Lpoint.Z = 0;//senpoints[0][2];
            Rpoint.X = senpoints[1][0];
            Rpoint.Y = senpoints[1][1];
            Rpoint.Z = 0;//senpoints[1][2];
        }

        /// <summary>
        /// 计算载荷对地观测范围
        /// </summary>
        /// <param name="sen"></param>
        /// <param name="centerx">当前星下点，BLH</param>
        /// <param name="centery"></param>
        /// <param name="centerz"></param>
        /// <param name="nextx">下一个星下点，BLH</param>
        /// <param name="nexty"></param>
        /// <param name="nextz"></param>
        /// <returns>先左后右，BL坐标</returns>
        public List<List<double>> ComputeSenCoverage(Sensor sen,
            double centerx, double centery, double centerz, double nextx, double nexty, double nextz)
        {
            //BLH转成ECR
            double[] center = BLHtoECR(centerx, centery, centerz);// new double[3] { centerx, centery, centerz };
            double[] next = BLHtoECR(nextx, nexty, nextz);// new double[3] { nextx, nexty, nextz };

            double[] v = new double[3] { next[0] - center[0], next[1] - center[1], next[2] - center[2] };
            //根据ECR坐标计算地面观测点的ECR坐标
            double[] senpoints = GetSensorPointsECI(sen, center[0], center[1], center[2], v[0], v[1], v[2]);
            double[] lpoints = { senpoints[0], senpoints[1], senpoints[2] };
            double[] rpoints = { senpoints[3], senpoints[4], senpoints[5] };

            double[] lpointsBL = ECRtoBL(lpoints);
            double[] rpointsBL = ECRtoBL(rpoints);

            List<List<double>> points = new List<List<double>>();
            List<double> p0 = new List<double>();
            p0.Add(lpointsBL[0]);
            p0.Add(lpointsBL[1]);
            points.Add(p0);

            p0 = new List<double>();
            p0.Add(rpointsBL[0]);
            p0.Add(rpointsBL[1]);
            points.Add(p0);

            return points;
        }
        #endregion

        #region 三维矩阵操作
        /// <summary>
        /// 矩阵相乘，*3矩阵乘以*1矩阵
        /// </summary>
        /// <param name="mx">3*3矩阵</param>
        /// <param name="my">3*1矩阵</param>
        /// <returns></returns>
        double[] MulMatrix3p1(double[,] mx, double[] my)
        {
            if (mx.Rank != 2 || mx.GetLength(0) != 3 || my.Length != 3)
            {
                Console.WriteLine("矩阵相乘出错！维数不对");
                return null;
            }
            double[] mres = new double[3];
            mres[0] = mx[0, 0] * my[0] + mx[0, 1] * my[1] + mx[0, 2] * my[2];
            mres[1] = mx[1, 0] * my[0] + mx[1, 1] * my[1] + mx[1, 2] * my[2];
            mres[2] = mx[2, 0] * my[0] + mx[2, 1] * my[1] + mx[2, 2] * my[2];

            return mres;
        }

        /// <summary>
        /// 两个*3的矩阵相乘
        /// </summary>
        /// <param name="mx">必须为*3形式</param>
        /// <param name="my">必须为*3形式</param>
        /// <returns></returns>
        double[,] MulMatrix3p3(double[,] mx, double[,] my)
        {
            if (mx.Rank != 2 || mx.GetLength(0) != 3 || my.Rank != 2 || my.GetLength(0) != 3)
            {
                Console.WriteLine("矩阵相乘出错！矩阵不是*3形式");
                return null;
            }
            double[,] mres = new double[3, 3];
            mres[0, 0] = mx[0, 0] * my[0, 0] + mx[0, 1] * my[1, 0] + mx[0, 2] * my[2, 0];
            mres[0, 1] = mx[0, 0] * my[0, 1] + mx[0, 1] * my[1, 1] + mx[0, 2] * my[2, 1];
            mres[0, 2] = mx[0, 0] * my[0, 2] + mx[0, 1] * my[1, 2] + mx[0, 2] * my[2, 2];
            mres[1, 0] = mx[1, 0] * my[0, 0] + mx[1, 1] * my[1, 0] + mx[1, 2] * my[2, 0];
            mres[1, 1] = mx[1, 0] * my[0, 1] + mx[1, 1] * my[1, 1] + mx[1, 2] * my[2, 1];
            mres[1, 2] = mx[1, 0] * my[0, 2] + mx[1, 1] * my[1, 2] + mx[1, 2] * my[2, 2];
            mres[2, 0] = mx[2, 0] * my[0, 0] + mx[2, 1] * my[1, 0] + mx[2, 2] * my[2, 0];
            mres[2, 1] = mx[2, 0] * my[0, 1] + mx[2, 1] * my[1, 1] + mx[2, 2] * my[2, 1];
            mres[2, 2] = mx[2, 0] * my[0, 2] + mx[2, 1] * my[1, 2] + mx[2, 2] * my[2, 2];

            return mres;
        }

        /// <summary>
        /// 3*3的向量叉乘
        /// </summary>
        /// <param name="v1">长度必须为</param>
        /// <param name="v2">长度必须为</param>
        /// <returns></returns>
        double[] ChaCheng(double[] v1, double[] v2)
        {
            if (v1.Length != 3 || v2.Length != 3)
            {
                Console.WriteLine("向量叉乘出错！不是三维向量相乘");
                return null;
            }
            double i, j, k;
            i = v1[1] * v2[2] - v1[2] * v2[1];
            j = v1[2] * v2[0] - v1[0] * v2[2];
            k = v1[0] * v2[1] - v1[1] * v2[0];

            double[] res = { i, j, k };
            return res;
        }

        /// <summary>
        /// 三维向量求模
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        double MOD3(double[] v)
        {
            if (v.Length != 3)
            {
                Console.WriteLine("向量求模出错！不是三维向量");
                return -1;
            }
            return Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
        }

        double MOD3p3(double[,] m)
        {
            if (m.Rank != 2 || m.GetLength(0) != 3)
            {
                Console.WriteLine("矩阵求模出错！不是*3矩阵");
                return -1;
            }
            return m[0, 0] * m[1, 1] * m[2, 2] +
                           m[1, 0] * m[2, 1] * m[0, 2] +
                           m[2, 0] * m[1, 2] * m[0, 1] -
                       m[0, 2] * m[1, 1] * m[2, 0] -
                       m[1, 2] * m[2, 1] * m[0, 0] -
                       m[2, 2] * m[1, 0] * m[0, 1];

        }

        /// <summary>
        /// 三维矩阵求逆
        /// </summary>
        /// <param name="m">3*3矩阵，模不为</param>
        /// <returns></returns>
        double[,] MatrixReverse(double[,] m)
        {
            if (m.Rank != 2 || m.GetLength(0) != 3)
            {
                Console.WriteLine("矩阵求逆出错！不是*3矩阵");
                return null;
            }

            double mod = MOD3p3(m);
            double[,] yu = Yuzishi(m);

            if (mod == 0)
            {
                Console.WriteLine("矩阵的模为，无法求逆!");
                return null;
            }

            for (int i = 0; i < yu.Rank; i++)
            {
                for (int j = 0; j < yu.GetLength(i); j++)
                {
                    yu[i, j] /= mod;
                }
            }
            return yu;

        }

        /// <summary>
        /// 求*3矩阵的余子式
        /// </summary>
        /// <param name="array">3*3矩阵</param>
        /// <returns></returns>
        double[,] Yuzishi(double[,] array)
        {
            double y00, y01, y02, y10, y11, y12, y20, y21, y22;
            //00
            y00 = array[1, 1] * array[2, 2] - array[1, 2] * array[2, 1];
            //01 -
            y01 = -(array[1, 0] * array[2, 2] - array[1, 2] * array[2, 0]);
            //02
            y02 = array[1, 0] * array[2, 1] - array[2, 0] * array[1, 1];
            //10 -
            y10 = -(array[0, 1] * array[2, 2] - array[0, 2] * array[2, 1]);
            //11
            y11 = array[0, 0] * array[2, 2] - array[0, 2] * array[2, 0];
            //12 -
            y12 = -(array[0, 0] * array[2, 1] - array[0, 1] * array[2, 0]);
            //20
            y20 = array[0, 1] * array[1, 2] - array[0, 2] * array[1, 1];
            //21 -
            y21 = -(array[0, 0] * array[1, 2] - array[0, 2] * array[1, 0]);
            //22
            y22 = array[0, 0] * array[1, 1] - array[0, 1] * array[1, 0];

            //double[,] yu = new double[3, 3] { { y00, y01, y02 }, { y10, y11, y12 }, { y20, y21, y22 } };
            double[,] yu = new double[3, 3] { { y00, y10, y20 }, { y01, y11, y21 }, { y02, y12, y22 } };

            return yu;
        }

        #endregion

        #region 坐标旋转

        /// <summary>
        /// 三维坐标绕X轴旋转
        /// </summary>
        /// <param name="x0">原x坐标</param>
        /// <param name="y0">原y坐标</param>
        /// <param name="z0">原z坐标</param>
        /// <param name="AngleRad">逆时针旋转角度，弧度</param>
        /// <returns></returns>
        double[] RotateX(double x0, double y0, double z0, double AngleRad)
        {
            double[] m0 = new double[3] { x0, y0, z0 };
            double[,] mRx = new double[3, 3] {{1,0,0},
                                            {0,Math.Cos(AngleRad),-Math.Sin(AngleRad)},
                                            {0,Math.Sin(AngleRad),Math.Cos(AngleRad)}};
            return MulMatrix3p1(mRx, m0);
        }

        /// <summary>
        /// 三维坐标绕Y轴旋转
        /// </summary>
        /// <param name="x0">原x坐标</param>
        /// <param name="y0">原y坐标</param>
        /// <param name="z0">原z坐标</param>
        /// <param name="AngleRad">逆时针旋转角度，弧度</param>
        /// <returns></returns>
        double[] RotateY(double x0, double y0, double z0, double AngleRad)
        {
            double[] m0 = new double[3] { x0, y0, z0 };
            double[,] mRy = new double[3, 3] {{Math.Cos(AngleRad),0,Math.Sin(AngleRad)},
                                              {0,1,0},
                                              {-Math.Sin(AngleRad),0,Math.Cos(AngleRad)}};
            return MulMatrix3p1(mRy, m0);
        }

        /// <summary>
        /// 三维坐标绕z轴旋转
        /// </summary>
        /// <param name="x0">原x坐标</param>
        /// <param name="y0">原y坐标</param>
        /// <param name="z0">原z坐标</param>
        /// <param name="AngleRad">逆时针旋转角度，弧度</param>
        /// <returns></returns>
        double[] RotateZ(double x0, double y0, double z0, double AngleRad)
        {
            double[] m0 = new double[3] { x0, y0, z0 };
            double[,] mRz = new double[3, 3] {{Math.Cos(AngleRad),-Math.Sin(AngleRad),0},
                                            {Math.Sin(AngleRad),Math.Cos(AngleRad),0},
                                            {0,0,1}};
            return MulMatrix3p1(mRz, m0);
        }

        #endregion

        /// <summary>
        /// 计算与180度经线的交点
        /// </summary>
        /// <param name="p1">起始点</param>
        /// <param name="p2">终止点</param>
        /// <param name="lftp">与-180度的交点</param>
        /// <param name="rgt">与180度的交点</param>
        protected void GetEdgePoints(Point p1, Point p2, out Point lftp, out Point rgtp)
        {
            if (p1.X - p2.X < -180)
            {
                //过-180度经线
                double k = Math.Atan((p2.Y - p1.Y) / (p2.X - p1.X - 360));
                lftp = new Point(-180, p1.Y - k * (p1.X + 180));
                rgtp = new Point(180, lftp.Y);
            }
            else if (p1.X - p2.X > 180)
            {
                //过180度经线
                double k = Math.Atan((p2.Y - p1.Y) / (p2.X - p1.X + 360));

                lftp = new Point(-180, p1.Y - k * (p1.X - 180));
                rgtp = new Point(180, lftp.Y);
            }
            else
            {
                lftp = null;
                rgtp = null;
            }
        }

        void GetEnvelope(List<Point> Geo, out double Xmax, out double Xmin, out double Ymax, out double Ymin)
        {
            Xmax = -181;
            Xmin = 181;
            Ymax = -91;
            Ymin = 91;
            foreach (Point mp in Geo)
            {
                Xmax = mp.X > Xmax ? mp.X : Xmax;
                Xmin = mp.X < Xmin ? mp.X : Xmin;
                Ymax = mp.Y > Ymax ? mp.Y : Ymax;
                Ymin = mp.Y < Ymin ? mp.Y : Ymin;
            }
        }
        #region 计算过境时间窗口
        /// <summary>
        /// 获取星下点轨迹在扩大后的目标区域内的部分
        /// </summary>
        /// <param name="dtTrack">星下点轨迹，经纬度</param>
        /// <param name="pTargetArea">目标区域，会自动扩大</param>
        /// <returns></returns>
        public DataTable GetTrackInAreaTime(DataTable dtTrack, List<Point> pTargetArea)
        {
            DataTable dtTrackInAreaTime = new DataTable();
            dtTrackInAreaTime.Columns.Add("orderid", typeof(string));
            dtTrackInAreaTime.Columns.Add("TimeIn", typeof(DateTime));
            dtTrackInAreaTime.Columns.Add("TimeOut", typeof(DateTime));
            dtTrackInAreaTime.Columns.Add("CircleCount", typeof(int));
            //扩大目标区域
            //IObjectCopy objcopy = new ObjectCopyClass();
            //IEnvelope pEnv = objcopy.Copy(pTargetArea.Envelope) as IEnvelope;
            //pEnv.Expand(5, 3, false);
            double EnvXmax, EnvXmin, EnvYmax, EnvYmin;
            GetEnvelope(pTargetArea, out EnvXmax, out EnvXmin, out EnvYmax, out EnvYmin);
            EnvXmax += 5;
            EnvXmin -= 5;
            EnvYmax += 3;
            EnvYmin -= 3;

            EnvXmax = EnvXmax > 180 ? 180 : EnvXmax;
            EnvXmin = EnvXmin < -180 ? 180 : EnvXmin;
            EnvYmax = EnvYmax > 90 ? 90 : EnvYmax;
            EnvYmin = EnvYmin < -90 ? -90 : EnvYmin;

            if (dtTrack == null || dtTrack.Rows.Count < 2)
            {
                return null;
            }

            bool inarea = false;

            for (int i = 1; i < dtTrack.Rows.Count; i++)
            {
                DataRow PreRow = dtTrack.Rows[i - 1];
                DataRow CurRow = dtTrack.Rows[i];

                Point p0 = new Point(Convert.ToDouble(PreRow["Lon"]), Convert.ToDouble(PreRow["Lat"]));
                Point p1 = new Point(Convert.ToDouble(CurRow["Lon"]), Convert.ToDouble(CurRow["Lat"]));

                //如果与目标区域的边框相交
                if (m_RelOpera.LineInsectEnvelope(p0, p1, EnvXmax, EnvXmin, EnvYmax, EnvYmin))
                {
                    if (!inarea)
                    {
                        //进入目标区域,记录上一个点的时间
                        DataRow dr = dtTrackInAreaTime.NewRow();
                        dr["orderid"] = "1";
                        dr["TimeIn"] = PreRow["Time"];

                        dtTrackInAreaTime.Rows.Add(dr);

                        inarea = true;
                        //Console.WriteLine("path in area, time: " + CurRow["time"].ToString());
                        //Console.WriteLine(p0.X.ToString() + "  " + p0.Y.ToString());
                    }
                }
                else
                {
                    if (inarea)
                    {
                        //离开目标区域,多记录一个点，方便后面计算扫描边线
                        DataRow lastrow = dtTrackInAreaTime.Rows[dtTrackInAreaTime.Rows.Count - 1];
                        lastrow["TimeOut"] = CurRow["Time"];
                        inarea = false;
                        //飞行圈数
                        lastrow["CircleCount"] = GetCircleCount(Convert.ToDateTime(lastrow["TimeIn"]));
                        //Console.WriteLine("path out area, time: " + CurRow["time"].ToString());
                    }
                }
            }
            return dtTrackInAreaTime;
        }

        /// <summary>
        /// 得出进出目标区域时间的精确值
        /// </summary>
        /// <param name="dtPath">扫描边线计算结果</param>
        /// <param name="pGeo">目标区域</param>
        /// <returns></returns>
        List<CRegion> GetPrecisePaths(Sensor sen, List<DataTable> dtPathList, DataTable dtTrackTime, List<Point> pGeo)
        {
            string timeformate = "yyyy-MM-dd HH:mm:ss.fff";
            TimeSpan tp = new TimeSpan(0, 0, 0, 1);
            TimeSpan shorttp = new TimeSpan(0, 0, 0, 0, 100);
            TimeSpan tinytp = new TimeSpan(0, 0, 0, 0, 10);
            List<CRegion> regionlist = new List<CRegion>();
            //IPointCollection pathgeo = new PolygonClass();

            foreach (DataTable dt in dtPathList)
            {
                if (dt != null && dt.Rows.Count >= 2)
                {
                    //细化进入时间
                    //取出四个点用于插值
                    DataRow[] drsIn = new DataRow[4];
                    if (dt.Rows.Count < 4)
                    {
                        drsIn = new DataRow[dt.Rows.Count];
                    }
                    for (int i = 0; i < drsIn.Length; i++)
                    {
                        drsIn[i] = dt.Rows[i];
                    }
                    object[] InRow = GetPreciseTime(drsIn, pGeo, tp, true);
                    //细化离开时间
                    //取出四个点用于插值
                    DataRow[] drsOut = new DataRow[4];
                    if (dt.Rows.Count < 4)
                    {
                        drsOut = new DataRow[dt.Rows.Count];
                    }
                    for (int i = 0; i < drsOut.Length; i++)
                    {
                        drsOut[drsOut.Length - 1 - i] = dt.Rows[dt.Rows.Count - 1 - i];
                    }

                    object[] OutRow = GetPreciseTime(drsOut, pGeo, tp, false);

                    bool OutAdded = false;
                    DateTime pstarttime = Convert.ToDateTime(InRow[1]), pendtime = Convert.ToDateTime(OutRow[1]);
                    DataTable dtNewPath = dt.Clone();
                    //添加开始时间
                    dtNewPath.Rows.Add(dtNewPath.NewRow());
                    dtNewPath.Rows[0].ItemArray = InRow;

                    foreach (DataRow dr in dt.Rows)
                    {
                        DateTime curtime = Convert.ToDateTime(dr["TIME"]);
                        if (curtime > pstarttime && curtime < pendtime)
                        {
                            //正常添加
                            dtNewPath.ImportRow(dr);
                        }

                        if (curtime > pendtime && !OutAdded)
                        {
                            //大于出区域时间，且出区域时间没有添加
                            dtNewPath.Rows.Add(dtNewPath.NewRow());
                            dtNewPath.Rows[dtNewPath.Rows.Count - 1].ItemArray = OutRow;
                            OutAdded = true;
                            break;
                        }

                    }

                    CRegion cr = new CRegion();
                    cr.SenName = sen.SenName;
                    cr.SatName = sen.SatName;
                    cr.StartTime = Convert.ToDateTime(dtNewPath.Rows[0]["TIME"]);
                    cr.EndTime = Convert.ToDateTime(dtNewPath.Rows[dtNewPath.Rows.Count - 1]["TIME"]);
                    cr.Width = sen.Width;
                    cr.resolution = sen.Resolution;
                    cr.SideAngle = sen.CurSideAngle;
                    cr.RegionColor = sen.SenColor;

                    List<Point> pcoll = new List<Point>();
                    List<Point> leftlist = new List<Point>();
                    List<Point> rightlist = new List<Point>();
                    foreach (DataRow dr in dtNewPath.Rows)
                    {
                        Point leftp = new Point(Convert.ToDouble(dr["LEFTLON"]), Convert.ToDouble(dr["LEFTLAT"]));
                        Point rightp = new Point(Convert.ToDouble(dr["RIGHTLON"]), Convert.ToDouble(dr["RIGHTLAT"]));
                        leftlist.Add(leftp);
                        rightlist.Add(rightp);
                    }
                    for (int i = 0; i < leftlist.Count; i++)
                    {
                        pcoll.Add(leftlist[i]);
                    }
                    for (int i = rightlist.Count - 1; i >= 0; i--)
                    {
                        pcoll.Add(rightlist[i]);
                    }
                    cr.pGeometry = pcoll;
                    //计算太阳高度角
                    double centerX = 0, centerY = 0, XMax, XMin, YMax, YMin;
                    GetEnvelope(pcoll, out XMax, out XMin, out YMax, out YMin);
                    centerX = (XMax + XMin) / 2;
                    centerY = (YMax + YMin) / 2;

                    cr.SunAngle = GetSolarAngle(cr.StartTime.ToUniversalTime(), centerX, centerY);
                    cr.SunAngle = Math.Round(cr.SunAngle, 3);

                    //圈数
                    DataRow[] drs = dtTrackTime.Select("timein <= '" + cr.StartTime.ToString() + "' and timeout >= '" + cr.StartTime.ToString() + "'");
                    if (drs.Length != 0)
                    {
                        cr.CircleCount = Convert.ToInt16(drs[0]["circlecount"]);
                    }
                    regionlist.Add(cr);
                }
            }

            return regionlist;
        }

        protected object[] GetPreciseTime(DataRow[] drTime, List<Point> pGeo, TimeSpan minStep, bool InAreaTime)
        {
            if (drTime.Length != 4)
            {
                Console.WriteLine("插值必须有四个点！");
                return null;
            }

            //转存
            DataRow[] drT = new DataRow[4];
            for (int i = 0; i < drTime.Length; i++)
            {
                drT[i] = drTime[i];
            }

            DateTime[] dts = new DateTime[4];
            double[,] points = new double[4, 4];

            for (int i = 0; i < 4; i++)
            {
                DataRow dr = drT[i];
                dts[i] = Convert.ToDateTime(dr["TIME"]);
                points[i, 0] = Convert.ToDouble(dr["LEFTLON"]);
                points[i, 1] = Convert.ToDouble(dr["LEFTLAT"]);
                points[i, 2] = Convert.ToDouble(dr["RIGHTLON"]);
                points[i, 3] = Convert.ToDouble(dr["RIGHTLAT"]);
            }

            //开始点和结束点
            Point LeftStartPoint = new Point(points[0, 0], points[0, 1]);
            Point RightStartPoint = new Point(points[0, 2], points[0, 3]);

            DateTime dt = dts[0];
            double minSecs = minStep.TotalSeconds;

            //开始的扫描线是否与目标区域相交
            bool StartLineIntersect = m_RelOpera.LineInsectArea(LeftStartPoint, RightStartPoint, pGeo.ToArray());
            if (StartLineIntersect && InAreaTime)
            {
                Console.WriteLine("第一点就与目标区域相交了，无法插值获取进入时间！");
                return null;
            }
            double TotalStep = (dts[3] - dts[0]).TotalSeconds;
            double MinStepSec = minStep.TotalSeconds;
            double[] tmpPoint = new double[4];// leftLon, leftLat, rightLon, rightLat;
            double lastleftlon, lastleftlat, lastrightlon, lastrightlat;
            lastleftlon = LeftStartPoint.X; lastleftlat = LeftStartPoint.Y;
            lastrightlon = RightStartPoint.X; lastrightlat = RightStartPoint.Y;
            DateTime tmpTime = dts[0];

            for (double CurStep = MinStepSec; CurStep <= TotalStep; CurStep += MinStepSec)
            {
                //插值得出中间点的扫描边线点
                for (int i = 0; i < 4; i++)
                {
                    tmpPoint[i] = Interpolate(CurStep, points[0, i],
                        points[1, i], points[2, i], points[3, i]);
                }
                //判断
                Point tmpLeft = new Point(tmpPoint[0], tmpPoint[1]);
                Point tmpRight = new Point(tmpPoint[2], tmpPoint[3]);
                Point tmpLastLeft = new Point(lastleftlon, lastleftlat);
                Point tmpLastRight = new Point(lastrightlon, lastrightlat);

                Point[] tmpGeo = new Point[] { tmpLeft, tmpRight, tmpLastRight, tmpLastLeft, tmpLeft };
                bool CurIntersect = m_RelOpera.AreaInsectArea(new List<Point>(tmpGeo), pGeo);

                if (CurIntersect != StartLineIntersect)
                {
                    //达到目标了
                    if (InAreaTime)
                    {
                        break;
                    }
                    else
                    {
                        lastleftlon = tmpLeft.X; lastleftlat = tmpLeft.Y;
                        lastrightlon = tmpRight.X; lastrightlat = tmpRight.Y;
                        tmpTime = dts[0].AddSeconds(CurStep);
                        if (StartLineIntersect)
                        {
                            break;
                        }
                        else
                        {
                            StartLineIntersect = true;
                        }
                    }
                }

                //缓存点坐标
                lastleftlon = tmpLeft.X; lastleftlat = tmpLeft.Y;
                lastrightlon = tmpRight.X; lastrightlat = tmpRight.Y;
                tmpTime = dts[0].AddSeconds(CurStep);
            }

            if (drTime.Length > 0)
            {
                object[] ResultArray = new object[drTime[0].ItemArray.Length];

                ResultArray[0] = drTime[0][0];
                ResultArray[1] = tmpTime;
                ResultArray[2] = lastleftlon;
                ResultArray[3] = lastleftlat;
                ResultArray[4] = lastrightlon;
                ResultArray[5] = lastrightlat;

                return ResultArray;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 插值函数，y0,y1,y2,y3之间相隔秒，插值结果在y1和y2之间
        /// </summary>
        /// <param name="u">待插值的自变量值</param>
        /// <param name="y0"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <param name="y3"></param>
        /// <returns>插值结果</returns>
        double Interpolate(double u, double y0, double y1, double y2, double y3)
        {
            //double step = 20;//各点之间的间隔
            double l0, l1, l2, l3;//插值基函数
            double result;//插值结果

            l0 = -((u - 20) * (u - 40) * (u - 60)) / 48000;
            l1 = (u * (u - 40) * (u - 60)) / 16000;
            l2 = -(u * (u - 20) * (u - 60)) / 16000;
            l3 = (u * (u - 20) * (u - 40)) / 48000;

            result = l0 * y0 + l1 * y1 + l2 * y2 + l3 * y3;

            return result;
        }

        /// <summary>
        /// 将过境时间窗口分离成扫描带
        /// </summary>
        /// <param name="dtPath">所有过境时间</param>
        /// <param name="NormalSpan">轨道计算的时间间隔</param>
        /// <returns>扫描带的进出时间集合</returns>
        protected List<DataTable> DivPaths(DataTable dtPath, TimeSpan NormalSpan)
        {
            List<DataTable> dtPathList = new List<DataTable>();
            DataTable tmpDt = dtPathResult.Clone();
            for (int i = 0; i < dtPath.Rows.Count - 1; i++)
            {
                DataRow curRow = dtPath.Rows[i];
                DataRow nextRow = dtPath.Rows[i + 1];
                string curSen = curRow["SENNAME"].ToString(), nextSen = nextRow["SENNAME"].ToString();
                DateTime curTime = Convert.ToDateTime(curRow["TIME"]);
                DateTime nextTime = Convert.ToDateTime(nextRow["TIME"]);

                //加到临时表里
                tmpDt.ImportRow(curRow);
                //判断临时表是否需要更新
                if (!(curSen == nextSen && nextTime - curTime <= NormalSpan))
                {
                    dtPathList.Add(tmpDt);
                    tmpDt = new DataTable();
                    tmpDt = dtPathResult.Clone();
                }
            }
            //加上最后一个
            dtPathList.Add(tmpDt);
            return dtPathList;
        }

        /// <summary>
        /// 获取扫描带与目标区域相交的部分
        /// </summary>
        /// <param name="dtPath">扫描带</param>
        /// <param name="pGeo">目标区域</param>
        /// <returns>扫描带的一部分</returns>
        protected DataTable GetInsectPaths(DataTable dtPath, List<Point> pGeo)
        {
            //指示是否开始记录
            bool record = false;
            int firstindex = -1, lastindex = -1;
            DataTable dtInsectPaths = dtPathResult.Clone();
            for (int i = 1; i < dtPath.Rows.Count; i++)
            {
                DataRow drPrev = dtPath.Rows[i - 1];
                DataRow drCur = dtPath.Rows[i];

                Point p0 = new Point(Convert.ToDouble(drCur["LEFTLON"]), Convert.ToDouble(drCur["LEFTLAT"]));
                Point p1 = new Point(Convert.ToDouble(drCur["RIGHTLON"]), Convert.ToDouble(drCur["RIGHTLAT"]));

                bool Insected = m_RelOpera.LineInsectArea(p0, p1, pGeo.ToArray());

                if (Insected)
                {
                    //左侧点与右侧点的连线与目标区域相交,记录前一点
                    dtInsectPaths.ImportRow(drPrev);
                    record = true;

                    firstindex = firstindex == -1 ? i : firstindex;
                }
                else
                {
                    if (record)
                    {
                        //不再相交，但仍为记录状态，记录前一点和当前点
                        dtInsectPaths.ImportRow(drPrev);
                        dtInsectPaths.ImportRow(drCur);
                        record = false;

                        lastindex = lastindex == -1 ? i : lastindex;
                    }
                }
            }
            //结束循环时，如果还是记录状态，记录最后一个点
            if (record)
            {
                dtInsectPaths.ImportRow(dtPath.Rows[dtPath.Rows.Count - 1]);

                lastindex = lastindex == -1 ? dtPath.Rows.Count - 1 : lastindex;
            }

            //补齐四个点用于插值
            if (dtInsectPaths.Rows.Count < 4 && dtInsectPaths.Rows.Count > 0)
            {
                //int firstindex = dtPath.Rows.IndexOf(dtInsectPaths.Rows[0]);
                //int lastindex = dtPath.Rows.IndexOf(dtInsectPaths.Rows[dtInsectPaths.Rows.Count - 1]);
                int lackcount = 4 - dtInsectPaths.Rows.Count;
                int allcount = dtPath.Rows.Count;

                if ((lastindex + lackcount) < allcount)
                {
                    //在最大索引之内,加上之后的点
                    for (int i = 0; i < lackcount; i++)
                    {
                        dtInsectPaths.ImportRow(dtPath.Rows[lastindex + i + 1]);
                    }
                    return dtInsectPaths;
                }
                else
                {
                    //加上之前的点
                    DataTable dtRestPath = dtInsectPaths.Clone();
                    for (int i = 0; i < lackcount; i++)
                    {
                        dtRestPath.ImportRow(dtPath.Rows[firstindex - i - 1]);
                    }
                    //集中所有的点
                    for (int i = 0; i < dtInsectPaths.Rows.Count; i++)
                    {
                        dtRestPath.ImportRow(dtInsectPaths.Rows[i]);
                    }
                    return dtRestPath;
                }
            }
            else
            {
                return dtInsectPaths;
            }
        }

        List<DataTable> GetInsectPaths(List<DataTable> dtRoughPathList, List<Point> pTargetArea)
        {
            List<DataTable> dtPathList = new List<DataTable>();
            foreach (DataTable dt in dtRoughPathList)
            {
                DataTable dtPath = GetInsectPaths(dt, pTargetArea);
                //WriteDt(dtPath, "intersectpath.txt", true);
                if (dtPath != null && dtPath.Rows.Count != 0)
                {
                    dtPathList.Add(dtPath);
                }
            }
            return dtPathList;
        }

        /// <summary>
        /// 根据轨迹计算扫描边线
        /// </summary>
        /// <param name="sen">载荷</param>
        /// <param name="dtTrack">轨迹表</param>
        /// <param name="dtPathTime">待计算的时间段</param>
        /// <returns>包含载荷名，时间，左侧和右侧点经纬度的表格</returns>
        public DataTable ComputePaths(Sensor sen, DataTable dtTrack, DataTable dtPathTime)
        {
            DataTable dtPath = dtPathResult.Clone();

            //对每个时间段进行计算
            for (int i = 0; i < dtPathTime.Rows.Count; i++)
            {
                DataRow drTime = dtPathTime.Rows[i];
                //获取每个时间段内的卫星轨迹数据
                DataRow[] drTracks = dtTrack.Select("time >= '" + drTime["TIMEIN"] + "' and time <= '" + drTime["TIMEOUT"] + "'"); ;

                if (drTracks != null && drTracks.Length >= 2)
                {
                    DataRow drCurPoint, drNextPoint;
                    double curLon, curLat, curAlt, nextLon, nextLat, nextAlt;
                    List<List<double>> tmpPath;
                    DataRow dr;
                    for (int j = 0; j < drTracks.Length - 1; j++)
                    {
                        drCurPoint = drTracks[j];
                        drNextPoint = drTracks[j + 1];
                        //根据当前点和下一点计算当前点的扫描边线点
                        curLon = Convert.ToDouble(drCurPoint["Lon"]);
                        curLat = Convert.ToDouble(drCurPoint["Lat"]);
                        curAlt = Convert.ToDouble(drCurPoint["Alt"]);
                        nextLon = Convert.ToDouble(drNextPoint["Lon"]);
                        nextLat = Convert.ToDouble(drNextPoint["Lat"]);
                        nextAlt = Convert.ToDouble(drNextPoint["Alt"]);

                        tmpPath = ComputeSenCoverage(sen, curLon, curLat, curAlt, nextLon, nextLat, nextAlt);
                        dr = dtPath.NewRow();
                        dr["SENNAME"] = sen.SenName;
                        dr["TIME"] = drCurPoint["TIME"];
                        dr["LEFTLON"] = tmpPath[0][0];
                        dr["LEFTLAT"] = tmpPath[0][1];
                        dr["RIGHTLON"] = tmpPath[1][0];
                        dr["RIGHTLAT"] = tmpPath[1][1];

                        dtPath.Rows.Add(dr);
                    }
                }
            }
            return dtPath;
        }

        /// <summary>
        /// 计算过境时间窗口
        /// </summary>
        /// <param name="sen">载荷</param>
        /// <param name="pTargetArea">目标区域</param>
        /// <param name="dtTrack">星下点轨迹</param>
        /// <param name="dtTrackInRegionTime">星下点轨迹在目标区域内的部分,包含进出时间和圈数</param>
        /// <param name="TrackSpan">星下点轨迹的时间间隔</param>
        /// <returns></returns>
        public List<CRegion> GetPathsInRegion(Sensor sen, List<Point> pTargetArea, DataTable dtTrack,
            DataTable dtTrackInRegionTime, TimeSpan TrackSpan)
        {
            //计算载荷的扫描范围
            DataTable dtRoughPath = ComputePaths(sen, dtTrack, dtTrackInRegionTime);
            //分离成扫描带
            List<DataTable> dtRoughPathList = DivPaths(dtRoughPath, TrackSpan);
            //WriteDt(dtRoughPathList, "afterdiv.txt");
            //与目标区域求交
            List<DataTable> dtPathList = GetInsectPaths(dtRoughPathList, pTargetArea);
            //WriteDt(dtPathList, "afterintersect.txt");
            //细化计算结果，生成CRegion形式的变量
            List<CRegion> RegionList = GetPrecisePaths(sen, dtPathList, dtTrackInRegionTime, pTargetArea);

            return RegionList;
        }

        #endregion

        #region NewMethod
        public List<CRegion> ComputePaths(DateTime starttime, DateTime stoptime, List<TargetArea> AreaList, PathMode pm)
        {
            List<CRegion> regionlist = new List<CRegion>();
            //计算卫星轨迹
            List<TrackPoint> TrackList = ComputeTrack2(starttime, stoptime, m_TrackTp);
            foreach (TargetArea ta in AreaList)
            {
                //外接矩形
                double Xmax, Xmin, Ymax, Ymin;
                GetEnvelope(ta.Geometry, out Xmax, out Xmin, out Ymax, out Ymin);

                double LXmax, LXmin, LYmax, LYmin;
                LXmax = Xmax + 10;
                LXmin = Xmin - 10;
                LYmax = Ymax + 5;
                LYmin = Ymin - 5;

                //外接矩形中的轨迹点
                List<List<TrackPoint>> RoughTrackList = GetInsidePoints(TrackList, LXmax, LXmin, LYmax, LYmin);

                //算观测范围
                List<List<StripUnit>> RoughStripUnitList = new List<List<StripUnit>>();
                foreach (Sensor sen in SensorList)
                {
                    if (sen.Checked)
                    {
                        //计算每个扫描带单元
                        foreach (List<TrackPoint> tpList in RoughTrackList)
                        {
                            RoughStripUnitList.Add(GetStripUnitList(this, sen, tpList));
                        }
                    }
                }

                //对小四边形和目标区域进行相交判断
                //先判断外接矩形
                List<List<StripUnit>> NormalStripUnitList = GetInterSectEnvPoints(RoughStripUnitList, Xmax, Xmin, Ymax, Ymin);
                //再判断具体区域
                List<List<StripUnit>> StripUnitList = GetInterSectPoints(NormalStripUnitList, ta.Geometry);

                //将小四边形串起来，形成扫描带
                List<CRegion> StripList = GetStripList(StripUnitList);
                //set areaid
                foreach (CRegion  cr in StripList )
                {
                    cr.AreaID = ta.ID;
                }
                regionlist.AddRange(StripList);
            }

            //给Region编号
            foreach (CRegion cr in regionlist)
            {
                cr.PathID = cr.SatName + cr.SenName + regionlist.IndexOf(cr);
            }
            return regionlist;
        }

        /// <summary>
        /// 获取与Area相交的部分
        /// </summary>
        /// <param name="NormalStripUnitList"></param>
        /// <param name="Area"></param>
        /// <returns></returns>
        List<List<StripUnit>> GetInterSectPoints(List<List<StripUnit>> NormalStripUnitList, List<Point> Area)
        {
            List<List<StripUnit>> ResList = new List<List<StripUnit>>();
            foreach (List<StripUnit> StripUnitList in NormalStripUnitList)
            {
                List<StripUnit> ResUnitList = new List<StripUnit>();
                //判断小扫描带是否与目标区域相交
                foreach (StripUnit su in StripUnitList)
                {
                    if (PolygonIntersects(su.StripPoints, Area))
                    {
                        ResUnitList.Add(su);
                    }
                }

                //保存结果
                if (ResUnitList.Count != 0)
                {
                    ResList.Add(ResUnitList);
                }
            }
            return ResList;
        }

        /// <summary>
        /// 获取在某个矩形之内的轨迹点
        /// </summary>
        /// <param name="RoughStripUnitList"></param>
        /// <param name="Xmax"></param>
        /// <param name="Xmin"></param>
        /// <param name="Ymax"></param>
        /// <param name="Ymin"></param>
        /// <returns></returns>
        List<List<StripUnit>> GetInterSectEnvPoints(List<List<StripUnit>> RoughStripUnitList, double Xmax, double Xmin, double Ymax, double Ymin)
        {
            List<List<StripUnit>> ResPoints = new List<List<StripUnit>>();
            foreach (List<StripUnit> StripUnitList in RoughStripUnitList)
            {
                List<StripUnit> ResUnitList = new List<StripUnit>();
                //判断小扫描带是否与目标区域相交
                foreach (StripUnit su in StripUnitList)
                {
                    if (IntersectRectangle(su.StripPoints, Xmax, Xmin, Ymax, Ymin))
                    {
                        ResUnitList.Add(su);
                    }
                }

                //保存结果
                if (ResUnitList.Count != 0)
                {
                    ResPoints.Add(ResUnitList);
                }
            }
            return ResPoints;
        }

        /// <summary>
        /// 将小四边形连接起来，形成Region
        /// </summary>
        /// <param name="StripUnitList"></param>
        /// <returns></returns>
        List<CRegion> GetStripList(List<List<StripUnit>> StripUnitList)
        {
            List<CRegion> RegionList = new List<CRegion>();
            foreach (List<StripUnit> tpList in StripUnitList)
            {
                if (tpList.Count == 0)
                {
                    break;
                }
                CRegion region = new CRegion();

                List<Point> LeftPoints = new List<Point>();
                List<Point> RightPoints = new List<Point>();

                for (int i = 0; i < tpList.Count; i++)
                {
                    //添加左上和右上点
                    LeftPoints.Add(tpList[i].StripPoints[3]);
                    RightPoints.Add(tpList[i].StripPoints[0]);
                }

                //添加左下和右下点
                LeftPoints.Add(tpList[tpList.Count - 1].StripPoints[2]);
                RightPoints.Add(tpList[tpList.Count - 1].StripPoints[1]);

                //先加右侧点
                List<Point> points = new List<Point>();
                points.AddRange(RightPoints);
                //再逆序添加左侧点
                for (int i = LeftPoints.Count - 1; i >= 0; i--)
                {
                    points.Add(LeftPoints[i]);
                }

                region.pGeometry = points;

                region.SatName = tpList[0].SatName;
                region.SenName = tpList[0].SenName;
                region.StartTime = tpList[0].StartTime;
                region.EndTime = tpList[tpList.Count - 1].StopTime;
                region.RegionColor = tpList[0].RegionColor;

                //计算太阳高度角
                double centerX = 0, centerY = 0, XMax, XMin, YMax, YMin;
                GetEnvelope(points, out XMax, out XMin, out YMax, out YMin);
                centerX = (XMax + XMin) / 2;
                centerY = (YMax + YMin) / 2;

                region.SunAngle = Math.Round(GetSolarAngle
                    (region.StartTime.ToUniversalTime(), centerX, centerY), 3);

                RegionList.Add(region);
            }
            return RegionList;
        }

        TrackPoint GetPrev(List<TrackPoint> TrackList, TrackPoint tp)
        {
            int i = TrackList.IndexOf(tp);
            if (i <= 0)
            {
                return null;
            }
            else
            {
                return TrackList[i - 1];
            }
        }

        TrackPoint GetNext(List<TrackPoint> TrackList, TrackPoint tp)
        {
            int i = TrackList.IndexOf(tp);
            if (i == TrackList.Count - 1 || i < 0)
            {
                return null;
            }
            else
            {
                return TrackList[i + 1];
            }
        }

        /// <summary>
        /// 找出在矩形之中的星下点轨迹,并自动分组
        /// </summary>
        /// <param name="SrcTrackPoints"></param>
        /// <param name="Xmax"></param>
        /// <param name="Xmin"></param>
        /// <param name="Ymax"></param>
        /// <param name="Ymin"></param>
        /// <returns></returns>
        List<List<TrackPoint>> GetInsidePoints(List<TrackPoint> SrcTrackPoints, double Xmax, double Xmin, double Ymax, double Ymin)
        {
            if (SrcTrackPoints == null || SrcTrackPoints.Count == 0)
            {
                return null;
            }
            List<List<TrackPoint>> ResList = new List<List<TrackPoint>>();
            List<TrackPoint> TmpList = new List<TrackPoint>();
            //记录上一个点是否在区域内
            bool PrevIn = InTheArea(SrcTrackPoints[0].BlhPoint, Xmin, Xmax, Ymin, Ymax);

            if (PrevIn)
            {
                TmpList.Add(SrcTrackPoints[0]);
            }

            for (int i = 1; i < SrcTrackPoints.Count; i++)
            {
                bool CurIn = InTheArea(SrcTrackPoints[i].BlhPoint, Xmin, Xmax, Ymin, Ymax);
                if (CurIn)
                {
                    TmpList.Add(SrcTrackPoints[i]);
                }
                else
                {
                    if (PrevIn)
                    {
                        //当前点不在,上一点在,更新TmpList
                        ResList.Add(TmpList);
                        TmpList = new List<TrackPoint>();
                    }
                }
                PrevIn = CurIn;
            }

            //加最后一个
            ResList.Add(TmpList);
            return ResList;
        }

        protected bool InTheArea(Point p, double XMin, double XMax, double YMin, double YMax)
        {
            if (p.X >= XMin && p.X <= XMax && p.Y >= YMin && p.Y <= YMax)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 多边形是否与矩形相交
        /// </summary>
        /// <returns></returns>
        bool IntersectRectangle(List<Point> polygon, double Xmax, double Xmin, double Ymax, double Ymin)
        {
            double Xmax0, Xmin0, Ymax0, Ymin0;

            GetEnvelope(polygon, out Xmax0, out Xmin0, out Ymax0, out Ymin0);
            return AxesIntersects(Xmax0, Xmin0, Xmax, Xmin) && AxesIntersects(Ymax0, Ymin0, Ymax, Ymin);
        }

        /// <summary>
        /// 坐标轴上是否相交
        /// </summary>
        /// <param name="Max1"></param>
        /// <param name="Min1"></param>
        /// <param name="Max2"></param>
        /// <param name="Min2"></param>
        /// <returns></returns>
        bool AxesIntersects(double Max1, double Min1, double Max2, double Min2)
        {
            return !(Max1 < Min2 || Max2 < Min1);
        }

        /// <summary>
        /// 根据轨道算观测范围
        /// </summary>
        /// <param name="sen"></param>
        /// <param name="SrcPointList"></param>
        /// <returns></returns>
        List<StripUnit> GetStripUnitList(Satellite sat, Sensor sen, List<TrackPoint> SrcPointList)
        {
            List<StripUnit> ResList = new List<StripUnit>();
            List<Point> LeftPoints = new List<Point>();
            List<Point> RightPoints = new List<Point>();

            foreach (TrackPoint tp in SrcPointList)
            {
                //算出两侧的观测点
                double[] LeftRightPoint = sat.GetSensorPointsBLH(sen, tp.Time, tp.EciPoint.X, tp.EciPoint.Y, tp.EciPoint.Z, tp.Vel.X, tp.Vel.Y, tp.Vel.Z);

                LeftPoints.Add(new Point(LeftRightPoint[0], LeftRightPoint[1]));
                RightPoints.Add(new Point(LeftRightPoint[2], LeftRightPoint[3]));
            }

            //观测点两两组合，形成小四边形，并构建stripunit
            for (int i = 0; i < LeftPoints.Count - 1; i++)
            {
                StripUnit su = new StripUnit();
                su.StripPoints.AddRange(new List<Point>() { LeftPoints[i], LeftPoints[i + 1], RightPoints[i + 1], RightPoints[i] });

                su.SatName = sen.SatName;
                su.SenName = sen.SenName;
                su.StartTime = SrcPointList[i].Time;
                su.StopTime = SrcPointList[i + 1].Time;
                su.FirstPoint = new Point(SrcPointList[i].BlhPoint.X, SrcPointList[i].BlhPoint.Y);
                su.LastPoint = new Point(SrcPointList[i + 1].BlhPoint.X, SrcPointList[i + 1].BlhPoint.Y);
                su.RegionColor = sen.SenColor;

                ResList.Add(su);
            }

            return ResList;
        }

        #region 凸多边形是否相交
        /// <summary>
        /// 判断两个凸多边形是否相交
        /// </summary>
        /// <param name="area1"></param>
        /// <param name="area2"></param>
        /// <returns></returns>
        public bool PolygonIntersects(List<Point> area1, List<Point> area2)
        {
            return m_RelOpera.AreaInsectArea(area1, area2);
        }

        public bool PointCollectionContainsPoint(List<Point> area, Point point)
        {
            Point start = new Point(-100, -100);
            int intersections = 0;

            for (int i = 0; i < area.Count; i++)
            {
                if (lineSegmentsIntersect(area[i], area[(i + 1) % area.Count], start, point))
                {
                    intersections++;
                }
            }

            return (intersections % 2) == 1;
        }

        private double determinant(Point vector1, Point vector2)
        {
            return vector1.X * vector2.Y - vector1.Y * vector2.X;
        }

        private bool lineSegmentsIntersect(Point _segment1_Start, Point _segment1_End, Point _segment2_Start, Point _segment2_End)
        {
            double det = determinant(MinusPoint(_segment1_End, _segment1_Start), MinusPoint(_segment2_Start, _segment2_End));
            double t = determinant(MinusPoint(_segment2_Start, _segment1_Start), MinusPoint(_segment2_Start, _segment2_End)) / det;
            double u = determinant(MinusPoint(_segment1_End, _segment1_Start), MinusPoint(_segment2_Start, _segment1_Start)) / det;
            return (t >= 0) && (u >= 0) && (t <= 1) && (u <= 1);
        }

        Point MinusPoint(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        #endregion
        #endregion
        /// <summary>
        /// 计算太阳高度角
        /// </summary>
        /// <param name="dtUTC">UTC时间</param>
        /// <param name="lon">经度，度</param>
        /// <param name="lat">纬度，度</param>
        /// <param name="HeightAngle">太阳高度角，度</param>
        double GetSolarAngle(DateTime dtUTC, double lon, double lat)
        {
            int timezone = 0;
            double N0, sitar, ED, dLon, Et;
            const double PI = 3.14159265;
            double dTimeAngle, gtdt, latitudeArc, HeightAngleArc;
            double HeightAngle;
            int N = dtUTC.DayOfYear;

            N0 = 79.6764 + 0.2422 * (dtUTC.Year - 1985) - Math.Floor((dtUTC.Year - 1985) / 4.0);

            sitar = 2 * PI * (N - N0) / 365.2422;
            ED = 0.3723 + 23.2567 * Math.Sin(sitar) + 0.1149 * Math.Sin(2 * sitar) - 0.1712 * Math.Sin(3 * sitar) - 0.758 * Math.Cos(sitar) + 0.3656 * Math.Cos(2 * sitar) + 0.0201 * Math.Cos(3 * sitar);
            ED = ED * PI / 180;           //ED本身有符号，无需判断正负。

            dLon = 0.0;

            dLon = lon - timezone * 15;

            Et = 0.0028 - 1.9857 * Math.Sin(sitar) + 9.9059 * Math.Sin(2 * sitar) - 7.0924 * Math.Cos(sitar) - 0.6882 * Math.Cos(2 * sitar); //视差

            gtdt = dtUTC.Hour + dtUTC.Minute / 60.0 + dtUTC.Second / 3600.0 + dLon / 15; //地方时
            gtdt = gtdt + Et / 60.0;
            dTimeAngle = 15.0 * (gtdt - 12);
            dTimeAngle = dTimeAngle * PI / 180;
            latitudeArc = lat * PI / 180;

            HeightAngleArc = Math.Asin(Math.Sin(latitudeArc) * Math.Sin(ED) + Math.Cos(latitudeArc) * Math.Cos(ED) * Math.Cos(dTimeAngle));

            HeightAngle = HeightAngleArc * 180 / PI;
            return HeightAngle;
        }

        void WriteDt(List<DataTable> dtList, string filename)
        {
            WriteDt(dtList[0], filename, false);
            for (int i = 1; i < dtList.Count; i++)
            {
                WriteDt(dtList[i], filename, true);
            }

        }

        void WriteDt(DataTable dt, string filename, bool append)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filename, append))
            {
                string ss = "";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    ss += dt.Columns[i].ColumnName + "    ";
                }

                sw.WriteLine(ss);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string s = "";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        s += dt.Rows[i][j].ToString() + "  ";
                    }
                    sw.WriteLine(s);
                }

                sw.WriteLine("-----------------------------");
            }

        }

        internal void SetNoardID(string noardid)
        { m_NoardID = noardid; }

        internal void SetOrbitHeight(double height)
        { m_satHeight = height; }
    }
}
