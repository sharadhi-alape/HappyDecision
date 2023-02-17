function SendSurveyResponse(url) {

    var answers = [];
    var questions = [];
    var currans;
    var ans2;
    var forminputs = document.forms["surveyForm"];
    var complete = document.forms["surveyForm"]["isDone"].value;
    var thankUrl = url.replace("QuizResult", "ThankYou");
    var errorUrl = url.replace("QuizResult", "ShowError?lastScreen=quiz");
    var workerId = document.forms["surveyForm"]["workerId"].value;
    
    thankUrl += "?workerId=" + workerId;
    
    for (var j = 2; j < 8; j++) {

        if (!forminputs.elements[j].name.startsWith("answer_")) {
            continue;
        }
        
        switch (j) {

            case 2: var name = forminputs.elements[2].name;
                var questionId = name.substring(name.indexOf('_') + 1);

                currans = $('input[name=answer_' + questionId + ']:checked').val();
                break;

            case 3: var name = forminputs.elements[7].name;
                var questionId = name.substring(name.indexOf('_') + 1);

                currans = $('input[name=answer_' + questionId + ']:checked').val();
                ans2 = $('input[name=answer_' + questionId + ']:checked').val();
                //ans2 = $('input[name=answer_' + questionId + ']:checked').val();
                break;

            case 4: var name = forminputs.elements[12].name;
                var questionId = name.substring(name.indexOf('_') + 1);

                currans = $('input[name=answer_' + questionId + ']:checked').val();
                if (currans == null && (ans2 == 1 || ans2 == 2)) {

                    break;

                } else if (currans == null) {
                    currans = "None"
                    break;
                }
                
                break;

            case 5: var name = forminputs.elements[14].name;
                var questionId = name.substring(name.indexOf('_') + 1);

                currans = $('input[name=answer_' + questionId + ']:checked').val();
                

                break;

            case 6: var name = forminputs.elements[19].name;
                var questionId = name.substring(name.indexOf('_') + 1);

                currans = forminputs.elements[19].value;
                break;

            case 7: var name = forminputs.elements[20].name;
                var questionId = name.substring(name.indexOf('_') + 1);

                currans = forminputs.elements[20].value;
                break;
        }
        
        answers.push(currans);
        questions.push(questionId);

    }

    for (var i = 0; i < answers.length; i++) {
        if ((answers[i] == "") || (answers[i] == null)) {
            alert("Please answer all the questions.");
            return;
        }
    }

    $.ajax({
        method: "POST",       
        url: url,
        data: {
            answers: answers,
            questions: questions,
            isDone: complete,
            workerId: workerId,
        },

        beforeSend: function () {
            $('#loadingmessage').show();
            $('#cont_str_info_id').prop("disabled", true);
        },

        success: function (html) {
            $(".content").html(html);
            $('#loadingmessage').hide(); // hide the loading message
            window.location.replace(thankUrl);
        },
        error: function (jqXHR, exception) {      
            window.location.replace(errorUrl);
        }
    });
}

function handleClick(myRadio, id) {


    //alert(myRadio.value);
    if (myRadio.value == 1 || myRadio.value == 2) {

        //alert("question_" + id)
        document.getElementById("question_" + id).style.display = "";
        

    }
    else {

        //document.getElementById("question_3").hidden = true;
        document.getElementById("question_" + id).style.display = "none";
        for (var j = 0; j < 2; j++) {
            var radid = "answer_4_" + j;
            document.getElementById(radid).checked = false;
        }
        
        
    }
}