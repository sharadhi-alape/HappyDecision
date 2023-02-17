using HappyDecision.DataContext;
using HappyDecision.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HappyDecision.Utils;
using Fluentx.Mvc;

using System.Diagnostics;
using System.IO;



namespace HappyDecision.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        private const string PREVIEW_ASSIGNMENT_ID = "ASSIGNMENT_ID_NOT_AVAILABLE";
        private static int ANSWERS_PER_CANDIDATE = 10;
        private static int TOTAL_NUMBER_OF_VOTERS = 29;
        private static Object CandidateLocker = new Object();
        

        public ActionResult Index(string workerId, string assignmentId = PREVIEW_ASSIGNMENT_ID, string hitId = null, string turkSubmitTo = null)
        {
            Dictionary<string, object> postData = new Dictionary<string, object>();
            if (assignmentId == PREVIEW_ASSIGNMENT_ID)
            {

                postData.Add("isPreview", true);
                Session["isPreview"] = true;

            }
            else
            {

                postData.Add("isPreview", false);
                Session["isPreview"] = false;
                lock (CandidateLocker)
                {
                    string line;
                    // The reason we use a lock here is to avoid choosing the same votingDixt / candidate when they hav N-1 answers, twice.
                    // We will always choose the votingDist with the most answers until it reaches the max amount of answers.
                    // We will always choose the candidate with the least amount of answers to try and keep them all even.
                    if (IsMobileUser())
                    {
                        return this.RedirectAndPost(Url.Action("Mobile", "Game"), null);
                    }
                    using (var db = new HappyDecisionExpirimentDbContext())
                    {
                        string workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        List<string> workerIds = new List<string>();
                        //string pathPrefix = "/CompletedParticipants/";
                        string pathFinWorker = workingDirectory + "CompletedParticipants\\WorkerIds.csv";
                        StreamReader file = new StreamReader(pathFinWorker);
                        while ((line = file.ReadLine()) != null)
                        {
                            string[] words = line.Split(',', '\t');
                            workerIds.Add(words[0]);
                        }
                        string[] completedWorkers = workerIds.ToArray();

                        if (db.SurveyResults.Count(s => s.WorkerId == workerId) > 0 || completedWorkers.Contains(workerId))
                        {
                            return this.RedirectAndPost(Url.Action("Sorry", "Game"), null);
                        }

                        

                        var survey = db.SurveyResults.Create();
                        survey.WorkerId = workerId;
                        survey.AssignmentId = assignmentId;
                        survey.HitId = hitId;
                        survey.TurkSubmitTo = turkSubmitTo;
                        Session["workerId"] = workerId;                        
                        survey.StartTime = Utils.ExtensionMethods.getTimeStamp();
                        var _20minago = survey.StartTime.AddMinutes(-20);

                        survey.DidComplete = false;
                        survey.VotingDistribution = db.VotingDistributions
                            .Include(vd => vd.Matrix.Candidates.Select(c => c.MatrixPositions))
                            .Include(vd => vd.ParameterValues.Select(pv => pv.Parameter))
                            .Include(vd => vd.ParameterValues.Select(pv => pv.Candidate.WinningSurveys))
                            .Include(vd => vd.Matrix.Questions)
                            //.Where(vd => vd.SurveyResults
                                // Count only surveys that are finished OR that are from the last 20 min (we want to give it a chance to finish and not alocate the same votingdist if 50 people start at the same time)
                                //.Count(sr => sr.DidComplete || sr.StartTime > _20minago) < (ANSWERS_PER_CANDIDATE * vd.NumberOfWinners))
                            .Where(vd => vd.VotingDistributionId == 5)
                            //.OrderBy(vd => vd.VotingDistributionId)
                            // Do not use .first() here as it sometomes leaves out some properties for unkown reason
                            .Take(1).ToArray().First();

                        List<Utils.CalculatedParameters> candidatesParameters;

                        candidatesParameters = Utils.Utils.calculateParameters(survey.VotingDistribution, TOTAL_NUMBER_OF_VOTERS);

                        List<int> winnerIds = new List<int>();

                        if (survey.VotingDistribution.Matrix.Candidates.Count == candidatesParameters.Count)
                        {
                            for (int i = 0; i < candidatesParameters.Count; i++)
                            {
                                if (candidatesParameters[i].HasMajority || candidatesParameters[i].HasPlurality || candidatesParameters[i].HasCondorcet || candidatesParameters[i].HasBorda)
                                {
                                    //WinnerIds.Add(candidatesParameters[i].CandidateId, ToString);
                                    winnerIds.Add(candidatesParameters[i].CandidateId);
                                }
                            }
                        }
                        int[] winnerList = winnerIds.ToArray();
                        //int dummyWinner = winnerList[0];
                        var temp = survey.VotingDistribution.Matrix.Candidates
                            .Where(c => winnerList.Contains(c.CandidateId));

                        survey.WinningCandidate = survey.VotingDistribution.Matrix.Candidates
                            .Where(c => winnerList.Contains(c.CandidateId))
                            .Where(c => c.Name=="Shugi")
                            //.OrderBy(c => c.WinningSurveys.Count(ws))
                            //.OrderBy(c => c.WinningSurveys.Count(ws => ws.VotingDistribution.VotingDistributionId == survey.VotingDistribution.VotingDistributionId))// < ANSWERS_PER_CANDIDATE
                            //.Where(c => c.WinningSurveys.Count(ws => ws.VotingDistribution.VotingDistributionId == survey.VotingDistribution.VotingDistributionId) < ANSWERS_PER_CANDIDATE)
                            //.Where(c => c.WinningSurveys.Count(ws => ws.VotingDistribution.Where(ws.VotingDistribution.VotingDistributionId == survey.VotingDistribution.VotingDistributionId)) < ANSWERS_PER_CANDIDATE)
                            //.OrderBy(c => c.WinningSurveys.Count(sr => sr.DidComplete || sr.StartTime > _20minago))
                            .OrderBy(c => Utils.ExtensionMethods.GetActiveOrCompletedCandidateSurvey(survey, c))
                            .First();

                        survey.UserPreference = Utils.ExtensionMethods.GetUserPreference(survey.VotingDistribution, survey.WinningCandidate);
                        //survey.UserCandidateId = survey.UserPreference[0];
                        //survey.UserCandidate = Utils.ExtensionMethods.GetCandidatefromID(survey.VotingDistribution, survey.UserPreference[0]);
                        survey.Bonus = Utils.ExtensionMethods.GetBonus(survey.VotingDistribution, survey.WinningCandidate, survey.UserPreference);


                        db.SurveyResults.Add(survey);
                        db.SaveChanges();
                        Session["survey"] = survey;
                    }
                }


            }
            //return this.RedirectAndPost(Url.Action("Intro", "Game"), postData);
            return View();
        }

        public ActionResult CreateDb(string code)
        {
            // Warning! this will delete all data in the db and will replace it with new data
            if (code != "DELETE_ALL") throw new UnauthorizedAccessException();

            // create candidates 
            SqlConnection.ClearAllPools();
            Database.SetInitializer<HappyDecisionExpirimentDbContext>(new DropCreateDatabaseIfModelChanges<HappyDecisionExpirimentDbContext>());


            using (var db = new HappyDecisionExpirimentDbContext())
            {
                db.Answers.RemoveRange(db.Answers);
                db.Questions.RemoveRange(db.Questions);
                db.SurveyResults.RemoveRange(db.SurveyResults);
                db.ParameterValues.RemoveRange(db.ParameterValues);
                db.VotingDistributions.RemoveRange(db.VotingDistributions);
                db.MatrixPositions.RemoveRange(db.MatrixPositions);
                db.Candidates.RemoveRange(db.Candidates);
                db.Matrices.RemoveRange(db.Matrices);
                db.Parameters.RemoveRange(db.Parameters);
                db.Demographies.RemoveRange(db.Demographies);
                db.Consents.RemoveRange(db.Consents);
                db.Errors.RemoveRange(db.Errors);
                db.ReadingTimes.RemoveRange(db.ReadingTimes);
                db.SaveChanges();

                var hasMajority = new Parameter()
                {
                    Name = "Has Majority",
                    Description = "Does the winning candidate win more than half of the total votes",
                    ParameterType = ParameterType.Boolean,
                };
                var hasPlurality = new Parameter()
                {
                    Name = "Has Plurality",
                    Description = "Does winning candidate win more votes than any other candidate",
                    ParameterType = ParameterType.Boolean,
                };

                var hasCondorcet = new Parameter()
                {
                    Name = "Has Condorcet",
                    Description = "Does winning candidate win head-to-head with all of the other candidates",
                    ParameterType = ParameterType.Boolean,
                };

                var hasBorda = new Parameter()
                {
                    Name = "Has Borda",
                    Description = "Does winning candidate win according to the borda rule",
                    ParameterType = ParameterType.Boolean,
                };

                var totalRoundsWon = new Parameter()
                {
                    Name = "Total rounds won",
                    Description = "Total number of rounds this candidate won head-to-head with all other candidates",
                    ParameterType = ParameterType.String,
                };

                var condorcetTies = new Parameter()
                {
                    Name = "Condorcet Ties",
                    Description = "The number of ties in head-to-head rounds against all other candidates",
                    ParameterType = ParameterType.String,
                };

                var pluralityTies = new Parameter()
                {
                    Name = "Plurality Ties",
                    Description = "The number of ties with other candidates in first place voting",
                    ParameterType = ParameterType.String,
                };

                var bordaTies = new Parameter()
                {
                    Name = "Borda Ties",
                    Description = "The number of ties with other candidates while scored in reverse proportion to the ranking",
                    ParameterType = ParameterType.String,
                };


                var firstPlaceVoting = new Parameter()
                {
                    Name = "First place voting",
                    Description = "The number of first place voting this candidate has",
                    ParameterType = ParameterType.String,
                };

                var bordaPoints = new Parameter()
                {
                    Name = "Total borda points",
                    Description = "Total number of points using borda points voting method",
                    ParameterType = ParameterType.String,
                };


                //var distanceFromPlurality = new Parameter()
                //{
                //    Name = "Distance from plurality",
                //    Description = "The ratio of this candidate's first place voting to the candidate with the most first place voting",
                //    ParameterType = ParameterType.String,
                //};

                var distanceFromMajority = new Parameter()
                {
                    Name = "Distance from majority",
                    Description = "The number of first place voting missing to this candidate for having majority",
                    ParameterType = ParameterType.String,
                };



                //db.Parameters.AddRange(new[] { hasMajority, hasPlurality, totalRoundsWon });
                //db.SaveChanges();

                var matrix = db.Matrices.Create();
                var candidates = new List<Candidate>() {
                    new Candidate() {
                        Name = "Branflakes",
                        MatrixPositions = new MatrixPositions() {
                            Value = new int[] {0,0,1,2,1,2}
                        },
                        Matrix = matrix
                    },
                    new Candidate() {
                        Name = "Cariot",
                        MatrixPositions = new MatrixPositions() {
                            Value = new int[] {1,2,0,0,2,1}
                        },
                        Matrix = matrix
                    },
                    new Candidate() {
                        Name = "Shugi",
                        MatrixPositions = new MatrixPositions() {
                            Value = new int[] {2,1,2,1,0,0}
                        },
                        Matrix = matrix
                    }
                };

                matrix.Candidates = candidates;

                int[,] VotingDistributionPhase1 = new int[,] { {6,4,4,7,4,4},{6,2,8,5,4,4},{1,4,7,6,7,4}
                                    ,{3,4,6,5,6,5},{6,2,3,2,7,9},{6,4,3,5,6,5},{2,6,5,5,8,3},{4,5,5,5,6,4}
                                    ,{3,4,3,7,8,4},{6,6,6,4,2,5},{3,5,5,4,5,7},{3,8,5,6,3,4},{5,2,6,4,6,6}
                                    ,{8,2,4,5,6,4},{6,4,3,6,6,4}};
                matrix.Candidates = candidates;

                matrix.VotingDistributions = Enumerable.Range(0, 15).Select(i =>
                    new VotingDistribution()
                    {
                        //Value = UniformVotingDistributionGenerator(matrix.Candidates.Count.Factorial(), TOTAL_NUMBER_OF_VOTERS),
                        //Matrix = matrix

                        Value = GetithVotingDistributionFromPhase1(VotingDistributionPhase1, i),
                        Matrix = matrix

                    }).ToList();

                if (matrix.MatrixId == 0)
                {
                    db.Matrices.Add(matrix);
                }
                db.SaveChanges();
                List<Utils.CalculatedParameters> candidatesParameters;
                foreach (var votingDistribution in matrix.VotingDistributions)
                {
                    // TEST PARAMETERS

                    candidatesParameters = Utils.Utils.calculateParameters(votingDistribution, TOTAL_NUMBER_OF_VOTERS);

                    votingDistribution.ParameterValues = new List<ParameterValue>();

                    //int winnerCount = 0;
                    votingDistribution.NumberOfWinners = 0;

                    for (int i = 0; i < candidatesParameters.Count(); i++)
                    {

                        votingDistribution.ParameterValues.Add(new ParameterValue() { Parameter = hasMajority, Candidate = candidates[i], Value = candidatesParameters[i].HasMajority.ToString(), VotingDistribution = votingDistribution, Explanation = candidatesParameters[i].HasMajorityExplanation, Weight = candidatesParameters[i].HasMajorityWeight });
                        votingDistribution.ParameterValues.Add(new ParameterValue() { Parameter = hasPlurality, Candidate = candidates[i], Value = candidatesParameters[i].HasPlurality.ToString(), VotingDistribution = votingDistribution, Explanation = candidatesParameters[i].HasPluralityExplanation, Weight = candidatesParameters[i].HasPluralityWeight });
                        votingDistribution.ParameterValues.Add(new ParameterValue() { Parameter = hasCondorcet, Candidate = candidates[i], Value = candidatesParameters[i].HasCondorcet.ToString(), VotingDistribution = votingDistribution, Explanation = candidatesParameters[i].HasCondorcetExplanation, Weight = candidatesParameters[i].HasCondorcetWeight });
                        votingDistribution.ParameterValues.Add(new ParameterValue() { Parameter = hasBorda, Candidate = candidates[i], Value = candidatesParameters[i].HasBorda.ToString(), VotingDistribution = votingDistribution, Explanation = candidatesParameters[i].HasBordaExplanation, Weight = candidatesParameters[i].HasBordaWeight });
                        votingDistribution.ParameterValues.Add(new ParameterValue() { Parameter = firstPlaceVoting, Candidate = candidates[i], Value = candidatesParameters[i].FirstPlaceVoting.ToString(), VotingDistribution = votingDistribution, Explanation = candidatesParameters[i].FirstPlaceVotingExplanation, Weight = candidatesParameters[i].FirstPlaceVotingWeight });
                        votingDistribution.ParameterValues.Add(new ParameterValue() { Parameter = bordaPoints, Candidate = candidates[i], Value = candidatesParameters[i].TotalBordaPoints.ToString(), VotingDistribution = votingDistribution, Explanation = candidatesParameters[i].TotalBordaPointsExplanation, Weight = candidatesParameters[i].TotalBordaPointsWeight });
                        votingDistribution.ParameterValues.Add(new ParameterValue() { Parameter = condorcetTies, Candidate = candidates[i], Value = candidatesParameters[i].CondorcetTies.ToString(), VotingDistribution = votingDistribution, Explanation = candidatesParameters[i].CondorcetTiesExplanation, Weight = candidatesParameters[i].CondorcetTiesWeight });
                        votingDistribution.ParameterValues.Add(new ParameterValue() { Parameter = pluralityTies, Candidate = candidates[i], Value = candidatesParameters[i].PluralityTies.ToString(), VotingDistribution = votingDistribution, Explanation = candidatesParameters[i].PluralityTiesExplanation, Weight = candidatesParameters[i].PluralityTiesWeight });
                        votingDistribution.ParameterValues.Add(new ParameterValue() { Parameter = distanceFromMajority, Candidate = candidates[i], Value = candidatesParameters[i].DistanceFromMajority.ToString(), VotingDistribution = votingDistribution, Explanation = candidatesParameters[i].DistanceFromMajorityExplanation, Weight = candidatesParameters[i].DistanceFromMajorityWeight });
                        //votingDistribution.ParameterValues.Add(new ParameterValue() { Parameter = distanceFromPlurality, Candidate = candidates[i], Value = candidatesParameters[i].DistanceFromPlurality.ToString(), VotingDistribution = votingDistribution, Explanation = candidatesParameters[i].DistanceFromPluralityExplanation, Weight = candidatesParameters[i].DistanceFromPluralityWeight });
                        votingDistribution.ParameterValues.Add(new ParameterValue() { Parameter = totalRoundsWon, Candidate = candidates[i], Value = candidatesParameters[i].TotalRoundsWon.ToString(), VotingDistribution = votingDistribution, Explanation = candidatesParameters[i].TotalRoundsWonExplanation, Weight = candidatesParameters[i].TotalRoundsWonWeight });
                        votingDistribution.ParameterValues.Add(new ParameterValue() { Parameter = bordaTies, Candidate = candidates[i], Value = candidatesParameters[i].BordaTies.ToString(), VotingDistribution = votingDistribution, Explanation = candidatesParameters[i].BordaTiesExplanation, Weight = candidatesParameters[i].BordaTiesWeight });

                        if (candidatesParameters[i].HasMajority || candidatesParameters[i].HasPlurality || candidatesParameters[i].HasCondorcet || candidatesParameters[i].HasBorda)
                        {
                            votingDistribution.NumberOfWinners++;
                        }
                    }

                    /*votingDistribution.ParameterValues = new List<ParameterValue>()
                    {
                        // TODO: calculate parameters:
                  
                        new ParameterValue() {Parameter = hasPlurality, Candidate=candidates[0], Value = "True", VotingDistribution=votingDistribution},
                        new ParameterValue() {Parameter = hasPlurality, Candidate=candidates[1], Value = "False", VotingDistribution=votingDistribution},
                        new ParameterValue() {Parameter = hasPlurality, Candidate=candidates[2], Value = "False", VotingDistribution=votingDistribution},

                        new ParameterValue() {Parameter = hasMajority, Candidate=candidates[0], Value = "False", VotingDistribution=votingDistribution},
                        new ParameterValue() {Parameter = hasMajority, Candidate=candidates[1], Value = "True", VotingDistribution=votingDistribution},
                        new ParameterValue() {Parameter = hasMajority, Candidate=candidates[2], Value = "False", VotingDistribution=votingDistribution},

                        new ParameterValue() {Parameter = totalRoundsWon, Candidate=candidates[0], Value = "1", VotingDistribution=votingDistribution},
                        new ParameterValue() {Parameter = totalRoundsWon, Candidate=candidates[1], Value = "2", VotingDistribution=votingDistribution},
                        new ParameterValue() {Parameter = totalRoundsWon, Candidate=candidates[2], Value = "3", VotingDistribution=votingDistribution}
                    };*/
                }
                db.ParameterValues.AddRange(matrix.VotingDistributions.SelectMany(vd => vd.ParameterValues));
                db.SaveChanges();




                // add questions:
                var questions = new List<Question>() {
                    new Question() {
                        //QuestionNumber = 1,
                        QuestionType = QuestionType.Scale,
                        Text = "How do you feel that your first preference, <insert preference>, has not won and you have not got the maximum bonus?",
                        ExtraNotes = "Initial Emotion",
                        Matrix = matrix
                    },

                    new Question() {
                        //QuestionNumber = 2,
                        QuestionType = QuestionType.Scale,
                        Text = "How satisfied are you with the SELECTION MECHANISM ending up with <insert winner> as the winner, given the data presented to you?",
                        ExtraNotes = "Satisfaction",
                        Matrix = matrix
                    },

                     
                    //new Question() {
                    //    QuestionType = QuestionType.Scale,
                    //    Text = "How satisfied are you with the CHOICE <insert winner> selected as the winner based on your own preference and the preference of others as reflected in the table?",
                    //    ExtraNotes = "Satisfaction",
                    //    Matrix = matrix
                    //},
                    new Question() {
                        //QuestionNumber = 3,
                        QuestionType = QuestionType.Scale,
                        Text = "Do you think <insert winner> was rightfully elected?",
                        ExtraNotes = "Agreement",
                        Matrix = matrix
                    },
                    new Question() {
                        //QuestionNumber = 4,
                        QuestionType = QuestionType.Closed,
                        //Text = "If your answer to the previous question was 'No' or 'Not Necessarily', which among the following is the more rightful winner? If not, please select 'None'. ",
                        Text = "I believe option <select> should have been elected, based on the data.",
                        ExtraNotes = "OtherOptions",
                        //Value = new string[] {"Branflakes", "Cariot", "Shugi"},
                        Matrix = matrix
                    },
                    //new Question() {
                    //    QuestionType = QuestionType.Closed,
                    //    Text = "I believe option <select> should not, in any case, be elected as a winner, based on the data",
                    //    Value = new string[] {"Branflakes", "Cariot", "Shugi", "None"},
                    //    Matrix = matrix
                    //},

                    new Question() {
                        //QuestionNumber = 5,
                        QuestionType = QuestionType.Open,
                        Text = "Please explain all of your answers.",
                        Matrix = matrix
                    },
                    //new Question() {
                    //    QuestionType = QuestionType.Open,
                    //    Text = "Do you think the election process was fair and effective, given the winner? If not, please tell us who do you think should have won, and why.",
                    //    Matrix = matrix
                    //},

                     new Question() {
                        //QuestionNumber = 5,
                        QuestionType = QuestionType.Open,
                        Text = "Do you have any thoughts or did you have any technical problems?",
                        ExtraNotes = "General Feedback",
                        Matrix = matrix
                    },
                };
                db.Questions.AddRange(questions);
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { workerId = Guid.NewGuid().ToString(), assignmentId = "stam" });
        }

        private static Random rand = new Random();
        public static int[] UniformVotingDistributionGenerator(int numberOfPermutations, int totalVoters)
        {
            // Initialize array - at least 1 vote for each permutation 
            int[] votingDistributation = Enumerable.Range(0, numberOfPermutations).Select(i => 1).ToArray();

            // Generate voting distribution 


            for (int i = numberOfPermutations; i < totalVoters; i++)
            {
                votingDistributation[rand.Next(0, numberOfPermutations)]++;
            }
            return votingDistributation;
        }

        public static int[] GetithVotingDistributionFromPhase1(int[,] votingDistribution, int i)
        {
            // Initialize array - at least 1 vote for each permutation 
            int[] votingDistributation = new int[votingDistribution.GetLength(1)];

            // Generate voting distribution 


            for (int j = 0; j < votingDistribution.GetLength(1); j++)
            {
                votingDistributation[j] = votingDistribution[i, j];
            }
            return votingDistributation;
        }

        public Boolean IsMobileUser()
        {
            string sUA = Request.UserAgent.Trim().ToLower();

            string[] options = { "ipod", "iphone", "android", "opera mobi", "fennec" };
            foreach (string option in options)
            {
                if (sUA.Contains(option))
                {
                    return true;
                }
            }

            if (sUA.Contains("windows phone os") && sUA.Contains("iemobile"))
            {
                return true;
            }

            // no evidence that the device is mobile.
            return false;
        }
    }
}
