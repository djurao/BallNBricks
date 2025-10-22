using System.Collections.Generic;
using UnityEngine;

namespace Misc
{
    public class SimpleObjectPooling : MonoBehaviour
    {
        public GameObject prefab;
        public List<GameObject> pool;
        public Transform initialParent;
        public GameObject GetObject()
        {
            foreach (var @object in pool)
            {
                if (!@object.activeInHierarchy)
                {
                    return @object;
                }
            }

            return CreateNewInstance();
        }
        private GameObject CreateNewInstance()
        {
            var newObj = Instantiate(prefab,initialParent);
            AddToPool(newObj);
            return newObj;
        }
        private void AddToPool(GameObject @object) => pool.Add(@object);

        public void DePool(GameObject @object)
        {
            if (pool.Contains(@object))
            {
                pool.Remove(@object);
            }
            Destroy(@object);
        }
    }
}
