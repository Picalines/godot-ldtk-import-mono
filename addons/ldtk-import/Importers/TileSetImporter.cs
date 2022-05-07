#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;
using Picalines.Godot.LDtkImport.Utils;

namespace Picalines.Godot.LDtkImport.Importers
{
    internal static class TileSetImporter
    {
        public static TileSet Import(WorldJson.TileSetDefinition tileSetJson, string sourceFile)
        {
            var texture = GD.Load<Texture>(GetTexturePath(tileSetJson, sourceFile));
            Image textureImage = texture.GetData();

            TileSet tileSet = new();

            int tileFullSize = tileSetJson.TileGridSize + tileSetJson.Spacing;
            int gridWidth = (tileSetJson.PxWidth - tileSetJson.Padding) / tileFullSize;
            int gridHeight = (tileSetJson.PxHeight - tileSetJson.Padding) / tileFullSize;

            int gridSize = gridWidth * gridHeight;

            for (int tileId = 0; tileId < gridSize; tileId++)
            {
                var tileRegion = GetTileRegion(tileId, tileSetJson);

                if (!textureImage.GetRect(tileRegion).IsInvisible())
                {
                    tileSet.CreateTile(tileId);
                    tileSet.TileSetTileMode(tileId, TileSet.TileMode.SingleTile);
                    tileSet.TileSetTexture(tileId, texture);
                    tileSet.TileSetRegion(tileId, tileRegion);
                }
            }

            return tileSet;
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