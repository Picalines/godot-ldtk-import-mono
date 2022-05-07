#if TOOLS

using Godot;

#pragma warning disable CS8618

namespace Picalines.Godot.LDtkImport.UI
{
    [Tool]
    internal sealed class FileSelectButton : Button
    {
        [Signal] private delegate void file_selected(string path);

        [Signal] private delegate void file_cleared();

        [Export] public readonly string FileExtension;

        private FileDialog _FileDialog;

        private string? _SelectedFile = null;

        public override void _Ready()
        {
            _FileDialog = GetNode<FileDialog>("./FileDialog");

            Connect("toggled", this, nameof(OnToggled));
            _FileDialog.Connect("file_selected", this, nameof(OnFileSelected));

            _FileDialog.Filters = new string[] { $"*.{FileExtension}" };

            Text = GetTextWhenNotSelected();
        }

        public string? SelectedFile
        {
            get => _SelectedFile;
            set
            {
                _SelectedFile = string.IsNullOrWhiteSpace(value) ? null : value;

                if (_SelectedFile is not null)
                {
                    OnFileSelected(_SelectedFile);
                }
                else
                {
                    OnFileCleared();
                }
            }
        }

        private void OnToggled(bool pressed)
        {
            if (pressed)
            {
                _FileDialog.PopupCentered();
            }
            else
            {
                OnFileCleared();
            }
        }

        private void OnFileSelected(string path)
        {
            _SelectedFile = path;

            Text = GetTextWhenSelected();

            SetPressedNoSignal(true);

            EmitSignal(nameof(file_selected), path);
        }

        private void OnFileCleared()
        {
            _SelectedFile = null;

            Text = GetTextWhenNotSelected();

            SetPressedNoSignal(false);

            EmitSignal(nameof(file_cleared));
        }

        private string GetTextWhenNotSelected() => $"select .{FileExtension} file...";

        private string GetTextWhenSelected() => $"selected: {_SelectedFile}";
    }
}

#endif
