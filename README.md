# Godot demo projects

Each folder containing a `project.godot` file is a demo project meant to
be used with [Godot Engine](https://godotengine.org), the open source
2D and 3D game engine.

## Godot versions

- The [`master`](https://github.com/godotengine/godot-demo-projects) branch is compatible with Godot's `master` development branch (next 4.x release).
- The [`3.x`](https://github.com/godotengine/godot-demo-projects/tree/3.x) branch is compatible with Godot's `3.x` development branch (next 3.x release).
- The other branches are compatible with the matching stable versions of Godot.
  - [Click here](https://github.com/godotengine/godot-demo-projects/branches) to see all branches.
  - For example, the [`2.1`](https://github.com/godotengine/godot-demo-projects/tree/2.1)
    branch is for demos compatible with Godot 2.1.x.

## Importing all demos

To import all demos at once in the project manager:

- Clone this repository or [download a ZIP archive](https://github.com/godotengine/godot-demo-projects/archive/master.zip).
  - If you've downloaded a ZIP archive, extract it somewhere.
- Open the Godot project manager and click the **Scan** button on the right.
- Choose the path to the folder containing all demos.
- All demos should now appear in the project manager.

## Try the demos in your browser

Most of the demos are exported to GitHub Pages. They can be viewed
[here](https://godotengine.github.io/godot-demo-projects/).

**Note:** The performance of Godot in a browser is lower than natively on
desktop. For the best performance, consider downloading the demos.

## Useful links

- [Main website](https://godotengine.org)
- [Source code](https://github.com/godotengine/godot)
- [Documentation](https://docs.godotengine.org/en/stable/)
- [Community hub](https://godotengine.org/community/)
- [TPS demo](https://github.com/godotengine/tps-demo)

## License

Those demos are distributed under the terms of the MIT license, as
described in the [LICENSE.md](LICENSE.md) file.

---

## Tips for Beginner from WattWatt
---

### 简单Godot Demo的学习顺序（仅适用于简单场景和系统，不适用于完整的游戏demo）
**第一步：先“玩”项目（非常重要）**
你要观察，并尝试列举出这个游戏所有的功能或Features：
  - 角色怎么移动？（有没有加速/惯性）
  - 跳跃手感如何？（是否有二段跳、缓冲）
  - 有没有敌人呢，他的行动模式与规律？
  - 有没有碰撞、受击、反馈？
  - UI 是怎么触发的？

**第二步：看 Scene（Node 树结构）**
1. 搞清楚每个 Node 的职责
  - 谁负责显示？（Sprite）
  - 谁负责碰撞？（Collision）
  - 谁负责逻辑？（Script 挂在哪？）

2. 看Script 挂在哪个 Node 上
功能或Feature = 某个 Node + 它的 Script

**第三步：看 Inspector（属性面板）**
重点看这些：
- 物理类
  - collision layer / mask
  - gravity / velocity 相关参数
- 视觉类
  - texture
  - animation
- 行为类
  - Camera 是否跟随
  - 是否开启 physics_process

🎯 很多“行为”其实不是代码写的，是配置出来的

**第四步：看 Signal（信号）**
谁触发 → 谁处理

**第五步：最后才看 Script**
**1、 先看入口函数**
```gdscript
func _physics_process(delta)
func _process(delta)
func _ready()
```
搞清楚：
- 什么时候执行
- 做了什么大事

**2、再看“输入 → 行为”**
```gdscript
Input.get_axis()
Input.is_action_pressed()
```
找：玩家操作 → 游戏行为

**3、最后看细节逻辑**
比如：
- move_and_slide
- jump
- 状态切换
---
### 🚀 Godot 项目逆向学习工作流 (小白进阶版)

> **核心原则：** 不要试图一次看懂全部。采用 **“黑盒观察 -> 骨架拆解 -> 局部解剖 -> 破坏性实验”** 的策略。

#### 第一阶段：宏观观察 (黑盒理解)
**目标：** 搞清楚这个项目“在干什么”，建立直观感受。

1.  **先“玩”游戏：** 运行项目，遍历所有操作（跳跃、射击、吃金币、暂停）。
2.  **观察表现：**
    *   角色移动有惯性吗？
    *   子弹是直线飞还是抛物线？
    *   吃到金币后 UI 有什么变化？
    *   镜头的移动规则
    *   敌人与Npc的行动模式
3.  **禁忌：** 此阶段禁止打开编辑器里的任何脚本（Script）。

#### 第二阶段：架构拆解 (看骨架)
**目标：** 理解项目的场景组织逻辑。

1.  **只看主场景一层：** 打开 `Game.tscn`（或主场景），收起所有节点。
    *   `Game` (根节点：控制游戏流程)
    *   `Level` (关卡容器：物理环境、背景)
    *   `InterfaceLayer` (UI层：显示分数、暂停菜单)
2.  **识别“胶卷”图标：** 
    *   看到节点右侧有“🎬 胶卷”图标，说明它是**实例化场景**。
    *   **思考：** 开发者为什么把 Player 独立出去？（为了复用和保持主场景简洁）。
3.  **检查项目设置：** 
    *   `Project -> Project Settings -> Input Map`：看开发者定义了哪些按键映射（如 `jump` 对应键盘哪个键）。

#### 第三阶段：功能模块化 (分块学习)
**目标：** 别按节点学，按“功能”学。一次只攻克一个系统。

| 模块系统 | 核心关注点 | 建议学习顺序 |
| :--- | :--- | :---: |
| **Player (核心)** | 移动逻辑、碰撞处理、动画切换 | 1 (优先) |
| **Camera (环境)** | 相机跟随、边界限制 (Limits) | 2 |
| **Combat (交互)** | 子弹生成 (Instance)、伤害判定 | 3 |
| **Enemy (AI)** | 自动巡逻 (RayCast)、转向逻辑 | 4 |
| **UI (系统)** | 按钮点击、游戏暂停 (Pause Mode) | 5 |

#### 🔍 第四阶段：系统深度解剖 (以 Player 为例)
**目标：** 搞清楚“输入 -> 处理 -> 输出”的闭环。

##### 1. 结构解剖 (Node Tree)
*   **Sprite2D / AnimatedSprite：** 负责“显示形象”。
*   **CollisionShape2D：** 负责“碰撞范围”。
*   **RayCast2D：** 负责“探测”（如探测地面、墙壁或悬崖）。

##### 2. 代码入口 (Script)
不要逐行读，只找核心生命周期函数：
*   `_ready()`: 初始化时做了什么？（比如获取组件引用）。
*   `_physics_process(delta)`: **每一帧**在处理什么物理逻辑？（最重要的逻辑入口）。
*   `get_input()`: 开发者自定义的函数，看它是怎么读取按键的。

##### 3. 关键变量搜索
*   **搜索 `velocity`**：看速度向量是如何在不同状态下被改变的。
*   **搜索 `move_and_slide`**：找到物理引擎驱动移动的最终出口。

#### 第五阶段：破坏性验证 (变通应用)
**目标：** 确认你真的懂了。**不改代码 = 没学会。**

1.  **参数微调：** 把 `JUMP_VELOCITY` 从 `-400` 改到 `-1000`。运行看跳跃高度的变化。
2.  **代码注释：** 注释掉 `is_on_floor()` 的判断代码。观察角色是否变成了“月球漫步”或无限多段跳。
3.  **节点删除：** 尝试关掉 `Camera2D` 的 `Smoothing` 或 `Limit`，看相机表现有什么不同。
4.  **结论：** 如果你改了一行代码，游戏表现符合你的预期，你才算学会了这个知识点。

#### 第六阶段：理解“系统连接” (信号与通信)
**目标：** 搞清楚节点之间怎么“跨频道说话”。

1.  **观察信号 (Signals)：**
    *   选中一个金币 (Coin)，点击右侧面板的 `Node -> Signals`。
    *   查看 `body_entered` 连接到了哪个脚本的哪个函数。
2.  **查找远程调用：**
    *   搜索代码中的 `get_node()`、`get_parent()` 或 `owner`。
    *   看 Player 死亡时是如何通知 UI 显示“游戏结束”的。

#### 第七阶段：遇到陌生节点怎么办？
**目标：** 建立自主查阅文档的习惯。

1.  **看名字猜测：** `Timer` 肯定跟时间有关，`CanvasLayer` 肯定跟 UI 层级有关。
2.  **看父类：** 鼠标悬停在节点上看继承关系（比如继承自 `Node2D` 说明有坐标，继承自 `Control` 说明是 UI）。
3.  **内置文档 (最快)：**
    *   在代码里按住 **Ctrl + 鼠标左键** 点击任何内置类名或函数名。
    *   或者选中节点后按 **F1**。
4.  **属性实验：** 在 `Inspector` 检查器面板里胡乱勾选一下开关，直接运行看画面变化。

##### 💡 给新手的最后建议
> **“先做加减法，再做创造法。”**
> 
> 在你尝试自己写一个新游戏之前，先试着在这个 Demo 里尝试**添加一个功能**（比如加一个双倍跳跃，或者加一种陷阱）。当你能在这个复杂的项目里“塞进”自己的代码而不导致崩溃时，你就已经真正上手 Godot 了。

---
### Demo评级

评级 | 评价 | 说明
--|-- | --
☆☆☆☆☆ | 新手入门 | 基础知识
☆☆☆☆ | 新手进阶 | 基础知识的巩固与进阶
☆☆☆ | 此次 | x
☆☆ | 最后学习 | x
☆ | 不推荐 | x

### bullet_shower
- **评分：**☆☆
- **文件夹：2D**
- 性能优化、对象管理

**评分理由**
本Demo展示了一种高性能的对象管理方式，建议新手优先打好基础，学会制作自己的游戏原型（即使是屎山），在实战中遇到性能瓶颈时再回头学习性能优化相关的demo

**教学内容**
- 让角色跟随鼠标移动
- 隐藏鼠标光标
- 使用碰撞信号(body_shape_entered和body_shape_exited)
- 一种高性能的放弃Node结构、使用数据驱动、直接操作 PhysicsServer（底层 API）的对象管理方式，适用场景：
	- 子弹系统（Bullet Hell）
	- 粒子系统（自定义粒子）
	- 大量简单单位（上千个）

### dynamic_tilemap_layers
- ☆☆☆
- 2D
- TileMapLayers

**评分理由**
Example of how to make a fake wall using TileMap's _tile_data_runtime_update() method
建议新手先去学习TileMapLayers的基本知识，再回来学如何制作隐藏房间。

**教学内容**
待补充

### platformer
- ☆☆☆☆☆
- 2D
- TileMapLayers

**评分理由**
Godot官方文档推荐教程
>[!note] This demo is a pixel art 2D platformer with graphics and sound.
>It shows you how to code characters and physics-based objects in a real game context. This is a relatively complete demo where the player can jump, walk on slopes, fire bullets, interact with enemies, and more. It contains one closed level, and the player is invincible, unlike the enemies.

一个完整的2D平台游戏Demo，实现了二段跳（类似MC的跳跃卡头加速效果），利用斜面助跑跳跃到更高高度，实体平台（上下都不可穿透），仅承重平台（可从下部跳跃穿透），移动的电梯平台可托举玩家，角色发射子弹，有碰撞体积阻碍的敌人，子弹击杀敌人，金币拾取与音效，镜头跟随玩家，场景设计，游戏暂停与继续，金币统计UI，游戏退出按钮，双玩家分屏操作
- Side-scrolling player controller using KinematicBody2D. 使用 KinematicBody2D 的横版卷轴玩家控制器。
- Can walk on and snap to slopes.可以行走并吸附在斜坡上。
- Can shoot, including while jumping.
- Enemies that crawl on the floor and change direction when they encounter an obstacle.敌人会在地面上爬行，遇到障碍物时会改变方向。
- Camera that stays within the level’s bounds. 始终保持在关卡范围内的摄像机
- Supports keyboard and gamepad controls.
- Platforms that can move in any direction.
- Gun that shoots bullets with rigid body (natural) physics.
- Collectible coins.
- Pause and pause menu.
- Pixel art visuals.
- Sound effects and music.

**教学内容**

**存在的Bug**
最下层的怪物爬行到最左侧碰到墙壁后不会回头，原地发呆

### 模版
- ☆☆☆☆☆
- 2D

**评分理由**

**教学内容**