#ifndef CHUNKS_LOGIC_H
#define CHUNKS_LOGIC_H

struct ChunkMinMax
 {
     float3 min_xyz;
     float3 max_xyz;
 };
struct Triangle
 {
     float3 vertinc0;
     float3 vertinc1;
     float3 vertinc2;
 };

bool InChunkTest(ChunkMinMax _chunk, Triangle _triangle)
{
    float3 cubeAxes[3] = {
        float3(1, 0, 0),
        float3(0, 1, 0),
        float3(0, 0, 1)
    };
    
    float3 edge0 = _triangle.vertinc1 - _triangle.vertinc0;
    float3 edge1 = _triangle.vertinc2 - _triangle.vertinc1;
    float3 edge2 = _triangle.vertinc0 - _triangle.vertinc2;
    
    float3 triangleNormal = cross(edge0, _triangle.vertinc2 - _triangle.vertinc0);
    
    float3 axes[7] = {
        cubeAxes[0], cubeAxes[1], cubeAxes[2],
        triangleNormal,
        cross(cubeAxes[0], edge0),
        cross(cubeAxes[1], edge0),
        cross(cubeAxes[2], edge0)
    };
    
    for (int i = 0; i < 7; i++)
    {
        float3 axis = axes[i];
        
        if (dot(axis, axis) < 0.0001)
            continue;
        
        axis = normalize(axis);
        
        float3 extent = (_chunk.max_xyz - _chunk.min_xyz) * 0.5;
        float3 center = (_chunk.min_xyz + _chunk.max_xyz) * 0.5;
        
        float chunkMin = dot(center, axis) - (
            abs(dot(extent, axis))
        );
        float chunkMax = dot(center, axis) + (
            abs(dot(extent, axis))
        );
        
        float triMin = min(
            min(dot(_triangle.vertinc0, axis), dot(_triangle.vertinc1, axis)),
            dot(_triangle.vertinc2, axis)
        );
        float triMax = max(
            max(dot(_triangle.vertinc0, axis), dot(_triangle.vertinc1, axis)),
            dot(_triangle.vertinc2, axis)
        );
        
        if (chunkMax < triMin || triMax < chunkMin)
            return false;
    }
    
    return true;
}
float3 VertincOfTriangle(uint vertincId)
{
    uint position = vertincId % 36;
    uint bufferId = vertincId / 36;
    
    switch (position)
    {
        case 0: return add_vertices_buffer[bufferId].vertices0;
        case 1: return add_vertices_buffer[bufferId].vertices1;
        case 2: return add_vertices_buffer[bufferId].vertices2;
        case 3: return add_vertices_buffer[bufferId].vertices3;
        case 4: return add_vertices_buffer[bufferId].vertices4;
        case 5: return add_vertices_buffer[bufferId].vertices5;
        case 6: return add_vertices_buffer[bufferId].vertices6;
        case 7: return add_vertices_buffer[bufferId].vertices7;
        case 8: return add_vertices_buffer[bufferId].vertices8;
        case 9: return add_vertices_buffer[bufferId].vertices9;
        case 10: return add_vertices_buffer[bufferId].vertices10;
        case 11: return add_vertices_buffer[bufferId].vertices11;
        case 12: return add_vertices_buffer[bufferId].vertices12;
        case 13: return add_vertices_buffer[bufferId].vertices13;
        case 14: return add_vertices_buffer[bufferId].vertices14;
        case 15: return add_vertices_buffer[bufferId].vertices15;
        case 16: return add_vertices_buffer[bufferId].vertices16;
        case 17: return add_vertices_buffer[bufferId].vertices17;
        case 18: return add_vertices_buffer[bufferId].vertices18;
        case 19: return add_vertices_buffer[bufferId].vertices19;
        case 20: return add_vertices_buffer[bufferId].vertices20;
        case 21: return add_vertices_buffer[bufferId].vertices21;
        case 22: return add_vertices_buffer[bufferId].vertices22;
        case 23: return add_vertices_buffer[bufferId].vertices23;
        case 24: return add_vertices_buffer[bufferId].vertices24;
        case 25: return add_vertices_buffer[bufferId].vertices25;
        case 26: return add_vertices_buffer[bufferId].vertices26;
        case 27: return add_vertices_buffer[bufferId].vertices27;
        case 28: return add_vertices_buffer[bufferId].vertices28;
        case 29: return add_vertices_buffer[bufferId].vertices29;
        case 30: return add_vertices_buffer[bufferId].vertices30;
        case 31: return add_vertices_buffer[bufferId].vertices31;
        case 32: return add_vertices_buffer[bufferId].vertices32;
        case 33: return add_vertices_buffer[bufferId].vertices33;
        case 34: return add_vertices_buffer[bufferId].vertices34;
        case 35: return add_vertices_buffer[bufferId].vertices35;
    }
}

