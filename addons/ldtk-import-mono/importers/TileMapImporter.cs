#if TOOLS

using Godot;
using System.Linq;
using Picalines.Godot.LDtkImport.Json;
using Picalines.Godot.LDtkImport.Utils;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class TileMapImporter
    {
        public static TileMap Import(string sourceFile, WorldJson worldJson, LevelJson.LayerInstance layer)
        {
            var tileSet = GD.Load<TileSet>(GetTileSetPath(sourceFile, worldJson, layer.TileSetDefUid ?? 0));

            var tileMap = new TileMap()
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
            var intMap = new TileMap()
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

        private static string GetTileSetPath(string sourceFile, WorldJson worldJson, int tileSetUid)
        {
            var tileSetJson = worldJson.Definitions.TileSets.First(t => t.Uid == tileSetUid);
            return $"{sourceFile.GetBaseDir()}/{WorldImportPlugin.TileSetsDirectory}/{tileSetJson.Identifier}.tres";
        }
    }
}

#endif
