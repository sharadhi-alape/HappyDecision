using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HappyDecision.Models
{
    public class Answer
    {
        public int AnswerId { get; set; }
        [Required]
        public SurveyResult SurveyResult { get; set; }
        [Required]
        public Question Question { get; set; }
        [Required]
        public string Value { get; set; }
    }
}