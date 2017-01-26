using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BackgroundTrackingCustomActions
{
    public class APICommunication
    {
        private string pRootURL = "http://app.getbeam.co/api";
        private Dictionary<string, string> pEndpoints = new Dictionary<string, string>();

        public APICommunication()
        {
            pEndpoints.Add("EmailCheck", "user");
        }

        public string CheckUserEmail(string _Email)
        {
            string userEndpoint = pEndpoints["EmailCheck"] ;

            string returnValue = string.Empty;

            if (!string.IsNullOrEmpty(userEndpoint ))
            {

                string url = string.Format("{0}/{1}?id={2}", pRootURL, userEndpoint, _Email);


                string jsonResponse = HTTPNodes.GetAsync(url);

                

                returnValue = jsonResponse;

            }


            return returnValue;
        }

    }
}
