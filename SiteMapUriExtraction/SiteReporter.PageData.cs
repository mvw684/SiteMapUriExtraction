// Copyright Mark J. van Wijk 2023

using System.Linq.Expressions;

using ClosedXML.Excel;

namespace SiteMapUriExtractor {

    public partial class SiteReporter {
        /// <summary>
        /// Public record for writing excel data
        /// </summary>
        public class PageData {

            private static int partsCount = 0;

            private string title;
            private string[] pathParts;
            private Uri uri;
            private Dictionary<Uri, PageDataReference> referencedPages = new ();
            private int referencedByCount;

            internal PageData(Uri root, Page page) {
                title = page.PageTitle;
                uri = page.Uri;
                var path = uri.AbsolutePath;
                var rootPath = root.AbsolutePath;
                if (path.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase)) {
                    path = path.Substring(rootPath.Length);
                }
                pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (pathParts.Length == 0) {
                    pathParts = new[] { "<home>" };
                }
                if (pathParts.Length > partsCount) {
                    partsCount = pathParts.Length;
                }
            }

            
            internal static void WritePageHeader(IXLWorksheet sheet) {
                var row = sheet.Row(1);
                int column = 1;
                for (int i = 0; i < partsCount; i++) {
                    row.Cell(column++).SetValue($"Part {i + 1}");
                }
                row.Cell(column++).SetValue("Comment");
                row.Cell(column++).SetValue("Title");

            }

            internal void WritePageRecord(IXLWorksheet sheet, int rowNumber) {
                var row = sheet.Row(rowNumber);

                int i = 0;
                int column = 1;
                for(; i < pathParts.Length; i++) {
                    row.Cell(column++).SetValue(pathParts[i]);
                }
                column = partsCount + 1;
                string referencedByString;
                switch(referencedByCount) {
                    case 0:
                        referencedByString = "Not Referenced";
                        break;
                    case 1:
                        referencedByString = $"Referenced by 1 page";
                        break;
                    default:
                        referencedByString = $"Referenced by {referencedByCount} pages";
                        break;

                }
                row.Cell(column++).SetValue(referencedByString);
                row.Cell(column++).SetLink(uri, title);
            }

            internal static void FormatPages(IXLWorksheet sheet) {
                sheet.Row(1).SetAutoFilter(true);
                sheet.SheetView.FreezeRows(1);
                sheet.Columns().AdjustToContents(1, partsCount + 2, 9d, 60d);
                sheet.Column(partsCount + 3).AdjustToContents(50d, 90d);
            }

            internal static void WritePageLinksHeader(IXLWorksheet sheet) {
                var row = sheet.Row(1);
                int column = 1;
                for (int i = 0; i < partsCount; i++) {
                    row.Cell(column++).SetValue($"Source Part {i + 1}");
                }
                row.Cell(column++).SetValue("Source Title");
                row.Cell(column++).SetValue("Link Type");
                for (int i = 0; i < partsCount; i++) {
                    row.Cell(column++).SetValue($"Target Part {i + 1}");
                }
                row.Cell(column++).SetValue("Target Title");
            }

            internal void WritePageLinksRecord(IXLWorksheet sheet, int rowNumber, PageDataReference target) {
                var row = sheet.Row(rowNumber);
                int column = 1;
                for (int i = 0; i < pathParts.Length; i++) {
                    row.Cell(column++).SetValue(pathParts[i]);
                }
                column = partsCount + 1;
                row.Cell(column++).SetLink(uri, title);
                row.Cell(column++).SetValue(target.ReferenceType);
                for (int i = 0; i < target.To.pathParts.Length; i++) {
                    row.Cell(column++).SetValue(target.To.pathParts[i]);
                }

                column = 2 * partsCount + 3;
                row.Cell(column++).SetLink(target.To.uri, target.To.title);
            }

            internal static void FormatPageLinks(IXLWorksheet sheet) {
                sheet.Row(1).SetAutoFilter(true);
                sheet.SheetView.FreezeRows(1);
                // path parts from
                sheet.Columns().AdjustToContents(1, partsCount, 9d, 60d);
                // title + uri from
                sheet.Column(partsCount + 1).AdjustToContents(50d, 90d);
                // reference type
                sheet.Column(partsCount + 2).AdjustToContents(9d, 60d);
                // path parts to
                sheet.Columns().AdjustToContents(partsCount + 3, 2 * partsCount + 3, 9d, 60d);
                // title + uri to
                sheet.Column(2 * partsCount + 4).AdjustToContents(50d, 90d);
            }

            internal void AddTarget(string referenceType, PageData targetData) {
                if (referencedPages.ContainsKey(targetData.uri)) {
                    return;
                }
                referencedPages.Add(targetData.uri, new PageDataReference { ReferenceType = referenceType, To = targetData });
                targetData.referencedByCount++;
            }

            internal List<PageDataReference> ReferencedPages {
                get {
                    var result = referencedPages.Values.ToList();
                    result.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.To.uri.AbsoluteUri, b.To.uri.AbsoluteUri));
                    return result;
                }
            }
        }
    }
}
