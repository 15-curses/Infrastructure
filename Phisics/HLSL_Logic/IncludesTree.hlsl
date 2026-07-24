#pragma region Дата - Буферы
    #include "Data.hlsl"
    #include "Buffers.hlsl"
#pragma endregion

#pragma region Вспомогательные модули:
    #include "FreeIndices/Logic.hlsl"
    #include "FreeIndices/AddBuffersLogic.hlsl"
#pragma endregion

#pragma region Алгаритмы для колайдеров:
    #include "Chunks/Colliders/TestContactAlgorithms/GJK.hlsl"
    #include "Chunks/Colliders/TestContactAlgorithms/AABB.hlsl"
    #include "Chunks/Colliders/TestContactAlgorithms/СontactСheck.hlsl"
    #include "Chunks/Colliders/Movement/СreatMovementTask.hlsl"
#pragma endregion

#pragma region Чанки:
    #include "Chunks/MoveInChanks.hlsl"
#pragma endregion

#pragma region Movment:
    #include "Chunks/Colliders/Movement/IntegrateObjectMovement.hlsl"
#pragma endregion
