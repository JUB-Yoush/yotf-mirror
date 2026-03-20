extends Node3D

const CAMERA_MAX_PITCH: float = deg_to_rad(70)
const CAMERA_MIN_PITCH: float = deg_to_rad(-89.9)
const CAMERA_RATIO: float = .625

@export var mouse_sensitivity: float = .002
@export var mouse_y_inversion: float = -1.0

func _ready() -> void:
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)


func _input(p_event: InputEvent) -> void:
	if p_event is InputEventMouseMotion and Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
		rotate_camera(p_event.relative)
		get_viewport().set_input_as_handled()
		return


func rotate_camera(p_relative:Vector2) -> void:
	rotation.y -= p_relative.x * mouse_sensitivity
	orthonormalize()
	rotation.x += p_relative.y * mouse_sensitivity * CAMERA_RATIO * mouse_y_inversion 
	rotation.x = clamp(rotation.x, CAMERA_MIN_PITCH, CAMERA_MAX_PITCH)
