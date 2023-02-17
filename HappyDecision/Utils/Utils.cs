using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
//using HappyDecision.Constants;
using HappyDecision.Models;

namespace HappyDecision.Utils
{
    public class CalculatedParameters
    {
        public int CandidateId;
        public string Name;
        public int index;
        public int TotalBordaPoints;
        public int DistanceFromMajority;
        public float DistanceFromPlurality;


        public int FirstPlaceVoting;
        public int TotalRoundsWon;
        public int CondorcetTies;
        public int PluralityTies;
        public int BordaTies;

        public bool HasCondorcet;
        public bool HasMajority;
        public bool HasPlurality;
        public bool HasBorda;

        public float HasPluralityWeight;
        public float HasMajorityWeight;
        public float DistanceFromMajorityWeight;
        // public float DistanceFromPluralityWeight;
        public float FirstPlaceVotingWeight;
        public float PluralityTiesWeight;

        public float HasBordaWeight;
        public float TotalBordaPointsWeight;
        public float BordaTiesWeight;

        public float HasCondorcetWeight;
        public float TotalRoundsWonWeight;
        public float CondorcetTiesWeight;

        public string HasPluralityExplanation;
        public string HasMajorityExplanation;
        public string DistanceFromMajorityExplanation;
        //public string DistanceFromPluralityExplanation;
        public string FirstPlaceVotingExplanation;
        public string PluralityTiesExplanation;

        public string HasBordaExplanation;
        public string TotalBordaPointsExplanation;
        public string BordaTiesExplanation;

        public string HasCondorcetExplanation;
        public string TotalRoundsWonExplanation;
        public string CondorcetTiesExplanation;
    }

    public class Utils
    {
        //static int count;
        //static List<string> permutations = new List<string>();

        /*public static int[] UniformVotingDistributionGenerator(int numberOfPermutations, int totalVoters) {

            int[] votingDistributation = new int[numberOfPermutations];

            // Initialize array 
            for (int i = 0; i < numberOfPermutations; i++)
            {
                votingDistributation[i] = 0;
            }

            // Generate voting distribution 
            Random rand = new Random();

            for (int i = 0; i < totalVoters; i++)
            {
                votingDistributation[rand.Next(0, numberOfPermutations)]++;
            }
            return votingDistributation;        
        }*/

        public static int[,] GetIntMatrixFromDb(VotingDistribution votingDistribution)
        {
            int[] Votes;
            int[,] CandidateMatrix;

            Votes = votingDistribution.Value;

            CandidateMatrix = new int[Votes.Length, votingDistribution.Matrix.Candidates.Count];
            foreach (var candidate in votingDistribution.Matrix.Candidates)
            {
                for (int i = 0; i < candidate.MatrixPositions.Value.Length; i++)
                {
                    CandidateMatrix[i, candidate.MatrixPositions.Value[i]] = candidate.CandidateId;
                }
            }

            return CandidateMatrix;
        }

        public static string GetCandidateNameFromID(VotingDistribution votingDistribution, int id)
        {
            foreach (var candidate in votingDistribution.Matrix.Candidates)
            {
                if (candidate.CandidateId == id)
                {
                    return candidate.Name;
                }
            }

            return null;
        }


        // public static List<string> GetPermutations()
        // {
        //     return permutations;
        // }

        // public static int[] ConvertIntHolderToIntArray(IList<IntHolder> IntHolderList) {
        //     int[] array = new int[IntHolderList.Count()];
        //     for (int i = 0; i < IntHolderList.Count(); i++ )
        //     {
        //         array[i] = IntHolderList[i].value;
        //     }

        //     return array;
        // }

        // public static void printMatrix(int[,] matrix)
        // {
        //     int rowLength = matrix.GetLength(0);
        //     int colLength = matrix.GetLength(1);

        //     for (int i = 0; i < colLength; i++)
        //     {
        //         for (int j = 0; j < rowLength; j++)
        //         {
        //             Debug.Write(string.Format("{0} ", matrix[j, i]));
        //         }
        //         Debug.Write(Environment.NewLine + Environment.NewLine);
        //     }
        // }

        /*
         * Return -1 in case of a tie and the winners index inside CalculatedParameters array in case of a winner in a one on one round.
         */

