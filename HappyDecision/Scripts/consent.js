function Agree(url) {
    var replaceUrl = url.replace("AfterConsent", "DemographicInfo");
    replaceUrl += "?workerId=" + document.forms["consentForm"]["workerId"].value;
    var failUrl = url.replace("AfterConsent", "ShowError?lastScreen=consent")
    $.ajax({
        type: "POST",       
        url: url,
        data: { workerId: document.forms["consentForm"]["workerId"].value},
        async: false,
        success: function () {            
            window.location.replace(replaceUrl);
        },
        error: function (jqXHR, exception) {            
            window.location.replace(failUrl);
        }
    });

    
}

function Disagree(url) {
    var replaceUrl = url.replace("AfterDisagreement", "Disagree");
    var failUrl = url.replace("AfterConsent", "ShowError?lastScreen=consent")
    $.ajax({
        type: "POST",       
        url: url,
        data: { workerId: document.forms["consentForm"]["workerId"].value },
        async: false,
        success: function () {            
            window.location.replace(replaceUrl);
        },
        error: function (jqXHR, exception) {           
            window.location.replace(failUrl);
        }

    });
}

