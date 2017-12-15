using System;
using System.Collections.Generic;

namespace ColdOBot.Osu.Requests
{
    public class GetUserRequest : OsuApiRequest<List<User>>
    {
        public GetUserRequest(string apiKey, bool isRipple, string user, bool isID = false, Mode mode = Mode.Osu, int days = 1) : base(apiKey, isRipple)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrWhiteSpace(user))
                throw new ArgumentException($"{nameof(user)} must be a not null string, but it was {user}");
            Parameters.Add("u", $"{user}");
            if (isID)
                Parameters.Add("type", "id");
            if (mode > Mode.Osu)
                Parameters.Add("m",$"{(int)mode}");
            if (days > 1)
                Parameters.Add("event_days",$"{Math.Min(31, days)}");
        }

        protected override string Target => "get_user";
    }
}