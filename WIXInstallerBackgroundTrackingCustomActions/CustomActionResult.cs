using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTrackingCustomActions
{
    public class CustomActionResult
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }

        public string Message { get; set; }

        private Dictionary<string, string> pData = new Dictionary<string, string>();
        public Dictionary<string, string> Data
        {
            get
            {
                return pData;
            }
        }
    }
}
