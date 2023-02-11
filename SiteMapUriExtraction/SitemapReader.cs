using System;
using System.Collections.Generic;
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
                Load(new Uri(sitemapUri));
            }
        }

        private void Load(Uri uri) {
            var cachedData = cache.Fetch(uri);

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
            throw new NotImplementedException();
        }

        private void ReadSiteMapIndex(XmlElement root) {
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
