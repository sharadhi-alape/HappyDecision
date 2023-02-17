using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HappyDecision.Models
{
    public class SurveyResult
    {
        [Key]
        public string WorkerId { get; set; }
        public string AssignmentId { get; set; }
        public string HitId { get; set; }
        public string TurkSubmitTo { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        [Required]
        public VotingDistribution VotingDistribution { get; set; }
        public Candidate WinningCandidate { get; set; }
        public bool DidComplete { get; set; }
        public virtual ICollection<Answer> Answers { get; set; }
        public int UserPreference { get; set; }
        public double Bonus { get; set; }
        //public int UserCandidateId { get; set; }C:\Users\User\source\repos\RedonePhase117\HappyDecision\HappyDecision\Models\SurveyResult.cs
    }
}