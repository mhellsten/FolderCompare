namespace FolderCompare
{
    /// <summary>
    /// Based on: https://stackoverflow.com/questions/43289/comparing-two-byte-arrays-in-net
    /// </summary>
    static class ArrayCompare
    {
        public static unsafe bool CompareArraysUnsafe(byte* bytePtr1, byte* bytePtr2, int length)
        {
            byte* lastAddr = bytePtr1 + length;
            byte* lastAddrMinus32 = lastAddr - 32;
            while (bytePtr1 < lastAddrMinus32) // unroll the loop so that we are comparing 32 bytes at a time.
            {
                if (*(ulong*)bytePtr1 != *(ulong*)bytePtr2) return false;
                if (*(ulong*)(bytePtr1 + 8) != *(ulong*)(bytePtr2 + 8)) return false;
                if (*(ulong*)(bytePtr1 + 16) != *(ulong*)(bytePtr2 + 16)) return false;
                if (*(ulong*)(bytePtr1 + 24) != *(ulong*)(bytePtr2 + 24)) return false;
                bytePtr1 += 32;
                bytePtr2 += 32;
            }
            while (bytePtr1 < lastAddr)
            {
                if (*bytePtr1 != *bytePtr2) return false;
                bytePtr1++;
                bytePtr2++;
            }
            return true;
        }

        public static unsafe bool CompareArraysUnsafe(byte[] array1, byte[] array2, int length)
        {
            fixed (byte* bytePtr1 = array1) fixed (byte* bytePtr2 = array2)
            {
                return CompareArraysUnsafe(bytePtr1, bytePtr2, length);
            }
        }
    }
}
