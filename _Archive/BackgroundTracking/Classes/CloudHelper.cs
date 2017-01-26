using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTracking.Classes
{
    public static class CloudHelper
    {
        public static async void WriteToCloud(CloudAppItem pURLActivityEntityT)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(GetConnectionString());
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            // Retrieve a reference to a container.
            CloudQueue queue = queueClient.GetQueueReference("beam-queue");

            // Create the queue if it doesn't already exist
            await queue.CreateIfNotExistsAsync();

            string serializedMessage = JsonConvert.SerializeObject(pURLActivityEntityT);
            CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(serializedMessage);
            queue.AddMessageAsync(cloudQueueMessage);
        }

        public static string GetConnectionString()
        {
            try
            {
                string connString = ConfigurationManager.AppSettings["StorageConnectionString"];
                if (string.IsNullOrEmpty(connString))
                {
                    return "UseDevelopmentStorage=true";
                }
                else
                {
                    return connString;
                }

            }
            catch (Exception)
            {
                return "UseDevelopmentStorage=true";
            }
        }
    }
}
