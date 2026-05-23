using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Infrastructure.Phisics
{
    public class PhysicsBody : MonoBehaviour
    {
        [SerializeField] private float _mass = 1f;
        [SerializeField] private float _drag = 0f;

        public float3 Velocity { get; set; }
        public float3 AngularVelocity { get; set; }
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
    }
}
