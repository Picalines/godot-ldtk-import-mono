#if TOOLS

using Godot;

namespace Picalines.Godot.LDtkImport.Utils
{
    internal static class ErrorMessage
    {
        private const string ErrorPrefix = "LDtk import error:";

        public static string FailedToImportLDtkProject(string ldtkFile) =>
            $"{ErrorPrefix} failed to import LDtk project at {ldtkFile}";

        public static string FailedToOpenFile(Error error) =>
            $"{ErrorPrefix} failed to open file: {error}";

        public static string FailedToParseJsonFile(string filePath, string parseError) =>
            $"{ErrorPrefix} {filePath}: {parseError}";

        public static string FailedToImportTileSet(string ldtkFile, string tileSetName) =>
            $"{ErrorPrefix} failed to import TileSet '{tileSetName}' from {ldtkFile}";
    }
}

#endif
