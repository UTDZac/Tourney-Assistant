using ChallongeMatchViewer.Helpers;
using ChallongeMatchViewer.Objects;
using ChallongeMatchViewer.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

// === Challonge API credentials ===
// Username: tourneylocator
// Password: bzkOdW22IjxizBUN75zyF17YuAFiVLmxPgG52OP7
// * The password is the API Key

namespace ChallongeMatchViewer
{
    class Program
    {
        //public const string CHALLONGE_TOURNAMENT_KEY = "3t64kdet";
        public const string CHALLONGE_USERNAME = "tourneylocator";
        public const string CHALLONGE_APIKEY = "bzkOdW22IjxizBUN75zyF17YuAFiVLmxPgG52OP7";
        public const string CHALLONGE_SUBDOMAIN = "tloc";
        public const string PUSHBULLET_APIKEY = "vqEzlziinVtS8fSEWByes0yuqGP3NIKg";
        public const int UPDATE_INTERVAL_SEC = 30;

        private static ChallongeTournament _tournament;

        private static string TournamentKey { get; set; }

        static void Main(string[] args)
        {
            var tournaments = ChallongeApiWrapper.GetTournamentList(CHALLONGE_USERNAME, CHALLONGE_APIKEY, CHALLONGE_SUBDOMAIN);

            CsvWriter.WriteLine("tournament-list.csv", "Name of Event, Challonge URL, Participants");
            foreach (var tournament in tournaments)
            {
                var tdata = ChallongeApiWrapper.GetTournament(tournament.ApiUrl, CHALLONGE_USERNAME, CHALLONGE_APIKEY);
                var name = tdata.Name;
                var url = tdata.UrlAddress();
                var numParticipants = tdata.Participants.Count;

                CsvWriter.WriteLine("tournament-list.csv", "{0}, {1}, {2}", name, url, numParticipants);

                var filename = string.Format("{0}-participants.csv", tdata.ApiUrl);
                CsvWriter.WriteLine(filename, "Tournament Name, Participant Name");

                foreach (var particiapant in tdata.Participants)
                {
                    var participantName = particiapant.Name;

                    CsvWriter.WriteLine(filename, "{0}, {1}", name, participantName);
                }

                filename = string.Format("{0}-results.csv", tdata.ApiUrl);
                CsvWriter.WriteLine(filename, "Tournament Name, Winner, Loser");

                foreach (var match in tdata.CompleteMatches)
                {
                    var p1name = match.Player1 == null ? "Bye" : match.Player1.Name;
                    var p2name = match.Player2 == null ? "Bye" : match.Player2.Name;

                    if (match.WinnerId == match.Player1Id)
                        CsvWriter.WriteLine(filename, "{0}, {1}, {2}", name, p1name, p2name);
                    else
                        CsvWriter.WriteLine(filename, "{0}, {1}, {2}", name, p2name, p1name);
                }
            }
        }

        private static void OldMain(string[] args)
        {
            TournamentKey = ConsoleInputProvider.GetTournamentKey();
            Console.WriteLine();
            var numStations = ConsoleInputProvider.GetNumberOfStations();
            Console.WriteLine();
            var stationsToExclude = ConsoleInputProvider.GetStationsToExclude();

            _tournament = ChallongeApiWrapper.GetTournament(TournamentKey, CHALLONGE_USERNAME, CHALLONGE_APIKEY);

            // Console Settings
            Console.Title = string.Format("Challonge Match Viewer - {0}", _tournament.Name);
            Console.WindowWidth = 100;
            //Console.WindowHeight = 84;
            Console.CursorVisible = false;

            // Wait for the tournament to start
            Console.WriteLine(string.Format("Waiting for the '{0}' tournament to start...", _tournament.Name));
            while (_tournament.OpenMatches.Count == 0)
            {
                System.Threading.Thread.Sleep(UPDATE_INTERVAL_SEC * 1000);

                _tournament = ChallongeApiWrapper.GetTournament(TournamentKey, CHALLONGE_USERNAME, CHALLONGE_APIKEY);
            }

            _tournament.StationManager.AddNewStations(numStations);
            foreach (var station in stationsToExclude)
            {
                _tournament.StationManager.RemoveStation(station);
                _tournament.StationManager.RemoveStation(string.Format("TV {0}", station));
            }

            // Create a timer to ping Challonge for updates to bracket
            var timer = new Timer(UPDATE_INTERVAL_SEC * 1000);
            timer.Elapsed += UpdateTournamentMatches;
            timer.Enabled = true;
            var startTime = DateTime.Now;

            UpdateTournamentMatches(null, null);

            while (_tournament.OpenMatches.Count != 0)
            {
                OutputMatchesToConsole(_tournament, startTime);

                System.Threading.Thread.Sleep(1000);
            }
        }

