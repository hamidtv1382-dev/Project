namespace AnalysisCallUser._01_Domain.Core.Entities
{
    public class City
    {
        public int CityID { get; set; }
        public string CityName { get; set; }
        public int CountryID { get; set; }

        public Country Country { get; set; }
        public ICollection<CallDetail> OriginCallDetails { get; set; }
        public ICollection<CallDetail> DestCallDetails { get; set; }
    }
}
