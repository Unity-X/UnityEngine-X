using System;
using System.Collections;
using System.Collections.Generic;


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
public struct ReadOnlyList<ListType, ReadType>
    where ListType : ReadType
{
    readonly List<ListType> _list;

    public ReadOnlyList(List<ListType> list)
    {
        _list = list ?? throw new System.NullReferenceException();
    }

    public ReadType this[int index] => _list[index];
    public int Count => _list.Count;
    public bool Contains(ListType value) => _list.Contains(value);
    public bool Contains(ReadType value)
    {
        if (value is ListType)
            return _list.Contains((ListType)value);
        return false;
    }

    public void CopyTo(ListType[] array, int index) => _list.CopyTo(array, index);
    public int IndexOf(ListType value) => _list.IndexOf(value);
    public int IndexOf(ReadType value)
    {
        if (value is ListType)
            return _list.IndexOf((ListType)value);
        return -1;
    }
    public ReadType Find(Predicate<ReadType> match)
    {
        for (int i = 0; i < _list.Count; i++)
        {
            if (match(_list[i]))
                return _list[i];
        }
        return default;
    }

    #region Enumerator
    public Enumerator GetEnumerator() => new Enumerator(_list);

    public struct Enumerator
    {
        readonly IList<ListType> _entities;
        int _i;
        int _count;

        public Enumerator(IList<ListType> entities)
        {
            _entities = entities;
            _count = _entities.Count;
            _i = -1;
        }

        public ReadType Current => _entities[_i];

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
    public static ReadOnlyList<ListType, ReadType> AsReadOnlyNoAlloc<ListType, ReadType>(this List<ListType> list)
        where ListType : ReadType
    {
        return new ReadOnlyList<ListType, ReadType>(list);
    }
}