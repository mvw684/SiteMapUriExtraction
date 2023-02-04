// Copyright Mark J. van Wijk 2023

using System.Net;

namespace SiteMapUriExtractor {

    /// <summary>
    /// Fetch and cache contents based on an URI and implement a retention scheme
    /// </summary>
    public class UriCache {
        private readonly DirectoryInfo cacheFolder;
        private readonly RetentionPolicy retention;
        private readonly HttpClient client;
        private Dictionary<Uri, CachedUriData> cachedData = new Dictionary<Uri, CachedUriData> ();

        /// <summary>
        /// Create the cache with a <see cref="Directory">location</see> and a <see cref="RetentionPolicy"/>
        /// </summary>
        public UriCache(DirectoryInfo cacheFolder, RetentionPolicy retention) {
            this.cacheFolder = cacheFolder;
            if (retention is RetentionPolicy.None) { 
                this.retention = RetentionPolicy.Header;
            } else {
                this.retention = retention;
            }
            client = new HttpClient();
        }

        /// <summary>
        /// Fetch content for a specific URI
        /// </summary>
        public CachedUriData Fetch(Uri uri) {
            if (cachedData.TryGetValue(uri, out var result)) {
                return result;
            }

            DirectoryInfo cachedFileLocation = GetCachedFileLocation(uri);
            bool needsHeader = false;
            bool needsGet = false;

            if (cachedFileLocation.Exists) {
                result = new CachedUriData(uri, cachedFileLocation);

                var age = DateTime.Now - cachedFileLocation.LastWriteTime;
                if (retention == RetentionPolicy.Header) {
                    needsHeader = true;
                } else if (retention == RetentionPolicy.Day) {
                    if (age.TotalDays >= 1) {
                        needsHeader = true;
                    }
                } else if (retention == RetentionPolicy.Week) {
                    if (age.TotalDays >= 7) {
                        needsHeader = true;
                    }
                }
            } else {
                needsGet = true;
            }
            if (needsHeader) {
                var getHeadTask = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));
                getHeadTask.Wait(); 
                if (!getHeadTask.IsCompletedSuccessfully) {
                    var exception = getHeadTask.Exception;
                    if (exception is null) {
                        throw new OperationFailedException($"HEAD {uri}, {getHeadTask.Result.StatusCode} {getHeadTask.Result.ReasonPhrase}");
                    } else {
                        throw new OperationFailedException($"HEAD {uri}", exception);

                    }
                }
                var header = getHeadTask.Result;
                if (header.IsSuccessStatusCode) {
                    var contentType = header.Headers.Where(a => a.Key == "Content - Type").First().Value;
                    // TODO: decide whether we can return cached data or need to reread from server
                    throw new NotImplementedException();
                } else {
                    throw new OperationFailedException($"HEAD {uri}, {header.StatusCode} {header.ReasonPhrase}");
                }
            }

            var getContentTask = client.GetAsync(uri);
            getContentTask.Wait();

            if (getContentTask.IsCompletedSuccessfully) {
                var content = getContentTask.Result;
                // TODO: create cached data from contents
                throw new NotImplementedException();

            }
            GC.KeepAlive(needsGet);
            throw new NotImplementedException();

        }

        private DirectoryInfo GetCachedFileLocation(Uri uri) {
            string path = Path.Combine(cacheFolder.FullName, uri.Host, uri.AbsolutePath);
            var result = new DirectoryInfo(path);
            return result;
        }
    }
}
