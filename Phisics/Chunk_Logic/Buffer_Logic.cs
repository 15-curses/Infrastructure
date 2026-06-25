using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Assets.Infrastructure.Phisics.Chunk_Logic
{
    public class Buffer_Logic
    {
        private int sizeX;
        private int sizeY;
        private int bufferCount;

        public Buffer_Logic(int _sizeX, int _sizeY, int _bufferCount)
        {
            sizeX = _sizeX;
            sizeY = _sizeY;
            bufferCount = _bufferCount;
        }

        private int bufferStride;

        private ComputeBuffer ChunkBuffer;

        public void Initialize()
        {
            InitializeBuffer();
        }

        private void InitializeBuffer()
        {
            bufferStride = Marshal.SizeOf<AdditionalBufferData>();
            ChunkBuffer = new ComputeBuffer(bufferCount, bufferStride);
        }
    }
}
