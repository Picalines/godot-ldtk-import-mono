using Godot;
using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class WorldImporter
    {
        public static void Import(string ldtkFilePath, LDtkImportSettings settings, WorldJson worldJson)
        {
            var worldNode = CreateWorldNode(ldtkFilePath, settings);

            worldNode.AddToGroup(LDtkConstants.GroupNames.Worlds, persistent: true);
            worldNode.SetMeta(LDtkConstants.MetaKeys.ProjectFilePath, ldtkFilePath);
            worldNode.SetMeta(LDtkConstants.MetaKeys.ImportSettingsFilePath, settings.FilePath);

            AddLevels(settings, worldJson, worldNode);

            SaveScene(ldtkFilePath, settings, worldNode);
        }

        private static void AddLevels(LDtkImportSettings settings, WorldJson worldJson, Node worldNode)
        {
            var levelsParent = GetLevelsParent(settings, worldNode);

            foreach (var levelInfo in worldJson.Levels)
            {
                var levelScenePath = $"{settings.OutputDirectory}/{levelInfo.Identifier}.tscn";

                Node levelScene;

                if (settings.WorldSceneSettings!.OnlyMarkers)
                {
                    levelScene = new Position2D()
                    {
                        Name = levelInfo.Identifier,
                        EditorDescription = levelScenePath,
                    };
                }
                else
                {
                    var levelPackedScene = GD.Load<PackedScene>(levelScenePath);
                    levelScene = levelPackedScene.Instance<Node>();
                }

                levelScene.SetMeta(LDtkConstants.MetaKeys.LevelScenePath, levelScenePath);

                levelsParent.AddChild(levelScene);

                if (levelScene is Node2D level2D)
                {
                    level2D.Position = levelInfo.WorldPos;
                }

                levelScene.Owner = worldNode;
            }
        }

        private static Node GetLevelsParent(LDtkImportSettings settings, Node worldNode)
        {
            if (settings.WorldSceneSettings?.LevelsParentNodeName is not string levelsParentNodeName)
            {
                return worldNode;
            }

            var levelsParent = worldNode.GetNodeOrNull(levelsParentNodeName)
                ?? new Node2D() { Name = levelsParentNodeName };

            levelsParent.Owner = worldNode;

            return levelsParent;
        }

        private static Node CreateWorldNode(string ldtkFilePath, LDtkImportSettings settings)
        {
            Node worldNode;

            if (settings.WorldSceneSettings!.BaseScenePath is string baseScenePath)
            {
                var basePackedScene = GD.Load<PackedScene>(baseScenePath);
                worldNode = basePackedScene.Instance<Node>();
            }
            else
            {
                worldNode = new();
            }

            worldNode.Name = ldtkFilePath.GetFile().BaseName();
            return worldNode;
        }

        private static void SaveScene(string ldtkFilePath, LDtkImportSettings settings, Node worldNode)
        {
            var savePath = $"{settings.OutputDirectory}/{worldNode.Name}.tscn";

            var packedLevelScene = new PackedScene();
            packedLevelScene.Pack(worldNode);

            if (ResourceSaver.Save(savePath, packedLevelScene) is not Error.Ok)
            {
                throw LDtkImportException.FailedToImportWorld(ldtkFilePath);
            }
        }
    }
}