void InChunk(uint chunkId, uint objId) // TODO: сделать проверку для всех чанков по условию, оптимизировать и проверить можно ли чанки для проверки иначе определять (без перебора) 
{
    ChunkMinMax chunkMinMax
    {
        chunks_buffer[chunkId].min_xyz,
        chunks_buffer[chunkId].max_xyz
    };
    
    uint startTrianglIndex = mesh_add_indexer_data[objId].startTrianglIndex;
    uint endTrianglIndex = mesh_add_indexer_data[objId].endTrianglIndex;
    uint counter = 0;
    uint obj_counter;
    
    while (true)
    {
        uint add_trianglesId = counter + startTrianglIndex;
        if (add_trianglesId == endTrianglIndex) return;
        TrianglesData triangleData  = add_triangles_buffer[add_trianglesId];
        float3 _triangle;
        
        for (int i = 0; i < 36; i++)
        {
            switch (i)
            {
                case 0: _triangle = triangleData.triangle0; break;
                case 1: _triangle = triangleData.triangle1; break;
                case 2: _triangle = triangleData.triangle2; break;  
                case 3: _triangle = triangleData.triangle3; break;
                case 4: _triangle = triangleData.triangle4; break;
                case 5: _triangle = triangleData.triangle5; break;
                case 6: _triangle = triangleData.triangle6; break;
                case 7: _triangle = triangleData.triangle7; break;
                case 8: _triangle = triangleData.triangle8; break;
                case 9: _triangle = triangleData.triangle9; break;
                case 10: _triangle = triangleData.triangle10; break;
                case 11: _triangle = triangleData.triangle11; break;
                case 12: _triangle = triangleData.triangle12; break;
                case 13: _triangle = triangleData.triangle13; break;
                case 14: _triangle = triangleData.triangle14; break;
                case 15: _triangle = triangleData.triangle15; break;
                case 16: _triangle = triangleData.triangle16; break;
                case 17: _triangle = triangleData.triangle17; break;
                case 18: _triangle = triangleData.triangle18; break;
                case 19: _triangle = triangleData.triangle19; break;
                case 20: _triangle = triangleData.triangle20; break;
                case 21: _triangle = triangleData.triangle21; break;
                case 22: _triangle = triangleData.triangle22; break;
                case 23: _triangle = triangleData.triangle23; break;
                case 24: _triangle = triangleData.triangle24; break;
                case 25: _triangle = triangleData.triangle25; break;
                case 26: _triangle = triangleData.triangle26; break;
                case 27: _triangle = triangleData.triangle27; break;
                case 28: _triangle = triangleData.triangle28; break;
                case 29: _triangle = triangleData.triangle29; break;
                case 30: _triangle = triangleData.triangle30; break;
                case 31: _triangle = triangleData.triangle31; break;
                case 32: _triangle = triangleData.triangle32; break;
                case 33: _triangle = triangleData.triangle33; break;
                case 34: _triangle = triangleData.triangle34; break;
                case 35: _triangle = triangleData.triangle35; break;
            }
            
            Triangle Triangle
            {
                VertincOfTriangle(_triangle.x),
                VertincOfTriangle(_triangle.y),
                VertincOfTriangle(_triangle.z)
            };
            
            if (InChunkTest(chunkMinMax, Triangle))
            {
                InterlockedAdd(chunks_obj_counter[1], 1, obj_counter); // TODO: добавить логику для переполнения сегмента буфера (чанка) 
                    
                switch (obj_counter)
                {
                    case 1: chunks_buffer[chunkId].obj0 = objId;
                    case 2: chunks_buffer[chunkId].obj1 = objId;
                    case 3: chunks_buffer[chunkId].obj2 = objId;
                    case 4: chunks_buffer[chunkId].obj3 = objId;
                    case 5: chunks_buffer[chunkId].obj4 = objId;
                    case 6: chunks_buffer[chunkId].obj5 = objId;
                    case 7: chunks_buffer[chunkId].obj6 = objId;
                    case 8: chunks_buffer[chunkId].obj7 = objId;
                    case 9: chunks_buffer[chunkId].obj8 = objId;
                    case 10: chunks_buffer[chunkId].obj9 = objId;
                    case 11: chunks_buffer[chunkId].obj10 = objId;
                    case 12: chunks_buffer[chunkId].obj11 = objId;
                    case 13: chunks_buffer[chunkId].obj12 = objId;
                    case 14: chunks_buffer[chunkId].obj13 = objId;
                    case 15: chunks_buffer[chunkId].obj14 = objId;
                    case 16: chunks_buffer[chunkId].obj15 = objId;
                    case 17: chunks_buffer[chunkId].obj16 = objId;
                    case 18: chunks_buffer[chunkId].obj17 = objId;
                    case 19: chunks_buffer[chunkId].obj18 = objId;
                    case 20: chunks_buffer[chunkId].obj19 = objId;
                    case 21: chunks_buffer[chunkId].obj20 = objId;
                    case 22: chunks_buffer[chunkId].obj21 = objId;
                    case 23: chunks_buffer[chunkId].obj22 = objId;
                    case 24: chunks_buffer[chunkId].obj23 = objId;
                    case 25: chunks_buffer[chunkId].obj24 = objId;
                    case 26: chunks_buffer[chunkId].obj25 = objId;
                    case 27: chunks_buffer[chunkId].obj26 = objId;
                    case 28: chunks_buffer[chunkId].obj27 = objId;
                    case 29: chunks_buffer[chunkId].obj28 = objId;
                    case 30: chunks_buffer[chunkId].obj29 = objId;
                    case 31: chunks_buffer[chunkId].obj30 = objId;
                    case 32: chunks_buffer[chunkId].obj31 = objId;
                    case 33: chunks_buffer[chunkId].obj32 = objId;
                    case 34: chunks_buffer[chunkId].obj33 = objId;
                    case 35: chunks_buffer[chunkId].obj34 = objId;
                    case 36: chunks_buffer[chunkId].obj35 = objId;
                    default: ;
                }
                
                return;
            }
        }
        counter++;
    }
}

