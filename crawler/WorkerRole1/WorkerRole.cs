using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using WebRole1;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using HtmlAgilityPack;
using System.Collections;
using System.Xml.Linq;
using System.Xml;

namespace WorkerRole1
{
    //This thing crawls the web
    public class WorkerRole : RoleEntryPoint
    {
        
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private CloudQueue queue; //URLs to crawl
        private CloudQueue admin; //commands to stop and start the crawler
        private CloudTable table; //the place things are stored
        private Boolean start = true;
        private int cCount;//urls crawled 
        private int counter; //size of index
        private Queue<urlTbl> urlQueue; //the queue of URLs that have been crawled and are waiting to be batched into the table
        private HashSet<string> checkList; //the set of URLs that have already been visted
        private List<string> errors;
        private string status; //crawlers current status
        private List<string> disallow; //directories to not go into
        private Queue<string> xml; //XML sitemaps to go through

        public override void Run()
        {
            int batch = 0;
            status = "crawling";
            while (true)
            {
                Thread.Sleep(50);
                batch++;
                //check for admin msg
                CloudQueueMessage message = queue.GetMessage(); 
                CloudQueueMessage adminMsg;
                
                    
                
                if (admin.PeekMessage() != null)
                {
                    adminMsg = admin.GetMessage();
                    if (adminMsg.AsString == "start")
                    {
                        //start up
                        init();
                        status = "crawling";

                        start = true;
                    }
                    else if (adminMsg.AsString == "stop")
                    {
                        //clear everything
                        //queue
                        //table
                        queue.Clear();
                        table.Delete();
                        status = "idle";
                        start = false;
                    }
                    admin.DeleteMessage(adminMsg);
                }
                
                if (start && message != null)
                {
                    string urlString = message.AsString;
                    addURL(urlString);
                    List<string> newURLS = getURLS(urlString);
                    List<string> checkedURLS = checkURLS(newURLS, urlString);
                    //switch to batch inserts
                    
                    foreach (string url in checkedURLS)
                    {
                        CloudQueueMessage u1 = new CloudQueueMessage(url);
                        queue.AddMessage(u1);
                    }
                    //3 is not a big batch number, but it wasnt working with a larger number
                    //atleast it saves some time
                    if (batch >= 10)
                    {
                        batch = 0;
                        batchData();
                    }
                    queue.DeleteMessage(message);
                }
                
            }
        }

