using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WavLoopSelector.Audio
{
    public unsafe class PCMStream : IAudioStream
    {
        public class UnknownChunkException : Exception
        {
            internal UnknownChunkException(string message) : base(message) { }
        }

        private IntPtr _allocatedHGlobal = IntPtr.Zero;

        private short* _source;

        private int _bps;
        private int _numSamples;
        private int _numChannels;
        private int _frequency;
        private int _samplePos;

        private bool _looped;
        private int _loopStart;
        private int _loopEnd;

        public WaveFormatTag Format { get { return WaveFormatTag.WAVE_FORMAT_PCM; } }
        public int BitsPerSample { get { return _bps; } }
        public int Samples { get { return _numSamples; } }
        public int Channels { get { return _numChannels; } }
        public int Frequency { get { return _frequency; } }

        public bool IsLooping { get { return _looped; } set { _looped = value; } }
        public int LoopStartSample { get { return _loopStart; } set { _loopStart = value; } }
        public int LoopEndSample { get { return _loopEnd; } set { _loopEnd = value; } }

        public int SamplePosition
        {
            get { return _samplePos; }
            set { _samplePos = Math.Max(Math.Min(value, _numSamples), 0); }
        }

        public PCMStream(byte[] wavData, bool ignoreUnknownChunks)
        {
            _allocatedHGlobal = Marshal.AllocHGlobal(wavData.Length);
            Marshal.Copy(wavData, 0, _allocatedHGlobal, wavData.Length);

            RIFFHeader* header = (RIFFHeader*)_allocatedHGlobal;
            _bps = header->_fmtChunk._bitsPerSample;
            _numChannels = header->_fmtChunk._channels;
            _frequency = (int)header->_fmtChunk._samplesSec;
            _numSamples = (int)(header->_dataChunk._chunkSize / header->_fmtChunk._blockAlign);

            _source = (short*)((byte*)_allocatedHGlobal + header->GetSize());
            _samplePos = 0;

            _looped = false;
            _loopStart = 0;
            _loopEnd = _numSamples;

            smplLoop[] loops = header->_smplLoops;
            if (loops.Length > 0)
            {
                _looped = true;
                _loopStart = (int)loops[0]._dwStart;
                _loopEnd = (int)loops[0]._dwEnd;
            }

            if (!ignoreUnknownChunks)
            {
                List<string> tags = new List<string>();
                byte* ptr = (byte*)&header->_fmtChunk;
                byte* end = (byte*)header + wavData.Length;
                while (ptr < end)
                {
                    string tag = new string((sbyte*)ptr, 0, 4);
                    if (tag != "fmt " && tag != "data" && tag != "smpl")
                        tags.Add(tag);

                    uint size = *((uint*)(ptr + 4));
                    ptr += size + 8;
                }
                if (tags.Count > 0)
                    throw new UnknownChunkException("PCMStream: The following chunks are present in the .wav file and will be discarded if you save: " + string.Join(", ", tags));
            }
        }

        public int ReadSamples(IntPtr destAddr, int numSamples)
        {
            short* sPtr = _source + (_samplePos * _numChannels);
            short* dPtr = (short*)destAddr;

            int max = Math.Min(numSamples, _numSamples - _samplePos);

            for (int i = 0; i < max; i++)
                for (int x = 0; x < _numChannels; x++)
                    *dPtr++ = *sPtr++;

            _samplePos += max;

            return max;
        }

        public void Wrap() 
        {
            SamplePosition = _loopStart;
        }

        public void Dispose()
        {
            if (_allocatedHGlobal != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_allocatedHGlobal);
                _allocatedHGlobal = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }
    }
}
