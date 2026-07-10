using Assets.Infrastructure.InputManager;
using Assets.Infrastructure.Phisics.Chunk_Logic;
using Assets.Infrastructure.Phisics.IndexOrchestrator;
using Assets.Infrastructure.Phisics.Move_Logic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Infrastructure.Phisics
{
    public class PhysicsMain : MonoBehaviour
    {
        #region Поля переменных
        [SerializeField] private int mainBufferCount = 1000;
        [SerializeField] private int additionalBufferCount = 1000;

        [SerializeField] private int3 _chunkSize;
        [SerializeField] private int3 _rangeSize;



        [SerializeField] private ComputeShader _physicsComputeShader;
        [SerializeField] private Vector3 _gravity = new Vector3(0, -9.81f, 0);

        private int _computeKernelId;


        private ChunkBuffer_Logic _chunkLogic;
        public Move_Buffers_Logic _moveLogic { get; private set; }

        private const int ThreadsPerGroup = 64;

        private Vector2 _accumulatedMouseDelta;
        #endregion

        #region Инициализация
        private void Awake()
        {
            InitializeClass();
            InitializeComputeBuffers();
            SetupComputeShader();
            MouseSub();
        }
        private void MouseSub() => InputSystem.SubMouse(MouseUpdate);
        private void MouseUpdate(Vector2 mouseDelta)
        {
            Vector2 normalizedDelta = new Vector2(
                mouseDelta.x / Screen.width,
                mouseDelta.y / Screen.height
            );

            _accumulatedMouseDelta += normalizedDelta;
        }

        private void InitializeClass()
        {
            _chunkLogic = new(_rangeSize, _chunkSize);
            _moveLogic = new(mainBufferCount, additionalBufferCount);
        }

        private void InitializeComputeBuffers()
        {
            _chunkLogic.Initialize();
            _moveLogic.Initialize();
            Orchestrator_Logic.Initialize();
        }

        private void SetupComputeShader()
        {
            _computeKernelId = _physicsComputeShader.FindKernel("PhysicsSimulation");
            _physicsComputeShader.SetVector("Gravity", _gravity);
        }
        #endregion

        #region FixedUpdate
        private void FixedUpdate()
        {
            DispatchComputeShader();
            _moveLogic.Move_Update();
        }

        private void DispatchComputeShader()
        {
            _physicsComputeShader.SetBuffer(_computeKernelId, "IndexOrchestratorBuffer", Orchestrator_Logic.IndexsBuffer);

            _physicsComputeShader.SetFloat("DeltaTime", Time.fixedDeltaTime);
            _physicsComputeShader.SetBuffer(_computeKernelId, "PhysicsObjectBuffer", _moveLogic.MainBuffer1);
            _physicsComputeShader.SetBuffer(_computeKernelId, "PhysicsAdditionalDataBuffer", _moveLogic.AdditionalBuffer1);
            _physicsComputeShader.SetBuffer(_computeKernelId, "ActiveObjectIndices", _moveLogic.ActiveIndicesBuffer);
            _physicsComputeShader.SetBuffer(_computeKernelId, "TaskCounter", _moveLogic.TaskCounterBuffer);
            _physicsComputeShader.SetBuffer(_computeKernelId, "ActiveObjectCount", _moveLogic.ActiveCountBuffer);

            _physicsComputeShader.SetBuffer(_computeKernelId, "ChunksBuffer", _chunkLogic.ChunksBuffer);

            bool hasMouse = _accumulatedMouseDelta.sqrMagnitude > 0f;
            if (hasMouse) _physicsComputeShader.SetFloats("MouseDelta", _accumulatedMouseDelta.x, _accumulatedMouseDelta.y, 1f);
            else _physicsComputeShader.SetFloats("MouseDelta", 0f, 0f, 0f);
            _accumulatedMouseDelta = Vector2.zero;

            int groups = Mathf.Clamp(Mathf.CeilToInt(_moveLogic.activeCount / (float)ThreadsPerGroup), 1, 256);
            _physicsComputeShader.Dispatch(_computeKernelId, groups, 1, 1);
        }
        #endregion
    }
}