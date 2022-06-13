using System;

namespace Picalines.Godot.LDtkImport
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class LDtkFieldAttribute : Attribute
    {
        public string LDtkFieldName { get; }

        public LDtkFieldAttribute(string ldtkFieldName)
        {
            LDtkFieldName = ldtkFieldName;
        }
    }
}
