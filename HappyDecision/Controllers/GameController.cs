using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Web.Mvc;
using HappyDecision.DataContext;
using HappyDecision.Utils;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using HappyDecision.Models.Dto;
using HappyDecision.Models;
using Fluentx.Mvc;


namespace HappyDecision.Controllers
{
    public class GameController : Controller
    {
        //
        // GET: /Game/
        private static List<Question> _exampleQuestions = new List<Question>() {
            new Question() {
                QuestionId = 7, QuestionType = QuestionType.Closed,
                Text = "How many voters prefer Cereal A over Cereal B and Cereal B over Cereal C?",
                Value = new string[] {"6", "7", "12", "24"}
            },
            new Question() {
                QuestionId = 17, QuestionType = QuestionType.Closed,
                Text = "How many voters prefer Cereal A over Cereal B over all of the 6 columns?",
                Value = new string[] {"6", "7", "12", "17"}
            }
        };

        public ActionResult Intro([System.Web.Http.FromBody]bool isPreview = true)
        {
            return View(isPreview);
        }

        public ActionResult Sorry()
        {
            return View();
        }
        public ActionResult Mobile()
        {
            return View();
        }
        
        
        public ActionResult Consent(string workerId)
        {
            
            Session["workerId"] = workerId;
            return View();

        }


        [HttpPost]
        public ActionResult AfterConsent(string workerId)

        {
           

            try
            {
                
                using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
                {
                    var consent = new Consent()
                    {
                        SurveyResultWorkerId = workerId,
                        DidAgree = true,
                    };
                    var readingTime = new ReadingTime
                    {
                        RTWorkerId = workerId,
                        Screen = "Informed Consent",
                        EndTime = Utils.ExtensionMethods.getTimeStampInString(),
                    };
                    db.ReadingTimes.Add(readingTime);
                    db.Consents.Add(consent);
                    db.SaveChanges();
                }
                
                return new HttpStatusCodeResult(200, "OK");
            }
            catch (Exception e)
            {
                string err = e.ToString();
                Utils.ExtensionMethods.WriteErrorToDB(workerId, err, "Game-AfterConsent");
                return new HttpStatusCodeResult(500, "Something went wrong. Please contact the HIT requester.");
            }           
        }

        [HttpPost]
        public ActionResult AfterDisagreement(string workerId)
        {           
            try
            {
                using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
                {
                    var consent = new Consent()
                    {                      
                        SurveyResultWorkerId =  workerId,
                        DidAgree = false,
                    };
                    var readingTime = new ReadingTime
                    {
                        RTWorkerId = workerId,
                        Screen = "Informed Consent",
                        EndTime = Utils.ExtensionMethods.getTimeStampInString(),
                    };
                    db.ReadingTimes.Add(readingTime);
                    db.Consents.Add(consent);
                    db.SaveChanges();
                }
                return new HttpStatusCodeResult(200, "OK");
            }
            catch (Exception e)
            {
                string err = e.ToString();
                Utils.ExtensionMethods.WriteErrorToDB(workerId, err, "Game-AfterDisagreement");
                return new HttpStatusCodeResult(500, "Something went wrong. Please contact the HIT requester.");
            }
                  
        }

        public ActionResult Disagree()
        {
            return View();
        }




        public ActionResult DemographicInfo(string workerId)
        {
            Session["workerId"] = workerId;
            return View();
        }

        [HttpPost]
        public Boolean IsAnswersCorrect(String cap, String square_root)
        {
            
            if (cap.Equals("rLdcp6b")  && Int32.Parse(square_root) == 4)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpPost]
        public ActionResult InsertDemographicInfo(String age, String gender, String country, String education, String workerId, int attempts, Boolean success)
        {
           
            try
            {
                using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
                {

                   
                    var demography = new Demography()
                    {
                        Age = age,
                        Country = country,                        
                        SurveyResultWorkerId = workerId,
                        Education = education,
                        Gender = gender,
                        Attempts = attempts,
                        IsSuccessful = success,
                    };

                    var readingTime = new ReadingTime
                    {
                        RTWorkerId = workerId,
                        Screen = "Demography Details",
                        EndTime = Utils.ExtensionMethods.getTimeStampInString(),
                    };

                    db.ReadingTimes.Add(readingTime);
                    db.Demographies.Add(demography);
                    db.SaveChanges();
                }
                return new HttpStatusCodeResult(200, "OK");
            }
            catch(Exception e)
            {
                string err = e.ToString();
                Utils.ExtensionMethods.WriteErrorToDB(workerId, err, "Game-InsertDemographicInfo");
                return new HttpStatusCodeResult(500, "Something went wrong. Please contact the HIT requester.");
            }
        }

