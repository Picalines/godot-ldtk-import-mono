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
        public static Node2D Import(LayerImportContext context)
        {
            var tileSet = GD.Load<TileSet>(GetTileSetPath(context));

            var tileMap = new TileMap()
            {
                TileSet = tileSet,
                Name = context.LayerJson.Identifier,
                Position = context.LayerJson.PxTotalOffset,
                CellSize = context.LayerJson.GridSizeV,
                Modulate = new Color(1, 1, 1, context.LayerJson.Opacity),
            };

            SetTiles(context.LayerJson, tileMap);

            if (context.LayerJson.Type is LayerType.IntGrid)
            {
                AddIntGrid(context.LayerJson, tileMap);
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

        private static string GetTileSetPath(LayerImportContext context)
        {
            var tileSetJson = context.WorldJson.Definitions.TileSets.First(t => t.Uid == (context.LayerJson.TileSetDefUid ?? 0));
            return $"{context.ImportSettings.OutputDirectory}/tilesets/{tileSetJson.Identifier}.tres";
        }
    }
}

#endif