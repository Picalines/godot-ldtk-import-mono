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
    public enum LevelNeighbourDirection
    {
        North,
        South,
        West,
        East,
    }

    public sealed class LevelJson
    {
        private LevelJson() { }

        [JsonProperty("identifier")]
        public string Identifier { get; private set; }

        [JsonProperty("uid")]
        public int Uid { get; private set; }

        [JsonProperty("worldX")]
        public int WorldX { get; private set; }

        [JsonProperty("worldY")]
        public int WorldY { get; private set; }

        public Vector2 WorldPos { get; private set; }

        [JsonProperty("pxWid")]
        public int PxWidth { get; private set; }

        [JsonProperty("pxHei")]
        public int PxHeight { get; private set; }

        public Vector2 PxSize { get; private set; }

        [JsonProperty("__bgColor")]
        [JsonConverter(typeof(ColorConverter))]
        public Color? BgColor { get; private set; }

        [JsonProperty("__bgPos")]
        public BackgroundPosition? BgPosition { get; private set; }

        [JsonProperty("bgRelPath")]
        public string? BgRelPath { get; private set; }

        [JsonProperty("externalRelPath")]
        public string? ExternalRelPath { get; private set; }

        [JsonProperty("layerInstances")]
        public IReadOnlyList<LayerInstance>? LayerInstances { get; private set; }

        [JsonProperty("fieldInstances")]
        public IReadOnlyList<FieldInstance> FieldInstances { get; private set; }

        [JsonProperty("__neighbours")]
        public IReadOnlyList<Neighbour> Neighbours { get; private set; }

        [OnDeserialized]
        private void Init(StreamingContext context)
        {
            WorldPos = new Vector2(WorldX, WorldY);
            PxSize = new Vector2(PxWidth, PxHeight);
        }

        public sealed class BackgroundPosition
        {
            private BackgroundPosition() { }

            [JsonProperty("cropRect")]
            [JsonConverter(typeof(Rect2Converter))]
            public Rect2 CropRect { get; private set; }

            [JsonProperty("scale")]
            [JsonConverter(typeof(Vector2Converter))]
            public Vector2 Scale { get; private set; }

            [JsonProperty("topLeftPx")]
            [JsonConverter(typeof(Vector2Converter))]
            public Vector2 TopLeftPxCoords { get; private set; }
        }

        public sealed class Neighbour
        {
            private Neighbour() { }

            [JsonProperty("levelUid")]
            public int LevelUid { get; private set; }

            [JsonProperty("dir")]
            [JsonConverter(typeof(LevelNeighbourDirectionConverter))]
            public LevelNeighbourDirection Direction { get; private set; }
        }

        public sealed class TileInstance
        {
            private TileInstance() { }

            [JsonProperty("px")]
            [JsonConverter(typeof(Vector2Converter))]
            public Vector2 LayerPxCoords { get; private set; }

            [JsonProperty("src")]
            [JsonConverter(typeof(Vector2Converter))]
            public Vector2 TileSetPxCoords { get; private set; }

            [JsonProperty("f")]
            public int FlipBits { get; private set; }

            public bool FlipX { get; private set; }
            public bool FlipY { get; private set; }

            [JsonProperty("t")]
            public int Id { get; private set; }

            [OnDeserialized]
            private void Init(StreamingContext context)
            {
                FlipX = System.Convert.ToBoolean(FlipBits & 1);
                FlipY = System.Convert.ToBoolean(FlipBits & 2);
            }
        }

        public sealed class FieldInstance
        {
            private FieldInstance() { }

            [JsonProperty("__identifier")]
            public string Identifier { get; private set; }

            [JsonProperty("__value")]
            [JsonConverter(typeof(FieldInstanceValueConverter))]
            public object Value { get; private set; }

            [JsonProperty("__type")]
            public string Type { get; private set; }

            [JsonProperty("defUid")]
            public string DefUid { get; private set; }

            [JsonProperty("__tags")]
            public IReadOnlyList<string> Tags { get; private set; }
        }

        public sealed class EntityInstance
        {
            private EntityInstance() { }

            public sealed class TileDisplay
            {
                private TileDisplay() { }

                [JsonProperty("tilesetUid")]
                public int TileSetUid { get; private set; }

                [JsonProperty("srcRect")]
                [JsonConverter(typeof(Rect2Converter))]
                public Rect2 SrcRect { get; private set; }
            }

            [JsonProperty("iid")]
            public string Id { get; private set; }

            [JsonProperty("__identifier")]
            public string Identifier { get; private set; }

            [JsonProperty("width")]
            public int Width { get; private set; }

            [JsonProperty("height")]
            public int Height { get; private set; }

            public Vector2 Size { get; private set; }

            [JsonProperty("__grid")]
            [JsonConverter(typeof(Vector2Converter))]
            public Vector2 GridCoords { get; private set; }

            [JsonProperty("__pivot")]
            [JsonConverter(typeof(Vector2Converter))]
            public Vector2 Pivot { get; private set; }

            [JsonProperty("__tile")]
            public TileDisplay? Tile { get; private set; }

            [JsonProperty("defUid")]
            public int DefUid { get; private set; }

            [JsonProperty("px")]
            [JsonConverter(typeof(Vector2Converter))]
            public Vector2 PxCoords { get; private set; }

            [JsonProperty("fieldInstances")]
            public IReadOnlyList<FieldInstance> FieldInstances { get; private set; }

            [OnDeserialized]
            private void Init(StreamingContext context)
            {
                Size = new Vector2(Width, Height);
            }
        }

        public sealed class LayerInstance
        {
            private LayerInstance() { }

            [JsonProperty("__identifier")]
            public string Identifier { get; private set; }

            [JsonProperty("__type")]
            [JsonConverter(typeof(StringEnumConverter))]
            public LayerType Type { get; private set; }

            [JsonProperty("__cWid")]
            public int CellsWidth { get; private set; }

            [JsonProperty("__cHei")]
            public int CellsHeight { get; private set; }

            public Vector2 CellsSize { get; private set; }

            [JsonProperty("__gridSize")]
            public int GridSize { get; private set; }

            public Vector2 GridSizeV { get; private set; }

            [JsonProperty("__opacity")]
            public int Opacity { get; private set; }

            [JsonProperty("__pxTotalOffsetX")]
            public int PxTotalOffsetX { get; private set; }

            [JsonProperty("__pxTotalOffsetY")]
            public int PxTotalOffsetY { get; private set; }

            public Vector2 PxTotalOffset { get; private set; }

            [JsonProperty("__tilesetDefUid")]
            public int? TileSetDefUid { get; private set; }

            [JsonProperty("__tilesetRelPath")]
            public string? TileSetRelPath { get; private set; }

            [JsonProperty("levelId")]
            public int LevelId { get; private set; }

            [JsonProperty("layerDefUid")]
            public int LayerDefUid { get; private set; }

            [JsonProperty("pxOffsetX")]
            public int PxOffsetX { get; private set; }

            [JsonProperty("pxOffsetY")]
            public int PxOffsetY { get; private set; }

            public Vector2 PxOffset { get; private set; }

            [JsonProperty("intGridCsv")]
            public IReadOnlyList<int> IntGrid { get; private set; }

            [JsonProperty("seed")]
            public int Seed { get; private set; }

            [JsonProperty("autoLayerTiles")]
            public IReadOnlyList<TileInstance> AutoLayerTiles { get; private set; }

            [JsonProperty("gridTiles")]
            public IReadOnlyList<TileInstance> GridTiles { get; private set; }

            [JsonProperty("entityInstances")]
            public IReadOnlyList<EntityInstance> EntityInstances { get; private set; }

            [OnDeserialized]
            private void Init(StreamingContext context)
            {
                PxTotalOffset = new Vector2(PxTotalOffsetX, PxTotalOffsetY);
                PxOffset = new Vector2(PxOffsetX, PxOffsetY);
                CellsSize = new Vector2(CellsWidth, CellsHeight);
                GridSizeV = new Vector2(GridSize, GridSize);
            }
        }
    }
}

#endif