using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HappyDecision.Models
{
    public class Error
    {
        public int ErrorId { get; set; }
        public  string PlayerWorkerId { get; set; }
        public string EMessage { get; set; }
        public string lastScreen { get; set; }
    }
}