// Copyright Mark J. van Wijk 2023


using ClosedXML.Excel;

namespace SiteMapUriExtractor {
    internal static class ClosedXmlExtensions {

        internal static void SetLink(this IXLCell cell, Uri uri, string title) {
            if (uri.IsAbsoluteUri) {
                cell.SetHyperlink(new XLHyperlink(uri, title + " - " + uri.AbsoluteUri));
                cell.SetValue(title);
            }
        }

        internal static void SetLink(this IXLCell cell, Uri uri) {
            if (uri.IsAbsoluteUri) {
                cell.SetHyperlink(new XLHyperlink(uri));
                cell.SetValue(uri.AbsoluteUri);
            }
        }
    }
}
