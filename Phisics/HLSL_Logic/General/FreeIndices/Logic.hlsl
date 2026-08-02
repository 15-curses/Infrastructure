/// <summary>
/// Добавляет индекс в список свободных позиций буфера.
/// </summary>
/// <param name="index">Индекс освобождаемой позиции в буфере.</param>
/// <param name="FreeIndices">Структурированный буфер, содержащий индексы свободных ячеек.</param>
/// <param name="FreeIndexCount">Буфер с счетчиком количества свободных индексов.</param>
void AddFreeIndex(uint index, RWStructuredBuffer<uint> FreeIndices, uint FreeIndexCount)
{
    uint count;
    InterlockedAdd(FreeIndexCount, 1, count);
    FreeIndices[count] = index;
}

/// <summary>
/// Получает индекс из пула свободных позиций буфера.
/// </summary>
/// <param name="index">Выходной параметр: полученный свободный индекс.</param>
/// <param name="FreeIndices">Структурированный буфер со свободными индексами.</param>
/// <param name="FreeIndexCount">Буфер с счетчиком свободных индексов.</param>
/// <returns>true, если свободный индекс найден; false, если пул пуст.</returns>
bool GetFreeIndex(out uint index, RWStructuredBuffer<uint> FreeIndices, uint FreeIndexCount)
{
    uint count;
    InterlockedAdd(FreeIndexCount, -1, count);
    if (count > 0)
    {
        index = FreeIndices[count - 1];
        return true;
    }
    return false;
}