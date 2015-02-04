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

/// <summary>
/// Summary description for WebService
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
[System.Web.Script.Services.ScriptService]
public class WebService : System.Web.Services.WebService
{
    private List<String> Suggestions = new List<String>();
    private static Trie wikiTrie;
    private PerformanceCounter memProcess = new PerformanceCounter("Memory", "Available MBytes");
    private static Boolean built = false;
    public WebService()
    {


        //Uncomment the following line if using designed components
        //InitializeComponent();
        if(!built){
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
      CloudConfigurationManager.GetSetting("StorageConnectionString"));

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
            while ((line = file.ReadLine()) != null)
            {
                float memUsage = memProcess.NextValue();
                if (memUsage < 50)
                {
                    break;
                }
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
        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(results);
        return json;
    }
}