        public ActionResult DemFailed()
        {
            return View();
        }

        public ActionResult Instructions(string workerId)
        {
            Session["workerId"] = workerId;
            return View();
        }
        public ActionResult Example(string workerId)
        {
            Session["workerId"] = workerId;
            return View(_exampleQuestions);
        }

        public ActionResult Transition(string workerId)
        {
            Session["workerId"] = workerId;
            var survey = Utils.ExtensionMethods.GetSurveyFromWorkerId(workerId);
            return View(new MatrixDto(survey.VotingDistribution, survey.WinningCandidate, survey.UserPreference));
            //return View(new MatrixDto(Session.GetVotingDistribution(), Session.GetSurvey().WinningCandidate, Session.GetSurvey().UserPreference));
        }

        public ActionResult InitResponse(string workerId)
        {
            
            Session["workerId"] = workerId;         
            var survey = Utils.ExtensionMethods.GetSurveyFromWorkerId(workerId);           
            var questions = Utils.ExtensionMethods.GetQuestionsFromDb().Where(ques => ques.ExtraNotes == "Initial Emotion");
            ViewBag.Matrix = new MatrixDto(survey.VotingDistribution, survey.WinningCandidate, survey.UserPreference);

            foreach (var question in questions)
            {
                if (question.QuestionType == QuestionType.Scale)
                {
                    question.Text = question.Text.Replace("<insert preference>", Utils.ExtensionMethods.GetUserFirstPreference(survey.VotingDistribution, survey.UserPreference));

                }
            }           
            return View(questions.ToList());
        }


        [HttpPost]
        public ActionResult InitialReaction(String ans, String qId, string workerId)
        {
            
            Session["workerId"] = workerId;
            try
            {
                using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
                {
                    var survey = db.SurveyResults.Find(workerId);                   
                    var questionsById = db.Questions                       
                        .ToDictionary(q => q.QuestionId, q => q);

                    var answers = new List<Answer>();                   
                    var answer = new Answer()
                    {
                        Question = questionsById[Int32.Parse(qId)],
                        SurveyResult = survey,
                        Value = ans,
                    };

                    var readingTime = new ReadingTime
                    {
                        RTWorkerId = workerId,
                        Screen = "Initial Response",
                        EndTime = Utils.ExtensionMethods.getTimeStampInString(),
                    };


                    db.ReadingTimes.Add(readingTime);
                    answers.Add(answer);
                    db.Answers.AddRange(answers);

                    db.SaveChanges();
                }

                return new HttpStatusCodeResult(200, "OK");
            }
            catch (Exception e)
            {
                string err = e.ToString();
                Utils.ExtensionMethods.WriteErrorToDB(Session["workerId"].ToString(), err, "Game-InitialReaction");
                return new HttpStatusCodeResult(500, "Something went wrong. Please contact the HIT requester.");
            }
        }


        public ActionResult VotingTable(string workerId)
        {
            Session["workerId"] = workerId;
            var survey = Utils.ExtensionMethods.GetSurveyFromWorkerId(workerId);
            return View(new MatrixDto(survey.VotingDistribution, survey.WinningCandidate, survey.UserPreference));
        }
        public ActionResult ResultsTable(string workerId)
        {
            Session["workerId"] = workerId;
            var survey = Utils.ExtensionMethods.GetSurveyFromWorkerId(workerId);          
            return View(new MatrixDto(survey.VotingDistribution, survey.WinningCandidate, survey.UserPreference));
        }

        public ActionResult Quiz(string workerId)
        {
            Session["workerId"] = workerId;
            var survey = Utils.ExtensionMethods.GetSurveyFromWorkerId(workerId);            
            var questions = Utils.ExtensionMethods.GetQuestionsFromDb().Where(ques => ques.ExtraNotes != "Initial Emotion");
            foreach (var question in questions)
            {
                if (question.QuestionType == QuestionType.Scale)
                {
                    question.Text = question.Text.Replace("<insert winner>", survey.WinningCandidate.Name);

                }

                if (question.ExtraNotes == "OtherOptions")
                {
                    question.Value = Utils.ExtensionMethods.GetCandidatesOtherThanWinner(survey.WinningCandidate);
                }
            }
            ViewBag.Matrix = new MatrixDto(survey.VotingDistribution, survey.WinningCandidate, survey.UserPreference);
            return View(questions.ToList());
        }

