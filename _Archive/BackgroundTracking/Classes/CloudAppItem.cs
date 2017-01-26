using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTracking.Classes
{
    public class CloudAppItem
    {
        public string url { get; set; }
        public string app { get; set; }
        public string email { get; set; }
        public string dbId { get; set; }
        public DateTime started { get; set; }
        public DateTime ended { get; set; }
    }
}
