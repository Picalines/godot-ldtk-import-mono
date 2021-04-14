using Godot;
using System.Linq;
using LDtkImport.Json;
using LDtkImport.Utils;

namespace LDtkImport.Importers
{
    public static class TileMapLayerImporter
    {
        public static string GetTileSetPath(LevelImportContext levelContext, int tileSetUid)
        {
            WorldJson.TileSetDef tileSetJson = levelContext.WorldJson.Defs.Tilesets.First(t => t.Uid == tileSetUid);
            return $"{levelContext.SourceFile.GetBaseDir()}/tilesets/{tileSetJson.Identifier}.tres";
        }

        public static TileMap Import(LevelImportContext levelContext, LevelJson.LayerInstance layer)
        {
            var tileSet = GD.Load<TileSet>(GetTileSetPath(levelContext, layer.TilesetDefUid ?? 0));

            TileMap tileMap = new()
            {
                TileSet = tileSet,
                Name = layer.Identifier,
                Position = layer.PxTotalOffset,
                CellSize = layer.GridSizeV,
                Modulate = new Color(1, 1, 1, layer.Opacity),
            };

            foreach (var tile in layer.AutoLayerTiles)
            {
                bool flipX = System.Convert.ToBoolean(tile.FlipBits & 1);
                bool flipY = System.Convert.ToBoolean(tile.FlipBits & 2);
                Vector2 gridCoords = CoordUtils.CoordIdToGridCoords(tile.InternalEditorData[1], layer.CellsWidth);
                tileMap.SetCellv(gridCoords, tile.TileId, flipX, flipY);
            }

            TileMap intMap = new()
            {
                Name = "IntGrid",
                CellSize = tileMap.CellSize,
            };

            foreach (var gridValue in layer.IntGrid)
            {
                var gridCoords = CoordUtils.CoordIdToGridCoords(gridValue.CoordId, layer.CellsWidth);
                intMap.SetCellv(gridCoords, gridValue.GridValue);
            }

            tileMap.AddChild(intMap);

            return tileMap;
        }
    }
}