// Copyright Mark J. van Wijk 2023

using HtmlAgilityPack;

namespace SiteMapUriExtractor {

    /// <summary>
    /// represents a page on a website
    /// </summary>
    public class Page {

        internal class Reference {
            private readonly string name;
            private readonly CachedUriState state;

            public Reference(string name, CachedUriState state) {
                this.name = name;
                this.state = state;
            }

            public string Name => name;

            public CachedUriState State => state;
        }

        private readonly CachedUriData data;
        private string pageTitle;
        private List<Reference> pageReferences = new List<Reference>();

        /// <summary>
        /// Create a page from the related cached data entry
        /// </summary>
        /// <param name="data"></param>
        public Page(CachedUriData data) {
            this.data = data;
            pageTitle = Path.GetFileNameWithoutExtension(data.CachedFile.Name);
        }

        /// <summary>
        /// Load the page and extract URIs and other relevant data
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Parse(UriCache cache) {
            var fileName = data.CachedFile.FullName;
            Console.WriteLine($"Parsing: {data.CachedFile.FullName}");
            if (fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase)) {
                var doc = new HtmlDocument();
                doc.DetectEncodingAndLoad(fileName);
                var root = doc.DocumentNode;
                var title = root.SelectSingleNode("//head//title");
                pageTitle = title.InnerText;
                var references = root.Descendants("a").ToList();
                foreach (var reference in references ) {
                    var target = reference.GetAttributeValue("href", null);
                    Uri targetUri;

                    if (target.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
                        targetUri = new Uri(target);
                    } else {
                        targetUri = new Uri(data.Uri, target);
                    }
                    var text = reference.InnerHtml.Trim();
                    var state = cache.GetState(targetUri);
                    pageReferences.Add(new Reference(text, state));


                }
            }
        }

        /// <summary>
        /// The Uri of this Page
        /// </summary>
        public Uri Uri => data.Uri;
    }
}
