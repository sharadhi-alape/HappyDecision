using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HappyDecision.Models
{
    public class Consent
    {
        public int ConsentId { get; set; }
        public string SurveyResultWorkerId { get; set; }
        public bool DidAgree { get; set; }
    }
}