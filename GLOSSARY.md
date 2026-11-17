# Dungeon Survivors

一款割草 / 弹幕生存游戏。规则层（纯 C#）与表现层（MonoBehaviour）分离，规则全部可在 EditMode 测试中验证。

## Language

### 架构

**规则层（Rules layer）**:
一局游戏全部规则所在的纯 C# 程序集 `DungeonSurvivors.Rules`，编译层面禁止引用 UnityEngine。
_Avoid_: 逻辑层、模型层、core

**表现层（Presentation layer）**:
读输入、驱动物理与画面 / 音效、把事件转发给 Run 并按 Run 的状态更新画面的 MonoBehaviour 代码；不含规则判断。
_Avoid_: 视图层、客户端

**对象池（Object pool）**:
回收并复用对象、避免反复创建 / 销毁的通用工具；纯 C# 核心为 `ObjectPool<T>`（程序集 `DungeonSurvivors.Pooling`），表现层用 `PrefabPool` 包装 prefab 实例。有上限：对象全部取出且已达上限时 `TryGet` 返回 false，调用方放弃这次生成。
_Avoid_: 缓存、Spawner、Factory

**回收（Release）**:
把用完的对象还给对象池，此时运行重置回调。重复回收同一对象被忽略；回收不属于该池的对象是调用方 bug，会抛异常。
_Avoid_: Return、Despawn、Recycle

### 一局游戏

**Run**:
一局游戏（从开始到胜利或失败）的规则状态，是规则层的入口。代码中为 `Run` 类。
_Avoid_: Game、Session、Match、Round

**Run 配置（RunConfig）**:
一局使用的全部数值的普通数据，包含玩家配置、敌人配置（目前只有追踪型敌人 `Chaser`）、开局武器配置（`StartingWeapon`）、环绕飞刀配置（`OrbitingKnives`）和经验曲线（`Progression`）。由 RunHost 在 Run 开始时从 Run 定义转换而来；Run 自己用玩家的数值，表现层组件从 `Run.Config` 读取各自的数值。
_Avoid_: Settings、Params

**玩家 / 敌人 / 武器配置（PlayerConfig / EnemyConfig / WeaponConfig / OrbitingKnivesConfig）**:
规则层中某一对象数值的不可变普通数据，构造时拒绝非法数值。玩家：最大生命值、受伤间隔、移动速度、拾取半径；敌人：生命值、移动速度、接触伤害、掉落经验；子弹类武器（`WeaponConfig`）：伤害、射击间隔、子弹速度、子弹飞行时间、散射角度，以及强化数值（`WeaponUpgradeConfig`：每级加多少、最高几级）；环绕飞刀（`OrbitingKnivesConfig`）：伤害、飞刀数量、半径、转速、命中间隔，以及强化数值（`OrbitingKnivesUpgradeConfig`）。
_Avoid_: Stats、Settings

**定义（Definition）**:
在编辑器中调整数值的 ScriptableObject 资产（`PlayerDefinition`、`EnemyDefinition`、`WeaponDefinition`、`OrbitingKnivesDefinition`、`ProgressionDefinition`，以及把它们组合在一起的 `RunDefinition`），各自用 `ToConfig()` 转换为对应的配置。放在 `Assets/_Project/Config/`；只在 Run 开始时读取一次，所以修改后下次运行生效。规则层不引用定义。
_Avoid_: Data、Preset、Template、用「配置」指 ScriptableObject

**Tick**:
由调用方推进 Run 时钟的一步，参数为经过的秒数；Run 自己从不读取时钟。
_Avoid_: Update、Step

**存活时间（Elapsed time）**:
Run 已推进的秒数；Run 结束后停止增长。代码中为 `ElapsedTime`。
_Avoid_: 游戏时间、timer

**受伤间隔（Hurt cooldown）**:
受到一次伤害后，再次受伤被忽略的秒数。代码中为 `HurtCooldown`。
_Avoid_: 无敌帧、i-frames、invincibility

**RunHost**:
表现层中持有本场景 Run 的组件：按 Run 定义创建 Run、每帧 Tick、Run 结束时处理结局（目前是日志加冻结画面）。其他组件通过它向 Run 报告事件。
_Avoid_: GameManager、RunManager

**击杀数（Kills）**:
本局被杀死的敌人数量。敌人死亡时由表现层报告给 Run；Run 结束后的击杀被忽略。代码中为 `Run.Kills` / `Run.RecordKill`。
_Avoid_: Score、kill count

**结局（Outcome）**:
Run 的胜负状态：进行中（`InProgress`）、失败（`Defeat`）；之后加入胜利。Run 结束后不再接受任何事件。
_Avoid_: Result、State、GameOver

### 经验与等级

**经验（XP）**:
玩家拾取经验宝石获得的点数，累积到下一级所需经验时升级。代码中 `Run.Xp` 是当前等级已攒的经验，`Run.GainXp` 报告获得的经验；Run 结束后获得的经验被忽略。
_Avoid_: Experience、EXP、points

**等级（Level）**:
玩家的等级，从 1 开始；经验够了立即升级，不等升级被处理。代码中为 `Run.Level`。
_Avoid_: Rank、Tier

