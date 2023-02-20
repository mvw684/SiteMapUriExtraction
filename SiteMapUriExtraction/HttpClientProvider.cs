// Copyright Mark J. van Wijk 2023

namespace SiteMapUriExtractor {
    internal class HttpClientProvider : IDisposable {
        private HttpClient? client;
        private readonly string server;

        internal static Dictionary<string, HttpClient> clients = new Dictionary<string, HttpClient>();

        //private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromDays(4));

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

        /// <summary>
        /// Cancellation token to use
        /// </summary>
        public CancellationToken CancellationToken => CancellationToken.None;

        internal HttpClientProvider(Uri uri) {
            server = uri.Host;
            if (clients.TryGetValue(server, out client)) {
                clients.Remove(server);
            } else {
                var handler = new HttpClientHandler();
                client = new HttpClient(handler, true);
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
