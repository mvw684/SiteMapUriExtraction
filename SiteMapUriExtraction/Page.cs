// Copyright Mark J. van Wijk 2023

using System.Collections.ObjectModel;

using HtmlAgilityPack;

namespace SiteMapUriExtractor {

    /// <summary>
    /// represents a page on a website
    /// </summary>
    public class Page {

        /// <summary>
        /// Reference to other URI or Page
        /// </summary>
        public class Reference {
            private readonly Page sourcePage;
            private readonly string name;
            private readonly Uri target;
            private readonly bool exists;
            private readonly Page? targetPage;

            /// <summary>
            /// Constructor
            /// </summary>
            public Reference(Page sourcePage, string name, Uri target, bool exists, Page? targetPage) {
                this.sourcePage = sourcePage;
                this.name = name;
                this.target = target;
                this.exists = exists;
                this.targetPage = targetPage;
            }

            /// <summary>
            /// Name, href text
            /// </summary>
            public string Name => name;

            /// <summary>
            /// Source page Uri
            /// </summary>
            public Uri Source => sourcePage.Uri;

            /// <summary>
            /// Source page title
            /// </summary>
            public string SourceTitle => sourcePage.pageTitle;

            /// <summary>
            /// Target Uri
            /// </summary>
            public Uri Target => target;

            /// <summary>
            /// Target Uri is accessible/exists
            /// </summary>
            public bool Exists => exists;


            /// <summary>
            /// Is a reference to one of the pages in the site maps
            /// </summary>
            public bool HasTargetPage => targetPage != null;

            /// <summary>
            /// Title (if a valid page)
            /// </summary>
            public string TargetTitle => targetPage is not null ? targetPage.pageTitle : name;
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
                    bool pageExists = cache.GetState(targetUri).PageExists;
                    
                    if (allPages.TryGetValue(targetUri, out var targetPage)) {
                        targetPage.References++;
                    }
                    var referenceData = new Reference(this, text, targetUri, pageExists, targetPage);
                    outgoingReferences.Add(referenceData);
                    if (targetPage is not null) {
                        targetPage.incomingReferences.Add(referenceData);
                    }
                    if (pageExists) {
                        Console.WriteLine($"{data.Uri.AbsoluteUri} -- {text} -> {targetUri.AbsoluteUri}");
                    } else {
                        Console.WriteLine($"{data.Uri.AbsoluteUri} -- {text} BROKEN!!!-> {targetUri.AbsoluteUri}");
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


        /// <summary>
        /// Outgoing references to other URIs/Pages
        /// </summary>
        public List<Reference> OutgoingReferences => outgoingReferences;

        /// <summary>
        /// Incoming references from other Pages
        /// </summary>
        internal List<Reference> IncomingReferences => incomingReferences;
    }
}
