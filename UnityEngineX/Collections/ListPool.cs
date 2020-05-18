using System.Collections;
using System.Collections.Generic;

namespace UnityEngineX
{
    public static class ListPool<T>
    {
        private static Queue<List<T>> s_queue = new Queue<List<T>>();

        public static List<T> Take()
        {
            return s_queue.Count == 0 ? new List<T>() : s_queue.Dequeue();
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            s_queue.Enqueue(list);
        }
    }
}
