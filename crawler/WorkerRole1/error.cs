using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace WebRole1
{
    public class errorTbl : TableEntity
    {
        /*
         * This is a simple class that stores the urls that were rejected for some reason during the crawl.
         * It takes a list and combines them all into a string so it can end up in tablestorage
         */
        public errorTbl(string name, List<string> data3)
        {
            //gotte get good keys here
            this.PartitionKey = name;
            this.RowKey = "errors";
            string data4 = "";
            foreach (string url in data3)
            {
                //if the errors take over 64000 bytes the table will have trouble so ill stop it early just in case
                if(data4.Length * sizeof(Char)<6000){
                    data4 = data4 + "," + url;
                }
                
            }
           
            this.ten = data4;

        }
        public errorTbl() { }
        public string[] data { get; set; }
        public string ten { get; set; }

    }
}