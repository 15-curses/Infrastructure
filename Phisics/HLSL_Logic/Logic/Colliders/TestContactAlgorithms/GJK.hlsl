/// <summary>
/// Результат проверки столкновения между двумя объектами по GJK алгоритму.
/// Содержит информацию о расстоянии, ближайших точках и параметрах контакта.
/// Используется как возвращаемое значение методом <see cref="GetClosestPoints3D"/>.
/// При пересечении фигур (intersects = true) distance равно 0, pointA и pointB совпадают в области перекрытия,
/// а normal указывает направление разделения.
/// </summary>
struct GJKResult3D
{
    /// <summary>
    /// Кратчайшее расстояние между фигурами. 
    /// Значение: >= 0. Равно 0 если фигуры пересекаются (intersects = true).
    /// </summary>
    float distance;
    
    /// <summary>
    /// Ближайшая точка на первой фигуре (A).
    /// При пересечении: точка в области перекрытия.
    /// При разделении: точка на поверхности фигуры A, ближайшая к фигуре B.
    /// </summary>
    float3 pointA;
    
    /// <summary>
    /// Ближайшая точка на второй фигуре (B).
    /// При пересечении: совпадает с pointA.
    /// При разделении: точка на поверхности фигуры B, ближайшая к фигуре A.
    /// </summary>
    float3 pointB;
    
    /// <summary>
    /// Флаг пересечения фигур.
    /// true: фигуры пересекаются, distance = 0.
    /// false: фигуры разделены, distance > 0.
    /// </summary>
    bool intersects;
    
    /// <summary>
    /// Нормаль к поверхности касания (единичный вектор).
    /// Указывает направление отталкивания от фигуры B относительно A.
    /// При разделении фигур: нормализованное направление от pointB к pointA.
    /// При пересечении: направление минимального разделения.
    /// </summary>
    float3 normal;
    
    /// <summary>
    /// Точка контакта между фигурами.
    /// Вычисляется как средняя точка: (pointA + pointB) / 2.
    /// При разделении: лежит на "поверхности контакта" между мешами.
    /// При пересечении: находится в зоне перекрытия.
    /// </summary>
    float3 contactPoint;
};

/// <summary>
/// Извлекает вершину из буфера вершин по индексу.
/// Вершины хранятся в упакованном формате: 36 вершин на один элемент буфера.
/// </summary>
/// <param name="meshId">Идентификатор меша в mesh_buffer</param>
/// <param name="vertexNum">Глобальный индекс вершины в меше</param>
/// <returns>Координаты вершины в 3D пространстве</returns>
float3 TakeVertex(uint meshId, uint vertexNum)
{
    uint vertexBufferID = vertexNum / 36;
    uint vertexInBufferID = vertexNum % 36;
    
    uint4 indices = MeshBuffer[meshId].verticesBuffer[vertexBufferID];
    
    return MeshVerticesBuffer[indices].vertices[vertexInBufferID];
}

/// <summary>
/// Находит опорную вершину фигуры в заданном направлении (поддержка GJK алгоритма).
/// Опорная вершина — это вершина с максимальным скалярным произведением с направлением поиска.
/// Используется для построения симплекса в алгоритме GJK.
/// </summary>
/// <param name="dir">Направление поиска (нормализованный вектор)</param>
/// <param name="meshId">Идентификатор меша</param>
/// <param name="vertexCount">Количество вершин в меше</param>
/// <returns>Опорная вершина — ближайшая вершина к границе фигуры в направлении dir</returns>
float3 SupportShape3DAB(float3 dir, uint meshId, uint vertexCount)
{
    float3 support = float3(0, 0, 0);
    float maxDot = -1e9;
    
    for (int vertexNum = 0; vertexNum < vertexCount; ++vertexNum)
    {
        float3 vertex = TakeVertex(meshId, vertexNum);
        float d = dot(vertex, dir);
        
        if (d > maxDot)
        {
            maxDot = d;
            support = vertex;
        }
    }
    
    return support;
}

