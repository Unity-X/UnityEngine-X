using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class StringBuilderPool
{
    private static Queue<StringBuilder> s_pool = new Queue<StringBuilder>();

    public static StringBuilder Take()
    {
        return s_pool.Count > 0 ? s_pool.Dequeue() : new StringBuilder();
    }

    public static void Release(StringBuilder stringBuilder)
    {
        stringBuilder.Clear();
        s_pool.Enqueue(stringBuilder);
    }
}
