using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace CustomAction
{
    public static class HTTPNodes
    {
        public static string GetAsync(string _url)
        {
            string returnValue = string.Empty;

            System.Net.Cache.HttpRequestCachePolicy noCachePolicy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(_url);
            

            httpWebRequest.CachePolicy = noCachePolicy;


            httpWebRequest.Method = "GET";
            HttpWebResponse response = null;

            using (WebResponse webResponse =  httpWebRequest.GetResponse())
            {
                response = (HttpWebResponse)webResponse;

                Stream objStream = response.GetResponseStream();

                StreamReader objReader = new StreamReader(objStream, Encoding.Default);

                returnValue =  objReader.ReadToEnd();


                return returnValue;

            }

        }

    }
}
