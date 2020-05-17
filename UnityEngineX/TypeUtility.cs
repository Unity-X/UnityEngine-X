﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            Assembly baseTypeAssembly = baseType.Assembly;
            string assemblyName = baseTypeAssembly.GetName().Name;


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


        private static IEnumerable<T> Concat<T>(IEnumerable<IEnumerable<T>> sequences)
        {
            return sequences.SelectMany(x => x);
        }
    }
}