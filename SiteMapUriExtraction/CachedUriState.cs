// Copyright Mark J. van Wijk 2023

namespace SiteMapUriExtractor {

    /// <summary>
    /// Cached meta data state for a specific Uri including
    /// </summary>
    public class CachedUriState {
        private readonly Uri uri;
        private bool exists;
        
        /// <summary>
        /// Create an instance of cached data with a well known location and URI
        /// </summary>
        public CachedUriState(Uri uri) {
            this.uri = uri;
        }

        /// <summary>
        /// Create an instance of cached data with a well known location and URI
        /// </summary>
        public CachedUriState(Uri uri, bool exists) {
            this.uri = uri;
            this.exists = exists;
        }

        /// <summary>
        /// The cached original URI
        /// </summary>
        public Uri Uri => uri;

        /// <summary>
        /// True if this page exists/is accessible
        /// </summary>
        public bool PageExists => exists;

        /// <summary>
        /// Retrieve data from server and put in cache
        /// </summary>
        public void CheckOnServer() {
            using var clientProvider = HttpClientProvider.Get(uri);
            var client = clientProvider.Client;
            Console.WriteLine($"Check existence: {uri.AbsoluteUri}");
            var request = new HttpRequestMessage(HttpMethod.Head, uri);
            var getHeadTask = client.SendAsync(request);
            getHeadTask.Wait();
            getHeadTask.ThrowIfTaskFailed("HEAD", uri);
            var headerResponse = getHeadTask.Result;
            exists = headerResponse.IsSuccessStatusCode;
            //foreach(var header in headerResponse.Headers) {
            //    var key = header.Key;
            //    var value = String.Join("/", header.Value);
            //    Console.WriteLine($"Header: {key} = {value}");
            //}
            //foreach (var header in headerResponse.TrailingHeaders) {
            //    var key = header.Key;
            //    var value = String.Join("/", header.Value);
            //    Console.WriteLine($"TrailingHeader: {key} = {value}");
            //}
            foreach (var header in headerResponse.Content.Headers) {
                var key = header.Key;
                var value = String.Join("/", header.Value);
                Console.WriteLine($"ContentHeader: {key} = {value}");
            }
            headerResponse.Dispose();
            getHeadTask.Dispose();
        }
    }
}
