using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngineX
{
/// <summary>
/// This is like the ReadOnlyCollection provided by .NET but in struct (instead of class) meaning it does't produce garbage
/// </summary>
public struct ReadOnlyList<T>
{
    readonly List<T> _list;

    public ReadOnlyList(List<T> list)
    {
        _list = list ?? throw new System.NullReferenceException();
    }

    public T this[int index] => _list[index];
    public int Count => _list.Count;
    public bool Contains(T value) => _list.Contains(value);
    public void CopyTo(T[] array, int index) => _list.CopyTo(array, index);
    public int IndexOf(T value) => _list.IndexOf(value);
    public T Find(Predicate<T> match) => _list.Find(match);
    public T[] ToArray() => _list.ToArray();

    public ReadOnlyListDynamic<U> DynamicCast<U>()
    {
        if (!typeof(U).IsAssignableFrom(typeof(T)))
        {
            throw new Exception($"Type {typeof(U).Name} is not assignable from {typeof(T).Name}");
        }

        return new ReadOnlyListDynamic<U>(_list);
    }

    #region Enumerator
    public Enumerator GetEnumerator() => new Enumerator(_list);

    public struct Enumerator
    {
        readonly IList<T> _entities;
        int _i;
        int _count;

        public Enumerator(IList<T> entities)
        {
            _entities = entities;
            _count = _entities.Count;
            _i = -1;
        }

        public T Current => _entities[_i];

        public bool MoveNext()
        {
            ++_i;
            return _i < _count;
        }
    }
    #endregion
}


/// <summary>
/// This is like the ReadOnlyCollection provided by .NET but in struct (instead of class) meaning it doesn't produce garbage
/// </summary>
public struct ReadOnlyListDynamic<T>
{
    readonly IList _list;

    public ReadOnlyListDynamic(IList list)
    {
        _list = list ?? throw new System.NullReferenceException();
    }

    public T this[int index] => (T)_list[index];
    public int Count => _list.Count;
    public bool Contains(T value) => _list.Contains(value);
    public void CopyTo(T[] array, int index) => _list.CopyTo(array, index);
    public int IndexOf(T value) => _list.IndexOf(value);
    public T[] ToArray()
    {
        T[] result = new T[_list.Count];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = (T)_list[i];
        }
        return result;
    }

    #region Enumerator
    public Enumerator<T> GetEnumerator() => new Enumerator<T>(_list);

    public struct Enumerator<U>
    {
        readonly IList _elements;
        int _i;
        int _count;

        public Enumerator(IList entities)
        {
            _elements = entities;
            _count = _elements.Count;
            _i = -1;
        }

        public U Current => (U)_elements[_i];

        public bool MoveNext()
        {
            ++_i;
            return _i < _count;
        }
    }
    #endregion
}

public static class ReadOnlyListExtensions
{
    public static ReadOnlyList<ListType> AsReadOnlyNoAlloc<ListType>(this List<ListType> list)
    {
        return new ReadOnlyList<ListType>(list);
    }
}
}