// Copyright Mark J. van Wijk 2023

using System.Collections.ObjectModel;
using System.Diagnostics;

using HtmlAgilityPack;

namespace SiteMapUriExtractor {

    /// <summary>
    /// represents a page on a website
    /// </summary>
    public partial class Page {

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
            HashSet<string> existingLinks = new HashSet<string>(StringComparer.Ordinal);

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
                    if (target is null) {
                        continue;
                    }
                    Uri targetUri;
                    Uri? tempUri;
                    if (Uri.TryCreate(target, UriKind.Absolute, out tempUri)) {
                        targetUri = tempUri;
                    } else if (Uri.TryCreate(data.Uri, target, out tempUri)) {
                        targetUri = tempUri;
                    } else {
                        if (target.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
                            targetUri = new Uri(target);
                        } else {
                            targetUri = new Uri(data.Uri, target);
                        }
                    }

                    var innertext = reference.InnerText.Trim();
                    var directInnerText = reference.GetDirectInnerText().Trim();
                    var innerHtml = reference.InnerHtml.Trim();
                    var text = innertext;
                    if (string.IsNullOrEmpty(text)) {
                        var attributes = reference.Attributes.Select(a => a.Name + "=" +a.Value).ToArray();
                        var childNodes = reference.ChildNodes.Select(a => a.Name + "=" + a.InnerHtml).ToArray();
                        text = directInnerText;
                        if (string.IsNullOrEmpty(text)) {
                            // use innerHtml
                            var child = reference.ChildNodes.FirstOrDefault(a => a.Name == "span");
                            if (child != null) {
                                text = child.GetAttributeValue("class", null);
                            }
                        }
                        if (string.IsNullOrEmpty(text)) {
                            var child = reference.ChildNodes.FirstOrDefault(a => a.Name == "img");
                            if (child != null) {
                                text = child.GetAttributeValue("title", null);
                                if (string.IsNullOrEmpty(text)) {
                                    text = child.GetAttributeValue("alt", null);
                                }
                                if (string.IsNullOrEmpty(text)) {
                                    text = "image";
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(text)) {
                            text = innerHtml;
                        }
                    }

                    var key = text + "@" + targetUri.AbsoluteUri;
                    if (existingLinks.Contains(key)) {
                        continue;
                    }
                    existingLinks.Add(key);
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

        /// <summary>
        /// The page title
        /// </summary>
        public string PageTitle => pageTitle;
    }
}
