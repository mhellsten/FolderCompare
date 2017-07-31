using System;

namespace FolderCompare
{
    [Flags]
    public enum ReturnCodes
    {
        NoErrors = 0,
        MissingFile = 1,
        DifferentFile = 1 << 2,
        NoAccess = 1 << 3,
        DirectoryNoAccess = 1 << 4,
        InvalidArgumentFolderDoesNotExist = 1 << 5,
        InvalidArguments = 1 << 6
    }
}
