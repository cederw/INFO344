"use strict";
//gets data from the webservice for the search page
window.onload = function(){
    //grabs the suggestions ans displays them on the side
    $('#sInput').on('input', function () {
        $.ajax({            
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/WebService1.asmx/suggest",
            data: "{'search':'" + $('#sInput').val() + "'}",
            dataType: "json",
            success: function (msg) {
                //console.log(msg);
                //console.log(msg.d);
                //builds a ul with the results of the ajax call to the webservice
                $(".searchSug").remove();
                var results = $(document.createElement('ul'));
                var sugs = msg.d.split(",");
                for (var i = 0; i < sugs.length; i++) {
                    var result = sugs[i];
                    result = result.replace("\"", "");
                    result = result.replace("[", "");
                    result = result.replace("]", "");
                    result = result.replace("_", " ");
                    result = result.substring(0,result.length-1);
                    console.log(result);

                    var resultLine = $(document.createElement('li'));
                    var t = document.createTextNode(result);
                    resultLine.append(t);
                    results.append(resultLine);
                }
                
                results.addClass("searchSug")
                $('#sug').append(results);
                
            },
            error: function (msg) {
                //i hope this never happens
                alert("Contact the webmaster! The trie has failed to build");
                console.log(msg);
            }
        });
        search(); //JUST LIKE GOOGLE, SEARCH EVERYTIME A LETTER IS ENTERED
    });

    //Onsearch when the button is clicked
    $("#submit").click(function () {
        search(); 
    });

    //search results, its massive because im not good at parsing json
    function search() {
        $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/WebService1.asmx/search",
            data: "{'search':'" + $('#sInput').val() + "'}",
            dataType: "json",
            success: function (msg) {
                //console.log(msg);
                //console.log(msg.d);
                //builds a ul with the results of the ajax call to the webservice
                $(".searchRes").remove();
                var results = $(document.createElement('ul'));
                var sugs = msg.d.split(",");
                var rank = 0;
                for (var i = 0; i < sugs.length; i++) {
                    var result = sugs[i];
                    result = result.replace("\"", "");
                    result = result.replace("[", "");
                    result = result.replace("]", "");
                    result = result.replace("_", " ");
                    result = result.substring(0, result.length - 1);
                    console.log(result);
                    if (result == "NBALIN") {
                        //display the NBA stats on the right side
                        var NBAresults = $(document.createElement('ul'));
                        for (var j = 0; j < 6;j++){
                            i++;
                            var result = sugs[i];
                            result = result.replace("\"", "");
                            result = result.replace("[", "");
                            result = result.replace("]", "");
                            result = result.replace("_", " ");
                            result = result.substring(0, result.length - 1);
                            if (result.indexOf("name") > -1) {
                                var resultLine = $(document.createElement('li'));
                                var t = document.createTextNode("Name: "+result.substring(8,result.length-1));
                                resultLine.append(t);
                                NBAresults.append(resultLine);
                            }
                            if (result.indexOf("GP") > -1) {
                                var resultLine = $(document.createElement('li'));
                                var t = document.createTextNode("GP: " + result.substring(9, result.length - 1));
                                resultLine.append(t);
                                NBAresults.append(resultLine);
                            }
                            if (result.indexOf("FGP") > -1) {
                                var resultLine = $(document.createElement('li'));
                                var t = document.createTextNode("FGP: " + result.substring(9, result.length - 1));
                                resultLine.append(t);
                                NBAresults.append(resultLine);
                            }
                            if (result.indexOf("TPP") > -1) {
                                var resultLine = $(document.createElement('li'));
                                var t = document.createTextNode("TPP: " + result.substring(9, result.length - 1));
                                resultLine.append(t);
                                NBAresults.append(resultLine);
                            }
                            if (result.indexOf("FTP") > -1) {
                                var resultLine = $(document.createElement('li'));
                                var t = document.createTextNode("FTP: " + result.substring(9, result.length - 1));
                                resultLine.append(t);
                                NBAresults.append(resultLine);
                            }
                            if (result.indexOf("PPG") > -1) {
                                var resultLine = $(document.createElement('li'));
                                var t = document.createTextNode("PPG: " + result.substring(9, result.length - 3));
                                resultLine.append(t);
                                NBAresults.append(resultLine);
                            }
                        }
                        NBAresults.addClass("searchRes")
                        $('#resuNBA').append(NBAresults);
                    } else if (result == "NBAerror") {
                        //do nothing
                    } else {
                        //dislay the results from the table
                        var a = document.createElement('a');
                        var resultLine = $(document.createElement('li'));
                        var t = document.createTextNode("#"+rank+" URL: " + result);
                        a.appendChild(t);
                        a.title = "#" + rank + " URL: " + result;
                        a.href = result;
                        resultLine.append(a);
                        results.append(resultLine);
                        rank++;
                    }
                    
                    
                }

                results.addClass("searchRes")
                $('#resu').append(results);

            },
            error: function (msg) {
                //i hope this never happens
                alert("Contact the webmaster! The trie has failed to build");
                console.log(msg);
            }
        });
    }


};
