using Assets.Infrastructure.ServiceLocator;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Infrastructure.Phisics
{
    public class PhysicsEntityManager : MonoBehaviour
    {
        private PhysicsSimulation _physicsSimulation;

        private void Awake()
        {
            if (_physicsSimulation == null)
                _physicsSimulation = ServiceContainer.Get<PhysicsSimulation>();
        }

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

                int index = _physicsSimulation.AddPhysicsObject(gameObject, mainBuffer,
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
            int additionalIndex = _physicsSimulation.RegisterAdditionalSlot(additionalBuffer);
            return additionalIndex;
        }
        public void RemovePhysicsEntity(int index) => _physicsSimulation.RemovePhysicsObject(index);
        public void AddLinkAdditionalToMain(int mainIndex, int additionalIndex)
            => _physicsSimulation.LinkAdditionalToMain(mainIndex, additionalIndex);

        public void SetUpdateMask(int index, int mask) => _physicsSimulation?.SetUpdateMask(index, mask);
    }
}
