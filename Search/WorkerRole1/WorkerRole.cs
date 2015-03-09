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
        private HashSet<string> disallow; //directories to not go into
        private Queue<string> xml; //XML sitemaps to go through

        public override void Run()
        {
            int batch = 0;
            status = "crawling";
            statTbl temp4 = new statTbl("status", status);
            TableOperation insertStatus = TableOperation.InsertOrReplace(temp4);
            table.ExecuteAsync(insertStatus);
            CloudQueueMessage message;
            CloudQueueMessage adminMsg;
            disallow.Add("mailto");
            while (true)
            {
                Thread.Sleep(50);
                batch++;
                //check for admin msg
                
                
                    
                
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
                message = queue.GetMessage();
                if (start && message != null)
                {
                    string urlString = message.AsString;
                    if (!checkList.Contains(urlString) )
                    {
                        HashSet<string> newURLS = getURLS(urlString);
                        HashSet<string> checkedURLS = checkURLS(newURLS, urlString);
                        //switch to batch inserts

                        foreach (string url in checkedURLS)
                        {
                            CloudQueueMessage u1 = new CloudQueueMessage(url);
                            queue.AddMessageAsync(u1);
                        }

                        //3 is not a big batch number, but it wasnt working with a larger number
                        //atleast it saves some time
                        if (batch >= 10)
                        {
                            batch = 0;
                            batchData();
                        }
                    }
                    queue.DeleteMessage(message);
                }
                
            }
        }


        public HashSet<string> getURLS(string urlString)
        {
            //first crawl the passed page and build its table object, then store it to be inserted later. 
            //then grab all the links on a page and return them so they can be added to the queue
            HashSet<string> linksOnPage = new HashSet<string>();
            HtmlWeb hw = new HtmlWeb();
            try
            {
                HtmlDocument doc;
                if (urlString.Contains("//") && !urlString.Contains("http://"))
                {
                     doc = hw.Load("http:"+urlString);
                     urlString = "http:" + urlString;
                }
                else
                {
                     doc = hw.Load(urlString);
                }
                

                //get the page title
                //doc.DocumentNode.SelectSingleNode("//head/title")
                string title = doc.DocumentNode.SelectSingleNode("//head/title").InnerHtml;
                HtmlNode mdnode = doc.DocumentNode.SelectSingleNode("//meta[@name='pubdate']");
                string date = "no date";
                if (mdnode != null)
                {
                    HtmlAttribute desc;

                    desc = mdnode.Attributes["content"];
                    date = desc.Value;
                } 




                foreach (string keyword in title.Split(' ')){
                    urlTbl temp = new urlTbl(urlString, title, keyword.ToLower(), date);
                    urlQueue.Enqueue(temp);
                    counter++;
                }
                
                checkList.Add(urlString);
                
                



                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    HtmlAttribute att = link.Attributes["href"];
                    if (!att.Value.Contains("http"))
                    {
                        linksOnPage.Add(att.Value);
                    }

                }
            }
            catch
            {
                if (!errors.Contains(urlString))
                {
                    errors.Add(urlString);
                    checkList.Add(urlString);
                }
                
            }
            
            return linksOnPage;
        }
        public HashSet<string> checkURLS(HashSet<string> newURLS, string urlString)
        {
            //makes sure each URL is valid for crawling before it is added to the queue
            HashSet<string> checkedURLS = new HashSet<string>();
            foreach (string value in newURLS)
            {
                bool e = false;
                
                string temp = "";

                if (!value.Contains("http"))
                {
                    if (value.Contains("www."))
                    {
                        temp = "http://" + value;
                    } else if(value.Contains(".cnn.com")){
                        string[] vals = value.Split('.');
                        temp = "http://";
                        for(int i = 0; i<vals.Length;i++){
                            if (!vals[i].Contains("cnn"))
                            {
                                temp = temp + vals[i]+".";
                            }
                            else
                            {
                                temp = temp + vals[i]+vals[i+1];
                                if(i+2!=vals.Length){
                                    //there is a dot somewhere unexpected
                                    e = true;
                                }
                                break;
                            }
                            
                        }
                    }
                    else if (value.Contains("//") && !value.Contains("http://"))
                    {
                        temp = "http:" + value;
                    }
                    else
                    {
                        if (urlString.Contains("cnn.com"))
                        {
                            
                                temp = "http://www.cnn.com" + value;

                        }
                        else if (urlString.Contains("bleacherreport.com"))
                        {
                                temp = "http://bleacherreport.com" + value;
                            

                        }
                    }
                    
                }

                
                    
                temp = value;
               
                
                    if (temp.Contains("cnn.com") || temp.Contains("bleacherreport.com"))
                    {
                        //maybe I should check to see if .html, but a lot of them are not
                        
                        foreach(string d in disallow){
                            if(temp.Contains(d)){
                                //this url is in a disallowed directory
                                if (!errors.Contains(temp))
                                {
                                    errors.Add(temp);
                                    
                                }
                                e = true;
                            }
                             
                            
                        }
                        if (temp.Contains("mailto") )
                        {
                            if (!errors.Contains(temp))
                            {
                                errors.Add(temp);
                            }
                            e = true;
                        }
                        if (!e)
                        {
                            checkedURLS.Add(temp);
                        }
                    }
                    else
                    {
                        //wroong website
                    }



                    cCount++;//each one of these is crawled, but may be rejected, will not match index size
                
            }
           
            return checkedURLS;
            
        }

        public void batchData()
        {
            //dump all stats and urls to table
            

            List<urlTbl> theRealLastTen = new List<urlTbl>();
            Stack<urlTbl> lsTen = new Stack<urlTbl>();
            while(urlQueue.Count>0){
                urlTbl temp = urlQueue.Dequeue();

                lsTen.Push(temp);
                
                TableOperation insertURL = TableOperation.InsertOrReplace(temp);
                table.ExecuteAsync(insertURL);
            }
            HashSet<string> tmp = new HashSet<string>();
            for (int i = 0; i < lsTen.Count;i++ )
            {
                //often this doesnt actually get to ten
                urlTbl tmp2 = lsTen.Pop();
                if(lsTen.Count>0&&!tmp.Contains(tmp2.url)){
                    tmp.Add(tmp2.url);
                    theRealLastTen.Add(tmp2);
                } 
               
                
            }
            

            

            
            
            //TableOperation insertOperation = TableOperation.Insert(customer1);
            
            errorTbl tempE = new errorTbl("error",errors);
            TableOperation insertError = TableOperation.InsertOrReplace(tempE);
            table.ExecuteAsync(insertError);
            
           
            
            statTbl temp2 = new statTbl("count", counter+"");
            TableOperation insertCount = TableOperation.InsertOrReplace(temp2);
            table.ExecuteAsync(insertCount);
            statTbl temp3 = new statTbl("cCount", cCount+"");
            TableOperation insertcC = TableOperation.InsertOrReplace(temp3);
            table.ExecuteAsync(insertcC);
            statTbl temp4 = new statTbl("status", status);
            TableOperation insertStatus = TableOperation.InsertOrReplace(temp4);
            table.ExecuteAsync(insertStatus);

            tenTbl temp5 = new tenTbl("lastten", theRealLastTen);
            TableOperation insertTen = TableOperation.InsertOrReplace(temp5);
            table.ExecuteAsync(insertTen);

            
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
            //go through the XML to find URLs, with some restrictions //very fast

            while(xml.Count>0){
                string z = xml.Dequeue();
                XName url;
                XName loc;
                if(z.Contains("cnn")){
                    url = XName.Get("url", "http://www.sitemaps.org/schemas/sitemap/0.9");
                    loc = XName.Get("loc", "http://www.sitemaps.org/schemas/sitemap/0.9");
                }
                else
                {
                    url = XName.Get("url", "http://www.google.com/schemas/sitemap/0.9");
                    loc = XName.Get("loc", "http://www.google.com/schemas/sitemap/0.9");
                }
                
                XDocument doc = XDocument.Load(z);
                List<string> urlList = doc.Root
                                .Elements(url)
                               .Elements(loc)
                               .Select(x => (string)x)
                               .ToList();
                foreach(string s in urlList){
                    CloudQueueMessage u1 = new CloudQueueMessage(s);
                    queue.AddMessageAsync(u1);
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
            disallow = new HashSet<string>();
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
