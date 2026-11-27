extends Node3D

const FIELD_LENGTH := 36.0
const FIELD_WIDTH := 22.0
const BALL_RADIUS := 0.22

var xr_interface: XRInterface
var xr_origin: XROrigin3D
var left_hand: XRController3D
var right_hand: XRController3D
var ball: RigidBody3D
var score_label: Label3D
var score_home := 0
var score_away := 0
var previous_left := Vector3.ZERO
var previous_right := Vector3.ZERO

func _ready() -> void:
	_build_stadium()
	_build_vr_rig()
	_build_ball()
	_update_scoreboard()
	_start_openxr()

func _build_stadium() -> void:
	var environment := WorldEnvironment.new()
	var env := Environment.new()
	env.background_mode = Environment.BG_SKY
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color(0.55, 0.62, 0.72)
	env.ambient_light_energy = 0.7
	environment.environment = env
	add_child(environment)

	var sun := DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-48.0, -25.0, 0.0)
	sun.light_energy = 1.2
	add_child(sun)

	_add_box("Pitch", Vector3(0, -0.12, 0), Vector3(FIELD_WIDTH, 0.2, FIELD_LENGTH), Color(0.08, 0.38, 0.16))
	_add_box("TouchlineNorth", Vector3(0, 0.005, -FIELD_LENGTH / 2.0), Vector3(FIELD_WIDTH, 0.025, 0.08), Color.WHITE)
	_add_box("TouchlineSouth", Vector3(0, 0.005, FIELD_LENGTH / 2.0), Vector3(FIELD_WIDTH, 0.025, 0.08), Color.WHITE)
	_add_box("TouchlineWest", Vector3(-FIELD_WIDTH / 2.0, 0.005, 0), Vector3(0.08, 0.025, FIELD_LENGTH), Color.WHITE)
	_add_box("TouchlineEast", Vector3(FIELD_WIDTH / 2.0, 0.005, 0), Vector3(0.08, 0.025, FIELD_LENGTH), Color.WHITE)
	_add_box("HalfwayLine", Vector3(0, 0.01, 0), Vector3(FIELD_WIDTH, 0.025, 0.06), Color.WHITE)

	var centre := MeshInstance3D.new()
	var ring := TorusMesh.new()
	ring.inner_radius = 2.95
	ring.outer_radius = 3.02
	centre.mesh = ring
	centre.position.y = 0.025
	centre.material_override = _material(Color.WHITE)
	add_child(centre)

	_add_goal(-FIELD_LENGTH / 2.0 + 0.5)
	_add_goal(FIELD_LENGTH / 2.0 - 0.5)

func _add_goal(z_position: float) -> void:
	var goal := Node3D.new()
	goal.position = Vector3(0, 0, z_position)
	add_child(goal)
	var depth := -1.6 if z_position < 0 else 1.6
	_add_goal_post(goal, Vector3(-3.2, 1.15, 0))
	_add_goal_post(goal, Vector3(3.2, 1.15, 0))
	_add_goal_post(goal, Vector3(0, 2.3, 0), Vector3(6.4, 0.12, 0.12))
	_add_goal_post(goal, Vector3(-3.2, 1.15, depth), Vector3(0.12, 2.3, 0.12))
	_add_goal_post(goal, Vector3(3.2, 1.15, depth), Vector3(0.12, 2.3, 0.12))
	_add_goal_post(goal, Vector3(0, 2.3, depth), Vector3(6.4, 0.12, 0.12))

func _add_goal_post(parent: Node3D, local_position: Vector3, size := Vector3(0.12, 2.3, 0.12)) -> void:
	var post := MeshInstance3D.new()
	var mesh := BoxMesh.new()
	mesh.size = size
	post.mesh = mesh
	post.position = local_position
	post.material_override = _material(Color.WHITE)
	parent.add_child(post)

