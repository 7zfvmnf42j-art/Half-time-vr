extends Node3D

const FIELD_LENGTH := 105.0
const FIELD_WIDTH := 68.0
const GOAL_WIDTH := 7.32
const GOAL_HEIGHT := 2.44
const GOAL_LINE_Z := -FIELD_LENGTH / 2.0
const BALL_RADIUS := 0.22
const MATCH_LENGTH_SECONDS := 300.0

var xr_interface: XRInterface
var xr_origin: XROrigin3D
var left_hand: XRController3D
var right_hand: XRController3D
var ball: RigidBody3D
var score_label: Label3D
var clock_label: Label3D
var score_home := 0
var score_away := 0
var seconds_left := MATCH_LENGTH_SECONDS
var match_running := false
var previous_left := Vector3.ZERO
var previous_right := Vector3.ZERO
var previous_left_valid := false
var previous_right_valid := false
var menu_layer: CanvasLayer
var menu_panel: PanelContainer
var status_label: Label
var league_hub: CanvasLayer
var goalkeeper: CharacterBody3D
var practice_label: Label3D
var practice_shots := 0
var practice_goals := 0
var practice_saves := 0
var shot_active := false
var shot_elapsed := 0.0
var practice_mode := true

func _ready() -> void:
	_build_stadium()
	_build_vr_rig()
	_build_ball()
	_build_scoreboard()
	_build_menu()
	league_hub = preload("res://scripts/league_hub.gd").new()
	add_child(league_hub)
	league_hub.close_requested.connect(_close_competitions)
	_start_openxr()
	_start_practice()

func _process(delta: float) -> void:
	if match_running and not practice_mode:
		seconds_left = maxf(0.0, seconds_left - delta)
		_update_scoreboard()
		if seconds_left <= 0.0:
			match_running = false
			_show_menu("FULL TIME")
			status_label.text = "FULL TIME  |  Final score: %d - %d" % [score_home, score_away]

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
	_add_box("PenaltyFront", Vector3(0, 0.02, GOAL_LINE_Z + 16.5), Vector3(40.32, 0.03, 0.06), Color.WHITE)
	_add_box("PenaltyLeft", Vector3(-20.16, 0.02, GOAL_LINE_Z + 8.25), Vector3(0.06, 0.03, 16.5), Color.WHITE)
	_add_box("PenaltyRight", Vector3(20.16, 0.02, GOAL_LINE_Z + 8.25), Vector3(0.06, 0.03, 16.5), Color.WHITE)
	_add_box("SixYardFront", Vector3(0, 0.02, GOAL_LINE_Z + 5.5), Vector3(18.32, 0.03, 0.06), Color.WHITE)
	_add_box("SixYardLeft", Vector3(-9.16, 0.02, GOAL_LINE_Z + 2.75), Vector3(0.06, 0.03, 5.5), Color.WHITE)
	_add_box("SixYardRight", Vector3(9.16, 0.02, GOAL_LINE_Z + 2.75), Vector3(0.06, 0.03, 5.5), Color.WHITE)
	_add_box("PenaltySpot", Vector3(0, 0.025, GOAL_LINE_Z + 11.0), Vector3(0.2, 0.035, 0.2), Color.WHITE)
	_add_box("CentreSpot", Vector3(0, 0.025, 0), Vector3(0.2, 0.035, 0.2), Color.WHITE)

	var centre := MeshInstance3D.new()
	var ring := TorusMesh.new()
	ring.inner_radius = 9.10
	ring.outer_radius = 9.16
	centre.mesh = ring
	centre.position.y = 0.025
	centre.material_override = _material(Color.WHITE)
	add_child(centre)

	_add_goal(GOAL_LINE_Z + 0.15)
	_add_goal(FIELD_LENGTH / 2.0 - 0.15)
	_add_goal_sensor(GOAL_LINE_Z - 0.45, false)
	_add_goal_sensor(FIELD_LENGTH / 2.0 + 0.45, true)
	_build_goalkeeper()
	_build_practice_display()

