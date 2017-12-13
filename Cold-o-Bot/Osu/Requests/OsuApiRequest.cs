using ColdOBot.Net;

namespace ColdOBot.Osu.Requests
{
    public abstract class OsuApiRequest<T> : ApiRequest<T>
    {
        private const string bancho_url = "https://osu.ppy.sh/api/";
        private const string ripple_url = "https://ripple.moe/api/";
        private readonly bool isRipple;

        protected override string BaseUri => isRipple ? ripple_url : bancho_url;

        protected OsuApiRequest(string apiKey, bool isRipple)
        {
            Parameters.Add("k", $"{apiKey}");
            this.isRipple = isRipple;
        }
    }
}
