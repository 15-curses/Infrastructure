using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets.Infrastructure.Phisics.Move_Logic
{
    public class Move_Buffers_Logic
    {
        public int mainBufferCount {  get; private set; }
        private int additionalBufferCount;

        private int mainBufferStride;
        private int additionalBufferStride;
        private int uintSize;

        private int[] activeIndices;
        public int activeCount { get; private set; }

        private ComputeBuffer MainBuffer0;
        private ComputeBuffer AdditionalBuffer0;

        public ComputeBuffer MainBuffer1 { get; private set; }
        public ComputeBuffer AdditionalBuffer1 { get; private set; }

        public ComputeBuffer ActiveIndicesBuffer { get; private set; }
        public ComputeBuffer TaskCounterBuffer { get; private set; }
        public ComputeBuffer ActiveCountBuffer { get; private set; } //

        private MovePoolManager poolManager;

        public AddMove addMove { get; private set; }

        public Move_Buffers_Logic(int _mainBufferCount, int _additionalBufferCount)
        {
            mainBufferCount = _mainBufferCount;
            additionalBufferCount = _additionalBufferCount;
        }

        public void Initialize()
        {
            poolManager = new(mainBufferCount);
            addMove = new(poolManager, MainBuffer1, AdditionalBuffer1, mainBufferCount);

            InitializeBufferSizes();
            InitializeBuffers();
        }

        private void InitializeBufferSizes()
        {
            mainBufferStride = Marshal.SizeOf<MainBufferData>();
            additionalBufferStride = Marshal.SizeOf<AdditionalBufferData>();
            uintSize = Marshal.SizeOf<uint>();
        }
        private void InitializeBuffers()
        {
            MainBuffer0 = new ComputeBuffer(mainBufferCount, mainBufferStride);
            MainBuffer1 = new ComputeBuffer(mainBufferCount, mainBufferStride);
            AdditionalBuffer0 = new ComputeBuffer(additionalBufferCount, additionalBufferStride);
            AdditionalBuffer1 = new ComputeBuffer(additionalBufferCount, additionalBufferStride);

            ActiveIndicesBuffer = new ComputeBuffer(mainBufferCount, sizeof(int));
            TaskCounterBuffer = new ComputeBuffer(1, uintSize);
            ActiveCountBuffer = new ComputeBuffer(1, uintSize);
        }

        public void Move_Update()
        {
            SendActiveIndicesToBuffers();
            ReadResultsFromBuffer();
            SwapBuffers();
        }

        private void SendActiveIndicesToBuffers()
        {
            activeCount = 0;
            foreach (var pair in poolManager.GetRegistry())
                activeIndices[activeCount++] = pair.Key;

            ActiveIndicesBuffer.SetData(activeIndices, 0, 0, activeCount);
            ActiveCountBuffer.SetData(new uint[] { (uint)activeCount });
            TaskCounterBuffer.SetData(new uint[] { 0 });
        }
        private void SwapBuffers()
        {
            var temp0 = MainBuffer1;
            MainBuffer1 = MainBuffer0;
            MainBuffer0 = temp0;

            var temp1 = AdditionalBuffer1;
            AdditionalBuffer1 = AdditionalBuffer0;
            AdditionalBuffer0 = temp1;
        }
        private void ReadResultsFromBuffer()
        {
            var results = new MainBufferData[mainBufferCount];

            MainBuffer0.GetData(results, 0, 0, mainBufferCount);

            foreach (var pair in poolManager.GetRegistry())
            {
                int index = pair.Key;
                MainBufferData resultData = results[index];
                int updateMask = (int)resultData.MassDragAdditionalFlags.w;

                if (updateMask == -1 || updateMask < 0 || updateMask >= 32) continue;

                switch (updateMask)
                {
                    // 00000 (0) - Ничего
                    case 0: break;

                    // 00001 (1) - AngularVelocity
                    case 1: UpdateAngularVelocityOnly(index, pair.Value, resultData); break;

                    // 00010 (2) - Velocity
                    case 2: UpdateVelocityOnly(index, pair.Value, resultData); break;

                    // 00011 (3) - Velocity + AngularVelocity
                    case 3: UpdateVelocityAngularVelocity(index, pair.Value, resultData); break;

                    // 00100 (4) - Rotation
                    case 4: UpdateRotationOnly(index, pair.Value, resultData); break;

                    // 00101 (5) - Rotation + AngularVelocity
                    case 5: UpdateRotationAngularVelocity(index, pair.Value, resultData); break;

                    // 00110 (6) - Rotation + Velocity
                    case 6: UpdateRotationVelocity(index, pair.Value, resultData); break;

                    // 00111 (7) - Rotation + Velocity + AngularVelocity
                    case 7: UpdateRotationVelocityAngularVelocity(index, pair.Value, resultData); break;

                    // 01000 (8) - Position
                    case 8: UpdatePositionOnly(index, pair.Value, resultData); break;

                    // 01001 (9) - Position + AngularVelocity
                    case 9: UpdatePositionAngularVelocity(index, pair.Value, resultData); break;

                    // 01010 (10) - Position + Velocity
                    case 10: UpdatePositionVelocity(index, pair.Value, resultData); break;

                    // 01011 (11) - Position + Velocity + AngularVelocity
                    case 11: UpdatePositionVelocityAngularVelocity(index, pair.Value, resultData); break;

                    // 01100 (12) - Position + Rotation
                    case 12: UpdatePositionRotation(index, pair.Value, resultData); break;

                    // 01101 (13) - Position + Rotation + AngularVelocity
                    case 13: UpdatePositionRotationAngularVelocity(index, pair.Value, resultData); break;

                    // 01110 (14) - Position + Rotation + Velocity
                    case 14: UpdatePositionRotationVelocity(index, pair.Value, resultData); break;

                    // 01111 (15) - Position + Rotation + Velocity + AngularVelocity (без массы)
                    case 15: UpdateAllExceptMass(index, pair.Value, resultData); break;

                    // 10000 (16) - MassData (масса, drag, доп буфер)
                    case 16: UpdateMassDataOnly(index, pair.Value, resultData); break;

                    // 10001 (17) - AngularVelocity + MassData
                    case 17: UpdateAngularVelocityMass(index, pair.Value, resultData); break;

                    // 10010 (18) - Velocity + MassData
                    case 18: UpdateVelocityMass(index, pair.Value, resultData); break;

                    // 10011 (19) - Velocity + AngularVelocity + MassData
                    case 19: UpdateVelocityAngularVelocityMass(index, pair.Value, resultData); break;

                    // 10100 (20) - Rotation + MassData
                    case 20: UpdateRotationMass(index, pair.Value, resultData); break;

                    // 10101 (21) - Rotation + AngularVelocity + MassData
                    case 21: UpdateRotationAngularVelocityMass(index, pair.Value, resultData); break;

                    // 10110 (22) - Rotation + Velocity + MassData
                    case 22: UpdateRotationVelocityMass(index, pair.Value, resultData); break;

                    // 10111 (23) - Rotation + Velocity + AngularVelocity + MassData
                    case 23: UpdateRotationVelocityAngularVelocityMass(index, pair.Value, resultData); break;

                    // 11000 (24) - Position + MassData
                    case 24: UpdatePositionMass(index, pair.Value, resultData); break;

                    // 11001 (25) - Position + AngularVelocity + MassData
                    case 25: UpdatePositionAngularVelocityMass(index, pair.Value, resultData); break;

                    // 11010 (26) - Position + Velocity + MassData
                    case 26: UpdatePositionVelocityMass(index, pair.Value, resultData); break;

                    // 11011 (27) - Position + Velocity + AngularVelocity + MassData
                    case 27: UpdatePositionVelocityAngularVelocityMass(index, pair.Value, resultData); break;

                    // 11100 (28) - Position + Rotation + MassData
                    case 28: UpdatePositionRotationMass(index, pair.Value, resultData); break;

                    // 11101 (29) - Position + Rotation + AngularVelocity + MassData
                    case 29: UpdatePositionRotationAngularVelocityMass(index, pair.Value, resultData); break;

                    // 11110 (30) - Position + Rotation + Velocity + MassData
                    case 30: UpdatePositionRotationVelocityMass(index, pair.Value, resultData); break;

                    // 11111 (31) - Всё
                    case 31: UpdateAll(index, pair.Value, resultData); break;
                }
            }
        }

        #region МЕТОДЫ ОБНОВЛЕНИЯ

        #region Группа 1: только один компонент
        private void UpdateAngularVelocityOnly(int index, GameObject obj, MainBufferData data)
        {
            if (poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyAngularVelocity(data, body);
        }

        private void UpdateVelocityOnly(int index, GameObject obj, MainBufferData data)
        {
            if (poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyVelocity(data, body);
        }

        private void UpdateRotationOnly(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
        }

        private void UpdatePositionOnly(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
        }

        private void UpdateMassDataOnly(int index, GameObject obj, MainBufferData data)
        {
            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }
        #endregion
        #region Группа 2: два компонента
        private void UpdateVelocityAngularVelocity(int index, GameObject obj, MainBufferData data)
        {
            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyAngularVelocity(data, body);
            }
        }

        private void UpdateRotationAngularVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            if (poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyAngularVelocity(data, body);
        }

        private void UpdateRotationVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            if (poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyVelocity(data, body);
        }

        private void UpdatePositionAngularVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            if (poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyAngularVelocity(data, body);
        }

        private void UpdatePositionVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            if (poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyVelocity(data, body);
        }

        private void UpdatePositionRotation(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
        }
        #endregion
        #region Группа 3: три компонента
        private void UpdateRotationVelocityAngularVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyAngularVelocity(data, body);
            }
        }

        private void UpdatePositionVelocityAngularVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyAngularVelocity(data, body);
            }
        }

        private void UpdatePositionRotationAngularVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            if (poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyAngularVelocity(data, body);
        }

        private void UpdatePositionRotationVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            if (poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyVelocity(data, body);
        }

        private void UpdateAllExceptMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyAngularVelocity(data, body);
            }
        }
        #endregion
        #region Группа 4: с массой (один компонент + масса)
        private void UpdateAngularVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyAngularVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdateVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdateRotationMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);

            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdatePositionMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);

            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }
        #endregion
        #region Группа 5: с массой (два компонента + масса)
        private void UpdateVelocityAngularVelocityMass(int index, GameObject obj, MainBufferData data)
        {

            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyAngularVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdateRotationAngularVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);

            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyAngularVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdateRotationVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);

            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdatePositionAngularVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);

            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyAngularVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdatePositionVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);

            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdatePositionRotationMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);

            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }
        #endregion
        #region Группа 6: с массой (три компонента + масса)
        private void UpdateRotationVelocityAngularVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);

            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyAngularVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdatePositionVelocityAngularVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);

            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyAngularVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdatePositionRotationAngularVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);

            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyAngularVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdatePositionRotationVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);

            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }
        #endregion
        #region Группа 7: всё
        private void UpdateAll(int index, GameObject obj, MainBufferData data)
        {
            if (poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyAngularVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }
        #endregion

        private void UpdatePhysicsBodyMass(MainBufferData data, PhysicsBody body) => body.Mass = data.MassDragAdditionalFlags.x;
        private void UpdatePhysicsBodyDrag(MainBufferData data, PhysicsBody body) => body.Drag = data.MassDragAdditionalFlags.y;
        private void UpdatePhysicsBodyVelocity(MainBufferData data, PhysicsBody body) => body.Velocity = data.Velocity.xyz;
        private void UpdatePhysicsBodyAngularVelocity(MainBufferData data, PhysicsBody body) => body.AngularVelocity = data.AngularVelocity.xyz;


        #endregion

        public void ClearBuffers()
        {
            MainBuffer0?.Release();
            MainBuffer1?.Release();
            AdditionalBuffer0?.Release();
            AdditionalBuffer1?.Release();
            ActiveIndicesBuffer?.Release();
            TaskCounterBuffer?.Release();
            ActiveCountBuffer?.Release();
        }
    }
}