        public static int OneOnOne(int[,] matrix, CalculatedParameters candidate1, CalculatedParameters candidate2, int numberOfCandidates, int[] votingDistribution, int permutationNumber)
        {
            int candidate1Points = 0, candidate2Points = 0;
            int j = 0;
            for (int i = 0; i < permutationNumber; i++)
            {
                while (matrix[i, j] != candidate1.CandidateId && matrix[i, j] != candidate2.CandidateId)
                {
                    j++;
                }

                if (matrix[i, j] == candidate1.CandidateId)
                {
                    candidate1Points += votingDistribution[i];
                }
                else
                {
                    candidate2Points += votingDistribution[i];
                }

                j = 0;
            }

            //Debug.WriteLine("candidate1Points: " + candidate1Points.ToString() + "candidate2Points: " + candidate2Points.ToString());

            if (candidate1Points == candidate2Points)
            {
                return -1;
            }
            if (candidate1Points > candidate2Points)
            {
                return candidate1.index;
            }
            else
            {
                return candidate2.index;
            }

        }


        public static int ToInt(bool value)
        {
            return value ? 1 : 0;
        }

        public static int GetCumulativeCondorcetCount(int candidateId, int[,] matrix, VotingDistribution votingdistribution, int permutationNumber)
        {
            int cumCondorcet = 0;
            int j = 0;
            foreach (var candidate in votingdistribution.Matrix.Candidates)
            {
                if (candidate.CandidateId != candidateId)
                {

                    for (int i = 0; i < permutationNumber; i++)
                    {
                        while (matrix[i, j] != candidateId && matrix[i, j] != candidate.CandidateId)
                        {
                            j++;
                        }

                        if (matrix[i, j] == candidateId)
                        {
                            cumCondorcet += votingdistribution.Value[i];
                        }

                        j = 0;
                    }


                }
            }




            return cumCondorcet;
        }

