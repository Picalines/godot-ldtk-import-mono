using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using LDtkImport.Json.Converters;
using Godot;
using System.Runtime.Serialization;

namespace LDtkImport.Json
{
    public class WorldJson : BaseJson<WorldJson.Root>
    {
        public class Root
        {
            [JsonProperty("jsonVersion")]
            public string JsonVersion { get; private set; }

            [JsonProperty("defaultPivotX")]
            public float DefaultPivotX { get; private set; }

            [JsonProperty("defaultPivotY")]
            public float DefaultPivotY { get; private set; }

            public Vector2 DefaultPivot { get; private set; }

            [JsonProperty("defaultGridSize")]
            public int DefaultGridSize { get; private set; }

            [JsonProperty("bgColor")]
            [JsonConverter(typeof(ColorConverter))]
            public Color BgColor { get; private set; }

            [JsonProperty("defaultLevelBgColor")]
            [JsonConverter(typeof(ColorConverter))]
            public Color DefaultLevelBgColor { get; private set; }

            [JsonProperty("externalLevels")]
            public bool ExternalLevels { get; private set; }

            [JsonProperty("worldLayout")]
            public string WorldLayout { get; private set; }

            [JsonProperty("worldGridWidth")]
            public int WorldGridWidth { get; private set; }

            [JsonProperty("worldGridHeight")]
            public int WorldGridHeight { get; private set; }

            [JsonProperty("defs")]
            public Defs Defs { get; private set; }

            [JsonProperty("levels")]
            public IReadOnlyList<LevelJson.Root> Levels { get; private set; }

            [OnDeserialized]
            private void Init(StreamingContext context)
            {
                DefaultPivot = new Vector2(DefaultPivotX, DefaultPivotY);
            }
        }

        public class IntGridValueDef
        {
            [JsonProperty("identifier")]
            public object Identifier { get; private set; }

            [JsonProperty("color")]
            public string Color { get; private set; }
        }

        public enum LayerType
        {
            Tiles,
            IntGrid,
            Entities,
            AutoLayer,
        }

        public class LayerDef
        {
            [JsonProperty("identifier")]
            public string Identifier { get; private set; }

            [JsonProperty("type")]
            [JsonConverter(typeof(StringEnumConverter))]
            public LayerType Type { get; private set; }

            [JsonProperty("uid")]
            public int Uid { get; private set; }

            [JsonProperty("gridSize")]
            public int GridSize { get; private set; }

            [JsonProperty("displayOpacity")]
            public int DisplayOpacity { get; private set; }

            [JsonProperty("pxOffsetX")]
            public int PxOffsetX { get; private set; }

            [JsonProperty("pxOffsetY")]
            public int PxOffsetY { get; private set; }

            public Vector2 PxOffset { get; private set; }

            [JsonProperty("intGridValues")]
            public IReadOnlyList<IntGridValueDef> IntGridValues { get; private set; }

            [JsonProperty("autoTilesetDefUid")]
            public int? AutoTilesetDefUid { get; private set; }

            [JsonProperty("autoSourceLayerDefUid")]
            public int? AutoSourceLayerDefUid { get; private set; }

            [JsonProperty("tilesetDefUid")]
            public int? TilesetDefUid { get; private set; }

            [OnDeserialized]
            private void Init(StreamingContext context)
            {
                PxOffset = new Vector2(PxOffsetX, PxOffsetY);
            }
        }

        public class EntityDef
        {
            [JsonProperty("identifier")]
            public string Identifier { get; private set; }

            [JsonProperty("uid")]
            public int Uid { get; private set; }

            [JsonProperty("width")]
            public int Width { get; private set; }

            [JsonProperty("height")]
            public int Height { get; private set; }

            public Vector2 Size { get; private set; }

            [JsonProperty("renderMode")]
            public string RenderMode { get; private set; }

            [JsonProperty("tilesetId")]
            public int? TilesetId { get; private set; }

            [JsonProperty("tileId")]
            public int? TileId { get; private set; }

            [JsonProperty("tileRenderMode")]
            public string TileRenderMode { get; private set; }

            [JsonProperty("pivotX")]
            public float PivotX { get; private set; }

            [JsonProperty("pivotY")]
            public float PivotY { get; private set; }

            public Vector2 Pivot { get; private set; }

            [OnDeserialized]
            private void Init(StreamingContext context)
            {
                Size = new Vector2(Width, Height);
                Pivot = new Vector2(PivotX, PivotY);
            }
        }

        public class TileSetDef
        {
            [JsonProperty("identifier")]
            public string Identifier { get; private set; }

            [JsonProperty("uid")]
            public int Uid { get; private set; }

            [JsonProperty("relPath")]
            public string TextureRelPath { get; private set; }

            [JsonProperty("pxWid")]
            public int PxWidth { get; private set; }

            [JsonProperty("pxHei")]
            public int PxHeight { get; private set; }

            public Vector2 PxSize { get; private set; }

            [JsonProperty("tileGridSize")]
            public int TileGridSize { get; private set; }

            public Vector2 TileGridSizeV { get; private set; }

            [JsonProperty("spacing")]
            public int Spacing { get; private set; }

            [JsonProperty("padding")]
            public int Padding { get; private set; }

            [OnDeserialized]
            private void Init(StreamingContext context)
            {
                PxSize = new Vector2(PxWidth, PxHeight);
                TileGridSizeV = new Vector2(TileGridSize, TileGridSize);
            }
        }

        public class EnumValue
        {
            [JsonProperty("id")]
            public string Id { get; private set; }

            [JsonProperty("tileId")]
            public int TileId { get; private set; }

            [JsonProperty("__tileSrcRect")]
            [JsonConverter(typeof(Rect2Converter))]
            public Rect2 TileSrcRect { get; private set; }
        }

        public class Enum
        {
            [JsonProperty("identifier")]
            public string Identifier { get; private set; }

            [JsonProperty("uid")]
            public int Uid { get; private set; }

            [JsonProperty("values")]
            public IReadOnlyList<EnumValue> Values { get; private set; }

            [JsonProperty("iconTilesetUid")]
            public int IconTilesetUid { get; private set; }

            [JsonProperty("externalRelPath")]
            public string? ExternalRelPath { get; private set; }
        }

        public class ExternalEnum : Enum
        {
            [JsonProperty("relPath")]
            public string RelPath { get; private set; }
        }

        public class Defs
        {
            [JsonProperty("layers")]
            public IReadOnlyList<LayerDef> Layers { get; private set; }

            [JsonProperty("entities")]
            public IReadOnlyList<EntityDef> Entities { get; private set; }

            [JsonProperty("tilesets")]
            public IReadOnlyList<TileSetDef> Tilesets { get; private set; }

            [JsonProperty("enums")]
            public IReadOnlyList<Enum> Enums { get; private set; }

            [JsonProperty("externalEnums")]
            public IReadOnlyList<ExternalEnum> ExternalEnums { get; private set; }
        }
    }
}
