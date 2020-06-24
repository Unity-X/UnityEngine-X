using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngineX;

namespace UnityEngineX
{
    public static class TypeUtility
    {
        public static IEnumerable<MethodInfo> GetStaticMethodsWithAttribute(Type attributeType)
        {
#if UNITY_EDITOR
            return UnityEditor.TypeCache.GetMethodsWithAttribute(attributeType).Where(m => m.IsStatic);
#else

            Assembly attributeAssembly = attributeType.Assembly;
            ConcurrentBag<MethodInfo> result = new ConcurrentBag<MethodInfo>();
            List<Thread> threads = new List<Thread>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // assembly must be referencing type
                if (assembly != attributeAssembly && !assembly.GetReferencedAssemblies().Contains(attributeAssembly))
                    continue;

                var t = new Thread(() =>
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                        {
                            if (Attribute.IsDefined(method, attributeType))
                                result.Add(method);
                        }
                    }
                });

                threads.Add(t);

                t.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            return result;
#endif
        }
        public static IEnumerable<MethodInfo> GetMethodsWithAttribute(Type attributeType)
        {
#if UNITY_EDITOR
            return UnityEditor.TypeCache.GetMethodsWithAttribute(attributeType);
#else

            Assembly attributeAssembly = attributeType.Assembly;

            ConcurrentBag<MethodInfo> result = new ConcurrentBag<MethodInfo>();
            List<Thread> threads = new List<Thread>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // assembly must be referencing type
                if (assembly != attributeAssembly && !assembly.GetReferencedAssemblies().Contains(attributeAssembly))
                    continue;

                var t = new Thread(() =>
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                        {
                            if (Attribute.IsDefined(method, attributeType))
                                result.Add(method);
                        }
                    }
                });

                threads.Add(t);

                t.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            return result;
#endif
        }

        public static IEnumerable<Type> GetTypesDerivedFrom(Type baseType)
        {
#if UNITY_EDITOR
            return UnityEditor.TypeCache.GetTypesDerivedFrom(baseType);
#else
            Assembly baseTypeAssembly = baseType.Assembly;
            List<Type> result = new List<Type>(128);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // assembly must be referencing type
                if (assembly != baseTypeAssembly && !assembly.GetReferencedAssemblies().Contains(baseTypeAssembly))
                    continue;

                foreach (var type in assembly.GetTypes())
                {
                    if (type != baseType && baseType.IsAssignableFrom(type))
                        result.Add(type);
                }
            }

            return result;
#endif
        }

        public static string GetPrettyFullName(this Type type)
        {
            StringBuilder stringBuilder = StringBuilderPool.Take();

            if (type.IsArray)
            {
                stringBuilder.Append(type.GetElementType().GetPrettyFullName());
                stringBuilder.Append("[]");
            }
            else if (type.IsGenericType)
            {
                Queue<Type> genericArguments = new Queue<Type>(type.GenericTypeArguments);
                string typeName = type.FullName;

                typeName = typeName.Substring(0, typeName.IndexOf('['));

                int begin;
                int end = -1;
                int i = 0;
                bool isParsingGenericArgCount = false;

                void appendNoGenericArgument()
                {
                    stringBuilder.Append(typeName.Substring(begin, end - begin));
                }
                void appendGenericArguments()
                {
                    string argCountTxt = typeName.Substring(begin, end - begin);
                    int argCount = int.Parse(argCountTxt);

                    stringBuilder.Append('<');
                    for (int a = 0; a < argCount; a++)
                    {
                        stringBuilder.Append(genericArguments.Dequeue().GetPrettyFullName());
                        if (argCount > 1 && a < argCount - 1)
                            stringBuilder.Append(", ");
                    }
                    stringBuilder.Append('>');
                }
                void append()
                {
                    begin = end + 1;
                    end = i;
                    if (begin < end)
                    {
                        if (isParsingGenericArgCount)
                            appendGenericArguments();
                        else
                            appendNoGenericArgument();
                    }
                }

                for (; i < typeName.Length; i++)
                {
                    if (isParsingGenericArgCount && !char.IsDigit(typeName[i]))
                    {
                        append();
                        isParsingGenericArgCount = false;
                    }

                    if (typeName[i] == '`')
                    {
                        append();
                        isParsingGenericArgCount = true;
                    }
                }

                append();
            }
            else
            {
                stringBuilder.Append(type.FullName);
            }

            stringBuilder.Replace('+', '.');

            string result = stringBuilder.ToString();

            StringBuilderPool.Release(stringBuilder);

            return result;
        }

        private static readonly Dictionary<Type, string> s_shorthandMap = new Dictionary<Type, string>
        {
            { typeof(Boolean), "bool" },
            { typeof(Byte), "byte" },
            { typeof(Char), "char" },
            { typeof(Decimal), "decimal" },
            { typeof(Double), "double" },
            { typeof(Single), "float" },
            { typeof(Int32), "int" },
            { typeof(Int64), "long" },
            { typeof(SByte), "sbyte" },
            { typeof(Int16), "short" },
            { typeof(String), "string" },
            { typeof(UInt32), "uint" },
            { typeof(UInt64), "ulong" },
            { typeof(UInt16), "ushort" },
        };

        public static string GetPrettyName(this Type type, bool shorthandFormIfPossible = true)
        {
            if (type.IsArray)
            {
                return type.GetElementType().GetPrettyName() + "[]";
            }
            else if (type.IsGenericType)
            {
                StringBuilder stringBuilder = StringBuilderPool.Take();

                string typeName = type.Name;
                int indexOfGenericArguments = typeName.IndexOf('`');
                int genericArgCount = typeName.ParseInt(indexOfGenericArguments + 1);
                string typeNameNoGenerics = typeName.Substring(0, indexOfGenericArguments);


                stringBuilder.Append(typeNameNoGenerics);
                stringBuilder.Append("<");
                var genericArgs = type.GenericTypeArguments;
                for (int i = genericArgs.Length - genericArgCount; i < genericArgs.Length; i++)
                {
                    stringBuilder.Append(GetPrettyName(genericArgs[i]));
                    if (i < genericArgs.Length - 1 && genericArgCount > 1)
                    {
                        stringBuilder.Append(", ");
                    }
                }
                stringBuilder.Append(">");

                string result = stringBuilder.ToString();

                StringBuilderPool.Release(stringBuilder);

                return result;
            }
            else
            {
                if (shorthandFormIfPossible && s_shorthandMap.TryGetValue(type, out string result))
                    return result;
                return type.Name;
            }
        }

        private static IEnumerable<T> Concat<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            return sequences.SelectMany(x => x);
        }

        public static Type FindType(string qualifiedTypeName)
        {
            return FindType(qualifiedTypeName, throwOnError: true);
        }

        public static Type FindType(string qualifiedTypeName, bool throwOnError)
        {
            Type t = Type.GetType(qualifiedTypeName, throwOnError);

            if (t != null)
            {
                return t;
            }
            else
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    t = asm.GetType(qualifiedTypeName, throwOnError);
                    if (t != null)
                        return t;
                }
                return null;
            }
        }

        public static bool IsAsync(this MethodInfo methodInfo)
        {
            return Attribute.IsDefined(methodInfo, typeof(AsyncStateMachineAttribute));
        }
    }
}