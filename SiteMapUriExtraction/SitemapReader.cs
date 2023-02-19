// Copyright Mark J. van Wijk 2023

using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml;

namespace SiteMapUriExtractor {

    /// <summary>
    /// Read site map files, see http://www.sitemaps.org/schemas/sitemap/
    /// </summary>
    public class SitemapReader {

        private readonly Dictionary<Uri, Page> pages = new Dictionary<Uri, Page>();
        private readonly UriCache cache;
        private Uri? rootUri;

        /// <summary>
        /// Construct the site map reader with the cache
        /// </summary>
        public SitemapReader(UriCache cache) {
            this.cache = cache;
        }

        /// <summary>
        /// Load the provided site maps
        /// </summary>
        public void Load(List<string> sitemaps) {
            foreach(var sitemapUri in sitemaps) {
                // assume cache is outdated, so add a date guaranteed newer as the cache
                Load(new Uri(sitemapUri), null);
            }
        }

        private void Load(Uri uri, DateTimeOffset? lastModified) {
            rootUri = new Uri(uri, ".");
            var cachedData = cache.Fetch(uri, lastModified);

            XmlDocument doc = new XmlDocument();
            doc.Load(cachedData.CachedFile.FullName);

            var root = doc.DocumentElement;
            if (root?.Name == "sitemapindex") {
                ReadSiteMapIndex(root);
            } else if (root?.Name == "urlset") {
                ReadSiteMap(root);
            }
        }

        private void ReadSiteMap(XmlElement root) {
            foreach(XmlNode node in root.ChildNodes) {
                if (node.Name == "url") {
                    var locNode = node["loc"];
                    var lastModNode = node["lastmod"];
                    if (ParseLoc(locNode, lastModNode, out var uri, out var lastModified)) {
                        AddPage(uri, lastModified);
                    }
                }
            }
        }

        private void ReadSiteMapIndex(XmlElement root) {
            foreach (XmlNode node in root.ChildNodes) {
                if (node.Name == "sitemap") {
                    var locNode = node["loc"];
                    var lastModNode = node["lastmod"];
                    if (ParseLoc(locNode, lastModNode, out var uri, out var lastModified)) { 
                        Load(uri, lastModified);
                    }
                }
            }
        }

        private static bool ParseLoc(
            XmlElement? locNode,
            XmlElement? lastModNode,
            out Uri uri,
            out DateTimeOffset lastModified
        ) {
            var siteMapUriString = locNode?.InnerText;
            var lastModString = lastModNode?.InnerText;
            bool ok = false;
            if (!string.IsNullOrEmpty(siteMapUriString) && !string.IsNullOrEmpty(lastModString)) {
                uri = new Uri(siteMapUriString);
                lastModified = DateTimeOffset.ParseExact(lastModString, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture).ToLocalTime();
                ok = true;
            } else {
                uri = new Uri("http://unknown");
                lastModified = DateTime.MinValue;
            }
            return ok;
        }

        private void AddPage(Uri uri, DateTimeOffset lastModified) {
            var cachedData = cache.Fetch(uri, lastModified);
            var page = new Page(cachedData);
            pages.Add(uri, page);
        }

        /// <summary>
        /// get all pages from the parsed site maps
        /// </summary>
        public ReadOnlyDictionary<Uri, Page> Pages => new ReadOnlyDictionary<Uri, Page>(pages);

        /// <summary>
        /// Root URI
        /// </summary>
        public Uri Root => rootUri is null ? new Uri("https://unknown") : rootUri;
    }
}
