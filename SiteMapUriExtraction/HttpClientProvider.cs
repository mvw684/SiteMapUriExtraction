// Copyright Mark J. van Wijk 2023

namespace SiteMapUriExtractor {

    /// <summary>
    /// Provide and cache HttpClients
    /// </summary>
    public class HttpClientProvider : IDisposable {
        private HttpClient? client;
        private readonly string server;

        internal static Dictionary<string, HttpClient> clients = new Dictionary<string, HttpClient>();


        /// <summary>
        /// Dispose and remove all currently open clients
        /// </summary>
        public static void DisposeAllClients() {
            var clientsToDispose = clients.Values.ToList();
            clients.Clear();
            foreach (var client in clientsToDispose) {
                client.Dispose();
            }
        }

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
                client.Timeout = TimeSpan.FromMinutes(60);
            }
        }

        /// <summary>
        /// Dispose this instance and return the client to the cache.
        /// </summary>
        public void Dispose() {
            if (client is null) {
                return;
            }
            clients[server] = client;
            client = null;
        }
    }
}