        public static List<CalculatedParameters> calculateParameters(VotingDistribution votingDistribution, int TotalNumberOfVoters)
        {
            //Matrix.VotersNumber = MatrixConstants.TotalVotersNumber;

            // calculate parameters 
            List<CalculatedParameters> candidatesParameters = new List<CalculatedParameters>();

            int[,] CurrentMatrix = GetIntMatrixFromDb(votingDistribution);
            int currentMaxFirstPlaceVoting = 0;
            int PluralityWinnerCount = 0;
            int BordaWinnerIndex = 0;
            int numberOfCandidates = votingDistribution.Matrix.Candidates.Count;
            int currentMaxBordaCount = 0;
            int cutoff = TotalNumberOfVoters / 2 + 1;

            // First Place Voting Calculation 
            for (int k = 0; k < numberOfCandidates; k++)
            {
                // Initialize 
                CalculatedParameters p = new CalculatedParameters();
                p.CandidateId = CurrentMatrix[0, k];
                p.index = k;
                p.FirstPlaceVoting = 0;
                p.TotalRoundsWon = 0;
                p.HasCondorcet = false;
                p.TotalBordaPoints = 0;
                p.TotalRoundsWon = 0;
                p.CondorcetTies = 0;
                p.PluralityTies = 0;
                p.HasMajority = false;
                p.HasPlurality = false;
                p.HasBorda = false;
                p.BordaTies = 0;

                for (int i = 0; i < votingDistribution.Matrix.Candidates.Count.Factorial(); i++)
                {
                    // candidateId got a first place voting
                    if (CurrentMatrix[i, 0] == p.CandidateId)
                    {
                        p.FirstPlaceVoting += votingDistribution.Value[i];
                        p.TotalBordaPoints += numberOfCandidates * votingDistribution.Value[i];
                    }
                    else
                    {
                        int j = 0;
                        for (j = 1; CurrentMatrix[i, j] != p.CandidateId; j++) ;
                        p.TotalBordaPoints += (numberOfCandidates - j) * votingDistribution.Value[i];
                    }


                }

                // Has majority calculation 
                if (p.FirstPlaceVoting > TotalNumberOfVoters / 2)
                {
                    p.HasMajority = true;
                }
                if (p.TotalBordaPoints > currentMaxBordaCount)
                {
                    currentMaxBordaCount = p.TotalBordaPoints;
                    BordaWinnerIndex = p.index;
                }
                if (p.FirstPlaceVoting >= currentMaxFirstPlaceVoting)
                {
                    currentMaxFirstPlaceVoting = p.FirstPlaceVoting;
                    //PluralitySecondPlaceIndex = PluralityCandidateIndex;
                    //PluralityCandidateIndex = p.index;
                }

                p.DistanceFromMajority = TotalNumberOfVoters / 2 - p.FirstPlaceVoting;
                candidatesParameters.Add(p);
            }

            // Calculate distance from plurality for each candidate
            //List<CalculatedParameters> PluralitySortedList = candidatesParameters.OrderByDescending(o => o.FirstPlaceVoting).ToList();

            //candidatesParameters[PluralityCandidateIndex].HasPlurality = true;
            //candidatesParameters[PluralityCandidateIndex].DistanceFromPlurality =
            //    (float)candidatesParameters[PluralityCandidateIndex].FirstPlaceVoting / (float)PluralitySortedList[1].FirstPlaceVoting;
            //for (int i = 1; i < numberOfCandidates; i++)
            //{
            //    candidatesParameters[PluralitySortedList[i].index].DistanceFromPlurality =
            //    (float)PluralitySortedList[i].FirstPlaceVoting / (float)candidatesParameters[PluralityCandidateIndex].FirstPlaceVoting;
            //}

            ////List<CalculatedParameters> BordaSortedList = candidatesParameters.OrderByDescending(o => o.TotalBordaPoints).ToList();

            //candidatesParameters[BordaWinnerIndex].HasBorda = true;

            for (int k = 0; k < candidatesParameters.Count(); k++)
            {

                if (candidatesParameters[k].FirstPlaceVoting == currentMaxFirstPlaceVoting)
                {

                    candidatesParameters[k].HasPlurality = true;
                    PluralityWinnerCount++;
                }
                candidatesParameters[k].DistanceFromPlurality = candidatesParameters[k].FirstPlaceVoting / currentMaxFirstPlaceVoting;

                if (candidatesParameters[k].TotalBordaPoints == currentMaxBordaCount)
                {

                    candidatesParameters[k].HasBorda = true;

                }


            }

            // calculate one-on-one rounds 
            int winnerIndex = 0;
            for (int i = 0; i < numberOfCandidates - 1; i++)
            {
                for (int j = i + 1; j < numberOfCandidates; j++)
                {
                    //Debug.WriteLine("Can1: " + i.ToString() + " Can2: " + j.ToString());
                    winnerIndex = OneOnOne(CurrentMatrix, candidatesParameters[i], candidatesParameters[j], numberOfCandidates, votingDistribution.Value, votingDistribution.Matrix.Candidates.Count.Factorial());
                    if (winnerIndex == -1) // it was a tie 
                    {
                        candidatesParameters[i].CondorcetTies++;
                        candidatesParameters[j].CondorcetTies++;
                    }
                    else
                    {
                        candidatesParameters[winnerIndex].TotalRoundsWon++;
                    }


                    if (candidatesParameters[i].FirstPlaceVoting == candidatesParameters[j].FirstPlaceVoting)
                    {

                        candidatesParameters[i].PluralityTies++;
                        candidatesParameters[j].PluralityTies++;

                    }


                    if (candidatesParameters[i].TotalBordaPoints == candidatesParameters[j].TotalBordaPoints)
                    {

                        candidatesParameters[i].BordaTies++;
                        candidatesParameters[j].BordaTies++;

                    }
                }

            }

            for (int i = 0; i < numberOfCandidates; i++)
            {
                if (candidatesParameters[i].TotalRoundsWon == numberOfCandidates - 1)
                {
                    candidatesParameters[i].HasCondorcet = true;
                }
            }


            //To generate explanations
            for (int i = 0; i < numberOfCandidates; i++)
            {

                candidatesParameters[i].FirstPlaceVotingWeight = (float)candidatesParameters[i].FirstPlaceVoting / TotalNumberOfVoters;
                candidatesParameters[i].FirstPlaceVotingExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has been ranked the first preference by {candidatesParameters[i].FirstPlaceVoting} voters";
                candidatesParameters[i].PluralityTiesWeight = (float)candidatesParameters[i].PluralityTies / numberOfCandidates;
                candidatesParameters[i].PluralityTiesExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} is tied with {candidatesParameters[i].PluralityTies} candidate(s) with respect to first place voting";

                if (candidatesParameters[i].HasMajority || candidatesParameters[i].HasPlurality)
                {



                    if (candidatesParameters[i].HasMajority)
                    {

                        candidatesParameters[i].DistanceFromMajorityWeight = (float)(candidatesParameters[i].FirstPlaceVoting - (float)(TotalNumberOfVoters / 2)) / (float)(TotalNumberOfVoters / 2);
                        candidatesParameters[i].DistanceFromMajorityExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has secured {(candidatesParameters[i].FirstPlaceVoting - cutoff)} votes more than the halfway threshold of {cutoff}";
                        //candidatesParameters[i].DistanceFromPluralityWeight = (float)candidatesParameters[i].FirstPlaceVoting / currentMaxFirstPlaceVoting;
                        //candidatesParameters[i].DistanceFromPluralityExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has the most number of voters rank it as the first preference";
                        candidatesParameters[i].HasMajorityWeight = (float)ToInt(candidatesParameters[i].HasMajority);
                        candidatesParameters[i].HasMajorityExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has secured more than {TotalNumberOfVoters / 2} votes";
                        candidatesParameters[i].HasPluralityWeight = (float)ToInt(candidatesParameters[i].HasPlurality);
                        candidatesParameters[i].HasPluralityExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has the most number of voters rank it as the first preference";
                        //candidatesParameters[i].PluralityTiesWeight = (float)candidatesParameters[i].PluralityTies / numberOfCandidates;
                        //candidatesParameters[i].PluralityTiesExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has {candidatesParameters[i].PluralityTies} for the first place";
                    }
                    else
                    {
                        candidatesParameters[i].DistanceFromMajorityWeight = (float)((TotalNumberOfVoters / 2) - (float)candidatesParameters[i].FirstPlaceVoting) / (float)(TotalNumberOfVoters / 2);
                        candidatesParameters[i].DistanceFromMajorityExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} needs {(cutoff - candidatesParameters[i].FirstPlaceVoting)} votes to cross the halfway threshold of {cutoff}";
                        //candidatesParameters[i].DistanceFromPluralityWeight = (float)candidatesParameters[i].FirstPlaceVoting / currentMaxFirstPlaceVoting;
                        //candidatesParameters[i].DistanceFromPluralityExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)}  has the most number of voters rank it as the first preference ";


                        candidatesParameters[i].HasMajorityWeight = (float)ToInt(candidatesParameters[i].HasMajority);
                        candidatesParameters[i].HasMajorityExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} is not the majority winner";
                        candidatesParameters[i].HasPluralityWeight = (float)ToInt(candidatesParameters[i].HasPlurality);
                        candidatesParameters[i].HasPluralityExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has the most number of voters rank it as the first preference ";
                    }


                }

