"use strict";

window.onload = function () {
    //load all initial data and edit it into the DOM
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/WebService.asmx/GetAvailableMBytes",
        dataType: "json",
        success: function (msg) {
            console.log(msg);
            console.log(msg.d);
            document.getElementById("mbytes").innerHTML = msg.d.replace(/["]+/g, '');
            

        },
        error: function (msg) {
            //i hope this never happens
            console.log(msg);
        }
    });
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/WebService.asmx/GetCPU",
        dataType: "json",
        success: function (msg) {
            document.getElementById("cpu").innerHTML = msg.d.replace(/["]+/g, '');


        },
        error: function (msg) {
            //i hope this never happens
            console.log(msg);
        }
    });
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/WebService.asmx/GetQCount",
        dataType: "json",
        success: function (msg) {
            document.getElementById("que").innerHTML = msg.d.replace(/["]+/g, '');


        },
        error: function (msg) {
            //i hope this never happens
            console.log(msg);
        }
    });
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/WebService.asmx/GetURLCount",
        dataType: "json",
        success: function (msg) {
            document.getElementById("crawled").innerHTML = msg.d.replace(/["]+/g, '');


        },
        error: function (msg) {
            //i hope this never happens
            console.log(msg);
        }
    });
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/WebService.asmx/GetLastTen",
        dataType: "json",
        success: function (msg) {
            console.log(msg);
            console.log(msg.d);
            $(".ten").remove();
            var results = $(document.createElement('ol'));
            var noQ = msg.d.replace(/["]+/g, '');
            var sugs = noQ.split(",");
            for (var i = 1; i < sugs.length; i++) {
                var result = sugs[i];
                
                

                var resultLine = $(document.createElement('li'));
                var t = document.createTextNode(result);
                resultLine.append(t);
                results.append(resultLine);
            }

            results.addClass("ten")
            $('#lten').append(results);


        },
        error: function (msg) {
            //i hope this never happens
            console.log(msg);
        }
    });
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/WebService.asmx/GetErrors",
        dataType: "json",
        success: function (msg) {
            $(".err").remove();
            var results = $(document.createElement('ul'));
            var noQ = msg.d.replace(/["]+/g, '');
            var sugs = noQ.split(",");
            for (var i = 1; i < sugs.length; i++) {
                var result = sugs[i];



                var resultLine = $(document.createElement('li'));
                var t = document.createTextNode(result);
                resultLine.append(t);
                results.append(resultLine);
            }

            results.addClass("err");
            $('#errors').append(results);
        },
        error: function (msg) {
            //i hope this never happens
            console.log(msg);
        }
    });
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/WebService.asmx/GetIndexSize",
        dataType: "json",
        success: function (msg) {
            document.getElementById("indexS").innerHTML = msg.d.replace(/["]+/g, '');


        },
        error: function (msg) {
            //i hope this never happens
            console.log(msg);
        }
    });
    
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/WebService.asmx/GetStatus",
        dataType: "json",
        success: function (msg) {
            document.getElementById("state").innerHTML = msg.d.replace(/["]+/g, '');


        },
        error: function (msg) {
            //i hope this never happens
            console.log(msg);
        }
    });




};
//start the crawler
function start() {
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/WebService.asmx/Start",
        dataType: "json",
        success: function (msg) {
            console.log("started");

        },
        error: function (msg) {
            //i hope this never happens
            console.log(msg);
        }
    });
}
//stop the crawler
function stop() {
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/WebService.asmx/Stop",
        dataType: "json",
        success: function (msg) {
            console.log("stopped");

        },
        error: function (msg) {
            //i hope this never happens
            console.log(msg);
        }
    });
}
//retreive the title to a URL
function ret() {
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/WebService.asmx/Retrieve",
        data: "{'urlString':'" + $('#urlInput').val() + "'}",
        dataType: "json",
        success: function (msg) {
            document.getElementById("urlre").innerHTML = msg.d.replace(/["]+/g, '');


        },
        error: function (msg) {
            //i hope this never happens
            console.log(msg);
        }
    });
}