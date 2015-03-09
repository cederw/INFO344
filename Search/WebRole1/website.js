/*
Copyright (c) 2011 Sam Phippen <samphippen@googlemail.com>
 
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
 
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
 
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
//I've heavily modified this code to make it work in the search engine context
//The original only displays static text and cannot hanndle input
//I have allows for it to display new information and to handle user input while trying ot look like a console
//I also took out a lot of features i didnt need. 
//still its mostly not my code, but its MIT licensed, so its allowed


var Typer = {
    text: null,
    accessCountimer: null,
    index: 0, // current cursor position
    speed: 2, // speed of the Typer
    file: "", //file, must be setted
    accessCount: 0, //times alt is pressed for Access Granted
    deniedCount: 0, //times caps is pressed for Access Denied
    init: function () {// inizialize Hacker Typer
         // inizialize timer for blinking cursor
        $.get(Typer.file, function (data) {// get the text file
            Typer.text = data;// save the textfile in Typer.text
            Typer.text = Typer.text.slice(0, Typer.text.length - 1);
        });
    },

    content: function () {
        return $("#console").html();// get console content
    },

    write: function (str) {// append to console content
        $("#console").append(str);
        return false;
    },





    addText: function (key) {//Main function to add the code
         if (Typer.text) { // otherway if text is loaded
            var cont = Typer.content(); // get the console content
            if (cont.substring(cont.length - 1, cont.length) == "|") // if the last char is the blinking cursor
                $("#console").html($("#console").html().substring(0, cont.length - 1)); // remove it before adding the text
            if (key.keyCode != 8) { // if key is not backspace
                Typer.index += Typer.speed;	// add to the index the speed
            } else {
                if (Typer.index > 0) // else if index is not less than 0 
                    Typer.index -= Typer.speed;//	remove speed for deleting text
            }
            var text = Typer.text.substring(0, Typer.index)// parse the text for stripping html enities
            var rtn = new RegExp("\n", "g"); // newline regex

            $("#console").html(text.replace(rtn, "<br/>"));// replace newline chars with br, tabs with 4 space and blanks with an html blank
            window.scrollBy(0, 50); // scroll to make sure bottom is always visible
        }
        
        
        
    },


   
}

function replaceUrls(text) {
    var http = text.indexOf("http://");
    var space = text.indexOf(".me ", http);
    if (space != -1) {
        var url = text.slice(http, space - 1);
        return text.replace(url, "<a href=\"" + url + "\">" + url + "</a>");
    } else {
        return text
    }
}
Typer.speed = 3;
Typer.file = "TextFile1.txt";
Typer.init();


var timer = setInterval("t();", 10);
function t() {
    Typer.addText({ "keyCode": 123748 });
    if (Typer.index > Typer.text.length) {
        clearInterval(timer);
        
        $('#inp').css("display", "block");
        document.getElementById("sInput").focus();
    }
}

