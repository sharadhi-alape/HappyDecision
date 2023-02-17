using HappyDecision.DataContext;
using HappyDecision.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HappyDecision.Utils
{
    public static class ExtensionMethods
    {
        public static String TIME_STAMP_FORMAT = "dd.MM.yyyy HH:mm:ss";

        public static int Factorial(this int n)
        {
            int result = 1;
            for (int i = 1; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        public static SurveyResult GetSurvey(this HttpSessionStateBase session)
        {
            return session["survey"] as SurveyResult;
        }

        public static VotingDistribution GetVotingDistribution(this HttpSessionStateBase session)
        {
            return session.GetSurvey().VotingDistribution;
        }

        public static Matrix GetMatrix(this HttpSessionStateBase session)
        {
            return session.GetVotingDistribution().Matrix;
        }

        public static int GetActiveOrCompletedCandidateSurvey(SurveyResult survey, Candidate c)
        {

            int count = 0;

            foreach (var sr in c.WinningSurveys)
            {
                if (sr.DidComplete || sr.StartTime > survey.StartTime.AddMinutes(-20))
                {

                    if (sr.VotingDistribution != null)
                    {
                        if (sr.VotingDistribution.VotingDistributionId == survey.VotingDistribution.VotingDistributionId)
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        public static int GetUserPreference(VotingDistribution votingDistribution, Candidate WinningCandidate)
        {

            int[,] CandidateMatrix = Utils.GetIntMatrixFromDb(votingDistribution);
            List<int> PossiblePositions = new List<int>();
            int UserPreferenceIndex;
            //int[] UserPreference;
            Random rand = new Random();
            for (int i = 0; i < votingDistribution.Matrix.Candidates.Count.Factorial(); i++)
            {
                if (CandidateMatrix[i, 0] != WinningCandidate.CandidateId)
                {
                    PossiblePositions.Add(i);
                }
            }
            //int[]  WinnerPositions = WinnerPositions.ToArray();
            //int[] PossibleUserPreference;
            UserPreferenceIndex = PossiblePositions[rand.Next(0, PossiblePositions.Count)];
            //int[] UserPreference = new int[] { CandidateMatrix[UserPreferenceIndex, 0], CandidateMatrix[UserPreferenceIndex, 1], CandidateMatrix[UserPreferenceIndex, 2] };
            //return({ CandidateMatrix[UserPreferenceIndex,0], CandidateMatrix[UserPreferenceIndex, 1], CandidateMatrix[UserPreferenceIndex, 2]})
            return UserPreferenceIndex;
        }
        public static string GetUserFirstPreference(VotingDistribution votingDistribution, int userPreference)
        {
            var candidates = GetCandidatesFromDb();
            string[,] CandidateMatrix = new string[votingDistribution.Value.Length, candidates.Count];
            foreach (var candidate in candidates)
            {
                for (int i = 0; i < candidate.MatrixPositions.Value.Length; i++)
                {
                    CandidateMatrix[i, candidate.MatrixPositions.Value[i]] = candidate.Name;
                }
            }
            return CandidateMatrix[userPreference, 0];
        }
        public static Candidate GetCandidatefromID(VotingDistribution votingDistribution, int candidateID)
        {
            foreach (var candidate in votingDistribution.Matrix.Candidates)
            {
                if (candidate.CandidateId == candidateID)
                {
                    return candidate;
                }
            }

            return null;
        }

        public static double GetBonus(VotingDistribution votingDistribution, Candidate WinningCandidate, int userPreference)
        {
            int[,] CandidateMatrix = Utils.GetIntMatrixFromDb(votingDistribution);
            int pos = 0;
            for (int i = 0; i < CandidateMatrix.GetLength(1); i++)
            {
                if (CandidateMatrix[userPreference, i] == WinningCandidate.CandidateId)
                {
                    pos = i;
                }
            }
            return 0.2 / Math.Pow(2, pos);
        }

        public static string[] GetCandidatesOtherThanWinner(Candidate winningCandidate)
        {
            string[] RemCandidates = new string[2];
            List<string> nonWinnerIds = new List<string>();
            foreach (var candidate in GetCandidatesFromDb())
            {
                if (candidate.Name != winningCandidate.Name)
                {
                    nonWinnerIds.Add(candidate.Name);
                }
            }
            //nonWinnerIds.Add("None");
            RemCandidates = nonWinnerIds.ToArray();
            //RemCandidates[2] = "None";
            return (RemCandidates);
        }

        public static void WriteErrorToDB(String worker_id, String error_msg, String url)
        {
            using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
            {
                var error = new Error()
                {
                    PlayerWorkerId = worker_id,
                    EMessage = error_msg,
                    lastScreen = url,
                };
                db.Errors.Add(error);
                db.SaveChanges();
            }
        }

        public static SurveyResult GetSurveyFromWorkerId(string wId)
        {
            //return session["survey"] as SurveyResult;
            //var survey = null;
            SurveyResult sr = new SurveyResult();
            using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
            {
                //sr = db.SurveyResults.Find(wId);
                var surveyResults = db.SurveyResults.ToList();
                var votingDistributions = db.VotingDistributions.ToList();
                var candidates = db.Candidates.ToList();

                foreach (var surveyResult in surveyResults)
                {

                    if (surveyResult.WorkerId == wId)
                    {
                        sr.WorkerId = wId;
                        sr.AssignmentId = surveyResult.AssignmentId;
                        sr.HitId = surveyResult.HitId;
                        sr.TurkSubmitTo = surveyResult.TurkSubmitTo;
                        sr.StartTime = surveyResult.StartTime;
                        sr.UserPreference = surveyResult.UserPreference;
                        sr.Bonus = surveyResult.Bonus;



                        foreach (var vod in votingDistributions)
                        {
                            foreach (var vsr in vod.SurveyResults)
                            {
                                if (vsr.WorkerId == surveyResult.WorkerId)
                                {
                                    sr.VotingDistribution = vod;
                                }
                            }
                        }

                        foreach (var c in candidates)
                        {
                            foreach (var csr in c.WinningSurveys)
                            {
                                if (csr.WorkerId == surveyResult.WorkerId)
                                {
                                    sr.WinningCandidate = c;
                                }
                            }
                        }

                        return sr;

                    }
                }

               

            }

            return null;
        }

        public static List<Question> GetQuestionsFromDb()
        {
            using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
            {

                return db.Questions.ToList();

            }
        }

        public static List<Candidate> GetCandidatesFromDb()
        {
            using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
            {
                var candidates = db.Candidates.ToList();
                var matrixPositions = db.MatrixPositions.ToList();
                foreach (var candidate in candidates)
                {
                    foreach(var matrixPosition in matrixPositions)
                    {
                        foreach(var c in matrixPosition.Candidates)
                        {
                            if(c.CandidateId == candidate.CandidateId)
                            {
                                candidate.MatrixPositions = matrixPosition;
                            }
                        }
                    }
                }
                return candidates;

            }
        }

        public static List<MatrixPositions> GetMatrixPositionsFromDb()
        {
            using (HappyDecisionExpirimentDbContext db = new HappyDecisionExpirimentDbContext())
            {
                var matrixPositions = db.MatrixPositions.ToList();
                foreach (var matrixPosition in matrixPositions)
                {
                    int[] position = matrixPosition.Value;
                    matrixPosition.Value = position;
                }

                return matrixPositions;

            }
        }

        public static List<ParameterValue> GetParameterValuesFromDb()
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
                                pvalue.Candidate.CandidateId = candidate.CandidateId;
                            }
                        }
                    }

                    foreach (var vd in votingDistributions)
                    {
                        foreach (var pval in vd.ParameterValues)
                        {
                            if (pval.ParameterValueId == pvalue.ParameterValueId)
                            {
                                pvalue.VotingDistribution.VotingDistributionId = vd.VotingDistributionId;
                            }
                        }
                    }
                }
                return pvalues;
            }
        }

        public static DateTime getTimeStamp()
        {
            TimeZoneInfo israelTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            DateTime utc = DateTime.UtcNow;
            DateTime dateTimeEnd = TimeZoneInfo.ConvertTimeFromUtc(utc, israelTimeZone);
            //return dateTimeEnd.ToString(TIME_STAMP_FORMAT);
            return dateTimeEnd;
        }

        public static String getTimeStampInString()
        {
            TimeZoneInfo israelTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            DateTime utc = DateTime.UtcNow;
            DateTime dateTimeEnd = TimeZoneInfo.ConvertTimeFromUtc(utc, israelTimeZone);
            return dateTimeEnd.ToString(TIME_STAMP_FORMAT);
            //return dateTimeEnd;
        }

        public static String GetTimeDiff(String time_start, String time_end)
        {
            DateTime dateTimeStart = DateTime.ParseExact(time_start, TIME_STAMP_FORMAT, System.Globalization.CultureInfo.InvariantCulture);
            DateTime dateTimeEnd = DateTime.ParseExact(time_end, TIME_STAMP_FORMAT, System.Globalization.CultureInfo.InvariantCulture);

            TimeSpan span = dateTimeEnd.Subtract(dateTimeStart);
            String hours = timeToStr(span.Hours);
            String minutes = timeToStr(span.Minutes);
            String seconds = timeToStr(span.Seconds);

            String diff = hours + ":" + minutes + ":" + seconds;
            return diff;
        }

        public static String timeToStr(long timeUnit)
        {
            String str = timeUnit + "";
            if (str.Length == 1)
            {
                str = "0" + str;
            }
            return str;
        }
    }
}