using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    /*
         * This is a simple class that stores a url and the page title for it
     * the important thing is that it modifies the uurl to make a unique rowkey while passing the requirement for rowkeys to nt have "/"
         */
    public class urlTbl : TableEntity
    {
        public urlTbl(string urlString, string pageTitle)
        {
            //gotta get good keys here
            if(urlString.Contains("cnn.com")){
                this.PartitionKey = "cnn";
            } else{
                this.PartitionKey = "bleacherReport";
            }
            /*
             * • The forward slash (/) character
 • The backslash (\) character
 • The number sign (#) character
 • The question mark (?) character
             */
            string row = urlString.Replace('/','.');
            row = row.Replace('\\', '.');
            row = row.Replace('#', '.');
            row = row.Replace('?', '.');
            this.RowKey = row;
            this.url = urlString;
            this.title = pageTitle;

        }
        public urlTbl() { }
        public string url { get; set; }
        public string title { get; set; }
    }
}