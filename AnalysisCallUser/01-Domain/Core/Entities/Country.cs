namespace AnalysisCallUser._01_Domain.Core.Entities
{
    public class Country
    {
        public int CountryID { get; set; }
        public string CountryName { get; set; }

        public ICollection<City> Cities { get; set; }
        public ICollection<CallDetail> OriginCallDetails { get; set; }
        public ICollection<CallDetail> DestCallDetails { get; set; }
    }
}
