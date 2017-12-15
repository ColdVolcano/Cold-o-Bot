using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ColdOBot.Net
{
    public abstract class ApiRequest<T> : ApiRequest
    {
        public new async Task<T> Perform() => JsonConvert.DeserializeObject<T>(await PerformDeserialized());

        public async Task<string> PerformDeserialized() => await base.Perform();
    }

    public abstract class ApiRequest
    {
        protected virtual RequestMethod RequestMethod => RequestMethod.Get;
        protected abstract string BaseUri { get; }
        protected abstract string Target { get; }
        protected readonly Dictionary<string, string> Parameters = new Dictionary<string, string>();

        public async Task<string> Perform()
        {
            if (RequestMethod == RequestMethod.Get)
                return await client.GetStringAsync(BaseUri + Target + (Parameters.Count > 0 ? "?" + string.Join("&", Parameters.Select(s => s.Key + "=" + s.Value)) : ""));
            return await (await client.PostAsync(BaseUri + Target, new FormUrlEncodedContent(Parameters))).Content.ReadAsStringAsync();
        }

        private static readonly HttpClient client = new HttpClient();
    }

    public enum RequestMethod
    {
        Get,
        Post
    }
}