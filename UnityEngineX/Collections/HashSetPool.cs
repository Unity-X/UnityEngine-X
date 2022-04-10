using System;
using System.Collections.Generic;

namespace UnityEngineX
{
    public static class HashSetPool<T>
    {
        public struct UsageHandle : IDisposable
        {
            private HashSet<T> _set;

            public UsageHandle(HashSet<T> set)
            {
                _set = set;
            }

            public void Dispose()
            {
                Release(_set);
            }
        }

        private static Queue<HashSet<T>> s_queue = new Queue<HashSet<T>>();

        public static HashSet<T> Take()
        {
            return s_queue.Count == 0 ? new HashSet<T>() : s_queue.Dequeue();
        }

        public static UsageHandle Take(out HashSet<T> set)
        {
            set = Take();
            return new UsageHandle(set);
        }

        public static void Release(HashSet<T> set)
        {
            set.Clear();
            s_queue.Enqueue(set);
        }
    }
}
