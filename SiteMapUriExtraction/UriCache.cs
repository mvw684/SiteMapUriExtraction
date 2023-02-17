// Copyright Mark J. van Wijk 2023

namespace SiteMapUriExtractor {

    /// <summary>
    /// Fetch and cache contents based on an URI and implement a retention scheme
    /// </summary>
    public class UriCache {
        private readonly DirectoryInfo cacheFolder;
        private readonly HttpClient client;
        private Dictionary<Uri, CachedUriData> cachedData = new Dictionary<Uri, CachedUriData> ();
        TimeSpan retention;

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
        public CachedUriData Fetch(Uri uri, DateTime lastModified) {
            if (cachedData.TryGetValue(uri, out var result)) {
                return result;
            }

            var cachedFileLocation = GetCachedFileLocation(uri);
            bool needsGet = false;

            if (cachedFileLocation.Exists) {
                if (lastModified + retention > cachedFileLocation.LastWriteTime) {
                    needsGet = true;
                }
            } else {
                needsGet = true;
            }

            result = new CachedUriData(uri, cachedFileLocation);

            if (needsGet) {
                result.GetFromServer(client);
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
