using Assets.Infrastructure.ServiceLocator;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Infrastructure.Physics.GPU
{
    public enum PhisicsDeliteTypes
    {
        Gravity,
        RunToOrbit
    }
    public class PhisicsBody : MonoBehaviour
    {
        public float4 velocity;
        public float4 angularVelocity;
        public float mass;
        public float drag;

        public bool gravity = false;

        private PhisicsMain phisics;

        private Dictionary<PhisicsDeliteTypes, int> indexValues = new();

        void Start()
        {
            phisics = ServiceContainer.Get<PhisicsMain>();

            var transform = gameObject.transform;

            if (gravity) AddGravity();
        }
        public void AddGravity()
        {
            var obj = gameObject;
            int gravityIndex = phisics.AddToMainBuffer(
                gObject: obj,
                _id: new float4(3, 1, 1, 1),
                _position: obj.transform.position
            );
            indexValues.Add(PhisicsDeliteTypes.Gravity, gravityIndex);
        }
        public void DelitePhisics(PhisicsDeliteTypes type)
        {
            if (indexValues.TryGetValue(type, out int index))
            {
                phisics.DeliteOfMainBuffer(index);
            }
        }
        public void RunToOrbit()
        {

        }
    }
}
