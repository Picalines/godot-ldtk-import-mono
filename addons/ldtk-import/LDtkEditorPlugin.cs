#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Importers;

namespace Picalines.Godot.LDtkImport
{
    [Tool]
    internal sealed class LDtkEditorPlugin : EditorPlugin
    {
        private const string ImportSettingsTemplatePath = "res://addons/ldtk-import/importSettingsTemplate.json";

        private const string ImportToolMenuItemName = "Import LDtk project";

        private const string ImportSettingsFileExtension = ".import.settings.json";

        private FileDialog _LDtkFileDialog = null!;

        private ConfirmationDialog _ImportSettingsFileCreationPopup = null!;

        public override void _EnterTree()
        {
            base._EnterTree();

            AddChild(_LDtkFileDialog = new FileDialog()
            {
                Resizable = true,
                RectSize = new Vector2(900, 700),
                Theme = CurrentEditorTheme,

                WindowTitle = "Select LDtk Project",
                ModeOverridesTitle = false,
                Mode = FileDialog.ModeEnum.OpenFile,
                Access = FileDialog.AccessEnum.Resources,

                Filters = new[] { "*.ldtk" },
            });

            AddChild(_ImportSettingsFileCreationPopup = new ConfirmationDialog()
            {
                Theme = CurrentEditorTheme,

                WindowTitle = "Import settings file was generated",
                DialogText = "Edit the file and click import again",
            });

            _ImportSettingsFileCreationPopup.GetOk().Text = "Open settings in external text editor";

            _ImportSettingsFileCreationPopup.GetCancel().Text = "Hide dialog";

            _LDtkFileDialog.Connect("file_selected", this, nameof(OnLDtkFileSelected));

            _ImportSettingsFileCreationPopup.GetOk().Connect("pressed", this, nameof(OpenSettingsInExternalEditor));

            AddToolMenuItem(ImportToolMenuItemName, this, nameof(ImportToolMenuItemHandler), "");
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            RemoveToolMenuItem(ImportToolMenuItemName);
        }

        private void ImportToolMenuItemHandler(string _)
        {
            _LDtkFileDialog.PopupCentered();
        }

        private void OnLDtkFileSelected(string ldtkFile)
        {
            using var file = new File();

            var settingsFilePath = ldtkFile + ImportSettingsFileExtension;

            if (!file.FileExists(settingsFilePath))
            {
                file.Open(ImportSettingsTemplatePath, File.ModeFlags.Read);
                var settingsTemplate = file.GetAsText();
                file.Close();

                file.Open(settingsFilePath, File.ModeFlags.Write);
                file.StoreString(settingsTemplate);
                file.Close();

                GD.Print($"LDtk import settings file created at {settingsFilePath}");

                _ImportSettingsFileCreationPopup.PopupCentered();
                return;
            }

            string outputDirectory;

            try
            {
                LDtkImporter.Import(ldtkFile, settingsFilePath, out outputDirectory);
            }
            catch (LDtkImportException exception)
            {
                GD.PushError($"LDtk import error: {exception.Message}");
                return;
            }

            GD.Print($"successfully imported LDtk project at {ldtkFile}");

            GetEditorInterface().GetFileSystemDock().NavigateToPath(outputDirectory);
        }

        private void OpenSettingsInExternalEditor()
        {
            var editorSettings = GetEditorInterface().GetEditorSettings();

            if (!(bool)editorSettings.GetSetting("text_editor/external/use_external_editor"))
            {
                return;
            }

            var importSettingsFilePath = ProjectSettings.GlobalizePath(_LDtkFileDialog.CurrentPath + ImportSettingsFileExtension);

            var externalEditorExecPath = (string)editorSettings.GetSetting("text_editor/external/exec_path");

            var execFlags = (string)editorSettings.GetSetting("text_editor/external/exec_flags");

            execFlags = execFlags.Replace("{file}", importSettingsFilePath);

            OS.Execute(externalEditorExecPath, execFlags.Split(' '), false);
        }

        private Theme CurrentEditorTheme => GetEditorInterface().GetBaseControl().Theme;
    }
}

#endif