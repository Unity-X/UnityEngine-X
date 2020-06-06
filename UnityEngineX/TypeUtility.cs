using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UnityEngineX
{
    public static class TypeUtility
    {
        public static IEnumerable<Type> GetTypesDerivedFrom(Type baseType)
        {
#if UNITY_EDITOR
            return UnityEditor.TypeCache.GetTypesDerivedFrom(baseType);
#else

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            //Assembly baseTypeAssembly = baseType.Assembly;
            //string assemblyName = baseTypeAssembly.GetName().Name;


            return

                // Join results
                Concat(assemblies

                    //// Assemblies that reference T's assembly (or T's assembly itself)
                    //.Where(assembly => assembly == baseTypeAssembly || assembly.GetReferencedAssemblies().Contains(x => x.Name == assemblyName))

                        // Get types in assembly
                        .Select(assembly => assembly.GetTypes()

                            // Get all types that inherit from T
                            .Where(type => baseType.IsAssignableFrom(type) && type != baseType)));
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

        public static string GetPrettyName(this Type type)
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
                return type.Name;
            }
        }

        private static IEnumerable<T> Concat<T>(IEnumerable<IEnumerable<T>> sequences)
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
    }
}