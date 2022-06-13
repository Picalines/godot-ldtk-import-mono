#if TOOLS

using Godot;
using System;

namespace Picalines.Godot.LDtkImport
{
    internal sealed class LDtkImportException : Exception
    {
        private LDtkImportException(string message) : base(message) { }

        public static LDtkImportException FailedToOpenFile(Error error) =>
            new($"failed to open file: {error}");

        public static LDtkImportException FailedToParseJsonFile(string filePath, string parseError) =>
            new($"{filePath}: {parseError}");

        public static LDtkImportException FailedToImportTileSet(string ldtkFile, string tileSetName) =>
            new($"failed to import TileSet '{tileSetName}' from {ldtkFile}");

        public static LDtkImportException FailedToImportLevel(string ldtkFile, string level) =>
            new($"failed to import level '{level}' from {ldtkFile}");

        public static LDtkImportException FailedToImportWorld(string ldtkFile) =>
            new($"failed to import world from {ldtkFile}");
    }
}

#endif