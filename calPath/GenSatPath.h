#ifndef GENSATPATH_H_
#define GENSATPATH_H_
#include <vector>
#include <string>
using namespace std;

//////////////////////////////////////////////////////////////////////
// class GenSatPath
// This class generate satellite paths
class GenSatPath
{
  public:
    GenSatPath(string SatDbFile, string OutputFolder);
    virtual ~GenSatPath();

    //void OutputLog(string strlog);
    string GetBjtTimeString();
    void Do();
    //bool CreateNewSat(string SatID, Dictionary<string, object> DictProperties);

  private:
    vector<string> m_TrackDbInitCmds;
    vector<string> m_PathDbInitCmds;
    void Init();
  /*
    typedef list<string> m_MailDetails;
    typedef list<Satellite> m_SatList;
    const string UTCTimeFormate = "yyyy-MM-dd HH:mm:ss";
    const int COMMIT_COUNT = 40000;
    string m_LastErrorMsg = "", m_SatDbFile = "", m_OutputFolder = "";
    void SatInit();
    */

    //Satellite CreateSat(string SatID, Dictionary<string, object> DictProperties) bool SetCurTle(SatTLE Sat, DateTime Dt) void GetTLE(Satellite Sat, DateTime Dt, out string[] strTLE) bool UpdateTLEs(string SatID = "") bool UpdatelSatTleFormURL(string updateurl, string SatID = "", Dictionary<string, string> Dict = null);
    //string GetUpdateTleCmd(string SatID, string line0, string line1, string line2);
    //string GetTLEtime(string line0, string line1, string line2);
    //Satellite GetSat(string SatName);
};
#endif