document.getElementById("subBtn").onclick =
    function () {

        document.getElementById("ltext").innerText = "";
        var stext = document.getElementById("stext").value;
        if (stext == "" || stext == null) return; 
        var sldropdownitemValues = document.getElementById("sl").value.split("-");

        var sl = sldropdownitemValues[0];
        var slIsRigthToLeft = sldropdownitemValues[1] == "False" ? false : true;
        
        var tldropdownitemValues = document.getElementById("tl").value.split("-");

        var tl = tldropdownitemValues[0];

        var tlIsRigthToLeft = tldropdownitemValues[1] == "False" ? false : true;

     
  
        document.getElementById("ltext").style.animation = "fade 4s infinite ease-in-out";

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


                document.getElementById("ltext").style.animation = "";
            }
        };

        xhttp.onerror = function ()
        {
            document.getElementById("ltext").innerHTML = "<span class='text-danger'>Something Went Wrong</span>";


            document.getElementById("ltext").style.animation = "";

        }


        xhttp.open("GET", "/api/search/translate?tl=" + tl + "&sl=" + sl + "&text=" + stext, true);

        xhttp.send();


    };