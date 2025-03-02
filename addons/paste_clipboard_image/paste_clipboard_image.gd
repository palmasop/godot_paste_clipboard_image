@tool
extends EditorPlugin

var _file_dialog: EditorFileDialog
var _pending_image: Image

func _enter_tree() -> void:
    _file_dialog = EditorFileDialog.new()
    _file_dialog.file_mode = EditorFileDialog.FILE_MODE_SAVE_FILE
    _file_dialog.access = EditorFileDialog.ACCESS_RESOURCES
    _file_dialog.filters = ["*.png ; PNG Images"]
    _file_dialog.file_selected.connect(_on_file_dialog_file_selected)
    add_child(_file_dialog)

func _exit_tree() -> void:
    if _file_dialog:
        _file_dialog.queue_free()

func _on_file_dialog_file_selected(path: String) -> void:
    if not _pending_image:
        push_error("No pending image to save")
        return
    
    if not path.begins_with("res://"):
        push_error("Can only save to res:// directory")
        return
    
    var err: Error = _pending_image.save_png(path)
    if err == OK:
        print_rich("[color=#5f5]Image saved to: %s[/color]" % path)
        get_editor_interface().get_resource_filesystem().scan()
        DisplayServer.clipboard_set("")
    else:
        push_error("Failed to save image: %d" % err)
    _pending_image = null

func _paste_clipboard_image() -> bool:
    if not DisplayServer.clipboard_has_image():
        return false
    
    _pending_image = DisplayServer.clipboard_get_image()
    if not _pending_image or _pending_image.get_width() == 0 or _pending_image.get_height() == 0:
        _pending_image = null
        return false
    
    var default_name = "ClipboardImage_%s.png" % Time.get_datetime_string_from_system().replace(":", "").replace(" ", "_")
    _file_dialog.current_file = default_name
    _file_dialog.popup_centered(Vector2i(800, 600))
    return true

func _shortcut_input(event: InputEvent) -> void:
    if event is InputEventKey and event.keycode == KEY_V and event.ctrl_pressed and event.pressed:
        if _paste_clipboard_image():
            get_viewport().set_input_as_handled()
