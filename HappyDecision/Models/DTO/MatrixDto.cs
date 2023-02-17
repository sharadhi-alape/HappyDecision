using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HappyDecision.Utils;

namespace HappyDecision.Models.Dto
{
    public class MatrixDto
    {
        public string[,] CandidateMatrix { get; set; }
        public int[] Votes { get; set; }
        public string WinningCandidate { get; set; }
        public string[] UserPreferenceString { get; set; }
        public ParameterValue[] WinningCandidateParameters;
        //public Candidate UserCandidate;
        public int UserPreferenceColumn { get; set; }

        public MatrixDto()
        {

        }

        public MatrixDto(VotingDistribution votingDistribution, Candidate winningCandidate, int UserPreference)
        {
            Votes = votingDistribution.Value;
            WinningCandidate = winningCandidate.Name;
            UserPreferenceColumn = UserPreference;
            var candidates = Utils.ExtensionMethods.GetCandidatesFromDb();
            var candidateMatrixPositions = Utils.ExtensionMethods.GetMatrixPositionsFromDb();

            CandidateMatrix = new string[Votes.Length, candidates.Count];
            foreach (var candidate in candidates)
            {
                for (int i = 0; i < candidate.MatrixPositions.Value.Length; i++)
                {
                    CandidateMatrix[i, candidate.MatrixPositions.Value[i]] = candidate.Name;
                }
            }
            //WinningCandidateParameters = votingDistribution.ParameterValues
            WinningCandidateParameters = Utils.ExtensionMethods.GetParameterValuesFromDb()
                .Where(p => p.Candidate.CandidateId == winningCandidate.CandidateId)
                .Where(p => p.VotingDistribution.VotingDistributionId == votingDistribution.VotingDistributionId)
                .OrderByDescending(p => p.Weight)
                //.Where(p => p.Weight > 0 && p.Weight < 1)
                .ToArray();
            UserPreferenceString = new string[3];
            //UserPreferenceString = $"{Utils.Utils.GetCandidateNameFromID(votingDistribution, UserPreference[0])} over {Utils.Utils.GetCandidateNameFromID(votingDistribution, UserPreference[1])} over {Utils.Utils.GetCandidateNameFromID(votingDistribution, UserPreference[2])}.";
            for (int i = 0; i < 3; i++)

            {

                UserPreferenceString[i] = CandidateMatrix[UserPreferenceColumn, i];

            }
        }
    }
}