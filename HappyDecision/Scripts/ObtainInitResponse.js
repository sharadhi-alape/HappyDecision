function SendInitResponse(url) {
    var postUrl = url.replace("VotingTable","InitialReaction")
    var forminputs = document.forms["init"];
    var errorUrl = url.replace("VotingTable", "ShowError?lastScreen=initResponse");

    
    var name = forminputs.elements[0].name;   
    var questionId = name.substring(name.indexOf('_') + 1);
    var answer = $('input[name=answer_' + questionId + ']:checked').val();
    var workerId = document.forms["init"]["workerId"].value;
    url += "?workerId=" + workerId;

    if (answer == "" || answer == null) {

        alert('Please answer the question');
        return;
    }

    $.ajax({
        method: "POST",
        url: postUrl,

        data: {
            ans: answer,
            qId: questionId,
            workerId: workerId,
        },

        
        success: function () {
            
            window.location.replace(url);
        },
        error: function (jqXHR, exception) {
            window.location.replace(errorUrl);
        }
    });
}