func _build_vr_rig() -> void:
	xr_origin = XROrigin3D.new()
	xr_origin.position = Vector3(0, 0, 8)
	add_child(xr_origin)

	var camera := XRCamera3D.new()
	camera.position = Vector3(0, 1.65, 0)
	xr_origin.add_child(camera)

	left_hand = XRController3D.new()
	left_hand.tracker = &"left_hand"
	left_hand.pose = &"default"
	xr_origin.add_child(left_hand)
	_add_controller_visual(left_hand)

	right_hand = XRController3D.new()
	right_hand.tracker = &"right_hand"
	right_hand.pose = &"default"
	xr_origin.add_child(right_hand)
	_add_controller_visual(right_hand)

func _add_controller_visual(controller: XRController3D) -> void:
	var hand := MeshInstance3D.new()
	var mesh := SphereMesh.new()
	mesh.radius = 0.09
	mesh.height = 0.18
	hand.mesh = mesh
	hand.material_override = _material(Color(0.95, 0.95, 0.95))
	controller.add_child(hand)

func _build_ball() -> void:
	ball = RigidBody3D.new()
	ball.name = "Football"
	ball.position = Vector3(0, BALL_RADIUS + 0.08, 0)
	ball.mass = 0.43
	ball.linear_damp = 0.18
	ball.angular_damp = 0.12
	ball.continuous_cd = true
	var sphere := SphereMesh.new()
	sphere.radius = BALL_RADIUS
	sphere.height = BALL_RADIUS * 2.0
	var visual := MeshInstance3D.new()
	visual.mesh = sphere
	visual.material_override = _material(Color.WHITE)
	ball.add_child(visual)
	var collision := CollisionShape3D.new()
	var shape := SphereShape3D.new()
	shape.radius = BALL_RADIUS
	collision.shape = shape
	ball.add_child(collision)
	add_child(ball)

func _start_openxr() -> void:
	xr_interface = XRServer.find_interface("OpenXR")
	if xr_interface and xr_interface.is_initialized():
		get_viewport().use_xr = true
	elif xr_interface:
		if xr_interface.initialize():
			get_viewport().use_xr = true
		else:
			push_warning("OpenXR did not start. Run this build on a supported Quest headset.")
	else:
		push_warning("OpenXR interface is unavailable. This scene can still be previewed without a headset.")

func _physics_process(_delta: float) -> void:
	if not is_instance_valid(ball):
		return
	_try_hand_touch(left_hand, previous_left)
	_try_hand_touch(right_hand, previous_right)
	if left_hand and left_hand.get_is_active():
		previous_left = left_hand.global_position
	if right_hand and right_hand.get_is_active():
		previous_right = right_hand.global_position

func _try_hand_touch(hand: XRController3D, previous_position: Vector3) -> void:
	if not hand or not hand.get_is_active():
		return
	var distance := hand.global_position.distance_to(ball.global_position)
	if distance < 0.42:
		var movement := hand.global_position - previous_position
		if movement.length() > 0.012:
			ball.apply_central_impulse(movement * 2.8)

func _update_scoreboard() -> void:
	if not score_label:
		score_label = Label3D.new()
		score_label.name = "Scoreboard"
		score_label.position = Vector3(0, 4.2, -FIELD_LENGTH / 2.0 + 2.0)
		score_label.font_size = 72
		score_label.modulate = Color.WHITE
		add_child(score_label)
	score_label.text = "HALFTIME VR   %d - %d" % [score_home, score_away]

func _add_box(node_name: String, position_3d: Vector3, size: Vector3, colour: Color) -> void:
	var body := StaticBody3D.new()
	body.name = node_name
	body.position = position_3d
	var mesh := BoxMesh.new()
	mesh.size = size
	var visual := MeshInstance3D.new()
	visual.mesh = mesh
	visual.material_override = _material(colour)
	body.add_child(visual)
	var collision := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = size
	collision.shape = shape
	body.add_child(collision)
	add_child(body)

func _material(colour: Color) -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	material.albedo_color = colour
	return material
