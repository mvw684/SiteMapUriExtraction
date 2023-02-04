// Copyright Mark J. van Wijk 2023

using System.Diagnostics;

namespace SiteMapUriExtractor {
    /// <summary>
    /// Main exe entry point, handling command line parsing and actual logic invocation
    /// </summary>
    public class Program {

        /// <summary>
        /// Standard main of the executable
        /// </summary>
        public static int Main(string[] args) {
            if (
                args.Length == 0 ||
                args.Any(
                    a =>
                        a.Equals("-?") ||
                        a.StartsWith("-h", StringComparison.OrdinalIgnoreCase) ||
                        a.StartsWith("--h", StringComparison.OrdinalIgnoreCase)
                )
            ) {
                Console.WriteLine(Process.GetCurrentProcess().ProcessName + "[--sitemap <sitemap/siteindex uri>]+ -OutputFolder folder name [--retentionPolicy Header|Day|Week]");
            } else {
                List<string> sitemaps = new List<string>();
                string outputFolder = string.Empty;
                var retention = RetentionPolicy.Day;
                for (int i = 0; i < args.Length; i += 2) {
                    string arg = args[i];
                    string value = string.Empty;
                    if (i + 1 == args.Length) {
                        Console.WriteLine($"Missing value for argument {arg} at position {i + 1}");
                        return -1;
                    } else {
                        value = args[i + 1];
                    }
                    var argUpper = arg.ToUpperInvariant();
                    switch (argUpper) {
                        case "--SITEMAP":
                            sitemaps.Append(value);
                            break;
                        case "--OUTPUTFOLDER":
                            outputFolder = value;
                            break;
                        case "--RETENTIONPOLICY":
                            if (!Enum.TryParse<RetentionPolicy>(value, out retention)) {
                                Console.WriteLine($"Unknown retention policy argument: {value}");
                                return -1;
                            }
                            break;

                        default:
                            Console.WriteLine($"Unknown command-line argument: {arg}");
                            return -1;
                    }
                }
                Execute(sitemaps, outputFolder, retention);

            }
            return 0;
        }

        private static void Execute(List<string> sitemaps, string outputFolderName, RetentionPolicy retention) {
            var outputFolder = new DirectoryInfo(outputFolderName);
            var cache = new UriCache(outputFolder.CreateSubdirectory("Cache"), retention);
            var sitemapReader = new SitemapReader(cache);
            sitemapReader.Load(sitemaps);
            var pages = sitemapReader.GetPagesFromSitemaps();
            foreach (var page in pages) {
                page.load();
            }


        }
    }
}

