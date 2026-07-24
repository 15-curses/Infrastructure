/// <summary>
/// Выполняет двухэтапную проверку коллизии между двумя объектами (Broad Phase -> Narrow Phase).
/// </summary>
/// <remarks>
/// Сначала метод проводит быструю фильтрацию через ограничивающие параллелепипеды (AABB). 
/// При обнаружении пересечения запускается точный алгоритм GJK для вычисления геометрического контакта.
/// <para>⚠️ <b>Внимание:</b> Убедитесь, что при инициализации <c>boxA</c> и <c>boxB</c> вторым аргументом передается <c>maxAABB</c>, а не <c>minAABB</c>.</para>
/// </remarks>
/// <param name="objId0">Идентификатор первого объекта в буфере <c>ObjectBuffer</c>.</param>
/// <param name="objId1">Идентификатор второго объекта в буфере <c>ObjectBuffer</c>.</param>
/// <param name="gjkResult">Выходной параметр, содержащий детальные результаты точной проверки геометрии (точки контакта, дистанция).</param>
/// <returns>
/// Возвращает <see langword="true"/>, если AABB-контейнеры объектов пересекаются и был выполнен точный расчет; 
/// в противном случае — <see langword="false"/>.
/// </returns>
bool СontactСheck(uint objId0, uint objId1, out GJKResult3D gjkResult)
{
    Object obj0 = ObjectBuffer[objId0];
    Object obj1 = ObjectBuffer[objId1];
    
    AABB boxA;
    boxA.min = obj0.minAABB;
    boxA.max = obj0.maxAABB;

    AABB boxB;
    boxB.min = obj1.minAABB;
    boxB.max = obj1.maxAABB;

    if (CheckAABBCollision3D(boxA, boxB))
    {
        uint obj0BufferIndex = obj0.objBufferIndex;
        uint obj1BufferIndex = obj1.objBufferIndex;
        uint vertexCount0 = MeshBuffer[obj0BufferIndex].vertexCount;
        uint vertexCount1 = MeshBuffer[obj1BufferIndex].vertexCount;
                    
        gjkResult = GetClosestPoints3D(obj0BufferIndex, vertexCount0, obj1BufferIndex, vertexCount1);
        
        return true;
    }
    return false;
}