        public void addURL(string urlString){
            //build the tableobject
            //assume is valid because checked before
            if (!checkList.Contains(urlString))
            {
                //get the page title
                string title = "";
                try
                {
                    HttpWebRequest request = (HttpWebRequest.Create(urlString) as HttpWebRequest);
                    HttpWebResponse response = (request.GetResponse() as HttpWebResponse);

                    using (Stream stream = response.GetResponseStream())
                    {
                        // compiled regex to check for <title></title> block
                        Regex titleCheck = new Regex(@"<title>\s*(.+?)\s*</title>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        int bytesToRead = 8092;
                        byte[] buffer = new byte[bytesToRead];
                        string contents = "";
                        int length = 0;
                        while ((length = stream.Read(buffer, 0, bytesToRead)) > 0)
                        {
                            
                            contents += Encoding.UTF8.GetString(buffer, 0, length);

                            Match m = titleCheck.Match(contents);
                            if (m.Success)
                            {
                                
                                title = m.Groups[1].Value.ToString();
                                break;
                            }
                            else if (contents.Contains("</head>"))
                            {
                                
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //failed to find the page title
                    errors.Add(urlString);
                }



                urlTbl temp = new urlTbl(urlString, title);
                checkList.Add(urlString);
                urlQueue.Enqueue(temp);
                counter++;


            }

        }

        public List<string> getURLS(string urlString)
        {
            //grab all the links on a page and return them so they can be added to the queue
            List<string> linksOnPage = new List<string>();
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = hw.Load(urlString);
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                HtmlAttribute att = link.Attributes["href"];
                linksOnPage.Add(att.Value);
            }
            return linksOnPage;
        }
        public List<string> checkURLS(List<string> newURLS, string urlString)
        {
            //makes sure each URL is valid for crawling before it is added to the queue
            List<string> checkedURLS = new List<string>();
            foreach (string value in newURLS)
            {

                
                string temp = "";
                if(urlString.Contains("cnn.com")){
                    if(!value.Contains("http")){
                        temp = "http://www.cnn.com" + value;
                    }
                    
                    
                    
                }
                else if (urlString.Contains("bleacherreport.com"))
                {
                    if (!value.Contains("http"))
                    {
                        temp = "http://bleacherreport.com" + value;
                    }
                    
                }
                
                    
                temp = value;
                
                    if (temp.Contains("cnn.com") || temp.Contains("bleacherreport.com"))
                    {
                        //maybe I should check to see if .html, but a lot of them are not
                        
                        foreach(string d in disallow){
                            if(temp.Contains(d)){
                                //this url is in a disallowed directory
                                errors.Add(temp);
                            } else{
                                checkedURLS.Add(temp);
                            }
                        }
                    }
                    else
                    {
                        //wroong website
                    }
                
                
                

                
            }
            cCount++;//each one of these is crawled, but may be rejected, will not match index size
            return checkedURLS;
            
        }

        public void batchData()
        {
            //dump all stats and urls to table
            TableBatchOperation batchBR = new TableBatchOperation();
            TableBatchOperation batchCNN = new TableBatchOperation();

            List<urlTbl> theRealLastTen = new List<urlTbl>();

            while(urlQueue.Count>0){
                urlTbl temp = urlQueue.Dequeue();
                if(urlQueue.Count<10){ //maybe should be <=
                    theRealLastTen.Add(temp);
                }
                if(temp.PartitionKey=="cnn"){
                    batchCNN.InsertOrReplace(temp);
                }
                else
                {
                    batchBR.InsertOrReplace(temp);
                }
                
            }
            

            if (batchBR.Count > 0)
            {
                table.ExecuteBatch(batchBR);
        
            }
            if (batchCNN.Count > 0)
            {
                table.ExecuteBatch(batchCNN);
            }
            
            //TableOperation insertOperation = TableOperation.Insert(customer1);
            
            errorTbl tempE = new errorTbl("error",errors);
            TableOperation insertError = TableOperation.InsertOrReplace(tempE);
            table.Execute(insertError);
            
           
            
            statTbl temp2 = new statTbl("count", counter+"");
            TableOperation insertCount = TableOperation.InsertOrReplace(temp2);
            table.Execute(insertCount);
            statTbl temp3 = new statTbl("cCount", cCount+"");
            TableOperation insertcC = TableOperation.InsertOrReplace(temp3);
            table.Execute(insertcC);
            statTbl temp4 = new statTbl("status", status);
            TableOperation insertStatus = TableOperation.InsertOrReplace(temp4);
            table.Execute(insertStatus);

            tenTbl temp5 = new tenTbl("lastten", theRealLastTen);
            TableOperation insertTen = TableOperation.InsertOrReplace(temp5);
            table.Execute(insertTen);

            
        }
        public void parseRobots(string content, string website){
            //find sitemaps and disallowed directories inn the robots.txt
            string[] lines = content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach(string line in lines){
                string[] words = line.Split(' ');
                if(words[0]=="Sitemap:"){
                    //add to xml list
                    //only use the NBA sitemap
                    if (words[1].Contains("cnn") || words[1].Contains("nba"))
                    {
                        xml.Enqueue(words[1]);
                    }
                    
                }
                if(words[0]=="Disallow:"){
                    //add to disallow list
                    disallow.Add(website+words[1]);
                }
            }
        }

        public void parseXML()
        {
            //go through the XML to find URLs, with some restrictions
            while(xml.Count>0){
                string x = xml.Dequeue();


                XElement sitemap = XElement.Load(x);



                
                if (x.Contains("cnn.com"))
                {
                    XName url = XName.Get("url", "http://www.sitemaps.org/schemas/sitemap/0.9");
                    XName loc = XName.Get("loc", "http://www.sitemaps.org/schemas/sitemap/0.9");
                    
                    foreach (var urlElement in sitemap.Elements(url))
                    {
                        var locElement = urlElement.Element(loc);
                        //make sure from this year
                        if (locElement.Value.Contains(".xml") && locElement.Value.Contains("2015"))
                        {
                            xml.Enqueue(locElement.Value);
                        }
                        else if (locElement.Value.Contains("2015") && (locElement.Value.Contains(".htm")||locElement.Value.Contains(".html")))
                        {
                            
                                
                                CloudQueueMessage u1 = new CloudQueueMessage(locElement.Value);
                                queue.AddMessage(u1);
                            
                        }
                    }

                     
                }
                else //if (x.Contains("bleacherreport.com"))
                {
                    XName url = XName.Get("url", "http://www.google.com/schemas/sitemap/0.9");
                    XName loc = XName.Get("loc", "http://www.google.com/schemas/sitemap/0.9");
                    foreach (var urlElement in sitemap.Elements(url))
                    {
                        var locElement = urlElement.Element(loc);
                       //I dont check for NBA here because I only allow the NBA sitemap to get here

                            CloudQueueMessage u1 = new CloudQueueMessage(locElement.Value);
                            queue.AddMessage(u1);


                        
                    }
                }
            }
        }

        public void init()
        {
            //start up function, ititalize variables and reset things
            
        
            //table.DeleteIfExists();
            table.CreateIfNotExists();
            queue.Clear();
            

            counter = 0;
            urlQueue = new Queue<urlTbl>();
            checkList = new HashSet<string>();
            errors = new List<string>();
            disallow = new List<string>();
            xml = new Queue<string>();
            urlQueue.Clear();
            checkList.Clear();
            errors.Clear();
            disallow.Clear();
            xml.Clear();


            status = "loading";
            batchData();


            WebClient client = new WebClient();
            Stream stream = client.OpenRead("http://bleacherreport.com/robots.txt");
            StreamReader reader = new StreamReader(stream);
            String content = reader.ReadToEnd();
            parseRobots(content, "http://bleacherreport.com");
            Stream stream2 = client.OpenRead("http://www.cnn.com/robots.txt");
            StreamReader reader2 = new StreamReader(stream2);
            String content2 = reader2.ReadToEnd();
            parseRobots(content2, "http://www.cnn.com");
            parseXML();
        }

        public override bool OnStart()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                           CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference("myurls");
            queue.CreateIfNotExists();
            admin = queueClient.GetQueueReference("admin");
            admin.CreateIfNotExists();
            admin.Clear();
           
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("sums");
            init();
            

            

            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
