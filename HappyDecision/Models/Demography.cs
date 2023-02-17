using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace HappyDecision.Models
{
    public class Demography
    {

        public int DemographyId { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public string Country { get; set; }
        public string Education { get; set; }
        public string SurveyResultWorkerId { get; set; }
        public int Attempts { get; set; }
        public Boolean IsSuccessful { get; set; }


    }
}