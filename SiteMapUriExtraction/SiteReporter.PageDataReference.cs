// Copyright Mark J. van Wijk 2023

namespace SiteMapUriExtractor {

    public partial class SiteReporter {
        internal class PageDataReference {
            internal required string ReferenceType { get; init; }

            internal required PageData To { get; init; }
        }
    }
}
