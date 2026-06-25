using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Infrastructure.Phisics.Chunk_Logic
{
    public class ChunkBuffer_Logic
    {
        private int3 rangeSize;
        private int3 chunkSize;
        private int3 chunkCount;
        private int bufferCount;
        public ComputeBuffer ChunkBuffer { get; private set; }

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
            ChunkBuffer = new ComputeBuffer(bufferCount, Marshal.SizeOf<Chunks>());
        }

        private void FillBuffer()
        {
            Chunks[] chunksData = new Chunks[bufferCount];
            int bufferIndex = 0;

            for (int chunkY = 0; chunkY < chunkCount.y; chunkY++)
            {
                for (int chunkZ = 0; chunkZ < chunkCount.z; chunkZ++)
                {
                    for (int chunkX = 0; chunkX < chunkCount.x; chunkX++)
                    {
                        int minX = -rangeSize.x + chunkX * chunkSize.x;
                        int maxX = minX + chunkSize.x;

                        int minY = -rangeSize.y + chunkY * chunkSize.y;
                        int maxY = minY + chunkSize.y;

                        int minZ = -rangeSize.z + chunkZ * chunkSize.z;
                        int maxZ = minZ + chunkSize.z;

                        chunksData[bufferIndex] = CreateChunk(minX, maxX, minY, maxY, minZ, maxZ);
                        bufferIndex++;
                    }
                }
            }

            ChunkBuffer.SetData(chunksData);
        }

        private Chunks CreateChunk(int minX, int maxX, int minY, int maxY, int minZ, int maxZ)
        {
            Chunks chunk = new Chunks();

            chunk.vertice0 = new float3(minX, minY, minZ);
            chunk.vertice1 = new float3(maxX, minY, minZ);
            chunk.vertice2 = new float3(minX, maxY, minZ);
            chunk.vertice3 = new float3(maxX, maxY, minZ);

            return chunk;
        }
    }
}