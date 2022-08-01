#if TOOLS

using Godot;
using System;
using System.Reflection;

namespace Picalines.Godot.LDtkImport
{
    internal static class LDtkImportMessage
    {
        public const string EntityReferencesInTilesAreNotSupported = "LDtk entity reference is ignored in tiles";

        public static string FailedToCreateDirectory(string path, Error error) =>
            $"failed to create an output directory '{path}': {error}";

        public static string FailedToOpenFile(Error error) =>
            $"failed to open file: {error}";

        public static string FailedToParseJsonFile(string filePath, string parseError) =>
            $"{filePath}: {parseError}";

        public static string FailedToImportTileSet(string ldtkFile, string tileSetName) =>
            $"failed to import TileSet '{tileSetName}' from {ldtkFile}";

        public static string FailedToImportLevel(string ldtkFile, string level) =>
            $"failed to import level '{level}' from {ldtkFile}";

        public static string FailedToImportWorld(string ldtkFile) =>
            $"failed to import world from {ldtkFile}";

        public static string EnumMemberIsNotDefined(string enumValueName, Type enumType, string memberName) =>
            $"LDtk field error: value '{enumValueName}' is not defined in {enumType} enum (member {memberName})";

        public static string EntityFieldTypeError(Type targetType, string memberName, Type? fieldValueType) =>
            $"LDtk field type error: C# script expected value of type {targetType} for member {memberName}, but received {fieldValueType?.ToString() ?? "null"}";

        public static string MissingExportAttributeError(MemberInfo member) =>
            $"{nameof(ExportAttribute)} is required when {nameof(LDtkFieldAttribute)} is used ({member.DeclaringType}.{member.Name})";
    }
}

#endif