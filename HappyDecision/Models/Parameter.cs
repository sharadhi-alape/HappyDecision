using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HappyDecision.Models
{
    public class Parameter
    {
        public int ParameterId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ParameterType ParameterType { get; set; }

        public virtual ICollection<ParameterValue> ParameterValues { get; set; }
    }
}