using System;
using System.Collections.Generic;
using System.Linq;

namespace ChallongeMatchViewer.Objects
{
    public class StationManager
    {
        public StationManager()
        {
            ExcludeStreamTVsFromAutoAssign = false;
            Stations = new Dictionary<string, ChallongeMatch>();
        }

        public bool ExcludeStreamTVsFromAutoAssign { get; set; }
        public Dictionary<string, ChallongeMatch> Stations { get; set; }

        public bool AddStation(string name)
        {
            if (Stations.ContainsKey(name))
                return false;

            Stations.Add(name, null);
            return true;
        }

        public bool RemoveStation(string name)
        {
            return Stations.Remove(name);
        }

        public void AddNewStations(int num)
        {
            int tvIndex = 1;
            int numAdded = 0;

            while (numAdded < num)
            {
                if (AddStation(string.Format("TV {0}", tvIndex++)))
                    numAdded++;
            }
        }

        public bool AssignMatch(string stationName, ChallongeMatch match)
        {
            if (!Stations.ContainsKey(stationName) || Stations.ContainsValue(match) || Stations[stationName] != null)
                return false;

            Stations[stationName] = match;
            return true;
        }

        public bool UnassignMatch(string stationName)
        {
            if (!Stations.ContainsKey(stationName))
                return false;

            Stations[stationName] = null;
            return true;
        }

        public string UnassignMatch(ChallongeMatch match)
        {
            if (!Stations.ContainsValue(match))
                return null;

            var stationName = Stations.Single(s => s.Value == match).Key;

            if (UnassignMatch(stationName))
                return stationName;
            else
                return null;
        }

        public string AssignNextAvailable(ChallongeMatch match)
        {
            if (Stations.ContainsValue(match) || !Stations.Values.Any(v => v == null))
                return null;

            string stationName;
            if (ExcludeStreamTVsFromAutoAssign)
                stationName = Stations.First(s => s.Value == null && !s.Key.ToLower().Contains("stream")).Key;
            else
                stationName = Stations.First(s => s.Value == null).Key;

            if (AssignMatch(stationName, match))
                return stationName;
            else
                return null;
        }
    }
}