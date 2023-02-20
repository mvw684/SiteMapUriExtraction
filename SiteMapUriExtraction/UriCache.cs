// Copyright Mark J. van Wijk 2023

using System.Diagnostics;

namespace SiteMapUriExtractor {

    /// <summary>
    /// Fetch and cache contents based on an URI and implement a retention scheme
    /// </summary>
    public class UriCache {
        private readonly DirectoryInfo cacheFolder;
        private readonly TimeSpan retention;
        private Dictionary<Uri, CachedUriData> cachedData = new Dictionary<Uri, CachedUriData> ();
        private Dictionary<Uri, CachedUriState> cachedState = new Dictionary<Uri, CachedUriState>();

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
        }

        /// <summary>
        /// Fetch content for a specific URI
        /// </summary>
        public CachedUriData Fetch(Uri uri, DateTimeOffset? lastModified) {
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
                var modifedAt = lastModified?.ToString("s");
                var okForReuse = lastModified is not null ? expirationTime > lastModified : expirationTime > DateTime.Now;
                
                if (!okForReuse){
                    needsGet = result.IsModifiedOnServer(retention);
                    if (!needsGet) {
                        result.CachedFile.LastWriteTime = DateTime.Now;
                        result.CachedFile.Refresh();
                    }
                }
            } else {
                needsGet = true;
            }

            if (needsGet) {
                result.GetFromServer();
                Console.WriteLine($"Cached: {uri.AbsoluteUri} -> {result.CachedFile.FullName}");
            } else {
                Console.WriteLine($"Reuse: {uri.AbsoluteUri} -> {result.CachedFile.FullName}");
            }
            cachedData[uri] = result;
            return result;
        }

        /// <summary>
        /// get URI state (existence etc)
        /// </summary>
        public CachedUriState GetState(Uri uri) {

            if (cachedState.TryGetValue(uri, out var state)) {
                return state;
            }

            if (cachedData.TryGetValue(uri, out var data)) {
                state = new CachedUriState(uri, true);
            } else {
                var scheme = uri.Scheme;
                switch(scheme) {
                    case "tel":
                    case "mailto":
                        state = new CachedUriState(uri, true);
                        break;
                    case "file":
                        state = new CachedUriState(uri, false);
                        break;
                    case "http":
                    case "https":
                        state = new CachedUriState(uri);
                        state.CheckOnServer();
                        break;

                    default:
                        state = new CachedUriState(uri);
                        state.CheckOnServer();
                        break;
                }
            }
            cachedState.Add(uri, state);
            return state;
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
