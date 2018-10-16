#ifndef SATELLITE_H_
#define SATELLITE_H_
#include "Color.h"
#include "Sensor.h"
#include "RelationOperator.h"
#include "DateTime.h"
#include "TrackPoint.h"
#include <vector>

using namespace std;
enum PathMode
{
  Day,
  Night,
  Both
};
class Satellite
{
protected:
  string m_SatName;

  Color m_DisplayColor;
  bool m_Checked;  //指示treeview上的选中状态
  int m_StepInSec; //正常步长,20s

  RelationOperator m_RelOpera;
  //卫星上的载荷
  vector<Sensor> m_SensorList;

  string m_NoardID;
  string m_LastError = "";

  virtual double GetHeight() { return 600.0; }
  void Init();

private:
  double m_SatHeight;

  double GetSatHeight() { return 600.0; }
  double TheltaG(DateTime dt);

  MulMatrix3p1(double mx[][3],double my[][3], double res[][3]);
  RotateX(double coord[], double AngleRad, double res[]);
  RotateY(double coord[], double AngleRad, double res[]);
  RotateZ(double coord[], double AngleRad, double res[]);
  ECRtoBL(double ecr[], double res[]);
  GetSensorPointsECI(Sensor sen, double r[], double v[],double res[]);
  IntersectSolution(double v[], double r[],double res[]);
  CrossProduct(double v1[], double v2[],double res[]);
  Yuzishi(double array[][3],double res[][3]);
  double MOD3p3(double[][3] m);
  ComputeReo(double r[], double v[],double res[][3]);
  MatrixReverse(double m[][3],double res[][3]);

  vector<double> MulMatrix3p1(vector<vector<double>> mx, vector<double> my);
  vector<double> RotateX(double x0, double y0, double z0, double AngleRad);
  vector<double> RotateY(double x0, double y0, double z0, double AngleRad);
  vector<double> RotateZ(double x0, double y0, double z0, double AngleRad);
  vector<double> ECRtoBL(vector<double> ecr);
  vector<double> GetSensorPointsECI(Sensor sen, double rx, double ry, double rz,
                                    double vx, double vy, double vz);
  vector<double> IntersectSolution(double vx, double vy, double vz, double rx, double ry, double rz);
  vector<double> CrossProduct(vector<double> v1, vector<double> v2);
  vector<vector<double>> Yuzishi(vector<vector<double>> array);
  double MOD3p3(vector<vector<double>> m);
  vector<vector<double>> ComputeReo(double rx, double ry, double rz, double vx, double vy, double vz);
  vector<vector<double>> MatrixReverse(vector<vector<double>> m);

public:
  Satellite(string SatId, string SatName);
  ~Satellite();

  string getLastError() { return m_LastError; }

  string getNoardID() { return m_NoardID; }

  string getSatName() { return m_SatName; }
  void setSatName(string value) { m_SatName = value; }

  Color getDisplayColor() { return m_DisplayColor; }
  void setDisplayColor(Color value) { m_DisplayColor = value; }

  bool getChecked() { return m_Checked; }
  void setChecked(bool value) { m_Checked = value; }

  vector<Sensor> getSensorList() { return m_SensorList; }
  void AddSensor(Sensor sen){m_SensorList.push_back(sen);}

  virtual vector<TrackPoint> ComputeTrack2(DateTime StartTime, DateTime EndTime, int StepTimeInSec) {}
  vector<double> GetSensorPointsBLH(Sensor sen, DateTime dt, double rx, double ry, double rz,
                                    double vx, double vy, double vz);
};
#endif
