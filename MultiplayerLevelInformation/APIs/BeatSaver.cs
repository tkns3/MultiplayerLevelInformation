using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MultiplayerLevelInformation.APIs
{
    public static class BeatSaver
    {
        static HttpClient _client = null;

        public static async Task<APIs.BeatSaver.MapDetail> GetMapDetail(string hash)
        {
            if (_client == null)
            {
                _client = new HttpClient();
            }

            var url = $"https://api.beatsaver.com/maps/hash/{hash}";
            Plugin.Log.Info(url);
            var res = await _client.GetStringAsync(url);
            var map = JsonConvert.DeserializeObject<APIs.BeatSaver.MapDetail>(res);
            return map;
        }

        public class MapDetail
        {
            public string id { get; set; }
            public MapDetailMetadata metadata { get; set; }
            public MapVersion[] versions { get; set; }


            public APIs.BeatSaver.MapDifficultyInfo GetDifficulty(MapDifficulty mapDifficulty, MapMode mapMode)
            {
                try
                {
                    return versions[0].diffs.Single(d => d.mapDifficulty == mapDifficulty && d.mapMode == mapMode);
                }
                catch (Exception ex)
                {
                    if (versions.Length > 0)
                    {
                        Plugin.Log.Debug($"BeatSaver.MapDetail is invalid. {versions[0].hash}/{mapDifficulty}/{mapMode} is not found. {ex}");
                    }
                    throw new Exception($"Failed to get map data from Beat Server.");
                }
            }

        }

        public class MapDetailMetadata
        {
            public float bpm { get; set; }
            public int duration { get; set; }
            public string levelAuthorName { get; set; }
            public string songAuthorName { get; set; }
            public string songName { get; set; }
            public string songSubName { get; set; }
        }

        public class MapVersion
        {
            public string hash { get; set; }
            public string coverURL { get; set; }
            public MapDifficultyInfo[] diffs { get; set; }
        }

        public class MapDifficultyInfo
        {
            public int bombs { get; set; }
            public string characteristic
            {
                get
                {
                    return _characteristic;
                }
                set
                {
                    _characteristic = value;
                    switch (value)
                    {
                        case "Standard": mapMode = MapMode.Standard; break;
                        case "OneSaber": mapMode = MapMode.OneSaber; break;
                        case "NoArrows": mapMode = MapMode.NoArrows; break;
                        case "Lightshow": mapMode = MapMode.Lightshow; break;
                        case "90Degree": mapMode = MapMode.Degree90; break;
                        case "360Degree": mapMode = MapMode.Degree360; break;
                        case "Lawless": mapMode = MapMode.Lawless; break;
                        default: mapMode = MapMode.Unknown; break;
                    }
                }
            }
            public string difficulty
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
                        case "Easy": mapDifficulty = APIs.MapDifficulty.Easy; break;
                        case "Normal": mapDifficulty = APIs.MapDifficulty.Normal; break;
                        case "Hard": mapDifficulty = APIs.MapDifficulty.Hard; break;
                        case "Expert": mapDifficulty = APIs.MapDifficulty.Expert; break;
                        case "ExpertPlus": mapDifficulty = APIs.MapDifficulty.ExpertPlus; break;
                        default: mapDifficulty = APIs.MapDifficulty.Unknown; break;
                    }
                }
            }
            public double njs { get; set; }
            public int notes { get; set; }
            public double nps { get; set; }
            public int obstacles { get; set; }
            public double offset { get; set; }
            public ParitySummary paritySummary { get; set; }
            public double stars { get; set; }
            public string label { get; set; } = "";

            private string _characteristic = "";
            private string _difficulty = "";

            internal MapMode mapMode;
            internal APIs.MapDifficulty mapDifficulty;

        }

        public class ParitySummary
        {
            public int errors { get; set; }
            public int warns { get; set; }
            public int resets { get; set; }
        }
    }
}
