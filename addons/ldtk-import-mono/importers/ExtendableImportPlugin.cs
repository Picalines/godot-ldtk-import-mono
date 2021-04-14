using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using Godot.Collections;
using GDArray = Godot.Collections.Array;

namespace LDtkImport.Importers
{
    public abstract class ExtendableImportPlugin<Extension, Attribute> : EditorImportPlugin
        where Extension : ImportPluginExtension
        where Attribute : System.Attribute
    {
        public override GDArray GetImportOptions(int preset) => new();

        protected FileImportContext ImportContext { get; private set; }
        protected Extension? UsedExtension { get; private set; }

        protected abstract Error Import();

        public sealed override int Import(string sourceFile, string savePath, Dictionary options, GDArray platformVariants, GDArray genFiles)
        {
            ImportContext = new FileImportContext
            {
                SourceFile = sourceFile,
                SavePath = savePath,
                Options = options,
                PlatformVariants = platformVariants,
                GenFiles = genFiles
            };

            UsedExtension = GetExtension();

            return (int)Import();
        }

        private Extension? GetExtension()
        {
            IEnumerable<Type> extensionTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && !type.IsGenericType)
                .Where(type => System.Attribute.IsDefined(type, typeof(Attribute)))
                .Where(type => typeof(Extension).IsAssignableFrom(type));

            Type? extensionType = extensionTypes.FirstOrDefault();
            if (extensionType is null)
            {
                return null;
            }

            if (extensionTypes.Count() > 1)
            {
                GD.PushWarning($"More than one {typeof(Extension).Name} found. Using the first one ({extensionType.FullName})");
            }

            var extensionInstance = (Extension)Activator.CreateInstance(extensionType);

            typeof(Extension)
                .GetProperty(nameof(ImportPluginExtension.ImportContext))
                .SetValue(extensionInstance, ImportContext);

            return extensionInstance;
        }
    }
}
