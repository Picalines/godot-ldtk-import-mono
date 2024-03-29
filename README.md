# godot-ldtk-import-mono

LDtk importer for Godot 3.4+ (specifically C# 10)

[Newtonsoft.Json](https://www.newtonsoft.com/json) **v11.0.2** is used for JSON deserialization

## State of project

I'm waiting for the release of Godot 4, who would have thought. The reasons are as follows:
 - .NET 6 support, which, to my understanding, will give the access to `System.Text.Json`. To be honest, that's why I'm not motivated to fix various `Newtonsoft.Json` vulnerabilities.
 - Better support of custom importers. I hope I can finally do a normal import of ldtk files without my json config.
 - New tilemaps will hopefully fix many problems (layers and entities)

In other words, I will need to rewrite a lot of things, and it will take quite a lot of time.

There *are* bugs in the project now, see issues tab.

## How to install

1. Add `addons/ldtk-import` to your Godot project
2. Build the C# solution
3. Enable plugin it in the settings

## Main features

You can use entity fields from LDtk with just one (two?) attribute(s)!

```csharp
using Godot;
using Picalines.Godot.LDtkImport;

public class Player : Node
{
                                  // "hp" is a field name from LDtk.
    [Export, LDtkField("hp")]     // Export is needed so that the field
    private readonly int _health; // is serialized into a scene file.
                                  // Properties are supported as well!

    public override void _Ready()
    {
        GD.Print($"player health is: {_health}");
    }
}
```

Plugin will check types of fields and give an error messages. It supports almost any field type:
 * Enums (its value names should match)
 * Points (Vector2 type for field/property)
 * EntityRefs (>=Node type. Plugin creates a special node that assings them in _Ready)
 * Arrays (of anything!) (T[] or List<T> type)

The only unsupported type is `Tile` (because I don't know what type to use for such field `:/`)

From a special `json` config plugin will generate a Godot scene files (see [how to use section](#how-to-use))
 * You can specify a base world and level scenes to include a special nodes and test them before importing
 * Entities are placed as scenes with assigned ldtk fields
 * Using `json` in tiles custom data you can place special nodes instead of tiles
 * Level fields are also supported!
 * To set tile collisions edit the auto generated `TileSet` resource
 * Background images and color are converted to `Sprite` and `ColorRect` nodes
 * The level depth value from LDtk is assigned to ZIndex property

Example of tile custom data for entity:
```json5
{
  "#entity": "Foliage",
  "#keepTileSprite": true, // plugin will search for top Sprite node
                           // and set its texture and region automatically!

  // Rest of fields are treated just like other fields!
  "wave": 4,
  "base": "LeftWall"

  // Special type examples:
  // - "MemberA" for enum Members { MemberA, MemberB, ... }
  // - "#FFCC00" for colors
  // - [4, 5] for exact Vector2
  // - { "cx": 4, "cy": 5 } for Vector2 where (4, 5) will be converted from cell coordinates to center of a level tile
  // - entity refs are not supported :/
}
```

`LDtkFieldAttribute` also can accept special data from LDtk:
 * `#size` for entity, tile and level... well... *sizes*
 * `#tileId` for... `:)`
 * `#tileSrc` for position of tile in TileSet texture (like top left of a sprite region)

Note: you can find all "magic strings" (group names, meta keys and special field names) in `LDtkConstants` class

## How to use

To import projects plugin needs a `.json` config. Create it by pressing `Project -> Tools -> Import LDtk project` in the top menu (you will need to select .ldtk file).

Here's what the config template looks like:
```json5
{
  // [The only required setting]
  // Folder in which the plugin will place
  // the generated tilesets and scenes
  "outputDir": "res://worlds/Overworld/",

  // If true, plugin will delete all (!) *.tscn files
  // from outputDir before importing. TileSets will
  // not be deleted to keep your changes like collisions
  "clearOutput": false,

  // Templates of the path by which the plugin
  // will search for the scene of entity
  // ({} is replaced by entity name from LDtk)
  "entityPaths": [
    "res://scenes/mobs/{}/{}.tscn",
    "res://scenes/decor/{}/{}.tscn"
  ],

  // If the plugin did not find the entity name
  // in entitySceneOverrides, one of entityPaths
  // will be used instead
  "entityPathOverrides": {
    "Item": "res://items/Item.tscn"
  },

  // If worldScene settings is present,
  // plugin will generate a scene with all
  // level scenes at their positions
  "worldScene": {
    // Use base to add additional nodes or script.
    // Note that you must reimport after making a
    // change in base World/Level scene. See
    // https://github.com/godotengine/godot-proposals/issues/3907
    "base": "res://scenes/world.tscn",

    // If base scene is used plugin will remove all
    // children whose names matches a mask as in
    // String.Match (case sensitive!).
    // Use this to test something before importing
    "baseIgnoreMask": "_*",

    // If present plugin will make (or search in base)
    // a seperate node for storing levels
    "levelsParent": "Levels",

    // If true plugin will place Position2D
    // nodes with level names and their scene
    // paths in EditorDescription property
    "onlyMarkers": false
  },

  "levelScene": {
    // Same as worldScene.base
    "base": "res://scenes/level.tscn",

    // Same as worldScene.baseIgnoreMask.
    // Has a higher priority than baseNodeMask!
    "baseIgnoreMask": "_*",

    // If base scene is used plugin will search for
    // children whose name matches baseNodeMask as in
    // String.Match (case sensitive!). Instead of creating
    // a new nodes plugin will use these children as a template.
    // Use this for custom materials, collision layers and etc.
    // Here's the list of nodes that the plugin will look for:
    // - TileMap in tile layer (clears existing tiles)
    // - Sprite and ColorRect for background
    "baseNodeMask": "Base*",

    // Same as worldScene.levelsParent.
    // Plugin will create/find a node for layer
    // and add generated children to it (entities or TileMaps)
    // (for example you can prepare a YSort node for entity layer)
    "layersParent": "Layers",

    // If present plugin will make (or search in base)
    // a seperate node for background (ColorRect and Sprite).
    // Else background will be placed before all other nodes
    "bgParent": "Background",

    // If true plugin will not include ColorRect node
    // for solid level background
    "ignoreBgColor": false
  }
}
```

After creating a config use `Project -> Tools -> Import LDtk project` again. To Reimport world you can open it's generated scene and press a special button in the inspector!

## ⚠ Notes

Almost any change to exported fields requires to reimport (because that's how Godot works)

To comfortably build the C# solution you will need to set `LangVersion` to `10.0` and enable `nullable`. You can do this by editing a `.csproj` file:

```xml
<PropertyGroup>
  <!-- ...there will be a target framework... -->
  <LangVersion>10.0</LangVersion>
  <Nullable>enable</Nullable>
</PropertyGroup>
<ItemGroup>
  <!-- also don't forget install Newtonsoft.Json v11.0.2 package! -->
  <PackageReference Include="Newtonsoft.Json" Version="11.0.2" />
</ItemGroup>
```

Almost all plugin related classes are included only in editor builds (`#if TOOLS`)

Internal plugin classes are marked as... well... *`internal`*. Don't use them for your game logic (because it won't compile the release build D:) (they have a different icon in your IDE!).
