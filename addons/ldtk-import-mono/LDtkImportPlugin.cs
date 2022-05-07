#if TOOLS

using Godot;

namespace Picalines.Godot.LDtkImport;

[Tool]
internal sealed class LDtkImportPlugin : EditorPlugin
{
	private const string ImportPanelScenePath = "res://addons/ldtk-import-mono/ui/ImportPanel.tscn";

	private Control? _ImportPanel;

	public override void _EnterTree()
	{
		base._EnterTree();

		_ImportPanel = GD.Load<PackedScene>(ImportPanelScenePath).Instance<Control>();

		AddControlToBottomPanel(_ImportPanel, "LDtk import");
	}

	public override void _ExitTree()
	{
		base._ExitTree();

		if (_ImportPanel is not null)
		{
			RemoveControlFromBottomPanel(_ImportPanel);

			_ImportPanel.QueueFree();

			_ImportPanel = null;
		}

	}
}

#endif
