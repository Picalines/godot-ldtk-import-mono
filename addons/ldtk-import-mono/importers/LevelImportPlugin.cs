#if TOOLS

using Godot;
using System.Linq;
using Picalines.Godot.LDtkImport.Json;
using GDArray = Godot.Collections.Array;
using GDDictionary = Godot.Collections.Dictionary;

namespace Picalines.Godot.LDtkImport.Importers
{
    [Tool]
    public sealed class LevelImportPlugin : EditorSceneImportPlugin
    {
        public const string EntityScenePathReplaceTarget = "$";

        private const string BaseScenePath = "BaseScenePath";
        private const string LayersParentName = "LayersParentName";
        private const string EntityScenePath = "EntityScenePath";

        public override string GetImporterName() => "ldtk.level";
        public override string GetVisibleName() => "LDtk Level";

        public override GDArray GetRecognizedExtensions() => new() { "ldtkl" };

        public override GDArray GetImportOptions(int preset) => new()
        {
            new GDDictionary()
            {
                ["name"] = BaseScenePath,
                ["default_value"] = "",
            },
            new GDDictionary()
            {
                ["name"] = LayersParentName,
                ["default_value"] = "Layers",
            },
            new GDDictionary()
            {
                ["name"] = EntityScenePath,
                ["default_value"] = $"res://entities/{EntityScenePathReplaceTarget}/{EntityScenePathReplaceTarget}.tscn",
            }
        };

        protected override Node ImportScene(string sourceFile, string savePath, GDDictionary options, GDArray platformVariants, GDArray genFiles)
        {
            var worldJson = JsonLoader.Load<WorldJson>(sourceFile.GetBaseDir() + ".ldtk");
            var levelJson = JsonLoader.Load<LevelJson>(sourceFile);

            var levelNode = CreateLevelScene(levelJson, options);

            AddLayers(sourceFile, options, worldJson, levelJson, levelNode);

            return levelNode;
        }

        private void AddLayers(string sourceFile, GDDictionary options, WorldJson worldJson, LevelJson levelJson, Node2D levelNode)
        {
            var layersNode = levelNode.GetNodeOrNull(options[LayersParentName].ToString())
                ?? new Node2D() { Name = options[LayersParentName].ToString() };

            if (layersNode.GetParent() is null)
            {
                levelNode.AddChild(layersNode);
            }

            foreach (var layer in levelJson.LayerInstances!.Reverse())
            {
                var layerNode = layer.Type is LayerType.Entities
                    ? EntityLayerImporter.Import(options[EntityScenePath].ToString(), worldJson, layer)
                    : TileMapImporter.Import(sourceFile, worldJson, layer);

                layersNode.AddChild(layerNode);
            }
        }

        private static Node2D CreateLevelScene(LevelJson levelJson, GDDictionary options)
        {
            var baseScenePath = options[BaseScenePath].ToString();

            var scene = baseScenePath != ""
                ? GD.Load<PackedScene>(baseScenePath).Instance<Node2D>(PackedScene.GenEditState.Disabled)
                : new Node2D();

            scene.Name = levelJson.Identifier;

            return scene;
        }

        public static string GetEntityScenePath(string entityPathTemplate, LevelJson.EntityInstance entityInstance)
        {
            return entityPathTemplate.Replace(EntityScenePathReplaceTarget, entityInstance.Identifier);
        }
    }
}

#endif
