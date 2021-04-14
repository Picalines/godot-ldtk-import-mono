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

            SetTiles(layer, tileMap);

            if (layer.IntGrid is not null)
            {
                AddIntGrid(layer, tileMap);
            }

            return tileMap;
        }

        private static void SetTiles(LevelJson.LayerInstance layer, TileMap tileMap)
        {
            int coordIdIndex = layer.AutoLayerTiles is null ? 0 : 1;

            foreach (var tile in layer.AutoLayerTiles ?? layer.GridTiles!)
            {
                bool flipX = System.Convert.ToBoolean(tile.FlipBits & 1);
                bool flipY = System.Convert.ToBoolean(tile.FlipBits & 2);
                Vector2 gridCoords = TileCoord.IdToGrid(tile.InternalEditorData[coordIdIndex], layer.CellsWidth);
                tileMap.SetCellv(gridCoords, tile.Id, flipX, flipY);
            }
        }

        private static void AddIntGrid(LevelJson.LayerInstance layer, TileMap tileMap)
        {
            TileMap intMap = new()
            {
                Name = "IntGrid",
                CellSize = tileMap.CellSize,
            };

            var gridWithIds = layer.IntGrid!.Select((value, id) => new { id, value });

            foreach (var p in gridWithIds.Where(p => p.value > 0))
            {
                var gridCoords = TileCoord.IdToGrid(p.id, layer.CellsWidth);
                intMap.SetCellv(gridCoords, p.value);
            }

            tileMap.AddChild(intMap);
        }
    }
}