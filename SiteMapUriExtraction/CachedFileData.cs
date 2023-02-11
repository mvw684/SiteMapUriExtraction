// Copyright Mark J. van Wijk 2023


using System.Diagnostics.CodeAnalysis;

namespace SiteMapUriExtractor {


    /// <summary>
    /// Folder and name for a file caching retrieved data
    /// </summary>
    public class CachedFileData {
        private readonly DirectoryInfo folder;
        private readonly string fileName;
        private string? extension;
        private FileInfo? file;

        /// <summary>
        /// Constructor with related data
        /// </summary>
        public CachedFileData(string folderPath, string fileName, string? extension) {
            folder = new DirectoryInfo(folderPath);
            this.fileName = fileName;
            this.extension = extension;
            folder.Create();
            if (extension is null) {
                var files = folder.GetFiles(fileName + ".*");
                if (files.Length == 1) {
                    file = files[0];
                    this.extension = file.Extension;
                }
            } else {
                Extension = extension;
            }
        }

        /// <summary>
        /// Extension can be known later, e.g. from the http get request result headers / mime type
        /// </summary>
        public string? Extension {
            get => extension;
            [MemberNotNull(nameof(File), nameof(extension))]
            set {
                if (
                    File is not null &&
                    value is not null &&
                    StringComparer.OrdinalIgnoreCase.Equals(extension, value)
                ) {
                    return;
                }
                extension = value ?? throw new ArgumentNullException(nameof(value));
                File = new FileInfo(Path.Combine(folder.FullName, fileName + extension));
            }
        }

        /// <summary>
        /// Current cached <see cref="FileInfo"/> (can be null if extension is not yet known)
        /// </summary> 
        public FileInfo? File {
            get => file;
            private set => file = value;
        }

        /// <summary>
        /// Last modification time of the cached file, <see cref="DateTime.MinValue"/> is not (yet) cached.
        /// </summary>
        public DateTime LastWriteTime => Exists ? file.LastWriteTime : DateTime.MinValue;

        /// <summary>
        /// File exists
        /// </summary>
        [MemberNotNullWhen(true, nameof(file), nameof(File))]
        public bool Exists => file is not null && file.Exists;
    }
}
