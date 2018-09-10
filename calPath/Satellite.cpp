#include "Satellite.h"
#include "JulianConvert.h"
#include <string>
#include <vector>
#include <cmath>
using namespace std;

const double R = 6378.15;
//YYYY-MM-DD HH24:MI:SS
const string BJTimeFormate = "yyyy-MM-dd HH:mm:ss"; //北京时间的格式
Satellite::Satellite(string SatId, string SatName)
{
}
Satellite::~Satellite()
{
}
void Satellite::Init()
{
}

double Satellite::TheltaG(DateTime dt)
{
    JulianConvert jul;
    return jul.ToGMST(dt.ToUniversalTime());
}

vector<double> Satellite::RotateZ(double x0, double y0, double z0, double AngleRad)
{
    vector<double> m0{x0, y0, z0};
    vector<vector<double>> mRz{{cos(AngleRad), -sin(AngleRad), 0},
                               {sin(AngleRad), cos(AngleRad), 0},
                               {0, 0, 1}};
    return MulMatrix3p1(mRz, m0);
}
vector<double> Satellite::RotateX(double x0, double y0, double z0, double AngleRad)
{
    vector<double> m0{x0, y0, z0};
    vector<vector<double>> mRx{{1, 0, 0},
                               {0, cos(AngleRad), -sin(AngleRad)},
                               {0, sin(AngleRad), cos(AngleRad)}};
    return MulMatrix3p1(mRx, m0);
}

vector<double> Satellite::RotateY(double x0, double y0, double z0, double AngleRad)
{
    vector<double> m0{x0, y0, z0};
    vector<vector<double>> mRy{{cos(AngleRad), 0, sin(AngleRad)},
                               {0, 1, 0},
                               {-sin(AngleRad), 0, cos(AngleRad)}};
    return MulMatrix3p1(mRy, m0);
}

vector<double> Satellite::MulMatrix3p1(vector<vector<double>> mx, vector<double> my)
{
    double m1 = mx[0][0] * my[0] + mx[0][1] * my[1] + mx[0][2] * my[2];
    double m2 = mx[1][0] * my[0] + mx[1][1] * my[1] + mx[1][2] * my[2];
    double m3 = mx[2][0] * my[0] + mx[2][1] * my[1] + mx[2][2] * my[2];

    vector<double> m{m1,m2,m3};
    return m;
}

vector<double> Satellite::ECRtoBL(vector<double> ecr)
{
    //ECR转换成BLH
    double lon_left = atan(ecr[1] / ecr[0]) * 180 / M_PI;
    double lat_left = atan(ecr[2] / (sqrt(ecr[0] * ecr[0] + ecr[1] * ecr[1]))) * 180 / M_PI;

    //经度范围在-180~180之间，而atan的值域为-90~90，因此需要对经度进行调整
    if (ecr[0] < 0)
    {
        lon_left = ecr[1] < 0 ? lon_left - 180 : lon_left + 180;
    }

    vector<double> blh{lon_left, lat_left};
    return blh;
}

vector<double> Satellite::GetSensorPointsECI(Sensor sen, double rx, double ry, double rz,
                                             double vx, double vy, double vz)
{
    //传感器坐标下的Z轴向量
    //（，，）
    //根据传感器的安装参数和当前侧摆角将Z轴坐标转换成卫星本体坐标系
    //只考虑侧摆方向的安装角，绕X轴旋转，向右为正
    //临时修改，因安装角与计算结果正好相反，故加了个负号
    vector<double> satz = RotateX(0, 0, 1, -sen.getInitAngle() * M_PI / 180);
    //double[] satz = RotateX(0, 0, 1, 0);

    //一些角度
    double SenCurAngle = sen.getCurSideAngle() * M_PI / 180;
    //粗略计算载荷的观测角
    double ObsAngle = sen.getObsAngle() > 0 ? sen.getObsAngle() : atan(sen.getWidth() / (GetSatHeight() * 2));
    double SenHalfAngle = (ObsAngle / 2) * M_PI / 180;
    //SenHalfAngle = 10 * Math.PI / 180;
    //SenCurAngle = 20 * Math.PI / 180;

    //根据卫星的姿态将卫星本体坐标系转换成轨道坐标系
    //暂时只考虑侧摆，绕X轴旋转，向右侧摆为负
    //载荷Z轴的旋转,向右为负
    vector<double> satsidez = RotateX(satz[0], satz[1], satz[2], -SenCurAngle);
    //将载荷的Z轴向左右旋转，得出观测的左右侧向量
    vector<double> sl = RotateX(satsidez[0], satsidez[1], satsidez[2], SenHalfAngle);

    vector<double> sr = RotateX(satsidez[0], satsidez[1], satsidez[2], -SenHalfAngle);

    //根据卫星的位置和速度将轨道坐标系转换成ECI
    //计算转换矩阵Reo
    vector<vector<double>> Reo = ComputeReo(rx, ry, rz, vx, vy, vz);

    //用Reo将sl[]和sr[]转换至ECI
    vector<double> sleci = MulMatrix3p1(Reo, sl);
    vector<double> sreci = MulMatrix3p1(Reo, sr);

    //观测向量与地球求交
    vector<double> pleci = IntersectSolution(sleci[0], sleci[1], sleci[2], rx, ry, rz);
    vector<double> preci = IntersectSolution(sreci[0], sreci[1], sreci[2], rx, ry, rz);

    //设定返回值
    vector<double> retvalue{pleci[0], pleci[1], pleci[2], preci[0], preci[1], preci[2]};
    return retvalue;
}

