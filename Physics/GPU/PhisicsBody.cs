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

        public GameObject obj;
        public float torque;
        public float radius;

        public bool gravity = false;
        public bool orbit = false;

        private static Vector2 mouseDelta;

        private PhisicsMain phisics;
        private InputManager.InputSystem inputSystem;

        private Dictionary<PhisicsDeliteTypes, int> indexValues = new();
        private Dictionary<PhisicsDeliteTypes, int2> indexValuesWhithAddBuffer = new();

        void Start()
        {
            phisics = ServiceContainer.Get<PhisicsMain>();
            inputSystem = ServiceContainer.Get<InputManager.InputSystem>();

            var transform = gameObject.transform;

            if (gravity) AddGravity();
            if (orbit)
            {
                euler = new Vector2(transform.eulerAngles.x, transform.eulerAngles.y);
                inputSystem.SubMouse(Mous);
            }
        }
        private void Update()
        {
            if (orbit) RunToOrbit();
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

        private GameObject centerObject;
        private float2 euler;
        private static void Mous(Vector2 _mouseDelta)
            => mouseDelta = _mouseDelta;
        public void RunToOrbit()
        {
            var obj = gameObject;
            var center = obj.transform.position;

            int addBufferIndex = phisics.AddToAddBuffer(
                data0: new float4(mouseDelta.x, mouseDelta.y, euler.x, euler.y),
                data1: new float4(torque, radius, 0, 0),
                data2: new float4(center.x, center.y, center.z, 0),
                data3: new float4(0, 0, 0, 0)
            );
            int mainIndex = phisics.AddToMainBuffer(
                gObject: obj,
                _id: new float4(1, 0, 0, 0),
                _position: obj.transform.position
            );
            indexValuesWhithAddBuffer.Add(
                PhisicsDeliteTypes.RunToOrbit,
                new int2(mainIndex, addBufferIndex)
            );
            Debug.Log($"{torque} ,{radius.ToString()},  {mouseDelta}, {center}");
        }
        #endregion
    }
}