**经验曲线（Progression）**:
每级所需经验的规则：从 n 级升到 n + 1 级需要 `FirstLevelXp + XpGrowth × (n − 1)`，每级比上一级多一点。代码中为 `ProgressionConfig`（`XpToLevelUpFrom`），在 `ProgressionDefinition` 中调整；当前等级还差多少看 `Run.XpToNextLevel`。
_Avoid_: XP table、level curve、用 Progression 指局外成长

**待处理的升级（Pending level-up）**:
已经达到、但还没让玩家三选一的升级。一次获得的经验够升多级时，每级排一个，不会丢失；升级三选一逐个处理（`Run.ChooseUpgrade`）。所有强化都满级时，剩下的待处理升级直接作废。代码中为 `Run.PendingLevelUps`。
_Avoid_: Level-up queue（指代码时）、unspent levels

**强化（Upgrade）**:
升级时可选的一项：把某件武器的某个数值提高一级（如直线弹幕的伤害、子弹数量、射速），或获得一件新武器（获得武器，如 `OrbitingKnivesUnlock`，最高 1 级）。新武器的数值强化在获得它之后才出现。每项有自己的最高等级，满级后不再出现；最高等级为 0 表示从不出现。每件武器自己决定当前提供哪些强化（`IUpgradeable`），Run 把选中的强化交给它所属的武器。代码中为 `Upgrade` 枚举，名字会作为游玩数据上报，改名要谨慎。
_Avoid_: Perk、Buff、Power-up、Skill

**升级三选一（Upgrade choice）**:
最早一个待处理的升级给出的选项：从可用的强化（未满级的）中不重复抽取至多三项，可用项不足三项时给出实际数量。玩家选一项后，强化生效，处理下一个待处理的升级。选择期间表现层冻结游戏时间。代码中为 `Run.UpgradeChoices` / `Run.ChooseUpgrade`，界面为 `LevelUpPanel`。
_Avoid_: Level-up menu、card draft、reward

**不重复抽取（Distinct draw）**:
从池中随机抽取若干个不同的项，随机数来自注入的 `IRandom`，所以同一种子抽出同样结果。代码中为 `DistinctDraw.Pick`。
_Avoid_: Shuffle、Roll

**经验宝石（XP gem）**:
敌人死亡时在原地掉落的拾取物，带着该敌人的掉落经验（`EnemyConfig.XpDrop`），从对象池取出；池满时这次掉落被跳过。不会自己消失。代码中为 `XpGem`。
_Avoid_: Orb、Pickup、Drop、Coin

**拾取半径（Pickup radius）**:
经验宝石与玩家的距离小于它时被拾取，经验立即交给 Run，宝石回收。是玩家配置的数值（`PlayerConfig.PickupRadius`），不是碰撞体大小。
_Avoid_: Magnet range、collect radius

### 敌人与刷怪

**追踪型敌人（Chaser）**:
直线朝玩家移动的第一种敌人。代码中为 `ChaserEnemy`。身体是 kinematic trigger：敌人之间互相重叠、不推玩家、穿墙。
_Avoid_: Zombie、Follower

**敌人生命值（Enemy health）**:
单个敌人的生命值规则。只有致命的那一击报告死亡，打在已死敌人身上的伤害被忽略。代码中为 `EnemyHealth`。
_Avoid_: HP、EnemyStats

**接触伤害（Contact damage）**:
敌人碰到玩家时报告给 Run 的伤害；是否生效由受伤间隔决定，敌人自己不计时。
_Avoid_: 碰撞伤害、touch damage

**刷怪圈（Spawn ring）**:
摄像机可视矩形向外扩一圈边距后的边框；敌人在边框上均匀随机的位置生成，所以总在屏幕外出现。代码中为 `SpawnRing`。
_Avoid_: 刷怪区、spawn area

**敌人生成器（Enemy spawner）**:
表现层组件 `EnemySpawner`：按刷怪计时器的节奏，在刷怪圈上从对象池取出敌人并交给它场景引用。不含规则判断；规则在刷怪计时器和刷怪圈里。
_Avoid_: 用 Spawner 指对象池或刷怪计时器

**刷怪计时器（Spawn timer）**:
按固定间隔给出该生成几个敌人；长帧不丢失生成次数。刷怪导演（W2）会取代固定间隔。代码中为 `SpawnTimer`。
_Avoid_: Spawner、wave timer

### 武器

**直线弹幕（Straight shot）**:
开局自带的武器：按射速从玩家位置朝最近的敌人发射一轮子弹，多发时呈扇形散开；没有敌人时、游戏冻结时不发射。伤害、子弹数量、射速随强化提高，当前数值在规则层的 `Run.StraightShot`，表现层组件为 `StraightShotWeapon`。
_Avoid_: Gun、Shooter、Projectile weapon

**子弹（Bullet）**:
武器发射的直线飞行物，从对象池取出；命中第一个敌人造成伤害后回收，飞行时间（lifetime）用完也回收。穿过墙和玩家。代码中为 `Bullet`。
_Avoid_: Projectile、Shot、Missile

