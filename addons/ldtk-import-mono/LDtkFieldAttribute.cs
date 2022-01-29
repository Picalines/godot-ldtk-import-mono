using System;

namespace Picalines.Godot.LDtkImport
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class LDtkFieldAttribute : Attribute
    {
        public readonly string? FieldEditorName;

        public LDtkFieldAttribute(string? fieldEditorName = null)
        {
            FieldEditorName = fieldEditorName;
        }
    }
}
