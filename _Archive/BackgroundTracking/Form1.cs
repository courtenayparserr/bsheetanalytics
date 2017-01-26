using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackgroundTracking
{
    public partial class Form1 : Form
    {
        #region PROPERTIES

        private Classes.TrackingHandler tracker = null;
        
        #endregion

        #region CONSTRUCTORS
        
        public Form1()
        {
            InitializeComponent();

            InitializeThis();

        }
        #endregion

        #region EVENTS
        
        private void btnRUN_Click(object sender, EventArgs e)
        {

           

        }

        private void Form1_Resize(object sender, System.EventArgs e)
        {

            //if ( FormWindowState.Minimized == WindowState)
            //{
                Hide();
            //}
        }

        private void iconTracking_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //Show();
            //WindowState = FormWindowState.Normal;
        }

        private void Tracker_SecondPassedReached(string _Text)
        {
            iconTracking.Text = _Text;
        }


        private void btnWriteToFile_Click(object sender, EventArgs e)
        {

            WriteToFile();
        }

        private void writeToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WriteToFile();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.ExitThread();
        }

        private  void OnProcessExit(object sender, EventArgs e)
        {
            if (tracker != null)
            {
                tracker.Token.Cancel();
                tracker.SaveAndClose();
            }
        }


        #endregion




        #region METHODS

        private void InitializeThis()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            tracker = new Classes.TrackingHandler();
            tracker.SecondPassedReached += Tracker_SecondPassedReached;

            string dblocation = Environment.GetEnvironmentVariable("LocalAppData") + "\\BackgroundTracking\\TrackingData"; //System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Database\\";

            //if (!System.IO.Directory.Exists(dblocation))
            //{
            //    System.IO.Directory.CreateDirectory(dblocation);
            //}

            //dblocation += "TrackingData";

            if (!System.IO.File.Exists(dblocation))
            {
                MessageBox.Show("Database not available!");

                Application.Exit();
                
                //System.Data.SQLite.SQLiteConnection.CreateFile(dblocation);

                //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("Data Source=" + dblocation + "; Version=3;"))
                //{
                //    string sql = "CREATE TABLE sessions(id INTEGER PRIMARY KEY, started TEXT NOT NULL)";
                //    System.Data.SQLite.SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand(sql, con);
                //    con.Open();
                //    cmd.ExecuteNonQuery();
                //    sql = "CREATE TABLE processes(id INTEGER PRIMARY KEY, session_id INT, handle int, name TEXT, FOREIGN KEY(session_id) REFERENCES sessions(id))";
                //    cmd = new System.Data.SQLite.SQLiteCommand(sql, con);
                //    cmd.ExecuteNonQuery();
                //    sql = "CREATE TABLE urls(id INTEGER PRIMARY KEY, session_id INT, url TEXT, FOREIGN KEY(session_id) REFERENCES sessions(id))";
                //    cmd = new System.Data.SQLite.SQLiteCommand(sql, con);
                //    cmd.ExecuteNonQuery();
                //    sql = "CREATE TABLE process_activities(id INTEGER PRIMARY KEY, started TEXT NOT NULL, ended TEXT NULL, process_id int, FOREIGN KEY(process_id) REFERENCES processes(id))";
                //    cmd = new System.Data.SQLite.SQLiteCommand(sql, con);
                //    cmd.ExecuteNonQuery();
                //    sql = "CREATE TABLE url_activities(id INTEGER PRIMARY KEY, started TEXT NOT NULL, ended TEXT NULL, url_id int, FOREIGN KEY(url_id) REFERENCES urls(id))";
                //    cmd = new System.Data.SQLite.SQLiteCommand(sql, con);
                //    cmd.ExecuteNonQuery();
                //    con.Close();
                //}

            }

            Classes.Globals.DBConnectionString = "Data Source=" + dblocation + "; Version=3;";

            Classes.Globals.DBcontext= new Database.TrackingDataModel(Classes.Globals.DBConnectionString);

            Classes.Globals.Session = new Database.sessions() { started = DateTime.Now.ToString() };
            Classes.Globals.DBcontext.sessions.Add(Classes.Globals.Session);
            Classes.Globals.DBcontext.SaveChanges();


            //using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection(Classes.Globals.DBConnectionString))
            //{
            //    System.Data.SQLite.SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand("INSERT INTO sessions (started) VALUES (datetime('now'))", con);
            //    con.Open();
            //    cmd.ExecuteNonQuery();

            //    cmd = new System.Data.SQLite.SQLiteCommand("select last_insert_rowid()", con);
            //    object oSessionID = cmd.ExecuteScalar();

            //    Classes.Globals.SessionID = oSessionID.ToString();

            //    con.Close();

            //}
            
            tracker.StartTimer();

            this.WindowState = FormWindowState.Minimized;

        }

        private void WriteToFile()
        {


            //public TrackingDataEntities(string _ConnectionString)
            //    : base(new SQLiteConnection() { ConnectionString = _ConnectionString }, true)
            //{
            //}
            
            var processActivities = (from pa in Classes.Globals.DBcontext.process_activities
                                         //join p in context.processes on pa.processes equals p
                                         //where string.Compare(pa.processes.session_id.ToString(), Classes.Globals.SessionID) == 0 //&& pa.ended != null && pa.started != null
                                     where pa.processes.sessions.id == Classes.Globals.Session.id
                                     //select new { Name = pa.processes.name, Started = pa.started, Ended = pa.ended }
                                     select pa
                             ).ToList();



            StringBuilder report = new StringBuilder();

            Dictionary<string, int> processes = new Dictionary<string, int>();

            foreach (var p in processActivities)
            {

                DateTime startedTemp, endedTemp;

                if (DateTime.TryParse(p.started, out startedTemp) && DateTime.TryParse(p.ended, out endedTemp))
                {
                    int seconds = (endedTemp - startedTemp).Seconds;

                    if (processes.ContainsKey(p.processes.name))
                    {
                        processes[p.processes.name] += seconds;
                    }
                    else
                    {
                        processes.Add(p.processes.name, seconds);
                    }

                }
            }

            report.AppendLine(string.Join(Environment.NewLine, processes.Select(x => x.Key + " > " + x.Value)));



            var urlActivities = (from url in Classes.Globals.DBcontext.url_activities
                                     //join p in context.processes on pa.processes equals p
                                     //where string.Compare(pa.processes.session_id.ToString(), Classes.Globals.SessionID) == 0 //&& pa.ended != null && pa.started != null
                                 where url.urls.sessions.id == Classes.Globals.Session.id
                                 //select new { Name = pa.processes.name, Started = pa.started, Ended = pa.ended }
                                 select url
                             ).ToList();





            Dictionary<string, int> urls = new Dictionary<string, int>();

            foreach (var u in urlActivities)
            {

                DateTime startedTemp, endedTemp;

                if (DateTime.TryParse(u.started, out startedTemp) && DateTime.TryParse(u.ended, out endedTemp))
                {
                    int seconds = (endedTemp - startedTemp).Seconds;

                    if (urls.ContainsKey(u.urls.url))
                    {
                        urls[u.urls.url] += seconds;
                    }
                    else
                    {
                        urls.Add(u.urls.url, seconds);
                    }

                }
            }

            report.AppendLine(string.Join(Environment.NewLine, urls.Select(x => x.Key + " > " + x.Value)));



            string reportFile = Environment.GetEnvironmentVariable("LocalAppData") + "\\BackgroundTracking\\report.log"; //System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\report.log";

            System.IO.File.WriteAllText(reportFile, report.ToString());

            System.Diagnostics.Process.Start(reportFile);
        }





        #endregion

    }
}
