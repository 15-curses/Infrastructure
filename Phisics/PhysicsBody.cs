using Assets.Infrastructure.InputManager;
using Assets.Infrastructure.ServiceLocator;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Infrastructure.Phisics
{
    public class PhysicsBody : MonoBehaviour
    {
        [SerializeField] private float _mass = 1f;
        [SerializeField] private float _drag = 0f;

        [SerializeField] private GameObject _addGameObject;
        [SerializeField] private float torque = 1f;
        [SerializeField] private float radius = 1f;

        public float3 Velocity { get; set; }
        public float3 AngularVelocity { get; set; }

        private Dictionary<int, int> physicsMethodsIndexes;
        private PhysicsEntityManager physicsManager;
        public float Mass
        {
            get => _mass;
            set => _mass = value;
        }
        public float Drag
        {
            get => _drag;
            set => _drag = value;
        }
        private void Start()
        {
            physicsManager = ServiceContainer.Get<PhysicsEntityManager>();
            physicsMethodsIndexes = new Dictionary<int, int>();
            Gravity();
            RunToOrbit();
        }
        public void Gravity() // Index = 1
        {
            int index = physicsManager.AddPhysicsEntity(
                gameObject,
                new float4(3, 0, 0, 0),
                position: gameObject.transform.position,
                velocity: Velocity
            );
            physicsMethodsIndexes[1] = index;
        }
        public void RemoveGravity() => physicsManager.RemovePhysicsEntity(physicsMethodsIndexes[1]);
        
        public void RunToOrbit()
        {
            var center = _addGameObject.transform.position;

            int additionalIndex = physicsManager.AddAdditionalBuffer(
                data0: new float4(0, 0, 0, 0),
                data1: new float4(torque, radius, 0, 0),
                data2: new float4(center.x, center.y, center.z, 1),
                data3: new float4(0, 0, 0, 0)
            );
            int mainIndex = physicsManager.AddPhysicsEntity(
                gameObject: gameObject,
                id: new float4(1, 0, 0, 0),
                position: gameObject.transform.position,
                additionalBufferIndex: additionalIndex
            );
            physicsManager.AddLinkAdditionalToMain(mainIndex, additionalIndex);
        }
    }
}
