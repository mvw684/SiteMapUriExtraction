// Copyright Mark J. van Wijk 2023

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsvHelper;

using DocumentFormat.OpenXml.Vml.Spreadsheet;

namespace SiteMapUriExtractor {
    internal class HttpClientProvider : IDisposable {
        private readonly HttpClient client;
        private readonly string server;

        internal static Dictionary<string, HttpClientProvider> clients = new Dictionary<string, HttpClientProvider>();

        /// <summary>
        /// Access/use the <see cref="HttpClient"/>
        /// </summary>
        public HttpClient Client => client;

        private HttpClientProvider(string server) {
            client = new HttpClient();
            this.server = server;
        }

        internal static HttpClientProvider Get(Uri uri) {
            string server = uri.Host;
            if (clients.TryGetValue(server, out var client)) {
                clients.Remove(server);
                return client;
            }
            return new HttpClientProvider(server);
        }

        public void Dispose() {
            clients[server] = this;
        }
    }
}
