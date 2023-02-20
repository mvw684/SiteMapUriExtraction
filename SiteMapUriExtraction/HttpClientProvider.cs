// Copyright Mark J. van Wijk 2023

namespace SiteMapUriExtractor {
    internal class HttpClientProvider : IDisposable {
        private HttpClient? client;
        private readonly string server;

        internal static Dictionary<string, HttpClient> clients = new Dictionary<string, HttpClient>();

        /// <summary>
        /// Access/use the <see cref="HttpClient"/>
        /// </summary>
        public HttpClient Client {
            get {
                if (client is null) {
                    throw new ObjectDisposedException(nameof(HttpClientProvider));
                }
                return client;
            }
        }

        internal HttpClientProvider(Uri uri) {
            server = uri.Host;
            if (clients.TryGetValue(server, out client)) {
                clients.Remove(server);
            } else {
                client = new HttpClient();
            }
        }

        public void Dispose() {
            if (client is null) {
                return;
            }
            clients[server] = client;
            client = null;
        }
    }
}
