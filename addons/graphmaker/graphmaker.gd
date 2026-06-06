@tool
extends EditorPlugin

var dock: EditorDock

func print_highlighted() -> void:
	var seleciton = EditorSelection.new()
	for node in seleciton.get_selected_nodes():
		print(node)

func _enable_plugin() -> void:
	# Add autoloads here.
	pass


func _disable_plugin() -> void:
	# Remove autoloads here.
	pass


func _enter_tree() -> void:
	# Initialization of the plugin goes here.
	# Load the dock scene and instantiate it.
	var dock_scene = preload("res://addons/graphmaker/graph_ui.tscn").instantiate()

	# Create the dock and add the loaded scene to it.
	dock = EditorDock.new()
	dock.add_child(dock_scene)

	dock.title = "GraphMaker"

	# Note that LEFT_UL means the left of the editor, upper-left dock.
	dock.default_slot = EditorDock.DOCK_SLOT_RIGHT_BR

	# Allow the dock to be on the left or right of the editor, and to be made floating.
	dock.available_layouts = EditorDock.DOCK_LAYOUT_VERTICAL | EditorDock.DOCK_LAYOUT_FLOATING

	add_dock(dock)

func _exit_tree() -> void:
	remove_dock(dock)
	dock.queue_free()
