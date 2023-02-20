// Copyright Mark J. van Wijk 2023

using CsvHelper.Excel;

using DocumentFormat.OpenXml.Spreadsheet;

namespace SiteMapUriExtractor {

    /// <summary>
    /// Report the identified pages and references
    /// </summary>
    public class SiteReporter {

        /// <summary>
        /// Public record for writing excel data
        /// </summary>
        public class RowData {

            internal RowData() {
                SourceTitle = string.Empty;
                SourceRelativeUri = string.Empty;
                SourceUri = string.Empty;
                LinkTitle = string.Empty;
                TargetTitle = string.Empty;
                TargetRelativeUri = string.Empty;
                TargetUri = string.Empty;
                Comment = string.Empty;
            }

            internal RowData(Uri root, Page notReferencedPage) : this() {
                TargetTitle = notReferencedPage.PageTitle;
                TargetUri = notReferencedPage.Uri.AbsoluteUri;
                Comment = "Not linked from other pages";

                TargetRelativeUri = GetRelative(root, notReferencedPage.Uri);
            }

            internal RowData(Uri root, Page.Reference reference) {
                SourceTitle = reference.SourceTitle;
                SourceRelativeUri = GetRelative(root, reference.Source);
                SourceUri = reference.Source.AbsoluteUri;
                LinkTitle = reference.Name;
                TargetTitle = reference.TargetTitle;
                TargetRelativeUri = GetRelative(root, reference.Target);
                TargetUri = reference.Target.AbsoluteUri;
                List<string> commentParts = new();
                if (!reference.Exists) {
                    commentParts.Add("Link does not exist");
                }
                if (!reference.HasTargetPage) {
                    commentParts.Add("To External");
                }
                Comment = string.Join(" / ", commentParts);

            }

            private string GetRelative(Uri root, Uri uri) {
                string relativeUri;
                var fullRoot = root.AbsoluteUri;
                var fullTarget = uri.AbsoluteUri;
                if (fullTarget.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase)) {
                    relativeUri = fullTarget.Substring(fullRoot.Length);
                } else {
                    relativeUri = "<External>";
                }
                return relativeUri;
            }

            /// <summary>
            /// See property name
            /// </summary>
            public string SourceTitle { get; init; }

            /// <summary>
            /// See property name
            /// </summary>
            public string SourceRelativeUri { get; init; }

            /// <summary>
            /// See property name
            /// </summary>
            public string SourceUri { get; init; }

            /// <summary>
            /// See property name
            /// </summary>
            public string LinkTitle { get; init; }

            /// <summary>
            /// See property name
            /// </summary>
            public string Comment { get; init; }

            /// <summary>
            /// See property name
            /// </summary>
            public string TargetTitle { get; init; }

            /// <summary>
            /// See property name
            /// </summary>
            public string TargetRelativeUri { get; init; }

            /// <summary>
            /// See property name
            /// </summary>
            public string TargetUri { get; init; }
        }

        private readonly List<Page> pages = new ();
        private readonly Uri root;

        /// <summary>
        /// Construct with the parsed pages to report on and the root Uri of the site.
        /// </summary>
        public SiteReporter(List<Page> pages, Uri root) {
            this.pages = pages;
            this.root = root;
        }

        /// <summary>
        /// Report the pages
        /// </summary>
        /// <param name="outputFolder"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Report(DirectoryInfo outputFolder) {
            var fileName = root.Host + "." + root.LocalPath.Replace("/", ".").Replace("..", ".").Trim('.') + ".xlsx";
            var filePath = Path.Combine(outputFolder.FullName, fileName);
            using var writer = new ExcelWriter(filePath);
            WriteHeader(writer);
            foreach (Page page in pages) {
                if (page.References == 0) {
                    WriteNotReferecedPage(writer, page);
                }
                foreach(var reference in page.OutgoingReferences) {
                    WriteReference(writer, reference);
                }
            }
            
        }

        private void WriteHeader(ExcelWriter writer) {
            writer.WriteHeader<RowData>();
            writer.NextRecord();
        }

        private void WriteNotReferecedPage(ExcelWriter writer, Page page) {
            var data = new RowData(root, page);
            writer.WriteRecord(data);
            writer.NextRecord();
        }

        private void WriteReference(ExcelWriter writer, Page.Reference reference) {
            var data = new RowData(root, reference);
            writer.WriteRecord(data);
            writer.NextRecord();
        }
    }
}
