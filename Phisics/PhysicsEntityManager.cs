using Unity.Mathematics;
using UnityEngine;

namespace Assets.Infrastructure.Phisics
{
    public class PhysicsEntityManager : MonoBehaviour
    {
        [SerializeField] private PhysicsSimulation _physicsSimulation;
        [SerializeField] private ComputeShader _computeShader;

        private void Awake()
        {
            if (_physicsSimulation == null)
                _physicsSimulation = GetComponent<PhysicsSimulation>();
        }

        public int AddPhysicsEntity(
            GameObject gameObject,
            float4 id,
            Vector3? position = null,
            float3? velocity = null,
            float3? rotation = null,
            float3? angularVelocity = null,
            float? mass = 1f,
            float? drag = 0f,
            int updateMask = UpdateMasks.All)
        {
            // TODO: Реализовать добавление через _physicsSimulation
            // Нужно добавить публичный метод в PhysicsSimulation для добавления объектов
            return -1;
        }

        public void RemovePhysicsEntity(int index)
        {
            // TODO: Реализовать удаление
        }

        public void SetUpdateMask(int index, int mask)
        {
            _physicsSimulation?.SetUpdateMask(index, mask);
        }

        public void SetPosition(int index, Vector3 position)
        {
            // TODO: Обновить позицию в _mainBufferData
        }
    }
}
