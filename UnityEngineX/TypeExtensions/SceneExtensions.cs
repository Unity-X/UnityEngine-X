using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEngineX
{
    public static class SceneExtensions
    {
        /// <summary>
        /// Returns the first component of type T found on root gameobjects.
        /// </summary>
        static public T FindComponentOnRoots<T>(this in Scene scene)
        {
            List<GameObject> rootObjs = ListPool<GameObject>.Take();
            scene.GetRootGameObjects(rootObjs);

            T result = default;

            for (int i = 0; i < rootObjs.Count; i++)
            {
                if (rootObjs[i].TryGetComponent(out result))
                {
                    break;
                }
            }

            ListPool<GameObject>.Release(rootObjs);

            return result;
        }
    }
}
