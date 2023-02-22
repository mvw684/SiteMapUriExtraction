﻿// Copyright Mark J. van Wijk 2023

using ClosedXML.Excel;

namespace SiteMapUriExtractor {

    public partial class SiteReporter {
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

            private static RowData MakeHeader() {
                return new RowData() {
                    SourceTitle = "Source Title",
                    SourceRelativeUri = "Source Relative URI",
                    SourceUri = "Source URI",
                    LinkTitle = "Link Title",
                    TargetTitle = "Target Title",
                    TargetRelativeUri = "Target Relative URI",
                    TargetUri = "Target URI",
                    Comment = "Comment"
                };
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

            internal static void WriteHeader(IXLWorksheet sheet) {
                MakeHeader().WriteRecord(sheet, 1);
            }

            internal void WriteRecord(IXLWorksheet sheet, int rowNumber) {
                var row = sheet.Row(rowNumber);

                int column = 1;
                row.Cell(column++).SetFormulaA1($"=HYPERLINK(\"{SourceUri}\",\"{SourceTitle}\")");
                row.Cell(column++).SetValue(SourceRelativeUri);
                row.Cell(column++).SetValue(SourceUri);
                row.Cell(column++).SetFormulaA1($"=HYPERLINK(\"{SourceUri}\",\"{LinkTitle}\")");
                row.Cell(column++).SetValue(Comment);
                row.Cell(column++).SetFormulaA1($"=HYPERLINK(\"{SourceUri}\",\"{TargetTitle}\")");
                row.Cell(column++).SetValue(TargetRelativeUri);
                row.Cell(column++).SetValue(TargetUri);
            }

            /// <summary>See property name</summary>
            public string SourceTitle { get; init; }

            /// <summary>See property name</summary>
            public string SourceRelativeUri { get; init; }

            /// <summary>See property name</summary>
            public string SourceUri { get; init; }

            /// <summary>See property name</summary>
            public string LinkTitle { get; init; }

            /// <summary>See property name</summary>
            public string Comment { get; init; }

            /// <summary>See property name</summary>
            public string TargetTitle { get; init; }

            /// <summary>See property name</summary>
            public string TargetRelativeUri { get; init; }

            /// <summary>See property name</summary>
            public string TargetUri { get; init; }
        }
    }
}
