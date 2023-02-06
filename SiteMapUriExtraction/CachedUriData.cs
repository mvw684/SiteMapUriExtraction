// Copyright Mark J. van Wijk 2023

using System.Text.Json;

namespace SiteMapUriExtractor {

    /// <summary>
    /// Cached data for a specific Uri including relevant meta data
    /// </summary>
    public class CachedUriData {
        private Uri uri;
        private Dictionary<string, string> metaData = new Dictionary<string, string>(StringComparer.Ordinal);
        private CachedFileData cachedFile;

        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Create an instance of cached data with a well known location and URI
        /// </summary>
        public CachedUriData(Uri uri, CachedFileData cachedFile) {
            this.uri = uri;
            this.cachedFile= cachedFile;
        }

        /// <summary>
        /// Last modification of the meat data
        /// </summary>
        public DateTime LastWriteTime => cachedFile.LastWriteTime;

        /// <summary>
        /// Retrieve data from server and put in cache
        /// </summary>
        public void GetFromServer(HttpClient client) {
            var getContentTask = client.GetAsync(uri);

            getContentTask.Wait();
            OperationFailedException.ThrowIfFailed("GET", uri, getContentTask);

            metaData.Clear();
            var content = getContentTask.Result.Content;
            var contentType = content.Headers.ContentType;
            foreach (var header in content.Headers) {
                string key = header.Key;
                string value = String.Join("/", header.Value);
                metaData.Add(key, value);
            }

            string extension = ".contents";
            switch (contentType?.MediaType) {
                case "text/xml":
                    extension = ".xml";
                    break;
                default:
                    throw new NotImplementedException("Cannot cache content type " + contentType);
            }
            if (cachedFile.Exists)

            contentFile = new FileInfo(Path.Combine(cachedFileLocation.FullName, "contents" + extension));
            content.CopyTo(contentFile.OpenWrite(), null, cancellationTokenSource.Token);
            contentFile.Refresh();

            JsonSerializer.Serialize(metaDataFile.OpenWrite(), metaData, metaData.GetType());

        }
    }
}
