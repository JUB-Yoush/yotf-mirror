extends Camera3D

@export var LERP_POWER: float = 5.0

@onready var _arm_position: Node3D = $"../Arm/ArmPosition"
@onready var _arm: Node3D = $"../Arm"

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	position = lerp(position, _arm_position.position, delta * LERP_POWER)
