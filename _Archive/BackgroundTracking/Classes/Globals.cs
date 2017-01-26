using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTracking.Classes
{
    public static class Globals
    {

        //private static string pDBConnectionString;
        public static string DBConnectionString { get; set; }

        public static Database.TrackingDataModel DBcontext { get; set; }

        public static Database.sessions Session { get; set; }


    }
}
