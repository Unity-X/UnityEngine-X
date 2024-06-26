﻿using System.Collections.Generic;
using System;
using UnityEditor;
using System.Collections;
using System.Reflection;
using UnityEngine;

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

    public static object GetObjectInstance(this SerializedProperty property)
    {
        if (property.serializedObject.targetObject == null)
            return null;

        string parentPath = GetPropertyParentPath(property.propertyPath);

        if (parentPath == "")
        {
            // we're already at the root-level
            return property.serializedObject.targetObject;
        }
        else
        {
            // we need to dig in deeper in the serialized data
            return GetObjectInstanceFromPath(property.serializedObject.targetObject, parentPath);
        }
    }

    public static object[] GetObjectInstances(this SerializedProperty property)
    {
        string parentPath = GetPropertyParentPath(property.propertyPath);

        if (parentPath == "")
        {
            // we're already at the root-level
            return property.serializedObject.targetObjects;
        }
        else
        {
            object[] targetObjects = property.serializedObject.targetObjects;
            object[] instances = new object[targetObjects.Length];

            for (int i = 0; i < instances.Length; i++)
            {
                // we need to dig in deeper in the serialized data
                instances[i] = GetObjectInstanceFromPath(targetObjects[i], parentPath);
            }
            return instances;
        }
    }

    public static SerializedProperty GetParentProperty(this SerializedProperty property)
    {
        string parentPath = GetPropertyParentPath(property.propertyPath);
        return property.serializedObject.FindProperty(parentPath);
    }

    private static string GetPropertyParentPath(string propertyPath)
    {
        // the serialized property path will look like this:
        // theRootObject.aSubProperty.Array.data[16].ourProperty
        string path = propertyPath;
        if (path.Contains("."))
        {
            path = path.Remove(path.LastIndexOf('.'));

            if (path.EndsWith(".Array")) // if the path end with .Array, we'll want to strip that away and restart
            {
                path = path.Remove(path.LastIndexOf('.'));
            }
        }
        else
        {
            path = "";
        }

        return path;
    }

    private static object GetObjectInstanceFromPath(object parentObject, string objectPath)
    {
        object currentObject = parentObject;

        string[] pathSerializedNames = objectPath.Split('.');
        try
        {
            for (int i = 0; i < pathSerializedNames.Length; i++)
            {
                if (currentObject == null)
                    break;

                if (pathSerializedNames[i] == "Array")
                {
                    // the serialized name will be like this 'Array.data[15].TheThingAfter'

                    ++i; // skip 'Array'

                    // we want to extract the '15' out of 'data[15]'
                    string dataIndex = pathSerializedNames[i].Substring("data".Length + 1, pathSerializedNames[i].Length - "data".Length - "[]".Length);
                    int index = int.Parse(dataIndex);
                    if (currentObject is IList list)
                    {
                        if (list.Count <= index)
                            return null;
                        currentObject = list[index];
                    }
                }
                else
                {
                    FieldInfo fieldInfo = currentObject.GetType().GetField(pathSerializedNames[i], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    currentObject = fieldInfo.GetValue(currentObject);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }

        return currentObject;
    }
}