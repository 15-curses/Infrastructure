using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Infrastructure.Phisics.ReadBack
{
    public static class BackBuffer_Logic
    {
        public static int indexsCount = 100000;
        private static IndexedStorage<uint> indexs = new(indexsCount);
        public static ComputeBuffer BackBuffer;

        private static IndexerData[] BufferData = new IndexerData[indexsCount];

        public static void Initialize() => BackBuffer = new(indexsCount / 4, Marshal.SizeOf<This>());
        
        public static void ReadBackBuffer()
        {
            BackBuffer.GetData(BufferData);
            for (uint index = 0; index < indexsCount; index++)
            {
                int indexInBuffer = (int)Math.Floor(index / 4f);
                uint slotIndex = index % 4;
                IndexerData element = BufferData[indexInBuffer];

                uint packedValue = 0;

                switch (slotIndex)
                {
                    case 0: packedValue = element.index0; break;
                    case 1: packedValue = element.index1; break;
                    case 2: packedValue = element.index2; break;
                    case 3: packedValue = element.index3; break;
                }
                ReadOperation(packedValue);
            }
            Array.Clear(BufferData, 0, indexsCount);
        }

        private static void ReadOperation(uint packedValue)
        {
            UnpackBinaryWithGroup(packedValue, out uint indexInBuffer, out uint operationID);

            switch (operationID)
            {
                case (uint)OperationType.None: break;
                case (uint)OperationType.Movement: break;
                case (uint)OperationType.AddMove: break;
                default: throw new ArgumentException($"Unknown operation ID: {operationID}");
            }
        }

        private static void UnpackBinaryWithGroup(uint packedValue, out uint indexInBuffer, out uint operationID)
        {
            operationID = (packedValue >> 24) & 0xFFu;
            indexInBuffer = packedValue & 0xFFFFFFu;
        }

        public enum OperationType : uint
        {
            None = 0,
            Movement = 1,
            AddMove = 2
        }
    }
}