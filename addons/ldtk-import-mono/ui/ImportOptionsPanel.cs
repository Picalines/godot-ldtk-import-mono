#if TOOLS

using System.Collections.Generic;
using Godot;
using Picalines.Godot.LDtkImport.Importers;
using Picalines.Godot.LDtkImport.Json;

#pragma warning disable CS8618

namespace Picalines.Godot.LDtkImport.UI
{
    [Tool]
    internal sealed class ImportOptionsPanel : Control
    {
        [Signal]
        private delegate void initialized();

        // Property values are set in the Godot editor

        public bool GenerateWorldScene { get; private set; }

        public string? BaseWorldScene { get => _BaseWorldScene; set => _BaseWorldScene = NullIfWhiteSpace(value); }

        public string? LevelsParentName { get => _LevelsParentName; set => _LevelsParentName = NullIfWhiteSpace(value); }

        public string? BaseLevelScene { get => _BaseLevelScene; set => _BaseLevelScene = NullIfWhiteSpace(value); }

        public string? LayersParentName { get => _LayersParentName; set => _LayersParentName = NullIfWhiteSpace(value); }

        private readonly Dictionary<string, string> _EntityScenePaths = new();

        private readonly Dictionary<string, string> _TileScenePaths = new();

        private string? _BaseWorldScene;

        private string? _LevelsParentName;

        private string? _BaseLevelScene;

        private string? _LayersParentName;

        private WorldJson _WorldJson;

        public void Initialize(string worldFilePath)
        {
            try
            {
                _WorldJson = JsonLoader.Load<WorldJson>(worldFilePath);
            }
            catch (System.Exception exception)
            {
                GD.PushError(exception.Message);
                return;
            }

            PreparePathTables();

            EmitSignal(nameof(initialized));

            Show();
        }

        public LDtkImportOptions GetOptions() => new(
            GetWorldImportOptions(),
            GetLevelImportOptions(),
            _EntityScenePaths,
            _TileScenePaths
        );

        private LDtkLevelImportOptions GetLevelImportOptions() => new(
            BaseLevelScene,
            LayersParentName
        );

        private LDtkWorldImportOptions? GetWorldImportOptions()
        {
            if (!GenerateWorldScene)
            {
                return null;
            }

            return new LDtkWorldImportOptions(
                BaseWorldScene,
                LevelsParentName
            );
        }

        private void PreparePathTables()
        {
            _EntityScenePaths.Clear();
            _TileScenePaths.Clear();
        }

        private static string? NullIfWhiteSpace(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}

#endif