                else
                {
                    candidatesParameters[i].HasPluralityWeight = (float)ToInt(candidatesParameters[i].HasPlurality);
                    candidatesParameters[i].HasPluralityExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has not secured the most first place votes";
                    candidatesParameters[i].HasMajorityWeight = (float)ToInt(candidatesParameters[i].HasMajority);
                    candidatesParameters[i].HasMajorityExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has not secured more than {TotalNumberOfVoters / 2} votes";

                    //candidatesParameters[i].DistanceFromPluralityWeight = (float)candidatesParameters[i].FirstPlaceVoting / currentMaxFirstPlaceVoting;
                    //candidatesParameters[i].DistanceFromPluralityExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has {currentMaxFirstPlaceVoting - candidatesParameters[i].FirstPlaceVoting} less than the candidate that has the most voters ranking it as their first preference";
                    candidatesParameters[i].DistanceFromMajorityWeight = (float)((TotalNumberOfVoters / 2) - (float)candidatesParameters[i].FirstPlaceVoting) / (float)(TotalNumberOfVoters / 2);
                    candidatesParameters[i].DistanceFromMajorityExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} needs {(cutoff - candidatesParameters[i].FirstPlaceVoting)} votes to cross the halfway threshold of {cutoff}";
                }

                //if (candidatesParameters[i].PluralityTies > 0)
                //{

                //    candidatesParameters[i].PluralityTiesWeight = (float)candidatesParameters[i].PluralityTies / numberOfCandidates;
                //    // To ask: Add a method to get the candidate with whom the voting is tied
                //    candidatesParameters[i].PluralityTiesExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has its first place voting tied with";

                //}
                //else
                //{
                //    candidatesParameters[i].PluralityTiesWeight = (float)candidatesParameters[i].PluralityTies / numberOfCandidates;
                //    candidatesParameters[i].PluralityTiesExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has {candidatesParameters[i].PluralityTies} tied for the first place";
                //}


                if (candidatesParameters[i].HasCondorcet)
                {

                    candidatesParameters[i].HasCondorcetWeight = (float)ToInt(candidatesParameters[i].HasCondorcet);
                    candidatesParameters[i].HasCondorcetExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has won head-to-head comparison with both the other candidates";
                    candidatesParameters[i].CondorcetTiesWeight = (float)candidatesParameters[i].CondorcetTies / numberOfCandidates;
                    candidatesParameters[i].CondorcetTiesExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has {candidatesParameters[i].CondorcetTies} ties for head to head comparison with other candidates";
                    //Calculate the head to head count
                    candidatesParameters[i].TotalRoundsWonWeight = (float)GetCumulativeCondorcetCount(candidatesParameters[i].CandidateId, CurrentMatrix, votingDistribution, votingDistribution.Matrix.Candidates.Count.Factorial()) / (TotalNumberOfVoters * (numberOfCandidates - 1));
                    candidatesParameters[i].TotalRoundsWonExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has won {candidatesParameters[i].TotalRoundsWon} rounds while winning {candidatesParameters[i].TotalRoundsWonWeight * 100} percent of the head-to-head comparisons";
                }
                else
                {
                    candidatesParameters[i].HasCondorcetWeight = (float)ToInt(candidatesParameters[i].HasCondorcet);
                    candidatesParameters[i].HasCondorcetExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has not won the head to head comparisons against both the candidates";
                    candidatesParameters[i].TotalRoundsWonWeight = (float)GetCumulativeCondorcetCount(candidatesParameters[i].CandidateId, CurrentMatrix, votingDistribution, votingDistribution.Matrix.Candidates.Count.Factorial()) / (TotalNumberOfVoters * (numberOfCandidates - 1));
                    candidatesParameters[i].TotalRoundsWonExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has won {candidatesParameters[i].TotalRoundsWon} rounds while winning {candidatesParameters[i].TotalRoundsWonWeight * 100} percent of the head-to-head comparisons";
                    candidatesParameters[i].CondorcetTiesWeight = (float)candidatesParameters[i].CondorcetTies / numberOfCandidates;
                    candidatesParameters[i].CondorcetTiesExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has {candidatesParameters[i].CondorcetTies} ties for head to head comparison with other candidates";


                }

                if (candidatesParameters[i].HasBorda)
                {
                    candidatesParameters[i].HasBordaWeight = (float)ToInt(candidatesParameters[i].HasBorda);
                    candidatesParameters[i].HasBordaExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has secured the highest score when points are given to the candidates in reverse proportion of their ranking";
                    candidatesParameters[i].TotalBordaPointsWeight = (float)candidatesParameters[i].TotalBordaPoints / (numberOfCandidates * TotalNumberOfVoters);
                    candidatesParameters[i].TotalBordaPointsExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has secured {candidatesParameters[i].TotalBordaPoints} points when scored in reverse proportion to the ranking";
                    candidatesParameters[i].BordaTiesWeight = (float)candidatesParameters[i].BordaTies / numberOfCandidates;
                    candidatesParameters[i].BordaTiesExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has {candidatesParameters[i].BordaTies} ties when scored in reverse proportion to the ranking";
                }
                else
                {
                    candidatesParameters[i].HasBordaWeight = (float)ToInt(candidatesParameters[i].HasBorda);
                    candidatesParameters[i].HasBordaExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has not secured the highest score when points are given to the candidates in reverse proportion of their ranking";
                    candidatesParameters[i].TotalBordaPointsWeight = (float)candidatesParameters[i].TotalBordaPoints / (numberOfCandidates * TotalNumberOfVoters);
                    candidatesParameters[i].TotalBordaPointsExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has secured {candidatesParameters[i].TotalBordaPoints} points when scored in reverse proportion to the ranking";
                    candidatesParameters[i].BordaTiesWeight = (float)candidatesParameters[i].BordaTies / numberOfCandidates;
                    candidatesParameters[i].BordaTiesExplanation = $"{GetCandidateNameFromID(votingDistribution, candidatesParameters[i].CandidateId)} has {candidatesParameters[i].BordaTies} ties when scored in reverse proportion to the ranking";
                }
            }


            return candidatesParameters;




        }

    }






    // public static void FindPermutations(string permutation, char[] set)
    // {

    //   if (set.Length == 1)
    //   {

    //      //Debug.WriteLine(permutation + set[0]);
    //      permutations.Add(permutation + set[0]);
    //      count++;
    //      return;
    //   }

    //   for (int i = 0; i < set.Length; i++)
    //   {

    //         char n = set[i];
    //         string newPermutation = permutation + n;
    //         char[] subset = new char[set.Length - 1];
    //         int j = 0;
    //         for (int k = 0; k < set.Length; k++) 
    //         {

    //             if (set[k] != n)
    //             {
    //                 subset[j++] = set[k];
    //             }
    //         }

    //         FindPermutations(newPermutation, subset);
    //   }

    //}


}
