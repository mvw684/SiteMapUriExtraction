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
            if (args.Length == 0) {
                Console.WriteLine(Process.GetCurrentProcess().ProcessName + "[--sitemap <sitemap uri>]+ -OutputFolder folder name");
            } else {
                List<string> sitemaps = new List<string>();
                string outputFolder = string.Empty;
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
                        default:
                            Console.WriteLine($"Unknown command-line argument: {arg}");
                            return -2;
                    }
                }
                Execute(sitemaps, outputFolder);

            }
            return 0;
        }

        private static void Execute(List<string> sitemaps, string outputFolder) {
            var sitemapReader = new SitemapReader();
            sitemapReader.Load(sitemaps);
            var uris = sitemapReader.GetActualSiteUrisFromSitemaps();

        }
    }
}

