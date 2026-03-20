extends CharacterBody3D

@export var MOVE_SPEED: float = 10.0
@export var FLY_SPEED: float = 2.0
@export var JUMP_SPEED: float = 10.0
@export var WEIGHT: float = 2.0
@export var ROTATION_SPEED: float = 10.0

@export var first_person: bool = false : 
	set(p_value):
		first_person = p_value
		if first_person:
			var tween: Tween = create_tween()
			tween.tween_property($CameraManager/Arm, "spring_length", 0.0, .33)
			tween.tween_callback($Body.set_visible.bind(false))
		else:
			$Body.visible = true
			create_tween().tween_property($CameraManager/Arm, "spring_length", 6.0, .33)

@export var gravity_enabled: bool = true :
	set(p_value):
		gravity_enabled = p_value
		if not gravity_enabled:
			velocity.y = 0
			
@export var collision_enabled: bool = true :
	set(p_value):
		collision_enabled = p_value
		$CollisionShapeBody.disabled = ! collision_enabled
		$CollisionShapeRay.disabled = ! collision_enabled

@onready var _skin: Node3D = $SophiaSkin

var gravity = ProjectSettings.get_setting("physics/3d/default_gravity")

func _physics_process(p_delta: float) -> void:
	var direction: Vector3 = get_camera_relative_input()
	var h_veloc: Vector2 = Vector2(direction.x, direction.z).normalized() * MOVE_SPEED
	
	if direction != Vector3.ZERO:
		var target_angle: float = atan2(direction.x, direction.z)
		_skin.rotation.y = lerp_angle(_skin.rotation.y, target_angle, ROTATION_SPEED * p_delta)
	
	if Input.is_action_pressed("jump") and is_on_floor():
		velocity.y = JUMP_SPEED
		
	if Input.is_action_just_pressed("quit"):
		get_tree().quit()
		
	if Input.is_key_pressed(KEY_SHIFT):
		h_veloc *= 2
		
	velocity.x = h_veloc.x
	velocity.z = h_veloc.y
	if gravity_enabled and not is_on_floor():
		velocity.y -= gravity * WEIGHT * p_delta
		
	move_and_slide()


# Returns the input vector relative to the camera. Forward is always the direction the camera is facing
func get_camera_relative_input() -> Vector3:
	var input_dir: Vector3 = Vector3.ZERO
	if Input.is_action_pressed("left"): # Left
		input_dir -= %Camera3D.global_transform.basis.x
	if Input.is_action_pressed("right"): # Right
		input_dir += %Camera3D.global_transform.basis.x
	if Input.is_action_pressed("up"): # Forward
		input_dir -= %Camera3D.global_transform.basis.z
	if Input.is_action_pressed("down"): # Backward
		input_dir += %Camera3D.global_transform.basis.z
	if Input.is_key_pressed(KEY_E): # Up
		velocity.y += FLY_SPEED + MOVE_SPEED*.016
	if Input.is_key_pressed(KEY_Q): # Down
		velocity.y -= FLY_SPEED + MOVE_SPEED*.016
	if Input.is_key_pressed(KEY_KP_ADD) or Input.is_key_pressed(KEY_EQUAL):
		MOVE_SPEED = clamp(MOVE_SPEED + .5, 5, 9999)
	if Input.is_key_pressed(KEY_KP_SUBTRACT) or Input.is_key_pressed(KEY_MINUS):
		MOVE_SPEED = clamp(MOVE_SPEED - .5, 5, 9999)
	return input_dir


func _input(p_event: InputEvent) -> void:
	if p_event is InputEventMouseButton and p_event.pressed:
		if p_event.button_index == MOUSE_BUTTON_WHEEL_UP:
			MOVE_SPEED = clamp(MOVE_SPEED + 5, 5, 9999)
		elif p_event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
			MOVE_SPEED = clamp(MOVE_SPEED - 5, 5, 9999)
		
	elif p_event is InputEventKey:
		if p_event.pressed:
			if p_event.keycode == KEY_V:
				first_person = ! first_person
			elif p_event.keycode == KEY_G:
				gravity_enabled = ! gravity_enabled
			elif p_event.keycode == KEY_C:
				collision_enabled = ! collision_enabled

		# Else if up/down released
		elif p_event.keycode in [ KEY_Q, KEY_E ]:
			velocity.y = 0
