var numOfTries = 0;

function validateForm(url) {
    

    numOfTries++;

    var validateUrl = url.replace("Instructions","IsAnswersCorrect");
    var insertUrl = url.replace("Instructions", "InsertDemographicInfo");
    var failUrl = url.replace("Instructions", "DemFailed");
    var passErrorUrl = url.replace("Instructions", "ShowError?lastScreen=botsPassed");
    var failErrorUrl = url.replace("Instructions", "ShowError?lastScreen=botsPassed");
    

    var workerId = document.forms["demForm"]["workerId"].value;
    url += "?workerId=" + workerId;
    var answers = [];
    // fill in answers.
    var age = document.getElementById('age_id').value;
    answers.push(age);

    var gender = document.getElementById('gender_id').value;
    answers.push(gender);

    var country = document.getElementById('country_id').value;
    answers.push(country);

    var education = document.getElementById('education_id').value;
    answers.push(education);

    var cap = document.getElementById('cap_id').value;
    answers.push(cap);

    var square_root = document.getElementById('sqrt_id').value;
    answers.push(square_root);

    for (var i = 0; i < answers.length; i++) {
        if ((answers[i] == "") || (answers[i] == null)) {
            alert("Please answer all the questions.");
            return;
        }
    }

    if ((age < 18) || (age > 120)) {
        alert("Invalid age.");
        return;
    }

    if (numOfTries < 7) {
        $.ajax({
            type: "POST",
            url: validateUrl,
            data: {
                cap: cap,
                square_root: square_root
            },
            async: false,
            success: function (result) {
                // correct
                if (result == "True") {
                    $.ajax({
                        type: "POST",
                        url: insertUrl,
                        data: {
                            age: age,
                            gender: gender,
                            country: country,
                            education: education,
                            workerId: workerId,                         
                            attempts: numOfTries,
                            success: true,
                        },
                        async: false,
                        success: function () {
                            window.location.replace(url); //to prevent page back
                        },
                        error: function (jqXHR, exception) {                      
                            window.location.replace(passErrorUrl);
                        }
                    });
                    return;
                }
                // incorrect
                else {
                    alert("Please try again...");
                    return;
                }
            }
        });
    }
    else {
        // write data.
        $.ajax({
            type: "POST",
            url: insertUrl,
            data: {
                age: age,
                gender: gender,
                country: country,
                education: education,
                workerId: workerId,              
                attempts: numOfTries,
                success: false,
            },
            async: false,
            success: function () {
                window.location.replace(failUrl); //to prevent page back
            },
            error: function (jqXHR, exception) {           
                window.location.replace(failErrorUrl);
            }
        });
        return;
    }
}


