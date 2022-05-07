#if TOOLS

using Godot;

namespace Picalines.Godot.LDtkImport
{
    internal static class ErrorMessage
    {
        private const string ErrorPrefix = "LDtk import error:";

        public static string FailedToOpenFile(Error error) =>
            $"{ErrorPrefix} failed to open file: {error}";

        public static string FailedToParseJsonFile(string filePath, string parseError) =>
            $"{ErrorPrefix} {filePath}: {parseError}";
    }
}

#endif
