#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using Picalines.Godot.LDtkImport.Utils;
using System.Collections.Generic;
using System.Linq;

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
                layerTileMaps.Add(stackTileMap);

                stackTileMap.Name = maxTileStackSize == 1 ? "TileMap" : $"TileLayer_{i}";
                stackTileMap.AddToGroup(StackLayerGroupName, persistent: false);

                layerNode.AddChild(stackTileMap);
                stackTileMap.Owner = layerNode;
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
            Dictionary<int, TileEntityCustomData>? tileEntities)
        {
            Node entityParent = tileMap.IsInGroup(StackLayerGroupName) ? tileMap.GetParent() : tileMap;

            foreach (var tile in tiles)
            {
                if (tileEntities?.TryGetValue(tile.Id, out var tileEntityData) is true)
                {
                    var tileEntity = EntityLayerImporter.TryInstantiateEntity(context, tileEntityData.EntityName);

                    if (tileEntity is not null)
                    {
                        entityParent.AddChild(tileEntity);
                        tileEntity.Owner = tileMap.Owner;

                        PrepareTileEntity(tileEntity, tileEntityData, tileMap, tile);
                    }
                }
                else
                {
                    var gridCoords = tileMap.WorldToMap(tile.LayerPxCoords);
                    tileMap.SetCellv(gridCoords, tile.Id, tile.FlipX, tile.FlipY);
                }
            }
        }

        private static void PrepareTileEntity(
            Node tileEntity,
            TileEntityCustomData tileEntityData,
            TileMap tileMap,
            LevelJson.TileInstance tile)
        {
            if (tileEntity is Node2D entity2D)
            {
                entity2D.Position = tile.LayerPxCoords + tileMap.CellSize / 2;

                entity2D.Scale = new Vector2()
                {
                    x = tile.FlipX ? -1 : 1,
                    y = tile.FlipY ? -1 : 1,
                };
            }

            if (tileEntityData.KeepTileSprite is true)
            {
                var firstSprite = GetFirstChildOfType<Sprite>(tileEntity);

                if (firstSprite is not null)
                {
                    firstSprite.Texture = tileMap.TileSet.TileGetTexture(0);
                    firstSprite.RegionEnabled = true;
                    firstSprite.RegionRect = new() { Position = tile.TileSetPxCoords, Size = tileMap.CellSize };
                }
            }

            var entityFields = tileEntityData.EntityFields.ToDictionary(pair => pair.Key, pair => pair.Value);
            entityFields[LDtkConstants.SpecialFieldNames.TileId] = tile.Id;
            entityFields[LDtkConstants.SpecialFieldNames.TileSource] = tile.TileSetPxCoords;
            entityFields[LDtkConstants.SpecialFieldNames.Size] = tileMap.CellSize;

            LDtkFieldAssigner.Assign(tileEntity, entityFields, new() { GridSize = tileMap.CellSize });
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
            intMap.Owner = layerNode;
        }

        private static Dictionary<int, TileEntityCustomData>? GetTileEntities(LevelImportContext context, LevelJson.LayerInstance layerJson)
        {
            if (layerJson.TileSetDefUid is null)
            {
                return null;
            }

            var tileSetDefinition = context.WorldJson.Definitions.TileSets
                .First(tileSetDef => tileSetDef.Uid == layerJson.TileSetDefUid);

            return tileSetDefinition.CustomData
                .Select(customData => new { customData.TileId, Json = customData.AsEntityData() })
                .Where(data => data.Json is not null)
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