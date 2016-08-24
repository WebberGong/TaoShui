using Newtonsoft.Json;

namespace Entity
{
    public class FootballData
    {
        private static string _emptyObjectJsonString;

        public string LeagueName { get; set; }
        public string HostTeam { get; set; }
        public string GuestTeam { get; set; }
        public string DrawnGame { get; set; }
        public string Score { get; set; }
        public string Time { get; set; }
        public string WholeConcedePoints { get; set; }
        public string WholeConcedeHostOdds { get; set; }
        public string WholeConcedeGuestOdds { get; set; }
        public string WholeSizeBallPoints { get; set; }
        public string WholeSizeBallHostOdds { get; set; }
        public string WholeSizeBallGuestOdds { get; set; }
        public string WholeOneByTwoHostOdds { get; set; }
        public string WholeOneByTwoGuestOdds { get; set; }
        public string WholeOneByTwoDrawnGameOdds { get; set; }
        public string HalfConcedePoints { get; set; }
        public string HalfConcedeHostOdds { get; set; }
        public string HalfConcedeGuestOdds { get; set; }
        public string HalfSizeBallPoints { get; set; }
        public string HalfSizeBallHostOdds { get; set; }
        public string HalfSizeBallGuestOdds { get; set; }
        public string HalfOneByTwoHostOdds { get; set; }
        public string HalfOneByTwoGuestOdds { get; set; }
        public string HalfOneByTwoDrawnGameOdds { get; set; }

        public static string GetEmptyObjectJsonString()
        {
            if (string.IsNullOrEmpty(_emptyObjectJsonString))
                _emptyObjectJsonString = JsonConvert.SerializeObject(new FootballData());
            return _emptyObjectJsonString;
        }
    }
}