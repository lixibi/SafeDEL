namespace SafeDeleteLibrary
{
    /// <summary>
    /// Constants used throughout the SafeDelete library
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Size of each write block in bytes (8KB)
        /// </summary>
        public const int BlockSize = 8192;

        /// <summary>
        /// Additional bytes to write beyond the original file size (4KB)
        /// </summary>
        public const int ExtraSize = 4096;

        /// <summary>
        /// Maximum number of concurrent workers for directory processing
        /// </summary>
        public const int MaxWorkers = 5;

        /// <summary>
        /// Number of times to overwrite the filename before deletion
        /// </summary>
        public const int NameOverwriteCount = 3;

        /// <summary>
        /// Common file extensions used for filename obfuscation
        /// </summary>
        public static readonly string[] CommonExtensions = {
            ".txt", ".doc", ".docx", ".pdf", ".xls", ".xlsx", ".ppt", ".pptx",
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".svg", ".psd",
            ".mp3", ".wav", ".wma", ".aac", ".ogg", ".flac", ".m4a", ".mid",
            ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm", ".m4v",
            ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".iso", ".cab",
            ".exe", ".dll", ".sys", ".msi", ".bat", ".cmd", ".reg", ".ini",
            ".html", ".htm", ".css", ".js", ".xml", ".json", ".yaml", ".sql",
            ".php", ".py", ".rb", ".sh", ".c", ".cpp", ".h", ".hpp", ".java",
            ".class", ".jar", ".war", ".ear", ".go", ".rs", ".swift", ".kt",
            ".apk", ".ipa", ".app", ".deb", ".rpm", ".pkg", ".dmg"
        };
    }
}
