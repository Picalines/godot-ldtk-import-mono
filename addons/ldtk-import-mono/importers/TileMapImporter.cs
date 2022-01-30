#if TOOLS

using Godot;
using System.Linq;
using Picalines.Godot.LDtkImport.Json;
using Picalines.Godot.LDtkImport.Utils;
using System.Collections.Generic;

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

            var tileSetDef = worldJson.Definitions.TileSets.First(t => t.Uid == layer.TileSetDefUid);

            var tileCustomData = GetTileEntityCustomData(tileSetDef);

            SetTiles(layer, tileMap, tileCustomData);

            if (layer.Type is LayerType.IntGrid)
            {
                AddIntGrid(layer, tileMap);
            }

            return tileMap;
        }

        private static void SetTiles(LevelJson.LayerInstance layer, TileMap tileMap, Dictionary<int, Dictionary<string, object>> tileScenes)
        {
            var tiles = layer.Type is LayerType.Tiles ? layer.GridTiles : layer.AutoLayerTiles;

            foreach (var tile in tiles)
            {
                if (tileScenes.TryGetValue(tile.Id, out var customData))
                {
                    InstanceSceneAtTile(tileMap, tile, customData);
                }
                else
                {
                    var gridCoords = tileMap.WorldToMap(tile.LayerPxCoords);
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

        private static void InstanceSceneAtTile(TileMap tileMap, LevelJson.TileInstance tile, Dictionary<string, object> customData)
        {
            var packedScene = GD.Load<PackedScene>(customData["scene"].ToString());
            var sceneInstance = packedScene.Instance<Node>();

            foreach (Node child in sceneInstance.GetChildren())
            {
                child.Free();
            }

            if (sceneInstance is Node2D node2D)
            {
                node2D.Position = tile.LayerPxCoords + tileMap.CellSize / 2;
            }

            LDtkFieldAssigner.Assign(sceneInstance, customData);

            tileMap.AddChild(sceneInstance);
        }

        private static Dictionary<int, Dictionary<string, object>> GetTileEntityCustomData(WorldJson.TileSetDefinition tileSetDef)
        {
            Dictionary<int, Dictionary<string, object>> tileCustomData = new();

            foreach (var customData in tileSetDef.CustomData)
            {
                var jsonData = customData.AsJson<Dictionary<string, object>>();

                if (jsonData?.ContainsKey("scene") ?? false)
                {
                    tileCustomData[customData.TileId] = jsonData;
                }
            }

            return tileCustomData;
        }

        private static string GetTileSetPath(string sourceFile, WorldJson worldJson, int tileSetUid)
        {
            var tileSetJson = worldJson.Definitions.TileSets.First(t => t.Uid == tileSetUid);
            return $"{sourceFile.GetBaseDir()}/{WorldImportPlugin.TileSetsDirectory}/{tileSetJson.Identifier}.tres";
        }
    }
}

#endif
