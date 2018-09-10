using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.IO;

namespace GenPath
{
	class GenerateSatPath
	{
		List<string> m_MailDetails = new List<string>();
		List<Satellite> m_SatList = new List<Satellite>();
		SqliteOperation m_Sqlite = null;
		SqliteOperation m_SqlTrack = null;
		SqliteOperation m_SqlPath = null;
		const string UTCTimeFormate = "yyyy-MM-dd HH:mm:ss";

		string m_LastErrorMsg = "", m_SatDbFile = "", m_OutputFolder = "";
		const int COMMIT_COUNT = 40000;

		public GenerateSatPath(string SatDbFile, string OutputFolder)
		{
			m_SatDbFile = SatDbFile;
			m_OutputFolder = OutputFolder;

			m_Sqlite = new SqliteOperation(SatDbFile);
		}

		List<string> GetTrackDbInitCmds()
		{
			List<string> InitCmds = new List<string>();
			InitCmds.Add("CREATE TABLE info(satid,satname,starttime,timeinfo);");
			InitCmds.Add("CREATE TABLE track(time int,x double,y double,z double, vx double, vy double, vz double, lon double, lat double, alt double);");
			InitCmds.Add("CREATE INDEX index_time on track(time);");
			return InitCmds;
		}
		List<string> GetPathDbInitCmds()
		{
			List<string> InitCmds = new List<string>();
			InitCmds.Add("CREATE TABLE info(satid,satname,senname,starttime,timeinfo);");
			InitCmds.Add("CREATE TABLE path(time int, lon1 double,lat1 double, lon2 double,lat2 double);");
			InitCmds.Add("CREATE INDEX index_pathtime on path(time);");
			InitCmds.Add("CREATE INDEX index_lat1 on path(lat1);");
			InitCmds.Add("CREATE INDEX index_lat2 on path(lat2);");
			InitCmds.Add("CREATE INDEX index_lon1 on path(lon1);");
			InitCmds.Add("CREATE INDEX index_lon2 on path(lon2);");
			return InitCmds;
		}

		void OutputLog(string strlog)
		{
			Console.WriteLine(strlog);
			m_MailDetails.Add(GetBjtTimeString() + ": " + strlog);
		}

		string GetBjtTimeString()
		{
			DateTime Dt = DateTime.UtcNow.AddHours(8);
			return Dt.ToString("yyyy-MM-dd HH:mm:ss");
		}

