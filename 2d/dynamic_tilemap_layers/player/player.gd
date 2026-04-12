extends CharacterBody2D

const WALK_FORCE = 600
const WALK_MAX_SPEED = 200
const STOP_FORCE = 1300
const JUMP_SPEED = 200

# 在 `ProjectSetting -> Physics -> 2D` 中可以看到`default_gravity`是 500 px/s^2
@onready var gravity: float = ProjectSettings.get_setting("physics/2d/default_gravity")

func _physics_process(delta: float) -> void:
	# Horizontal movement code. First, get the player's input.
	# 按下键盘时，Input.get_axis()返回值只有±1
	# 操作手柄摇杆时，Input.get_axis()返回值是[-1,1]区间内的float
	var walk := WALK_FORCE * (Input.get_axis(&"move_left", &"move_right"))
	# Slow down the player if they're not trying to move.
	# 0.2的阈值确保轻微触碰手柄摇杆不会导致玩家移动
	if abs(walk) < WALK_FORCE * 0.2:
		# The velocity, slowed down a bit, and then reassigned.
		# m·△v = F·△t，这里△t = delta,m = 1，△v = a·△t = STOP_FORCE * delta
		# v1 = v0 + △v; move_toward的第一个参数`from`就相当于是v0，把返回值v1赋值给velocity.x
		velocity.x = move_toward(velocity.x, 0, STOP_FORCE * delta)
	else:
		# △v = a·△t = walk * delta
		velocity.x += walk * delta
	# Clamp to the maximum horizontal movement speed.
	velocity.x = clamp(velocity.x, -WALK_MAX_SPEED, WALK_MAX_SPEED)

	# Vertical movement code. Apply gravity.
	velocity.y += gravity * delta

	# Move based on the velocity and snap to the ground.
	# TODO: This information should be set to the CharacterBody properties instead of arguments: snap, Vector2.DOWN, Vector2.UP
	# TODO: Rename velocity to linear_velocity in the rest of the script.
	move_and_slide()

	# Check for jumping. is_on_floor() must be called after movement code.
	if is_on_floor() and Input.is_action_just_pressed(&"jump"):
		velocity.y = -JUMP_SPEED
