using Newtonsoft.Json;

namespace ColdOBot.LookAtMyScore
{
    public struct Image
    {
        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("image")]
        public ImageData Data;
    }

    public struct ImageData
    {
        [JsonProperty("id")]
        public string ID;

        [JsonProperty("url")]
        public string Url;
    }
}