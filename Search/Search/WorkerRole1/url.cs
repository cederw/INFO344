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
        public urlTbl(string urlString, string pageTitle, string key, string date)
        {
            this.PartitionKey = key;
            //gotta get good keys here
           
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
            this.date = date;

        }
        public urlTbl() { }
        public string url { get; set; }
        public string date { get; set; }
        public string title { get; set; }
    }
}