func _add_goal(z_position: float) -> void:
	var goal := Node3D.new()
	goal.position = Vector3(0, 0, z_position)
	add_child(goal)
	var depth := -2.0 if z_position < 0 else 2.0
	_add_goal_post(goal, Vector3(-GOAL_WIDTH / 2.0, GOAL_HEIGHT / 2.0, 0), Vector3(0.12, GOAL_HEIGHT, 0.12))
	_add_goal_post(goal, Vector3(GOAL_WIDTH / 2.0, GOAL_HEIGHT / 2.0, 0), Vector3(0.12, GOAL_HEIGHT, 0.12))
	_add_goal_post(goal, Vector3(0, GOAL_HEIGHT, 0), Vector3(GOAL_WIDTH + 0.12, 0.12, 0.12))
	_add_goal_post(goal, Vector3(-GOAL_WIDTH / 2.0, GOAL_HEIGHT / 2.0, depth), Vector3(0.12, GOAL_HEIGHT, 0.12))
	_add_goal_post(goal, Vector3(GOAL_WIDTH / 2.0, GOAL_HEIGHT / 2.0, depth), Vector3(0.12, GOAL_HEIGHT, 0.12))
	_add_goal_post(goal, Vector3(0, GOAL_HEIGHT, depth), Vector3(GOAL_WIDTH + 0.12, 0.12, 0.12))
	_add_goal_post(goal, Vector3(0, GOAL_HEIGHT / 2.0, depth), Vector3(GOAL_WIDTH, 0.035, 0.035))
	_add_goal_post(goal, Vector3(-GOAL_WIDTH / 2.0, GOAL_HEIGHT / 2.0, depth / 2.0), Vector3(0.035, GOAL_HEIGHT, absf(depth)))
	_add_goal_post(goal, Vector3(GOAL_WIDTH / 2.0, GOAL_HEIGHT / 2.0, depth / 2.0), Vector3(0.035, GOAL_HEIGHT, absf(depth)))

func _add_goal_sensor(z_position: float, home_scores: bool) -> void:
	var sensor := Area3D.new()
	sensor.name = "HomeGoalSensor" if home_scores else "AwayGoalSensor"
	sensor.position = Vector3(0, 1.15, z_position)
	var collision := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = Vector3(GOAL_WIDTH - 0.2, GOAL_HEIGHT - 0.08, 0.8)
	collision.shape = shape
	sensor.add_child(collision)
	sensor.body_entered.connect(_on_goal_body_entered.bind(home_scores))
	add_child(sensor)

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
	xr_origin.position = Vector3(0, 0, -22.0)
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
	ball.position = Vector3(0, BALL_RADIUS + 0.03, -22.9)
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

func _build_scoreboard() -> void:
	score_label = Label3D.new()
	score_label.name = "Scoreboard"
	score_label.position = Vector3(0, 4.2, -FIELD_LENGTH / 2.0 + 2.0)
	score_label.font_size = 72
	score_label.modulate = Color.WHITE
	add_child(score_label)

	clock_label = Label3D.new()
	clock_label.name = "MatchClock"
	clock_label.position = Vector3(0, 3.35, -FIELD_LENGTH / 2.0 + 2.0)
	clock_label.font_size = 42
	clock_label.modulate = Color(1.0, 0.85, 0.25)
	add_child(clock_label)
	_update_scoreboard()

