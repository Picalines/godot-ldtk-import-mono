#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using GDArray = Godot.Collections.Array;
using GDDictionary = Godot.Collections.Dictionary;

namespace Picalines.Godot.LDtkImport.Importers;

[Tool]
public sealed class WorldImportPlugin : EditorSceneImportPlugin
{
    public const string TileSetsDirectory = "tilesets";

    private const string BaseScenePath = "BaseScenePath";
    private const string LevelsParentName = "LevelsParentName";

    public override string GetImporterName() => "ldtk.world";
    public override string GetVisibleName() => "LDtk World";

    public override GDArray GetRecognizedExtensions() => new() { "ldtk" };

    public override GDArray GetImportOptions(int preset) => new()
    {
        new GDDictionary()
        {
            ["name"] = BaseScenePath,
            ["default_value"] = "",
        },
        new GDDictionary()
        {
            ["name"] = LevelsParentName,
            ["default_value"] = "Levels",
        }
    };

    protected override Node ImportScene(string sourceFile, string savePath, GDDictionary options, GDArray platformVariants, GDArray genFiles)
    {
        CheckRequiredDirs(sourceFile);

        var worldJson = JsonLoader.Load<WorldJson>(sourceFile);

        ImportTileSets(sourceFile, genFiles, worldJson);

        var worldNode = CreateWorldScene(sourceFile, options);

        PlaceLevels(sourceFile, options, worldJson, worldNode);

        return worldNode;
    }

    private void PlaceLevels(string sourceFile, GDDictionary options, WorldJson worldJson, Node2D worldNode)
    {
        if (!worldJson.ExternalLevels)
        {
            GD.PushError($"{sourceFile}: please turn on the 'Save levels separately' project option");
            return;
        }

        var levelsParent = worldNode.GetNodeOrNull(options[LevelsParentName].ToString())
            ?? new Node2D { Name = options[LevelsParentName].ToString() };

        if (levelsParent.GetParent() is null)
        {
            worldNode.AddChild(levelsParent);
        }

        foreach (LevelJson levelJson in worldJson.Levels)
        {
            var path = $"{sourceFile.GetBaseDir()}/{levelJson.ExternalRelPath}";
            var scene = GD.Load<PackedScene>(path);
            var instance = scene.Instance();

            if (instance is not Node2D levelNode)
            {
                GD.PushError($"scene of type {nameof(Node2D)} expected at {path}");
                return;
            }

            foreach (Node child in instance.GetChildren())
            {
                child.Free();
            }

            levelNode.Position = levelJson.WorldPos;

            levelsParent.AddChild(levelNode);
        }
    }

    private void ImportTileSets(string sourceFile, GDArray genFiles, WorldJson worldJson)
    {
        var tileSetsDir = $"{sourceFile.BaseName()}/{TileSetsDirectory}";

        foreach (var tileSetJson in worldJson.Definitions.TileSets)
        {
            var savePath = $"{tileSetsDir}/{tileSetJson.Identifier}.tres";

            TileSet tileSet;

            using (var file = new File())
            {
                if (file.FileExists(savePath))
                {
                    tileSet = GD.Load<TileSet>(savePath);
                    TileSetImporter.ApplyChanges(tileSet, tileSetJson, sourceFile);
                    continue;
                }
            }

            tileSet = TileSetImporter.CreateNew(tileSetJson, sourceFile);

            if (ResourceSaver.Save(savePath, tileSet) != Error.Ok)
            {
                GD.PushError($"failed to import tileset '{tileSetJson.Identifier}'");
                continue;
            }

            genFiles.Add(savePath);
        }
    }

    private static Node2D CreateWorldScene(string sourceFile, GDDictionary options)
    {
        var baseScenePath = options[BaseScenePath].ToString();

        var scene = baseScenePath != ""
            ? GD.Load<PackedScene>(baseScenePath).Instance<Node2D>(PackedScene.GenEditState.Disabled)
            : new Node2D();

        scene.Name = sourceFile.BaseName().Substring(sourceFile.GetBaseDir().Length);

        return scene;
    }

    private void CheckRequiredDirs(string sourceFile)
    {
        using var dir = new Directory();
        var baseDir = sourceFile.BaseName();
        dir.MakeDirRecursive($"{baseDir}/{TileSetsDirectory}");
    }
}

#endif
