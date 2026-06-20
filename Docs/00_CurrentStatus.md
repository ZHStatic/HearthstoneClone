# Current Status

最后更新：2026-06-19

## 当前阶段

阶段 1、阶段 1.5、阶段 2.1、阶段 2.1.5、阶段 2.2、阶段 2.3、阶段 2.4、阶段 2.4.5、阶段 2.5.0 已完成。

当前停靠点：

```text
阶段 2.5.0 已完成：事件系统基础链路
```

冲锋的最小链路已经测试通过：`CardData` 配置关键词，`Minion` 复制关键词，`GameManager` 在召唤后让冲锋随从立刻可以攻击，`CardView` 可以在手牌描述区显示“冲锋”。

嘲讽的代码链路已经写入：`KeywordType` 增加 `Taunt`，`GameManager` 在攻击随从和攻击英雄前检查防守方是否有活着的嘲讽随从，`CardView` 和 `MinionView` 可以显示“嘲讽”。

战吼的最小链路已经测试通过：`BattlecryType` 定义战吼类型，`CardData` 支持配置战吼类型和通用数值，`GameManager` 在随从召唤成功后调用 `ResolveAfterSummon()` 处理冲锋和战吼，`CardView` 可以在手牌描述区显示战吼文字。

事件系统基础链路已经测试通过：`GameEventType` 定义事件类型，`GameEvent` 承载事件数据，`GameEventBus` 管理订阅和发布，`GameManager` 可以发布 `CardPlayed` 和 `MinionSummoned`，Console 日志已确认监听回调会执行。

## 当前可玩内容

当前已验证 Play 模式能完成：

```text
抽牌
出牌
召唤随从
召唤冲锋随从并立即攻击
召唤带战吼的随从并触发一次性效果
手牌和场上随从显示关键词
手牌显示战吼说明
施放单目标伤害法术
费用不足和非法操作提示
随从攻击随从
随从攻击英雄
结束回合
胜负判定
```

当前测试卡牌：

- 基础随从：训练新兵、河湾猎手、城墙守卫、战场斗士、岩石巨人。
- 冲锋随从：疾风斥候，2 费，2/1，关键词为 `Charge`。
- 嘲讽随从：城墙守卫，3 费，2/5，关键词为 `Taunt`。
- 基础法术：火花，1 费，造成 2 点伤害，目标为任意角色。
- 战吼随从：火焰学徒，2 费，2/2，战吼为对敌方英雄造成 1 点伤害。
- 战吼随从：书卷侍从，2 费，1/2，战吼为抽 1 张牌。

## 当前代码结构

### Core 层

| 文件 | 职责 |
|------|------|
| `CardData.cs` | ScriptableObject 卡牌模板数据 |
| `CardType.cs` | 卡牌类型：随从、法术 |
| `SpellTargetType.cs` | 单目标法术可选择的目标范围 |
| `KeywordType.cs` | 关键词类型：当前支持 `Charge`、`Taunt` |
| `BattlecryType.cs` | 战吼类型：当前支持对敌方英雄造成伤害、抽牌 |
| `GameEventType.cs` | 游戏事件类型：当前包含出牌、召唤、死亡、回合开始和回合结束 |
| `GameEvent.cs` | 游戏事件数据：记录事件类型和相关上下文 |
| `GameEventBus.cs` | 事件总线：管理事件订阅和发布 |
| `Card.cs` | 手牌/牌库中的运行时卡牌实例 |
| `Hero.cs` | 英雄生命、受伤、治疗、死亡判断 |
| `Player.cs` | 手牌、牌库、法力水晶、抽牌、出牌 |
| `Board.cs` | 双方战场随从列表和召唤位置限制 |
| `Minion.cs` | 场上随从的攻击、生命、所属玩家、攻击权限和关键词 |
| `GameManager.cs` | 当前阶段的对局流程调度，包含冲锋召唤处理、嘲讽攻击目标检查、最小战吼结算和基础事件发布 |

### UI 层

| 文件 | 职责 |
|------|------|
| `CardView.cs` | 显示一张手牌、关键词文字、战吼文字并转发点击 |
| `HandView.cs` | 根据手牌列表生成多个 `CardView` |
| `MinionView.cs` | 显示一个场上随从、点击和选中高亮 |
| `BoardView.cs` | 根据一方战场列表生成多个 `MinionView` |
| `GameUIController.cs` | 连接 UI 和 `GameManager`，处理点击、选择状态、反馈和刷新 |

## 文档分工

以后按这个分工维护文档，避免重复：

| 文档 | 只负责 |
|------|--------|
| `Docs/00_CurrentStatus.md` | 当前进度、当前停靠点、下一步 |
| `Docs/01_ProjectPlan.md` | 项目长期路线和阶段目标 |
| `Docs/02_CoreArchitecture.md` | Core 层职责、依赖、边界和后续拆分点 |
| `Docs/03_UIArchitecture.md` | UI 层职责、点击输入、刷新方式 |
| `Docs/04_FeatureFlows.md` | 玩家操作到代码调用的流程 |
| `Docs/05_InterviewNotes.md` | 面试时怎么讲这个项目 |
| `Docs/Learning/` | 学习笔记，不要求和正式架构文档完全同步 |

## 已确认

