using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ColdOBot.Net
{
    public abstract class ApiRequest<T> : ApiRequest
    {
        public new async Task<T> Perform()
        {
            return JsonConvert.DeserializeObject<T>(await PerformDeserialized());
        }

        public async Task<string> PerformDeserialized() => await base.Perform();
    }

    public abstract class ApiRequest
    {
        protected abstract string BaseUri { get; }
        protected abstract string Target { get; }
        protected readonly List<string> Parameters = new List<string>();

        public async Task<string> Perform()
        {
            var req = WebRequest.Create(BaseUri + Target + "?" + string.Join("&", Parameters));
            using (var response = await req.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream ?? throw new InvalidOperationException($"{nameof(stream)} should never be null")))
                return await reader.ReadToEndAsync();
        }
    }
}