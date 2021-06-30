using System;
using System.Collections.Concurrent;
using UnityEngineX;

namespace UnityEngineX
{
    public class ConcurrentPool<T>
    {
        public struct ScopedGet : IDisposable
        {
            private readonly T _obj;
            private readonly ConcurrentPool<T> _pool;

            public ScopedGet(T obj, ConcurrentPool<T> pool)
            {
                _obj = obj;
                _pool = pool;
            }

            public void Dispose()
            {
                _pool.Return(_obj);
            }
        }

        private readonly ConcurrentBag<T> _objects;
        private readonly Func<T> _objectGenerator;

        public ConcurrentPool(Func<T> objectGenerator)
        {
            _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            _objects = new ConcurrentBag<T>();
        }

        public T Get() => _objects.TryTake(out T item) ? item : _objectGenerator();
        public ScopedGet GetScoped(out T obj)
        {
            obj = Get();
            return new ScopedGet(obj, this);
        }

        public void Return(T item) => _objects.Add(item);
    }
}