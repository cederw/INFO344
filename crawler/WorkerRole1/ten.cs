using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class tenTbl : TableEntity
    {
        /*
         * This is a simple class that stores the last 10 urls crawled
         * It takes a list and combines them all into a string so it can end up in tablestorage
         */
        public tenTbl(string name, List<urlTbl> data3)
        {
            //gotte get good keys here
            this.PartitionKey = name;
            this.RowKey = "list";
            string data4 = "";
            foreach(urlTbl url in data3){
                data4 = data4+","+url.url;
            }
            this.ten = data4;

        }
        public tenTbl() { }
        public string[] data { get; set; }
        public string ten { get; set; }

    }
}