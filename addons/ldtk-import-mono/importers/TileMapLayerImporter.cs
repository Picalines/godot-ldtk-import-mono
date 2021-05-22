#if TOOLS

using Godot;
using System.Linq;
using LDtkImport.Json;
using LDtkImport.Utils;

namespace LDtkImport.Importers
{
    public static class TileMapLayerImporter
    {
        public static string GetTileSetPath(FileImportContext importContext, LevelImportContext levelContext, int tileSetUid)
        {
            var tileSetJson = levelContext.WorldJson.Definitions.TileSets.First(t => t.Uid == tileSetUid);
            return $"{importContext.SourceFile.GetBaseDir()}/tilesets/{tileSetJson.Identifier}.tres";
        }

        public static TileMap Import(FileImportContext importContext, LevelImportContext levelContext, LevelJson.LayerInstance layer)
        {
            return Import(GetTileSetPath(importContext, levelContext, layer.TileSetDefUid ?? 0), layer);
        }

        public static TileMap Import(string path, LevelJson.LayerInstance layer)
        {
            var tileSet = GD.Load<TileSet>(path);

            TileMap tileMap = new()
            {
                TileSet = tileSet,
                Name = layer.Identifier,
                Position = layer.PxTotalOffset,
                CellSize = layer.GridSizeV,
                Modulate = new Color(1, 1, 1, layer.Opacity),
            };

            SetTiles(layer, tileMap);

            if (layer.Type == LayerType.IntGrid)
            {
                AddIntGrid(layer, tileMap);
            }

            return tileMap;
        }

        private static void SetTiles(LevelJson.LayerInstance layer, TileMap tileMap)
        {
            var tiles = layer.Type == LayerType.Tiles ? layer.GridTiles : layer.AutoLayerTiles;

            foreach (var tile in tiles)
            {
                Vector2 gridCoords = tileMap.WorldToMap(tile.LayerPxCoords);
                tileMap.SetCellv(gridCoords, tile.Id, tile.FlipX, tile.FlipY);
            }
        }

        private static void AddIntGrid(LevelJson.LayerInstance layer, TileMap tileMap)
        {
            TileMap intMap = new()
            {
                Name = "IntGrid",
                CellSize = tileMap.CellSize,
            };

            var cellsWithId = layer.IntGrid.Select((value, id) => new { id, value });
            var nonEmptyCells = cellsWithId.Where(p => p.value > 0);

            foreach (var p in nonEmptyCells)
            {
                var gridCoords = TileCoord.IdToGrid(p.id, layer.CellsWidth);
                intMap.SetCellv(gridCoords, p.value);
            }

            tileMap.AddChild(intMap);
        }
    }
}

#endif
