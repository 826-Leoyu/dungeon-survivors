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

**结局（Outcome）**:
Run 的胜负状态：进行中（`InProgress`）、失败（`Defeat`）；之后加入胜利。Run 结束后不再接受任何事件。
_Avoid_: Result、State、GameOver

## 命名约定

- 规则层程序集：`DungeonSurvivors.Rules`，位于 `game/Assets/_Project/Scripts/Rules/`，命名空间 `DungeonSurvivors.Rules`，`noEngineReferences: true`。
- 规则层测试程序集：`DungeonSurvivors.Rules.Tests`（EditMode），位于 `game/Assets/_Project/Tests/EditMode/`，命名空间 `DungeonSurvivors.Rules.Tests`。
- 测试类按被测规则命名 `Run<规则>Tests`（如 `RunHealthTests`），测试方法写成「情境_结果」（如 `DamageWithinHurtCooldown_IsIgnored`）。
- 新规则（经验、升级抽取、刷怪导演、胜负、计分）加入同一程序集，并自带测试。
