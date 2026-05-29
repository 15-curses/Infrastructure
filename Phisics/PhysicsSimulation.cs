using Assets.Infrastructure.InputManager;
using System;
using System.Runtime.InteropServices;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Assets.Infrastructure.Phisics
{
    public class PhysicsSimulation : MonoBehaviour
    {
        #region Поля переменных
        [SerializeField] private int _maxParticleCount = 1000;
        [SerializeField] private ComputeShader _physicsComputeShader;
        [SerializeField] private Vector3 _gravity = new Vector3(0, -9.81f, 0);

        private int _mainBufferStride;
        private int _additionalBufferStride;
        private int _computeKernelId;
        private int _threadGroupsCount;

        private ComputeBuffer _readMainBuffer;
        private ComputeBuffer _writeMainBuffer;
        private ComputeBuffer _additionalBuffer;

        private MainBufferData[] _mainBufferData;
        private AdditionalBufferData[] _additionalBufferData;

        private ObjectPoolManager _poolManager;

        private const int ThreadsPerGroup = 64;
        #endregion

        #region Инициализация
        private void Awake()
        {
            _poolManager = new ObjectPoolManager(_maxParticleCount);

            InitializeBufferSizes();
            InitializeComputeBuffers();
            InitializeDataArrays();
            SetupComputeShader();

            InputSystem.SubMouse(MouseUpdate);
        }
        private void MouseUpdate(Vector2 mouseDelta)
            => _physicsComputeShader.SetFloats("_MouseDelta", mouseDelta.x, mouseDelta.y);

        private void InitializeBufferSizes()
        {
            _mainBufferStride = Marshal.SizeOf<MainBufferData>();
            _additionalBufferStride = Marshal.SizeOf<AdditionalBufferData>();
            _threadGroupsCount = Mathf.CeilToInt(_maxParticleCount / (float)ThreadsPerGroup);
        }

        private void InitializeComputeBuffers()
        {
            _writeMainBuffer = new ComputeBuffer(_maxParticleCount, _mainBufferStride);
            _readMainBuffer = new ComputeBuffer(_maxParticleCount, _mainBufferStride);
            _additionalBuffer = new ComputeBuffer(_maxParticleCount, _additionalBufferStride);
        }

        private void InitializeDataArrays()
        {
            _mainBufferData = new MainBufferData[_maxParticleCount];
            _additionalBufferData = new AdditionalBufferData[_maxParticleCount];
            Array.Clear(_mainBufferData, 0, _mainBufferData.Length);
            Array.Clear(_additionalBufferData, 0, _additionalBufferData.Length);
        }

        private void SetupComputeShader()
        {
            _computeKernelId = _physicsComputeShader.FindKernel("Physics");
            _physicsComputeShader.SetVector("_Gravity", _gravity);
        }
        #endregion

        #region FixedUpdate
        private void FixedUpdate()
        {
            SendDataToBuffers();
            DispatchComputeShader();
            ReadResultsFromBuffer();
            SwapBuffers();
        }
        
        private void SendDataToBuffers()
        {
            _writeMainBuffer.SetData(_mainBufferData);
            _additionalBuffer.SetData(_additionalBufferData);
        }

        private void DispatchComputeShader()
        {
            _physicsComputeShader.SetFloat("_DeltaTime", Time.fixedDeltaTime);
            _physicsComputeShader.SetBuffer(_computeKernelId, "MainBuffer", _writeMainBuffer);
            _physicsComputeShader.SetBuffer(_computeKernelId, "AdditionalBuffer", _additionalBuffer);
            _physicsComputeShader.Dispatch(_computeKernelId, _threadGroupsCount, 1, 1);
            _additionalBuffer.GetData(_additionalBufferData);
        }

        private void ReadResultsFromBuffer()
        {
            var results = new MainBufferData[_maxParticleCount];
            _readMainBuffer.GetData(results, 0, 0, _maxParticleCount);

            foreach (var pair in _poolManager.GetRegistry())
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
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            if (_poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyAngularVelocity(data, body);
        }

        private void UpdateVelocityOnly(int index, GameObject obj, MainBufferData data)
        {
            _mainBufferData[index].Velocity = data.Velocity;
            if (_poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyVelocity(data, body);
        }

        private void UpdateRotationOnly(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            _mainBufferData[index].Rotation = data.Rotation;
        }

        private void UpdatePositionOnly(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            _mainBufferData[index].Position = data.Position;
        }

        private void UpdateMassDataOnly(int index, GameObject obj, MainBufferData data)
        {
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }
        #endregion

        #region Группа 2: два компонента
        private void UpdateVelocityAngularVelocity(int index, GameObject obj, MainBufferData data)
        {
            _mainBufferData[index].Velocity = data.Velocity;
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            if (_poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyAngularVelocity(data, body);
            }
        }

        private void UpdateRotationAngularVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            _mainBufferData[index].Rotation = data.Rotation;
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            if (_poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyAngularVelocity(data, body);
        }

        private void UpdateRotationVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            _mainBufferData[index].Rotation = data.Rotation;
            _mainBufferData[index].Velocity = data.Velocity;
            if (_poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyVelocity(data, body);
        }

        private void UpdatePositionAngularVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            _mainBufferData[index].Position = data.Position;
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            if (_poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyAngularVelocity(data, body);
        }

        private void UpdatePositionVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            _mainBufferData[index].Position = data.Position;
            _mainBufferData[index].Velocity = data.Velocity;
            if (_poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyVelocity(data, body);
        }

        private void UpdatePositionRotation(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            _mainBufferData[index].Position = data.Position;
            _mainBufferData[index].Rotation = data.Rotation;
        }
        #endregion

        #region Группа 3: три компонента
        private void UpdateRotationVelocityAngularVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            _mainBufferData[index].Rotation = data.Rotation;
            _mainBufferData[index].Velocity = data.Velocity;
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            if (_poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyAngularVelocity(data, body);
            }
        }

        private void UpdatePositionVelocityAngularVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            _mainBufferData[index].Position = data.Position;
            _mainBufferData[index].Velocity = data.Velocity;
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            if (_poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyAngularVelocity(data, body);
            }
        }

        private void UpdatePositionRotationAngularVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            _mainBufferData[index].Position = data.Position;
            _mainBufferData[index].Rotation = data.Rotation;
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            if (_poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyAngularVelocity(data, body);
        }

        private void UpdatePositionRotationVelocity(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            _mainBufferData[index].Position = data.Position;
            _mainBufferData[index].Rotation = data.Rotation;
            _mainBufferData[index].Velocity = data.Velocity;
            if (_poolManager.TryGetPhysicsBody(index, out var body))
                UpdatePhysicsBodyVelocity(data, body);
        }

        private void UpdateAllExceptMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            _mainBufferData[index].Position = data.Position;
            _mainBufferData[index].Rotation = data.Rotation;
            _mainBufferData[index].Velocity = data.Velocity;
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            if (_poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyAngularVelocity(data, body);
            }
        }
        #endregion

        #region Группа 4: с массой (один компонент + масса)
        private void UpdateAngularVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyAngularVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdateVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            _mainBufferData[index].Velocity = data.Velocity;
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdateRotationMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            _mainBufferData[index].Rotation = data.Rotation;
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdatePositionMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            _mainBufferData[index].Position = data.Position;
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }
        #endregion

        #region Группа 5: с массой (два компонента + масса)
        private void UpdateVelocityAngularVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            _mainBufferData[index].Velocity = data.Velocity;
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
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
            _mainBufferData[index].Rotation = data.Rotation;
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyAngularVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdateRotationVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            _mainBufferData[index].Rotation = data.Rotation;
            _mainBufferData[index].Velocity = data.Velocity;
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdatePositionAngularVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            _mainBufferData[index].Position = data.Position;
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyAngularVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }

        private void UpdatePositionVelocityMass(int index, GameObject obj, MainBufferData data)
        {
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            _mainBufferData[index].Position = data.Position;
            _mainBufferData[index].Velocity = data.Velocity;
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
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
            _mainBufferData[index].Position = data.Position;
            _mainBufferData[index].Rotation = data.Rotation;
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
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
            _mainBufferData[index].Rotation = data.Rotation;
            _mainBufferData[index].Velocity = data.Velocity;
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
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
            _mainBufferData[index].Position = data.Position;
            _mainBufferData[index].Velocity = data.Velocity;
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
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
            _mainBufferData[index].Position = data.Position;
            _mainBufferData[index].Rotation = data.Rotation;
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
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
            _mainBufferData[index].Position = data.Position;
            _mainBufferData[index].Rotation = data.Rotation;
            _mainBufferData[index].Velocity = data.Velocity;
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
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
            obj.transform.position = new Vector3(data.Position.x, data.Position.y, data.Position.z);
            obj.transform.rotation = Quaternion.Euler(data.Rotation.x, data.Rotation.y, data.Rotation.z);
            _mainBufferData[index].Position = data.Position;
            _mainBufferData[index].Rotation = data.Rotation;
            _mainBufferData[index].Velocity = data.Velocity;
            _mainBufferData[index].AngularVelocity = data.AngularVelocity;
            UpdateMassDragAdditionalData(index, data);
            if (_poolManager.TryGetPhysicsBody(index, out var body))
            {
                UpdatePhysicsBodyVelocity(data, body);
                UpdatePhysicsBodyAngularVelocity(data, body);
                UpdatePhysicsBodyMass(data, body);
                UpdatePhysicsBodyDrag(data, body);
            }
        }
        #endregion

        private void UpdateMassDragAdditionalData(int index, MainBufferData data)
        {
            _mainBufferData[index].MassDragAdditionalFlags.x = data.MassDragAdditionalFlags.x;
            _mainBufferData[index].MassDragAdditionalFlags.y = data.MassDragAdditionalFlags.y;
            _mainBufferData[index].MassDragAdditionalFlags.z = data.MassDragAdditionalFlags.z;
        }

        private void UpdatePhysicsBodyMass(MainBufferData data, PhysicsBody body) => body.Mass = data.MassDragAdditionalFlags.x;
        private void UpdatePhysicsBodyDrag(MainBufferData data, PhysicsBody body) => body.Drag = data.MassDragAdditionalFlags.y;
        private void UpdatePhysicsBodyVelocity(MainBufferData data, PhysicsBody body) => body.Velocity = data.Velocity.xyz;
        private void UpdatePhysicsBodyAngularVelocity(MainBufferData data, PhysicsBody body) => body.AngularVelocity = data.AngularVelocity.xyz;

        private void SwapBuffers()
        {
            var temp = _writeMainBuffer;
            _writeMainBuffer = _readMainBuffer;
            _readMainBuffer = temp;
        }
        #endregion
        #endregion

        private void OnDestroy()
        {
            _readMainBuffer?.Release();
            _writeMainBuffer?.Release();
            _additionalBuffer?.Release();
        }

        #region ЛОГИКА ДОБАВЛЕНИЯ И УДАЛЕНИЯ ОБЕКТА
        public int AddPhysicsObject(GameObject gameObject, MainBufferData initialData, PhysicsBody body)
        {
            if (!_poolManager.TryGetMainSlot(out int index))
                return -1;

            _poolManager.RegisterGameObject(index, gameObject);

            var physicsBody = body 
                ?? gameObject.GetComponent<PhysicsBody>() 
                ?? gameObject.AddComponent<PhysicsBody>();

            _poolManager.RegisterPhysicsBody(index, physicsBody);
            _mainBufferData[index] = initialData;

            return index;
        }

        public void RemovePhysicsObject(int index)
        {
            if (index < 0 || index >= _maxParticleCount) return;

            _mainBufferData[index] = new MainBufferData();
            _poolManager.ReturnMainSlot(index);
        }

        public int RegisterAdditionalSlot(AdditionalBufferData additionalData)
        {
            if (_poolManager.TryGetAdditionalSlot(out int additionalIndex))
            {
                _additionalBufferData[additionalIndex] = additionalData;
                return additionalIndex;
            }
            return -1;

        }    
        public void LinkAdditionalToMain(int mainBuferIndex, int additionalIndex)
            => _poolManager.RegisterUnificationOfBuffers(mainBuferIndex, additionalIndex);
        
        #endregion

        #region Публичные методы для управления
        public void SetUpdateMask(int index, int mask)
        {
            if (index >= 0 && index < _maxParticleCount)
            {
                _mainBufferData[index].MassDragAdditionalFlags.w = mask;
            }
        }

        public int GetUpdateMask(int index)
        {
            return index >= 0 && index < _maxParticleCount ?
                (int)_mainBufferData[index].MassDragAdditionalFlags.w : -1;
        }
        #endregion
    }
}