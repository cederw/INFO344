using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;

namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    [System.Web.Script.Services.ScriptService]
    public class WebService : System.Web.Services.WebService
    {
        //lol does this even initialize these things?
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                          CloudConfigurationManager.GetSetting("StorageConnectionString"));
         private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
         private static   CloudTable table = tableClient.GetTableReference("sums");
         


        private static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        private static CloudQueue admin = queueClient.GetQueueReference("admin");
        private static CloudQueue queue = queueClient.GetQueueReference("myurls");
        private PerformanceCounter memProocess = new PerformanceCounter("Memory", "Available MBytes");
        private PerformanceCounter totalProcessorTimeCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private JavaScriptSerializer serializer = new JavaScriptSerializer();

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Start()
        {
            //start the crawler
           
            
            admin.CreateIfNotExists();
            CloudQueueMessage bl = new CloudQueueMessage("start");
            admin.AddMessage(bl);
            var json = serializer.Serialize("Started");
            return json;
        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Stop()
        {
            //stop the crawler
            admin.CreateIfNotExists();
            CloudQueueMessage bl = new CloudQueueMessage("stop");
            admin.AddMessage(bl);
            var json = serializer.Serialize("stopped");
            return json;
        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Retrieve(string urlString)
        {
            //get the title related to the passed url
            string pKey = "";
            string results;
            if (urlString.Contains("cnn.com"))
            {
                pKey = "cnn";
            }
            else
            {
                pKey = "bleacherReport";
            }
            string row = urlString.Replace('/', '.');
            row = row.Replace('\\', '.');
            row = row.Replace('#', '.');
            row = row.Replace('?', '.');
            table.CreateIfNotExists();
            TableOperation retrieveOperation = TableOperation.Retrieve<urlTbl>(pKey, row);
            TableResult retrievedResult = table.Execute(retrieveOperation);
            urlTbl fetchedEntity = retrievedResult.Result as urlTbl;
            if (fetchedEntity != null)
            {
                results =  fetchedEntity.title;
            }
            else
            {
                results =  "That URL has not been crawled yet :(";
            }
            
            var json = serializer.Serialize(results);
            return json;
        }
     
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        //returns how mant MBytes are avalible.
        public string GetAvailableMBytes()
        {
            //MB of RAM availible
            float memUsage = memProocess.NextValue();
            string results =  memUsage+"MB";
            var json = serializer.Serialize(results);
            return json;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        //returns how mant MBytes are avalible.
        public string GetCPU()
        {
                //CPU % in use
            totalProcessorTimeCounter.NextValue();
            System.Threading.Thread.Sleep(1000);// 1 second wait
            string results = totalProcessorTimeCounter.NextValue()+"%";
            var json = serializer.Serialize(results);
            return json;
        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetQCount()
        {
            //returns the urls still in queue
            queue.FetchAttributes();
            string results = queue.ApproximateMessageCount.Value+"";
            var json = serializer.Serialize(results);
            return json;
        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetURLCount()
        {
            //returns the urls that have been crawled
            string results;
            table.CreateIfNotExists();
            TableOperation retrieveOperation = TableOperation.Retrieve<statTbl>("cCount", "row");
            TableResult retrievedResult = table.Execute(retrieveOperation);
            statTbl fetchedEntity = retrievedResult.Result as statTbl;
            if (fetchedEntity != null)
            {
                results = fetchedEntity.data;
            }
            else
            {
                results = 0+"";
            }
            var json = serializer.Serialize(results);
            return json;
        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetLastTen()
        {
            //returns the last 10 crawled URLs
            string results;
            table.CreateIfNotExists();
            TableOperation retrieveOperation = TableOperation.Retrieve<tenTbl>("lastten", "list");
            TableResult retrievedResult = table.Execute(retrieveOperation);
            tenTbl fetchedEntity = retrievedResult.Result as tenTbl;
            if (fetchedEntity != null)
            {
                results = fetchedEntity.ten;
            }
            else
            {
                string empty = "";
                results = empty;
            }
            var json = serializer.Serialize(results);
            return json;
        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetErrors()
        {
            //returns errors
            string results;
            table.CreateIfNotExists();
            TableOperation retrieveOperation = TableOperation.Retrieve<errorTbl>("error", "errors");
            TableResult retrievedResult = table.Execute(retrieveOperation);
            errorTbl fetchedEntity = retrievedResult.Result as errorTbl;
            if (fetchedEntity != null)
            {
                results =  fetchedEntity.ten;
            }
            else
            {
                string empty = "";
                results = empty;
                
            }
            var json = serializer.Serialize(results);
            return json;
        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetIndexSize()
        {
            //return size of the table
            string results;
            table.CreateIfNotExists();
            TableOperation retrieveOperation = TableOperation.Retrieve<statTbl>("count", "row");
            TableResult retrievedResult = table.Execute(retrieveOperation);
            statTbl fetchedEntity = retrievedResult.Result as statTbl;
            if (fetchedEntity != null)
            {
                results = fetchedEntity.data;
            }
            else
            {
                results =  0+"";
            }
            var json = serializer.Serialize(results);
            return json;
        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetStatus()
        {
            //return status of the crawler
            string results;
            table.CreateIfNotExists();
            TableOperation retrieveOperation = TableOperation.Retrieve<statTbl>("status", "row");
            TableResult retrievedResult = table.Execute(retrieveOperation);
            statTbl fetchedEntity = retrievedResult.Result as statTbl;
            if (fetchedEntity != null)
            {
                results = fetchedEntity.data;
            }
            else
            {
                results = "Idle";
            }
            var json = serializer.Serialize(results);
            return json;
        }
    }
}
