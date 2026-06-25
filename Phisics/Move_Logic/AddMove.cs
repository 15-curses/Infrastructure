using System;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Infrastructure.Phisics.Move_Logic
{
    public class AddMove
    {
        private int mainBufferCount;

        private MovePoolManager poolManager;

        private ComputeBuffer mainBuffer1;
        private ComputeBuffer additionalBuffer1;

        public AddMove(MovePoolManager _poolManager, ComputeBuffer _mainBuffer1, ComputeBuffer _additionalBuffer1, int _mainBufferCount)
        {
            poolManager = _poolManager;
            mainBuffer1 = _mainBuffer1;
            additionalBuffer1 = _additionalBuffer1;
            mainBufferCount = _mainBufferCount;
        }

        private int AddPhysicsObject(GameObject gameObject, MainBufferData initialData, PhysicsBody body)
        {
            if (!poolManager.TryGetMainSlot(out int index))
                return -1;

            poolManager.RegisterGameObject(index, gameObject);

            var physicsBody = body
                ?? gameObject.GetComponent<PhysicsBody>()
                ?? gameObject.AddComponent<PhysicsBody>();

            poolManager.RegisterPhysicsBody(index, physicsBody);

            mainBuffer1.SetData(new MainBufferData[1] { initialData }, 0, index, 1);

            return index;
        }
        private void RemovePhysicsObject(int index)
        {
            if (index < 0 || index >= mainBufferCount) return;

            mainBuffer1.SetData(new MainBufferData[1] { new MainBufferData() }, 0, index, 1);
            poolManager.ReturnMainSlot(index);
        }
        private int RegisterAdditionalSlot(AdditionalBufferData additionalData)
        {
            if (poolManager.TryGetAdditionalSlot(out int additionalIndex))
            {
                additionalBuffer1.SetData(new AdditionalBufferData[1] { additionalData }, 0, additionalIndex, 1);
                return additionalIndex;
            }
            return -1;
        }

        private void LinkAdditionalToMain(int mainBuferIndex, int additionalIndex)
            => poolManager.RegisterUnificationOfBuffers(mainBuferIndex, additionalIndex);



        public int AddPhysicsEntity(
            GameObject gameObject,
            float4 id,
            Vector3? position = null,
            float3? velocity = null,
            float3? rotation = null,
            float3? angularVelocity = null,
            float mass = 1f,
            float drag = 0f,
            int additionalBufferIndex = -1,
            int updateMask = MainBufferData.UpdateMasks.All,
            PhysicsBody body = null)
        {
            try
            {
                var mainBuffer = new MainBufferData
                {
                    Id = id,
                    Position = position.HasValue
                    ? new float4(position.Value.x, position.Value.y, position.Value.z, 0)
                    : float4.zero,

                    Velocity = velocity.HasValue
                    ? new float4(velocity.Value, 0)
                    : float4.zero,

                    Rotation = rotation.HasValue
                    ? new float4(rotation.Value, 0)
                    : float4.zero,

                    AngularVelocity = angularVelocity.HasValue
                    ? new float4(angularVelocity.Value, 0)
                    : float4.zero,

                    MassDragAdditionalFlags = new float4(mass, drag, additionalBufferIndex, updateMask)
                };

                int index = AddPhysicsObject(gameObject, mainBuffer,
                    body != null ? body : gameObject.GetComponent<PhysicsBody>());

                return index;
            }
            catch (Exception e)
            {
                Debug.LogError($"Ошибка AddPhysicsEntity: {e}");
                // TODO: кастом ошибку (не нашлось места в буфере или ошибка записи в буфер)
                return -1;
            }
        }
        public int AddAdditionalBuffer(float4? data0 = null, float4? data1 = null, float4? data2 = null, float4? data3 = null)
        {
            var additionalBuffer = new AdditionalBufferData
            {
                Data0 = (float4)data0,
                Data1 = (float4)data1,
                Data2 = (float4)data2,
                Data3 = (float4)data3
            };
            int additionalIndex = RegisterAdditionalSlot(additionalBuffer);
            return additionalIndex;
        }
        public void RemovePhysicsEntity(int index) => RemovePhysicsObject(index);
        public void AddLinkAdditionalToMain(int mainIndex, int additionalIndex)
            => LinkAdditionalToMain(mainIndex, additionalIndex);
    }
}