using Assets.Infrastructure.ServiceLocator;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Infrastructure.Physics.GPU
{
    public class PhisicsBody : MonoBehaviour
    {
        public float4 velocity;
        public float4 angularVelocity;
        public float mass;
        public float drag;

        public bool gravity = false;

        private PhisicsMain phisics;

        private int gravityIndex;
        void Start()
        {
            phisics = ServiceContainer.Get<PhisicsMain>();

            var transform = gameObject.transform;

            if (gravity) AddGravity();
        }
        public void AddGravity()
        {
            var obj = gameObject;
            gravityIndex = phisics.AddToBuffer(
                gObject: obj,
                _id: new float4(3, 1, 1, 1),
                _position: obj.transform.position
            );
        }
        public void DeliteGravity()
        {
            phisics.DeliteOFBuffer(gravityIndex);
        }
    }
}