func _build_menu() -> void:
	menu_layer = CanvasLayer.new()
	menu_layer.layer = 5
	add_child(menu_layer)

	var background := ColorRect.new()
	background.color = Color(0.015, 0.035, 0.06, 0.88)
	background.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	menu_layer.add_child(background)

	var centre := CenterContainer.new()
	centre.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	menu_layer.add_child(centre)

	menu_panel = PanelContainer.new()
	menu_panel.custom_minimum_size = Vector2(560, 0)
	centre.add_child(menu_panel)

	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.035, 0.09, 0.13, 0.98)
	style.border_color = Color(0.2, 0.78, 0.48)
	style.set_border_width_all(3)
	style.set_corner_radius_all(18)
	style.content_margin_left = 34
	style.content_margin_right = 34
	style.content_margin_top = 30
	style.content_margin_bottom = 30
	menu_panel.add_theme_stylebox_override("panel", style)

	var layout := VBoxContainer.new()
	layout.add_theme_constant_override("separation", 18)
	menu_panel.add_child(layout)

	var title := Label.new()
	title.text = "HALFTIME VR"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 42)
	title.add_theme_color_override("font_color", Color(0.3, 1.0, 0.62))
	layout.add_child(title)

	var subtitle := Label.new()
	subtitle.text = "STEP ONTO THE PITCH"
	subtitle.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	subtitle.add_theme_font_size_override("font_size", 18)
	layout.add_child(subtitle)

	status_label = Label.new()
	status_label.text = "Ready for kick-off"
	status_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	layout.add_child(status_label)

	var start_button := Button.new()
	start_button.name = "KickOffButton"
	start_button.text = "KICK OFF"
	start_button.custom_minimum_size = Vector2(0, 58)
	start_button.add_theme_font_size_override("font_size", 24)
	start_button.pressed.connect(_start_match)
	layout.add_child(start_button)

	var locker_button := Button.new()
	locker_button.text = "LOCKER ROOM  —  COMING SOON"
	locker_button.custom_minimum_size = Vector2(0, 48)
	locker_button.disabled = true
	layout.add_child(locker_button)

	var competitions_button := Button.new()
	competitions_button.text = "COMPETITIONS  /  LEAGUES"
	competitions_button.custom_minimum_size = Vector2(0, 52)
	competitions_button.pressed.connect(_open_competitions)
	layout.add_child(competitions_button)

	var note := Label.new()
	note.text = "Early prototype  •  Local practice match"
	note.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	note.add_theme_font_size_override("font_size", 14)
	note.modulate = Color(0.72, 0.78, 0.82)
	layout.add_child(note)


func _open_competitions() -> void:
	menu_layer.visible = false
	league_hub.visible = true

func _close_competitions() -> void:
	menu_layer.visible = true

func _start_match() -> void:
	score_home = 0
	score_away = 0
	seconds_left = MATCH_LENGTH_SECONDS
	match_running = true
	_reset_ball()
	_update_scoreboard()
	menu_layer.visible = false

func _show_menu(message: String) -> void:
	menu_layer.visible = true
	status_label.text = message
	var start_button := menu_panel.find_child("KickOffButton", true, false)
	if start_button is Button:
		start_button.text = "PLAY AGAIN"

func _on_goal_body_entered(body: Node3D, home_scores: bool) -> void:
	if body != ball or not match_running:
		return
	if not home_scores:
		practice_goals += 1
	_update_practice_display()
	shot_active = false
	_reset_ball()

func _reset_ball() -> void:
	ball.freeze = true
	ball.global_position = Vector3(0, BALL_RADIUS + 0.03, -22.9)
	ball.linear_velocity = Vector3.ZERO
	ball.angular_velocity = Vector3.ZERO
	ball.freeze = false

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

func _physics_process(delta: float) -> void:
	if not is_instance_valid(ball):
		return
	_update_goalkeeper(delta)
	if shot_active:
		shot_elapsed += delta
		if is_instance_valid(goalkeeper) and ball.global_position.distance_to(goalkeeper.global_position + Vector3(0, 0.25, 0)) < 1.25 and ball.global_position.y < 2.4:
			practice_saves += 1
			shot_active = false
			_update_practice_display()
			_reset_ball()
		elif ball.global_position.z < GOAL_LINE_Z - 4.0 or shot_elapsed > 7.0:
			shot_active = false
			_reset_ball()
	if left_hand and left_hand.get_is_active():
		var current_left := left_hand.global_position
		if previous_left_valid:
			_try_hand_touch(left_hand, previous_left)
		previous_left = current_left
		previous_left_valid = true
	if right_hand and right_hand.get_is_active():
		var current_right := right_hand.global_position
		if previous_right_valid:
			_try_hand_touch(right_hand, previous_right)
		previous_right = current_right
		previous_right_valid = true

func _try_hand_touch(hand: XRController3D, previous_position: Vector3) -> void:
	if not hand or not hand.get_is_active():
		return
	# Controller positions are mapped to low virtual boot positions for shooting practice.
	var virtual_boot_position := hand.global_position + Vector3(0, -0.72, -0.12)
	var virtual_previous_boot := previous_position + Vector3(0, -0.72, -0.12)
	if virtual_boot_position.distance_to(ball.global_position) < 0.62:
		var movement := virtual_boot_position - virtual_previous_boot
		if movement.length() > 0.012:
			if not shot_active:
				practice_shots += 1
				shot_active = true
				shot_elapsed = 0.0
				_update_practice_display()
			ball.apply_central_impulse(movement * 4.2)

