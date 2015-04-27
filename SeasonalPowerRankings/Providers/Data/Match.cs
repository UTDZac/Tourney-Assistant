namespace ChallongeMatchViewer.Providers.Data
{
    public class Match
    {
        public int id { get; set; }
        public string state { get; set; }
        public int round { get; set; }
        public string identifier { get; set; }
        public int? player1_id { get; set; }
        public int? player1_prereq_match_id { get; set; }
        public int? player2_id { get; set; }
        public int? player2_prereq_match_id { get; set; }
        public int? loser_id { get; set; }
        public int? winner_id { get; set; }
    }
}
