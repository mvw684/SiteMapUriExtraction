// Copyright Mark J. van Wijk 2023

using System.Security.Cryptography;

namespace SiteMapUriExtractor {

    /// <summary>
    /// Cached meta data state for a specific Uri including
    /// </summary>
    public class CachedUriState {
        private readonly Uri uri;
        private HttpResponseMessage? headerResponse;
        private DateTimeOffset lastModified;
        
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Create an instance of cached data with a well known location and URI
        /// </summary>
        public CachedUriState(Uri uri) {
            this.uri = uri;
        }

        /// <summary>
        /// The cached original URI
        /// </summary>
        public Uri Uri => uri;

        /// <summary>
        /// True if this page exists/is accessible
        /// </summary>
        public bool PageExists => headerResponse != null && headerResponse.IsSuccessStatusCode;

        /// <summary>
        /// Retrieve data from server and put in cache
        /// </summary>
        public void CheckOnServer(HttpClient client) {
            lastModified = DateTimeOffset.Now;
            var request = new HttpRequestMessage(HttpMethod.Head, uri);
            var getHeadTask = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));
            getHeadTask.Wait();
            getHeadTask.ThrowIfTaskFailed("HEAD", uri);
            headerResponse = getHeadTask.Result;
            foreach(var header in headerResponse.Headers) {
                var key = header.Key;
                var value = String.Join("/", header.Value);
                Console.WriteLine($"Header: {key} = {value}");
            }
            foreach (var header in headerResponse.TrailingHeaders) {
                var key = header.Key;
                var value = String.Join("/", header.Value);
                Console.WriteLine($"TrailingHeader: {key} = {value}");
            }
            foreach (var header in headerResponse.Content.Headers) {
                var key = header.Key;
                var value = String.Join("/", header.Value);
                Console.WriteLine($"ContentHeader: {key} = {value}");
            }
            GC.KeepAlive(headerResponse);
        }
    }
}
