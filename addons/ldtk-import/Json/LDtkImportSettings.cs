#if TOOLS

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Picalines.Godot.LDtkImport.Json
{
    internal sealed class LDtkImportSettings
    {
        private LDtkImportSettings() { }

        public string? FilePath { get; set; }

        [JsonProperty("outputDir", Required = Required.Always)]
        public string OutputDirectory { get; private set; } = null!;

        [JsonProperty("clearOutput")]
        public bool ClearOutput { get; private set; } = false;

        [JsonProperty("entityPaths")]
        public IReadOnlyList<string>? EntityScenePathTemplates { get; private set; }

        [JsonProperty("entityPathOverrides")]
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

        public IEnumerable<string> GetPossibleEntityPaths(string entityName)
        {
            if (EntityScenePathOverrides?.TryGetValue(entityName, out var overridenPath) ?? false)
            {
                yield return overridenPath;
            }
            else if (EntityScenePathTemplates is not null)
            {
                var resolvedTemplates = EntityScenePathTemplates.Select(template => template.Replace("{}", entityName));

                foreach (var path in resolvedTemplates)
                {
                    yield return path;
                }
            }
        }
    }

    internal abstract class SceneImportSettings
    {
        [JsonProperty("base")]
        public string? BaseScenePath { get; private set; }
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

        [JsonProperty("bgParent")]
        public string? BackgroundParentNodeName { get; private set; }
    }
}

#endif