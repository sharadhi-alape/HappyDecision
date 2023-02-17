using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HappyDecision.Models
{
    public class Matrix
    {
        public int MatrixId { get; set; }
        [Required]
        public virtual ICollection<VotingDistribution> VotingDistributions { get; set; }

        public virtual ICollection<Candidate> Candidates { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
    }
}