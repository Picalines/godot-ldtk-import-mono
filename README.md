# godot-ldtk-import-mono

LDtk importer for Godot 3.4+ (specifically C# 10)

[Newtonsoft.Json](https://www.newtonsoft.com/json) is used for JSON deserialization

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
                                  // Auto properties are supported as well!

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

From a special `json` config plugin will generate a Godot scene files (see [How to use section](#how-to-use))
 * You can specify a base world and level scenes to include a special nodes
 * Entities are placed as scenes with assigned fields
 * Using `json` in tiles custom data you can place special nodes instead of tiles
 * Level fields are also supported!
 * To set tile collisions edit the auto generated `TileSet` resource
 * Background images and color are converted to `Sprite` and `ColorRect` nodes
 * Add `YSort` tag to Entity layer in LDtk to use `YSort` node
 * The level depth value from LDtk is assigned to ZIndex property

Example of tile custom data for entity:
```json5
{
  "$entity": "Foliage",
  "$keepTileSprite": true, // plugin will search for top Sprite node
                           // and set its texture and region automatically!

  // Rest of fields are treated just like other fields!
  "wave": 4,
  "base": "LeftWall"
}
```

`LDtkFieldAttribute` also can accept special data from LDtk:
 * `$size` for entity, tile and level... well... *sizes*
 * `$tileId` for... `:)`
 * `$tileSrc` for position of tile in TileSet texture (like top left of a sprite region)

## How to use

To import projects plugin needs a `.json` config. Create it by pressing `Project -> Tools -> Import LDtk project` in the top menu (you will need to select .ldtk file).

Config will looks like this:
```json5
{
  // [!The only required setting!]
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
    // Use base to add additional nodes or script
    "base": "res://scenes/world.tscn",

    // If present, plugin will make (or search in base)
    // a seperate node for storing levels
    "levelsParent": "Levels",

    // If true, plugin will place Position2D
    // nodes with level names and their scene
    // paths in EditorDescription property
    "onlyMarkers": false
  },

  "levelScene": {
    // same as worldScene.base
    "base": "res://scenes/level.tscn",

    // same as worldScene.levelsParent
    "layersParent": "Layers",

    // If present, plugin will make (or search in base)
    // a seperate node for background (ColorRect or Sprite).
    // Else background will be placed before all other nodes
    "bgParent": "Background"
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

Internal plugin classes are marked as... well... *`internal`*. Don't use them for your game logic (because it won't compile the release build `D:`) (they have a different icon in your IDE!).
