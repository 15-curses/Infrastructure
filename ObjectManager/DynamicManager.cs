using System.Collections.Generic;
using UnityEngine;

namespace Assets.Infrastructure.ObjectManager
{
    public class DynamicManager : MonoBehaviour
    {
        #region Создание объекта
        private List<GameObject> allObjects;
        private List<GameObject> kinematicObjects;
        /// <summary>
        /// Создает объект в сцене
        /// </summary>
        public List<GameObject> SpawnObject(ObjectGroupAsset asset)
        {
            allObjects = new List<GameObject>();
            kinematicObjects = new List<GameObject>();

            foreach (ObjectAsset _objAsset in asset.GetAllObjects())
            {
                Rigidbody rb = _objAsset.prefab.GetComponent<Rigidbody>();
                GameObject gameObject;

                if (rb != null && rb.isKinematic == false)
                {
                    rb.isKinematic = true;
                    gameObject = Instantiate(_objAsset.prefab);
                    rb.isKinematic = false;
                    kinematicObjects.Add(gameObject);
                }
                else gameObject = Instantiate(_objAsset.prefab);

                gameObject.name = _objAsset.name;

                allObjects.Add(gameObject);
            }
            foreach (GameObject obj in kinematicObjects)
                obj.GetComponent<Rigidbody>().isKinematic = false;

            return allObjects;
        }

        public void Deinitialize(List<GameObject> objects)
        {
            foreach (GameObject obj in objects)
                DeleteObjectLogic(obj);
        }
        #endregion
        #region Удаление объекта
        public void DeleteObjectLogic(GameObject gameObject) => Destroy(gameObject);

        #endregion
        #region Телепорт объекта
        public GameObject TeleportLogic(GameObject gameObject, Vector3 newPosition)
        {
            gameObject.transform.position = newPosition;
            return gameObject;
        }
        #endregion
        #region Получение Rigidbody
        private Rigidbody RigidbodyOfThis(GameObject gameObject) =>
            gameObject.GetComponent<Rigidbody>();
        #endregion

        #region Методы для управления физикой
        #region [ Не Кинематик ]
        public void PushInDirectionLogic(GameObject gameObject, Vector3 endV3, Vector3? startV3 = null)
        {
            if (startV3 != null)
            {
                gameObject = TeleportLogic(gameObject, (Vector3)startV3);
            }

            var rb = RigidbodyOfThis(gameObject);

            if (rb == null)
            {
                // ошибку
                return;
            }
            rb.AddForce(endV3, ForceMode.Impulse);
        }


        #endregion
        #region [ Кинематик ]
        public void MoveToPositionLogic(GameObject gameObject, Vector3 endV3, float speed = 0)
        {
            if (gameObject == null | endV3 == null | speed == 0) return;

            gameObject.transform.position = Vector3.MoveTowards(
                gameObject.transform.position,
                endV3,
                speed * Time.deltaTime
            );
        }
        #endregion
        #endregion
    }
}