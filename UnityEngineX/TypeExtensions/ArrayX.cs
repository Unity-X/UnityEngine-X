using System;

namespace UnityEngineX
{
    public class ArrayX
    {
        public static T[] SubArray<T>(T[] array, int index, int count)
        {
            if (count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            var result = new T[count];

            if(count > 0)
            {
                Array.Copy(array, index, result, 0, count);
            }

            return result;
        }
        
        public static T[] Concat<T>(T[] a, T[] b)
        {
            if (a is null)
                throw new ArgumentNullException(nameof(a));

            if (b is null)
                throw new ArgumentNullException(nameof(b));

            var result = new T[a.Length + b.Length];
            a.CopyTo(result, 0);
            b.CopyTo(result, a.Length);

            return result;
        }
    }
}