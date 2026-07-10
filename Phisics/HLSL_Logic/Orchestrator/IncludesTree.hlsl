#include "Constants.hlsl"
#include "OrcData.hlsl"
#include "Assets/Infrastructure/Phisics/HLSL_Logic/Movement/MoveData.hlsl"
#include "Assets/Infrastructure/Phisics/HLSL_Logic/BackInfToCpu/BackData.hlsl"
#include "Assets/Infrastructure/Phisics/HLSL_Logic/Chunks/ChunksData.hlsl"
#include "Assets/Infrastructure/Phisics/HLSL_Logic/Colliders/CollidersData.hlsl"
#include "Assets/Infrastructure/Phisics/HLSL_Logic/FreeIndices/Buffers.hlsl"
#include "Assets/Infrastructure/Phisics/HLSL_Logic/FreeIndices/Logic.hlsl"


#include "Assets/Infrastructure/Phisics/HLSL_Logic/Movement/MathUtils.hlsl"

#include "Buffers.hlsl"

#include "Assets/Infrastructure/Phisics/HLSL_Logic/Chunks/ChunkLogic.hlsl"

#include "Assets/Infrastructure/Phisics/HLSL_Logic/BackInfToCpu/BackLogic.hlsl"
#include "Assets/Infrastructure/Phisics/HLSL_Logic/Movement/MoveVariants.hlsl"
#include "Assets/Infrastructure/Phisics/HLSL_Logic/Movement/MovmentLogic.hlsl"
#include "ReadOperation.hlsl"