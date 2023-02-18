// Copyright Mark J. van Wijk 2023

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace SiteMapUriExtractor {

    /// <summary>
    /// represents a page on a website
    /// </summary>
    public class Page {

        private readonly CachedUriData data;

        /// <summary>
        /// Create a page from the related cached data entry
        /// </summary>
        /// <param name="data"></param>
        public Page(CachedUriData data) {
            this.data = data;
        }

        /// <summary>
        /// Load the page and extract URIs and other relevant data
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Parse() {
            var fileName = data.CachedFile.FullName;
            if (fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase)) {
                var doc = new HtmlDocument();
                doc.LoadHtml(fileName);
                var root = doc.DocumentNode;
                var title = root.SelectSingleNode("//head//title");
                GC.KeepAlive(title);
            }
        }

        /// <summary>
        /// The Uri of this Page
        /// </summary>
        public Uri Uri => data.Uri;
    }
}
