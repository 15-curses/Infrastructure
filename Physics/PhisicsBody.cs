using Assets.Infrastructure.Physics.GPU;
using Assets.Infrastructure.ServiceLocator;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using InputSystem = Assets.Infrastructure.InputManager.InputSystem;

namespace Assets.Infrastructure.Physics
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

        public GameObject obj;
        public float torque;
        public float radius;

        public bool gravity = false;
        public bool orbit = false;

        private PhisicsMain phisics;

        private Dictionary<PhisicsDeliteTypes, int> indexValues = new();
        private Dictionary<PhisicsDeliteTypes, int2> indexValuesWhithAddBuffer = new();

        void Start()
        {
            phisics = ServiceContainer.Get<PhisicsMain>();

            var transform = gameObject.transform;

            if (gravity) AddGravity();
            if (orbit)
            {
                euler = new Vector2(transform.eulerAngles.x, transform.eulerAngles.y);
                InputSystem.SubMouse(RunToOrbit);
            }
        }

        public void AddGravity()
        {
            var obj = gameObject;
            int gravityIndex = phisics.AddToMainBuffer(
                gObject: obj,
                _id: new float4(3, 0, 0, 0),
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

        #region Орбитальное движение

        private float2 euler;
        int addBufferIndex;
        int mainIndex;

        public void RunToOrbit(Vector2 mouseDelta)
        {
            var center = obj.transform.position;

            addBufferIndex = phisics.AddToAddBuffer(
                data0: new float4(mouseDelta.x, mouseDelta.y, euler.x, euler.y),
                data1: new float4(torque, radius, 0, 0),
                data2: new float4(center.x, center.y, center.z, 1),
                data3: new float4(0, 0, 0, 0)
            );
            mainIndex = phisics.AddToMainBuffer(
                gObject: obj,
                _id: new float4(1, 0, 0, 0),
                _position: obj.transform.position
            );
            indexValuesWhithAddBuffer[PhisicsDeliteTypes.RunToOrbit]
                = new int2(mainIndex, addBufferIndex);
        }
        #endregion
    }
}