int DivCeil(int a, int b) { return (a + b - 1) / b; }

void GetAABBChunkIDs(float3 minPoint, float3 maxPoint, float3 chunksCount, float chunkSize,
                     int3 chunkOffset, uint objId)
{
    int minChunkIdxX = (int)(minPoint.x / chunkSize) + chunkOffset.x;
    int minChunkIdxY = (int)(minPoint.y / chunkSize) + chunkOffset.y;
    int minChunkIdxZ = (int)(minPoint.z / chunkSize) + chunkOffset.z;
    
    int maxChunkIdxX = (int)(maxPoint.x / chunkSize) + chunkOffset.x;
    int maxChunkIdxY = (int)(maxPoint.y / chunkSize) + chunkOffset.y;
    int maxChunkIdxZ = (int)(maxPoint.z / chunkSize) + chunkOffset.z;
    
    uint countX = (uint)chunksCount.x;
    uint countY = (uint)chunksCount.y;
    uint countZ = (uint)chunksCount.z;
    
    minChunkIdxX = max(minChunkIdxX, 0);
    minChunkIdxY = max(minChunkIdxY, 0);
    minChunkIdxZ = max(minChunkIdxZ, 0);
    
    maxChunkIdxX = min(maxChunkIdxX, (int)countX - 1);
    maxChunkIdxY = min(maxChunkIdxY, (int)countY - 1);
    maxChunkIdxZ = min(maxChunkIdxZ, (int)countZ - 1);
    
    for (int chunkIdxY = minChunkIdxY; chunkIdxY <= maxChunkIdxY; chunkIdxY++)
    {
        for (int chunkIdxZ = minChunkIdxZ; chunkIdxZ <= maxChunkIdxZ; chunkIdxZ++)
        {
            for (int chunkIdxX = minChunkIdxX; chunkIdxX <= maxChunkIdxX; chunkIdxX++)
            {
                uint chunkBufferIndex = (uint)(chunkIdxY * (int)countZ * (int)countX + 
                                          chunkIdxZ * (int)countX + 
                                          chunkIdxX);
                InChunk(chunkBufferIndex, objId);    
            }
        }
    }
}

