using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomAction
{
    public class APICommunication
    {
        private string pRootURL = "http://getbeam.co/api";
        private Dictionary<string, string> pEndpoints = new Dictionary<string, string>();

        public APICommunication()
        {
            pEndpoints.Add("EmailCheck", "user");
        }

        public bool CheckUserEmail(string _Email)
        {
            string userEndpoint = pEndpoints["EmailCheck"] ;

            bool returnValue = false;

            if (!string.IsNullOrEmpty(userEndpoint ))
            {

                string url = string.Format("{0}/{1}/{2}", pRootURL, userEndpoint, _Email);
                


                try
                {

                    string jsonResponse = HTTPNodes.GetAsync(url);
                    //_Order.Sent = DateTime.Now;

                    bool bTemp;
                    if (bool.TryParse(jsonResponse, out bTemp))
                    {
                        returnValue = bTemp;
                    }
                    
                }                
                catch (Exception ex)
                {

                   

                }

            }

            
            return returnValue;
        }

    }
}
