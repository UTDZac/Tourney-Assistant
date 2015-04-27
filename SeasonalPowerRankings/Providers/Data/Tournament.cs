using System.Collections.Generic;

namespace ChallongeMatchViewer.Providers.Data
{
    public class Tournament
    {
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string subdomain { get; set; }
        public string state { get; set; }
        public object created_at { get; set; }
        public object completed_at { get; set; }
        public List<ParticipantRoot> participants { get; set; }
        public List<MatchRoot> matches { get; set; }
    }
}
