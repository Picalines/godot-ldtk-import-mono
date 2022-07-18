#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using Picalines.Godot.LDtkImport.Utils;
using System.Collections.Generic;
using System.Linq;

using TileEntityDictionary = System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>>;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class TileMapImporter
    {
        private const string StackLayerGroupName = "LDtkStackLayer";

        public static void Import(LevelImportContext context, LevelJson.LayerInstance layerJson, Node layerNode)
        {
            var tileSet = GetTileSetPath(context, layerJson) is string tileSetPath
                ? GD.Load<TileSet>(tileSetPath)
                : null;

            var tiles = layerJson.Type switch
            {
                LayerType.Tiles => layerJson.GridTiles,
                _ => layerJson.AutoLayerTiles,
            };

            var tileStacks = tiles.GroupBy(tile => tile.LayerPxCoords);

            var maxTileStackSize = tileStacks
                .Select(stack => stack.Count())
                .DefaultIfEmpty(0)
                .Max();

            var layerTileMaps = new List<TileMap>(maxTileStackSize);

            var baseTileMap = BaseSceneImporter.FindOrCreateBaseNode<TileMap>(layerNode);

            baseTileMap.Clear();

            baseTileMap.TileSet = tileSet;
            baseTileMap.CellSize = layerJson.GridSizeV;

            for (int i = 0; i < maxTileStackSize; i++)
            {
                var stackTileMap = (baseTileMap.Duplicate() as TileMap)!;

                stackTileMap.Name = maxTileStackSize == 1 ? "TileMap" : $"TileLayer_{i}";
                stackTileMap.AddToGroup(StackLayerGroupName, persistent: false);

                layerTileMaps.Add(stackTileMap);
                layerNode.AddChild(stackTileMap);
            }

            baseTileMap.Free();
            baseTileMap = null;

            var tileLayers = tileStacks
                .SelectMany(stack => stack.Select((tile, index) => new { tile, layer = index }))
                .GroupBy(tile => tile.layer, elementSelector: tile => tile.tile);

            var tileEntities = GetTileEntities(context, layerJson);

            foreach (var tileLayer in tileLayers)
            {
                SetTilesOrEntities(context, layerTileMaps[tileLayer.Key], tileLayer, tileEntities);
            }

            if (layerJson.Type is LayerType.IntGrid)
            {
                AddIntGrid(layerJson, layerNode);
            }
        }

        private static void SetTilesOrEntities(
            LevelImportContext context,
            TileMap tileMap,
            IEnumerable<LevelJson.TileInstance> tiles,
            TileEntityDictionary? tileEntities)
        {
            foreach (var tile in tiles)
            {
                if (tileEntities?.TryGetValue(tile.Id, out var tileCustomData) is true)
                {
                    var entity = TryCreateTileEntity(context, tileCustomData, tile, tileMap.CellSize, tileMap.TileSet);

                    if (entity is null)
                    {
                        continue;
                    }

                    Node entityParent = tileMap;

                    if (tileMap.IsInGroup(StackLayerGroupName))
                    {
                        entityParent = tileMap.GetParent();
                    }

                    entityParent.AddChild(entity);
                }
                else
                {
                    var gridCoords = tileMap.WorldToMap(tile.LayerPxCoords);
                    tileMap.SetCellv(gridCoords, tile.Id, tile.FlipX, tile.FlipY);
                }
            }
        }

        private static Node? TryCreateTileEntity(
            LevelImportContext context,
            Dictionary<string, object> tileCustomData,
            LevelJson.TileInstance tile,
            Vector2 cellSize,
            TileSet tileSet)
        {
            var tileEntity = EntityLayerImporter.TryInstantiate(context, (string)tileCustomData[LDtkConstants.SpecialFieldNames.TileEntityName]);

            if (tileEntity is null)
            {
                return null;
            }

            if (tileEntity is Node2D entity2D)
            {
                entity2D.Position = tile.LayerPxCoords + cellSize / 2;

                entity2D.Scale = new Vector2()
                {
                    x = tile.FlipX ? -1 : 1,
                    y = tile.FlipY ? -1 : 1,
                };
            }

            var entityFields = tileCustomData
                .Where(pair => pair.Key != LDtkConstants.SpecialFieldNames.TileEntityName)
                .Append(new(LDtkConstants.SpecialFieldNames.TileId, tile.Id))
                .Append(new(LDtkConstants.SpecialFieldNames.TileSource, tile.TileSetPxCoords))
                .Append(new(LDtkConstants.SpecialFieldNames.Size, cellSize))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            if (entityFields.TryGetValue(LDtkConstants.SpecialFieldNames.KeepTileSprite, out var keepTileSprite) && keepTileSprite is true)
            {
                var firstSprite = GetFirstChildOfType<Sprite>(tileEntity);

                if (firstSprite is not null)
                {
                    firstSprite.Texture = tileSet.TileGetTexture(0);
                    firstSprite.RegionEnabled = true;
                    firstSprite.RegionRect = new() { Position = tile.TileSetPxCoords, Size = cellSize };
                }
            }

            LDtkFieldAssigner.Assign(tileEntity, entityFields, new() { GridSize = cellSize });

            return tileEntity;
        }

        private static void AddIntGrid(LevelJson.LayerInstance layer, Node layerNode)
        {
            var intMap = new TileMap()
            {
                Name = "IntGrid",
                CellSize = layer.GridSizeV,
            };

            var nonEmptyCells = layer.IntGrid
                .Select((value, id) => new { id, value })
                .Where(p => p.value > 0);

            foreach (var p in nonEmptyCells)
            {
                var gridCoords = TileCoord.IdToGrid(p.id, layer.CellsWidth);
                intMap.SetCellv(gridCoords, p.value);
            }

            layerNode.AddChild(intMap);
        }

        private static TileEntityDictionary? GetTileEntities(LevelImportContext context, LevelJson.LayerInstance layerJson)
        {
            if (layerJson.TileSetDefUid is null)
            {
                return null;
            }

            var tileSetDefinition = context.WorldJson.Definitions.TileSets
                .First(tileSetDef => tileSetDef.Uid == layerJson.TileSetDefUid);

            return tileSetDefinition.CustomData
                .Select(customData => new { customData.TileId, Json = customData.AsJson<Dictionary<string, object>>() })
                .Where(data => data.Json?.ContainsKey(LDtkConstants.SpecialFieldNames.TileEntityName) is true)
                .ToDictionary(data => data.TileId, data => data.Json!);
        }

        private static string? GetTileSetPath(LevelImportContext context, LevelJson.LayerInstance layerJson)
        {
            if (layerJson.TileSetDefUid is null)
            {
                return null;
            }

            var tileSetJson = context.WorldJson.Definitions.TileSets
                .First(t => t.Uid == layerJson.TileSetDefUid);

            return tileSetJson.EmbedAtlas is null
                ? $"{context.ImportSettings.OutputDirectory}/tilesets/{tileSetJson.Identifier}.tres"
                : null;
        }

        private static T? GetFirstChildOfType<T>(Node node) where T : Node
        {
            if (node is T found)
            {
                return found;
            }

            foreach (Node child in node.GetChildren())
            {
                if (GetFirstChildOfType<T>(child) is T foundInner)
                {
                    return foundInner;
                }
            }

            return null;
        }
    }
}

#endif