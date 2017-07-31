using System;
using System.Collections.Generic;
using System.IO;

namespace FolderCompare
{
    static class FileAccess
    {
        /// <summary>
        /// Get files from directory recursively
        /// </summary>
        public static IEnumerable<string> GetFiles(CompareStatus status, string folder)
        {
            string[] files = null;
            string[] directories = null;
            try
            {
                files = Directory.GetFiles(folder);
                directories = Directory.GetDirectories(folder);
            }
            catch
            {
                Console.WriteLine("No access to directory: " + folder);
                status.DirectoryNoAccess++;
                yield break;
            }

            foreach (var file in files)
            {
                yield return file;
            }
            foreach (var directory in directories)
            {
                foreach (var file in GetFiles(status, directory))
                {
                    yield return file;
                }
            }
        }

        public static Stream OpenFile(string filename)
        {
            try
            {
                return File.OpenRead(filename);
            }
            catch
            {
                Console.WriteLine("No access to file: " + filename);
                return null;
            }
        }
    }
}
