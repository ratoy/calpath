#ifndef TARGETAREA_H_
#define TARGETAREA_H_
#include "MyPoint.h"
#include "Color.h"
#include <string>
#include <vector>
using namespace std;

class TargetArea
{
  private:
    string m_Name, m_ID;
    vector<MyPoint> m_Geometry;
    Color *m_AreaColor;
    Color *m_OutLineColor;
    double m_OutLineWidth;
    bool m_Checked;

    void GetRandomColor(Color **pColor, int Min, int Max, int Alpha);
    int GetRandomValue(int Min, int Max);
    void DefaultStyleInit();
    void VariablesInit();

  public:
    bool getChecked() const { return m_Checked; }
    void setChecked(bool checked) { m_Checked = checked; }

    Color *getOutLineColor() const { return m_OutLineColor; }
    void setOutLineColor(Color *c) { m_OutLineColor = c; }

    Color *getAreaColor() const { return m_AreaColor; }
    void setAreaColor(Color *c) { m_AreaColor = c; }

    double getOutLineWidth() const { return m_OutLineWidth; }
    void setOutLineWidth(double c) { m_OutLineWidth = c; }

    string getID() const { return m_ID; }

    string getName() const { return m_Name; }
    void setName(string s) { m_Name = s; }

    vector<MyPoint> getGeometry() const { return m_Geometry; }
    void setGeometry(vector<MyPoint> s) { m_Geometry = s; }

    TargetArea(string ID, string Name, vector<MyPoint> Geometry);
    TargetArea(string ID, string Name);
    TargetArea(string ID);
    ~TargetArea();
};
#endif