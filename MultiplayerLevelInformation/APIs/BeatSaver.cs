using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Policy;
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
            public MapDifficulty[] diffs { get; set; }
        }

        public class MapDifficulty
        {
            public int bombs { get; set; }
            public string characteristic { get; set; }
            public string difficulty { get; set; }
            public double njs { get; set; }
            public int notes { get; set; }
            public double nps { get; set; }
            public int obstacles { get; set; }
            public double offset { get; set; }
            public ParitySummary paritySummary { get; set; }
            public double stars { get; set; }
            public string label { get; set; } = "";
        }

        public class ParitySummary
        {
            public int errors { get; set; }
            public int warns { get; set; }
            public int resets { get; set; }
        }
    }
}
