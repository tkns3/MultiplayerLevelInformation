using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static StandardScoreSyncState;

namespace MultiplayerLevelInformation.APIs
{
    internal static class ScoreSaber
    {
        static HttpClient _client = null;

        public static async Task<LeaderboardInfo> GetLeaderboard(string hash, MapDifficulty mapDifficulty, MapMode mapMode)
        {
            if (_client == null)
            {
                _client = new HttpClient();
            }

            int difficulty;
            switch (mapDifficulty)
            {
                case MapDifficulty.Easy: difficulty = 1; break;
                case MapDifficulty.Normal: difficulty = 3; break;
                case MapDifficulty.Hard: difficulty = 5; break;
                case MapDifficulty.Expert: difficulty = 7; break;
                case MapDifficulty.ExpertPlus: difficulty = 9; break;
                default: difficulty = 1; break;
            };
            string mode;
            switch (mapMode)
            {
                case MapMode.Standard: mode = "SoloStandard"; break;
                case MapMode.OneSaber: mode = "SoloOneSaber"; break;
                case MapMode.NoArrows: mode = "SoloNoArrows"; break;
                case MapMode.Degree90: mode = "Solo90Degree"; break;
                case MapMode.Degree360: mode = "Solo360Degree"; break;
                case MapMode.Lightshow: mode = "SoloLightshow"; break;
                case MapMode.Lawless: mode = "SoloLawless"; break;
                default: mode = "SoloStandard"; break;
            };
            string url = $"https://scoresaber.com/api/leaderboard/by-hash/{hash}/info?difficulty={difficulty}&mode={mode}";

            Plugin.Log.Info(url);
            var res = await _client.GetStringAsync(url);
            var info = JsonConvert.DeserializeObject<APIs.ScoreSaber.LeaderboardInfo>(res);
            return info;
        }

        public class LeaderboardInfo
        {
            public long id { get; set; }
            public string songHash
            {
                get
                {
                    return _songHash;
                }
                set
                {
                    _songHash = value.ToLower();
                }
            }
            public string songName { get; set; } = "";
            public string songSubName { get; set; } = "";
            public string songAuthorName { get; set; } = "";
            public string levelAuthorName { get; set; } = "";
            public DifficultyInfo difficulty { get; set; } = new DifficultyInfo();
            public long maxScore { get; set; }
            public DateTime createdDate { get; set; }
            public DateTime? rankedDate { get; set; }
            public DateTime? qualifiedDate { get; set; }
            public DateTime? lovedDate { get; set; }
            public bool ranked { get; set; }
            public bool qualified { get; set; }
            public bool loved { get; set; }
            public long maxPP { get; set; }
            public double stars { get; set; }
            public bool positiveModifiers { get; set; }
            public long plays { get; set; }
            public long dailyPlays { get; set; }
            public string coverImage { get; set; } = "";
            public Score? playerScore { get; set; }
            public List<DifficultyInfo> difficulties { get; set; } = new List<DifficultyInfo>();

            private string _songHash = "";
        }

        public class DifficultyInfo
        {
            public long leaderboardId { get; set; }
            public long difficulty
            {
                get
                {
                    return _difficulty;
                }
                set
                {
                    _difficulty = value;
                    switch (value)
                    {
                        case 1: mapDifficulty = MapDifficulty.Easy; break;
                        case 3: mapDifficulty = MapDifficulty.Normal; break;
                        case 5: mapDifficulty = MapDifficulty.Hard; break;
                        case 7: mapDifficulty = MapDifficulty.Expert; break;
                        case 9: mapDifficulty = MapDifficulty.ExpertPlus; break;
                        default: mapDifficulty = MapDifficulty.Unknown; break;
                    }
                }
            }
            public string gameMode
            {
                get
                {
                    return _gameMode;
                }
                set
                {
                    _gameMode = value;
                    switch (value)
                    {
                        case "SoloStandard": mapMode = MapMode.Standard; break;
                        case "SoloOneSaber": mapMode = MapMode.OneSaber; break;
                        case "SoloNoArrows": mapMode = MapMode.NoArrows; break;
                        case "SoloLightshow": mapMode = MapMode.Lightshow; break;
                        case "Solo90Degree": mapMode = MapMode.Degree90; break;
                        case "Solo360Degree": mapMode = MapMode.Degree360; break;
                        case "SoloLawless": mapMode = MapMode.Lawless; break;
                        default: mapMode = MapMode.Unknown; break;
                    }
                }
            }
            public string difficultyRaw { get; set; } = "";

            private string _gameMode = "";
            private long _difficulty;

            internal MapMode mapMode;
            internal MapDifficulty mapDifficulty;
        }
    }
}
