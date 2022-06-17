namespace Picalines.Godot.LDtkImport
{
    public static class LDtkConstants
    {
        public static class GroupNames
        {
            public const string Worlds = "LDtkWorlds";

            public const string Levels = "LDtkLevels";

            public const string Entities = "LDtkEntities";
        }

        public static class MetaKeys
        {
            public const string ImportSettingsFilePath = "LDtkImportSettinsFilePath";

            public const string ProjectFilePath = "LDtkProjectFilePath";

            public const string InstanceId = "LDtkInstanceId";

            public const string LevelScenePath = "LDtkLevelScenePath";
        }

        public const string SpecialFieldPrefix = "#";

        public static class SpecialFieldNames
        {
            public const string Size = $"{SpecialFieldPrefix}size";

            public const string TileEntityName = $"{SpecialFieldPrefix}entity";

            public const string TileId = $"{SpecialFieldPrefix}tileId";

            public const string TileSource = $"{SpecialFieldPrefix}tileSrc";

            public const string KeepTileSprite = $"{SpecialFieldPrefix}keepTileSprite";
        }
    }
}
