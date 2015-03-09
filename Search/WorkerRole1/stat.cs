using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class statTbl : TableEntity
    {
        /*
         * This is a simple class that stores a statistic
         */
        public statTbl(string name, string data2)
        {
            //gotte get good keys here
            this.PartitionKey = name;
            this.RowKey = "row";
            this.data = data2;

        }

        public statTbl() { }
        public string data { get; set; }

        
    }
}