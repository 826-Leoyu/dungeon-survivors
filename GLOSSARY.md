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
一局使用的数值（最大生命值、受伤间隔等）的普通数据；由表现层从 ScriptableObject 转换而来。
_Avoid_: Settings、Params

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
表现层中持有本场景 Run 的组件：按 Inspector 数值创建 Run、每帧 Tick、Run 结束时处理结局（目前是日志加冻结画面）。其他组件通过它向 Run 报告事件。
_Avoid_: GameManager、RunManager

**结局（Outcome）**:
Run 的胜负状态：进行中（`InProgress`）、失败（`Defeat`）；之后加入胜利。Run 结束后不再接受任何事件。
_Avoid_: Result、State、GameOver

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

## 命名约定

- 规则层程序集：`DungeonSurvivors.Rules`，位于 `game/Assets/_Project/Scripts/Rules/`，命名空间 `DungeonSurvivors.Rules`，`noEngineReferences: true`。
- 规则层测试程序集：`DungeonSurvivors.Rules.Tests`（EditMode），位于 `game/Assets/_Project/Tests/EditMode/`，命名空间 `DungeonSurvivors.Rules.Tests`。
- 测试类按被测规则命名 `Run<规则>Tests`（如 `RunHealthTests`）；不属于 Run 的规则类型用 `<类型>Tests`（如 `EnemyHealthTests`），测试方法写成「情境_结果」（如 `DamageWithinHurtCooldown_IsIgnored`）。
- 对象池程序集：`DungeonSurvivors.Pooling`，位于 `game/Assets/_Project/Scripts/Pooling/Core/`，`noEngineReferences: true`；测试程序集 `DungeonSurvivors.Pooling.Tests` 位于 `game/Assets/_Project/Tests/EditMode/Pooling/`。GameObject 适配层 `PrefabPool` 在 `Scripts/Pooling/` 下（不属于该程序集）。
- 新规则（经验、升级抽取、刷怪导演、胜负、计分）加入同一程序集，并自带测试。
