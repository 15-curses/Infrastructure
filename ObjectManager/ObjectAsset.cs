using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Infrastructure.ObjectManager
{
    [CreateAssetMenu(fileName = "New 3D Object", menuName = "3D Object System/Object Asset")]
    public class ObjectAsset : ScriptableObject
    {
        #region [Ассет]
        [Header("Основная информация")]
        [Tooltip("Уникальный идентификатор")]
        public int iD;

        [Tooltip("Описание объекта")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("Префаб объекта")]
        public GameObject prefab;
        #endregion
    }
}
