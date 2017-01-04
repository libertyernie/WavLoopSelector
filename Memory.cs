using System;

namespace WavLoopSelector
{
    public unsafe static class Memory
    {
        internal static unsafe void Fill(IntPtr dest, uint length, byte value)
        {
            byte* ptr = (byte*)dest;
            for (uint i = 0; i < length; i++) {
                *(ptr++) = value; 
            }
        }
    }
}
