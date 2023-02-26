// Copyright Mark J. van Wijk 2023

namespace SiteMapUriExtractor {

    /// <summary>
    /// Reference to other URI or Page
    /// </summary>
    public class Reference {
        private readonly Page sourcePage;
        private readonly string name;
        private readonly Uri target;
        private readonly bool exists;
        private readonly Page? targetPage;
        private string referenceType;

        /// <summary>
        /// Constructor
        /// </summary>
        public Reference(Page sourcePage, string name, Uri target, bool exists, Page? targetPage, string referenceType) {
            this.sourcePage = sourcePage;
            this.name = name;
            this.target = target;
            this.exists = exists;
            this.targetPage = targetPage;
            this.referenceType = referenceType;
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
        public string SourceTitle => sourcePage.PageTitle;

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
        /// The target page, throws if null
        /// </summary>
        public Page TargetPage => targetPage ?? throw new NullReferenceException("targetPage");

        /// <summary>
        /// Title (if a valid page)
        /// </summary>
        public string TargetTitle => targetPage is not null ? targetPage.PageTitle : name;

        /// <summary>
        /// The type of reference
        /// </summary>
        public string ReferenceType => referenceType;

    }
}
