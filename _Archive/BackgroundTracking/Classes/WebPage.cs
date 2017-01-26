using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BackgroundTracking.Classes
{
    public class WebPage :ITrackingItem
    {
        #region PROPERTIES

        private string pURL = string.Empty;
        public string URL
        {
            get
            {
                return pURL;
            }
        }


        private Database.urls pURLEntity = null;
        private Database.url_activities pURLActivityEntity = null;
        //private string pID = string.Empty;
        //private string pActivityID = string.Empty;


        private List<ActiveTracking> pActiveTimestamps = new List<ActiveTracking>();

        public int TotalActiveSeconds
        {
            get
            {
                return pActiveTimestamps.Sum(x => x.TotalActiveSeconds);
            }

        }



        #endregion
        #region CONSTRUCTORS

        public WebPage(string _Url)
        {
            pURL = _Url;
            AddToDBProcesses();


        }

        #endregion
        #region EVENTS

        #endregion

        #region METHODS
        private void AddToDBProcesses()
        {
            pURLEntity = new Database.urls() { url = pURL, sessions = Globals.Session };
            Globals.DBcontext.urls.Add(pURLEntity);
            Globals.DBcontext.SaveChanges();

            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection(Classes.Globals.DBConnectionString))
            //{
            //    System.Data.SQLite.SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand("INSERT INTO urls (session_id, url) VALUES (@p1, @p2)", con);
            //    cmd.Parameters.AddWithValue("p1", Globals.SessionID);
            //    cmd.Parameters.AddWithValue("p2", pURL);
            //    con.Open();
            //    cmd.ExecuteNonQuery();

            //    cmd = new System.Data.SQLite.SQLiteCommand("select last_insert_rowid()", con);
            //    object oURL = cmd.ExecuteScalar();

            //    pID = oURL.ToString();

            //    con.Close();

            //}
        }


        public int Activate()
        {
            return Activate(DateTime.Now);
        }

        public int Activate(DateTime _Timestamp)
        {
            ActiveTracking track = pActiveTimestamps.LastOrDefault(x => !x.IsDeactivated);
            if (track == null)
            {
                track = new ActiveTracking(_Timestamp);
                pActiveTimestamps.Add(track);
            }
            track.Activate(_Timestamp);

            if (pURLActivityEntity == null)
            {
                pURLActivityEntity = new Database.url_activities() { urls = pURLEntity, started = _Timestamp.ToString() };
                Globals.DBcontext.url_activities.Add(pURLActivityEntity);
                Globals.DBcontext.SaveChanges();

                //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection(Classes.Globals.DBConnectionString))
                //{
                //    System.Data.SQLite.SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand("INSERT INTO url_activities (started, url_id) VALUES (@p1, @p2)", con);
                //    cmd.Parameters.AddWithValue("p1", _Timestamp);
                //    cmd.Parameters.AddWithValue("p2", pID);

                //    con.Open();
                //    cmd.ExecuteNonQuery();

                //    cmd = new System.Data.SQLite.SQLiteCommand("select last_insert_rowid()", con);
                //    object oActivityID = cmd.ExecuteScalar();

                //    pActivityID = oActivityID.ToString();

                //    con.Close();

                //}

            }


            return track.TotalActiveSeconds;

        }

        public int Deactivate()
        {
            return Deactivate(DateTime.Now);
        }

        public int Deactivate(DateTime _Timestamp)
        {
            ActiveTracking track = pActiveTimestamps.LastOrDefault(x => !x.IsDeactivated);
            if (track != null)
            {
                track.Deactivate(_Timestamp);

                if (pURLActivityEntity != null)
                {
                    pURLActivityEntity.ended = _Timestamp.ToString();
                    Globals.DBcontext.SaveChanges();
                    CloudAppItem urlToWrite = new CloudAppItem();
                    urlToWrite.ended = _Timestamp;
                    urlToWrite.started = Convert.ToDateTime(pURLActivityEntity.started);
                    urlToWrite.url = pURLActivityEntity.urls.url;
                    CloudHelper.WriteToCloud(urlToWrite);
                    pURLActivityEntity = null;
                }


                //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection(Classes.Globals.DBConnectionString))
                //{
                //    System.Data.SQLite.SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand("UPDATE url_activities SET ended = @p1 WHERE id = @p2", con);
                //    cmd.Parameters.AddWithValue("p1", _Timestamp);
                //    cmd.Parameters.AddWithValue("p2", pActivityID);

                //    con.Open();
                //    cmd.ExecuteNonQuery();
                //    con.Close();

                //    pActivityID = string.Empty;


                //}

                return track.TotalActiveSeconds;
            }
            else
            {
                return 0;
            }

            
        }




        #endregion
    }
}
