using System.Diagnostics;

namespace FolderCompare
{
    class CompareStatus
    {
        public int Missing { get; set; }
        public int Different { get; set; }
        public int NoAccess { get; set; }
        public int DirectoryNoAccess { get; set; }
        public int Total { get; set; }
        public long BytesRead { get; set; }
        public Stopwatch Stopwatch { get; } = new Stopwatch();
    }
}
