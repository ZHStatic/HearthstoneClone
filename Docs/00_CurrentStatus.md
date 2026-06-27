# Current Status

最后更新：2026-06-27

## 当前阶段

阶段 1、阶段 1.5、阶段 2.1、阶段 2.1.5、阶段 2.2、阶段 2.3、阶段 2.4、阶段 2.4.5、阶段 2.5.0、阶段 2.5.1、阶段 2.6、阶段 2.7、阶段 2.8、阶段 2.9 已完成。
阶段 2.9 战斗日志与代码整理已经收口；阶段 2.10 的核心结构优化已完成。UI 拆分和 UI 复用刷新暂时延后，等功能更完整后统一大改。

当前停靠点：

```text
阶段 2.9 代码链路已写入：战斗日志、圣盾反馈修正、关键词显示整理
当前重点：阶段 3 AI 基础行动
已完成：文档记忆 -> 结果对象 -> Player 封装 -> 动作建模 -> 动作生成验证入口
暂缓：UI 拆分 -> UI 复用刷新
下一步：阶段 3 让 AI 复用动作模型，先能选择并执行基础动作
```

冲锋的最小链路已经测试通过：`CardData` 配置关键词，`Minion` 复制关键词，`GameManager` 在召唤后让冲锋随从立刻可以攻击，`CardView` 可以在手牌描述区显示“冲锋”。

嘲讽的代码链路已经写入：`KeywordType` 增加 `Taunt`，`GameManager` 在攻击随从和攻击英雄前检查防守方是否有活着的嘲讽随从，`CardView` 和 `MinionView` 可以显示“嘲讽”。

战吼的最小链路已经测试通过：`BattlecryType` 定义战吼类型，`CardData` 支持配置战吼类型和通用数值，`GameManager` 在随从召唤成功后调用 `ResolveAfterSummon()` 处理冲锋和战吼，`CardView` 可以在手牌描述区显示战吼文字。

事件系统基础链路已经测试通过：`GameEventType` 定义事件类型，`GameEvent` 承载事件数据，`GameEventBus` 管理订阅和发布，`GameManager` 可以发布 `CardPlayed`、`MinionSummoned` 和 `MinionDied`，Console 日志已确认监听回调会执行。

亡语的第一版代码链路已经写入：`DeathrattleType` 定义亡语类型，`CardData` 支持配置亡语类型和通用数值，`CardView` 和 `MinionView` 可以显示亡语文字，`GameManager` 监听 `MinionDied` 并在死亡随从有亡语时结算“对敌方英雄造成伤害”。
Unity Play 模式已确认：“亡语炸弹人”死亡后会触发亡语，敌方英雄生命减少 1。

圣盾的第一版代码链路已经写入并通过 Unity Play 模式验证：`KeywordType` 增加 `DivineShield`，`Minion.TakeDamage()` 在随从第一次受到正数伤害时移除圣盾并抵消该次伤害，`CardView` 和 `MinionView` 可以显示“圣盾”。

阶段 2 收尾复盘已经完成：`README.md` 已同步当前功能，`Docs/06_Stage2Review.md` 已整理阶段 2 成果、演示脚本、架构取舍和进入 AI 前检查点。

阶段 2.9 代码链路已经写入：新增 `BattleLogEntry` 和 `BattleLogger`，`GameManager` 通过 `GameManager.BattleLog.cs` 记录回合、出牌、召唤、攻击、伤害、圣盾抵消、死亡和游戏结束；`GameUIController` 的法术反馈优先读取最近一次结算日志，避免火花打到圣盾随从时误报实际伤害；`KeywordTextFormatter` 已抽出 UI 层关键词文本格式化逻辑。

阶段 2.10 第一轮 Core 操作结果标准化已经写入：新增 `GameActionFailureReason` 和 `GameActionResult`，`GameManager` 已为随从出牌和法术释放提供详细结果方法，旧 `bool Try...` 方法保留为兼容入口；`GameUIController` 的随从出牌和法术释放反馈已改为读取 Core 返回的 `GameActionResult`。

阶段 2.10 第二轮 Player 状态封装已经写入：`Player` 内部继续用 `List<Card>` 管理手牌和牌库，对外通过 `IReadOnlyList<Card>` 暴露 `Hand` / `Deck`；`GameManager` 和 `GameUIController` 已改为通过 `HasCardInHand(card)` 判断手牌归属。

