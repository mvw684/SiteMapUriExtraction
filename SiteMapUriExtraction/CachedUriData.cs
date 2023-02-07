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
        /// Retrieve data from server and put in cache
        /// </summary>
        public void GetFromServer(HttpClient client) {
            var getContentTask = client.GetAsync(uri);

            getContentTask.Wait();
            OperationFailedException.ThrowIfFailed("GET", uri, getContentTask);
            var content = getContentTask.Result.Content;
            var contentType = content.Headers.ContentType;
            string extension;
            switch (contentType?.MediaType) {
                case "text/xml":
                    extension = ".xml";
                    break;
                default:
                    throw new NotImplementedException("Cannot cache content type " + contentType);
            }
            cachedFile.Extension = extension;
            content.CopyTo(cachedFile.File.OpenWrite(), null, cancellationTokenSource.Token);
            cachedFile.File.Refresh();
        }
    }
}
