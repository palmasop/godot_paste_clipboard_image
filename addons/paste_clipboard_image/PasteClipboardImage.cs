using Godot;
using System;

[Tool]
public partial class PasteClipboardImage : EditorPlugin
{
	EditorFileDialog _fileDialog;
	Image _pendingImage;

	public override void _EnterTree()
	{
		_fileDialog = new EditorFileDialog();
		_fileDialog.FileMode = EditorFileDialog.FileModeEnum.SaveFile;
		_fileDialog.Access = EditorFileDialog.AccessEnum.Resources;
		_fileDialog.Filters = new string[] { "*.png ; PNG Images" };
		_fileDialog.FileSelected += OnFileDialogFileSelected;
		AddChild(_fileDialog);
	}

	public override void _ExitTree() => _fileDialog?.QueueFree();

	void OnFileDialogFileSelected(string path)
	{
		if (_pendingImage == null)
		{
			GD.PushError("No pending image to save");
			return;
		}
		if (!path.StartsWith("res://"))
		{
			GD.PushError("Can only save to res:// directory");
			return;
		}

		Error err = _pendingImage.SavePng(path);
		if (err == Error.Ok)
		{
			GD.PrintRich($"[color=#5f5]Image saved to: {path}[/color]");
			EditorInterface.Singleton.GetResourceFilesystem().Scan();
		}
		else
			GD.PushError($"Failed to save image: {err}");
		_pendingImage = null;
	}

	public bool OnPasteClipboardImage()
	{
		if (!DisplayServer.ClipboardHasImage())
			return false;

		_pendingImage = DisplayServer.ClipboardGetImage();
		if (_pendingImage == null || _pendingImage.GetWidth() == 0 || _pendingImage.GetHeight() == 0)
		{
			_pendingImage = null;
			return false;
		}

		EditorInterface.Singleton.GetResourceFilesystem().Scan();

		string defaultName = $"ClipboardImage_{DateTime.Now:yyyyMMdd_HHmmss}.png";
		_fileDialog.CurrentFile = defaultName;
		_fileDialog.PopupCentered(new Vector2I(800, 600));
		return true;
	}

	public override void _ShortcutInput(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent &&
			keyEvent.Keycode == Key.V && keyEvent.CtrlPressed &&
			keyEvent.Pressed && OnPasteClipboardImage())
			GetViewport().SetInputAsHandled();
	}
}