阶段 2.10 第三轮动作建模已经写入：新增 `GameActionType`、`GameAction` 和 `GameActionGenerator`。`GameActionGenerator` 只读取当前局面并枚举合法动作，不执行动作、不做 AI 决策。`GameManager` 新增 `logLegalActionsOnTurnStart` 调试开关，可在回合开始后打印当前玩家合法动作列表。

阶段 2.10 第四轮动作执行闭环已经写入：`GameManager` 新增 `ExecuteAction(GameAction)`，可统一执行出牌、施法、攻击和结束回合；攻击入口新增 `TryAttackMinionDetailed()` / `TryAttackHeroDetailed()`；`GameActionGenerator` 已改为复用 `GameManager` 的出牌、施法目标、攻击和嘲讽验证，避免 AI 动作生成与 Core 执行规则重复。

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
手牌和场上随从显示亡语说明
亡语随从死亡后触发伤害
圣盾随从第一次受到伤害时抵消伤害
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
- 亡语随从：亡语炸弹人，2 费，1/1，亡语为对敌方英雄造成 1 点伤害。
- 圣盾随从：圣盾卫士，2 费，2/2，关键词为 `DivineShield`。

## 当前代码结构

### Core 层

| 文件 | 职责 |
|------|------|
| `Core/Cards/CardData.cs` | ScriptableObject 卡牌模板数据 |
| `Core/Cards/CardType.cs` | 卡牌类型：随从、法术 |
| `Core/Cards/Card.cs` | 手牌/牌库中的运行时卡牌实例 |
| `Core/Effects/SpellTargetType.cs` | 单目标法术可选择的目标范围 |
| `Core/Effects/KeywordType.cs` | 关键词类型：当前支持 `Charge`、`Taunt`、`DivineShield` |
| `Core/Effects/BattlecryType.cs` | 战吼类型：当前支持对敌方英雄造成伤害、抽牌 |
| `Core/Effects/DeathrattleType.cs` | 亡语类型：当前支持对敌方英雄造成伤害 |
| `Core/Events/GameEventType.cs` | 游戏事件类型：当前包含出牌、召唤、死亡、回合开始和回合结束 |
| `Core/Events/GameEvent.cs` | 游戏事件数据：记录事件类型和相关上下文 |
| `Core/Events/GameEventBus.cs` | 事件总线：管理事件订阅和发布 |
| `Core/Logging/BattleLogEntry.cs` | 单条战斗日志快照，记录类型、来源、目标、尝试数值、实际数值和文本 |
| `Core/Logging/BattleLogger.cs` | 本局战斗日志记录器，支持追加、查询最近日志和简单统计 |
| `Core/Actions/GameActionFailureReason.cs` | 游戏操作失败原因枚举，例如费用不足、目标非法、战场已满 |
| `Core/Actions/GameActionResult.cs` | 游戏操作结果，包含成功状态、失败原因、反馈文本和可选日志 |
| `Core/Actions/GameActionType.cs` | 游戏动作类型：出牌、施法、攻击、结束回合 |
| `Core/Actions/GameAction.cs` | 单条游戏动作数据，只记录动作意图，不执行规则 |
| `Core/Actions/GameActionGenerator.cs` | 合法动作生成器，只读取局面并创建 `GameAction` 列表 |
| `Core/Entities/Hero.cs` | 英雄生命、受伤、治疗、死亡判断 |
| `Core/Entities/Player.cs` | 手牌、牌库、法力水晶、抽牌、出牌；对外只读暴露手牌和牌库 |
| `Core/Entities/Board.cs` | 双方战场随从列表和召唤位置限制 |
| `Core/Entities/Minion.cs` | 场上随从的攻击、生命、所属玩家、攻击权限、关键词和圣盾消耗 |
| `GameManager.cs` | 当前阶段的对局流程调度，包含冲锋召唤处理、嘲讽攻击目标检查、最小战吼结算、亡语结算、基础事件发布和战斗日志入口 |
| `Core/Logging/GameManager.BattleLog.cs` | `GameManager` 的日志与伤害记录 helper，拆文件但不拆新系统 |

### UI 层

