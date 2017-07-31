using System;

namespace FolderCompare
{
    [Flags]
    public enum ReturnCodes
    {
        NoErrors = 0,
        MissingFile = 1,
        DifferentFile = 1 << 1,
        NoAccess = 1 << 2,
        DirectoryNoAccess = 1 << 3,
        InvalidArgumentFolderDoesNotExist = 1 << 4,
        InvalidArguments = 1 << 5
    }
}
