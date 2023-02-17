using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HappyDecision.Models
{
    public class VotingDistribution : HasComplexValue<int[]>
    {
        public int VotingDistributionId { get; set; }
        public Matrix Matrix { get; set; }
        public virtual ICollection<ParameterValue> ParameterValues { get; set; }
        public int NumberOfWinners { get; set; }
        public virtual ICollection<SurveyResult> SurveyResults { get; set; }
    }
}