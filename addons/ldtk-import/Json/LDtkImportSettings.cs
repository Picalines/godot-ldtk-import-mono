#if TOOLS

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Picalines.Godot.LDtkImport.Json
{
    internal sealed class LDtkImportSettings
    {
        private LDtkImportSettings() { }

        [JsonProperty("outputDir", Required = Required.Always)]
        public string OutputDirectory { get; private set; } = null!;

        [JsonProperty("entityScenePath")]
        public string? EntityScenePathTemplate { get; private set; }

        [JsonProperty("entitySceneOverrides")]
        public IReadOnlyDictionary<string, string>? EntityScenePathOverrides { get; private set; }

        [JsonProperty("worldScene")]
        public WorldSceneImportSettings? WorldSceneSettings { get; private set; }

        [JsonProperty("levelScene")]
        public LevelSceneImportSettings? LevelSceneSettings { get; private set; }

        [OnDeserialized]
        private void Init(StreamingContext _)
        {
            OutputDirectory = OutputDirectory.TrimEnd('/');
        }

        public string? GetEntityScenePath(string entityName)
        {
            if (EntityScenePathOverrides?.TryGetValue(entityName, out var path) ?? false)
            {
                return path;
            }

            return EntityScenePathTemplate?.Replace("{}", entityName);
        }
    }

    internal abstract class SceneImportSettings
    {
        [JsonProperty("baseScenePath")]
        public string? BaseScenePath { get; private set; }

        [JsonProperty("scriptPath")]
        public string? ScriptPath { get; private set; }
    }

    internal sealed class WorldSceneImportSettings : SceneImportSettings
    {
        private WorldSceneImportSettings() { }

        [JsonProperty("levelsParent")]
        public string? LevelsParentNodeName { get; private set; }

        [JsonProperty("onlyMarkers")]
        public bool OnlyMarkers { get; private set; } = false;
    }

    internal sealed class LevelSceneImportSettings : SceneImportSettings
    {
        private LevelSceneImportSettings() { }

        [JsonProperty("layersParent")]
        public string? LayersParentNodeName { get; private set; }
    }
}

#endif