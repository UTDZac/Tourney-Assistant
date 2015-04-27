using System;
using System.Collections.Generic;
using System.Linq;

namespace ChallongeMatchViewer.Objects
{
    public class ChallongeTournament
    {
        public ChallongeTournament()
        {
            Name = string.Empty;
            ApiUrl = string.Empty;
            State = string.Empty;
            MaxRound = 0;
            MinRound = 0;

            Participants = new List<ChallongeParticipant>();
            OpenMatches = new List<ChallongeMatch>();
            PendingMatches = new List<ChallongeMatch>();
            CompleteMatches = new List<ChallongeMatch>();

            StationManager = new StationManager();
        }

        public string Name { get; set; }
        public string ApiUrl { get; set; }
        public string State { get; set; }
        public int MaxRound { get; set; }
        public int MinRound { get; set; }

        public List<ChallongeParticipant> Participants { get; set; }
        public List<ChallongeMatch> OpenMatches { get; set; }
        public List<ChallongeMatch> PendingMatches { get; set; }
        public List<ChallongeMatch> CompleteMatches { get; set; }

        public StationManager StationManager { get; set; }

        public string UrlAddress()
        {
            if (ApiUrl.Contains('-'))
            {
                var s = ApiUrl.Split('-');
                return string.Format("{0}.challonge.com/{1}", s[0], s[1]);
            }

            return string.Format("www.challonge.com/{0}", ApiUrl);
        }

        public string RoundLabel(ChallongeMatch match)
        {
            if (match.Round == MaxRound)
                return "Grand Finals";
            if (match.Round == MaxRound - 1)
                return "Winners Finals";
            if (match.Round == MaxRound - 2)
                return "Winners Semifinals";
            if (match.Round == MinRound)
                return "Losers Finals";
            if (match.Round == MinRound + 1)
                return "Losers Semifinals";

            return string.Format("{0} Round {1}", match.Round > 0 ? "Winners" : "Losers", Math.Abs(match.Round));
        }
    }
}
