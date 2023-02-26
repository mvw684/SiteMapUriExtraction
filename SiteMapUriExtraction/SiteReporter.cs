// Copyright Mark J. van Wijk 2023

using ClosedXML.Excel;

namespace SiteMapUriExtractor {

    /// <summary>
    /// Report the identified pages and references
    /// </summary>
    public partial class SiteReporter {

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
            var rootFileName = root.Host + "." + root.LocalPath.Replace("/", ".").Replace("..", ".").Trim('.');
            var backup = rootFileName + ".bak.xlsx";
            var fileName = rootFileName + ".xlsx";

            var backupPath = Path.Combine(outputFolder.FullName, backup);
            var filePath = Path.Combine(outputFolder.FullName, fileName);

            if (File.Exists(backupPath) && File.Exists(filePath)) {
                File.Delete(backupPath);
            }
            if (File.Exists(filePath)) {
                File.Move(filePath, backupPath);
            }

            using var workBook = new XLWorkbook();

            var pagesSheet = workBook.Worksheets.Add("Pages");
            var pagesData = GetPageData();
            ReportPages(pagesSheet, pagesData);
            PageData.FormatPages(pagesSheet);

            var pageLinksSheet = workBook.Worksheets.Add("Page Links");

            ReportPageLinks(pageLinksSheet, pagesData);
            PageData.FormatPageLinks(pageLinksSheet);

            var linksSheet = workBook.Worksheets.Add("All Links");
            ReportLinks(linksSheet);
            LinkData.Format(linksSheet);
            workBook.SaveAs(filePath);

        }

        private List<PageData> GetPageData() {
            List<PageData> pagesData = new List<PageData>();
            Dictionary<Uri, PageData> byUri = new();
            foreach (Page page in pages) {
                var data = new PageData(root, page);
                pagesData.Add(data);
                byUri.Add(page.Uri, data);
            }

            foreach (Page page in pages) {
                if (!byUri.TryGetValue(page.Uri, out var data)) {
                    continue;
                }
                foreach(var reference in page.OutgoingReferences) {
                    if (reference.HasTargetPage) {
                        var targetPage = reference.TargetPage;
                        if (!byUri.TryGetValue(targetPage.Uri, out var targetData)) {
                            continue;
                        }
                        if (page.Uri == targetPage.Uri) {
                            continue;
                        }
                        data.AddTarget(reference.ReferenceType, targetData);
                    }
                }
            }

            return pagesData;
        }


        private void ReportPages(IXLWorksheet sheet, List<PageData> pagesData) {
            PageData.WritePageHeader(sheet);
            int row = 2;
            foreach (var pageData in pagesData) {
                pageData.WritePageRecord(sheet, row++);
            }
        }

        private void ReportPageLinks(IXLWorksheet sheet, List<PageData> pagesData) {
            PageData.WritePageLinksHeader(sheet);
            int row = 2;
            foreach (var pageData in pagesData) {
                foreach (var referencedPage in pageData.ReferencedPages) {
                    pageData.WritePageLinksRecord(sheet, row++, referencedPage);
                }
            }
        }

        private void ReportLinks(IXLWorksheet sheet) {
            LinkData.WriteHeader(sheet);
            int row = 2;
            foreach (Page page in pages) {
                if (page.References == 0) {
                    WriteNotReferecedPage(sheet, page, row++);
                }
                foreach (var reference in page.OutgoingReferences) {
                    WriteReference(sheet, reference, row++);
                }
            }
        }

        private void WriteNotReferecedPage(IXLWorksheet sheet, Page page, int row) {
            var data = new LinkData(root, page);
            data.WriteRecord(sheet, row);
        }

        private void WriteReference(IXLWorksheet sheet, Reference reference, int row) {
            var data = new LinkData(root, reference);
            data.WriteRecord(sheet, row);
        }
    }
}
