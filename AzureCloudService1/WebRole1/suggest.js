"use strict";

window.onload = function(){
    
    $('#searchInput').on('input', function () {
        $.ajax({            
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/WebService.asmx/suggest",
            data: "{'search':'" + $('#searchInput').val() + "'}",
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
                $('#simpleSearch').append(results);
                
            },
            error: function (msg) {
                //i hope this never happens
                alert("Contact the webmaster! The trie has failed to build");
            }
        });
    });
};