        public ActionResult DummyQuiz(string workerId)
        {
            Session["workerId"] = workerId;
            var survey = Utils.ExtensionMethods.GetSurveyFromWorkerId(workerId);
            //var questions = Session.GetMatrix().Questions.Where(ques => ques.ExtraNotes != "Initial Emotion"); ;
            //var questions = Utils.ExtensionMethods.GetQuestionsFromDb().Where(ques => ques.ExtraNotes != "Initial Emotion");

            var questions = Utils.ExtensionMethods.GetQuestionsFromDb();
            foreach (var question in questions)
            {
                if (question.ExtraNotes == "Satisfaction" || question.ExtraNotes == "Agreement")
                {
                    question.Text = question.Text.Replace("<insert winner>", survey.WinningCandidate.Name);

                }

                if (question.ExtraNotes == "Initial Emotion")
                {
                    question.Text = question.Text.Replace("<insert preference>", Utils.ExtensionMethods.GetUserFirstPreference(survey.VotingDistribution, survey.UserPreference));
                }

                if (question.ExtraNotes == "OtherOptions")
                {
                    question.Value = Utils.ExtensionMethods.GetCandidatesOtherThanWinner(survey.WinningCandidate);
                }
            }
            ViewBag.Matrix = new MatrixDto(survey.VotingDistribution, survey.WinningCandidate, survey.UserPreference);
            return View(questions.ToList());
        }

        [HttpPost]
        public ActionResult QuizResult(String[] answers, String[] questions, String isDone, string workerId)
        {
           
            Session["workerId"] = workerId;
            try
            {
                using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
                {
                    var survey = db.SurveyResults.Find(workerId);
                    
                    var questionsById = db.Questions
                        
                        .ToDictionary(q => q.QuestionId, q => q);

                    var answs = new List<Answer>();                  


                    for (int i = 0; i < answers.Length; i++)
                    {
                        var answerEntry = new Answer
                        {
                            Question = questionsById[Int32.Parse(questions[i])],
                            SurveyResult = survey,
                            Value = answers[i],
                        };
                        answs.Add(answerEntry);
                    }
                    var readingTime = new ReadingTime
                    {
                        RTWorkerId = workerId,
                        Screen = "Final Survey",
                        EndTime = Utils.ExtensionMethods.getTimeStampInString(),
                    };
                    db.ReadingTimes.Add(readingTime);
                    db.Answers.AddRange(answs);
                    survey.DidComplete = Convert.ToBoolean(isDone);
                    survey.EndTime = Utils.ExtensionMethods.getTimeStamp();
                    db.SaveChanges();
                }

                return new HttpStatusCodeResult(200, "OK");
            }
            catch (Exception e)
            {
                string err = e.ToString();
                Utils.ExtensionMethods.WriteErrorToDB(Session["workerId"].ToString(), err, "Game-QuizResult");
                return new HttpStatusCodeResult(500, "Something went wrong. Please contact the HIT requester.");
            }
        }

        public ActionResult ThankYou(string workerId)
        {
            Session["workerId"] = workerId;
            return View();
        }

        [HttpPost]
        public ActionResult AfterSubmit([System.Web.Http.FromBody]string workerId)
        {
            Dictionary<string, object> postData = new Dictionary<string, object>();
            var survey = Utils.ExtensionMethods.GetSurveyFromWorkerId(workerId);
            postData.Add("assignmentId", survey.AssignmentId);
            postData.Add("workerId", workerId);
            postData.Add("hitId", survey.HitId);           
            string amtURL = survey.TurkSubmitTo + "/mturk/externalSubmit";
            if (survey.TurkSubmitTo == null)
            {
                return Redirect("https://sarnelab.cs.biu.ac.il/ExplainingVoting/mturk/externalSubmit");
            }
            else
            {
                return this.RedirectAndPost(amtURL, postData);
            }
        }

        public ActionResult ShowError()
        {
            
            string lastScreen = Request.QueryString["lastScreen"];
            string workerId = "";
            if (Session["workerId"] != null)
            {
                workerId = Session["workerId"].ToString();
            }
            Utils.ExtensionMethods.WriteErrorToDB(workerId, "error", lastScreen);
            return View();
        }

        [HttpPost]
        public ActionResult InsertTimes(string workerId, string screen)
        {
            Session["workerId"] = workerId;
            try
            {
                using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
                {
                    var readingTime = new ReadingTime
                    {
                        RTWorkerId = workerId,
                        Screen = screen,
                        EndTime = Utils.ExtensionMethods.getTimeStampInString(),
                    };
                    db.ReadingTimes.Add(readingTime);
                    db.SaveChanges();
                }
                return new HttpStatusCodeResult(200, "OK");
            }
            catch (Exception e)
            {
                string err = e.ToString();
                string screenName = "Game-InsertTimes-" + screen; 
                Utils.ExtensionMethods.WriteErrorToDB(Session["workerId"].ToString(), err, screenName);
                return new HttpStatusCodeResult(500, "Something went wrong. Please contact the HIT requester.");
            }
        }
    }
}
