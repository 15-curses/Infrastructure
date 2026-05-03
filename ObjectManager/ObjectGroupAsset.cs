using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Infrastructure.ObjectManager
{
    [CreateAssetMenu(fileName = "New Object Group", menuName = "3D Object System/Object Group Asset")]
    public class ObjectGroupAsset : ScriptableObject
    {
        #region [Ассет]
        [Header("Информация о группе")]
        [Tooltip("Название группы")]
        public string groupName;

        [Tooltip("Описание группы")]
        [TextArea(1, 2)]
        public string description;

        [Header("Объекты в группе")]
        [Tooltip("Список ассетов объектов, входящих в группу")]
        public List<ObjectAsset> objects = new List<ObjectAsset>();

        [Header("Вложенные группы")]
        [Tooltip("Другие группы, которые включаются в эту")]
        public List<ObjectGroupAsset> includeGroups = new List<ObjectGroupAsset>();
        #endregion

        #region [Получение из ассета]
        /// <summary>
        /// Возвращает все объекты из данной группы и всех приложенных
        /// </summary>
        public List<ObjectAsset> GetAllObjects()
        {
            var allObjects = new List<ObjectAsset>(objects);

            foreach (var group in includeGroups)
            {
                if (group != null)
                {
                    allObjects.AddRange(group.GetAllObjects());
                }
            }
            return allObjects;
        }
        /// <summary>
        /// Возвращает bool,в зависимости есть ли объект в группе
        /// </summary>
        public bool Contains(ObjectAsset obj)
        {
            if (objects.Contains(obj)) return true;

            foreach (var group in includeGroups)
            {
                if (group != null && group.Contains(obj))
                    return true;
            }

            return false;
        }
        #endregion


    }
}
