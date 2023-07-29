document.getElementById("subBtn").onclick =
    function () {
        var stext = document.getElementById("stext").value;
        var sldropdownitemValues = document.getElementById("sl").value.split("-");

        var sl = sldropdownitemValues[0];
        var slIsRigthToLeft = sldropdownitemValues[1] == "False" ? false : true;
        
        var tldropdownitemValues = document.getElementById("tl").value.split("-");

        var tl = tldropdownitemValues[0];

        var tlIsRigthToLeft = tldropdownitemValues[1] == "False" ? false : true;


        document.getElementById("loading").innerText = "Loading ... ";
        var xhttp = new XMLHttpRequest();
        xhttp.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200) {
                // Typical action to be performed when the document is ready:

                var jsonResponse = JSON.parse(xhttp.responseText);
                if (jsonResponse["code"] == "200") {
  tlIsRigthToLeft ? document.getElementById("ltext").dir = "rtl" : document.getElementById("ltext").dir = "ltr";
                   
                    document.getElementById("ltext").innerText = jsonResponse["result"];
                } else {
                   document.getElementById("ltext").innerHTML = "<span class='text-danger'>" + jsonResponse["desc"] +"</span>";
                }

                document.getElementById("loading").innerText = "";
            }
        };

        xhttp.onerror = function ()
        {
            document.getElementById("ltext").innerHTML = "<span class='text-danger'>Something Went Wrong</span>";

            document.getElementById("loading").innerText = "";

        }


        xhttp.open("GET", "/api/search/translate?tl=" + tl + "&sl=" + sl + "&text=" + stext, true);

        xhttp.send();


    };