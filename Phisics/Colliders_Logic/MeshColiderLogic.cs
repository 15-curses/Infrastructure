using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Infrastructure.Phisics.Colliders_Logic
{
    public class MeshColiderLogic
    {
        private int TrianglesBufferCount;
        private int MeshIndexerBufferCount;
        private int VerticesBufferCount;

        private int AddTrianglesBufferCount;
        private int AddMeshIndexerBufferCount;
        private int AddVerticesBufferCount;

        private ComputeBuffer TrianglesBuffer;
        private ComputeBuffer VerticesBuffer;
        private ComputeBuffer MeshIndexerBuffer;

        private ComputeBuffer AddTrianglesBuffer;
        private ComputeBuffer AddVerticesBuffer;
        private ComputeBuffer AddMeshIndexerBuffer;

        private int trianglesBufferStride;
        private int verticesBufferStride;
        private int meshIndexerBufferStride;

        private static int trianglesPosInBuffer;
        private static int verticesPosInBuffer;
        private static int meshIndexerPosInBuffer;

        public MeshColiderLogic(int _trianglesBufferStride, int _verticesBufferStride, int _meshIndexerBufferStride)
        {
            trianglesBufferStride = _trianglesBufferStride;
            verticesBufferStride = _verticesBufferStride;
            meshIndexerBufferStride = _meshIndexerBufferStride;
        }

        public void Initialize()
        {
            InitializeBufferSizes();
            InitializeBuffers();
        }

        private void InitializeBufferSizes()
        {
            verticesBufferStride = Marshal.SizeOf<VerticesData>();
            trianglesBufferStride = Marshal.SizeOf<TrianglesData>();
            meshIndexerBufferStride = Marshal.SizeOf<MeshIndexerData>();
        }

        private void InitializeBuffers()
        {
            TrianglesBuffer = new(TrianglesBufferCount, trianglesBufferStride);
            VerticesBuffer = new(VerticesBufferCount, verticesBufferStride);
            MeshIndexerBuffer = new(MeshIndexerBufferCount, meshIndexerBufferStride);

            AddTrianglesBuffer = new(AddTrianglesBufferCount, trianglesBufferStride);
            AddVerticesBuffer = new(AddVerticesBufferCount, verticesBufferStride);
            AddMeshIndexerBuffer = new(AddMeshIndexerBufferCount, meshIndexerBufferStride);
        }

        public void SetAddData()
        {
            trianglesPosInBuffer = 0;
            verticesPosInBuffer = 0;
            meshIndexerPosInBuffer = 0;

            
        }

        private void FillMeshData(MeshFilter meshFilter)
        {
            if (meshFilter == null)
            {
                Debug.LogError("MeshFilter не найден!");                // TODO: кастам ошибку
            }

            Mesh mesh = meshFilter.mesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            int colliderType = 1;

            int verticesArrayCount = (int)Math.Ceiling(vertices.Length / 36.0);
            VerticesData[] verticesDataArray = new VerticesData[verticesArrayCount];

            FillVerticesData(ref verticesDataArray, vertices, verticesArrayCount);

            int trianglesArrayCount = (int)Math.Ceiling(triangles.Length / 108.0);
            TrianglesData[] trianglesDataArray = new TrianglesData[trianglesArrayCount];

            FillTrianglesData(ref trianglesDataArray, triangles);

            MeshIndexerData meshIndexer = new();
            FillMeshIndexerData(ref meshIndexer, triangles, colliderType);

            //AddVerticesBuffer.SetData(verticesDataArray);
            //AddTrianglesBuffer.SetData(verticesDataArray);
            //AddMeshIndexerBuffer.SetData(trianglesDataArray);
        }

        private void FillVerticesData(ref VerticesData[] verticesDataArray, Vector3[] vertices, int verticesArrayCount)
        {
            int batchSize = 36;

            for (int batch = 0; batch < verticesArrayCount; batch++)
            {
                VerticesData verticesData = new();
                int baseIndex = batch * batchSize;

                for (int i = 0; i < batchSize && baseIndex + i < vertices.Length; i++)
                {
                    float3 vertex = vertices[baseIndex + i];

                    switch (i)
                    {
                        case 0: verticesData.vertices0 = vertex; break;
                        case 1: verticesData.vertices1 = vertex; break;
                        case 2: verticesData.vertices2 = vertex; break;
                        case 3: verticesData.vertices3 = vertex; break;
                        case 4: verticesData.vertices4 = vertex; break;
                        case 5: verticesData.vertices5 = vertex; break;
                        case 6: verticesData.vertices6 = vertex; break;
                        case 7: verticesData.vertices7 = vertex; break;
                        case 8: verticesData.vertices8 = vertex; break;
                        case 9: verticesData.vertices9 = vertex; break;
                        case 10: verticesData.vertices10 = vertex; break;
                        case 11: verticesData.vertices11 = vertex; break;
                        case 12: verticesData.vertices12 = vertex; break;
                        case 13: verticesData.vertices13 = vertex; break;
                        case 14: verticesData.vertices14 = vertex; break;
                        case 15: verticesData.vertices15 = vertex; break;
                        case 16: verticesData.vertices16 = vertex; break;
                        case 17: verticesData.vertices17 = vertex; break;
                        case 18: verticesData.vertices18 = vertex; break;
                        case 19: verticesData.vertices19 = vertex; break;
                        case 20: verticesData.vertices20 = vertex; break;
                        case 21: verticesData.vertices21 = vertex; break;
                        case 22: verticesData.vertices22 = vertex; break;
                        case 23: verticesData.vertices23 = vertex; break;
                        case 24: verticesData.vertices24 = vertex; break;
                        case 25: verticesData.vertices25 = vertex; break;
                        case 26: verticesData.vertices26 = vertex; break;
                        case 27: verticesData.vertices27 = vertex; break;
                        case 28: verticesData.vertices28 = vertex; break;
                        case 29: verticesData.vertices29 = vertex; break;
                        case 30: verticesData.vertices30 = vertex; break;
                        case 31: verticesData.vertices31 = vertex; break;
                        case 32: verticesData.vertices32 = vertex; break;
                        case 33: verticesData.vertices33 = vertex; break;
                        case 34: verticesData.vertices34 = vertex; break;
                        case 35: verticesData.vertices35 = vertex; break;
                    }
                }

                verticesDataArray[batch] = verticesData;
            }
        }
        private void FillTrianglesData(ref TrianglesData[] trianglesDataArray, int[] triangles)
        {
            int totalTriangles = triangles.Length / 3;
            int batchSize = 36;
            int batchCount = (totalTriangles + batchSize - 1) / batchSize;

            TrianglesData[] result = new TrianglesData[batchCount];
            int triangleCount = 0;

            for (int batch = 0; batch < batchCount; batch++)
            {
                TrianglesData data = new();

                for (int i = 0; i < batchSize && triangleCount * 3 < triangles.Length; i++)
                {
                    int baseIndex = triangleCount * 3;

                    float3 triangle = new float3(
                        triangles[baseIndex],
                        triangles[baseIndex + 1],
                        triangles[baseIndex + 2]);

                    switch (i)
                    {
                        case 0: data.triangle0 = triangle; break;
                        case 1: data.triangle1 = triangle; break;
                        case 2: data.triangle2 = triangle; break;
                        case 3: data.triangle3 = triangle; break;
                        case 4: data.triangle4 = triangle; break;
                        case 5: data.triangle5 = triangle; break;
                        case 6: data.triangle6 = triangle; break;
                        case 7: data.triangle7 = triangle; break;
                        case 8: data.triangle8 = triangle; break;
                        case 9: data.triangle9 = triangle; break;
                        case 10: data.triangle10 = triangle; break;
                        case 11: data.triangle11 = triangle; break;
                        case 12: data.triangle12 = triangle; break;
                        case 13: data.triangle13 = triangle; break;
                        case 14: data.triangle14 = triangle; break;
                        case 15: data.triangle15 = triangle; break;
                        case 16: data.triangle16 = triangle; break;
                        case 17: data.triangle17 = triangle; break;
                        case 18: data.triangle18 = triangle; break;
                        case 19: data.triangle19 = triangle; break;
                        case 20: data.triangle20 = triangle; break;
                        case 21: data.triangle21 = triangle; break;
                        case 22: data.triangle22 = triangle; break;
                        case 23: data.triangle23 = triangle; break;
                        case 24: data.triangle24 = triangle; break;
                        case 25: data.triangle25 = triangle; break;
                        case 26: data.triangle26 = triangle; break;
                        case 27: data.triangle27 = triangle; break;
                        case 28: data.triangle28 = triangle; break;
                        case 29: data.triangle29 = triangle; break;
                        case 30: data.triangle30 = triangle; break;
                        case 31: data.triangle31 = triangle; break;
                        case 32: data.triangle32 = triangle; break;
                        case 33: data.triangle33 = triangle; break;
                        case 34: data.triangle34 = triangle; break;
                        case 35: data.triangle35 = triangle; break;
                    }

                    triangleCount++;
                }

                result[batch] = data;
            }

            trianglesDataArray = result;
        }
        private void FillMeshIndexerData(ref MeshIndexerData meshIndexer, int[] triangles, float colliderType)
        {
            MeshIndexerData data = new();
            data.colliderType = colliderType;

            int batchSize = 32;
            int processCount = Mathf.Min(triangles.Length, batchSize);

            data.trianglesBufferID0 = new float3(
                triangles[0],
                1 < processCount ? triangles[1] : 0,
                2 < processCount ? triangles[2] : 0);

            for (int i = 1; i <= 7; i++)
            {
                int startIdx = 3 + (i - 1) * 4;

                float4 bufferData = new float4(
                    startIdx < processCount ? triangles[startIdx] : 0,
                    startIdx + 1 < processCount ? triangles[startIdx + 1] : 0,
                    startIdx + 2 < processCount ? triangles[startIdx + 2] : 0,
                    startIdx + 3 < processCount ? triangles[startIdx + 3] : 0);

                switch (i)
                {
                    case 1: data.trianglesBufferID1 = bufferData; break;
                    case 2: data.trianglesBufferID2 = bufferData; break;
                    case 3: data.trianglesBufferID3 = bufferData; break;
                    case 4: data.trianglesBufferID4 = bufferData; break;
                    case 5: data.trianglesBufferID5 = bufferData; break;
                    case 6: data.trianglesBufferID6 = bufferData; break;
                    case 7: data.trianglesBufferID7 = bufferData; break;
                }
            }

            meshIndexer = data;
        }
    }
}
