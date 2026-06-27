using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Infrastructure.Phisics.IndexOrchestrator
{
    public static class Orchestrator_Logic
    {
        public static int indexsCount = 100000;
        private static IndexedStorage<uint> indexs = new(indexsCount);
        public static ComputeBuffer IndexsBuffer;

        public static void Initialize() => IndexsBuffer = new(indexsCount / 4, Marshal.SizeOf<This>());

        public static int PackBinaryWithGroup(uint indexInBuffer, byte operationID)
        {
            uint binaryDecimal = 0;
            for (int i = 0; i < 24; i++)
                binaryDecimal = binaryDecimal * 10 + ((indexInBuffer >> (23 - i)) & 1);

            uint result = ((uint)operationID << 24) | binaryDecimal;

            return AddIndex(result);
        }

        private static int AddIndex(uint indexInBuffer)
        {
            int indexInStorage = indexs.Add(indexInBuffer);

            int indexInThisBuffer = indexInStorage / 4;
            uint slot = indexInBuffer % 4;

            IndexerData[] tmp = new IndexerData[1];
            IndexsBuffer.GetData(tmp, 0, indexInThisBuffer, 1);
            var newBuffer = tmp[0];

            switch (slot)
            {
                case 0: newBuffer.index0 = indexInBuffer; break;
                case 1: newBuffer.index1 = indexInBuffer; break;
                case 2: newBuffer.index2 = indexInBuffer; break;
                case 3: newBuffer.index3 = indexInBuffer; break;
            }

            IndexsBuffer.SetData(new IndexerData[1] { newBuffer }, 0, indexInThisBuffer, 1);
            return indexInStorage;
        }
    }
}
public struct IndexerData
{
    public uint index0;
    public uint index1;
    public uint index2;
    public uint index3;
}
/*
public class OperationsID
{
    public const byte None = 0;
    public const byte Move = 1;
    public const byte AddMove = 2;
    public const byte type3 = 3;
    public const byte type4 = 4;
    public const byte type5 = 5;
    public const byte type6 = 6;
    public const byte type7 = 7;
    public const byte type8 = 8;
    public const byte type9 = 9;
    public const byte type10 = 10;
    public const byte type11 = 11;
    public const byte type12 = 12;
    public const byte type13 = 13;
    public const byte type14 = 14;
    public const byte type15 = 15;
    public const byte type16 = 16;
    public const byte type17 = 17;
    public const byte type18 = 18;
    public const byte type19 = 19;
    public const byte type20 = 20;
}*/