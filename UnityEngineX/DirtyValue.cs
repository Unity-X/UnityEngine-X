using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Value wrapper that easily identifies if a change in value has occured.
/// </summary>
public struct DirtyValue<T>
{
    // fbessette:
    // This would be more intuitive as '_forcedDirty', but inverting it like that ensures the default value of the field
    // matches with the design intention (being initially dirty by default)
    private bool _notForcedDirty;

    // 'true' when the user calls ClearDirty(). Back to 'false' as soon as Set(..) is called
    private bool _cleared;

    private T _previousValue;
    private T _value;

    public DirtyValue(T initialValue, bool initiallyDirty = true)
    {
        _previousValue = initialValue;
        _value = initialValue;
        _notForcedDirty = !initiallyDirty;
        _cleared = false;
    }

    /// <summary>
    /// Is the current value different from the previous one ?
    /// </summary>
    public bool IsDirty
    {
        get
        {
            if (_cleared)      // if acknowledged
                return false;

            if (!_notForcedDirty)   // if forced dirty
                return true;

            return !EqualityComparer<T>.Default.Equals(_previousValue, _value);
        }
    }

    /// <summary>
    /// Get the previous value (the value when the last ClearDirty() was called)
    /// </summary>
    public T GetPrevious() => _previousValue;

    /// <summary>
    /// Get the value
    /// </summary>
    public T Get() => _value;

    /// <summary>
    /// Set the value
    /// </summary>
    public void Set(in T value)
    {
        if (_cleared)
        {
            _notForcedDirty = true;
            _previousValue = _value;
            _cleared = false;
        }
        _value = value;
    }

    /// <summary>
    /// Clear any dirtiness if there is any. Returns 'true' if the value was dirty.
    /// </summary>
    public bool ClearDirty()
    {
        bool isDirty = IsDirty;

        _cleared = true;

        return isDirty;
    }

    /// <summary>
    /// Forces the struct to be dirty, even if the current value and previous value are equal
    /// </summary>
    public void ForceDirty()
    {
        _notForcedDirty = false;
    }

    public static implicit operator T(DirtyValue<T> dirtyValue) => dirtyValue.Get();
}

/// <summary>
/// Reference wrapper that easily identifies if a change in reference has occured.
/// </summary>
public struct DirtyRef<T>
{
    // fbessette:
    // This would be more intuitive as '_forcedDirty', but inverting it like that ensures the default value of the field
    // matches with the design intention (being initially dirty by default)
    private bool _notForcedDirty;

    // 'true' when the user calls ClearDirty(). Back to 'false' as soon as Set(..) is called
    private bool _cleared;

    private T _previousValue;
    private T _value;

    public DirtyRef(T initialValue, bool initiallyDirty = true)
    {
        _previousValue = initialValue;
        _value = initialValue;
        _notForcedDirty = !initiallyDirty;
        _cleared = false;
    }

    /// <summary>
    /// Is the current value different from the previous one ?
    /// </summary>
    public bool IsDirty
    {
        get
        {
            if (_cleared)      // if acknowledged
                return false;

            if (!_notForcedDirty)   // if forced dirty
                return true;

            return !ReferenceEquals(_previousValue, _value);
        }
    }

    /// <summary>
    /// Get the previous value (the value when the last ClearDirty() was called)
    /// </summary>
    public T GetPrevious() => _previousValue;

    /// <summary>
    /// Get the value
    /// </summary>
    public T Get() => _value;

    /// <summary>
    /// Set the value
    /// </summary>
    public void Set(in T value)
    {
        if (_cleared)
        {
            _notForcedDirty = true;
            _previousValue = _value;
            _cleared = false;
        }
        _value = value;
    }

    /// <summary>
    /// Clear any dirtiness if there is any. Returns 'true' if the value was dirty.
    /// </summary>
    public bool ClearDirty()
    {
        bool isDirty = IsDirty;

        _cleared = true;

        return isDirty;
    }

    /// <summary>
    /// Forces the struct to be dirty, even if the current value and previous value are equal
    /// </summary>
    public void ForceDirty()
    {
        _notForcedDirty = false;
    }

    public static implicit operator T(DirtyRef<T> dirtyValue) => dirtyValue.Get();
}