/// <summary>
/// Проецирует начало координат (origin) на отрезок в 3D пространстве.
/// Вычисляет параметр позиции на отрезке и ближайшую точку.
/// </summary>
/// <param name="w0">Начальная точка отрезка</param>
/// <param name="w1">Конечная точка отрезка</param>
/// <param name="t">Выходной параметр. Параметр позиции на отрезке в диапазоне [0.0, 1.0]. 
/// Значение 0.0 соответствует w0, значение 1.0 соответствует w1</param>
/// <param name="p">Выходной параметр. Ближайшая точка на отрезке</param>
void ProjectOntoSegment(float3 w0, float3 w1, out float t, out float3 p)
{
    float3 edge = w1 - w0;
    float denom = dot(edge, edge);
    t = denom < 0.00001 ? 0.0 : clamp(dot(-w0, edge) / denom, 0.0, 1.0);
    p = w0 + t * edge;
}

/// <summary>
/// Проецирует начало координат (origin) на треугольник в 3D пространстве.
/// Вычисляет барицентрические координаты и ближайшую точку на поверхности или границе треугольника.
/// </summary>
/// <param name="w0">Первая вершина треугольника</param>
/// <param name="w1">Вторая вершина треугольника</param>
/// <param name="w2">Третья вершина треугольника</param>
/// <param name="lambdas">Выходной параметр. Барицентрические координаты (u, v, w) проекции. Сумма всегда равна 1.0</param>
/// <param name="p">Выходной параметр. Ближайшая точка на треугольнике или его границе</param>
void ProjectOntoTriangle(float3 w0, float3 w1, float3 w2, out float3 lambdas, out float3 p)
{
    float3 e0 = w1 - w0;
    float3 e1 = w2 - w0;
    float d00 = dot(e0, e0);
    float d01 = dot(e0, e1);
    float d11 = dot(e1, e1);
    float d20 = dot(-w0, e0);
    float d21 = dot(-w0, e1);
    
    float denom = d00 * d11 - d01 * d01;
    if (abs(denom) < 0.00001) {
        lambdas = float3(1, 0, 0); p = w0; return;
    }
    
    float v = (d11 * d20 - d01 * d21) / denom;
    float w = (d00 * d21 - d01 * d20) / denom;
    float u = 1.0 - v - w;
    
    // Если проекция внутри треугольника
    if (v >= 0.0 && w >= 0.0 && u >= 0.0) {
        lambdas = float3(u, v, w);
        p = w0 * u + w1 * v + w2 * w;
    } else {
        // Если снаружи, проецируем на 3 ребра и ищем минимум
        float t01, t02, t12; float3 p01, p02, p12;
        ProjectOntoSegment(w0, w1, t01, p01);
        ProjectOntoSegment(w0, w2, t02, p02);
        ProjectOntoSegment(w1, w2, t12, p12);
        
        float l01 = dot(p01, p01);
        float l02 = dot(p02, p02);
        float l12 = dot(p12, p12);
        
        if (l01 <= l02 && l01 <= l12) { lambdas = float3(1.0 - t01, t01, 0.0); p = p01; }
        else if (l02 <= l01 && l02 <= l12) { lambdas = float3(1.0 - t02, 0.0, t02); p = p02; }
        else { lambdas = float3(0.0, 1.0 - t12, t12); p = p12; }
    }
}

