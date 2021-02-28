using System;

namespace UnityEngineX
{
    public class ArrayX
    {
        public static T[] Slice<T>(T[] array, int index, int count)
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
    }
}