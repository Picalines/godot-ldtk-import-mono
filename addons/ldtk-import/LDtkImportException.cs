#if TOOLS

using System;

namespace Picalines.Godot.LDtkImport
{
    internal sealed class LDtkImportException : Exception
    {
        public LDtkImportException(string message) : base($"LDtk import error: {message}") { }
    }
}

#endif