func _start_practice() -> void:
	practice_mode = true
	match_running = true
	score_home = 0
	score_away = 0
	practice_shots = 0
	practice_goals = 0
	practice_saves = 0
	shot_active = false
	_reset_ball()
	_update_scoreboard()
	_update_practice_display()
	menu_layer.visible = false

func _build_goalkeeper() -> void:
	goalkeeper = CharacterBody3D.new()
	goalkeeper.name = "PracticeAI_Goalkeeper"
	goalkeeper.position = Vector3(0, 0.95, GOAL_LINE_Z + 1.0)
	var body_shape := CollisionShape3D.new()
	var capsule := CapsuleShape3D.new()
	capsule.radius = 0.48
	capsule.height = 1.9
	body_shape.shape = capsule
	goalkeeper.add_child(body_shape)
	var body_visual := MeshInstance3D.new()
	var body_mesh := CapsuleMesh.new()
	body_mesh.radius = 0.48
	body_mesh.height = 1.9
	body_visual.mesh = body_mesh
	body_visual.material_override = _material(Color(0.95, 0.55, 0.08))
	goalkeeper.add_child(body_visual)
	var head := MeshInstance3D.new()
	var head_mesh := SphereMesh.new()
	head_mesh.radius = 0.22
	head_mesh.height = 0.44
	head.mesh = head_mesh
	head.position = Vector3(0, 1.05, 0)
	head.material_override = _material(Color(0.88, 0.68, 0.52))
	goalkeeper.add_child(head)
	for side in [-1.0, 1.0]:
		var arm := MeshInstance3D.new()
		var arm_mesh := BoxMesh.new()
		arm_mesh.size = Vector3(0.85, 0.22, 0.24)
		arm.mesh = arm_mesh
		arm.position = Vector3(side * 0.72, 0.28, 0)
		arm.rotation.z = side * -0.12
		arm.material_override = _material(Color(0.95, 0.55, 0.08))
		goalkeeper.add_child(arm)
	add_child(goalkeeper)

func _update_goalkeeper(_delta: float) -> void:
	if not is_instance_valid(goalkeeper) or not is_instance_valid(ball):
		return
	var keeper_z := GOAL_LINE_Z + 1.0
	var target_x := 0.0
	var ball_is_threat := ball.global_position.z < GOAL_LINE_Z + 24.0 and ball.linear_velocity.z < -0.5
	if ball_is_threat:
		var travel_time := clampf((keeper_z - ball.global_position.z) / minf(ball.linear_velocity.z, -0.5), 0.0, 1.2)
		target_x = ball.global_position.x + ball.linear_velocity.x * travel_time
	target_x = clampf(target_x, -GOAL_WIDTH / 2.0 + 0.65, GOAL_WIDTH / 2.0 - 0.65)
	goalkeeper.velocity.x = clampf((target_x - goalkeeper.global_position.x) * 4.0, -5.0, 5.0)
	goalkeeper.velocity.y = 0.0
	goalkeeper.velocity.z = 0.0
	goalkeeper.move_and_slide()

func _build_practice_display() -> void:
	practice_label = Label3D.new()
	practice_label.name = "ShootingPracticeStats"
	practice_label.position = Vector3(0, 4.0, GOAL_LINE_Z + 8.0)
	practice_label.font_size = 42
	practice_label.modulate = Color(0.85, 1.0, 0.9)
	practice_label.text = "SHOTS  0     GOALS  0     SAVES  0"
	add_child(practice_label)

func _update_practice_display() -> void:
	if is_instance_valid(practice_label):
		practice_label.text = "SHOTS  %d     GOALS  %d     SAVES  %d" % [practice_shots, practice_goals, practice_saves]

func _update_scoreboard() -> void:
	if score_label:
		score_label.text = "HOME  %d  -  %d  AWAY" % [score_home, score_away]
	if clock_label:
		var total_seconds := int(ceil(seconds_left))
		clock_label.text = "%02d:%02d" % [total_seconds / 60, total_seconds % 60]

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
