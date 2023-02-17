using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HappyDecision.Models
{
    public class ReadingTime
    {
        public int ReadingTimeId { get; set; }
        public string RTWorkerId { get; set; }
        public string Screen { get; set; }
        //public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}