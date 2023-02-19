// Copyright Mark J. van Wijk 2023

using System.Text.Json;

namespace SiteMapUriExtractor {

    /// <summary>
    /// Cached data for a specific Uri including relevant meta data
    /// </summary>
    public class CachedUriData {
        private readonly Uri uri;
        private readonly CachedFileData cachedFile;

        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Create an instance of cached data with a well known location and URI
        /// </summary>
        public CachedUriData(Uri uri, CachedFileData cachedFile) {
            this.uri = uri;
            this.cachedFile= cachedFile;
        }

        /// <summary>
        /// Last modification of the meta data
        /// </summary>
        public DateTime LastWriteTime => cachedFile.LastWriteTime;

        /// <summary>
        /// The cached original URI
        /// </summary>
        public Uri Uri => uri;

        /// <summary>
        /// The file from cache
        /// </summary>
        public FileInfo CachedFile => cachedFile.Exists ? cachedFile.File : throw new FileNotFoundException(uri.ToString());


        /// <summary>
        /// Check whether the item is changed on the server compared to the given cache retention period
        /// </summary>
        public bool IsModifiedOnServer(HttpClient client, TimeSpan retention) {
            var lastModified = LastWriteTime;
            var request = new HttpRequestMessage(HttpMethod.Head, uri);
            request.Headers.IfModifiedSince = lastModified - retention;
            //request.Headers.Add("If-Modified-Since", lastModified.ToUniversalTime().ToString("R"));

            var getHeadTask = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));
            getHeadTask.Wait();
            getHeadTask.ThrowIfRequestFailed("HEAD", uri);
            var headerResponse = getHeadTask.Result;
            foreach (var header in headerResponse.Headers) {
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
            if (headerResponse.StatusCode == System.Net.HttpStatusCode.NotModified) {
                return false;
            }
            return headerResponse.IsSuccessStatusCode;
        }

        /// <summary>
        /// Retrieve data from server and put in cache
        /// </summary>
        public void GetFromServer(HttpClient client) {


            var getContentTask = client.GetAsync(uri);

            getContentTask.Wait();
            getContentTask.ThrowIfRequestFailed("GET", uri);
            var content = getContentTask.Result.Content;
            var contentType = content.Headers.ContentType;
            string extension;
            switch (contentType?.MediaType) {
                case "text/xml":
                    extension = ".xml";
                    break;
                case "text/html":
                    extension = ".html";
                    break;
                default:
                    throw new NotImplementedException("Cannot cache content type " + contentType);
            }
            cachedFile.Extension = extension;
            using (var cachedData = cachedFile.File.OpenWrite()) {
                content.CopyTo(cachedData, null, cancellationTokenSource.Token);
            }
            cachedFile.File.Refresh();
        }
    }
}