		public void Do(string NoardIds="")
		{
			DateTime Dt = DateTime.UtcNow;
			DateTime StartTime = new DateTime(Dt.Year, Dt.Month, Dt.Day , 0, 0, 0, DateTimeKind.Utc).AddHours(-1);
			DateTime StopTime = StartTime.AddDays(7).AddHours(2);

			m_MailDetails.Clear();
			string tmpstr = string.Format("Generating paths from {0} to {1} (UTC)...", StartTime, StopTime);
			OutputLog(tmpstr);

			TimeSpan tsp = new TimeSpan(0, 0, 20);
			try
			{
				//get satellite list
				tmpstr = "Initialize satellites...";
				OutputLog(tmpstr);
				SatInit();
				//update tle
				tmpstr = "Updating TLE...";
				OutputLog(tmpstr);
				UpdateTLEs();
				//compute track
				List<string> SpecificSats=new List<string> ();
				if(NoardIds.Trim().Length !=0)
				{
					string[] sats=NoardIds.Split(',');
					SpecificSats =new List<string>(sats);
				}
				OutputLog("Sat count:"+m_SatList.Count);
				if(SpecificSats.Count ==0)
				{
					//clear track folder
					System.IO.DirectoryInfo di = new DirectoryInfo(m_OutputFolder);

					foreach (FileInfo file in di.GetFiles())
					{
						file.Delete(); 
					}
					foreach (DirectoryInfo dir in di.GetDirectories())
					{
						dir.Delete(true); 
					}
				}
				foreach (Satellite sat in m_SatList)
				{
					if(SpecificSats.Count !=0&&!SpecificSats.Contains(sat.NoardID))
					{
						OutputLog("continued...");
						continue;
					}

					tmpstr = "Generating satellite " + sat.SatName + " ...";
					OutputLog(tmpstr);
					string TrackFile = System.IO.Path.Combine(m_OutputFolder, sat.SatName + ".sqlite");
					if (System.IO.File.Exists(TrackFile))
					{
						tmpstr = "Clearing db file " + TrackFile + " ...";
						OutputLog(tmpstr);
						m_SqlTrack = new SqliteOperation(TrackFile);
						m_SqlTrack.ExecuteSql("delete from track");
						m_SqlTrack.ExecuteSql("delete from info");
					}
					else
					{
						tmpstr = "Creating db file " + TrackFile + " ...";
						OutputLog(tmpstr);
						m_SqlTrack = new SqliteOperation(TrackFile);
						m_SqlTrack.BatProcess(GetTrackDbInitCmds());
					}

					List<TrackPoint> Points = sat.ComputeTrack2(StartTime, StopTime, tsp);
					//write to db
					List<string> cmds = new List<string>();
					cmds.Add(string.Format("insert into info values('{0}','{1}','{2}','UTC')",
								sat.NoardID, sat.SatName, StartTime));
					for (int i = 0; i < Points.Count; i++)
					{
						TrackPoint tp = Points[i];
						cmds.Add(string.Format("insert into track values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')",
									i, tp.EciPoint.X, tp.EciPoint.Y, tp.EciPoint.Z, tp.Vel.X, tp.Vel.Y, tp.Vel.Z,
									tp.BlhPoint.X, tp.BlhPoint.Y, tp.BlhPoint.Z));
					}

					//write to track db
					m_SqlTrack.BatProcess(cmds);

					foreach (Sensor sen in sat.SensorList)
					{
						tmpstr = "Generating sensor " + sen.SenID + " ...";
						OutputLog(tmpstr);
						string PathFolder = System.IO.Path.Combine(m_OutputFolder, sat.SatName);
						if (!System.IO.Directory.Exists(PathFolder))
						{
							System.IO.Directory.CreateDirectory(PathFolder);
						}
						string PathFile = System.IO.Path.Combine(PathFolder, sen.SenName + ".sqlite");
						if (System.IO.File.Exists(PathFile))
						{
							tmpstr = "Clearing db file " + PathFile + " ...";
							OutputLog(tmpstr);
							m_SqlPath = new SqliteOperation(PathFile);
							m_SqlPath.ExecuteSql("delete from path");
							m_SqlPath.ExecuteSql("delete from info");
						}
						else
						{
							tmpstr = "Creating db file " + PathFile + " ...";
							OutputLog(tmpstr);
							m_SqlPath = new SqliteOperation(PathFile);
							m_SqlPath.BatProcess(GetPathDbInitCmds());
						}
						cmds.Clear();
						cmds.Add(string.Format("insert into info values('{0}','{1}','{2}','{3}','UTC')",
									sat.NoardID, sat.SatName, sen.SenName, StartTime));
						for (int i = 0; i < Points.Count; i++)
						{
							TrackPoint tp = Points[i];
							//compute sensor path point
							double[] senblh = sat.GetSensorPointsBLH(sen, tp.Time,
									tp.EciPoint.X, tp.EciPoint.Y, tp.EciPoint.Z, tp.Vel.X, tp.Vel.Y, tp.Vel.Z);


							cmds.Add(string.Format("insert into path values('{0}','{1}','{2}','{3}','{4}')",
										i, senblh[0], senblh[1], senblh[2], senblh[3]));

						}

						//write the rest points
						m_SqlPath.BatProcess(cmds);
					}
				}
			}
			catch (Exception e)
			{
				OutputLog("Error: " + e.Message);
			}
			OutputLog("Generation completed!");
			//SendMail();
		}

#region SatSenInit
		/// <summary>
		/// get satellites and sensors
		/// </summary>
		void SatInit()
		{
			m_SatList.Clear();
			//read sat.db
			string QueryCmd = "select * from satellite";
			DataTable dtSat = m_Sqlite.Query(QueryCmd);

			if (dtSat ==null )
			{
				return;
			}
			for (int i = 0; i < dtSat.Rows.Count; i++)
			{
				DataRow dr = dtSat.Rows[i];
				//query properties
				string SatID = Convert.ToString(dr["ID"]);
				Dictionary<string, object> DictProperties = new Dictionary<string, object>();
				/*
				   QueryCmd = string.Format("select * from properties where id='{0}'", SatID);
				   DataTable dtProperties = m_Sqlite.Query(QueryCmd);
				   if (dtProperties != null)
				   {
				   foreach (DataRow drprop in dtProperties.Rows)
				   {
				   DictProperties[Convert.ToString(drprop[0])] = drprop[1];
				   }
				   }

*/
				DictProperties["Name"] = dr["NAME"];
				DictProperties["OleColor"] = dr["OLECOLOR"];
				DictProperties["IsChecked"] = dr["ISCHECKED"];

				Satellite sat = CreateSat(SatID, DictProperties);
				//query sensor
				QueryCmd = string.Format("select * from sensor where satid='{0}'", SatID);
				DataTable dtSen = m_Sqlite.Query(QueryCmd);
				if (dtSen == null)
				{
					continue;
				}
				foreach (DataRow drsen in dtSen.Rows)
				{
					Sensor sen = new Sensor(SatID, Convert.ToString(drsen["ID"]));
					sen.SenName = Convert.ToString(drsen["NAME"]);
					sen.SatName = Convert.ToString(drsen["SATNAME"]);
					sen.Resolution = Convert.ToDouble(drsen["RESOLUTION"]);
					sen.Width = Convert.ToDouble(drsen["WIDTH"]);
					sen.InitAngle = Convert.ToDouble(drsen["INITANGLE"]);
					sen.RightSideAngle = Convert.ToDouble(drsen["RIGHTSIDEANGLE"]);
					sen.LeftSideAngle = Convert.ToDouble(drsen["LEFTSIDEANGLE"]);
					sen.ObserveAngle = Convert.ToDouble(drsen["OBSERVEANGLE"]);
					sen.SenColor = ColorTranslator.FromOle(Convert.ToInt32(drsen["OLECOLOR"]));
					sen.Checked = Convert.ToBoolean(drsen["ISCHECKED"]);

					sat.SensorList.Add(sen);
				}

				m_SatList.Add(sat);
			}
		}


