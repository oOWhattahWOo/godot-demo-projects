using Godot;
using System;
using System.Linq;

// This demo is an example of controlling a high number of 2D objects with logic
// and collision without using nodes in the scene. This technique is a lot more
// efficient than using instancing and nodes, but requires more programming and
// is less visual. Bullets are managed together in the `bullets.gd` script.

/** 本Demo展示了一种高性能的对象管理方式：
	- 放弃 Node 树结构
	- 使用数据驱动（Data-Oriented）
	- 直接操作 PhysicsServer（底层 API）
本质：这是一个“数据驱动 + 批量处理（Batch）”的实现方式，类似 ECS（Entity Component System）的简化版本
------------------------------------------------------
传统做法：
	- 一个子弹 = 一个 Node2D
	- 引擎帮你更新位置
	- 自动绘制
	- 引擎自动管理生命周期（节点删除时自动释放资源）
------------------------------------------------------
这里的做法：
	- 一个子弹 = 一个 Bullet 数据对象 + 一个 RID
	- 你手动更新
	- 你自己 _Draw()
	- 你必须手动释放底层资源（RID）
------------------------------------------------------
性能极高（可支持上千甚至上万对象）：
	1. 避免大量 Node 创建/销毁开销
	2. 避免每个 Node 的 _process / _physics_process 调度成本
	3. 批量更新数据（CPU cache 友好）
	4. 批量绘制（减少 draw call）
------------------------------------------------------
代价：
	- 可读性下降
	- 调试困难（没有 Node 可视化）
	- 需要手动管理生命周期（包括 RID）
	- 容易出错（同步问题、内存问题）
------------------------------------------------------
适用场景：
	- 子弹系统（Bullet Hell）
	- 粒子系统（自定义粒子）
	- 大量简单单位（上千个）
不适用：
	- 复杂逻辑对象（角色、UI）
	- 需要编辑器可视化的对象
**/

/*** 具体技术实现细节：
1.Declare a bullet class with three properties: (Vector2)position, (float)speed, and (Rid)body
2.Agree on the number of bullets `BulletCount`, `MaxSpeed`, and `MinSpeed`
3.Load the png image resource `bulletTexture` for the bullet from path `BulletImagePath`
4.Use the `bullets` array to manage all bullet instances
5.在_Ready()方法中
	a.	为shape = PhysicsServer2D.circle_shape_create()创建一个圆形碰撞体，并设置半径PhysicsServer2D.shape_set_data(shape, 8)
	b.	通过for循环创建多个子弹实例：
		i.		创建对象var bullet := Bullet.new()
		ii.		设置随机速度bullet.speed
		iii.	设置碰撞体bullet.body
			A.		创建bullet.body = PhysicsServer2D.body_create()
***/

public partial class Main : Node2D
{
	public class Bullet
	{
		public Vector2 position;
		public float speed;

		//	The body is stored as an RID, which is an "opaque" way to access resources.
		//	With large amounts of objects (thousands or more), it can be significantly
		//	faster to use RIDs compared to a high-level approach.
		public Rid body;
	}

	const int BulletCount = 500;
	const float MinSpeed = 20f;
	const float MaxSpeed = 80f;

	const string BulletImagePath = "res://arts/bullet.png";
	private Texture2D bulletImage;

	private readonly Bullet[] bullets = new Bullet[BulletCount];
	private Rid shape;

	public override void _Ready()
	{
		bulletImage = GD.Load<Texture2D>(BulletImagePath);

		shape = PhysicsServer2D.CircleShapeCreate();
		//	Set the collision shape's radius for each bullet in pixels.
		PhysicsServer2D.ShapeSetData(shape, 8);

		for (int i = 0; i < BulletCount; i++)
		{
            var bullet = new Bullet
            {
                // Give each bullet its own random speed.
                // C#用户有时更倾向于使用 .NET 原生的 Random 类或 Godot 的 RandomNumberGenerator 实例，因为 GD.Randf() 是全局静态的。不过对于这个 Demo，GD.Randf() 是最简便的选择。
                speed = (float)GD.RandRange(MinSpeed, MaxSpeed),                
                body = PhysicsServer2D.BodyCreate()
            };

			/**	PhysicsServer2D 中的 Rid 是统一的资源句柄类型，但其内部指向的资源类型不同
				- CircleShapeCreate() 返回的 Rid → 指向 Shape（碰撞形状）
				- BodyCreate() 返回的 Rid → 指向 Body（物理对象）
			不同类型的 Rid 只能传入对应的 API：
				- ShapeSetData() 只能操作 Shape 类型的 Rid
				- BodySetSpace() 只能操作 Body 类型的 Rid
				- BodyAddShape() 只能将 Shape 类型的 Rid 添加到 Body 类型的 Rid 上
			虽然它们在 C# 中类型相同（Rid），但语义和用途完全不同。
			**/
            PhysicsServer2D.BodySetSpace(bullet.body, GetWorld2D().Space);
			// 这里的shape是碰撞形状（CollisionShape）
			PhysicsServer2D.BodyAddShape(bullet.body, shape);
			// Don't make bullets check collision with other bullets to improve performance.
			PhysicsServer2D.BodySetCollisionMask(bullet.body, 0);

			// Place bullets randomly on the viewport and move bullets outside the play area so that they fade in nicely.
			bullet.position = new Vector2(
				(1 + GD.Randf()) * GetViewportRect().Size.X,
				GD.Randf() * GetViewportRect().Size.Y
				);

			bullets[i] = bullet;
		}


	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Order the CanvasItem to update every frame.
		// Godot 不会每帧自动调用 _draw()，必须主动请求
		/**
		通过queue_redraw()通知Godot，下一帧请调用 _draw(),如果删掉这段代码:
			- 子弹在视觉上就不会移动
			- 但是子弹的碰撞体积通过_physics_process还在移动
			- 导致玩家看不到子弹图片与动画效果，但是角色被子弹的碰撞体积击中了
		**/
		QueueRedraw();
	}

