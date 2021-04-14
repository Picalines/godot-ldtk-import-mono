using Godot;
using Godot.Collections;
using LDtkImport.Json;

namespace LDtkImport.Importers
{
    [Tool]
    public class WorldImport : ExtendableImportPlugin<WorldImportExtension, LDtkImportExtensionAttribute>
    {
        public override string GetImporterName() => "ldtk.world";
        public override string GetVisibleName() => "World Importer";

        public override string GetResourceType() => nameof(Resource);

        public override Array GetRecognizedExtensions() => new() { "ldtk" };
        public override string GetSaveExtension() => "tres";

        public override int GetPresetCount() => 1;
        public override string GetPresetName(int preset) => "default";

        public override bool GetOptionVisibility(string option, Dictionary options) => true;

        protected override Error Import()
        {
            if (ImportContext.SourceFile.EndsWith(".backup.ldtk"))
            {
                return Error.Ok;
            }

            string baseDir = ImportContext.SourceFile.BaseName();
            using (var dir = new Directory())
            {
                if (!dir.DirExists(baseDir))
                {
                    return Error.FileNotFound;
                }
            }

            var worldData = WorldJson.Load(ImportContext.SourceFile);

            if (UsedExtension is not null)
            {
                typeof(WorldImportExtension)
                    .GetProperty(nameof(WorldImportExtension.WorldJson))
                    .SetValue(UsedExtension, worldData);
            }

            foreach (WorldJson.TileSetDef tileSet in worldData.Defs.Tilesets)
            {
                if (TileSetImport.Import(tileSet, ImportContext.SourceFile) != Error.Ok)
                {
                    return Error.Bug;
                }
            }

            UsedExtension?.OnWorldImported();

            return ResourceSaver.Save($"{ImportContext.SavePath}.{GetSaveExtension()}", new Resource());
        }
    }
}
