using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTrackingCustomActions
{
    public class SQLiteCommunication
    {

        public bool CreateDatabase(string _DatabaseLocation)
        {
            bool returnValue = false;

            if (!System.IO.File.Exists(_DatabaseLocation))
            {
                System.Data.SQLite.SQLiteConnection.CreateFile(_DatabaseLocation);

                using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("Data Source=" + _DatabaseLocation + "; Version=3;"))
                {
                    string sql = "CREATE TABLE sessions(id INTEGER PRIMARY KEY, started TEXT NOT NULL)";
                    System.Data.SQLite.SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand(sql, con);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    sql = "CREATE TABLE processes(id INTEGER PRIMARY KEY, session_id INT, handle int, name TEXT, FOREIGN KEY(session_id) REFERENCES sessions(id))";
                    cmd = new System.Data.SQLite.SQLiteCommand(sql, con);
                    cmd.ExecuteNonQuery();
                    sql = "CREATE TABLE urls(id INTEGER PRIMARY KEY, session_id INT, url TEXT, FOREIGN KEY(session_id) REFERENCES sessions(id))";
                    cmd = new System.Data.SQLite.SQLiteCommand(sql, con);
                    cmd.ExecuteNonQuery();
                    sql = "CREATE TABLE process_activities(id INTEGER PRIMARY KEY, started TEXT NOT NULL, ended TEXT NULL, process_id int, FOREIGN KEY(process_id) REFERENCES processes(id))";
                    cmd = new System.Data.SQLite.SQLiteCommand(sql, con);
                    cmd.ExecuteNonQuery();
                    sql = "CREATE TABLE url_activities(id INTEGER PRIMARY KEY, started TEXT NOT NULL, ended TEXT NULL, url_id int, FOREIGN KEY(url_id) REFERENCES urls(id))";
                    cmd = new System.Data.SQLite.SQLiteCommand(sql, con);
                    cmd.ExecuteNonQuery();

                    sql = "CREATE TABLE user_profile(id INTEGER PRIMARY KEY, email TEXT NOT NULL)";
                    cmd = new System.Data.SQLite.SQLiteCommand(sql, con);
                    cmd.ExecuteNonQuery();

                    con.Close();
                    returnValue = true;
                }

            }


            return returnValue;

        }

        public bool StoreUserEmail(string _Email, string _DatabaseLocation)
        {
            bool returnValue = false;

            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection("Data Source=" + _DatabaseLocation + "; Version=3;"))
            {
                string sql = "INSERT INTO user_profile (email) VALUES (@p1)";
                System.Data.SQLite.SQLiteCommand cmd = new System.Data.SQLite.SQLiteCommand(sql, con);
                cmd.Parameters.AddWithValue("@p1", _Email);
                cmd.ExecuteNonQuery();

                con.Close();
                returnValue = true;

            }

            return returnValue;

        }

    }
}