	public override void _PhysicsProcess(double delta)
	{
		var transform2d = new Transform2D(0, Vector2.Zero);
		var offset = GetViewportRect().Size.X + 16;
		foreach (var bullet in bullets)
		{
			bullet.position.X -= bullet.speed * (float)delta;
			if (bullet.position.X < -16)
			{
				// Move the bullet back to the right when it left the screen.
				bullet.position.X = offset;
			}

			// 在 Node 系统中，Transform（位置/旋转/缩放）由引擎自动维护
			// 而本Demo绕过了 Node，这样做可以提高性能
			// 但导致数据（bullet.position）和物理引擎里（bullet.body (RID)）没有任何自动绑定关系
			// 所以你必须通过PhysicsServer2D.body_set_state(...)手动同步
			// 相当于：把我计算的位置，告诉物理引擎
            transform2d.Origin = bullet.position;
			PhysicsServer2D.BodySetState(bullet.body, PhysicsServer2D.BodyState.Transform, transform2d);
		}
	}

	public override void _Draw()
	{
		//	Instead of drawing each bullet individually in a script attached to each bullet,
		//	we are drawing *all* the bullets at once here.
		//	因为没有用 Sprite / Node2D 来显示子弹
		//	所以必须手动把所有子弹画出来
		//	一次 draw call 画很多对象，比数百个 Node 快很多
		var offset = - bulletImage.GetSize() / 2;
		foreach (var bullet in bullets)
		{
			DrawTexture(bulletImage, bullet.position + offset);
		}
	}

	public override void _ExitTree()
	{
		// Perform cleanup operations (required to exit without error messages in the console).
		/**	_exit_tree在节点被移除时调用（销毁前）, 触发时机：
			# 场景关闭
			# 节点被 queue_free()
			# 切换场景
		底层资源PhysicsServer2D.body_create() 和 PhysicsServer2D.circle_shape_create()，不会自动释放
		必须手动释放 PhysicsServer2D.free_rid(...)
		**/
		foreach (var bullet in bullets)
		{
			PhysicsServer2D.FreeRid(bullet.body);
		}
		PhysicsServer2D.FreeRid(shape);
	}
}

/** 进阶阅读：
# 【进阶拓展：本demo与对象池（Object Pool）对比】
	# 1️.核心目标
		# Demo（RID + 批处理）：提高运行时性能（更新 + 渲染）
		# 对象池（Object Pool）：避免频繁创建/销毁对象
	# 2️.解决的问题
		# Demo：Node 太多 → 卡
		# 对象池：频繁 new / free → 卡 + 内存抖动
	# 3️.是否使用 Node
		# Demo：❌ 不用 Node（直接操作底层）
		# 对象池：✅ 通常还是用 Node
	# 4️.生命周期管理
		# Demo：你手动管理（必须 free_rid）
		# 对象池：池子帮你复用对象（减少创建销毁）
	# 5️.适用规模
		# Demo：上千 / 上万对象
		# 对象池：几十 ~ 几百（甚至上千，但不极端）
	# 6️.编程复杂度
		# Demo：🚀 很高（底层 + 手动同步）
		# 对象池：中等（逻辑复杂但仍在 Node 体系内）
	
# 【进阶拓展：更极端的数据驱动（SoA）】
	# 本 Demo 目前是 AoS（Array of Structs）：一个数组存了 5000 个 Bullet 对象。
	# 因为 GDScript 中对象依然有内存开销，如果想要【极致性能】，可以改用 SoA：
		# var bullet_positions := PackedVector2Array() # 连续内存，对 CPU Cache 极其友好
		# var bullet_speeds := PackedFloat32Array()
		# var bullet_rids := [] # 存放 RID
	# 这样可以完全省去 Bullet.new() 的对象分配开销，并且遍历速度翻倍。
**/