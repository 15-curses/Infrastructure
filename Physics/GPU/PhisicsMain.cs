using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Infrastructure.Physics.GPU
{
    struct Main_Buffer
    {
        public float4 id;
        public float4 position;
        public float4 velocity;
        public float4 rotation;
        public float4 angularVelocity;
        public float4 mass_drag_hasError_addBuffer;
    };
    struct Add_Buffer
    {
        public float4 data0;
        public float4 data1;
        public float4 data2;
        public float4 data3;
    }
    public class PhisicsMain : MonoBehaviour
    {
        public int countSize = 1000;
        public int strideOfMain_Buffer;
        public int strideOfAdd_Buffer;

        public ComputeShader computeShader;

        public Vector3 Gravity = new Vector3(0, -9.8f, 0);

        private Dictionary<int, GameObject> slovaric = new();

        Stack<int> freeSlotsMain;
        Stack<int> freeSlotsAdd;

        private int kernel;

        private ComputeBuffer readMain_Buffer;
        private ComputeBuffer sendMain_Buffer;

        private ComputeBuffer sendAdd_Buffer;

        private int groups;

        private Main_Buffer[] main_Buffers;
        private Add_Buffer[] add_Buffers;

        void Awake()
        {
            freeSlotsMain = new Stack<int>(countSize);
            freeSlotsAdd = new Stack<int>(countSize);

            for (int i = countSize - 1; i >= 0; i--)
            {
                freeSlotsMain.Push(i);
                freeSlotsAdd.Push(i);
            }

            strideOfMain_Buffer = Marshal.SizeOf<Main_Buffer>();
            strideOfAdd_Buffer = Marshal.SizeOf<Add_Buffer>();

            groups = Mathf.CeilToInt(countSize / 64f);

            readMain_Buffer = new ComputeBuffer(countSize, strideOfMain_Buffer);
            sendMain_Buffer = new ComputeBuffer(countSize, strideOfMain_Buffer);
            main_Buffers = new Main_Buffer[countSize];

            sendAdd_Buffer = new ComputeBuffer(countSize, strideOfAdd_Buffer);
            add_Buffers = new Add_Buffer[countSize];

            kernel = computeShader.FindKernel("Physics");
            computeShader.SetVector("_Gravity", Gravity);

            System.Array.Clear(main_Buffers, 0, main_Buffers.Length);
            System.Array.Clear(add_Buffers, 0, add_Buffers.Length);
        }

        private void FixedUpdate()
        {
            sendMain_Buffer.SetData(main_Buffers);
            sendAdd_Buffer.SetData(add_Buffers);

            computeShader.SetFloat("_DeltaTime", Time.fixedDeltaTime);
            computeShader.SetBuffer(kernel, "main_Buffer", sendMain_Buffer);
            computeShader.SetBuffer(kernel, "add_Buffer", sendAdd_Buffer);
            computeShader.Dispatch(kernel, groups, 1, 1);

            ReadResults(readMain_Buffer);

            var temp = sendMain_Buffer;
            sendMain_Buffer = readMain_Buffer;
            readMain_Buffer = temp;
        }

        public int AddToMainBuffer(
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
            if (freeSlotsMain == null || freeSlotsMain.Count == 0) return -1;

            if (freeSlotsMain.TryPop(out int index))
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

                main_Buffers[index] = new Main_Buffer
                {
                    id = _id,
                    position = position,
                    velocity = velocity,
                    rotation = rotation,
                    angularVelocity = angularVelocity,
                    mass_drag_hasError_addBuffer = new float4(_mass ?? 0, _drag ?? 0, 0, 0)
                };
                return index;
            }
            return -1;
        }
        public int AddToAddBuffer(
         float4 data0,
         float4 data1,
         float4 data2,
         float4 data3)
        {
            if (freeSlotsAdd == null || freeSlotsAdd.Count == 0) return -1;
            if (freeSlotsAdd.TryPop(out int index))
            {
                add_Buffers[index] = new Add_Buffer
                {
                    data0 = data0,
                    data1 = data1,
                    data2 = data2,
                    data3 = data3
                };
                return index;
            }
            return -1;
        }

        public void DeliteOfMainBuffer(int index)
        {
            main_Buffers[index] = new Main_Buffer();
            freeSlotsMain.Push(index);
            if (slovaric.ContainsKey(index))
            {
                slovaric.Remove(index);
            }
        }
        public void DeliteOfAddBuffer(int index)
        {
            main_Buffers[index] = new Main_Buffer();
            freeSlotsMain.Push(index);

        }

        private void ReadResults(ComputeBuffer buffer)
        {
            Main_Buffer[] results = new Main_Buffer[countSize];
            buffer.GetData(results, 0, 0, countSize);

            foreach (var (index, obj) in slovaric)
            {
                if (results[index].mass_drag_hasError_addBuffer.z == 0)
                {
                    var segment = results[index];
                    if (segment.position.w == 1)
                    {
                        obj.transform.position = new Vector3(segment.position.x, segment.position.y, segment.position.z);
                        main_Buffers[index].position = new float4(segment.position.x, segment.position.y, segment.position.z, segment.position.w);
                    }
                    if (segment.rotation.w == 1)
                    {
                        obj.transform.rotation = Quaternion.Euler(segment.rotation.x, segment.rotation.y, segment.rotation.z);
                        main_Buffers[index].rotation = segment.rotation;
                    }
                    if (segment.velocity.w == 1)
                        main_Buffers[index].velocity = segment.velocity;
                    if (segment.angularVelocity.w == 1)
                        main_Buffers[index].angularVelocity = segment.angularVelocity;

                    main_Buffers[index].mass_drag_hasError_addBuffer.x = segment.mass_drag_hasError_addBuffer.x;
                    main_Buffers[index].mass_drag_hasError_addBuffer.y = segment.mass_drag_hasError_addBuffer.y;

                    var pbObj = obj.GetComponent<PhisicsBody>();
                    if (pbObj != null)
                    {
                        pbObj.velocity = segment.velocity;
                        pbObj.angularVelocity = segment.angularVelocity;
                        pbObj.mass = segment.mass_drag_hasError_addBuffer.x;
                        pbObj.drag = segment.mass_drag_hasError_addBuffer.y;
                    }
                }
            }
        }
    }
}