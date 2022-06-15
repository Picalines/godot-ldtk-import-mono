#if TOOLS

using Godot;

namespace Picalines.Godot.LDtkImport
{
    [Tool]
    internal sealed class LDtkInspectorPlugin : EditorInspectorPlugin
    {
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

            return node.IsInGroup(LDtkConstants.GroupNames.Worlds);
        }

        public override void ParseBegin(Object @object)
        {
            base.ParseBegin(@object);

            _CurrentNode = @object as Node;

            var reloadButton = new Button()
            {
                Text = "Reimport LDtk world",
                Theme = EditorPlugin.CurrentEditorTheme,
            };

            reloadButton.Connect("pressed", this, nameof(OnReloadButtonPressed));

            AddCustomControl(reloadButton);
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