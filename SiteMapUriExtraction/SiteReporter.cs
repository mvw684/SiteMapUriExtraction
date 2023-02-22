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

            if (File.Exists(backupPath)) {
                File.Delete(backupPath);
            }
            if (File.Exists(filePath)) {
                File.Move(filePath, backupPath);
            }

            using var workBook = new XLWorkbook();

            var pagesSheet = workBook.Worksheets.Add("Pages");
            var linksSheet = workBook.Worksheets.Add("Links");
            ReportLinks(linksSheet);
            ReportPages(pagesSheet);
            RowData.Format(linksSheet);
            PageData.Format(pagesSheet);
            workBook.SaveAs(filePath);

        }

        private void ReportPages(IXLWorksheet sheet) {
            List<PageData> pageDatas = new List<PageData>();
            foreach(Page page in pages) {
                pageDatas.Add(new PageData(page));
            }
            PageData.WriteHeader(sheet);
            int row = 2;
            foreach(var pageData in pageDatas) {
                pageData.WriteRecord(sheet, row++);
            }
        }

        private void ReportLinks(IXLWorksheet sheet) {
            RowData.WriteHeader(sheet);
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
            var data = new RowData(root, page);
            data.WriteRecord(sheet, row);
        }

        private void WriteReference(IXLWorksheet sheet, Reference reference, int row) {
            var data = new RowData(root, reference);
            data.WriteRecord(sheet, row);
        }
    }
}
