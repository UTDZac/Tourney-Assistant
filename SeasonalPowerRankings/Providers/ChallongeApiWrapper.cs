using ChallongeMatchViewer.Objects;
using ChallongeMatchViewer.Providers.Data;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChallongeMatchViewer.Providers
{
    public class ChallongeApiWrapper
    {
        public const string CHALLONGE_API_URL = "https://api.challonge.com/v1/";

        /// <summary>Retrieve a set of tournaments created with the specified account.</summary>
        /// <param name="username">The Challonge username to retrieve the data.</param>
        /// <param name="apikey">The Challonge API key, generated from https://challonge.com/settings/developer. </param>
        public static List<ChallongeTournament> GetTournamentList(string username, string apikey)
        {
            return GetTournamentList(username, apikey, null);
        }

        /// <summary>Retrieve a set of tournaments created with the specified account.</summary>
        /// <param name="username">The Challonge username to retrieve the data.</param>
        /// <param name="apikey">The Challonge API key, generated from https://challonge.com/settings/developer. </param>
        /// <param name="subdomain">The subdomain parameter is required to retrieve a list of organization-hosted tournaments.</param>
        public static List<ChallongeTournament> GetTournamentList(string username, string apikey, string subdomain)
        {
            var requestUrl = "tournaments";

            var client = new RestClient(CHALLONGE_API_URL);
            client.Authenticator = new HttpBasicAuthenticator(username, apikey);

            var request = new RestRequest(requestUrl, Method.GET);

            if (!string.IsNullOrEmpty(subdomain))
                request.AddParameter("subdomain", subdomain);

            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            var result = JsonConvert.DeserializeObject<TournamentRoot[]>(response.Content);

            return result.Select(t => t.tournament).Select(t => new ChallongeTournament
            {
                Name = t.name,
                ApiUrl = string.IsNullOrEmpty(t.subdomain) ? t.url : t.subdomain + "-" + t.url,
                State = t.state
            }).ToList();
        }

        /// <summary>Retrieve a single tournament created with the specified account.</summary>
        /// <param name="tournamentKey">The ID or URL string that identifies the tournament. In the form "SUBDOMAIN-URLKEY"</param>
        /// <param name="username">The Challonge username to retrieve the data.</param>
        /// <param name="apikey">The Challonge API key, generated from https://challonge.com/settings/developer. </param>
        public static ChallongeTournament GetTournament(string tournamentKey, string username, string apikey)
        {
            var requestUrl = string.Format("tournaments/{0}", tournamentKey);

            var client = new RestClient(CHALLONGE_API_URL);
            client.Authenticator = new HttpBasicAuthenticator(username, apikey);

            var request = new RestRequest(requestUrl, Method.GET);
            request.AddParameter("include_participants", "1");
            request.AddParameter("include_matches", "1");

            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            var result = JsonConvert.DeserializeObject<TournamentRoot>(response.Content);
            var data = result.tournament;

            var tournament = new ChallongeTournament
            {
                Name = data.name,
                ApiUrl = string.IsNullOrEmpty(data.subdomain) ? data.url : data.subdomain + "-" + data.url,
                MaxRound = data.matches.DefaultIfEmpty(new MatchRoot { match = new Match { round = 0 } }).Max(m => m.match.round),
                MinRound = data.matches.DefaultIfEmpty(new MatchRoot { match = new Match { round = 0 } }).Min(m => m.match.round)
            };

            tournament.Participants = data.participants.Select(p => new ChallongeParticipant
            {
                Id = p.participant.id,
                Name = p.participant.name,
                Email = p.participant.misc != null ? p.participant.misc.ToString() : string.Empty
            }).ToList();

            tournament.OpenMatches = data.matches.Select(m => m.match)
                .Where(m => m.state == "open")
                .Select(m => new ChallongeMatch
                {
                    Id = m.id,
                    State = MatchType.Open,
                    Round = m.round,
                    Identifier = m.identifier,
                    Player1Id = m.player1_id ?? -1,
                    Player2Id = m.player2_id ?? -1,
                    Player1PrevMatchId = m.player1_prereq_match_id ?? -1,
                    Player2PrevMatchId = m.player2_prereq_match_id ?? -1,
                    Player1 = tournament.Participants.DefaultIfEmpty(null).SingleOrDefault(p => p.Id == m.player1_id),
                    Player2 = tournament.Participants.DefaultIfEmpty(null).SingleOrDefault(p => p.Id == m.player2_id),
                    CreatedAt = DateTime.Now,
                    Station = string.Empty
                }).ToList();

            tournament.PendingMatches = data.matches.Select(m => m.match)
                .Where(m => m.state == "pending" && (m.player1_id != null || m.player2_id != null))
                .Select(m => new ChallongeMatch
                {
                    Id = m.id,
                    State = MatchType.Pending,
                    Round = m.round,
                    Identifier = m.identifier,
                    Player1Id = m.player1_id ?? -1,
                    Player2Id = m.player2_id ?? -1,
                    Player1PrevMatchId = m.player1_prereq_match_id ?? -1,
                    Player2PrevMatchId = m.player2_prereq_match_id ?? -1,
                    Player1 = tournament.Participants.DefaultIfEmpty(null).SingleOrDefault(p => p.Id == m.player1_id),
                    Player2 = tournament.Participants.DefaultIfEmpty(null).SingleOrDefault(p => p.Id == m.player2_id),
                    CreatedAt = DateTime.Now,
                    Station = string.Empty
                }).ToList();

            tournament.CompleteMatches = data.matches.Select(m => m.match)
                .Where(m => m.state == "complete")
                .Select(m => new ChallongeMatch
                {
                    Id = m.id,
                    State = MatchType.Complete,
                    Round = m.round,
                    Identifier = m.identifier,
                    Player1Id = m.player1_id ?? -1,
                    Player2Id = m.player2_id ?? -1,
                    Player1PrevMatchId = m.player1_prereq_match_id ?? -1,
                    Player2PrevMatchId = m.player2_prereq_match_id ?? -1,
                    WinnerId = m.winner_id ?? -1,
                    LoserId = m.loser_id ?? -1,
                    Player1 = tournament.Participants.DefaultIfEmpty(null).SingleOrDefault(p => p.Id == m.player1_id),
                    Player2 = tournament.Participants.DefaultIfEmpty(null).SingleOrDefault(p => p.Id == m.player2_id),
                    Station = string.Empty
                }).ToList();

            return tournament;
        }

        /// <summary>Retrieve a tournament's participant list.</summary>
        /// <param name="tournamentKey">The ID or URL string that identifies the tournament. In the form "SUBDOMAIN-URLKEY"</param>
        /// <param name="username">The Challonge username to retrieve the data.</param>
        /// <param name="apikey">The Challonge API key, generated from https://challonge.com/settings/developer. </param>
        public static List<ChallongeParticipant> GetParticipantList(string tournamentKey, string username, string apikey)
        {
            var requestUrl = string.Format("tournaments/{0}/participants", tournamentKey);

            var client = new RestClient(CHALLONGE_API_URL);
            client.Authenticator = new HttpBasicAuthenticator(username, apikey);

            var request = new RestRequest(requestUrl, Method.GET);
            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            var result = JsonConvert.DeserializeObject<ParticipantRoot[]>(response.Content);

            return result.Select(p => p.participant).Select(p => new ChallongeParticipant
            {
                Id = p.id,
                Name = p.name,
                Email = p.misc != null ? p.misc.ToString() : string.Empty
            }).ToList();
        }

        /// <summary>Retrieve a tournament's participant list.</summary>
        /// <param name="tournamentKey">The ID or URL string that identifies the tournament. In the form "SUBDOMAIN-URLKEY"</param>
        /// <param name="username">The Challonge username to retrieve the data.</param>
        /// <param name="apikey">The Challonge API key, generated from https://challonge.com/settings/developer. </param>
        /// <param name="participants">List of participants to add (name & email).</param>
        public static void AddParticipant(string tournamentKey, string username, string apikey, ChallongeParticipant participant)
        {
            if (participant == null || string.IsNullOrEmpty(participant.Name))
                return;

            var requestUrl = string.Format("tournaments/{0}/participants", tournamentKey);

            var client = new RestClient(CHALLONGE_API_URL);
            client.Authenticator = new HttpBasicAuthenticator(username, apikey);

            var request = new RestRequest(requestUrl, Method.POST);
            request.AddParameter("participant[name]", participant.Name);

            if (!string.IsNullOrEmpty(participant.Email))
                request.AddParameter("participant[misc]", participant.Email);

            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine(string.Format("- Added: {0}", participant.Name));
            }
            else
            {
                Console.WriteLine(string.Format("- Failed to add participant: {0}", participant.Name));
            }
        }

        /// <summary>Retrieve a tournament's participant list.</summary>
        /// <param name="tournamentKey">The ID or URL string that identifies the tournament. In the form "SUBDOMAIN-URLKEY"</param>
        /// <param name="username">The Challonge username to retrieve the data.</param>
        /// <param name="apikey">The Challonge API key, generated from https://challonge.com/settings/developer. </param>
        /// <param name="participants">List of participants to add (name & email).</param>
        public static void BulkAddParticipants(string tournamentKey, string username, string apikey, List<ChallongeParticipant> participants)
        {
            var requestUrl = string.Format("tournaments/{0}/participants", tournamentKey);

            var client = new RestClient(CHALLONGE_API_URL);
            client.Authenticator = new HttpBasicAuthenticator(username, apikey);

            foreach (var participant in participants)
            {
                var request = new RestRequest(requestUrl, Method.POST);
                request.AddParameter("participant[name]", participant.Name);

                if (!string.IsNullOrEmpty(participant.Email))
                    request.AddParameter("participant[misc]", participant.Email);

                var response = client.Execute(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine(string.Format("- Added: {0}", participant.Name));
                }
                else
                {
                    Console.WriteLine(string.Format("- Failed to add participant: {0}", participant.Name));
                    continue;
                }
            }
        }

        /// <summary>Retrieve a tournament's match list.</summary>
        /// <param name="tournamentKey">The ID or URL string that identifies the tournament. In the form "SUBDOMAIN-URLKEY"</param>
        /// <param name="username">The Challonge username to retrieve the data.</param>
        /// <param name="apikey">The Challonge API key, generated from https://challonge.com/settings/developer. </param>
        public static List<ChallongeMatch> GetMatchList(string tournamentKey, string username, string apikey)
        {
            var requestUrl = string.Format("tournaments/{0}/matches", tournamentKey);

            var client = new RestClient(CHALLONGE_API_URL);
            client.Authenticator = new HttpBasicAuthenticator(username, apikey);

            var request = new RestRequest(requestUrl, Method.GET);
            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            var result = JsonConvert.DeserializeObject<MatchRoot[]>(response.Content);

            return result.Select(m => m.match).Select(m => new ChallongeMatch
            {
                Id = m.id,
                State = m.state == "complete" ? MatchType.Complete : m.state == "open" ? MatchType.Open : MatchType.Pending,
                Round = m.round,
                Identifier = m.identifier,
                Player1Id = m.player1_id ?? -1,
                Player2Id = m.player2_id ?? -1,
                Player1PrevMatchId = m.player1_prereq_match_id ?? -1,
                Player2PrevMatchId = m.player2_prereq_match_id ?? -1,
                WinnerId = m.winner_id ?? -1,
                LoserId = m.loser_id ?? -1,
                CreatedAt = DateTime.Now,
                Station = string.Empty
            }).ToList();
        }

        /// <summary>Update/submit the score(s) for a match.</summary>
        /// <param name="tournamentKey">The ID or URL string that identifies the tournament. In the form "SUBDOMAIN-URLKEY"</param>
        /// <param name="username">The Challonge username to retrieve the data.</param>
        /// <param name="apikey">The Challonge API key, generated from https://challonge.com/settings/developer. </param>
        /// <param name="match">The <see cref="ChallongeMatch" /> object to update.</param>
        public static void UpdateMatch(string tournamentKey, string username, string apikey, ChallongeMatch match)
        {
            var requestUrl = string.Format("tournaments/{0}/matches/{1}", tournamentKey, match.Id);

            var client = new RestClient(CHALLONGE_API_URL);
            client.Authenticator = new HttpBasicAuthenticator(username, apikey);

            var request = new RestRequest(requestUrl, Method.PUT);
            request.AddParameter("match[scores_csv]", match.WinnerId == match.Player1Id ? "1-0" : "0-1"); // scores_csv is required
            request.AddParameter("match[winner_id]", match.WinnerId.ToString());

            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return;
        }

        /// <summary>Update/submit the score(s) for a match.</summary>
        /// <param name="tournamentKey">The ID or URL string that identifies the tournament. In the form "SUBDOMAIN-URLKEY"</param>
        /// <param name="username">The Challonge username to retrieve the data.</param>
        /// <param name="apikey">The Challonge API key, generated from https://challonge.com/settings/developer. </param>
        /// <param name="match">The <see cref="ChallongeMatch" /> object to clear result.</param>
        public static void ClearMatch(string tournamentKey, string username, string apikey, ChallongeMatch match)
        {
            var requestUrl = string.Format("tournaments/{0}/matches/{1}", tournamentKey, match.Id);

            var client = new RestClient(CHALLONGE_API_URL);
            client.Authenticator = new HttpBasicAuthenticator(username, apikey);

            var request = new RestRequest(requestUrl, Method.PUT);
            request.AddParameter("match[scores_csv]", null); // scores_csv is required
            request.AddParameter("match[winner_id]", null);

            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return;
        }
    }
}