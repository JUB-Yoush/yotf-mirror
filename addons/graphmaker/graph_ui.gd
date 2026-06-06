@tool
extends Control

@onready var new_btn: Button = $"%NewBtn"
@onready var bisect_btn: Button = $"%BisectBtn"

var selection: EditorSelection
var nav_node_packed: PackedScene = preload("res://src/entities/cave/nav_node.tscn")

func _ready() -> void:
    selection = EditorInterface.get_selection()
    new_btn.pressed.connect(new_pressed)
    bisect_btn.pressed.connect(bisect_pressed)
    pass

func new_pressed() -> void:
    pass

func bisect_pressed() -> void:
    var selected_nodes: Array[Node] = EditorInterface.get_selection().get_selected_nodes()

    for node in selected_nodes:
        if !node.has_method("AddNeighbor"):
            push_error("Non NavNode Selected for bisection")
            return

    if len(selected_nodes) != 2:
            push_error("more or less than 2 nodes selected")
            return
        
    var nei_first: Array = selected_nodes[0].get("neighbors")
    var nei_second: Array = selected_nodes[0].get("neighbors")

    var new_node = nav_node_packed.instantiate()
    get_tree().root.add_child(new_node)
    #selected_nodes[0].add_child(new_node)
    # disconnect from each other
    # make new node, add to tree
    # add new node to both lists
    # place new node in between 
    
    print(selected_nodes[1].get("neighbors"))
