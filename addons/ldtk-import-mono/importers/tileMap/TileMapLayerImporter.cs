#if TOOLS

using Godot;
using System.Linq;
using LDtkImport.Json;
using LDtkImport.Utils;

namespace LDtkImport.Importers
{
    public class TileMapLayerImporter
    {
        public FileImportContext FileContext { get; init; }
        public LevelImportContext SceneContext { get; init; }

        public TileMap Import(LevelJson.LayerInstance layer)
        {
            var tileSet = GD.Load<TileSet>(GetTileSetPath(layer.TileSetDefUid ?? 0));

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

        private string GetTileSetPath(int tileSetUid)
        {
            var tileSetJson = SceneContext.WorldJson.Definitions.TileSets.First(t => t.Uid == tileSetUid);
            return $"{FileContext.SourceFile.GetBaseDir()}/tilesets/{tileSetJson.Identifier}.tres";
        }
    }
}

#endif