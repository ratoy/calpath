#include "GenSatPath.h"
#include <ctime>
#include "TrackPoint.h"
#include "TargetArea.h"
#include "DateTime.h"
#include "StripUnit.h"
#include "MyPoint.h"
#include "Satellite.h"
#include "Sensor.h"
#include "SatTLE.h"
#include <iostream>
#include <string>
#include <vector>

using namespace std;

GenSatPath::GenSatPath(string SatDbFile, string OutputFolder)
{
}
GenSatPath::~GenSatPath()
{
}

void GenSatPath::Init()
{
	m_TrackDbInitCmds.push_back("CREATE TABLE info(satid,satname,starttime,timeinfo);");
	m_TrackDbInitCmds.push_back("CREATE TABLE track(time int,x double,y double,z double, vx double, vy double, vz double, lon double, lat double, alt double);");
	m_TrackDbInitCmds.push_back("CREATE INDEX index_time on track(time);");

	m_PathDbInitCmds.push_back("CREATE TABLE info(satid,satname,senname,starttime,timeinfo);");
	m_PathDbInitCmds.push_back("CREATE TABLE path(time int, lon1 double,lat1 double, lon2 double,lat2 double);");
	m_PathDbInitCmds.push_back("CREATE INDEX index_pathtime on path(time);");
	m_PathDbInitCmds.push_back("CREATE INDEX index_lat1 on path(lat1);");
	m_PathDbInitCmds.push_back("CREATE INDEX index_lat2 on path(lat2);");
	m_PathDbInitCmds.push_back("CREATE INDEX index_lon1 on path(lon1);");
	m_PathDbInitCmds.push_back("CREATE INDEX index_lon2 on path(lon2);");
}

string GenSatPath::GetBjtTimeString()
{
	return "bj time";
}
void GenSatPath::Do()
{
	/*
	MyPoint p1(10, 10);
	MyPoint p2(20, 30);
	MyPoint p3 = p1 + p2;
	cout << "p3.x: " + to_string(p3.getX()) + " p3.y: " + to_string(p3.getY()) << endl;

	TrackPoint tp(100L, 1.1, 2.3, 4.5);
	cout << "time in tp: " << tp.getlTime() << endl;

	Color c(200, 200, 200, 200);
	cout << "red: " << c.getRed() << endl;

	TargetArea ta("id", "name");
	cout << "id: " << ta.getName() << endl;
	cout << "id: " << ta.getAreaColor()->getRed() << endl;

	DateTime dt(2010, 2, 2,3,14,10);
	dt.setTimezone(8);
	DateTime dtUtc=dt.ToUniversalTime();
	cout << "time: " << dtUtc.toString() << endl;

	StripUnit su;
	su.setSatName("satellite");
	cout << "sat name: " << su.getSatName() << endl;
*/

	SatTLE sat("a");
	Sensor sen("a", "sen");
	sen.setObsAngle(10);

	sat.AddSensor(sen);

	string str1 = "LANDSAT 7";
	string str2 = "1 25682U 99020A   18250.16131232  .00000059  00000-0  23083-4 0  9992";
	string str3 = "2 25682  98.1656 318.8025 0001546  86.6712 273.4684 14.57135088 31687";
	sat.SetCurTLE(str1, str2, str3);
	//compute track
	DateTime StartTime(2018, 2, 2, 3, 14, 10);
	DateTime StopTime(2018, 2, 12, 3, 24, 10);
	int tsp = 20;
	cout << "computing track..." << endl;

	clock_t start = clock();
	vector<TrackPoint> Points = sat.ComputeTrack2(StartTime, StopTime, tsp);
	/*
	for (auto &p : Points) // access by reference to avoid copying
	{
		cout << "x: " << to_string(p.getBlhPoint().getX()) << " y: " + to_string(p.getBlhPoint().getY()) << endl;
	}
	*/
	//save to db
	clock_t stop = clock();
	cout << "total seconds: " << (double)(stop - start) / CLOCKS_PER_SEC << endl;

	start = clock();
	//compute path
	cout << "computing path..." << endl;
	for (auto &sen : sat.getSensorList())
	{
		//cout << "computing " << sen.getSenName() << endl;
		for (auto &tp : Points)
		{
			//compute sensor path point
			vector<double> senblh = sat.GetSensorPointsBLH(sen, tp.getTime(), tp.getEciPoint().getX(),
														   tp.getEciPoint().getY(), tp.getEciPoint().getZ(), tp.getVel().getX(), tp.getVel().getY(), tp.getVel().getZ());

			//cout << "left lon: " << senblh[0] << " left lat: " << senblh[1] << " right lon: " << senblh[2] << " right lat: " << senblh[3] << endl;
		}
	}
	//save to db
	stop = clock();
	cout << "total count: " << Points.size() << endl;
	cout << "total seconds: " << (double)(stop - start) / CLOCKS_PER_SEC << endl;
}
