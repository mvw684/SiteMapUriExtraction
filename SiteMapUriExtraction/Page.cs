// Copyright Mark J. van Wijk 2023

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public void load() {
            GC.KeepAlive(data);
            throw new NotImplementedException();
        }
    }
}
