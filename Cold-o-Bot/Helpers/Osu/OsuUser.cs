using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace ColdOBot.Helpers.Osu
{
    public struct OsuUser
    {
#pragma warning disable IDE1006
        [JsonProperty("user_id")]
        private string user_id { set => int.TryParse(value, out ID); }

        [JsonIgnore]
        public int ID;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("count300")]
        private string count300 { set => int.TryParse(value, out Hit300);}

        [JsonIgnore]
        public int Hit300;

        [JsonProperty("count100")]
        private string count100 { set => int.TryParse(value, out Hit100); }

        [JsonIgnore]
        public int Hit100;

        [JsonProperty("count50")]
        private string count50 { set => int.TryParse(value, out Hit50); }

        [JsonIgnore]
        public int Hit50;

        [JsonProperty("playcount")]
        private string playcount { set => int.TryParse(value, out PlayCount); }

        [JsonIgnore]
        public int PlayCount;

        [JsonProperty("ranked_score")]
        private string ranked_score { set => long.TryParse(value, out RankedScore); }

        [JsonIgnore]
        public long RankedScore;

        [JsonProperty("total_score")]
        private string total_score { set => long.TryParse(value, out TotalScore); }

        [JsonIgnore]
        public long TotalScore;
        
        [JsonProperty("pprank")]
        private string pprank { set => int.TryParse(value, out Rank); }

        [JsonIgnore]
        public int Rank;

        [JsonProperty("level")]
        private string level { set => float.TryParse(value, out Level); }

        [JsonIgnore]
        public float Level;

        [JsonProperty("pp_raw")]
        private string pp_raw { set => float.TryParse(value, out PP); }  

        [JsonIgnore]
        public float PP;

        [JsonProperty("accuracy")]
        private string accuracy { set => double.TryParse(value, out Accuracy); }

        [JsonIgnore]
        public double Accuracy;

        [JsonProperty("count_rank_ss")]
        private string count_rank_ss { set => int.TryParse(value, out SSRankings); }

        [JsonIgnore]
        public int SSRankings;

        [JsonProperty("count_rank_s")]
        private string count_rank_s { set => int.TryParse(value, out SRankings); }

        [JsonIgnore]
        public int SRankings;

        [JsonProperty("count_rank_a")]
        private string count_rank_a { set => int.TryParse(value, out ARankings); }

        [JsonIgnore]
        public int ARankings;

        [JsonProperty("country")]
        public string Country;

        [JsonProperty("pp_country_rank")]
        private string pp_country_rank { set => int.TryParse(value, out CountryRank); }

        [JsonIgnore]
        public int CountryRank;

        [JsonProperty("events")]
        private string events { set => Events = JsonConvert.DeserializeObject<List<OsuEvent>>(value); }

        [JsonIgnore]
        public List<OsuEvent> Events;
    }

    public struct OsuEvent
    {
        [JsonProperty("display_html")]
        public string DisplayHtml;

        [JsonProperty("beamtap_id")]
        private string beatmap_id { set => int.TryParse(value, out BeatmapID); }

        [JsonIgnore]
        public int BeatmapID;

        [JsonProperty("beamtapset_id")]
        private string beatmapset_id { set => int.TryParse(value, out BeatmapsetID); }

        [JsonIgnore]
        public int BeatmapsetID;

        [JsonProperty("date")]
        private string last_update { set => Date = DateTimeOffset.ParseExact(value + "+08:00", "yyyy-MM-dd HH:mm:sszzzz", CultureInfo.InvariantCulture); }

        [JsonIgnore]
        public DateTimeOffset Date;

        [JsonProperty("epicfactor")]
        private string epicfactor { set => int.TryParse(value, out Epicness); }

        [JsonIgnore]
        public int Epicness;
    }
}