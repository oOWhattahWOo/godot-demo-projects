using Godot;
using System;
// This demo is an example of controlling a high number of 2D objects with logic
// and collision without using nodes in the scene. This technique is a lot more
// efficient than using instancing and nodes, but requires more programming and
// is less visual. Bullets are managed together in the `bullets.gd` script.

/**具体技术实现细节：
 * 1.使用GlobalPosition让玩家跟随鼠标移动 InputEventMouseMotion.Position
 * 2.通过Input.MouseMode = Input.MouseModeEnum.Hidden隐藏鼠标光标
 * 3.玩家节点通过BodyShapeEntered和BodyShapeExited信号，统计当前有多少个敌人碰撞在玩家身上
 * 4.通过Godot引擎的AnimatedSprite2D节点，播放动画红脸和绿脸
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").Frame = 1;
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").Frame = 0；
 **/

public partial class Player : Area2D
{
	//The number of bullets currently touched by the player.
	private int collisionCount = 0;
	private AnimatedSprite2D animatedSprite2D;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// The player follows the mouse cursor automatically, so there's no point in displaying the mouse cursor.
		animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		Input.MouseMode = Input.MouseModeEnum.Hidden;
	}

	public override void _Input(InputEvent inputEvent)
	{
		//Getting the movement of the mouse so the sprite can follow its position.
		if (inputEvent is InputEventMouseMotion mouseMotion)
		{
			// 推荐使用GlobalPosition，因为它是相对于整个场景的坐标（世界坐标）。
			GlobalPosition = mouseMotion.GlobalPosition;

			// 此处不使用 Position，因为它是相对于父节点的坐标（局部坐标）。相当于移动到 “父节点坐标系里的某个位置”
			// 如果父节点的位置发生变化（平移、缩放、旋转、剪切变形），Position的值也会随之改变，会导致玩家的位置不准确。
			// Position = mouseMotion.Position;

			/*坐标使用规则：
			1. 鼠标 / 屏幕坐标
				→ 使用 GlobalPosition
			2. UI / 父节点内部布局
				→ 使用 Position
			3. 想让对象跟随父节点一起移动
				→ 使用 Position
			4. 想让对象不受父节点影响（绝对定位）
				→ 使用 GlobalPosition
			*/
		}
	}

	/*Godot 碰撞信号机制（Node vs PhysicsServer2D）
	========================================================
	【核心结论】
	使用 PhysicsServer2D 创建的物理对象（BodyCreate / ShapeCreate）
	属于“底层物理系统”，不属于 Node 体系，因此：
	✔ 只能触发：
		- body_shape_entered
		- body_shape_exited
	❌ 不能触发：
		- body_entered
		- body_exited
	--------------------------------------------------------
	【原因解释】
	Godot 的碰撞系统分两层：
	① Node 层（高层封装）
		如：
			- Area2D
			- RigidBody2D
			- CharacterBody2D
		特点：
			- 有 Node 身份（在 SceneTree 中）
			- 引擎自动管理碰撞对象映射
		提供简化信号：
			✔ body_entered / body_exited
		含义：
			→ “哪个 Node 进入/离开了区域”
	② PhysicsServer2D 层（底层系统）
		如：
			- PhysicsServer2D.BodyCreate()
			- PhysicsServer2D.ShapeCreate()
		特点：
			- 没有 Node
			- 只有 Rid（资源句柄）
			- 引擎不维护 SceneTree 关系
			- 需要手动管理
		提供信号：
			✔ body_shape_entered / body_shape_exited
		含义：
			→ “哪个物理 Body 的哪个 Shape 发生碰撞”
	--------------------------------------------------------
	【关键区别总结】
	body_entered：
		→ Node 级别碰撞（简单）
		→ 只关心“是谁碰到了我”
	body_shape_entered：
		→ PhysicsServer 级别碰撞（底层）
		→ 可识别具体 shape（更精细）
	--------------------------------------------------------
	【当前项目属于】
	本代码使用 PhysicsServer2D 手动管理 Bullet：
		✔ 无 Node
		✔ 使用 Rid 管理物理对象
		✔ 属于高性能底层实现
	因此必须使用：
		body_shape_entered
	========================================================
	*/
	public void OnBodyShapeEntered(Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex)
	{	
		// Player got touched by a bullet so sprite changes to sad face.
		collisionCount++;
		if (collisionCount > 0)
		{
			animatedSprite2D.Frame = 1;
		}
	}

	public void OnBodyShapeExited(Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex)
	{
		// When non of the bullets are touching the player, sprite changes to happy face.
		collisionCount--;
		if (collisionCount == 0)
		{
			animatedSprite2D.Frame = 0;
		}
	}
}
