/// <summary>
/// Представляет ограничивающий параллелепипед (AABB) в пространстве.
/// Используется для быстрой проверки коллизий между объектами.
/// </summary>
struct AABB
{
    /// <summary>Минимальная точка (нижний-левый-передний угол) параллелепипеда.</summary>
    float3 min;
    
    /// <summary>Максимальная точка (верхний-правый-задний угол) параллелепипеда.</summary>
    float3 max;
};

/// <summary>
/// Трансформирует локальный AABB в мировой на основе позиции и готовой матрицы вращения.
/// </summary>
/// <remarks>
/// Использует алгоритм Джима Арво (Jim Arvo) для быстрого вычисления минимально объемлющего 
/// мирового хитбокса без явного поворота всех 8 вершин объекта.
/// </remarks>
/// <param name="box">Локальный неизменяемый AABB объекта (на входе), Мировой AABB (на выходе).</param>
/// <param name="worldPos">Мировая позиция графического центра (Pivot) объекта.</param>
/// <param name="rotMat">Текущая мировая матрица вращения 3x3.</param>
void TransformAABB(inout AABB box, float3 worldPos, float3x3 rotMat)
{
    float3 localCenter  = (box.min + box.max) * 0.5f;
    float3 localExtents = (box.max - box.min) * 0.5f;
    float3 worldCenter  = mul(localCenter, rotMat) + worldPos;
    float3 worldExtents;
    worldExtents.x = dot(abs(rotMat[0]), localExtents);
    worldExtents.y = dot(abs(rotMat[1]), localExtents);
    worldExtents.z = dot(abs(rotMat[2]), localExtents);
    box.min = worldCenter - worldExtents;
    box.max = worldCenter + worldExtents;
}

/// <summary>
/// Проверяет пересечение двух ограничивающих параллелепипедов (AABB) в 3D пространстве с учетом их трансформации.
/// Автоматически переводит локальные боксы в мировые координаты на основе их позиции и углов поворота Эйлера.
/// </summary>
/// <param name="boxA">Исходный локальный AABB первого объекта.</param>
/// <param name="boxB">Исходный локальный AABB второго объекта.</param>
/// <returns>true, если трансформированные мировые параллелепипеды пересекаются; иначе false.</returns>
/// <remarks>
/// Внутри функции вызывается TransformAABB для пересчета габаритов боксов с учетом вращения, 
/// после чего применяется теорема о разделяющих осях (SAT) для трех глобальных осей (X, Y, Z).
/// Масштаб обоих объектов должен быть равен (1,1,1).
/// </remarks>
bool CheckAABBCollision3D(
    AABB boxA, AABB boxB)
{ 
    return (boxA.min.x <= boxB.max.x && boxA.max.x >= boxB.min.x) &&
           (boxA.min.y <= boxB.max.y && boxA.max.y >= boxB.min.y) &&
           (boxA.min.z <= boxB.max.z && boxA.max.z >= boxB.min.z);
}
