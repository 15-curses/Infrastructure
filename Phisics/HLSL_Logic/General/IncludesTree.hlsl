#pragma region Data & Buffers
#include "Data.hlsl"
#include "Buffers.hlsl"
#pragma endregion

#pragma region Helper Modules
#include "Assets/Infrastructure/Phisics/HLSL_Logic/General/FreeIndices/Logic.hlsl"
#include "Assets/Infrastructure/Phisics/HLSL_Logic/General/FreeIndices/AddBuffersLogic.hlsl"
#include "BasicCounter.hlsl"
#pragma endregion

#pragma region Collider Algorithms
#include "Assets/Infrastructure/Phisics/HLSL_Logic/Logic/Colliders/TestContactAlgorithms/GJK.hlsl"
#include "Assets/Infrastructure/Phisics/HLSL_Logic/Logic/Colliders/TestContactAlgorithms/AABB.hlsl"
#include "Assets/Infrastructure/Phisics/HLSL_Logic/Logic/Colliders/TestContactAlgorithms/СontactСheck.hlsl"
#pragma endregion

#pragma region Movement Task
#include "Assets/Infrastructure/Phisics/HLSL_Logic/Logic/Movement/СreatMovementTask.hlsl"
#pragma endregion

#pragma region Chunks
#include "Assets/Infrastructure/Phisics/HLSL_Logic/Logic/Chunks/MoveInChanks.hlsl"
#pragma endregion

#pragma region Movement
#include "Assets/Infrastructure/Phisics/HLSL_Logic/Logic/Movement/IntegrateObjectMovement.hlsl"
#include "Assets/Infrastructure/Phisics/HLSL_Logic/Logic/Movement/MathUtils.hlsl"
#include "Assets/Infrastructure/Phisics/HLSL_Logic/Logic/Movement/MoveVariants.hlsl"
#include "Assets/Infrastructure/Phisics/HLSL_Logic/Main/VelocityKernel/MoveShaders.hlsl"
#pragma endregion

#pragma region TouchCheck
#include "FindChunks.hlsl"
#include "BuildGrid.hlsl"
#include "TouchCheck.hlsl"
#pragma endregion