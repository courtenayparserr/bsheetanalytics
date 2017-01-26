using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTracking.Classes
{
    public class ApplicationProcess :ITrackingItem
    {
        #region PROPERTIES

        private string pName = string.Empty;
        public string Name
        {
            get
            {
                return pName;
            }
        }

        private IntPtr pHandle = IntPtr.Zero;
        public IntPtr Handle
        {
            get
            {
                return pHandle;
            }
        }

        private Database.processes pProcessEntity = null;
        private Database.process_activities pProcessActivityEntity = null;
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

        private ActiveTracking pCurrentlyActiveTrack = null;



        #endregion

        #region CONSTRUCTORS

        public ApplicationProcess(string _name, IntPtr _handle)
        {
            pName = _name;
            pHandle = _handle;
            AddToDBProcesses();
        }

        #endregion

        #region EVENTS

        #endregion

        #region METHODS

        private void AddToDBProcesses()
        {
            pProcessEntity = new Database.processes() { name = pName, handle = pHandle.ToInt32(), sessions = Globals.Session };
            Globals.DBcontext.processes.Add(pProcessEntity);
            Globals.DBcontext.SaveChanges();

            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection(Classes.Globals.DBConnectionString))
            //{
            //    System.Data.SQLite.SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand("INSERT INTO processes (session_id, handle, name) VALUES (@p1, @p2, @p3)", con);
            //    cmd.Parameters.AddWithValue("p1", Globals.SessionID);
            //    cmd.Parameters.AddWithValue("p2", pHandle);
            //    cmd.Parameters.AddWithValue("p3", pName);
            //    con.Open();
            //    cmd.ExecuteNonQuery();

            //    cmd = new System.Data.SQLite.SQLiteCommand("select last_insert_rowid()", con);
            //    object oProcessID = cmd.ExecuteScalar();

            //    pID = oProcessID.ToString();

            //    con.Close();

            //}
        }

        public int Activate()
        {
            return Activate(DateTime.Now);
        }

        public int Activate(DateTime _Timestamp)
        {
            //ActiveTracking track = pActiveTimestamps.LastOrDefault(x => !x.IsDeactivated);
            //if (track == null)
            //{
            //    track = new ActiveTracking(_Timestamp);
            //    pActiveTimestamps.Add(track);
            //}
            //track.Activate(_Timestamp);

            if (pCurrentlyActiveTrack == null)
            {
                pCurrentlyActiveTrack = new ActiveTracking(_Timestamp);
                pActiveTimestamps.Add(pCurrentlyActiveTrack);
            }

            pCurrentlyActiveTrack.Activate(_Timestamp);

            if (pProcessActivityEntity == null)
            {
                pProcessActivityEntity = new Database.process_activities() { processes = pProcessEntity , started = _Timestamp.ToString() };
                Globals.DBcontext.process_activities.Add(pProcessActivityEntity);
                Globals.DBcontext.SaveChanges();

            }

            //return track.TotalActiveSeconds;
            return pCurrentlyActiveTrack.TotalActiveSeconds;

        }

        public int Deactivate()
        {
            return Deactivate(DateTime.Now);
        }

        public int Deactivate(DateTime _Timestamp)
        {
            //ActiveTracking track = pActiveTimestamps.LastOrDefault(x => !x.IsDeactivated);
            //if (track != null)
            if (pCurrentlyActiveTrack != null)
            {
                //track.Deactivate(_Timestamp);
                pCurrentlyActiveTrack.Deactivate(_Timestamp);

                if (pProcessActivityEntity != null)
                {
                    pProcessActivityEntity.ended = _Timestamp.ToString();
                    Globals.DBcontext.SaveChanges();
                    CloudAppItem urlToWrite = new CloudAppItem();
                    urlToWrite.ended = _Timestamp;
                    urlToWrite.started = Convert.ToDateTime(pProcessActivityEntity.started);
                    urlToWrite.app = pProcessActivityEntity.processes.name;
                    CloudHelper.WriteToCloud(urlToWrite);
                    pProcessActivityEntity = null;
                }

                //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection(Classes.Globals.DBConnectionString))
                //{
                //    System.Data.SQLite.SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand("UPDATE process_activities SET ended = @p1 WHERE id = @p2", con);
                //    cmd.Parameters.AddWithValue("p1", _Timestamp);
                //    cmd.Parameters.AddWithValue("p2", pActivityID);

                //    con.Open();
                //    cmd.ExecuteNonQuery();                    
                //    con.Close();

                //    pActivityID = string.Empty;


                //}

                //return track.TotalActiveSeconds;
                return pCurrentlyActiveTrack.TotalActiveSeconds;

            }
            else
            {
                return 0;
            }


        }
        #endregion
    }
}
