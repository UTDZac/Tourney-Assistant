using ChallongeMatchViewer.Objects;
using ChallongeMatchViewer.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChallongeTournamentResgitration
{
    class Program
    {
        public const string CHALLONGE_TOURNAMENT_KEY = "tloc-iab28";
        public const string CHALLONGE_USERNAME = "utdzac";
        public const string CHALLONGE_APIKEY = "MXMDaSnkM0utYij30bme02hqZ8eR76ccRi8vA4W4";
        public const string PUSHBULLET_APIKEY = "vqEzlziinVtS8fSEWByes0yuqGP3NIKg";
        public const string PARTICIPANTS_FILEPATH = "ChallongeParticipants.txt";
        public const string NEW_PARTICIPANTS_FILEPATH = "NewChallongeParticipants.txt";

        static void Main(string[] args)
        {
            Console.WriteLine(string.Format("Reading in participants from file: {0}", NEW_PARTICIPANTS_FILEPATH));
            var newParticipants = GetParticipantsFromFile(NEW_PARTICIPANTS_FILEPATH);
            Console.WriteLine();

            //Console.WriteLine(string.Format("Reading in participants from file: {0}", PARTICIPANTS_FILEPATH));
            //var existingParticipants = GetParticipantsFromFile(PARTICIPANTS_FILEPATH);
            //Console.WriteLine();

            //var mergedParticipants = newParticipants.Concat(existingParticipants.Where(np => !newParticipants.Any(ep => ep.Name == np.Name))).ToList();

            //Console.WriteLine(string.Format("Updating participants in file: {0}", PARTICIPANTS_FILEPATH));
            //UpdateParticipantFile(PARTICIPANTS_FILEPATH, mergedParticipants);
            //Console.WriteLine();

            Console.WriteLine(string.Format("Adding participants to tournament: {0}", CHALLONGE_TOURNAMENT_KEY));
            ChallongeApiWrapper.BulkAddParticipants(CHALLONGE_TOURNAMENT_KEY, CHALLONGE_USERNAME, CHALLONGE_APIKEY, newParticipants);
            //ChallongeApiWrapper.BulkAddParticipants(CHALLONGE_TOURNAMENT_KEY, CHALLONGE_USERNAME, CHALLONGE_APIKEY, mergedParticipants);
            Console.WriteLine();

            Console.WriteLine("Processing complete. Press any key to exit...");
            Console.ReadKey();
        }

        public static List<ChallongeParticipant> GetParticipantsFromFile(string filepath)
        {
            var participants = new List<ChallongeParticipant>();

            using (StreamReader reader = File.OpenText(filepath))
            {
                var line = String.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line) || line == "Username, Email")
                        continue;

                    var split = line.Split(',');

                    if (split.Length == 1)
                    {
                        participants.Add(new ChallongeParticipant { Name = split[0].Trim() });
                    }
                    else
                    {
                        var email = split.DefaultIfEmpty(string.Empty).LastOrDefault();
                        var username = split.Except(new[] { email }).Aggregate((s1, s2) => s1 + s2);

                        participants.Add(new ChallongeParticipant { Name = username.Trim(), Email = email.Trim() });
                    }
                }
            }

            return participants;
        }

        public static void UpdateParticipantFile(string filepath, List<ChallongeParticipant> participants)
        {
            using (StreamWriter writer = new StreamWriter(filepath))
            {
                writer.WriteLine("Username, Email");

                foreach (var participant in participants.OrderBy(p => p.Name))
                {
                    if (string.IsNullOrEmpty(participant.Email))
                        writer.WriteLine(participant.Name);
                    else
                        writer.WriteLine(string.Format("{0}, {1}", participant.Name, participant.Email));
                }
            }
        }
    }
}