/// <summary>
/// Метод проверки по GJK алгоритму для нахождения ближайших точек между двумя мешами.
/// </summary>
/// <param name="meshId0">ID первого меша</param>
/// <param name="vertexCount0">Количество вершин в первом меше</param>
/// <param name="meshId1">ID второго меша</param>
/// <param name="vertexCount1">Количество вершин во втором меше</param>
/// <returns>
/// GJKResult3D 
/// </returns>
GJKResult3D GetClosestPoints3D(uint meshId0, uint vertexCount0, uint meshId1, uint vertexCount1)
{
    GJKResult3D result;
    result.distance = 0.0;
    result.pointA = float3(0,0,0);
    result.pointB = float3(0,0,0);
    result.intersects = false;

    // В 3D симплекс содержит до 4-х вершин (тетраэдр)
    float3 sA[4]; float3 sB[4]; float3 sW[4];
    int size = 0;

    // Начальный вектор поиска (по оси Y)
    float3 d = float3(0.0, 1.0, 0.0); 

    // Фиксированный шейдерный цикл (обычно сходится за 6-9 итераций)
    for (int iter = 0; iter < 16; ++iter)
    {
        // 1. Получение новой крайней точки разности Минковского
        float3 pA = SupportShape3DAB(d, meshId0, vertexCount0);
        float3 pB = SupportShape3DAB(-d, meshId1, vertexCount1);
        float3 pW = pA - pB;

        // Проверка сходимости: если мы больше не продвигаемся в сторону начала координат
        if (size > 0)
        {
            if (dot(pW, d) - dot(sW[size - 1], d) < 0.0001) 
            {
                break; 
            }
        }

        // Добавляем новую вершину в конец массивов симплекса
        sA[size] = pA; sB[size] = pB; sW[size] = pW;
        size++;

        // Переменные для обновления геометрии
        float3 closestW = float3(0,0,0);
        float4 lambdas = float4(0,0,0,0); // Барицентрические веса для 4-х индексов

        // 2. Обработка топологии симплекса
        if (size == 1)
        {
            closestW = sW[0];
            lambdas[0] = 1.0;
        }
        else if (size == 2)
        {
            float t;
            ProjectOntoSegment(sW[0], sW[1], t, closestW);
            lambdas[0] = 1.0 - t; lambdas[1] = t;
            
            if (t >= 1.0) { sA[0] = sA[1]; sB[0] = sB[1]; sW[0] = sW[1]; size = 1; }
            else if (t <= 0.0) { size = 1; }
        }
        else if (size == 3)
        {
            // Треугольник: sW[0], sW[1], sW[2]
            float3 l3;
            ProjectOntoTriangle(sW[0], sW[1], sW[2], l3, closestW);
            lambdas.xyz = l3;
            
            // Динамическая редукция вершин треугольника на основе весов
            if (l3.x <= 0.0) { sA[0] = sA[1]; sB[0] = sB[1]; sW[0] = sW[1]; sA[1] = sA[2]; sB[1] = sB[2]; sW[1] = sW[2]; size = 2; }
            else if (l3.y <= 0.0) { sA[1] = sA[2]; sB[1] = sB[2]; sW[1] = sW[2]; size = 2; }
            else if (l3.z <= 0.0) { size = 2; }
        }
        else if (size == 4)
        {
            // Тетраэдр: sW[0], sW[1], sW[2], sW[3]
            // Вычисляем определитель матрицы объёма (знаковый объем)
            float3 e0 = sW[1] - sW[0];
            float3 e1 = sW[2] - sW[0];
            float3 e2 = sW[3] - sW[0];
            
            float3 n0 = cross(sW[2] - sW[1], sW[3] - sW[1]);
            float3 n1 = cross(sW[3] - sW[0], sW[2] - sW[0]);
            float3 n2 = cross(sW[1] - sW[0], sW[3] - sW[0]);
            float3 n3 = cross(sW[2] - sW[0], sW[1] - sW[0]);
            
            float d0 = dot(-sW[1], n0);
            float d1 = dot(-sW[0], n1);
            float d2 = dot(-sW[0], n2);
            float d3 = dot(-sW[0], n3);
            
            float det = dot(e0, cross(e1, e2));
            float signDet = det >= 0.0 ? 1.0 : -1.0;
            
            // Если начало координат находится "внутри" всех граней тетраэдра
            if ((d0 * signDet >= 0.0) && (d1 * signDet >= 0.0) && (d2 * signDet >= 0.0) && (d3 * signDet >= 0.0))
            {
                float deepestDist = 1e9;
                float3 deepestPointA = float3(0,0,0);
                float3 deepestPointB = float3(0,0,0);
                float3 deepestNormal = float3(0,1,0);

                float3 faces[4][3] = { sW[1], sW[2], sW[3],
                                       sW[0], sW[3], sW[2],
                                       sW[0], sW[1], sW[3],
                                       sW[0], sW[2], sW[1] 
                };

                for (int f = 0; f < 4; f++)
                {
                    float3 f0 = faces[f][0];
                    float3 f1 = faces[f][1];
                    float3 f2 = faces[f][2];

                    float3 edge0 = f1 - f0;
                    float3 edge1 = f2 - f0;
                    float d00 = dot(edge0, edge0);
                    float d01 = dot(edge0, edge1);
                    float d11 = dot(edge1, edge1);
                    float denom = d00 * d11 - d01 * d01;

                    if (abs(denom) < 0.00001) continue;

                    float v = (d11 * dot(-f0, edge0) - d01 * dot(-f0, edge1)) / denom;
                    float w = (d00 * dot(-f0, edge1) - d01 * dot(-f0, edge0)) / denom;
                    float u = 1.0 - v - w;
    
                    if (u >= 0.0 && v >= 0.0 && w >= 0.0)
                    {
                        float3 pa = sA[0] * u + sA[1] * v + sA[2] * w;
                        float3 pb = sB[0] * u + sB[1] * v + sB[2] * w;
                        float dist = dot(pa - pb, pa - pb);
        
                        if (dist < deepestDist)
                        {
                            deepestDist = dist;
                            deepestPointA = pa;
                            deepestPointB = pb;
                            deepestNormal = normalize(pa - pb);
                        }
                    }
                }
                
                result.intersects = true;
                result.pointA = deepestPointA;
                result.pointB = deepestPointB;
                result.distance = sqrt(deepestDist);
                result.normal = deepestNormal;
                result.contactPoint = (deepestPointA + deepestPointB) * 0.5;
                
                return result;
            }
            
            // Если снаружи — проецируем на все 4 треугольные грани и ищем минимум
            float3 c0, c1, c2, c3; float3 l0, l1, l2, l3;
            ProjectOntoTriangle(sW[1], sW[2], sW[3], l0, c0);
            ProjectOntoTriangle(sW[0], sW[3], sW[2], l1, c1);
            ProjectOntoTriangle(sW[0], sW[1], sW[3], l2, c2);
            ProjectOntoTriangle(sW[0], sW[2], sW[1], l3, c3);
            
            float dst0 = dot(c0, c0); float dst1 = dot(c1, c1);
            float dst2 = dot(c2, c2); float dst3 = dot(c3, c3);
            
            if (dst0 <= dst1 && dst0 <= dst2 && dst0 <= dst3) {
                closestW = c0; lambdas = float4(0.0, l0.x, l0.y, l0.z);
                sA[0] = sA[1]; sB[0] = sB[1]; sW[0] = sW[1]; sA[1] = sA[2]; sB[1] = sB[2]; sW[1] = sW[2]; sA[2] = sA[3]; sB[2] = sB[3]; sW[2] = sW[3];
            } else if (dst1 <= dst0 && dst1 <= dst2 && dst1 <= dst3) {
                closestW = c1; lambdas = float4(l1.x, 0.0, l1.z, l1.y);
                sA[1] = sA[3]; sB[1] = sB[3]; sW[1] = sW[3];
            } else if (dst2 <= dst0 && dst2 <= dst1 && dst2 <= dst3) {
                closestW = c2; lambdas = float4(l2.x, l2.y, 0.0, l2.z);
                sA[2] = sA[3]; sB[2] = sB[3]; sW[2] = sW[3];
            } else {
                closestW = c3; lambdas = float4(l3.x, l3.z, l3.y, 0.0);
            }
            size = 3; // Сбрасываем размер обратно до треугольника
        }

        // Обновляем вектор направления к началу координат для следующего шага
        d = -closestW;

        // Восстановление мировых координат точек на текущей итерации
        result.pointA = float3(0,0,0);
        result.pointB = float3(0,0,0);
        for (int i = 0; i < size; ++i)
        {
            result.pointA += sA[i] * lambdas[i];
            result.pointB += sB[i] * lambdas[i];
        }
        result.distance = length(closestW);

        // Если вектор направления стал нулевым — мы достигли абсолютного минимума
        if (dot(d, d) < 0.00001) { break; }
    }
    return result;
}