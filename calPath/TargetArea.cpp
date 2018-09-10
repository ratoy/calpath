#include "TargetArea.h"
#include "stdlib.h"
#include <cmath>
#include <time.h>
#include <iostream>
using namespace std;

TargetArea::TargetArea(string ID, string Name, vector<MyPoint> Geometry)
{
    m_Name = Name;
    m_ID = ID;
    m_Geometry = Geometry;
    VariablesInit();
    DefaultStyleInit();
}

TargetArea::TargetArea(string ID, string Name)
{
    m_Name = Name;
    m_ID = ID;
    VariablesInit();
    DefaultStyleInit();
}
TargetArea::TargetArea(string ID)
{
    m_Name = m_ID = ID;
    VariablesInit();
    DefaultStyleInit();
}
void TargetArea::VariablesInit()
{
    m_OutLineWidth = 1.0;
    m_Checked = false;
}
void TargetArea::DefaultStyleInit()
{
    cout<<"getting area color"<<endl;
    GetRandomColor(&m_AreaColor, 0, 255, 150);
    cout<<"getting outline color"<<endl;
    GetRandomColor(&m_OutLineColor, 0, 100, 255);
    m_OutLineWidth = 2;
}
void TargetArea::GetRandomColor(Color **pColor, int Min, int Max, int Alpha)
{
    int MinValue = fmax(0, Min);
    int MaxValue = fmin(255, Max);
    unsigned r = (unsigned)GetRandomValue(MinValue, MaxValue);
    unsigned g = (unsigned)GetRandomValue(MinValue, MaxValue);
    unsigned b = (unsigned)GetRandomValue(MinValue, MaxValue);
    *pColor = new Color((unsigned)Alpha, r, g, b);
}

int TargetArea::GetRandomValue(int Min, int Max)
{
    srand((unsigned)time(NULL)); //初始化随机数种子
    return (rand() % (Max - Min + 1)) + Min;
}
TargetArea::~TargetArea()
{
    delete m_AreaColor;
    delete m_OutLineColor;
}