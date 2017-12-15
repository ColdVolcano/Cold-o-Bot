using System;
using System.Globalization;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace ColdOBot.Osu
{
    public struct Score
    {
#pragma warning disable IDE1006 // Naming Styles
        [JsonProperty("score_id")]
        private string score_id { set => int.TryParse(value, out ID); }

        [JsonIgnore]
        public int ID;

        [JsonProperty("score")]
        private string score { set => int.TryParse(value, out TotalScore); }

        [JsonIgnore]
        public int TotalScore;

        [JsonProperty("count300")]
        private string count300 { set => int.TryParse(value, out Hit300); }

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

        [JsonProperty("countmiss")]
        private string countmiss { set => int.TryParse(value, out Miss); }

        [JsonIgnore]
        public int Miss;

        [JsonProperty("maxcombo")]
        public string maxcombo { set => int.TryParse(value, out MaxCombo); }

        [JsonIgnore]
        public int MaxCombo;

        [JsonProperty("countkatu")]
        private string countkatu { set => int.TryParse(value, out HitKatu); }

        [JsonIgnore]
        public int HitKatu;

        [JsonProperty("countgeki")]
        private string countgeki { set => int.TryParse(value, out HitGeki); }

        [JsonIgnore]
        public int HitGeki;

        [JsonProperty("perfect")]
        private string perfect { set => Perfect = value != "0";}

        [JsonIgnore]
        public bool Perfect;

        [JsonProperty("enabled_mods")]
        private string enabled_mods { set => Enum.TryParse(value, out Mods); }

        [JsonIgnore]
        public Mod Mods;

        [JsonProperty("user_id")]
        private string user_id { set => int.TryParse(value, out UserID); }

        [JsonIgnore]
        public int UserID;

        [JsonProperty("date")]
        private string date { set => Date = value == null ? new DateTimeOffset() : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture); }

        [JsonIgnore]
        public DateTimeOffset Date;

        [JsonProperty("rank")]
        public string Rank;

        [JsonProperty("")]
        private string pp { set => float.TryParse(value, out PP); }

        [JsonIgnore]
        public float PP;
#pragma warning restore IDE1006 // Naming Styles
    }
}