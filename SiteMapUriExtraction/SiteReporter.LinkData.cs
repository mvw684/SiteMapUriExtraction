// Copyright Mark J. van Wijk 2023

using ClosedXML.Excel;

using DocumentFormat.OpenXml.Vml.Office;

namespace SiteMapUriExtractor {

    public partial class SiteReporter {

        /// <summary>
        /// Public record for writing excel data
        /// </summary>
        public class LinkData {

            /// <summary>See property name</summary>
            public string SourceTitle { get; init; }

            /// <summary>See property name</summary>
            public string SourceRelativeUri { get; init; }

            /// <summary>See property name</summary>
            public Uri SourceUri { get; init; }

            /// <summary>See property name</summary>
            public string LinkTitle { get; init; }

            /// <summary>
            /// The type of reference
            /// </summary>
            public string ReferenceType { get; init; }

            /// <summary>See property name</summary>
            public string Comment { get; init; }

            /// <summary>See property name</summary>
            public string TargetTitle { get; init; }

            /// <summary>See property name</summary>
            public string TargetRelativeUri { get; init; }

            /// <summary>See property name</summary>
            public Uri TargetUri { get; init; }

            internal LinkData(Uri root, Page notReferencedPage) {
                SourceTitle = string.Empty;
                SourceRelativeUri = string.Empty;
                SourceUri = new Uri("", UriKind.Relative);
                LinkTitle = string.Empty;
                TargetTitle = notReferencedPage.PageTitle;
                TargetUri = notReferencedPage.Uri;
                Comment = "Not linked from other pages";
                TargetRelativeUri = GetRelative(root, notReferencedPage.Uri);
                ReferenceType = "Missing";
            }

            internal LinkData(Uri root, Reference reference) {
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
                ReferenceType = reference.ReferenceType;
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
                row.Cell(column++).SetValue("Source Title").CreateComment().AddText("Title of the source page");
                row.Cell(column++).SetValue("Source Relative URI").CreateComment().AddText("Relative URI of the source page");
                row.Cell(column++).SetValue("Source Uri").CreateComment().AddText("Full URI of the source page");
                row.Cell(column++).SetValue("Link Title").CreateComment().AddText("Clickable text used for this link in the source page");
                row.Cell(column++).SetValue("Link Type").CreateComment().AddText("Type of link, text, menu, page list");
                row.Cell(column++).SetValue("Comment").CreateComment()
                    .AddText("Comment on the link").AddNewLine()
                    .AddText("To External: Not in the site_map").AddNewLine()
                    .AddText("Link does not exist: Not reachable");
                row.Cell(column++).SetValue("Target Title").CreateComment().AddText("Title of the target page");
                row.Cell(column++).SetValue("Target Relative URI").CreateComment().AddText("Relative URI of the target page");
                row.Cell(column++).SetValue("Target URI").CreateComment().AddText("Full URI of the target page");
            }

            internal void WriteRecord(IXLWorksheet sheet, int rowNumber) {
                var row = sheet.Row(rowNumber);

                int column = 1;
                row.Cell(column++).SetLink(SourceUri, SourceTitle);
                row.Cell(column++).SetValue(SourceRelativeUri);
                row.Cell(column++).SetLink(SourceUri);
                row.Cell(column++).SetLink(TargetUri, LinkTitle);
                row.Cell(column++).SetValue(ReferenceType);
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

                // reference type
                sheet.Column(5).Width = 13;
                sheet.Column(5).Style.Alignment.SetShrinkToFit(true);

                // comment
                sheet.Column(6).Width = 13;
                sheet.Column(6).Style.Alignment.SetShrinkToFit(true);

                // target title
                sheet.Column(7).AdjustToContents(10d, 50d);

                // target relative uri
                sheet.Column(8).Width = 50;
                sheet.Column(8).Style.Alignment.SetShrinkToFit(true);

                // target uri
                sheet.Column(9).Width = 12;
            }

            
        }
    }
}