void AddMesh(uint objId,
    MeshAddIndexerData mesh_add_data) // TODO: ЧЗХ что в чанк логике делает метод AddVerticesAndTriangles
{
    MeshIndexerData mesh_data;
    uint verticeCounter = 0;
    for (uint addVerticeIndex = mesh_add_data.startVerticeIndex;
        addVerticeIndex < mesh_add_data.endVerticeIndex; addVerticeIndex++)
    {
        uint verticesIndex;
        if (GetFreeIndex(verticesIndex, free_vertices_indices, free_vertices_count))
        {
            vertices_buffer[verticesIndex] = add_vertices_buffer[addVerticeIndex];
            switch (verticeCounter)
            {
                case 0: mesh_data.verticeBufferID0 = verticesIndex; break;
                case 1: mesh_data.verticeBufferID1 = verticesIndex; break;
                case 2: mesh_data.verticeBufferID2 = verticesIndex; break;
                case 3: mesh_data.verticeBufferID3 = verticesIndex; break;
                case 4: mesh_data.verticeBufferID4 = verticesIndex; break;
                case 5: mesh_data.verticeBufferID5 = verticesIndex; break;
                default:  ; // TODO: ошибку в пул ошибок и чзх откуда столько точек?
            }
            verticeCounter++;
        }
        else{} // TODO: ошибку в пул ошибок и очишение буфера от новых-мусорных индексов
    }
    
    uint trianglesCounter = 0;
    for (uint addTriangleIndex = mesh_add_data.startTrianglIndex;
        addTriangleIndex < mesh_add_data.endTrianglIndex; addTriangleIndex++)
    {
        uint trianglesIndex;
        if (GetFreeIndex(trianglesIndex, free_triangles_indices, free_triangles_count))
        {
            triangles_buffer[trianglesIndex] = add_triangles_buffer[addTriangleIndex];
            switch (trianglesCounter)
            {
                case 0: mesh_data.trianglesBufferID0 = trianglesIndex; break;
                case 1: mesh_data.trianglesBufferID1 = trianglesIndex; break;
                case 2: mesh_data.trianglesBufferID2 = trianglesIndex; break;
                case 3: mesh_data.trianglesBufferID3 = trianglesIndex; break;
                case 4: mesh_data.trianglesBufferID4 = trianglesIndex; break;
                case 5: mesh_data.trianglesBufferID5 = trianglesIndex; break;
                case 6: mesh_data.trianglesBufferID6 = trianglesIndex; break;
                case 7: mesh_data.trianglesBufferID7 = trianglesIndex; break;
                default:  ; // TODO: ошибку в пул ошибок и чзх откуда столько триугол?
            }
            trianglesCounter++;
        }
        else{} // TODO: ошибку в пул ошибок и очишение буфера от новых-мусорных индексов
    }
    mesh_data.center = mesh_add_data.center;
    mesh_data.cubSize = mesh_add_data.cubSize;
    mesh_data.colliderType = mesh_add_data.colliderType;
    mesh_data.freeInf = mesh_add_data.freeInf;
    
    uint count;
    InterlockedAdd(mesh_buffer_counter[0], 1, count);
    mesh_buffer[count] = mesh_data;
}

void AddObjMesh(uint objId)
{   
    MeshAddIndexerData mesh_add_data = mesh_add_indexer_data[objId];
    float3 halfSize = mesh_add_data.cubSize / 2;
    float3 min = mesh_add_data.center - halfSize;
    float3 max = mesh_add_data.center + halfSize;
    
    GetAABBChunkIDs(min, max, chunksCount, chunk_Size, chunkOffset, objId);
    
    AddMesh(objId, mesh_add_data);
}



#endif
