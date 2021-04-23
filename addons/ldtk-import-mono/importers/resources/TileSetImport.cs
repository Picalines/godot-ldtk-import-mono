using Godot;
using LDtkImport.Json;
using LDtkImport.Utils;

namespace LDtkImport.Importers
{
    public static class TileSetImporter
    {
        public static Error Import(WorldJson.TileSetDef tileSetJson, string sourceFile, WorldImportExtension? extension)
        {
            const string tileSetsDir = "tilesets";

            TileSet tileSet = CreateTileSet(tileSetJson, sourceFile, extension);

            using (var dir = new Directory())
            {
                dir.Open(sourceFile.BaseName());
                var error = dir.MakeDir(tileSetsDir);
                if (!(error == Error.Ok || error == Error.AlreadyExists))
                {
                    return error;
                }
            }

            string savePath = $"{sourceFile.BaseName()}/{tileSetsDir}/{tileSetJson.Identifier}.tres";

            return ResourceSaver.Save(savePath, tileSet);
        }

        public static string GetTexturePath(WorldJson.TileSetDef tileSetJson, string worldSourceFile)
        {
            return worldSourceFile.GetBaseDir() + "/" + tileSetJson.TextureRelPath;
        }

        private static TileSet CreateTileSet(WorldJson.TileSetDef tileSetJson, string sourceFile, WorldImportExtension? extension)
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
                Rect2 tileRegion = GetTileRegion(tileId, tileSetJson);
                Image tileImage = textureImage.GetRect(tileRegion);
                if (!tileImage.IsInvisible())
                {
                    tileSet.CreateTile(tileId);
                    tileSet.TileSetTileMode(tileId, TileSet.TileMode.SingleTile);
                    tileSet.TileSetTexture(tileId, texture);
                    tileSet.TileSetRegion(tileId, tileRegion);
                }
            }

            if (extension is not null)
            {
                extension.PrepareTileSet(tileSet, tileSetJson);
            }

            return tileSet;
        }

        private static Rect2 GetTileRegion(int tileId, WorldJson.TileSetDef tileSetJson)
        {
            var pixelTile = TileCoord.IdToPx(tileId, tileSetJson);
            return new Rect2(pixelTile, tileSetJson.TileGridSizeV);
        }
    }
}
