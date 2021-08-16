## godot-ldtk-import-mono

LDtk importer for Godot 3.3+ (specifically C# 9)

[Newtonsoft.Json](https://www.newtonsoft.com/json) is used for JSON deserialization

This plugin adds support for 2 file formats for Godot's importer: `.ldtk` and `.ldtkl`. Here's a short tutorial on how to use it

1. Add `.ldtk` file (*world*) to the filesystem
2. Enable the *'Save levels seperatly'* option in LDtk
3. On save LDtk should create a directory with `.ldtkl` files. This plugin will make this files treated as scenes
4. Reimport the world file. This will create `TileSet` resources in `YOUR_WORLD/tilesets` folder
5. Select all levels with `Shift+click` and reimport them

After some *sticking around* you should get the idea: `.ldtk` file is a world scene, which includes all `.ldtkl` scene instances

But the real purpose of the plugin is working with C#

> ⚠ This plugin is very early in development, sometimes it crashes the editor when reimporting maps

### WorldImportExtension

```csharp
#if TOOLS // Godot will not be able to export your game without that!

using Godot;
using Picalines.Godot.LDtkImport.Importers;
using Picalines.Godot.LDtkImport.Json;

namespace Editor
{
    [LDtkImportExtension]
    public class WorldImporter : WorldImportExtension
    {
        // In methods you have acces to SceneContext and ImportContext properties
        // SceneContext contains JSON information from LDtk
        // ImportContext contains info from Godot's EditorImportPlugin.Import arguments

        public override void PrepareTileSet(TileSet tileSet, WorldJson.TileSetDefinition json)
        {
            // Here you can add collision for tileSet
        }

        public override void OnSceneBuilt(Node2D node)
        {
            // Use that for final preparation
        }
    }
}

#endif
```

### LevelImportExtension

```csharp
#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Importers;
using Picalines.Godot.LDtkImport.Json;

namespace Editor
{
    [LDtkImportExtension]
    public class LevelImporter : LevelImportExtension
    {
        // SceneContext and ImportContext are also available here

        public override void OnSceneBuilt(Node2D node)
        {
            // Use that for final preparation
        }

        public override string? GetEntityScenePath(LevelJson.EntityInstance entity)
        {
            // More on this below...
        }
    }
}

#endif
```

### Entities

```csharp
public override string? GetEntityScenePath(LevelJson.EntityInstance entity)
{
    // This will make the plugin to instance your entity scenes in levels
    // Return null to ignore
    return $"res://entities/{entity.Identifier}/{entity.Identifier}.tscn";
}
```

Let's imagine that you have some `Enemy` entity with `Health` property

```csharp
using Godot;

public class Enemy : Node2D
{
    [Export] public int Health { get; private set; }
}
```

And then you want to assign some custom field to `Health` property in the editor

```csharp
#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Importers;
using Picalines.Godot.LDtkImport.Json;

namespace Editor
{
    [LDtkEntityImporter("Enemy")] // "Enemy" is your entity identifier from LDtk
    public class EnemyImporter : EntityImporterExtension
    {
        // Only SceneContext is available this time

        public override void PrepareInstance(LevelJson.EntityInstance json, Node2D scene)
        {
            var hp = json.FieldInstances.GetValue<int>("health");

            scene.Set(nameof(Enemy.Health), hp);
        }
    }
}

#endif
```

#### YSorting

```csharp
public override bool UseYSortForEntityLayer(LevelJson.LayerInstance entityLayer)
{
    // This will set type of entity layer node to YSort instead of Node2D
    // Return conditions to filter specific layers
    return true;
}
```
