using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HappyDecision.Models
{
    public class MatrixPositions : HasComplexValue<int[]>
    {
        public int MatrixPositionsId { get; set; }
        public virtual ICollection<Candidate> Candidates { get; set; }
    }
}