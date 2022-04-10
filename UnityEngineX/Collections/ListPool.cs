using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngineX
{
    public static class ListPool<T>
    {
        public struct UsageHandle : IDisposable
        {
            private List<T> _list;

            public UsageHandle(List<T> list)
            {
                _list = list;
            }

            public void Dispose()
            {
                Release(_list);
            }
        }

        private static Queue<List<T>> s_queue = new Queue<List<T>>();

        public static List<T> Take() => Take(4);

        public static List<T> Take(int capacity)
        {
            return s_queue.Count == 0 ? new List<T>(capacity) : s_queue.Dequeue();
        }

        public static UsageHandle Take(int capacity, out List<T> list)
        {
            list = Take(capacity);
            return new UsageHandle(list);
        }
        public static UsageHandle Take(out List<T> list) => Take(4, out list);

        public static void Release(List<T> list)
        {
            list.Clear();
            s_queue.Enqueue(list);
        }
    }
}
