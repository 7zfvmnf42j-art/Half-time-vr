extends CanvasLayer

signal close_requested

const SAVE_PATH := "user://halftime_leagues.cfg"

var leagues: Array[Dictionary] = []
var selected_league := -1
var status_label: Label
var league_name_input: LineEdit
var follow_input: LineEdit
var team_name_input: LineEdit
var application_league_input: LineEdit
var league_list: VBoxContainer
var application_list: VBoxContainer

func _ready() -> void:
	layer = 8
	_load_data()
	_build_ui()
	visible = false

func _build_ui() -> void:
	var background := ColorRect.new()
	background.color = Color(0.012, 0.025, 0.045, 0.97)
	background.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(background)

	var margin := MarginContainer.new()
	margin.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	margin.add_theme_constant_override("margin_left", 36)
	margin.add_theme_constant_override("margin_right", 36)
	margin.add_theme_constant_override("margin_top", 24)
	margin.add_theme_constant_override("margin_bottom", 24)
	background.add_child(margin)

	var layout := VBoxContainer.new()
	layout.add_theme_constant_override("separation", 12)
	margin.add_child(layout)

	var header := HBoxContainer.new()
	layout.add_child(header)
	var title := Label.new()
	title.text = "HALFTIME VR  /  COMPETITIONS"
	title.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	title.add_theme_font_size_override("font_size", 30)
	title.add_theme_color_override("font_color", Color(0.3, 1.0, 0.62))
	header.add_child(title)
	var back := Button.new()
	back.text = "BACK"
	back.pressed.connect(func(): visible = false; close_requested.emit())
	header.add_child(back)

	var subtitle := Label.new()
	subtitle.text = "Create competitions, build a following and apply to join leagues."
	subtitle.add_theme_font_size_override("font_size", 16)
	layout.add_child(subtitle)

	var warning := Label.new()
	warning.text = "LOCAL PROTOTYPE  •  Online accounts, cross-device league data and live spectating are not connected yet."
	warning.add_theme_color_override("font_color", Color(1.0, 0.78, 0.32))
	warning.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	layout.add_child(warning)

	var columns := HBoxContainer.new()
	columns.size_flags_vertical = Control.SIZE_EXPAND_FILL
	columns.add_theme_constant_override("separation", 18)
	layout.add_child(columns)

	var left := VBoxContainer.new()
	left.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	left.size_flags_vertical = Control.SIZE_EXPAND_FILL
	columns.add_child(left)

	var create_title := Label.new()
	create_title.text = "CREATE A LEAGUE"
	create_title.add_theme_font_size_override("font_size", 20)
	left.add_child(create_title)
	league_name_input = LineEdit.new()
	league_name_input.placeholder_text = "League name"
	left.add_child(league_name_input)
	var create_button := Button.new()
	create_button.text = "CREATE LEAGUE"
	create_button.pressed.connect(_create_league)
	left.add_child(create_button)

	var follow_title := Label.new()
	follow_title.text = "FOLLOW A LEAGUE"
	follow_title.add_theme_font_size_override("font_size", 20)
	left.add_child(follow_title)
	follow_input = LineEdit.new()
	follow_input.placeholder_text = "Enter exact league name"
	left.add_child(follow_input)
	var follow_button := Button.new()
	follow_button.text = "FOLLOW"
	follow_button.pressed.connect(_follow_league)
	left.add_child(follow_button)

	var apply_title := Label.new()
	apply_title.text = "APPLY AS A TEAM"
	apply_title.add_theme_font_size_override("font_size", 20)
	left.add_child(apply_title)
	application_league_input = LineEdit.new()
	application_league_input.placeholder_text = "League name"
	left.add_child(application_league_input)
	team_name_input = LineEdit.new()
	team_name_input.placeholder_text = "Your team name"
	left.add_child(team_name_input)
	var apply_button := Button.new()
	apply_button.text = "SEND TEAM APPLICATION"
	apply_button.pressed.connect(_apply_as_team)
	left.add_child(apply_button)

	var right := VBoxContainer.new()
	right.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	right.size_flags_vertical = Control.SIZE_EXPAND_FILL
	columns.add_child(right)

	var leagues_title := Label.new()
	leagues_title.text = "LEAGUES"
	leagues_title.add_theme_font_size_override("font_size", 20)
	right.add_child(leagues_title)
	var league_scroll := ScrollContainer.new()
	league_scroll.size_flags_vertical = Control.SIZE_EXPAND_FILL
	right.add_child(league_scroll)
	league_list = VBoxContainer.new()
	league_list.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	league_scroll.add_child(league_list)

	var applications_title := Label.new()
	applications_title.text = "TEAM APPLICATIONS  /  YOUR LEAGUES"
	applications_title.add_theme_font_size_override("font_size", 18)
	right.add_child(applications_title)
	var application_scroll := ScrollContainer.new()
	application_scroll.custom_minimum_size.y = 120
	right.add_child(application_scroll)
	application_list = VBoxContainer.new()
	application_list.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	application_scroll.add_child(application_list)

	status_label = Label.new()
	status_label.text = "Create a league or follow an existing one."
	status_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	layout.add_child(status_label)
	_refresh_lists()