| 文件 | 职责 |
|------|------|
| `UI/Views/CardView.cs` | 显示一张手牌、关键词文字、战吼文字、亡语文字并转发点击 |
| `UI/Views/HandView.cs` | 根据手牌列表生成多个 `CardView` |
| `UI/Views/MinionView.cs` | 显示一个场上随从、关键词文字、亡语文字、点击和选中高亮 |
| `UI/Views/BoardView.cs` | 根据一方战场列表生成多个 `MinionView` |
| `UI/Controllers/GameUIController.cs` | 连接 UI 和 `GameManager`，处理点击、选择状态、反馈和刷新 |
| `UI/Formatters/KeywordTextFormatter.cs` | UI 层关键词文本格式化工具，供 `CardView` 和 `MinionView` 复用 |

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
| `Docs/06_Stage2Review.md` | 阶段 2 收尾复盘、演示脚本和进入 AI 前检查点 |
| `Docs/Learning/` | 学习笔记，不要求和正式架构文档完全同步 |

## 固定巡检方法

做较大范围代码整理、架构调整或 UI 反馈修正前，先执行：

```powershell
& 'C:/Users/Static/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe' '.codex/skills/hearthstone-code-review/scripts/find_review_candidates.py'
```

巡检重点：

- `TakeDamage()` / `Heal()` 返回值是否被忽略。
- UI 是否显示 Core 实际结算结果，而不是预估结果。
- 关键词、战吼、亡语、状态文本 formatter 是否重复。
- Core 是否仍然不依赖 UI。
- `GameManager` 和 `GameUIController` 是否继续膨胀。
- 中文文档用 PowerShell 读取时使用 `-Encoding UTF8`。

## 已确认

- Unity Play 模式可以运行。
- 牌库中配置有效 `CardData` 后，手牌可以显示。
- 空的 `CardData` 会被 `Player` 跳过，避免开局空引用。
- 随从牌可以通过 `GameManager.TryPlayMinionCard(card)` 召唤。
- 配置了 `Charge` 的随从召唤后会立刻进入可攻击状态。
- 手牌和场上随从可以显示“冲锋”“嘲讽”等关键词文字。
- 手牌可以显示“战吼：对敌方英雄造成 X 点伤害”和“战吼：抽 X 张牌”。
- 代码已支持手牌显示“亡语：对敌方英雄造成 X 点伤害”。
- 代码已支持场上随从显示“亡语:X”。
- 亡语炸弹人的亡语已在 Play 模式测试通过：死亡后敌方英雄减少 1 点生命。
- 火焰学徒的战吼已在 Play 模式测试通过：打出后敌方英雄立刻减少 1 点生命。
- 书卷侍从的战吼已在 Play 模式测试通过：打出后己方抽 1 张牌。
- Unity Play 模式已确认：手牌和场上随从可以显示“圣盾”。
- Unity Play 模式已确认：圣盾随从第一次受到正数伤害时抵消该次伤害并失去圣盾，第二次伤害正常扣血。
- 阶段 2.9 新增脚本已通过 Unity 编译检查，未出现编译错误。
- `GameManager` 已不再忽略 `TakeDamage()` 的返回值，伤害 helper 会记录尝试伤害和实际伤害。
- `GameUIController` 法术成功反馈已改为显示 Core 返回的 `GameActionResult.Message`，避免 UI 直接猜测实际结算结果。
- `CardView` 和 `MinionView` 已复用 `KeywordTextFormatter` 显示关键词。
- `GameManager.TryPlayMinionCardDetailed()`、`TryPlaySpellCardOnMinionDetailed()`、`TryPlaySpellCardOnHeroDetailed()` 已接入详细操作结果。
- `Player.Hand` / `Player.Deck` 已改为只读列表，外部不能直接 `Add` / `Remove` 手牌或牌库。
- `GameManager` 和 `GameUIController` 已通过 `Player.HasCardInHand(card)` 判断手牌归属。
- `GameActionGenerator.GenerateLegalActions(gameManager)` 已能枚举当前玩家的出牌、施法、攻击和结束回合动作。
- `GameActionGenerator` 已复用 `GameManager` 验证方法，不再重复维护出牌、法术目标、攻击和嘲讽规则。
- `GameManager.ExecuteAction(GameAction)` 已能统一执行出牌、施法、攻击和结束回合动作。
- `GameManager` 已新增 `Log Legal Actions On Turn Start` 调试开关，用于 Play Mode Console 验证动作生成结果。
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

