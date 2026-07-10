using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

namespace Assets.Infrastructure.Phisics.Chunk_Logic
{
    public class ChunkBuffer_Logic
    {
        private int3 rangeSize;
        private int3 chunkSize;
        private int3 chunkCount;
        private int bufferCount;
        public ComputeBuffer ChunksBuffer { get; private set; }

        public ChunkBuffer_Logic(int3 _rangeSize, int3 _chunkSize)
        {
            rangeSize = _rangeSize;
            chunkSize = _chunkSize;
        }

        public void Initialize()
        {
            CalculationOfChunksCount();
            InitializeBuffer();
            FillBuffer();
        }

        private void CalculationOfChunksCount()
        {
            chunkCount = new int3(
                (rangeSize.x * 2 + chunkSize.x - 1) / chunkSize.x,
                (rangeSize.y * 2 + chunkSize.y - 1) / chunkSize.y,
                (rangeSize.z * 2 + chunkSize.z - 1) / chunkSize.z
            );

            bufferCount = chunkCount.x * chunkCount.y * chunkCount.z;
        }

        private void InitializeBuffer()
        {
            ChunksBuffer = new ComputeBuffer(bufferCount, Marshal.SizeOf<Chunks>());
        }
        
        private void FillBuffer()
        {
            Chunks[] chunksData = new Chunks[bufferCount];
            uint bufferIndex = 0;

            for (float chunkY = 0; chunkY < chunkCount.y; chunkY++)
            {
                for (float chunkZ = 0; chunkZ < chunkCount.z; chunkZ++)
                {
                    for (float chunkX = 0; chunkX < chunkCount.x; chunkX++)
                    {
                        bufferIndex = (uint)(chunkY * chunkCount.z * chunkCount.x + chunkZ * chunkCount.x + chunkX);

                        float minX = -rangeSize.x + chunkX * chunkSize.x;
                        float maxX = minX + chunkSize.x;

                        float minY = -rangeSize.y + chunkY * chunkSize.y;
                        float maxY = minY + chunkSize.y;

                        float minZ = -rangeSize.z + chunkZ * chunkSize.z;
                        float maxZ = minZ + chunkSize.z;

                        chunksData[bufferIndex] = CreateChunk(minX, maxX, minY, maxY, minZ, maxZ);
                    }
                }
            }

            ChunksBuffer.SetData(chunksData);
        }

        private Chunks CreateChunk(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            Chunks chunk = new Chunks();

            chunk.min_xyz = new float3(minX, minY, minZ);
            chunk.max_xyz = new float3(maxX, maxY, maxZ);

            return chunk;
        }
    }
}