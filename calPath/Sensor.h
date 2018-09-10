#ifndef SENSOR_H_
#define SENSOR_H_
#include "Color.h"
#include <string>
using namespace std;
class Sensor
{
private:
  string m_SenId, m_SatId, m_SenName, m_SatName;
  string m_CountryName;
  double m_Width;
  double m_ObsAngle; //观测角
  Color m_SenColor;
  double m_CurSideAngle; //当前的侧摆角
  double m_Resolution;
  bool m_Checked;          //指示treeview上的选中状态
  double m_RightSideAngle; //向右侧摆的角度
  double m_LeftSideAngle;  //向左侧摆的角度
  double m_InitAngle;      //载荷安装角
  void Init();

public:
  Sensor(string satId, string sensorId);
  ~Sensor();

  string getSatId() { return m_SatId; }
  string getSenId() { return m_SenId; }

  double getResolution() { return m_Resolution; }
  void setResolution(double value) { m_Resolution = value; }

  double getObsAngle() { return m_ObsAngle; }
  void setObsAngle(double value) { m_ObsAngle = value; }

  bool getChecked() { return m_Checked; }
  void setChecked(bool value) { m_Checked = value; }

  double getLeftSideAngle() { return m_LeftSideAngle; }
  void setLeftSideAngle(double value) { m_LeftSideAngle = value; }

  double getRightSideAngle() { return m_RightSideAngle; }
  void setRightSideAngle(double value) { m_RightSideAngle = value; }

  string getSenName() { return m_SenName; }
  void setSenName(string value) { m_SenName = value; }

  string getSatName() { return m_SatName; }
  void setSatName(string value) { m_SatName = value; }

  string getCountryName() { return m_CountryName; }
  void setCountryName(string value) { m_CountryName = value; }

  double getWidth() { return m_Width; }
  void setWidth(double value) { m_Width = value; }

  Color getSenColor() { return m_SenColor; }
  void setSenColor(Color value) { m_SenColor = value; }

  double getCurSideAngle() { return m_CurSideAngle; }
  void setCurSideAngle(double value) { m_CurSideAngle = value; }

  double getInitAngle() { return m_InitAngle; }
  void setInitAngle(double value) { m_InitAngle = value; }
};
#endif