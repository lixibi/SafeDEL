using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SafeDeleteLibrary
{
    /// <summary>
    /// Provides secure file and directory deletion functionality using multiple overwrite patterns
    /// </summary>
    public static class SafeDelete
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private static readonly SemaphoreSlim _workerSemaphore = new SemaphoreSlim(Constants.MaxWorkers, Constants.MaxWorkers);

        // DoD 5220.22-M standard erasure patterns
        private static readonly ErasePattern[] _dodPatterns = {
            ErasePattern.CreatePattern(0x00, Constants.BlockSize), // All zeros
            ErasePattern.CreatePattern(0xFF, Constants.BlockSize), // All ones
            ErasePattern.CreateRandomPattern()                     // Random data
        };

        // Simplified Gutmann patterns
        private static readonly ErasePattern[] _gutmannPatterns = {
            ErasePattern.CreatePattern(0x55, Constants.BlockSize), // 01010101
            ErasePattern.CreatePattern(0xAA, Constants.BlockSize), // 10101010
            ErasePattern.CreatePattern(0x92, Constants.BlockSize), // 10010010
            ErasePattern.CreatePattern(0x49, Constants.BlockSize), // 01001001
            ErasePattern.CreatePattern(0x00, Constants.BlockSize), // All zeros
            ErasePattern.CreatePattern(0xFF, Constants.BlockSize), // All ones
            ErasePattern.CreateRandomPattern()                     // Random data
        };

        /// <summary>
        /// Securely deletes a single file using multiple overwrite patterns
        /// </summary>
        /// <param name="filePath">Path to the file to delete</param>
        /// <exception cref="ArgumentException">Thrown when filePath is null or empty</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist</exception>
        /// <exception cref="SafeDeleteException">Thrown when deletion fails</exception>
        public static void SecureDeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}", filePath);

            try
            {
                SecureDeleteFileInternal(filePath);
            }
            catch (Exception ex) when (!(ex is SafeDeleteException))
            {
                throw new SafeDeleteException($"Failed to securely delete file: {ex.Message}", filePath, "SecureDeleteFile", ex);
            }
        }

        /// <summary>
        /// Asynchronously secures deletes a single file using multiple overwrite patterns
        /// </summary>
        /// <param name="filePath">Path to the file to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="ArgumentException">Thrown when filePath is null or empty</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist</exception>
        /// <exception cref="SafeDeleteException">Thrown when deletion fails</exception>
        public static async Task SecureDeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}", filePath);

            try
            {
                await Task.Run(() => SecureDeleteFileInternal(filePath), cancellationToken);
            }
            catch (Exception ex) when (!(ex is SafeDeleteException))
            {
                throw new SafeDeleteException($"Failed to securely delete file: {ex.Message}", filePath, "SecureDeleteFileAsync", ex);
            }
        }

        /// <summary>
        /// Securely deletes a directory and all its contents using multiple overwrite patterns
        /// </summary>
        /// <param name="directoryPath">Path to the directory to delete</param>
        /// <exception cref="ArgumentException">Thrown when directoryPath is null or empty</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist</exception>
        /// <exception cref="SafeDeleteException">Thrown when deletion fails</exception>
        public static void SecureDeleteDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("Directory path cannot be null or empty", nameof(directoryPath));

            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            try
            {
                SecureDeleteDirectoryInternal(directoryPath);
            }
            catch (Exception ex) when (!(ex is SafeDeleteException))
            {
                throw new SafeDeleteException($"Failed to securely delete directory: {ex.Message}", directoryPath, "SecureDeleteDirectory", ex);
            }
        }

        /// <summary>
        /// Asynchronously secures deletes a directory and all its contents using multiple overwrite patterns
        /// </summary>
        /// <param name="directoryPath">Path to the directory to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="ArgumentException">Thrown when directoryPath is null or empty</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist</exception>
        /// <exception cref="SafeDeleteException">Thrown when deletion fails</exception>
        public static async Task SecureDeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("Directory path cannot be null or empty", nameof(directoryPath));

            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            try
            {
                await Task.Run(() => SecureDeleteDirectoryInternal(directoryPath), cancellationToken);
            }
            catch (Exception ex) when (!(ex is SafeDeleteException))
            {
                throw new SafeDeleteException($"Failed to securely delete directory: {ex.Message}", directoryPath, "SecureDeleteDirectoryAsync", ex);
            }
        }

        private static void SecureDeleteFileInternal(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var fileSize = fileInfo.Length;

            // Securely overwrite filename
            var currentPath = SecureOverwriteName(filePath);

            // Open file for overwriting
            using var fileStream = new FileStream(currentPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            
            var writeSize = fileSize + Constants.ExtraSize;

            // Apply all erasure patterns
            ApplyPatterns(fileStream, writeSize, _dodPatterns);
            ApplyPatterns(fileStream, writeSize, _gutmannPatterns);
            RandomOverwrite(fileStream, writeSize);

            // Truncate to original size
            fileStream.SetLength(fileSize);
            fileStream.Flush();
            fileStream.Close();

            // Delete the file
            File.Delete(currentPath);
        }

        private static void SecureDeleteDirectoryInternal(string directoryPath)
        {
            // Get all files in directory recursively
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories).ToList();

            if (files.Count == 0)
            {
                // Empty directory - just rename and delete
                var newPath = SecureOverwriteName(directoryPath);
                Directory.Delete(newPath, true);
                return;
            }

            // Process files concurrently
            var tasks = files.Select(async filePath =>
            {
                await _workerSemaphore.WaitAsync();
                try
                {
                    SecureDeleteFileInternal(filePath);
                }
                finally
                {
                    _workerSemaphore.Release();
                }
            });

            Task.WaitAll(tasks.ToArray());

            // Delete directories from bottom up
            var directories = Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories)
                .OrderByDescending(d => d.Length) // Longest paths first (deepest directories)
                .ToList();

            directories.Add(directoryPath); // Add root directory last

            foreach (var dir in directories)
            {
                try
                {
                    var newPath = SecureOverwriteName(dir);
                    Directory.Delete(newPath, false); // Don't recurse since we're going bottom-up
                }
                catch
                {
                    // If renaming fails, try to delete with original name
                    Directory.Delete(dir, false);
                }
            }
        }

        private static string SecureOverwriteName(string path)
        {
            var directory = Path.GetDirectoryName(path) ?? "";
            var currentPath = path;

            for (int i = 0; i < Constants.NameOverwriteCount; i++)
            {
                var randomName = GenerateRandomName();
                var randomExtension = GenerateRandomExtension();
                var newPath = Path.Combine(directory, randomName + randomExtension);

                if (File.Exists(currentPath))
                {
                    File.Move(currentPath, newPath);
                }
                else if (Directory.Exists(currentPath))
                {
                    Directory.Move(currentPath, newPath);
                }
                else
                {
                    throw new SafeDeleteException($"Path does not exist: {currentPath}", currentPath, "SecureOverwriteName");
                }

                currentPath = newPath;
            }

            return currentPath;
        }

        private static string GenerateRandomName()
        {
            var bytes = new byte[16];
            _rng.GetBytes(bytes);
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        private static string GenerateRandomExtension()
        {
            var bytes = new byte[4];
            _rng.GetBytes(bytes);
            var index = Math.Abs(BitConverter.ToInt32(bytes, 0)) % Constants.CommonExtensions.Length;
            return Constants.CommonExtensions[index];
        }

        private static byte[] GenerateRandomData(int size)
        {
            var data = new byte[size];
            _rng.GetBytes(data);
            return data;
        }

        private static void ApplyPatterns(FileStream fileStream, long writeSize, ErasePattern[] patterns)
        {
            foreach (var pattern in patterns)
            {
                fileStream.Seek(0, SeekOrigin.Begin);

                var remaining = writeSize;
                while (remaining > 0)
                {
                    var size = (int)Math.Min(Constants.BlockSize, remaining);
                    byte[] data;

                    if (pattern.IsRepeated && pattern.Data != null)
                    {
                        // Use fixed pattern
                        data = size == Constants.BlockSize ? pattern.Data : pattern.Data.Take(size).ToArray();
                    }
                    else
                    {
                        // Generate random data
                        data = GenerateRandomData(size);
                    }

                    fileStream.Write(data, 0, data.Length);
                    remaining -= size;
                }

                fileStream.Flush();
            }
        }

        private static void RandomOverwrite(FileStream fileStream, long writeSize)
        {
            fileStream.Seek(0, SeekOrigin.Begin);

            var remaining = writeSize;
            while (remaining > 0)
            {
                var size = (int)Math.Min(Constants.BlockSize, remaining);
                var data = GenerateRandomData(size);

                fileStream.Write(data, 0, data.Length);
                remaining -= size;
            }

            fileStream.Flush();
        }

        /// <summary>
        /// Releases resources used by the SafeDelete class
        /// </summary>
        public static void Dispose()
        {
            _rng?.Dispose();
            _workerSemaphore?.Dispose();
        }
    }
}