- 当时 Unity 已自动生成事件脚本文件夹 `.meta`；当前事件脚本已随目录整理移动到 `Assets/Scripts/Core/Events`。
- 已创建 `GameEventType.cs`、`GameEvent.cs`、`GameEventBus.cs`。
- `GameManager` 每局开始时创建新的 `GameEventBus`。
- `GameManager` 在卡牌成功打出后发布 `CardPlayed`。
- `GameManager` 在随从成功召唤后发布 `MinionSummoned`。
- 已用 `logGameEvents` 调试开关订阅 `CardPlayed` 和 `MinionSummoned`。
- Play 模式已确认：打出随从时 Console 输出 `CardPlayed` 和 `MinionSummoned`。
- Play 模式已确认：打出法术时 Console 输出 `CardPlayed`。

## 阶段 2.5.1 已验证

- `GameManager` 在随从死亡清理时发布 `MinionDied`。
- `MinionDied` 事件把死亡随从写入 `TargetMinion`。
- `logGameEvents` 调试日志已订阅 `MinionDied`。
- Play 模式已确认：随从死亡时 Console 输出 `MinionDied`，并显示死亡随从名字。

## 阶段 2.6 已验证

- 已新增 `DeathrattleType.cs`，当前包含 `None` 和 `DealDamageToEnemyHero`。
- `CardData` 已支持配置 `Deathrattle Type` 和 `Deathrattle Value`。
- `CardView` 已支持显示“亡语：对敌方英雄造成 X 点伤害”。
- `MinionView` 已支持显示“亡语:X”。
- `GameManager` 已注册规则事件监听，收到 `MinionDied` 后会尝试结算死亡随从的亡语。
- 当前第一个亡语效果：对死亡随从拥有者的敌方英雄造成 `DeathrattleValue` 点伤害。
- Unity Play 模式已确认：亡语炸弹人死亡后，Console 输出 `MinionDied`，敌方英雄生命减少 1，死亡随从从战场移除。

## 阶段 2.7 已验证

- 已在 `KeywordType` 中新增 `DivineShield`。
- `Minion` 已新增 `HasDivineShield`。
- `Minion.TakeDamage()` 已支持圣盾抵消第一次正数伤害，并移除 `DivineShield`。
- `CardView` 已支持手牌显示“圣盾”。
- `MinionView` 已支持场上随从显示“圣盾”。
- Unity Play 模式已确认：第一次伤害被抵消并移除圣盾，第二次伤害正常扣血。

## 阶段 2.8 已完成

- 已更新 `README.md`，同步阶段 2 当前能力、测试卡牌、演示路径和架构重点。
- 已新增 `Docs/06_Stage2Review.md`，集中记录阶段 2 成果、关键代码链路、架构取舍、5 分钟演示脚本和进入 AI 前检查点。
- 阶段 2.10 已补完进入阶段 3 前的核心结构优化，UI 大改暂缓到功能更完整后。
- 本阶段不改 C# 规则代码，只做文档和项目状态收口。

## 阶段 2.9 代码链路已写入

- 已新增 `BattleLogEntry.cs` 和 `BattleLogger.cs`，用于记录本局战斗日志。
- 已把 `GameManager` 改为 `partial`，并新增 `GameManager.BattleLog.cs` 存放日志和伤害记录 helper。
- `DamageMinion()` / `DamageHero()` 会包装 `TakeDamage()`，记录尝试伤害和实际伤害。
- 圣盾抵消时会记录 `DivineShieldPrevented` 日志，并把它作为最近一次操作反馈。
- `GameUIController` 法术反馈已优先使用 `LastActionLogEntry.Message`，避免火花打圣盾时误报“造成 2 点伤害”。
- 已新增 `KeywordTextFormatter.cs`，`CardView` 和 `MinionView` 复用同一套关键词文本格式化逻辑。
- 项目专用扫描脚本已确认：当前没有忽略 `TakeDamage()` 返回值、误导性法术伤害反馈、重复关键词 formatter 或关键词字符串手动拼接候选。
- Unity 已确认无编译错误；阶段 2.9 作为代码整理阶段已收口。
- 后续做结构和 UI 优化前，仍需把火花打圣盾、随从攻击圣盾、战吼伤害、亡语伤害、死亡和游戏结束日志顺序作为回归验证清单。

## 阶段 2.10 当前结论

