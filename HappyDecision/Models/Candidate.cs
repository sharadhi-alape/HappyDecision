using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HappyDecision.Models
{
    public class Candidate
    {
        public int CandidateId { get; set; }
        public Matrix Matrix { get; set; }

        public string Name { get; set; }
        public MatrixPositions MatrixPositions { get; set; }
        public virtual ICollection<SurveyResult> WinningSurveys { get; set; }
        public virtual ICollection<ParameterValue> ParameterValues { get; set; }
    }
}