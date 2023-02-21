// Copyright Mark J. van Wijk 2023

using CsvHelper.Excel;

using DocumentFormat.OpenXml.Spreadsheet;

namespace SiteMapUriExtractor {

    public partial class SiteReporter {
        /// <summary>
        /// Public record for writing excel data
        /// </summary>
        public class PageData {

            private static int partsCount = 0;

            private string title;
            private string filePart = string.Empty;
            private ArraySegment<string> pathParts;
            private Uri uri;
            internal PageData(Page page) {

                title = page.PageTitle;
                uri = page.Uri;
                var path = uri.AbsolutePath;
                var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

                if (path.Length > 0) {
                    filePart = parts[parts.Length - 1];
                    pathParts = new ArraySegment<string>(parts, 0, parts.Length - 1);
                    if (pathParts.Count > partsCount) {
                        partsCount = pathParts.Count;
                    }
                }
            }

            internal static void WriteHeader(ExcelWriter writer) {

                for (int i = 0; i < partsCount; i++) {
                    writer.WriteField($"Part {i + 1}");
                }
                writer.WriteField("Page");
                writer.WriteField("Title");
                writer.WriteField("URI");
                writer.NextRecord();
            }

            internal void WriteRecord(ExcelWriter writer) {
                int i = 0;
                for(; i < pathParts.Count; i++) {
                    writer.WriteField(pathParts[i]);
                }
                for (; i < partsCount; i++) {
                    writer.WriteField("");
                }
                writer.WriteField(filePart);
                writer.WriteField(title);
                writer.WriteField(uri.AbsoluteUri);
                writer.NextRecord();
            }
        }
    }
}
