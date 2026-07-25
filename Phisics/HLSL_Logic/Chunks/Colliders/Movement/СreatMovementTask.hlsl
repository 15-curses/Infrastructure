/// <summary>
/// Вычисляет инверсный тензор инерции для объекта на основе геометрии описанного AABB.
/// </summary>
/// <param name="obj">Физическое тело для расчета.</param>
/// <returns>Диагональная матрица 3х3, представляющая инвертированный момент инерции.</returns>
float3x3 GetInverseInertiaTensor(Object obj)
{
    if (obj.mass <= 0.0f) return float3x3(0,0,0, 0,0,0, 0,0,0);
    float3 size = obj.maxAABB - obj.minAABB;
    float radius = length(size) * 0.5f;
    float inertia = 0.4f * obj.mass * (radius * radius);
    float invInertia = 1.0f / max(inertia, 0.0001f);
    
    return float3x3(
        invInertia, 0.0f, 0.0f,
        0.0f, invInertia, 0.0f,
        0.0f, 0.0f, invInertia
    );
}
/// <summary>
/// Разрешает физическое столкновение двух объектов с использованием импульсного метода.
/// </summary>
/// <remarks>
/// Шаг 1: Позиционное расталкивание (метод Баумгарте) во избежание "слипания".
/// Шаг 2: Расчет нормального импульса отскока.
/// Шаг 3: Ограничение касательного трения по закону Кулона.
/// </remarks>
/// <param name="objA">Первое сталкивающееся тело.</param>
/// <param name="objB">Второе сталкивающееся тело.</param>
/// <param name="gjk">Структура данных с геометрическими параметрами контакта.</param>
void ComputePhysicsResponse(inout Object objA, inout Object objB, GJKResult3D gjk)
{
    // Выходим, если коллизии нет
    if (!gjk.intersects) return;
    
    // Расчет инверсных масс
    float invMassA = objA.mass > 0.0f ? (1.0f / objA.mass) : 0.0f;
    float invMassB = objB.mass > 0.0f ? (1.0f / objB.mass) : 0.0f;
    float totalInvMass = invMassA + invMassB;

    if (totalInvMass == 0.0f) return;

    // Сохраняем исходные позиции до расталкивания для честного расчета рычагов
    float3 currentPosA = objA.position;
    float3 currentPosB = objB.position;

    // 1. РАЗРЕШЕНИЕ ПРОНИКНОВЕНИЯ (Расталкивание объектов)
    float penetration = length(gjk.pointA - gjk.pointB);
    const float slop = 0.01f; // Допустимое проникновение (в метрах)
    
    if (penetration > slop)
    {
        const float percent = 0.4f; // Процент разрешения проникновения за кадр
        float3 correction = gjk.normal * (max(penetration - slop, 0.0f) / totalInvMass * percent);
        
        // Изменяем позиции объектов напрямую
        objA.position += correction * invMassA;
        objB.position -= correction * invMassB;
    }

    // 2. ПОДГОТОВКА К РАСЧЕТУ ИМПУЛЬСОВ
    float3x3 invInertiaA = GetInverseInertiaTensor(objA);
    float3x3 invInertiaB = GetInverseInertiaTensor(objB);

    // Рассчитываем центры масс на основе исходных позиций
    float3 worldMassCenterA = currentPosA + mul((float3x3)objA.transform, objA.massCenter);
    float3 worldMassCenterB = currentPosB + mul((float3x3)objB.transform, objB.massCenter);

    float3 rA = gjk.contactPoint - worldMassCenterA;
    float3 rB = gjk.contactPoint - worldMassCenterB;

    // Расчет скоростей в точке контакта
    float3 vContactA = objA.velocity + cross(objA.angularVelocity, rA);
    float3 vContactB = objB.velocity + cross(objB.angularVelocity, rB);
    float3 relativeVelocity = vContactA - vContactB;

    // Проекция на нормаль (отскок)
    float velAlongNormal = dot(relativeVelocity, gjk.normal);
    if (velAlongNormal > 0.0f) return; // Объекты уже отдаляются

    float restitution = 0.2f; // Коэффициент упругости
    
    if (abs(velAlongNormal) < 0.15f) // если менее 0,15 метров в сек, то движение не происходит
    {
        restitution = 0.0f;
    }
    
    // Угловые факторы для нормали
    float3 angularComponentA = cross(mul(invInertiaA, cross(rA, gjk.normal)), rA);
    float3 angularComponentB = cross(mul(invInertiaB, cross(rB, gjk.normal)), rB);
    float angularFactor = dot(angularComponentA + angularComponentB, gjk.normal);

    // 3. РАСЧЕТ НОРМАЛЬНОГО ИМПУЛЬСА (ОТТЕЛКИВАНИЕ)
    float impulseMagnitude = -(1.0f + restitution) * velAlongNormal;
    impulseMagnitude /= (totalInvMass + angularFactor);
    float3 impulse = gjk.normal * impulseMagnitude;

    // 4. РАСЧЕТ ТАНГЕНЦИАЛЬНОГО ИМПУЛЬСА (ТРЕНИЕ)
    float3 tangent = relativeVelocity - (gjk.normal * velAlongNormal);
    float tangentLen = length(tangent);

    float3 frictionImpulse = float3(0, 0, 0);

    if (tangentLen > 0.0001f)
    {
        tangent = normalize(tangent);

        // Эквивалент массы для углового момента вдоль касательной трения
        float3 tangentComponentA = cross(mul(invInertiaA, cross(rA, tangent)), rA);
        float3 tangentComponentB = cross(mul(invInertiaB, cross(rB, tangent)), rB);
        float frictionAngularFactor = dot(tangentComponentA + tangentComponentB, tangent);

        // Импульс, необходимый для ПОЛНОЙ остановки тангенциального скольжения
        float frictionMagnitude = -dot(relativeVelocity, tangent);
        frictionMagnitude /= (totalInvMass + frictionAngularFactor);

        // Вычисляем комбинированные коэффициенты трения для пары объектов
        float muStatic  = sqrt(max(objA.staticFriction, 0.0f)  * max(objB.staticFriction, 0.0f));
        float muDynamic = sqrt(max(objA.dynamicFriction, 0.0f) * max(objB.dynamicFriction, 0.0f));

        // Проверяем закон Кулона для трения покоя
        if (abs(frictionMagnitude) < impulseMagnitude * muStatic)
        {
            // Объект "прилипает" (силы трения покоя достаточно, чтобы остановить сдвиг)
            frictionImpulse = tangent * frictionMagnitude;
        }
        else
        {
            // Объект срывается в скольжение, используем динамическое трение
            float maxDynamicFriction = impulseMagnitude * muDynamic;
            frictionMagnitude = clamp(frictionMagnitude, -maxDynamicFriction, maxDynamicFriction);
            frictionImpulse = tangent * frictionMagnitude;
        }
    }

    // 5. ПРИМЕНЕНИЕ ВСЕХ ИМПУЛЬСОВ К СКОРОСТЯМ
    float3 totalImpulse = impulse + frictionImpulse;

    // Изменяем скорости объектов напрямую
    objA.velocity        += totalImpulse * invMassA;
    objA.angularVelocity += mul(invInertiaA, cross(rA, totalImpulse));

    objB.velocity        -= totalImpulse * invMassB;
    objB.angularVelocity -= mul(invInertiaB, cross(rB, totalImpulse));
}

