#if TOOLS

#pragma warning disable CS8618

using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Picalines.Godot.LDtkImport.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Picalines.Godot.LDtkImport.Json
{
    internal enum WorldLayout
    {
        Free,
        GridVania,
        LinearHorizontal,
        LinearVertical,
    }

    internal enum LayerType
    {
        Tiles,
        IntGrid,
        Entities,
        AutoLayer,
    }

    internal sealed class WorldJson
    {
        private WorldJson() { }

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
        [JsonConverter(typeof(StringEnumConverter))]
        public WorldLayout WorldLayout { get; private set; }

        [JsonProperty("worldGridWidth")]
        public int WorldGridWidth { get; private set; }

        [JsonProperty("worldGridHeight")]
        public int WorldGridHeight { get; private set; }

        public Vector2 WorldGridSize { get; private set; }

        [JsonProperty("defs")]
        public DefinitionsCollection Definitions { get; private set; }

        [JsonProperty("levels")]
        public IReadOnlyList<LevelJson> Levels { get; private set; }

        [OnDeserialized]
        private void Init(StreamingContext context)
        {
            DefaultPivot = new Vector2(DefaultPivotX, DefaultPivotY);
            WorldGridSize = new Vector2(WorldGridWidth, WorldGridHeight);
        }

        public sealed class IntGridValueDefinition
        {
            private IntGridValueDefinition() { }

            [JsonProperty("identifier")]
            public string? Identifier { get; private set; }

            [JsonProperty("color")]
            [JsonConverter(typeof(ColorConverter))]
            public Color Color { get; private set; }

            [JsonProperty("value")]
            public int Value { get; private set; }
        }

        public sealed class LayerDefinition
        {
            private LayerDefinition() { }

            [JsonProperty("identifier")]
            public string Identifier { get; private set; }

            [JsonProperty("type")]
            [JsonConverter(typeof(StringEnumConverter))]
            public LayerType Type { get; private set; }

            [JsonProperty("uid")]
            public int Uid { get; private set; }

            [JsonProperty("gridSize")]
            public int GridSize { get; private set; }

            public Vector2 GridSizeV { get; private set; }

            [JsonProperty("pxOffsetX")]
            public int PxOffsetX { get; private set; }

            [JsonProperty("pxOffsetY")]
            public int PxOffsetY { get; private set; }

            public Vector2 PxOffset { get; private set; }

            [JsonProperty("intGridValues")]
            public IReadOnlyList<IntGridValueDefinition>? IntGridValues { get; private set; }

            [JsonProperty("autoTilesetDefUid")]
            public int? AutoTileSetDefUid { get; private set; }

            [JsonProperty("autoSourceLayerDefUid")]
            public int? AutoSourceLayerDefUid { get; private set; }

            [JsonProperty("tilesetDefUid")]
            public int? TileSetDefUid { get; private set; }

            [OnDeserialized]
            private void Init(StreamingContext context)
            {
                PxOffset = new Vector2(PxOffsetX, PxOffsetY);
                GridSizeV = new Vector2(GridSize, GridSize);
            }
        }

        public sealed class EntityDefinition
        {
            private EntityDefinition() { }

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
            public int? TileSetId { get; private set; }

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

        public sealed class TileSetDefinition
        {
            private TileSetDefinition() { }

            public sealed class TileCustomData
            {
                private TileEntityCustomData? _EntityData = null;

                private TileCustomData() { }

                [JsonProperty("tileId")]
                public int TileId { get; private set; }

                [JsonProperty("data")]
                public string Data { get; private set; }

                public TileEntityCustomData? AsEntityData()
                {
                    try
                    {
                        return _EntityData ??= JsonConvert.DeserializeObject<TileEntityCustomData>(Data);
                    }
                    catch (JsonException)
                    {
                        return null;
                    }
                }
            }

            public sealed class TileEnumTag
            {
                private TileEnumTag() { }

                [JsonProperty("enumValueId")]
                public string EnumValueId { get; private set; }

                [JsonProperty("tileIds")]
                public IReadOnlyList<int> TileIds { get; private set; }
            }

            [JsonProperty("identifier")]
            public string Identifier { get; private set; }

            [JsonProperty("uid")]
            public int Uid { get; private set; }

            [JsonProperty("embedAtlas")]
            public string? EmbedAtlas { get; private set; }

            [JsonProperty("relPath")]
            public string? TextureRelPath { get; private set; }

            [JsonProperty("pxWid")]
            public int PxWidth { get; private set; }

            [JsonProperty("__cHei")]
            public int GridHeight { get; private set; }

            [JsonProperty("__cWid")]
            public int GridWidth { get; private set; }

            [JsonProperty("pxHei")]
            public int PxHeight { get; private set; }

            public Vector2 PxSize { get; private set; }

            public Vector2 GridSize { get; private set; }

            [JsonProperty("tileGridSize")]
            public int TileGridSize { get; private set; }

            public Vector2 TileGridSizeV { get; private set; }

            [JsonProperty("spacing")]
            public int Spacing { get; private set; }

            [JsonProperty("padding")]
            public int Padding { get; private set; }

            [JsonProperty("customData")]
            public IReadOnlyList<TileCustomData> CustomData { get; private set; }

            [JsonProperty("enumTags")]
            public IReadOnlyList<TileEnumTag> EnumTags { get; private set; }

            [JsonProperty("tagsSourceEnumUid")]
            public int? TagsSourceEnumUid { get; private set; }

            [OnDeserialized]
            private void Init(StreamingContext context)
            {
                PxSize = new Vector2(PxWidth, PxHeight);
                GridSize = new Vector2(GridWidth, GridHeight);
                TileGridSizeV = new Vector2(TileGridSize, TileGridSize);
            }
        }

        public sealed class EnumValue
        {
            private EnumValue() { }

            [JsonProperty("id")]
            public string Id { get; private set; }

            [JsonProperty("tileId")]
            public int? TileId { get; private set; }

            [JsonProperty("__tileSrcRect", NullValueHandling = NullValueHandling.Ignore)]
            [JsonConverter(typeof(Rect2Converter))]
            public Rect2? TileSrcRect { get; private set; }

            [JsonProperty("color")]
            public int Color { get; private set; }
        }

        public class Enum
        {
            protected Enum() { }

            [JsonProperty("identifier")]
            public string Identifier { get; private set; }

            [JsonProperty("uid")]
            public int Uid { get; private set; }

            [JsonProperty("values")]
            public IReadOnlyList<EnumValue> Values { get; private set; }

            [JsonProperty("iconTilesetUid")]
            public int IconTileSetUid { get; private set; }

            [JsonProperty("externalRelPath")]
            public string? ExternalRelPath { get; private set; }
        }

        public sealed class ExternalEnum : Enum
        {
            private ExternalEnum() { }

            [JsonProperty("relPath")]
            public string RelPath { get; private set; }
        }

        public sealed class DefinitionsCollection
        {
            private DefinitionsCollection() { }

            [JsonProperty("layers")]
            public IReadOnlyList<LayerDefinition> Layers { get; private set; }

            [JsonProperty("entities")]
            public IReadOnlyList<EntityDefinition> Entities { get; private set; }

            [JsonProperty("tilesets")]
            public IReadOnlyList<TileSetDefinition> TileSets { get; private set; }

            [JsonProperty("enums")]
            public IReadOnlyList<Enum> Enums { get; private set; }

            [JsonProperty("externalEnums")]
            public IReadOnlyList<ExternalEnum> ExternalEnums { get; private set; }
        }
    }
}

#endif