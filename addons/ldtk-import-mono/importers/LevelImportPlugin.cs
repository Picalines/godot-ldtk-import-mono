#if TOOLS

using Godot;
using System;
using System.Linq;
using Picalines.Godot.LDtkImport.Json;
using GDArray = Godot.Collections.Array;
using GDDictionary = Godot.Collections.Dictionary;

namespace Picalines.Godot.LDtkImport.Importers
{
    [Tool]
    public sealed class LevelImportPlugin : EditorSceneImportPlugin
    {
        private const string EntityScenePath = "EntityScenePath";

        public override string GetImporterName() => "ldtk.level";
        public override string GetVisibleName() => "LDtk Level";

        public override GDArray GetRecognizedExtensions() => new() { "ldtkl" };

        public override GDArray GetImportOptions(int preset) => new()
        {
            new GDDictionary()
            {
                ["name"] = EntityScenePath,
                ["default_value"] = $"res://entities/{EntityLayerImporter.EntityScenePathReplaceTarget}/",
            }
        };

        protected override Node ImportScene(string sourceFile, string savePath, GDDictionary options, GDArray platformVariants, GDArray genFiles)
        {
            var worldJson = JsonLoader.Load<WorldJson>(sourceFile.GetBaseDir() + ".ldtk");
            var levelJson = JsonLoader.Load<LevelJson>(sourceFile);

            Node2D levelNode = new() { Name = levelJson.Identifier };

            AddLayers(sourceFile, options[EntityScenePath].ToString(), worldJson, levelJson, levelNode);

            return levelNode;
        }

        private void AddLayers(string sourceFile, string entityScenePath, WorldJson worldJson, LevelJson levelJson, Node2D levelNode)
        {
            var tileMapImporter = new Lazy<TileMapLayerImporter>(() => new(sourceFile, worldJson));

            var entityLayerImporter = new Lazy<EntityLayerImporter>(() => new(entityScenePath));

            foreach (var layer in levelJson.LayerInstances!.Reverse())
            {
                var layerNode = layer.Type == LayerType.Entities
                    ? entityLayerImporter.Value.Import(layer)
                    : tileMapImporter.Value.Import(layer);

                levelNode.AddChild(layerNode);
            }
        }
    }
}

#endif
