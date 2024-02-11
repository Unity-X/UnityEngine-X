using System.Collections.Generic;
using System;
using UnityEditor;

public static class SerializedPropertyExtensions
{
    private sealed class FunctorComparer<T> : IComparer<T>
    {
        Comparison<T> _comparison;

        public FunctorComparer(Comparison<T> comparison)
        {
            _comparison = comparison;
        }

        public int Compare(T x, T y)
        {
            return _comparison(x, y);
        }
    }

    public static void SortArray(this SerializedProperty property, int index, int count, Comparison<SerializedProperty> comparison)
    {
        var comparer = new FunctorComparer<SerializedProperty>(comparison);
        SortArray(property, index, count, comparer);
    }

    public static void SortArray(this SerializedProperty property, Comparison<SerializedProperty> comparison)
    {
        var comparer = new FunctorComparer<SerializedProperty>(comparison);
        SortArray(property, 0, property.arraySize, comparer);
    }

    public static void SortArray(this SerializedProperty property, IComparer<SerializedProperty> comparer)
    {
        SortArray(property, 0, property.arraySize, comparer);
    }

    public static void SortArray(this SerializedProperty property, int index, int count, IComparer<SerializedProperty> comparer)
    {
        if (!property.isArray)
            throw new ArgumentException("Property is not an array", nameof(property));
        
        if (index < 0)
            throw new ArgumentException("Index is out of range.", nameof(index));

        int hi = index + count - 1;

        if (hi >= property.arraySize)
            throw new ArgumentException("Arguments index and count reach outside of the array range.");

        if (property.arraySize <= 1)
            return;

        for (int i = index; i < hi; i++)
        {
            int j = i;

            SerializedProperty itemA = property.GetArrayElementAtIndex(i + 1);

            while (j >= index && comparer.Compare(itemA, property.GetArrayElementAtIndex(j)) < 0)
            {
                j--;
            }

            if (i != j)
                property.MoveArrayElement(i + 1, j + 1);
        }
    }
}