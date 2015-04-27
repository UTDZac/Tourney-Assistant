using System;
using System.Collections.Generic;
using System.Linq;

namespace ChallongeMatchViewer.Objects
{
    public class ChallongeMatch
    {
        public ChallongeMatch()
        {
            Id = -1;
            State = MatchType.Pending;
            Round = 0;
            Identifier = string.Empty;
            Player1Id = -1;
            Player2Id = -1;
            Player1PrevMatchId = -1;
            Player2PrevMatchId = -1;
            LoserId = -1;
            WinnerId = -1;

            Player1 = null;
            Player2 = null;
            CreatedAt = DateTime.Now;
            Station = string.Empty;
            SentNotification = false;
        }

        public int Id { get; set; }
        public MatchType State { get; set; }
        public int Round { get; set; }
        public string Identifier { get; set; }
        public int Player1Id { get; set; }
        public int Player2Id { get; set; }
        public int Player1PrevMatchId { get; set; }
        public int Player2PrevMatchId { get; set; }
        public int WinnerId { get; set; }
        public int LoserId { get; set; }

        public ChallongeParticipant Player1 { get; set; }
        public ChallongeParticipant Player2 { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Station { get; set; }
        public bool SentNotification { get; set; }

        public override string ToString()
        {
            if (Player1 == null || Player2 == null)
                return string.Empty;

            if (!string.IsNullOrEmpty(Station))
                return string.Format("{0} - {1} vs. {2}", Station, Player1.Name, Player2.Name);

            return string.Format("{0} vs. {1}", Player1.Name, Player2.Name);
        }
    }
}