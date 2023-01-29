using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
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
            using var _ = ListPool<GameObject>.Get(out List<GameObject> rootObjs);
            scene.GetRootGameObjects(rootObjs);

            T result = default;

            for (int i = 0; i < rootObjs.Count; i++)
            {
                if (rootObjs[i].TryGetComponent(out result))
                {
                    break;
                }
            }

            return result;
        }
    }
}
