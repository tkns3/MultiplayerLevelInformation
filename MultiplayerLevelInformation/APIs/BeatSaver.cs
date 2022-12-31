using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace MultiplayerLevelInformation.APIs
{
    internal static class BeatSaver
    {
        static HttpClient _client = null;

        public static async Task<APIs.BeatSaver.MapDetail> GetMapDetail(string hash)
        {
            if (_client == null)
            {
                _client = new HttpClient();
            }

            var res = await _client.GetStringAsync($"https://api.beatsaver.com/maps/hash/{hash}");
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
            public MapDifficulty[] diffs { get; set; }
        }

        public class MapDifficulty
        {
            public int bombs { get; set; }
            public string characteristic { get; set; }
            public string difficulty { get; set; }
            public float njs { get; set; }
            public int notes { get; set; }
            public double nps { get; set; }
            public int obstacles { get; set; }
            public float offset { get; set; }
        }
    }
}
