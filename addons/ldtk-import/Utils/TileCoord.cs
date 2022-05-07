#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Json;

namespace Picalines.Godot.LDtkImport.Utils
{
    internal static class TileCoord
    {
        public static Vector2 IdToPx(int tileId, int atlasGridSize, int atlasGridWidth, int padding, int spacing)
        {
            var gridCoords = IdToGrid(tileId, atlasGridWidth);
            var pixelTileX = padding + gridCoords.x * (atlasGridSize + spacing);
            var pixelTileY = padding + gridCoords.y * (atlasGridSize + spacing);
            return new Vector2(pixelTileX, pixelTileY);
        }

        public static Vector2 IdToPx(int tileId, WorldJson.TileSetDefinition tileSetJson)
        {
            var atlasGridWidth = tileSetJson.PxWidth / tileSetJson.TileGridSize;
            return IdToPx(tileId, tileSetJson.TileGridSize, atlasGridWidth, tileSetJson.Padding, tileSetJson.Spacing);
        }

        public static Vector2 IdToGrid(int tileId, int atlasGridWidth) => new()
        {
            x = tileId - atlasGridWidth * Mathf.FloorToInt(tileId / atlasGridWidth),
            y = Mathf.FloorToInt(tileId / atlasGridWidth),
        };
    }
}

#endif