﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        private PerformanceCounter memProocess = new PerformanceCounter("Memory", "Available MBytes");
        [WebMethod]
        //returns how mant MBytes are avalible.
        public float GetAvailableMBytes()
        {
            float memUsage = memProocess.NextValue();
            return memUsage;
        }
    }
}