**射击冷却（Fire cooldown）**:
决定武器何时可以开火的规则：开火后需等待一个射击间隔（射速强化改变间隔后，从下一发开始生效）；一开始就是就绪状态；就绪状态不累积，没有目标的时间不会攒成一串连发。代码中为 `FireCooldown`。与刷怪计时器不同，后者会补上长帧丢失的次数。
_Avoid_: Fire timer、reload

**扇形散射（Fan spread）**:
一轮多发子弹的瞄准方式：以朝向目标的方向为中心，相邻子弹相隔固定角度（散射角度），左右对称；只有一发时正对目标。代码中为 `FanSpread`。
_Avoid_: Shotgun、spread shot（指代码时）

**最近目标（Nearest target）**:
武器瞄准的敌人：离射击者最近的一个；距离相同时取列表中靠前的。代码中为 `NearestTarget`。
_Avoid_: Lock-on、aim assist

**环绕飞刀（Orbiting knives）**:
通过升级三选一获得的武器：若干把飞刀均匀分布、绕玩家旋转，碰到敌人造成伤害；飞刀不会用完，也不从对象池取出。获得前不显示、不造成伤害；游戏冻结时停止转动。飞刀数量、伤害、半径、转速随强化提高，当前数值在规则层的 `Run.OrbitingKnives`，表现层组件为 `OrbitingKnivesWeapon`，每把飞刀为 `Knife`。
_Avoid_: Orbiter、Satellite、Blade ring

**命中间隔（Hit interval）**:
同一个敌人被同一件武器再次命中前需等待的秒数；一直接触的武器（环绕飞刀）每个物理步都报告接触，命中间隔决定哪些命中生效，多把飞刀同时碰到也只算一次。按敌人的生成编号（`ChaserEnemy.SpawnId`，对象池复用时也会换新）计时。代码中为 `HitCooldowns`。与受伤间隔不同，后者保护玩家。
_Avoid_: Damage tick、i-frames、hit cooldown（指概念时）

**环绕布局（Orbit layout）**:
飞刀在圆周上的位置：均匀分布、整体按同一角度旋转，0 号飞刀从右侧开始，逆时针转动。代码中为 `OrbitLayout`。
_Avoid_: Ring formation

### 输入

**浮动摇杆（Floating joystick）**:
触屏移动输入：手指在任意非 UI 区域按下时摇杆出现在按下点，拖动距离除以半径即力度（最大为 1），松手归零。由按下它的那根手指独占。规则在纯 C# 的 `FloatingStick`；表现层 `FloatingJoystick` 把值写进虚拟手柄左摇杆，所以 `Player/Move` 不区分设备。
_Avoid_: 虚拟摇杆、VirtualJoystick、Thumbstick

### 摄像机

**可视面积（Visible area）**:
屏幕上能看到的世界面积（平方单位）。无论横屏、竖屏或屏幕多长，都保持不变，让不同设备的难度接近；默认 177.8，即 16:9 屏幕在正交尺寸 5 时的 17.8 x 10。代码中为 `visibleArea`（`AdaptiveCamera` 的字段）；由它算出正交尺寸的规则是 `ViewFit.OrthographicSize`。
_Avoid_: 视野、FOV、zoom

**自适应摄像机（Adaptive camera）**:
表现层组件 `AdaptiveCamera`：屏幕比例变化（改窗口大小、旋转手机）时，按可视面积重算正交摄像机尺寸。不强制旋转屏幕。
_Avoid_: CameraScaler、ResponsiveCamera

## 命名约定

- 规则层程序集：`DungeonSurvivors.Rules`，位于 `game/Assets/_Project/Scripts/Rules/`，命名空间 `DungeonSurvivors.Rules`，`noEngineReferences: true`。
- 规则层测试程序集：`DungeonSurvivors.Rules.Tests`（EditMode），位于 `game/Assets/_Project/Tests/EditMode/`，命名空间 `DungeonSurvivors.Rules.Tests`。
- 测试类按被测规则命名 `Run<规则>Tests`（如 `RunHealthTests`）；不属于 Run 的规则类型用 `<类型>Tests`（如 `EnemyHealthTests`），测试方法写成「情境_结果」（如 `DamageWithinHurtCooldown_IsIgnored`）。
- 对象池程序集：`DungeonSurvivors.Pooling`，位于 `game/Assets/_Project/Scripts/Pooling/Core/`，`noEngineReferences: true`；测试程序集 `DungeonSurvivors.Pooling.Tests` 位于 `game/Assets/_Project/Tests/EditMode/Pooling/`。GameObject 适配层 `PrefabPool` 在 `Scripts/Pooling/` 下（不属于该程序集）。
- 新规则（经验、升级抽取、刷怪导演、胜负、计分）加入同一程序集，并自带测试。
- 数值配置：规则层的普通数据命名为 `<对象>Config`，放在 `Scripts/Rules/`；对应的 ScriptableObject 命名为 `<对象>Definition`，脚本放在 `Scripts/Config/`，资产放在 `Assets/_Project/Config/`。之后新增的武器、敌人、升级、刷怪曲线都按此方式配置。
