// Copyright Mark J. van Wijk 2023

using CsvHelper.Excel;

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
            var linksFileName = rootFileName + ".links.xlsx";
            var pagesFileName = rootFileName + ".pages.xlsx";
            var linksFilePath = Path.Combine(outputFolder.FullName, linksFileName);
            var pagesFilePath = Path.Combine(outputFolder.FullName, pagesFileName);

            ReportLinks(linksFilePath);
            ReportPages(pagesFilePath);
        }

        private void ReportPages(string pagesFilePath) {
            if (File.Exists(pagesFilePath)) {
                File.Delete(pagesFilePath);
            }
            using var writer = new ExcelWriter(pagesFilePath, "Pages");
            List<PageData> pageDatas = new List<PageData>();
            foreach(Page page in pages) {
                pageDatas.Add(new PageData(page));
            }
            PageData.WriteHeader(writer);
            foreach(var pageData in pageDatas) {
                pageData.WriteRecord(writer);
            }
        }

        private void ReportLinks(string linksFilePath) {
            if (File.Exists(linksFilePath)) {
                File.Delete(linksFilePath);
            }
            using var writer = new ExcelWriter(linksFilePath, "Links");
            WriteHeader(writer);
            foreach (Page page in pages) {
                if (page.References == 0) {
                    WriteNotReferecedPage(writer, page);
                }
                foreach (var reference in page.OutgoingReferences) {
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
