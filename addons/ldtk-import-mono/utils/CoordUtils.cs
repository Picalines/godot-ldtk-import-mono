using Godot;
using LDtkImport.Json;

namespace LDtkImport.Utils
{
    public static class CoordUtils
    {
        public static Vector2 TileIdToPxCoords(int tileId, int atlasGridSize, int atlasGridWidth, int padding, int spacing)
        {
            var gridCoords = TileIdToGridCoords(tileId, atlasGridWidth);
            var pixelTileX = padding + gridCoords.x * (atlasGridSize + spacing);
            var pixelTileY = padding + gridCoords.y * (atlasGridSize + spacing);
            return new Vector2(pixelTileX, pixelTileY);
        }

        public static Vector2 TileIdToPxCoords(int tileId, WorldJson.TileSetDef tileSetJson)
        {
            var atlasGridWidth = tileSetJson.PxWidth / tileSetJson.TileGridSize;
            return TileIdToPxCoords(tileId, tileSetJson.TileGridSize, atlasGridWidth, tileSetJson.Padding, tileSetJson.Spacing);
        }

        public static Vector2 TileIdToGridCoords(int tileId, int atlasGridWidth) => new()
        {
            x = tileId - atlasGridWidth * Mathf.FloorToInt(tileId / atlasGridWidth),
            y = Mathf.FloorToInt(tileId / atlasGridWidth),
        };

        public static Vector2 CoordIdToGridCoords(int coordId, int gridWidth)
        {
            var gridY = coordId / gridWidth;
            var gridX = coordId - gridY * gridWidth;
            return new Vector2(gridX, gridY);
        }
    }
}
