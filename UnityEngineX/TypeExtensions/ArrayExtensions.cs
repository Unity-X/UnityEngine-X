using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngineX
{
    public static class ArrayExtensions
    {
        public static bool IsValidIndex(this Array array, int index)
        {
            return index >= 0 && index < array.Length;
        }

        public static bool IsValidIndex(this Array array, uint index)
        {
            return index < array.Length;
        }

        public static T TryGetAt<T>(this T[] array, uint index)
        {
            array.TryGetAt(index, out T result);
            return result;
        }
        public static T TryGetAt<T>(this T[] array, int index)
        {
            array.TryGetAt(index, out T result);
            return result;
        }
        public static bool TryGetAt<T>(this T[] array, uint index, out T result)
        {
            if (array.IsValidIndex(index))
            {
                result = array[index];
                return true;
            }
            result = default;
            return false;
        }
        public static bool TryGetAt<T>(this T[] array, int index, out T result)
        {
            if (array.IsValidIndex(index))
            {
                result = array[index];
                return true;
            }
            result = default;
            return false;
        }

        public static bool Contains(this Array array, object obj)
        {
            for (int i = 0; i < array.Length; i++)
                if (array.GetValue(i).Equals(obj))
                    return true;
            return false;
        }

        public static bool ContainsNull(this Array array)
        {
            for (int i = 0; i < array.Length; i++)
                if (array.GetValue(i) == null)
                    return true;
            return false;
        }

        public static bool Contains<T>(this T[] array, Predicate<T> predicate)
        {
            for (int i = 0; i < array.Length; i++)
                if (predicate(array[i]))
                    return true;
            return false;
        }

        public static bool Contains<T>(this T[] array, T element)
        {
            for (int i = 0; i < array.Length; i++)
                if (EqualityComparer<T>.Default.Equals(array[i], element))
                    return true;
            return false;
        }

        public static bool Contains<T>(this Array array)
        {
            for (int i = 0; i < array.Length; i++)
                if (array.GetValue(i) is T)
                    return true;
            return false;
        }

        public static T Find<T>(this T[] array, Predicate<T> predicate)
        {
            for (int i = 0; i < array.Length; i++)
                if (predicate(array[i]))
                    return array[i];
            return default;
        }
        public static T Find<T>(this Array array)
        {
            for (int i = 0; i < array.Length; i++)
                if (array.GetValue(i) is T)
                    return (T)array.GetValue(i);
            return default;
        }

        public static T Last<T>(this T[] list)
        {
            return list[list.Length - 1];
        }
        public static int LastIndex(this Array list)
        {
            return list.Length - 1;
        }
        public static int CountOf<T>(this T[] list, T element)
        {
            int amount = 0;
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].Equals(element))
                    amount++;
            }
            return amount;
        }

        public static T PickRandom<T>(this T[] list)
        {
            if (list.Length == 0)
                return default(T);
            if (list.Length == 1)
                return list[0];
            return list[UnityEngine.Random.Range(0, list.Length)];
        }

        public static void Shuffle<T>(this T[] list)
        {
            T temp;
            for (int i = list.Length - 1; i >= 1; i--)
            {
                int chosen = UnityEngine.Random.Range(0, i + 1);
                if (chosen == i)
                    continue;

                temp = list[chosen];
                list[chosen] = list[i];
                list[i] = temp;
            }
        }

        public static float Sum(this float[] list)
        {
            float sum = 0;
            for (int i = 0; i < list.Length; i++)
            {
                sum += list[i];
            }
            return sum;
        }

        public static T[] SubArray<T>(this T[] array, int startIndex, int length)
        {
            int num;
            if (array == null || (num = array.Length) == 0)
            {
                return new T[0];
            }

            if (startIndex < 0 || length <= 0 || startIndex + length > num)
            {
                return new T[0];
            }

            if (startIndex == 0 && length == num)
            {
                return array;
            }

            T[] array2 = new T[length];
            Array.Copy(array, startIndex, array2, 0, length);
            return array2;
        }
    }
}
