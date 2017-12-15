using System.Collections.Generic;
using ColdOBot.Osu;
using Newtonsoft.Json;

namespace ColdOBot.LookAtMyScore
{
    public struct MultiScore
    {
        [JsonProperty("result")] public string Result;

        [JsonProperty("data")] public MultiScoreData Data;
    }

    public struct MultiScoreData
    {
        [JsonProperty("beatmap")] public Beatmap Beatmap;

        public Mode Gamemode => Beatmap.Gamemode;

        [JsonProperty("scores")] public List<Score> Scores;

        [JsonProperty("textData")] public List<List<string>> TextData;
    }
}