using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ColdOBot.Helpers.Osu.Requests
{
    public abstract class ApiRequest<T>
    {
        private const string bancho_url = "https://osu.ppy.sh/api/";
        private const string ripple_url = "https://ripple.moe/api/";
        protected abstract string Target { get; }
        protected readonly List<string> Parameters = new List<string>();
        private readonly string apiKey;
        private readonly bool isRipple;

        protected ApiRequest(string apiKey, bool isRipple)
        {
            this.isRipple = isRipple;
            this.apiKey = apiKey;
        }

        public async Task<T> Perform()
        {
            return JsonConvert.DeserializeObject<T>(await PerformDeserialized());
        }

        public async Task<string> PerformDeserialized()
        {
            Parameters.Add($"k={apiKey}");
            var req = WebRequest.Create((isRipple ? ripple_url : bancho_url) + Target + "?" + string.Join("&", Parameters));

            using (var response = await req.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream ?? throw new InvalidOperationException($"{nameof(stream)} should never be null")))
                return await reader.ReadToEndAsync();
        }
    }
}