        private static void UpdateTournamentMatches(Object source, ElapsedEventArgs e)
        {
            var participants = ChallongeApiWrapper.GetParticipantList(TournamentKey, CHALLONGE_USERNAME, CHALLONGE_APIKEY);

            foreach (var participant in participants)
            {
                if (_tournament.Participants.Any(p => p.Id == participant.Id))
                {
                    var tp = _tournament.Participants.Single(p => p.Id == participant.Id);
                    if (tp.Name != participant.Name)
                        tp.Name = participant.Name;
                    if (tp.Email != participant.Email)
                        tp.Email = participant.Email;
                }
            }

            var matches = ChallongeApiWrapper.GetMatchList(TournamentKey, CHALLONGE_USERNAME, CHALLONGE_APIKEY);

            ProcessMatches(_tournament, matches);
            AssignStations(_tournament);
            //SendNotifications(_tournament);
        }

        public static void ProcessMatches(ChallongeTournament tournament, List<ChallongeMatch> matches)
        {
            // Assign player objects from the participants list to the matches retrieved from Challonge
            matches.ForEach(m => m.Player1 = tournament.Participants.DefaultIfEmpty(null).SingleOrDefault(p => p.Id == m.Player1Id));
            matches.ForEach(m => m.Player2 = tournament.Participants.DefaultIfEmpty(null).SingleOrDefault(p => p.Id == m.Player2Id));

            // Reset COMPLETE matches
            var newCompleteMatches = tournament.OpenMatches.Where(om => matches.Any(m => m.State == MatchType.Complete && m.Id == om.Id));
            //tournament.CompleteMatches.AddRange(newCompleteMatches);
            tournament.CompleteMatches = newCompleteMatches.ToList();

            // Remove from OPEN list any completed/pending Challonge matches.
            tournament.OpenMatches.RemoveAll(om => matches.Where(m => m.State != MatchType.Open).Any(m => m.Id == om.Id));

            // Remove from OPEN list matches that are not part of Challonge open matches.
            tournament.OpenMatches.RemoveAll(om => !matches.Where(m => m.State == MatchType.Open).Any(m => m.Id == om.Id));

            // Add new Challonge matches that are now OPEN and not already in the OPEN list.
            var newOpenMatches = matches.Where(m => m.State == MatchType.Open && !tournament.OpenMatches.Any(om => om.Id == m.Id));
            tournament.OpenMatches.AddRange(newOpenMatches);

            // Reset PENDING matches
            tournament.PendingMatches = matches.Where(m => m.State == MatchType.Pending 
                && (m.Player1Id != -1 || m.Player2Id != -1) // Only show matches where one participant is waiting.
                && tournament.OpenMatches.Any(om => om.Id == m.Player1PrevMatchId || om.Id == m.Player2PrevMatchId)).ToList();
        }

        public static void AssignStations(ChallongeTournament tournament)
        {
            // First free up stations that are not assign an OPEN match
            foreach (var match in tournament.StationManager.Stations.Values)
            {
                if (!tournament.OpenMatches.Any(m => m.Id == match.Id))
                {
                    tournament.StationManager.UnassignMatch(match);
                }
            }
            
            // Assign stations to open matches where available. Loser bracket matches are highest priority
            foreach (var match in tournament.OpenMatches.Where(m => string.IsNullOrEmpty(m.Station)).OrderBy(m => m.Round > 0 ? 99999 + m.Round : Math.Abs(m.Round)).ThenBy(m => m.CreatedAt))
            {
                var stationName = tournament.StationManager.AssignNextAvailable(match);
                match.Station = stationName;
                match.CreatedAt = DateTime.Now;
            }
        }

        // Delay this whole method.
        public static void SendNotifications(ChallongeTournament tournament)
        {
            foreach (var match in tournament.OpenMatches.Where(m => !m.SentNotification && !string.IsNullOrEmpty(m.Station)))
            {
                var title = "Your next match is ready!";
                var message = string.Format("{0} | {1}", match.ToString(), tournament.Name);

                if (!string.IsNullOrEmpty(match.Player1.Email))
                    PushbulletApiWrapper.PushToEmail(PUSHBULLET_APIKEY, match.Player1.Email, title, message);

                if (!string.IsNullOrEmpty(match.Player2.Email))
                    PushbulletApiWrapper.PushToEmail(PUSHBULLET_APIKEY, match.Player2.Email, title, message);

                match.SentNotification = true;
            }
        }

