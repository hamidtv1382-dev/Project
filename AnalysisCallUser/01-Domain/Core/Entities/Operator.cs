using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnalysisCallUser._01_Domain.Core.Entities
{
    public class Operator
    {
        public int OperatorID { get; set; }
        public string OperatorName { get; set; }

        public int CountryID { get; set; }

        [ForeignKey("CountryID")]
        public Country Country { get; set; }

        public ICollection<CallDetail> OriginCallDetails { get; set; }
        public ICollection<CallDetail> DestCallDetails { get; set; }
    }
}