阶段 2.10 本轮先完成进入阶段 3 前最关键的结构优化：

```text
1. 文档记忆同步：已完成
2. 结果对象：已完成
3. Player 封装：已完成
4. 动作建模：已完成
5. 动作生成验证入口：已完成
6. UI 拆分：暂缓
7. UI 复用刷新：暂缓
```

计划新增或调整的代码：

| 方向 | 目标 |
|------|------|
| `GameActionFailureReason` / `GameActionResult` | 已接入：让 Core 返回明确失败原因和反馈文本，UI 不再只根据 `bool` 猜测 |
| `Player` 状态封装 | 已接入：`Hand` / `Deck` 对外只读，外部不能直接修改手牌和牌库 |
| `GameActionType` / `GameAction` / `GameActionGenerator` | 已接入：只描述和枚举合法动作，不写 AI 决策 |
| `GameManager.ExecuteAction(GameAction)` | 已接入：统一执行动作，供玩家输入和 AI 复用 |
| 动作生成验证 | 已接入：`GameManager` 可在回合开始打印合法动作列表，默认关闭 |
| UI 拆分 | 暂缓：功能更完整后统一大改 |
| UI 复用刷新 | 暂缓：功能更完整后统一大改 |

注意：

- 这些改动涉及 C# 新类或较大调整时，仍然先列属性清单，再写代码。
- Prefab / Scene 布局不由 Codex 直接改；新增 Text、绑定字段、字号颜色和位置优先在 Unity Editor 中完成。

## 阶段 2 结论

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

亡语代码链路：

```text
CardData 配置 DeathrattleType 和 DeathrattleValue
-> 玩家打出随从牌
-> 随从进入战场
-> 随从死亡
-> CleanupDeadMinions()
-> PublishMinionDied(minion)
-> EventBus 通知 ResolveDeathrattleOnMinionDied()
-> ResolveDeathrattle(minion)
-> DealDeathrattleDamageToEnemyHero(minion)
-> CheckGameOver()
```

圣盾代码链路：

```text
CardData 配置 DivineShield
-> Minion 复制关键词
-> CardView / MinionView 显示“圣盾”
-> GameManager.DamageMinion(...) 调用 Minion.TakeDamage(amount)
-> 如果有圣盾，RemoveKeyword(DivineShield)
-> 本次实际伤害返回 0，CurrentHealth 不减少
-> BattleLogger 记录“尝试伤害”和“实际伤害 0”
-> GameActionResult.Message 把本次结算反馈交给 UI 显示
-> 下一次受到伤害时正常扣血
```

需要记住的风险点：

- `GameManager` 已经负责回合、出牌、法术、攻击、死亡清理和胜负判断，后续不能无限加规则特判。
- `GameUIController` 已经负责攻击选择、法术选择、英雄点击、操作反馈和刷新，后续 UI 状态复杂时需要拆分。
- 当前法术和最小战吼直接由 `GameManager` 结算，这是阶段性简化，不是成熟项目最终做法。
- 当前圣盾直接写在 `Minion.TakeDamage()`，这是阶段性简化。后续如果出现免疫、法术伤害加成、吸血、伤害翻倍等机制，应抽出 `DamageResolver` 或 `CombatResolver`。
- 当前冲锋、嘲讽和前两个无目标战吼不急着迁移到事件系统。事件系统已经验证出牌、召唤和死亡事件，亡语已经开始通过 `MinionDied` 事件触发。

下一步判断：

```text
CardView 和 MinionView 已能显示关键词文字。
CardView 和 MinionView 已复用 KeywordTextFormatter。
嘲讽已经开始影响攻击目标选择。
战吼已经开始验证“召唤后触发效果”的思想。
亡语已经开始验证“死亡后触发效果”的思想。
圣盾已经开始验证“受到伤害时修改伤害结果”的思想。
战斗日志已经开始验证“规则结算可观测性”的思想。
```

## 下一步

阶段 2.10 本轮核心优化已经完成，下一步进入阶段 3：

```text
阶段 3.0：AI 基础行动
目标：让 AI 复用 GameActionGenerator 枚举合法动作，再选择一条基础动作执行。
第一版先追求“能自动行动”，不做评分函数和搜索。

UI 拆分 / UI 复用刷新：
暂缓到功能更完整后统一整理。
```

继续写代码前，仍然先写属性清单，再动代码。
