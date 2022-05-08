#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using Picalines.Godot.LDtkImport.Utils;
using System.Linq;

namespace Picalines.Godot.LDtkImport.Importers
{
    // TODO: tile entities

    internal static class TileMapImporter
    {
        public static TileMap Import(LevelImportContext context, LevelJson.LayerInstance layerJson)
        {
            var tileSet = GD.Load<TileSet>(GetTileSetPath(context, layerJson));

            var tileMap = new TileMap()
            {
                TileSet = tileSet,
                Name = layerJson.Identifier,
                Position = layerJson.PxTotalOffset,
                CellSize = layerJson.GridSizeV,
                Modulate = new Color(1, 1, 1, layerJson.Opacity),
            };

            SetTiles(layerJson, tileMap);

            if (layerJson.Type is LayerType.IntGrid)
            {
                AddIntGrid(layerJson, tileMap);
            }

            return tileMap;
        }

        private static void SetTiles(LevelJson.LayerInstance layer, TileMap tileMap)
        {
            var tiles = layer.Type is LayerType.Tiles
                ? layer.GridTiles
                : layer.AutoLayerTiles;

            foreach (var tile in tiles)
            {
                var gridCoords = tileMap.WorldToMap(tile.LayerPxCoords);
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

        private static string GetTileSetPath(LevelImportContext context, LevelJson.LayerInstance layerJson)
        {
            var tileSetJson = context.WorldJson.Definitions.TileSets
                .First(t => t.Uid == (layerJson.TileSetDefUid ?? 0));

            return $"{context.ImportSettings.OutputDirectory}/tilesets/{tileSetJson.Identifier}.tres";
        }
    }
}

#endif