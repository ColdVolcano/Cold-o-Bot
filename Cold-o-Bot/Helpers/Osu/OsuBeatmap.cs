using System;
using System.Globalization;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace ColdOBot.Helpers.Osu
{
    public struct OsuBeatmap
    {
#pragma warning disable IDE1006
        [JsonProperty("approved")]
        private string approved { set => Enum.TryParse(value, out RankedState); }

        [JsonIgnore]
        public RankedState RankedState;

        [JsonProperty("approved_date")]
        private string approved_date { set => ApprovedDate = value == null ? new DateTimeOffset() : DateTimeOffset.ParseExact(value + "+08:00", "yyyy-MM-dd HH:mm:sszzzz", CultureInfo.InvariantCulture); }

        [JsonIgnore]
        public DateTimeOffset ApprovedDate;

        [JsonProperty("last_update")]
        private string last_update { set => LastUpdate = DateTimeOffset.ParseExact(value + "+08:00", "yyyy-MM-dd HH:mm:sszzzz", CultureInfo.InvariantCulture); }

        [JsonIgnore]
        public DateTimeOffset LastUpdate;

        [JsonProperty("artist")]
        public string Artist;

        [JsonProperty("beamtap_id")]
        private string beatmap_id { set => int.TryParse(value, out BeatmapID); }

        [JsonIgnore]
        public int BeatmapID;

        [JsonProperty("beamtapset_id")]
        private string beatmapset_id { set => int.TryParse(value, out BeatmapsetID); }

        [JsonIgnore]
        public int BeatmapsetID;

        [JsonProperty("bpm")]
        private string bpm { set => double.TryParse(value, out BPM); }

        [JsonIgnore]
        public double BPM;

        [JsonProperty("creator")]
        public string Author;

        [JsonProperty("difficultyrating")]
        private string difficultyrating { set => double.TryParse(value, out StarRating); }

        [JsonIgnore]
        public double StarRating;

        [JsonProperty("diff_size")]
        private string diff_size { set => float.TryParse(value, out CS); }

        [JsonIgnore]
        public float CS;

        [JsonProperty("diff_overall")]
        private string diff_overall { set => float.TryParse(value, out OD); }

        [JsonIgnore]
        public float OD;

        [JsonProperty("diff_approach")]
        private string diff_approach { set => float.TryParse(value, out AR); }

        [JsonIgnore]
        public float AR;

        [JsonProperty("diff_drain")]
        private string diff_drain { set => float.TryParse(value, out HP); }

        [JsonIgnore]
        public float HP;

        [JsonProperty("hit_length")]
        public string hit_length { set => int.TryParse(value, out DrainTime); }

        [JsonIgnore]
        public int DrainTime;

        [JsonProperty("source")]
        public string Source;

        [JsonProperty("genre_id")]
        private string genre_id { set => Enum.TryParse(value, out Genre); }

        [JsonIgnore]
        public OsuGenre Genre;

        [JsonProperty("language_id")]
        private string language_id { set => Enum.TryParse(value, out Language); }

        [JsonIgnore]
        public OsuLanguage Language;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("total_length")]
        public string total_length { set => int.TryParse(value, out Length); }

        [JsonIgnore]
        public int Length;

        [JsonProperty("version")]
        public string Version;

        [JsonProperty("file_md5")]
        public string MD5Hash;

        [JsonProperty("mode")]
        private string mode { set => Enum.TryParse(value, out Gamemode); }

        [JsonIgnore]
        public OsuMode Gamemode;

        [JsonProperty("tags")]
        public string Tags;

        [JsonProperty("favourite_count")]
        public string favourite_count { set => int.TryParse(value, out Favourites); }

        [JsonIgnore]
        public int Favourites;

        [JsonProperty("playcount")]
        public string playcount { set => int.TryParse(value, out Plays); }

        [JsonIgnore]
        public int Plays;

        [JsonProperty("passcount")]
        public string passcount { set => int.TryParse(value, out Passes); }

        [JsonIgnore]
        public int Passes;

        [JsonProperty("max_combo")]
        public string max_combo { set => MaxCombo = int.TryParse(value, out int i) ? (int?)i : null; }

        [JsonIgnore]
        public int? MaxCombo;
#pragma warning restore IDE1006
    }
}