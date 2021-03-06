﻿using System.Collections.Generic;

namespace ColdOBot.Osu.Requests
{
    public class GetBeatmapsRequest : OsuApiRequest<List<Beatmap>>
    {
        public GetBeatmapsRequest(string apiKey, bool isRipple, int beatmapSetID = 0, int beatmapID = 0, string user = null, Mode mode = Mode.All, int limit = 500) : base(apiKey, isRipple)
        {
            if (beatmapSetID != 0)
                Parameters.Add("s", "" + beatmapSetID);
            if (beatmapID != 0)
                Parameters.Add("b", "" + beatmapID);
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrWhiteSpace(user))
                Parameters.Add("u", user);
            if (mode != Mode.All)
            {
                Parameters.Add("m", "" + (int)mode);
                if (mode > Mode.Osu)
                    Parameters.Add("a", "1");
            }
            if (limit < 500)
            {
                Parameters.Add("limit", "" + limit);
            }
        }

        protected override string Target => "get_beatmaps";
    }
}