        public static void OutputMatchesToConsole(ChallongeTournament tournament, DateTime startTime)
        {
            if (tournament.OpenMatches.Count == 0)
                return;

            var maxNameLength = tournament.Participants.Max(p => p.Name.Length);
            var maxCodeLength = Math.Max(tournament.OpenMatches.Max(m => m.Identifier.Length), 2);
            var maxStationlength = tournament.OpenMatches.Max(m => m.Station == null ? 0 : m.Station.Length);

            Console.Clear();
            Console.WriteLine(string.Format("View the bracket on your phone here: {0}", tournament.UrlAddress()));

            var swappedPlayers = tournament.OpenMatches.Select(m => new ChallongeMatch
            {
                Station = m.Station,
                Player1 = m.Player2,
                Player2 = m.Player1,
                Identifier = m.Identifier,
                Round = m.Round,
                CreatedAt = m.CreatedAt
            });

            var header = string.Format("{0} | {1} | Match ID | Bracket Location",
                "TV #".PadRight(maxStationlength),
                "Who plays who".PadRight(maxNameLength * 2 + 5)
            );
            //Console.WriteLine(header);
            Console.WriteLine(string.Empty.PadRight(header.Length + 10, '-'));

            foreach (var match in tournament.OpenMatches.Concat(swappedPlayers).OrderBy(m => m.Player1.Name))
            {
                // Don't output matches that don't have TVs
                if (string.IsNullOrEmpty(match.Station))
                {
                    if (tournament.OpenMatches.Contains(match) && !tournament.PendingMatches.Contains(match))
                        tournament.PendingMatches.Add(match);

                    continue;
                }

                Console.Write(string.Format("{0} | {1} <-> {2} | Match {3} | {4} | ",
                    (match.Station == null ? string.Empty : match.Station).PadRight(maxStationlength),
                    match.Player1.Name.PadRight(maxNameLength),
                    match.Player2.Name.PadRight(maxNameLength),
                    match.Identifier.PadRight(maxCodeLength),
                    tournament.RoundLabel(match).PadRight(18)
                ));

                if (DateTime.Now.Second % 2 == 0)
                {
                    if (match.CreatedAt.AddMinutes(30) <= DateTime.Now)
                    {
                        //Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                    }
                    else if (match.CreatedAt.AddMinutes(15) <= DateTime.Now)
                    {
                        //Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                    }
                }

                if (!string.IsNullOrEmpty(match.Station))
                    Console.WriteLine((DateTime.Now - match.CreatedAt).ToString("mm':'ss").PadLeft(5));
                else
                    Console.WriteLine(string.Empty.PadRight(5));

                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
            }
            Console.WriteLine();

            Console.WriteLine("Upcoming Matches");
            Console.WriteLine(string.Empty.PadRight(header.Length + 10, '-'));
            foreach (var match in tournament.PendingMatches.OrderBy(m => m.Player1 != null && m.Player2 != null ? "1" : "2").ThenBy(m => m.Player1 != null ? m.Player1.Name : m.Player2.Name))
            {
                if (match.Player1 != null && match.Player2 != null)
                {
                    Console.WriteLine(string.Format("{0} <-> {1}",
                        match.Player1.Name.PadRight(maxNameLength),
                        match.Player2.Name.PadRight(maxNameLength)
                    ));
                }
                else
                {
                    var player = match.Player1 != null ? match.Player1 : match.Player2;
                    var matchWaitingOn = tournament.OpenMatches.Single(c => c.Id == (match.Player1 != null ? match.Player2PrevMatchId : match.Player1PrevMatchId));
                    var winnerOrLoser = match.Round < 0 && matchWaitingOn.Round > 0 ? "Loser" : "Winner";


                    Console.WriteLine(string.Format("{0} <-> {1} of Match {2}",
                        player.Name.PadRight(maxNameLength),
                        winnerOrLoser,
                        matchWaitingOn.Identifier));
                }

            }
            Console.WriteLine();

            var timeDiff = UPDATE_INTERVAL_SEC - (DateTime.Now - startTime).Seconds % UPDATE_INTERVAL_SEC;
            Console.WriteLine(string.Format("Next update in... {0} second{1}.", timeDiff.ToString().PadLeft(2, '0'), timeDiff == 1 ? string.Empty : "s"));
        }
    }
}
