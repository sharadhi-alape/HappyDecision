using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HappyDecision.Models.Dto;
using HappyDecision.Models;
using HappyDecision.DataContext;
using HappyDecision.Utils;
using System.Data.Entity;
using System.Data.SqlClient;
using Fluentx.Mvc;
using System.Diagnostics;


namespace HappyDecision.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report

        public ActionResult Index()
        {

            //var corrWorkerID = null;
            return View();


        }

        [HttpPost]
        public ActionResult ValidateAndRedirect()
        {
            string tableName = null;
            //string password = null;
            //bool flag = false;
            Dictionary<string, object> postData = new Dictionary<string, object>();
            foreach (var key in Request.Form.AllKeys)
            {
                if (key == "TableName")
                {
                    tableName = $"View{Request.Form[key]}";
                }
                else if (key == "Password")
                {
                    if (Request.Form[key] == "ExplainingDecisionMaking")
                    {
                        postData.Add("isValidated", true);
                    }
                    else
                    {
                        postData.Add("isValidated", false);
                    }
                }
            }

            return this.RedirectAndPost(Url.Action(tableName, "Report"), postData);
        }

        public ActionResult ViewAnswers([System.Web.Http.FromBody]bool isValidated = false)
        {
            if (isValidated)
            {
                using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
                {

                    var answers = db.Answers.ToList();
                    var surveyResults = db.SurveyResults.ToList();
                    var questionsList = db.Questions.ToList();
                    foreach (var ans in answers)
                    {

                        foreach (var sur in surveyResults)
                        {
                            foreach (var a in sur.Answers)
                            {
                                if (a.AnswerId == ans.AnswerId)
                                {
                                    ans.SurveyResult.WorkerId = sur.WorkerId;
                                }
                            }
                        }

                        foreach (var question in questionsList)
                        {
                            foreach (var a in question.Answers)
                            {
                                if (a.AnswerId == ans.AnswerId)
                                {
                                    ans.Question.QuestionId = question.QuestionId;
                                }
                            }
                        }

                    }
                    return View(answers);
                }
            }
            else
            {
                return this.RedirectAndPost(Url.Action("Sorry", "Report"), null);
            }
        }

        public ActionResult ViewParameterValues([System.Web.Http.FromBody]bool isValidated = false)
        {
            if (isValidated)
            {
                using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
                {
                    var pvalues = db.ParameterValues.ToList();
                    var parameters = db.Parameters.ToList();
                    var candidates = db.Candidates.ToList();
                    var votingDistributions = db.VotingDistributions.ToList();
                    foreach (var pvalue in pvalues)
                    {
                        foreach (var parameter in parameters)
                        {
                            foreach (var pval in parameter.ParameterValues)
                            {
                                if (pval.ParameterValueId == pvalue.ParameterValueId)
                                {
                                    pvalue.Parameter.Name = parameter.Name;
                                }
                            }
                        }

                        foreach (var candidate in candidates)
                        {
                            foreach (var pval in candidate.ParameterValues)
                            {
                                if (pval.ParameterValueId == pvalue.ParameterValueId)
                                {
                                    pvalue.Candidate.Name = candidate.Name;
                                }
                            }
                        }

                        foreach (var vd in votingDistributions)
                        {
                            foreach (var pval in vd.ParameterValues)
                            {
                                if (pval.ParameterValueId == pvalue.ParameterValueId)
                                {
                                    pvalue.VotingDistribution.InnerValue = vd.InnerValue;
                                }
                            }
                        }
                    }

                    return View(pvalues);
                }
            }
            else
            {
                return this.RedirectAndPost(Url.Action("Sorry", "Report"), null);
            }

        }

        public ActionResult ViewConsents([System.Web.Http.FromBody]bool isValidated = false)
        {
            if (isValidated)
            {
                using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
                {
                    var consents = db.Consents.ToList();
                    return View(consents);
                }
            }
            else
            {
                return this.RedirectAndPost(Url.Action("Sorry", "Report"), null);
            }
        }

        public ActionResult ViewDemographies([System.Web.Http.FromBody]bool isValidated = false)
        {
            if (isValidated)
            {
                using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
                {
                    var demographies = db.Demographies.ToList();
                    return View(demographies);
                }
            }
            else
            {
                return this.RedirectAndPost(Url.Action("Sorry", "Report"), null);
            }
        }

        public ActionResult ViewQuestions([System.Web.Http.FromBody]bool isValidated = false)
        {
            if (isValidated)
            {
                using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
                {
                    var questions = db.Questions.ToList();
                    return View(questions);
                }
            }
            else
            {
                return this.RedirectAndPost(Url.Action("Sorry", "Report"), null);
            }
        }

        public ActionResult ViewVotingDistributions([System.Web.Http.FromBody]bool isValidated = false)
        {
            if (isValidated)
            {
                using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
                {
                    var votingDistributions = db.VotingDistributions.ToList();
                    foreach (var votingDistribution in votingDistributions)
                    {
                        int[] Votes = votingDistribution.Value;
                        votingDistribution.Value = Votes;
                    }
                    return View(votingDistributions);
                }
            }
            else
            {
                return this.RedirectAndPost(Url.Action("Sorry", "Report"), null);
            }
        }

        public ActionResult ViewSurveyResults([System.Web.Http.FromBody]bool isValidated = false)
        {
            if (isValidated)
            {
                using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
                {
                    var surveyResults = db.SurveyResults.ToList();
                    var votingDistributions = db.VotingDistributions.ToList();
                    var candidates = db.Candidates.ToList();
                    foreach (var sr in surveyResults)
                    {
                        foreach (var vd in votingDistributions)
                        {
                            foreach (var sur in vd.SurveyResults)
                            {
                                if (sur.WorkerId == sr.WorkerId)
                                {
                                    sr.VotingDistribution.VotingDistributionId = vd.VotingDistributionId;
                                }
                            }
                        }
                    }

                    foreach (var sr in surveyResults)
                    {
                        foreach (var c in candidates)
                        {
                            foreach (var sur in c.WinningSurveys)
                            {
                                if (sur.WorkerId == sr.WorkerId)
                                {
                                    sr.WinningCandidate.Name = c.Name;
                                }
                            }
                        }
                    }
                    return View(surveyResults);

                }
            }
            else
            {
                return this.RedirectAndPost(Url.Action("Sorry", "Report"), null);
            }
        }

        public ActionResult ViewCandidates([System.Web.Http.FromBody]bool isValidated = false)
        {
            if (isValidated)
            {
                using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
                {
                    var candidates = db.Candidates.ToList();
                    return View(candidates);
                }
            }
            else
            {
                return this.RedirectAndPost(Url.Action("Sorry", "Report"), null);
            }
        }

        public ActionResult Sorry()
        {
            return View();
        }
    }
}