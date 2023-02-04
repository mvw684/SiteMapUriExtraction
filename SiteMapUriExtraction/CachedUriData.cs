// Copyright Mark J. van Wijk 2023

using System.Text.Json;

namespace SiteMapUriExtractor {

    /// <summary>
    /// Cached data for a specific Uri including relevant meta data
    /// </summary>
    public class CachedUriData {
        private DirectoryInfo cachedFileLocation;
        private Uri uri;
        private FileInfo metaDataFile;
        private Dictionary<string, string> metaData = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>
        /// Create an instance of cached data with a well known location and URI
        /// </summary>
        public CachedUriData(Uri uri, DirectoryInfo cachedFileLocation) {
            this.uri = uri;
            this.cachedFileLocation = cachedFileLocation;
            metaDataFile = new FileInfo(Path.Combine(cachedFileLocation.FullName, "metData.json"));
            if (metaDataFile.Exists) {
                var doc = JsonDocument.Parse(metaDataFile.OpenRead());
                foreach(var property in doc.RootElement.EnumerateObject()) {
                    metaData.Add(property.Name, property.Value.ToString());
                }
            }
        }

        /// <summary>
        /// Last modification of the meat data
        /// </summary>
        public DateTime LastModfied => metaDataFile.Exists ? metaDataFile.LastWriteTime : DateTime.MinValue;


    }
}
