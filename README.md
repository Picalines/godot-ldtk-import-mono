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

### Entities

Let's imagine that you have some `Enemy` entity with `Health` property, that can be set in LDtk

```csharp
using Godot;
using Picalines.Godot.LDtkImport;

[LDtkEntity]
public class Enemy : Node2D
{
    [Export, LDtkField] public int Health { get; private set; }
}
```
