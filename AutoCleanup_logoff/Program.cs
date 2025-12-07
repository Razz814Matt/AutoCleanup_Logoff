using System.Runtime.Versioning;
using System.IO;
using System.Collections.Generic;
using System;
using System.Diagnostics; // Required for Debug.WriteLine (optional, but useful)

namespace AutoCleanup_logoff
{
    public static class Cleaner
    {
        // Path for logging errors when running at logoff
        private const string LogFilePath = @"C:\Temp\CleanupLog.txt";

        [SupportedOSPlatform("windows10.0.19041.0")]
        public static void ClearAllData()
        {
            try
            {
                // Ensure the log directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath)!);

                ClearDownloadsFolder();
                ClearEdgeHistoryData();
                ClearChromeHistoryData();

                File.AppendAllText(LogFilePath, $"Cleanup Succeeded at {DateTime.Now}\n");
            }
            catch (Exception ex)
            {
                // This catch only handles errors in the main flow, 
                // but individual cleanup functions should handle specific file errors.
                File.AppendAllText(LogFilePath, $"CRITICAL Cleanup Failure: {ex.Message} at {DateTime.Now}\n");
            }
        }

        // --- Downloads Folder Logic ---

        [SupportedOSPlatform("windows10.0.19041.0")]
        // FIX 1: Added 'static'
        private static string GetDownloadsFolderLocation()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userProfile, "Downloads");
        }

        [SupportedOSPlatform("windows10.0.19041.0")]
        private static void ClearDownloadsFolder()
        {
            string downloadsPath = GetDownloadsFolderLocation();

            if (!Directory.Exists(downloadsPath))
            {
                return;
            }

            try
            {
                // Clear files in the root folder
                string[] files = Directory.GetFiles(downloadsPath);
                foreach (string file in files)
                {
                    try { File.Delete(file); } // FIX 2: Added internal try/catch
                    catch (IOException ex) { Debug.WriteLine($"Failed to delete file: {ex.Message}"); }
                }

                // Clear subdirectories
                string[] directories = Directory.GetDirectories(downloadsPath);
                foreach (string directory in directories)
                {
                    try { Directory.Delete(directory, true); } // FIX 2: Added internal try/catch
                    catch (IOException ex) { Debug.WriteLine($"Failed to delete directory: {ex.Message}"); }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(LogFilePath, $"Downloads Cleanup Failed: {ex.Message} at {DateTime.Now}\n");
                // Do NOT re-throw, allow Edge/Chrome cleanup to run.
            }
        }

        // --- Chrome History/Cache Logic ---

        [SupportedOSPlatform("windows10.0.19041.0")]
        // FIX 1: Added 'static'
        private static string GetChromeDefaultProfilePath()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, @"Google\Chrome\User Data\Default");
        }

        [SupportedOSPlatform("windows10.0.19041.0")]
        private static void ClearChromeHistoryData()
        {
            string profilePath = GetChromeDefaultProfilePath();
            if (!Directory.Exists(profilePath))
            {
                File.AppendAllText(LogFilePath, $"Chrome Profile Not Found at {DateTime.Now}\n");
                return;
            }

            var itemsToDelete = new List<string>
            {
                Path.Combine(profilePath, "Cache"),
                Path.Combine(profilePath, "Code Cache"),
                Path.Combine(profilePath, "History"),
                Path.Combine(profilePath, "Cookies")
            };

            foreach (var itemPath in itemsToDelete)
            {
                try
                {
                    if (Directory.Exists(itemPath))
                    {
                        Directory.Delete(itemPath, true);
                    }
                    else if (File.Exists(itemPath))
                    {
                        File.Delete(itemPath);
                    }
                }
                // FIX 2: Specifically catch file lock errors (like journal.baj) and continue
                catch (IOException ex)
                {
                    Debug.WriteLine($"Failed to delete {itemPath} for Chrome: {ex.Message}");
                    // Do not break the loop or re-throw the error
                }
                catch (Exception ex)
                {
                    File.AppendAllText(LogFilePath, $"Chrome Cleanup Failed on {itemPath}: {ex.Message} at {DateTime.Now}\n");
                }
            }
        }

        // --- EDGE HISTORY/CACHE LOGIC ---

        [SupportedOSPlatform("windows10.0.19041.0")]
        // FIX 1: Added 'static'
        private static string GetEdgeDefaultProfilePath()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default");
        }

        [SupportedOSPlatform("windows10.0.19041.0")]
        private static void ClearEdgeHistoryData()
        {
            string profilePath = GetEdgeDefaultProfilePath();

            if (!Directory.Exists(profilePath))
            {
                File.AppendAllText(LogFilePath, $"Edge Profile Not Found at {DateTime.Now}\n");
                return;
            }

            var itemsToDelete = new List<string>
            {
                Path.Combine(profilePath, "Cache"),
                Path.Combine(profilePath, "Code Cache"),
                Path.Combine(profilePath, "History"),
                Path.Combine(profilePath, "Cookies")
            };

            foreach (var itemPath in itemsToDelete)
            {
                try
                {
                    if (Directory.Exists(itemPath))
                    {
                        Directory.Delete(itemPath, true);
                    }
                    else if (File.Exists(itemPath))
                    {
                        File.Delete(itemPath);
                    }
                }
                // FIX 2: Specifically catch file lock errors (like journal.baj) and continue
                catch (IOException ex)
                {
                    Debug.WriteLine($"Failed to delete {itemPath} for Edge: {ex.Message}");
                    // Do not break the loop or re-throw the error
                }
                catch (Exception ex)
                {
                    File.AppendAllText(LogFilePath, $"Edge Cleanup Failed on {itemPath}: {ex.Message} at {DateTime.Now}\n");
                }
            }
        }
    }

    
    public class Program
    {
        // This is the entry point that the Scheduled Task will run
        public static void Main(string[] args)
        {
            Cleaner.ClearAllData();
        }
    }
}