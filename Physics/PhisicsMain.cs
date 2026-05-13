using Assets.Infrastructure.Physics.GPU;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Infrastructure.Physics
{
    struct Main_Buffer
    {
        public float4 id;
        public float4 position;
        public float4 velocity;
        public float4 rotation;
        public float4 angularVelocity;
        public float4 mass_drag_hasError;
    };
    public class PhisicsMain : MonoBehaviour
    {
        public int countSize = 1000;
        public int strideOfBuffer;

        public ComputeShader computeShader;

        public Vector3 Gravity = new Vector3(0, -9.8f, 0);

        private Dictionary<int, GameObject> slovaric = new();
        Stack<int> freeSlots;

        private int kernel;

        private ComputeBuffer readBuffer;
        private ComputeBuffer sendBuffer;

        private int groups;


        private Main_Buffer[] toSendBuffer;
        void Awake()
        {
            freeSlots = new Stack<int>(countSize);
            for (int i = countSize - 1; i >= 0; i--)
            {
                freeSlots.Push(i);
            }
            strideOfBuffer = Marshal.SizeOf<Main_Buffer>();
            groups = Mathf.CeilToInt(countSize / 64f);
            readBuffer = new ComputeBuffer(countSize, strideOfBuffer);
            sendBuffer = new ComputeBuffer(countSize, strideOfBuffer);
            toSendBuffer = new Main_Buffer[countSize];

            kernel = computeShader.FindKernel("Physics");
            computeShader.SetVector("_Gravity", Gravity);

            System.Array.Clear(toSendBuffer, 0, toSendBuffer.Length);
        }
        private void FixedUpdate()
        {
            sendBuffer.SetData(toSendBuffer);

            computeShader.SetFloat("_DeltaTime", Time.fixedDeltaTime);
            computeShader.SetBuffer(kernel, "_Buffer", sendBuffer);
            computeShader.Dispatch(kernel, groups, 1, 1);

            ReadResults(readBuffer);

            var temp = sendBuffer;
            sendBuffer = readBuffer;
            readBuffer = temp;
        }

        public int AddToBuffer(
            GameObject gObject,
            float4 _id,
            Vector3? _position = null,
            float3? _velocity = null,
            float3? _rotation = null,
            float3? _angularVelocity = null,
            float? _mass = 0,
            float? _drag = 0
            )
        {
            if (freeSlots == null || freeSlots.Count == 0) return -1;

            if (freeSlots.TryPop(out int index))
            {
                slovaric.Add(index, gObject);

                float4 position = float4.zero;
                if (_position.HasValue)
                {
                    Vector3 pos = _position.Value;
                    position = new float4(pos.x, pos.y, pos.z, 0);
                }

                float4 velocity = float4.zero;
                if (_velocity.HasValue)
                {
                    float3 vel = _velocity.Value;
                    velocity = new float4(vel.x, vel.y, vel.z, 0);
                }

                float4 rotation = float4.zero;
                if (_rotation.HasValue)
                {
                    float3 rot = _rotation.Value;
                    rotation = new float4(rot.x, rot.y, rot.z, 0);
                }

                float4 angularVelocity = float4.zero;
                if (_angularVelocity.HasValue)
                {
                    float3 angVel = _angularVelocity.Value;
                    angularVelocity = new float4(angVel.x, angVel.y, angVel.z, 0);
                }

                toSendBuffer[index] = new Main_Buffer
                {
                    id = _id,
                    position = position,
                    velocity = velocity,
                    rotation = rotation,
                    angularVelocity = angularVelocity,
                    mass_drag_hasError = new float4(_mass ?? 0, _drag ?? 0, 0, 0)
                };
                return index;
            }
            return -1;
        }
        public void DeliteOFBuffer(int index)
        {
            toSendBuffer[index] = new Main_Buffer();
            freeSlots.Push(index);
        }
        private void ReadResults(ComputeBuffer buffer)
        {
            Main_Buffer[] results = new Main_Buffer[countSize];
            buffer.GetData(results, 0, 0, countSize);

            foreach (var (index, obj) in slovaric)
            {
                if (results[index].mass_drag_hasError.z == 0)
                {
                    var segment = results[index];
                    obj.transform.position = new Vector3(segment.position.x, segment.position.y, segment.position.z);
                    obj.transform.rotation = Quaternion.Euler(segment.rotation.x, segment.rotation.y, segment.rotation.z);

                    toSendBuffer[index].position = new float4(segment.position.x, segment.position.y, segment.position.z, 0);
                    toSendBuffer[index].velocity = segment.velocity;
                    toSendBuffer[index].angularVelocity = segment.angularVelocity;
                    toSendBuffer[index].mass_drag_hasError.x = segment.mass_drag_hasError.x;
                    toSendBuffer[index].mass_drag_hasError.y = segment.mass_drag_hasError.y;

                    var pbObj = obj.GetComponent<PhisicsBody>();
                    if (pbObj != null)
                    {
                        pbObj.velocity = segment.velocity;
                        pbObj.angularVelocity = segment.angularVelocity;
                        pbObj.mass = segment.mass_drag_hasError.x;
                        pbObj.drag = segment.mass_drag_hasError.y;
                    }
                }
            }
        }
    }
}
