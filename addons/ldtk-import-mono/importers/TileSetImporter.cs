#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using Picalines.Godot.LDtkImport.Utils;

namespace Picalines.Godot.LDtkImport.Importers
{
    public static class TileSetImporter
    {
        public static TileSet CreateNew(WorldJson.TileSetDefinition tileSetJson, string sourceFile)
        {
            var texture = GD.Load<Texture>(GetTexturePath(tileSetJson, sourceFile));
            var textureImage = texture.GetData();

            var tileSet = new TileSet();

            UpdateTiles(tileSet, tileSetJson, texture, textureImage);

            return tileSet;
        }

        public static void ApplyChanges(TileSet tileSet, WorldJson.TileSetDefinition tileSetJson, string sourceFile)
        {
            var texture = GD.Load<Texture>(GetTexturePath(tileSetJson, sourceFile));
            var textureImage = texture.GetData();

            UpdateTiles(tileSet, tileSetJson, texture, textureImage);
        }

        private static void UpdateTiles(TileSet tileSet, WorldJson.TileSetDefinition tileSetJson, Texture texture, Image textureImage)
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

        private static string GetTexturePath(WorldJson.TileSetDefinition tileSetJson, string worldSourceFile)
        {
            return worldSourceFile.GetBaseDir() + "/" + tileSetJson.TextureRelPath;
        }

        private static Rect2 GetTileRegion(int tileId, WorldJson.TileSetDefinition tileSetJson) => new()
        {
            Position = TileCoord.IdToPx(tileId, tileSetJson),
            Size = tileSetJson.TileGridSizeV,
        };
    }
}

#endif
