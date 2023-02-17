

function changePage(url, form) {
    
    var workerId = document.forms[form]["workerId"].value;

    var screen = form.substring(0, form.lastIndexOf("Form"))
    var index = url.lastIndexOf("/");
    var freeUrl = url.substring(0, index + 1);
   
    var insertUrl = freeUrl + 'InsertTimes'
    
    var failUrl = freeUrl + 'ShowError?lastScreen=' + form;
    
    url += "?workerId=" + workerId;
    
    $.ajax({
        type: "POST",
        
        url: insertUrl,
        data: {
            workerId: workerId,
            screen: screen,
            
        },
        async: false,
        success: function () {            
            window.location.replace(url);
        },
        error: function (jqXHR, exception) {           
            window.location.replace(failUrl);
        }
    });
}
