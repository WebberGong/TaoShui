using System.Collections.Generic;

namespace WebSite
{
    public class FootballLeague
    {
        public FootballLeague(string name, IList<FootballMatch> matches)
        {
            Name = name;
            Matches = matches;
        }

        public string Name { get; set; }
        public IList<FootballMatch> Matches { get; set; }
    }
}