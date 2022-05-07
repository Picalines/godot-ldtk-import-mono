#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using Picalines.Godot.LDtkImport.Utils;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class TileSetImporter
    {
        public static TileSet CreateNew(string ldtkFilePath, WorldJson.TileSetDefinition tileSetJson)
        {
            var texture = GD.Load<Texture>(GetTexturePath(ldtkFilePath, tileSetJson));
            Image textureImage = texture.GetData();

            TileSet tileSet = new();

            UpdateTiles(tileSetJson, tileSet, texture, textureImage);

            return tileSet;
        }

        public static void ApplyChanges(string ldtkFilePath, WorldJson.TileSetDefinition tileSetJson, TileSet tileSet)
        {
            var texture = GD.Load<Texture>(GetTexturePath(ldtkFilePath, tileSetJson));
            Image textureImage = texture.GetData();

            UpdateTiles(tileSetJson, tileSet, texture, textureImage);
        }

        private static void UpdateTiles(WorldJson.TileSetDefinition tileSetJson, TileSet tileSet, Texture texture, Image textureImage)
        {
            int tileFullSize = tileSetJson.TileGridSize + tileSetJson.Spacing;
            int gridWidth = (tileSetJson.PxWidth - tileSetJson.Padding) / tileFullSize;
            int gridHeight = (tileSetJson.PxHeight - tileSetJson.Padding) / tileFullSize;

            int gridSize = gridWidth * gridHeight;

            var usedTileIds = tileSet.GetTilesIds();

            for (int tileId = 0; tileId < gridSize; tileId++)
            {
                if (usedTileIds.Contains(tileId))
                {
                    tileSet.TileSetTexture(tileId, texture);
                    continue;
                }

                var tileRegion = GetTileRegion(tileId, tileSetJson);

                if (!textureImage.GetRect(tileRegion).IsInvisible())
                {
                    tileSet.CreateTile(tileId);
                    tileSet.TileSetTileMode(tileId, TileSet.TileMode.SingleTile);
                    tileSet.TileSetTexture(tileId, texture);
                    tileSet.TileSetRegion(tileId, tileRegion);
                }
            }
        }

        private static string GetTexturePath(string ldtkFilePath, WorldJson.TileSetDefinition tileSetJson)
        {
            return ldtkFilePath.GetBaseDir() + "/" + tileSetJson.TextureRelPath;
        }

        private static Rect2 GetTileRegion(int tileId, WorldJson.TileSetDefinition tileSetJson) => new()
        {
            Position = TileCoord.IdToPx(tileId, tileSetJson),
            Size = tileSetJson.TileGridSizeV,
        };
    }
}

#endif