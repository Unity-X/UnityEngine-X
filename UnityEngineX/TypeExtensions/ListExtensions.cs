using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngineX
{
    public static class ListExtensions
    {
        public static int RemoveFirst<T>(this List<T> list, Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    list.RemoveAt(i);
                    return i;
                }
            }
            return -1;
        }

        public static int Resize<T>(this List<T> list, int count, Action<T> onCreate = null, Action<T> onRemove = null) where T : new()
        {
            int diff = count - list.Count;

            if (diff > 0)
            {
                while (list.Count < count)
                {
                    var newT = new T();
                    onCreate?.Invoke(newT);
                    list.Add(newT);
                }
            }
            else
            {
                while (list.Count > count)
                {
                    int idx = list.Count - 1;
                    var oldT = list[idx];
                    onRemove?.Invoke(oldT);
                    list.RemoveAt(idx);
                }
            }
            return diff;
        }

        public static void RemoveFirst<T>(this List<T> list)
        {
            list.RemoveAt(0);
        }

        public static int RemoveNulls<T>(this List<T> list)
        {
            int c = 0;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == null)
                {
                    c++;
                    list.RemoveAt(i);
                }
            }
            return c;
        }

        public static void RemoveLast<T>(this List<T> list)
        {
            int index = list.LastIndex();
            if (index >= 0)
            {
                list.RemoveAt(index);
            }
        }

        public static void RemoveFrom<T>(this List<T> list, int index)
        {
            list.RemoveRange(index, list.Count - index);
        }

        public static void SwapWithLast<T>(this List<T> list, int index)
        {
            if (list.Count <= 1)
                return;

            int lastIndex = list.Count - 1;
            T temp = list[lastIndex];

            //Swap chosen element with element at end of list
            list[lastIndex] = list[index];
            list[index] = temp;
        }

        public static void Swap<T>(this List<T> list, int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(indexA));

            if (indexB < 0 || indexB >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(indexB));

            T temp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = temp;
        }

        public static void MoveFirst<T>(this List<T> list, int index)
        {
            if (list.Count <= 1)
                return;

            T element = list[index];

            list.RemoveAt(index);
            list.Insert(0, element);
        }

        public static void MoveLast<T>(this List<T> list, int index)
        {
            if (list.Count <= 1)
                return;

            T element = list[index];

            list.RemoveAt(index);
            list.Add(element);
        }

        public static T First<T>(this List<T> list)
        {
            return list[0];
        }

        public static T Last<T>(this List<T> list)
        {
            return list[list.Count - 1];
        }

        public static int LastIndex<T>(this List<T> list)
        {
            return list.Count - 1;
        }

        public static T Pop<T>(this List<T> list)
        {
            int lastIndex = list.Count - 1;
            T last = list[lastIndex];
            list.RemoveAt(lastIndex);
            return last;
        }

        public static int CountOf<T>(this List<T> list, T element)
        {
            int amount = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Equals(element))
                    amount++;
            }
            return amount;
        }

        public static int CountOf<T>(this List<T> list, Predicate<T> predicate)
        {
            int amount = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                    amount++;
            }
            return amount;
        }

        public static T PickRandom<T>(this List<T> list)
        {
            if (list.Count == 0)
                return default(T);
            if (list.Count == 1)
                return list[0];
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static void Shuffle<T>(this List<T> list)
        {
            T temp;
            for (int i = list.Count - 1; i >= 1; i--)
            {
                int chosen = UnityEngine.Random.Range(0, i + 1);
                if (chosen == i)
                    continue;

                temp = list[chosen];
                list[chosen] = list[i];
                list[i] = temp;
            }
        }

        public static bool Contains<T>(this IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is T)
                    return true;
            }

            return false;
        }

        public static bool FindFirstOfType<T>(this IEnumerable enumerable, out T result)
        {
            foreach (var item in enumerable)
            {
                if (item is T t)
                {
                    result = t;
                    return true;
                }
            }

            result = default;
            return false;
        }

        public static bool FindFirstOfType<T>(this List<T> list, Type type, out T result)
        {
            int index = list.IndexOf(type);
            if (index != -1)
            {
                result = list[index];
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static int IndexOf<T>(this List<T> list, Type type)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetType() == type)
                    return i;
            }
            return -1;
        }

        public static bool AddUnique<T>(this List<T> list, T value)
        {
            if (!list.Contains(value))
            {
                list.Add(value);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void AddRange<T>(this List<T> list, T value, int count)
        {
            for (int i = 0; i < count; i++)
            {
                list.Add(value);
            }
        }
        public static bool IsValidIndex<T>(this List<T> list, int index)
        {
            return index >= 0 && index < list.Count;
        }

        public static float Sum(this List<float> list)
        {
            float sum = 0;
            for (int i = 0; i < list.Count; i++)
            {
                sum += list[i];
            }
            return sum;
        }
    }

}