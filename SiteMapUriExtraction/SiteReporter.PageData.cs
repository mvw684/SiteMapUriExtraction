// Copyright Mark J. van Wijk 2023

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
            internal PageData(Page page) {

                title = page.PageTitle;
                uri = page.Uri;
                var path = uri.AbsolutePath;
                pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (pathParts.Length > partsCount) {
                    partsCount = pathParts.Length;
                }
            }

            internal static void WriteHeader(IXLWorksheet sheet) {
                var row = sheet.Row(1);
                int column = 1;
                for (int i = 0; i < partsCount; i++) {
                    row.Cell(column++).SetValue($"Part {i + 1}");
                }
                row.Cell(column++).SetValue("Title");
                row.Cell(column++).SetValue("URI");

            }

            internal void WriteRecord(IXLWorksheet sheet, int rowNumber) {
                var row = sheet.Row(rowNumber);

                int i = 0;
                int column = 1;
                for(; i < pathParts.Length; i++) {
                    row.Cell(column++).SetValue(pathParts[i]);
                }
                column = partsCount + 1;
                var nextField = (char)('A' + (partsCount + 1));
                var titleWithLink = $"=HYPERLINK({nextField}{rowNumber},\"{title}\")";
                row.Cell(column++).SetFormulaA1(titleWithLink);
                row.Cell(column++).SetValue(uri.AbsoluteUri);
            }
        }
    }
}
