using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HappyDecision.Models
{
    public class ParameterValue
    {
        public int ParameterValueId { get; set; }
        public Parameter Parameter { get; set; }
        public Candidate Candidate { get; set; }
        public VotingDistribution VotingDistribution { get; set; }
        public string Value { get; set; }
        public float Weight { get; set; }
        public string Explanation { get; set; }
    }
}