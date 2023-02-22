// Copyright Mark J. van Wijk 2023

using ClosedXML.Excel;

namespace SiteMapUriExtractor {

    public partial class SiteReporter {
        /// <summary>
        /// Public record for writing excel data
        /// </summary>
        public class RowData {

            internal RowData(Uri root, Page notReferencedPage) {
                SourceTitle = string.Empty;
                SourceRelativeUri = string.Empty;
                SourceUri = new Uri("", UriKind.Relative);
                LinkTitle = string.Empty;
                TargetTitle = notReferencedPage.PageTitle;
                TargetUri = notReferencedPage.Uri;
                Comment = "Not linked from other pages";
                TargetRelativeUri = GetRelative(root, notReferencedPage.Uri);
            }

            internal RowData(Uri root, Reference reference) {
                SourceTitle = reference.SourceTitle;
                SourceRelativeUri = GetRelative(root, reference.Source);
                SourceUri = reference.Source;
                LinkTitle = reference.Name;
                TargetTitle = reference.TargetTitle;
                TargetRelativeUri = GetRelative(root, reference.Target);
                TargetUri = reference.Target;
                if (!reference.Exists) {
                    Comment = "Link does not exist";
                } else if (!reference.HasTargetPage) {
                    Comment = "To External";
                } else {
                    Comment = string.Empty;
                }
            }

            private string GetRelative(Uri root, Uri uri) {
                string relativeUri;
                var fullRoot = root.AbsoluteUri;
                var fullTarget = uri.AbsoluteUri;
                if (fullTarget.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase)) {
                    relativeUri = fullTarget.Substring(fullRoot.Length).Trim('/');
                    if (string.IsNullOrEmpty(relativeUri)) {
                        relativeUri = "/";
                    }
                } else {
                    relativeUri = "<External>";
                }
                return relativeUri;
            }

            internal static void WriteHeader(IXLWorksheet sheet) {
                var row = sheet.Row(1);
                int column = 1;
                row.Cell(column++).SetValue("Source Title");
                row.Cell(column++).SetValue("Source Relative URI");
                row.Cell(column++).SetValue("Source Uri");
                row.Cell(column++).SetValue("Link Title");
                row.Cell(column++).SetValue("Comment");
                row.Cell(column++).SetValue("Target Title");
                row.Cell(column++).SetValue("Target Relative URI");
                row.Cell(column++).SetValue("Target URI");
            }

            internal void WriteRecord(IXLWorksheet sheet, int rowNumber) {
                var row = sheet.Row(rowNumber);

                int column = 1;
                row.Cell(column++).SetLink(SourceUri, SourceTitle);
                row.Cell(column++).SetValue(SourceRelativeUri);
                row.Cell(column++).SetLink(SourceUri);
                row.Cell(column++).SetLink(TargetUri, LinkTitle);
                row.Cell(column++).SetValue(Comment);
                row.Cell(column++).SetLink(TargetUri, TargetTitle);
                row.Cell(column++).SetValue(TargetRelativeUri);
                row.Cell(column++).SetLink(TargetUri);
            }

            internal static void Format(IXLWorksheet sheet) {
                sheet.Row(1).SetAutoFilter(true);
                sheet.SheetView.FreezeRows(1);

                // source title
                sheet.Column(1).AdjustToContents(10d, 50d);

                // source relative uri
                sheet.Column(2).Width = 50;
                sheet.Column(2).Style.Alignment.SetShrinkToFit(true);

                // uri 
                sheet.Column(3).Width = 12;

                // link title
                sheet.Column(4).Width = 40;
                sheet.Column(4).Style.Alignment.SetShrinkToFit(true);

                // comment
                sheet.Column(5).Width = 13;
                sheet.Column(5).Style.Alignment.SetShrinkToFit(true);

                // target title
                sheet.Column(6).AdjustToContents(10d, 50d);

                // target relative uri
                sheet.Column(7).Width = 50;
                sheet.Column(7).Style.Alignment.SetShrinkToFit(true);

                // target uri
                sheet.Column(8).Width = 12;
            }

            /// <summary>See property name</summary>
            public string SourceTitle { get; init; }

            /// <summary>See property name</summary>
            public string SourceRelativeUri { get; init; }

            /// <summary>See property name</summary>
            public Uri SourceUri { get; init; }

            /// <summary>See property name</summary>
            public string LinkTitle { get; init; }

            /// <summary>See property name</summary>
            public string Comment { get; init; }

            /// <summary>See property name</summary>
            public string TargetTitle { get; init; }

            /// <summary>See property name</summary>
            public string TargetRelativeUri { get; init; }

            /// <summary>See property name</summary>
            public Uri TargetUri { get; init; }
        }
    }
}