func _create_league() -> void:
	var league_name := league_name_input.text.strip_edges()
	if league_name.is_empty():
		_set_status("Enter a name for your league.")
		return
	if _find_league(league_name) >= 0:
		_set_status("A league with that name already exists on this device.")
		return
	leagues.append({
		"name": league_name,
		"owner": "You",
		"followers": ["You"],
		"teams": [],
		"applications": []
	})
	league_name_input.clear()
	_save_data()
	_refresh_lists()
	_set_status("League created: " + league_name)

func _follow_league() -> void:
	var name := follow_input.text.strip_edges()
	var index := _find_league(name)
	if index < 0:
		_set_status("That league isn't in this local prototype yet.")
		return
	var followers: Array = leagues[index].get("followers", [])
	if not followers.has("You"):
		followers.append("You")
		leagues[index]["followers"] = followers
		_save_data()
		_refresh_lists()
		_set_status("You're now following " + name + ".")
	else:
		_set_status("You're already following " + name + ".")

func _apply_as_team() -> void:
	var league_name := application_league_input.text.strip_edges()
	var team_name := team_name_input.text.strip_edges()
	var index := _find_league(league_name)
	if index < 0:
		_set_status("League not found on this device.")
		return
	if team_name.is_empty():
		_set_status("Enter your team name.")
		return
	var applications: Array = leagues[index].get("applications", [])
	for application in applications:
		if str(application.get("team", "")).to_lower() == team_name.to_lower():
			_set_status("That team has already applied.")
			return
	applications.append({"team": team_name, "status": "Pending"})
	leagues[index]["applications"] = applications
	_save_data()
	_refresh_lists()
	_set_status("Application sent to " + league_name + ".")

func _approve_application(league_index: int, application_index: int) -> void:
	var applications: Array = leagues[league_index].get("applications", [])
	if application_index >= applications.size():
		return
	var application: Dictionary = applications[application_index]
	application["status"] = "Accepted"
	var teams: Array = leagues[league_index].get("teams", [])
	if not teams.has(application.get("team", "")):
		teams.append(application.get("team", ""))
	leagues[league_index]["applications"] = applications
	leagues[league_index]["teams"] = teams
	_save_data()
	_refresh_lists()
	_set_status(str(application.get("team", "Team")) + " accepted into " + str(leagues[league_index].get("name", "league")) + ".")

func _reject_application(league_index: int, application_index: int) -> void:
	var applications: Array = leagues[league_index].get("applications", [])
	if application_index >= applications.size():
		return
	applications[application_index]["status"] = "Declined"
	leagues[league_index]["applications"] = applications
	_save_data()
	_refresh_lists()
	_set_status("Team application declined.")

func _refresh_lists() -> void:
	if not is_instance_valid(league_list):
		return
	for child in league_list.get_children():
		child.queue_free()
	for child in application_list.get_children():
		child.queue_free()
	if leagues.is_empty():
		var empty := Label.new()
		empty.text = "No leagues yet. Create the first one."
		league_list.add_child(empty)
	for i in range(leagues.size()):
		var league: Dictionary = leagues[i]
		var card := PanelContainer.new()
		var stack := VBoxContainer.new()
		card.add_child(stack)
		var name_label := Label.new()
		name_label.text = str(league.get("name", "League"))
		name_label.add_theme_font_size_override("font_size", 18)
		stack.add_child(name_label)
		var details := Label.new()
		details.text = "%d followers  •  %d teams  •  Owner: %s" % [league.get("followers", []).size(), league.get("teams", []).size(), str(league.get("owner", "Unknown"))]
		stack.add_child(details)
		league_list.add_child(card)
		if str(league.get("owner", "")) == "You":
			var applications: Array = league.get("applications", [])
			for j in range(applications.size()):
				var application: Dictionary = applications[j]
				if str(application.get("status", "")) != "Pending":
					continue
				var row := HBoxContainer.new()
				var app_label := Label.new()
				app_label.text = "%s wants to join %s" % [str(application.get("team", "Team")), str(league.get("name", "league"))]
				app_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
				row.add_child(app_label)
				var accept := Button.new()
				accept.text = "ACCEPT"
				accept.pressed.connect(_approve_application.bind(i, j))
				row.add_child(accept)
				var reject := Button.new()
				reject.text = "DECLINE"
				reject.pressed.connect(_reject_application.bind(i, j))
				row.add_child(reject)
				application_list.add_child(row)
			if applications.is_empty():
				var no_apps := Label.new()
				no_apps.text = "No applications for " + str(league.get("name", "league"))
				application_list.add_child(no_apps)

func _find_league(name: String) -> int:
	for i in range(leagues.size()):
		if str(leagues[i].get("name", "")).to_lower() == name.to_lower():
			return i
	return -1

func _set_status(message: String) -> void:
	status_label.text = message

func _save_data() -> void:
	var config := ConfigFile.new()
	config.set_value("leagues", "items", leagues)
	var error := config.save(SAVE_PATH)
	if error != OK:
		_set_status("Could not save league data on this device.")

func _load_data() -> void:
	var config := ConfigFile.new()
	if config.load(SAVE_PATH) == OK:
		var saved: Variant = config.get_value("leagues", "items", [])
		if saved is Array:
			for item in saved:
				if item is Dictionary:
					leagues.append(item)
