using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SiteMapUriExtractor {

    /// <summary>
    /// Read site map files, see http://www.sitemaps.org/schemas/sitemap/
    /// </summary>
    public class SitemapReader {

        private readonly Dictionary<Uri, Page> pages = new Dictionary<Uri, Page>();
        private readonly UriCache cache;

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
                // assume changed, so fetch from server
                Load(new Uri(sitemapUri), DateTime.Now);
            }
        }

        private void Load(Uri uri, DateTime lastModified) {
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
                if (node.Name == "sitemap") {
                    var locNode = node["loc"];
                    var lastModNode = node["lastmod"];

                    var siteMapUriString = locNode?.InnerText;
                    var lastModString = lastModNode?.InnerText;
                    if (!string.IsNullOrEmpty(siteMapUriString) && !string.IsNullOrEmpty(lastModString)) {
                        var uri = new Uri(siteMapUriString);
                        var lastModified = DateTime.ParseExact(lastModString, "O", CultureInfo.InvariantCulture).ToLocalTime();
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

                    var siteMapUriString = locNode?.InnerText;
                    var lastModString = lastModNode?.InnerText;
                    if (!string.IsNullOrEmpty(siteMapUriString) && !string.IsNullOrEmpty(lastModString)) {
                        var uri = new Uri(siteMapUriString);
                        var lastModified = DateTime.ParseExact(lastModString, "O", CultureInfo.InvariantCulture).ToLocalTime();
                        Load(uri, lastModified);
                    }
                }
            }
        }

        private void AddPage(Uri uri, DateTime lastModified) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// get all html pages from the parsed site maps
        /// </summary>
        public List<Page> GetPagesFromSitemaps() {
            throw new NotImplementedException();
        }
    }
}
