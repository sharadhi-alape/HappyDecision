using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HappyDecision.Models
{
    public class Question : HasComplexValue<string[]>
    {
        public int QuestionId { get; set; }
        //public int QuestionNumber { get; set; }
        public Matrix Matrix { get; set; }
        public virtual ICollection<Answer> Answers { get; set; }
        [Required]
        public string Text { get; set; }
        public QuestionType QuestionType { get; set; }
        public string ExtraNotes { get; set; }
    }
}