using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace FolderCompare
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || (args.Length == 3 && args[2] != "/slice") || args.Length > 3)
            {
                Usage();
            }

            var slice = args.Length == 3;
            var sourceFolder = args[0];
            var destinationFolder = args[1];
            CheckFolderExists(sourceFolder);
            CheckFolderExists(destinationFolder);

            int? skip = null, take = null;
            if (slice)
            {
                Console.WriteLine("Counting files to determine current slice");
                var count = FileAccess.GetFiles(new CompareStatus(), sourceFolder).Count();
                var dayNumber = (int)DateTime.Today.DayOfWeek;
                skip = dayNumber * count / 7;
                take = count / 7 + 1;
                Console.WriteLine($"Comparing {take:n0} files starting on index {skip:n0}");
            }

            CompareStatus status = new CompareStatus();
            status.Stopwatch.Start();
            using (var timer = new Timer(new TimerCallback((s) => PrintStatus(s)), status, 5000, 5000))
            {                
                CompareFolders(status, sourceFolder, destinationFolder, skip, take);                
            }
            status.Stopwatch.Stop();
            Console.WriteLine($"Completed in: {status.Stopwatch.Elapsed}; Total size: {status.BytesRead:n0}; Total files: {status.Total}; Missing: {status.Missing}; Different: {status.Different}");

            Environment.Exit((int)
                (ReturnCodes.NoErrors | 
                (status.Missing > 0 ? ReturnCodes.MissingFile : ReturnCodes.NoErrors) | 
                (status.Different > 0 ? ReturnCodes.DifferentFile : ReturnCodes.NoErrors) |
                (status.DirectoryNoAccess > 0 ? ReturnCodes.DirectoryNoAccess: ReturnCodes.NoErrors) |
                (status.NoAccess > 0 ? ReturnCodes.NoAccess : ReturnCodes.NoErrors)));
        }

        private static void PrintStatus(object state)
        {
            var status = (CompareStatus)state;
            Console.WriteLine($"{status.BytesRead / 1024 / 1024:n0} MB read; Speed: {status.BytesRead / status.Stopwatch.Elapsed.TotalSeconds / 1000:n0} KB/s");
        }

        private static void CompareFolders(CompareStatus status, string sourceFolder, string destinationFolder, int? skip, int? take)
        {
            var files = FileAccess.GetFiles(status, sourceFolder);
            files = skip.HasValue && take.HasValue ? files.Skip(skip.Value).Take(take.Value) : files;
            foreach (var sourceFile in files)
            {
                var destinationFile = sourceFile.Replace(sourceFolder, destinationFolder);
                status.Total++;

                if (!File.Exists(destinationFile))
                {
                    Console.WriteLine("Missing: " + destinationFile);
                    status.Missing++;
                    continue;
                }

                if (!CompareFiles(status, sourceFile, destinationFile))
                {
                    Console.WriteLine("File is different: " + destinationFile);
                    status.Different++;
                }
            }
        }

        private static bool CompareFiles(CompareStatus status, string sourceFile, string destinationFile)
        {
            var blockSize = 64 * 1024;
            var sourceBuffer = new byte[blockSize];
            var destinationBuffer = new byte[blockSize];

            using (var sourceStream = FileAccess.OpenFile(sourceFile))
            using (var destinationStream = FileAccess.OpenFile(destinationFile))
            {
                if (sourceStream == null || destinationStream == null)
                {
                    status.NoAccess++;
                }
                else
                {
                    while (true)
                    {
                        var sourceRead = sourceStream.ReadAsync(sourceBuffer, 0, blockSize);
                        var destinationRead = destinationStream.ReadAsync(destinationBuffer, 0, blockSize);
                        if (sourceRead.Result == 0)
                        {
                            break;
                        }

                        status.BytesRead += sourceRead.Result;
                        if (sourceRead.Result != destinationRead.Result)
                        {
                            return false;
                        }

                        if (!ArrayCompare.CompareArraysUnsafe(sourceBuffer, destinationBuffer, sourceBuffer.Length))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }        

        private static void CheckFolderExists(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Console.Error.WriteLine("Folder does not exist: " + folder);
                Environment.Exit((int)ReturnCodes.InvalidArguments);
            }
        }

        private static void Usage()
        {
            Console.Error.WriteLine("FolderCompare.exe <folder1> <folder2> [/slice]");
            Console.WriteLine("  Return codes (ORed bits)");
            foreach (var returnCode in Enum.GetValues(typeof(ReturnCodes)))
            {
                Console.Error.WriteLine($"  -- {returnCode}: {(int)returnCode}");
            }
            
            Environment.Exit((int)ReturnCodes.InvalidArguments);
        }
    }
}