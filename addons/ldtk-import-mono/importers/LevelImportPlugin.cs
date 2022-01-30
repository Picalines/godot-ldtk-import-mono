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

        private const string EntityScenePath = "EntityScenePath";

        public override string GetImporterName() => "ldtk.level";
        public override string GetVisibleName() => "LDtk Level";

        public override GDArray GetRecognizedExtensions() => new() { "ldtkl" };

        public override GDArray GetImportOptions(int preset) => new()
        {
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

            Node2D levelNode = new() { Name = levelJson.Identifier };

            AddLayers(sourceFile, options[EntityScenePath].ToString(), worldJson, levelJson, levelNode);

            return levelNode;
        }

        private void AddLayers(string sourceFile, string entityPathTemplate, WorldJson worldJson, LevelJson levelJson, Node2D levelNode)
        {
            foreach (var layer in levelJson.LayerInstances!.Reverse())
            {
                var layerNode = layer.Type is LayerType.Entities
                    ? EntityLayerImporter.Import(entityPathTemplate, worldJson, layer)
                    : TileMapImporter.Import(sourceFile, worldJson, layer);

                levelNode.AddChild(layerNode);
            }
        }

        public static string GetEntityScenePath(string entityPathTemplate, LevelJson.EntityInstance entityInstance)
        {
            return entityPathTemplate.Replace(EntityScenePathReplaceTarget, entityInstance.Identifier);
        }
    }
}

#endif
