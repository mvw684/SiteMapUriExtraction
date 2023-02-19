// Copyright Mark J. van Wijk 2023

namespace SiteMapUriExtractor {

    /// <summary>
    /// Fetch and cache contents based on an URI and implement a retention scheme
    /// </summary>
    public class UriCache {
        private readonly DirectoryInfo cacheFolder;
        private readonly HttpClient client;
        private readonly TimeSpan retention;
        private Dictionary<Uri, CachedUriData> cachedData = new Dictionary<Uri, CachedUriData> ();
        private Dictionary<Uri, CachedUriState> cachedState = new Dictionary<Uri, CachedUriState> ();

        /// <summary>
        /// Create the cache with a <see cref="Directory">location</see> and a <see cref="RetentionPolicy"/>
        /// </summary>
        public UriCache(DirectoryInfo cacheFolder, RetentionPolicy retentionPolicy) {
            this.cacheFolder = cacheFolder;
            switch(retentionPolicy) {
                case RetentionPolicy.Hour:
                    retention = TimeSpan.FromHours(1);
                    break;
                case RetentionPolicy.Day:
                    retention = TimeSpan.FromDays(1);
                    break;
                case RetentionPolicy.Week:
                    retention = TimeSpan.FromDays(7);
                    break;
                default:
                    retention = TimeSpan.Zero;
                    break;

            }
            client = new HttpClient();
        }

        /// <summary>
        /// Fetch content for a specific URI
        /// </summary>
        public CachedUriData Fetch(Uri uri, DateTimeOffset lastModified) {
            if (cachedData.TryGetValue(uri, out var result)) {
                return result;
            }

            var cachedFileLocation = GetCachedFileLocation(uri);
            bool needsGet = false;
            result = new CachedUriData(uri, cachedFileLocation);

            if (cachedFileLocation.Exists) {
                var cachedTime = cachedFileLocation.LastWriteTime;
                var expirationTime = cachedTime + retention;

                var cachedAt = cachedTime.ToString("s");
                var expiresAt = expirationTime.ToString("s");
                var modifedAt = lastModified.ToString("s");
                var okForReuse = expirationTime > lastModified;
                
                if (!okForReuse){
                    needsGet = result.IsModifiedOnServer(client, retention);
                    if (!needsGet) {
                        result.CachedFile.LastWriteTime = DateTime.Now - retention;
                        result.CachedFile.Refresh();
                    }
                }
            } else {
                needsGet = true;
            }

            if (needsGet) {
                result.GetFromServer(client);
                Console.WriteLine($"Cached: {uri.AbsoluteUri} -> {result.CachedFile.FullName}");
            } else {
                Console.WriteLine($"Reuse: {uri.AbsoluteUri} -> {result.CachedFile.FullName}");
            }
            cachedData[uri] = result;
            return result;
        }

        private CachedFileData GetCachedFileLocation(Uri uri) {

            string uriPath = uri.AbsolutePath;
            uriPath = uriPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            uriPath = uriPath.Trim(Path.DirectorySeparatorChar);
            string path = Path.Combine(cacheFolder.FullName, uri.Host, uriPath);

            string folder = Path.GetDirectoryName(path)!;
            string fullFileName = Path.GetFileName(path);
            string fileName = Path.GetFileNameWithoutExtension(fullFileName);
            string extension = Path.GetExtension(fullFileName);
            var result = new CachedFileData(folder, fileName, extension);
            return result;
        }
    }
}
