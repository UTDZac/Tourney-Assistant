using ChallongeMatchViewer.Objects;
using ChallongeMatchViewer.Providers.Data;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChallongeMatchViewer.Providers
{
    public class ConsoleInputProvider
    {
        public static string GetTournamentKey()
        {
            Console.WriteLine("Example tournament key for http://tloc.challonge.com/iab12 is 'tloc-iab12'.");
            Console.Write("Enter the Challonge tournament key for your tournament: ");
            
            return Console.ReadLine().Trim();
        }

        public static int GetNumberOfStations()
        {
            Console.Write("How many stations (TVs) will be auto-generated: ");
            int numStations = -1;
            int.TryParse(Console.ReadLine(), out numStations);

            if (numStations < -1)
            {
                Console.WriteLine("[Error] Please enter 0 or more stations.");
                return GetNumberOfStations();
            }

            return numStations;
        }

        public static IEnumerable<string> GetStationsToExclude()
        {
            Console.Write("Will you need to exclude any stations from being auto-assigned matches (y/n)? ");
            var doExclude = Console.ReadLine();

            if (doExclude.Trim().ToLower() != "y")
                return Enumerable.Empty<string>();

            Console.WriteLine("Enter each station to exclude individually. Enter 'done' to finish.");
            var stationsToExclude = new List<string>();
            while(true)
            {
                var input = Console.ReadLine().Trim().ToLower();
                if (input == "done")
                    break;

                stationsToExclude.Add(input);
            }

            return stationsToExclude.AsEnumerable<string>();
        }
    }
}