- Unity Play 模式可以运行。
- 牌库中配置有效 `CardData` 后，手牌可以显示。
- 空的 `CardData` 会被 `Player` 跳过，避免开局空引用。
- 随从牌可以通过 `GameManager.TryPlayMinionCard(card)` 召唤。
- 配置了 `Charge` 的随从召唤后会立刻进入可攻击状态。
- 手牌和场上随从可以显示“冲锋”“嘲讽”等关键词文字。
- 手牌可以显示“战吼：对敌方英雄造成 X 点伤害”和“战吼：抽 X 张牌”。
- 火焰学徒的战吼已在 Play 模式测试通过：打出后敌方英雄立刻减少 1 点生命。
- 书卷侍从的战吼已在 Play 模式测试通过：打出后己方抽 1 张牌。
- 法术牌可以进入选目标状态，并通过 `TryPlaySpellCardOnMinion` / `TryPlaySpellCardOnHero` 结算。
- 出牌成功后，手牌减少、法力减少、战场或目标血量刷新。
- 结束回合后，当前行动者切换，UI 刷新。
- 随从攻击随从、随从攻击英雄和胜负判定已测试通过。

## 阶段 2.4 已验证

- Unity 已导入 `BattlecryType.cs` 并生成 `.meta`。
- 已创建测试卡“火焰学徒”。
- 配置：`CardType = Minion`，`Cost = 2`，`Attack = 2`，`Health = 2`。
- 配置：`Battlecry Type = DealDamageToEnemyHero`，`Battlecry Damage = 1`。
- 已把“火焰学徒”加入 `GameManager` 的测试牌库。
- Play 模式已确认：手牌描述区显示“战吼：对敌方英雄造成 1 点伤害”。
- Play 模式已确认：打出后敌方英雄立刻减少 1 点生命。
- Play 模式已确认：打出的随从仍然正常进入战场。

## 阶段 2.4.5 已验证

- Unity 已自动生成“书卷侍从”的 `.asset` 和 `.meta`。
- 已创建测试卡“书卷侍从”。
- 配置：`CardType = Minion`，`Cost = 2`，`Attack = 1`，`Health = 2`。
- 配置：`Battlecry Type = DrawCard`，`Battlecry Value = 1`。
- 已把“书卷侍从”加入 `GameManager` 的测试牌库。
- Play 模式已确认：手牌描述区显示“战吼：抽 1 张牌”。
- Play 模式已确认：打出后己方手牌通过战吼补抽 1 张。
- Play 模式已确认：打出的随从仍然正常进入战场。

## 阶段 2.5.0 已验证

- Unity 已自动生成 `Assets/Scripts/Events` 文件夹 `.meta`。
- 已创建 `GameEventType.cs`、`GameEvent.cs`、`GameEventBus.cs`。
- `GameManager` 每局开始时创建新的 `GameEventBus`。
- `GameManager` 在卡牌成功打出后发布 `CardPlayed`。
- `GameManager` 在随从成功召唤后发布 `MinionSummoned`。
- 已用 `logGameEvents` 调试开关订阅 `CardPlayed` 和 `MinionSummoned`。
- Play 模式已确认：打出随从时 Console 输出 `CardPlayed` 和 `MinionSummoned`。
- Play 模式已确认：打出法术时 Console 输出 `CardPlayed`。

## 阶段 2.2 / 2.3 / 2.4 / 2.4.5 / 2.5.0 结论

当前代码不需要推倒重来，冲锋可以作为最小关键词验证保留在现有结构中。
嘲讽也暂时可以留在 `GameManager` 的攻击目标判断里，不急着拆 `CombatResolver`。
战吼当前只做最小链路：随从召唤成功后触发一次性效果，不急着上完整 `GameEventBus`。

已验证链路：

```text
CardData 配置 Charge
-> Minion 复制关键词
-> GameManager 召唤后识别 Charge
-> 新随从 CanAttack = true
-> CardView 显示“冲锋”
-> UI 显示 Ready
```

嘲讽代码链路：

```text
CardData 配置 Taunt
-> Minion 复制关键词
-> CardView / MinionView 显示“嘲讽”
-> GameManager.TryAttackMinion 检查攻击目标是否合法
-> GameManager.TryAttackHero 检查防守方是否有活着的嘲讽随从
```

战吼代码链路：

```text
CardData 配置 BattlecryType 和 BattlecryValue
-> 玩家打出随从牌
-> GameManager 创建 Minion 并召唤到 Board
-> ResolveAfterSummon(minion)
-> ApplySummonKeywords(minion)
-> ResolveBattlecry(minion)
-> DealBattlecryDamageToEnemyHero(minion)
-> CheckGameOver()
```

需要记住的风险点：

- `GameManager` 已经负责回合、出牌、法术、攻击、死亡清理和胜负判断，后续不能无限加规则特判。
- `GameUIController` 已经负责攻击选择、法术选择、英雄点击、操作反馈和刷新，后续 UI 状态复杂时需要拆分。
- 当前法术和最小战吼直接由 `GameManager` 结算，这是阶段性简化，不是成熟项目最终做法。
- 当前冲锋、嘲讽和前两个无目标战吼不急着迁移到事件系统。事件系统先只验证出牌和召唤事件，下一步再接死亡事件。

下一步判断：

```text
CardView 和 MinionView 已能显示关键词文字。
嘲讽已经开始影响攻击目标选择。
战吼已经开始验证“召唤后触发效果”的思想。
亡语开始前，需要更认真地引入事件系统。
```

## 下一步

先做 Unity 验证：

```text
阶段 2.5.0 事件系统基础链路已完成。下一步接入 `MinionDied` 死亡事件，为亡语做准备。
```

继续写代码前，仍然先写属性清单，再动代码。
