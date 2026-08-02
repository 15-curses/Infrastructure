float3x3 EulerToRotationMatrix(float3 angles)
{
    float cx = cos(angles.x); float sx = sin(angles.x);
    float cy = cos(angles.y); float sy = sin(angles.y);
    float cz = cos(angles.z); float sz = sin(angles.z);

    float3x3 m;
    // Строка 0
    m[0][0] = cy * cz + sy * sx * sz;
    m[0][1] = cz * sy * sx - cy * sz;
    m[0][2] = cx * sy;
    
    // Строка 1
    m[1][0] = cx * sz;
    m[1][1] = cx * cz;
    m[1][2] = -sx;
    
    // Строка 2
    m[2][0] = cy * sx * sz - cz * sy;
    m[2][1] = cy * cz * sx + sy * sz;
    m[2][2] = cy * cx;
    
    return m;
}

/// <summary>
/// Выполняет шаг физической интеграции (Symplectic Euler) для обновления позиций и скоростей.
/// </summary>
void IntegrateObjectMovement(inout Object obj)
{
    // 0. Проверка на статику или сон
    if (obj.mass <= 0.0f || obj.updateMask == 0.0f)
    {
        return;
    }

    // 1. Внешние силы и Drag
    obj.velocity += Gravity * DeltaTime;
    
    float linDrag = max(1.0f - obj.staticFriction * DeltaTime, 0.0f);
    float angDrag = max(1.0f - obj.dynamicFriction * DeltaTime, 0.0f);
    obj.velocity        *= linDrag;
    obj.angularVelocity *= angDrag;

    // Извлекаем текущую матрицу вращения (по строкам)
    float3x3 currentRotMatrix = float3x3(
        obj.transform[0].xyz,
        obj.transform[1].xyz,
        obj.transform[2].xyz
    );

    // 2. Находим текущий МИРОВОЙ центр масс ДО движения
    // Используем mul(vector, matrix) для HLSL row-major логики строк-векторов
    float3 worldMassCenter = obj.position + mul(obj.massCenter, currentRotMatrix);

    // 3. Линейное движение
    worldMassCenter += obj.velocity * DeltaTime;

    // 4-5. Угловое движение (Исправлена кососимметричная матрица и порядок умножения)
    float3x3 deltaRotation = float3x3(
         0.0f,                   obj.angularVelocity.z, -obj.angularVelocity.y,
        -obj.angularVelocity.z,  0.0f,                    obj.angularVelocity.x,
         obj.angularVelocity.y, -obj.angularVelocity.x,  0.0f
    );
    
    // Поворот применяется К текущей матрице справа: R_new = R_old + R_old * [w]x * dt
    float3x3 newRotMatrix = currentRotMatrix + mul(currentRotMatrix, deltaRotation) * DeltaTime;
    
    // Ортонормализация Gram-Schmidt для строк матрицы
    newRotMatrix[0] = normalize(newRotMatrix[0]);
    newRotMatrix[1] = normalize(newRotMatrix[1] - newRotMatrix[0] * dot(newRotMatrix[1], newRotMatrix[0]));
    newRotMatrix[2] = normalize(cross(newRotMatrix[0], newRotMatrix[1])); // Добавлен normalize для Z

    // 6. Вычисляем НОВУЮ мировую позицию Pivot графики
    obj.position = worldMassCenter - mul(obj.massCenter, newRotMatrix);

    // 7. Обновляем итоговую матрицу transform 4x4 (Row-Major стандарт HLSL)
    obj.transform[0].xyz = newRotMatrix[0];
    obj.transform[1].xyz = newRotMatrix[1];
    obj.transform[2].xyz = newRotMatrix[2];
    obj.transform[3].xyz = obj.position; 
    
    obj.transform[0].w = 0.0f;
    obj.transform[1].w = 0.0f;
    obj.transform[2].w = 0.0f;
    obj.transform[3].w = 1.0f;
    
    // 8. Обновление мирового AABB из ЛОКАЛЬНОГО неизменяемого хитбокса (ИСПРАВЛЕНО)
    AABB worldAABB;
    worldAABB.min = obj.minAABB; // Используем локальный шаблон
    worldAABB.max = obj.maxAABB;
    
    // Трансформируем локальный AABB на основе новой позиции и вращения
    TransformAABB(worldAABB, obj.position, newRotMatrix);
    UpdateChunkIDsForAABB(worldAABB, obj.chunksIDs);
    
    // Сохраняем результат для Broadphase детекции коллизий
    obj.minAABB = worldAABB.min;
    obj.maxAABB = worldAABB.max;
}