vector<double> Satellite::IntersectSolution(double vx, double vy, double vz, double rx, double ry, double rz)
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
    vector<double> res;

    double A = vx * vx + vy * vy + vz * vz, B = 2 * (vx * rx + vy * ry + vz * rz),
           C = rx * rx + ry * ry + rz * rz - 1;
    double k, delta = B * B - 4 * A * C;
    if (delta < 0)
    {
        cout << "交线方程无解！" << endl;
        return res;
    }
    //由于要求出球面上距(rx,ry,rz)点较近的点，因此如果B>0，则解取+号，否则取-号
    if (B > 0)
    {
        k = (-B + sqrt(delta)) / (2 * A);
    }
    else
    {
        k = (-B - sqrt(delta)) / (2 * A);
    }

    res.push_back(rx + k * vx);
    res.push_back(ry + k * vy);
    res.push_back(rz + k * vz);
    return res;
}

vector<vector<double>> Satellite::ComputeReo(double rx, double ry, double rz, double vx, double vy, double vz)
{
    //单位化
    double modr = sqrt(rx * rx + ry * ry + rz * rz);
    double modv = sqrt(vx * vx + vy * vy + vz * vz);
    rx /= modr;
    ry /= modr;
    rz /= modr;
    vx /= modv;
    vy /= modv;
    vz /= modv;

    vector<double> r{rx, ry, rz};
    vector<double> v{vx, vy, vz};
    //求H
    vector<double> h = CrossProduct(r, v);

    //Roe[0]
    vector<double> Roe0 = CrossProduct(h, r);

    vector<vector<double>> Roe{{Roe0[0], Roe0[1], Roe0[2]},
                               {-h[0], -h[1], -h[2]},
                               {-r[0], -r[1], -r[2]}};
    //求逆
    vector<vector<double>> Reo = MatrixReverse(Roe);
    return Reo;
}

vector<double> Satellite::CrossProduct(vector<double> v1, vector<double> v2)
{
    vector<double> res;
    if (v1.size() != 3 || v2.size() != 3)
    {
        cout << "向量叉乘出错！不是三维向量相乘" << endl;
        return res;
    }
    double i, j, k;
    i = v1[1] * v2[2] - v1[2] * v2[1];
    j = v1[2] * v2[0] - v1[0] * v2[2];
    k = v1[0] * v2[1] - v1[1] * v2[0];

    res = {i, j, k};
    return res;
}

vector<vector<double>> Satellite::MatrixReverse(vector<vector<double>> m)
{
    vector<vector<double>> yu;
    if (m.size() != 3 || m[0].size() != 3 || m[1].size() != 3 || m[2].size() != 3)
    {
        cout << "矩阵求逆出错！不是*3矩阵" << endl;
        return yu;
    }

    double mod = MOD3p3(m);
    yu = Yuzishi(m);

    if (mod == 0)
    {
        cout << "矩阵的模为，无法求逆!" << endl;
        return yu;
    }

    for (int i = 0; i < yu.size(); i++)
    {
        for (int j = 0; j < yu[i].size(); j++)
        {
            yu[i][j] /= mod;
        }
    }
    return yu;
}

double Satellite::MOD3p3(vector<vector<double>> m)
{
    if (m.size() != 3 || m[0].size() != 3 || m[1].size() != 3 || m[2].size() != 3)
    {
        cout << "矩阵求模出错！不是3*3矩阵" << endl;
        return -1;
    }
    return m[0][0] * m[1][1] * m[2][2] +
           m[1][0] * m[2][1] * m[0][2] +
           m[2][0] * m[1][2] * m[0][1] -
           m[0][2] * m[1][1] * m[2][0] -
           m[1][2] * m[2][1] * m[0][0] -
           m[2][2] * m[1][0] * m[0][1];
}

vector<vector<double>> Satellite::Yuzishi(vector<vector<double>> array)
{
    double y00, y01, y02, y10, y11, y12, y20, y21, y22;
    //00
    y00 = array[1][1] * array[2][2] - array[1][2] * array[2][1];
    //01 -
    y01 = -(array[1][0] * array[2][2] - array[1][2] * array[2][0]);
    //02
    y02 = array[1][0] * array[2][1] - array[2][0] * array[1][1];
    //10 -
    y10 = -(array[0][1] * array[2][2] - array[0][2] * array[2][1]);
    //11
    y11 = array[0][0] * array[2][2] - array[0][2] * array[2][0];
    //12 -
    y12 = -(array[0][0] * array[2][1] - array[0][1] * array[2][0]);
    //20
    y20 = array[0][1] * array[1][2] - array[0][2] * array[1][1];
    //21 -
    y21 = -(array[0][0] * array[1][2] - array[0][2] * array[1][0]);
    //22
    y22 = array[0][0] * array[1][1] - array[0][1] * array[1][0];

    vector<vector<double>> yu{{y00, y10, y20}, {y01, y11, y21}, {y02, y12, y22}};

    return yu;
}

vector<double> Satellite::GetSensorPointsBLH(Sensor sen, DateTime dt, double rx, double ry, double rz,
                                             double vx, double vy, double vz)
{
    //计算载荷观测范围的ECI坐标
    vector<double> seneci = GetSensorPointsECI(sen, rx, ry, rz, vx, vy, vz);

    //分离出左右侧点
    vector<double> pleci{seneci[0], seneci[1], seneci[2]};
    vector<double> preci{seneci[3], seneci[4], seneci[5]};
    //根据时间将ECI转换成ECR
    double thetaG = TheltaG(dt);
    vector<double> plecr = RotateZ(pleci[0], pleci[1], pleci[2], -thetaG);
    vector<double> precr = RotateZ(preci[0], preci[1], preci[2], -thetaG);

    //ECR转换成BLH
    vector<double> left = ECRtoBL(plecr);
    vector<double> right = ECRtoBL(precr);

    vector<double> points{left[0], left[1], right[0], right[1]};
    return points;
}