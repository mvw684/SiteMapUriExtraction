// Copyright Mark J. van Wijk 2023

using System.Collections.ObjectModel;

using HtmlAgilityPack;

namespace SiteMapUriExtractor {

    /// <summary>
    /// represents a page on a website
    /// </summary>
    public class Page {

        internal class Reference {
            private readonly Page sourcePage;
            private readonly string name;
            private readonly CachedUriState state;
            private readonly Page? targetPage;

            public Reference(Page sourcePage, string name, CachedUriState state, Page? targePage) {
                this.sourcePage = sourcePage;
                this.name = name;
                this.state = state;
                this.targetPage = targePage;
            }

            public string Name => name;

            public CachedUriState State => state;
        }

        private readonly CachedUriData data;
        private string pageTitle;
        private List<Reference> outgoingReferences = new List<Reference>();
        private List<Reference> incomingReferences = new List<Reference>();

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
        public void Parse(UriCache cache, ReadOnlyDictionary<Uri, Page> allPages) {
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
                    var text = reference.InnerText.Trim();
                    var state = cache.GetState(targetUri);
                    Console.WriteLine($"{data.Uri.AbsoluteUri} -- {text} -> {targetUri.AbsoluteUri}");
                    if (allPages.TryGetValue(targetUri, out var targetPage)) {
                        targetPage.References++;
                    }
                    var referenceData = new Reference(this, text, state, targetPage);
                    outgoingReferences.Add(new Reference(this, text, state, targetPage));
                    if (targetPage is not null) {
                        targetPage.incomingReferences.Add(referenceData);
                    }
                }
            }
        }

        /// <summary>
        /// The Uri of this Page
        /// </summary>
        public Uri Uri => data.Uri;

        /// <summary>
        /// Indication of number of incoming references
        /// </summary>
        public int References { get; private set; }
    }
}
