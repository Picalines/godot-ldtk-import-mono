#if TOOLS

using Godot;
using Picalines.Godot.LDtkImport.Importers;

namespace Picalines.Godot.LDtkImport
{
    [Tool]
    internal sealed partial class LDtkEditorPlugin : EditorPlugin
    {
        private const string ImportSettingsTemplatePath = "res://addons/ldtk-import/importSettingsTemplate.json";

        private const string ImportToolMenuItemName = "Import LDtk project";

        private const string ImportSettingsFileExtension = ".import.settings.json";

        private LDtkInspectorPlugin? _InspectorPlugin;

        private FileDialog _LDtkFileDialog = null!;

        private ConfirmationDialog _ImportSettingsFileCreationPopup = null!;

        public override void _EnterTree()
        {
            base._EnterTree();

            AddProjectFileOpenDialog();

            AddSettingsFileCreationPopup();

            AddToolMenuItem(ImportToolMenuItemName, this, nameof(ImportToolMenuItemHandler), "");

            AddInspectorPlugin(_InspectorPlugin = new() { EditorPlugin = this });
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            RemoveToolMenuItem(ImportToolMenuItemName);

            if (_InspectorPlugin is not null)
            {
                RemoveInspectorPlugin(_InspectorPlugin);
            }
        }

        private void AddProjectFileOpenDialog()
        {
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

            _LDtkFileDialog.Connect("file_selected", this, nameof(ImportProject));
        }

        private void AddSettingsFileCreationPopup()
        {
            AddChild(_ImportSettingsFileCreationPopup = new ConfirmationDialog()
            {
                Theme = CurrentEditorTheme,
                WindowTitle = "Import settings file was generated",
                DialogText = "Edit the file and click import again",
            });

            var okButton = _ImportSettingsFileCreationPopup.GetOk();
            okButton.Text = "Open settings in external text editor";
            okButton.Connect("pressed", this, nameof(OpenSettingsInExternalEditor));

            _ImportSettingsFileCreationPopup.GetCancel().Text = "Hide dialog";
        }

        private void ImportToolMenuItemHandler(string _)
        {
            _LDtkFileDialog.PopupCentered();
        }

        public void ImportProject(string ldtkFile)
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

            try
            {
                LDtkImporter.Import(ldtkFile, settingsFilePath);
            }
            catch (LDtkImportException exception)
            {
                GD.PushError($"LDtk import error: {exception.Message}");
                return;
            }

            GD.Print($"successfully imported LDtk project at {ldtkFile}");
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

        public Theme CurrentEditorTheme => GetEditorInterface().GetBaseControl().Theme;
    }
}

#endif