		Satellite CreateSat(string SatID, Dictionary<string, object> DictProperties)
		{
			SatFactory satfac = new SatFactory();
			Satellite sat = satfac.CreateSatellite(SatID, GetFromDic(DictProperties, "SatType", SatFactory.SatType.TLE));
			if (sat == null)
			{
				return null;
			}
			sat.SatName = GetFromDic(DictProperties, "Name", SatID);
			sat.DisplayColor = ColorTranslator.FromOle(GetFromDic(DictProperties, "OleColor", 0));
			sat.Checked = GetFromDic(DictProperties, "IsChecked", false);

			sat.DictProperties = DictProperties;

			//set current tle
			SetCurTle(sat as SatTLE, DateTime.Now);
			return sat;
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

		/// <summary>
		/// create new sat
		/// </summary>
		/// <param name="SatName"></param>
		/// <param name="DictProperties"></param>
		public bool CreateNewSat(string SatID, Dictionary<string, object> DictProperties)
		{
			//find existing satid
			DataTable dt = m_Sqlite.Query("select id from satellite where id='" + SatID + "'");
			if (dt != null && dt.Rows.Count != 0)
			{
				//satid should be unique
				m_LastErrorMsg = "SatID '" + SatID + "' already exist!";
				return false;
			}
			Satellite sat = new Satellite(SatID);
			m_SatList.Add(sat);

			//set current tle
			return SetCurTle(sat as SatTLE, DateTime.Now);
		}

		bool SetCurTle(SatTLE Sat, DateTime Dt)
		{
			//set current tle
			string[] tle;
			GetTLE(Sat, Dt, out tle);

			if (tle == null || tle.Length < 3)
			{
				m_LastErrorMsg = "can not set tle for '" + Sat.SatName + "'";
				return false;
			}
			else
			{
				(Sat as SatTLE).SetCurTLE(tle[0], tle[1], tle[2]);
				return true;
			}
		}

		void GetTLE(Satellite Sat, DateTime Dt, out string[] strTLE)
		{
			strTLE = new string[3] { "", "", "" };
			//to be tested
			TimeSpan TimeDist = new TimeSpan(800, 0, 0, 0);

			//get tle from db
			DateTime tleTime = DateTime.Parse("1/1/1000 1:00:01 AM");
			DateTime minTleTime = new DateTime(2008, 10, 10);

			DataTable datas = new DataTable();
			//find tle in one week
			string strDataCmd = "select * from TLE where satid= '" + Sat.NoardID + "' and time > '" +
				Dt.AddDays(-1).ToString(UTCTimeFormate) + "' and time < '" +
				Dt.AddDays(0.5).ToString(UTCTimeFormate) + "' order by time desc";

			datas = m_Sqlite.Query(strDataCmd);

			if (datas == null || datas.Rows.Count == 0) {
				//get random one
				strDataCmd = "select * from TLE where satid= '" + Sat.NoardID + "' order by time desc";
				datas = m_Sqlite.Query (strDataCmd);
				if (datas != null && datas.Rows.Count != 0) {
					strTLE [0] = datas.Rows [0] [0].ToString ();
					strTLE [1] = datas.Rows [0] ["LINE1"].ToString ();
					strTLE [2] = datas.Rows [0] ["LINE2"].ToString ();
				}
			} else {
				//get tle
				DataRow row = datas.Rows [0];
				if (datas != null && datas.Rows.Count != 0) {
					strTLE [0] = row [0].ToString ();
					strTLE [1] = row ["LINE1"].ToString ();
					strTLE [2] = row ["LINE2"].ToString ();
				}
			}
		}

#endregion

#region update tle
		bool UpdateTLEs(string SatID = "")
		{
			bool UpdateOK = false;
			string QueryCmd = "select url from tle_url";
			List<string> UpdateTleURLs = new List<string>();//更新TLE的网址
			Dictionary<string,string> DictNoardIdName = new Dictionary<string, string> ();
			DataTable dt = m_Sqlite.Query(QueryCmd);

			if (dt != null)
			{
				for (int i = 0; i < dt.Rows.Count; i++)
				{
					UpdateTleURLs.Add(dt.Rows[i][0].ToString());
				}
			}
			foreach (string saturl in UpdateTleURLs)
			{
				UpdateOK |= UpdatelSatTleFormURL(saturl, SatID,DictNoardIdName);
			}

			//update all_sat 
			OutputLog ("updating all satellites...");
			m_Sqlite.ExecuteSql ("drop table all_sat");
			m_Sqlite.ExecuteSql ("create table ALL_SAT(NOARDID,NAME,ADDED)");
			List<string> InsertAllSats = new List<string> ();
			foreach (KeyValuePair<string,string> kv in DictNoardIdName) {
				InsertAllSats.Add (string.Format("insert into all_sat values('{0}','{1}', 0)",
				                                 kv.Key,kv.Value));
			}

			UpdateOK |= m_Sqlite.BatProcess (InsertAllSats);
			UpdateOK |= m_Sqlite.ExecuteSql("update all_sat set ADDED=1 where noardid in (select id from satellite)");

			OutputLog ("updating all satellites completed!");
			return UpdateOK;
		}

		bool UpdatelSatTleFormURL(string updateurl, string SatID = "",Dictionary<string,string> Dict=null)
		{
			WebClient Client = new WebClient();
			string tmpfile = System.IO.Path.GetTempFileName();
			try
			{
				Client.DownloadFile(updateurl, tmpfile);
			}
			catch (Exception e)
			{
				m_LastErrorMsg = e.Message;
				return false;
			}

			OutputLog ("tle downloaded!");
			string line = "",name="",id="";
			int linecount = 0;
			string[] tle = new string[3];
			List<string[]> TleList = new List<string[]>();
			List<string> UpdateCmds = new List<string>();
			using (StreamReader sr = new StreamReader(tmpfile))
			{
				while ((line = sr.ReadLine()) != null && line.Length != 0)
				{
					//data is not null
					tle[linecount] = line;
					switch (linecount) {
						case 0:
							//satellite name
							name = line.Trim ();
							break;
						case 1:
						//line 1
							//noardid
							id = line.Substring (2, 6);
							if (Dict != null) { 
								Dict [id] = name;
							}
							break;
					case 2:
						//line 2 
						//add to list
							TleList.Add(tle);
							tle = new string[3];

							linecount = -1;
							break;
						default :
							break;
					}

					linecount++;
				}
			}

			OutputLog ("parsing tle completed!");
			//delete tles
			m_Sqlite.DeleteData ("delete from tle");
			//add new
			if (SatID.Length == 0)
			{
				//update all
				foreach (string[] tmptle in TleList)
				{
					string noradid = tmptle[1].Substring(2, 6);
							//update tle to db
							UpdateCmds.Add(GetUpdateTleCmd(noradid, tmptle[0], tmptle[1], tmptle[2]));
				}
			}
			else
			{
				Satellite sat = GetSat(SatID);
				if (sat != null)
				{
					foreach (string[] tmptle in TleList)
					{
						string noradid = tmptle[1].Substring(2, 6);
						if (string.Compare(noradid, SatID, true) == 0)
						{
							//update tle to db
							UpdateCmds.Add(GetUpdateTleCmd(noradid, tmptle[0], tmptle[1], tmptle[2]));
							break;
						}
					}
				}
			}

			//for(int i=0;i<UpdateCmds.Count;i++)
			//{
				//OutputLog(UpdateCmds[i]);
			//}
			return m_Sqlite.BatProcess(UpdateCmds);
		}

		string GetUpdateTleCmd(string SatID,  string line0, string line1, string line2)
		{
			string tletime = GetTLEtime(line0, line1, line2);
			string InsertCmd = string.Format("insert into tle values ('{0}','{1}','{2}','{3}')", SatID,
					 tletime, line1, line2);
			return InsertCmd;
		}

		/// <summary>
		/// get tle time in utc
		/// </summary>
		/// <param name="line0"></param>
		/// <param name="line1"></param>
		/// <param name="line2"></param>
		/// <returns></returns>
		static string GetTLEtime(string line0, string line1, string line2)
		{
			int dbyear; double dbday;
			dbyear = Convert.ToInt16(line1.Substring(18, 2));
			dbday = Convert.ToDouble(line1.Substring(20, 12));

			DateTime dt = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			dt = dt.AddYears(dbyear);
			dt = dt.AddDays(dbday - 1);

			return dt.ToString("yyyy-MM-dd HH:mm:ss");
		}

		Satellite GetSat(string SatName)
		{
			foreach (Satellite sat in m_SatList)
			{
				if (string.Compare(sat.NoardID, SatName, true) == 0 ||
						string.Compare(sat.SatName, SatName, true) == 0)
				{
					return sat;
				}
			}
			return null;
		}

#endregion
		void SendMail()
		{
			string fromemail = "wuhao3209@aol.com", toemail = "wuhao3209@aol.com",
				   passwd = "147896325", smtphost = "smtp.aol.com";
			MailAddress to = new MailAddress(toemail);
			MailAddress from = new MailAddress(fromemail);

			MailMessage mail = new MailMessage(from, to);
			DateTime DtHeadBJ = DateTime.UtcNow.AddHours(8);
			mail.Subject = "GenPath log " + DtHeadBJ.ToString("yyyy-MM-dd HH:mm:ss" + " (BJT)");

			string mailbody = "";
			foreach (string str in m_MailDetails)
			{
				mailbody += str + Environment.NewLine;
			}

			mail.Body = mailbody;

			SmtpClient smtp = new SmtpClient();
			smtp.Host = smtphost;
			smtp.Port = 587;

			smtp.Credentials = new NetworkCredential(fromemail, passwd);
			//smtp.EnableSsl = true;
			smtp.Send(mail);
		}
	}
}
