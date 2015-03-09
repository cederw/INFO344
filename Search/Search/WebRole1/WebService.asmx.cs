using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure;
using System.IO;
using WebRole1;
using System.Diagnostics;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Net;

/// <summary>
/// Summary description for WebService
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
[System.Web.Script.Services.ScriptService]
public class WebService : System.Web.Services.WebService
{
    private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                          CloudConfigurationManager.GetSetting("StorageConnectionString"));
    private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
    private static CloudTable table = tableClient.GetTableReference("sums");



    private static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
    private static CloudQueue admin = queueClient.GetQueueReference("admin");
    private static CloudQueue queue = queueClient.GetQueueReference("myurls");
    private PerformanceCounter memProocess = new PerformanceCounter("Memory", "Available MBytes");
    private PerformanceCounter totalProcessorTimeCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    private JavaScriptSerializer serializer = new JavaScriptSerializer();


    private int countLines = 0;
    private string lastTitle = "";
    private static Dictionary<string, List<string>> cache = new Dictionary<string, List<string>>();


    private List<String> Suggestions = new List<String>();
    private static Trie wikiTrie;
    private static Boolean built = false;
    public WebService()
    {


        //Uncomment the following line if using designed components
        //InitializeComponent();
        if(!built){
            countLines = 0;

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("test");

            // Retrieve reference to a blob named "myblob.txt"
            CloudBlockBlob blockBlob2 = container.GetBlockBlobReference("enwiki-20150112-all-titles-in-ns0preProcessed");

            string filePath = System.IO.Path.GetTempPath() + "\\wiki.txt";

            using (var fileStream = System.IO.File.OpenWrite(filePath))
            {
                blockBlob2.DownloadToStream(fileStream);
            }
            string line;
            wikiTrie = new Trie();
            //now find the file and build the trie until 50 MB left
            System.IO.StreamReader file = new System.IO.StreamReader(filePath);
            while ((line = file.ReadLine()) != null && memProocess.NextValue() > 9000)
            {
                countLines++;
                wikiTrie.AddTitle(line);
            }
            built = true;
        }
        
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]

    public string suggest(string search)
    {
        //tell the trie to do the work and then return it to ajax call as a json
        string[] results = wikiTrie.SearchForPrefix(search);
        
        for (int i = 0; i < results.Length; i++)
        {
            Suggestions.Add(results[i]);
        }
        lastTitle = results[results.Length - 1];
        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(results);
        return json;
    }
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
            results = fetchedEntity.title;
        }
        else
        {
            results = "That URL has not been crawled yet :(";
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
        string results = memUsage + "MB";
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
        string results = totalProcessorTimeCounter.NextValue() + "%";
        var json = serializer.Serialize(results);
        return json;
    }
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetQCount()
    {
        //returns the urls still in queue
        queue.FetchAttributes();
        string results = queue.ApproximateMessageCount.Value + "";
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
            results = 0 + "";
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
            results = 0 + "";
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
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetLines()
    {
        //return status of the crawler
        string results = countLines+"";
        
        var json = serializer.Serialize(results);
        return json;
    }
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetLastTitle()
    {
        //return size of the table
        string results = lastTitle;
        
        
        var json = serializer.Serialize(results);
        return json;
    }
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string search(string search)
    {
        List<string> results = new List<string>();
        if (cache.ContainsKey(search))
        {
            results =  cache[search];
        }
        else
        {
            
            string phpString = search.Replace(' ', '+'); //////////////////////make check all 
            string url = "http://ec2-54-149-16-161.us-west-2.compute.amazonaws.com/api.php?name=" + phpString;
            var request = WebRequest.Create(url);
            string text = "";
            var response = (HttpWebResponse)request.GetResponse();
            request.ContentType = "application/json; charset=utf-8";

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd(); //goonna have no idea what the response looks like
            }
            if (text == "")
            { //error
                results.Add("NBAerror");
            }
            else
            {
                results.Add(text);//CSV THE PLAYER.. or just pass the json x.x
            }

            //Dictionary<string, int> counts = new Dictionary<string, int>();
            List<urlTbl> urls = new List<urlTbl>();
            foreach(string keyword in search.Split(' ')){
                TableQuery<urlTbl> query = new TableQuery<urlTbl>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, keyword));
                foreach (urlTbl entity in table.ExecuteQuery(query))
                {
                   // if(counts.ContainsKey(entity.url)){
                    //    counts[entity.url] = counts[entity.url]++;
                    //} else{
                        urls.Add(entity);
                     //   counts.Add(entity.url,1);
                    //}
                    
                }
            }

            var ranked = urls.Where(x => true).GroupBy(x => x.url).Select(x => new
            {
               
                Count = x.Count()
            }).OrderByDescending(x => x.Count);
            //search table
            //rder by linq

            //cache 
            cache.Add(search, results);
            
        }
        var json = serializer.Serialize(results);
        return json;
        
    }
}