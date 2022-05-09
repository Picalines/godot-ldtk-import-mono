#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using Picalines.Godot.LDtkImport.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Picalines.Godot.LDtkImport.Importers
{
    // TODO: tile entities

    internal static class TileMapImporter
    {
        private const string TileEntityNameField = "$entity";

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

            if (layerJson.Type is LayerType.IntGrid)
            {
                AddIntGrid(layerJson, tileMap);
            }

            SetTiles(context, layerJson, tileMap);

            return tileMap;
        }

        private static void SetTiles(LevelImportContext context, LevelJson.LayerInstance layerJson, TileMap tileMap)
        {
            var tileSetDefinition = context.WorldJson.Definitions.TileSets
                .First(tileSetDef => tileSetDef.Uid == layerJson.TileSetDefUid);

            var tileEntities = GetTileEntities(tileSetDefinition);

            var tiles = layerJson.Type is LayerType.Tiles
                ? layerJson.GridTiles
                : layerJson.AutoLayerTiles;

            foreach (var tile in tiles)
            {
                var gridCoords = tileMap.WorldToMap(tile.LayerPxCoords);

                if (tileEntities.TryGetValue(tile.Id, out var tileCustomData))
                {
                    TryInstantiateTileEntity(context, tileMap, tile, tileCustomData);
                }
                else
                {
                    tileMap.SetCellv(gridCoords, tile.Id, tile.FlipX, tile.FlipY);
                }
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

        private static void TryInstantiateTileEntity(LevelImportContext context, TileMap tileMap, LevelJson.TileInstance tile, Dictionary<string, object> tileCustomData)
        {
            var entityName = (string)tileCustomData[TileEntityNameField];

            var tileEntity = EntityLayerImporter.TryInstantiate(context, entityName);

            if (tileEntity is null)
            {
                return;
            }

            var entityFields = tileCustomData.Where(pair => pair.Key != TileEntityNameField).ToDictionary(pair => pair.Key, pair => pair.Value);

            LDtkFieldAssigner.Assign(tileEntity, entityFields);

            tileMap.AddChild(tileEntity);

            if (tileEntity is Node2D entity2D)
            {
                entity2D.Position = tile.LayerPxCoords + tileMap.CellSize / 2;

                var scale = Vector2.One;

                if (tile.FlipX) scale.x *= -1;
                if (tile.FlipY) scale.y *= -1;

                entity2D.Scale = scale;
            }
        }

        private static Dictionary<int, Dictionary<string, object>> GetTileEntities(WorldJson.TileSetDefinition tileSetDefinition)
        {
            var tileEntities = new Dictionary<int, Dictionary<string, object>>();

            foreach (var customData in tileSetDefinition.CustomData)
            {
                var json = customData.AsJson<Dictionary<string, object>>();

                if (json?.ContainsKey(TileEntityNameField) ?? false)
                {
                    tileEntities.Add(customData.TileId, json);
                }
            }

            return tileEntities;
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