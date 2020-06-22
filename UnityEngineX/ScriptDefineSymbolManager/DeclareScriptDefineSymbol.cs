using System;

namespace UnityEngineX
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class DeclareScriptDefineSymbol : Attribute
    {
        /// <summary>
        /// The name of the symbol
        /// </summary>
        public string Name;

        /// <summary>
        /// The description displayed in the Project Settings
        /// </summary>
        public string Description;

        public DeclareScriptDefineSymbol(string name, string description)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }
    }
}