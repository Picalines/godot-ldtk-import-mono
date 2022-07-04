#if TOOLS

using Godot;

namespace Picalines.Godot.LDtkImport
{
    [Tool]
    internal sealed class LDtkInspectorPlugin : EditorInspectorPlugin
    {
        private const string ReimportButtonText = "Reimport LDtk world";

        public LDtkEditorPlugin EditorPlugin { get; init; } = null!;

        private Node? _CurrentNode;

        public override bool CanHandle(Object @object)
        {
            if (@object is not Node node)
            {
                return false;
            }

            if (node != EditorPlugin.GetEditorInterface().GetEditedSceneRoot())
            {
                return false;
            }

            return node.IsInGroup(LDtkConstants.GroupNames.Worlds) || node.IsInGroup(LDtkConstants.GroupNames.Levels);
        }

        public override void ParseBegin(Object @object)
        {
            base.ParseBegin(@object);

            _CurrentNode = @object as Node;

            if (_CurrentNode?.IsInGroup(LDtkConstants.GroupNames.Worlds) ?? false)
            {
                var reloadButton = new Button()
                {
                    Text = ReimportButtonText,
                    Theme = EditorPlugin.CurrentEditorTheme,
                };

                reloadButton.Connect("button_down", reloadButton, "set", new() { "text", "in progress..." });

                reloadButton.Connect("button_up", reloadButton, "set", new() { "text", ReimportButtonText });

                reloadButton.Connect("pressed", this, nameof(OnReloadButtonPressed));

                AddCustomControl(reloadButton);
            }

            if (_CurrentNode?.GetMeta(LDtkConstants.MetaKeys.ImportSettingsFilePath) is string settingsFilePath)
            {
                var openSettingsButton = new Button()
                {
                    Text = "Open LDtk import settings file",
                    Theme = EditorPlugin.CurrentEditorTheme,
                };

                openSettingsButton.Connect("pressed", EditorPlugin, nameof(LDtkEditorPlugin.OpenFileInExternalEditor), new() { settingsFilePath });

                AddCustomControl(openSettingsButton);
            }
        }

        private void OnReloadButtonPressed()
        {
            if (_CurrentNode?.GetMeta(LDtkConstants.MetaKeys.ProjectFilePath) is not string ldtkFilePath)
            {
                return;
            }

            var editorInterface = EditorPlugin.GetEditorInterface();

            EditorPlugin.ImportProject(ldtkFilePath);

            editorInterface.CallDeferred("reload_scene_from_path", _CurrentNode.Filename);
        }
    }
}

#endif