# Current Status

最后更新：2026-07-04

## 当前阶段

阶段 1、阶段 1.5、阶段 2.1、阶段 2.1.5、阶段 2.2、阶段 2.3、阶段 2.4、阶段 2.4.5、阶段 2.5.0、阶段 2.5.1、阶段 2.6、阶段 2.7、阶段 2.8、阶段 2.9 已完成。
阶段 2.9 战斗日志与代码整理已经收口；阶段 2.10 的核心结构优化已完成。阶段 3 已完成 AI 基础行动、第一版动作选择、策略验证入口、第一版评估函数、快照模拟基础链路、评分优先级动作选择收口、保守度调校入口、出随从模拟补强、亡语伤害模拟、同回合后续攻击预估、具体手牌快照和基于手牌快照的继续出牌模拟。阶段 3.13 英雄技能最小闭环已完成：Core 规则、动作系统、AI 快照模拟链路、玩家 UI 代码入口、Unity 绑定和 Play 模式验证均已收口。阶段 4 UI / 交互 / 表现打磨已完成规划；阶段 4.1 反馈文本和操作状态整理已完成；阶段 4.2 合法目标高亮和按钮视觉状态已完成代码链路和 Play 模式验证；阶段 4.3 炉石式战斗界面信息层级已完成 Editor 布局调整和 Play 模式验收；阶段 4.4 卡牌和随从显示拆分已完成代码、Prefab 绑定和 Play 模式验收。

当前停靠点：

```text
阶段 3.13 已完成：英雄技能最小闭环
阶段 4.1 已完成：反馈文本和操作状态整理
阶段 4.2 已完成：合法目标高亮和按钮视觉状态
阶段 4.3 已完成：炉石式战斗界面信息层级
阶段 4.4 已完成：卡牌和随从显示拆分
当前重点：阶段 4.5 基础动画和音效
已完成：FeedbackText / GameOverText 拆分；GameOverText 只显示胜负；FeedbackText 显示普通操作反馈；selectedAttacker、selectedSpellCard、isSelectingHeroSkillTarget 三种 UI 操作状态已集中整理；法术、攻击、英雄技能的主要规则失败反馈已改为读取 Core 返回的 GameActionResult.Message；攻击、法术、英雄技能选目标时会基于 Core 验证结果显示合法/非法目标高亮；英雄、英雄技能和结束回合按钮已有基础视觉状态；`BattlePrototype` 已完成阶段 4.3 Canvas 信息层级布局调整；阶段 4.4 已新增 `CardTextFormatter`，手牌卡牌使用 `TypeText` / `EffectText` 拆分卡牌类型和规则文本，场上随从使用 `StatusText` / `KeywordText` / `DeathrattleText` 拆分 Ready、关键词和亡语
已观察：Play 模式中 AI 连续出牌和连续攻击行为基本合理；玩家和 AI 都能正常使用英雄技能；阶段 4.3 布局验收中出牌、攻击、法术、英雄技能、结束回合和反馈显示均正常；阶段 4.4 中随从牌、法术牌、Ready、关键词和亡语显示已通过 Prefab 绑定和 Play 模式验证
已整理：阶段 3 AI v1 回归清单见 `Docs/08_AIReview.md`；圣盾 AI 攻击项未自然覆盖，但不视为失败；阶段 4.3 Editor 布局执行清单和阶段 4.4 Prefab 绑定清单见 `Docs/03_UIArchitecture.md`
已规划：阶段 4.1 反馈文本和操作状态 -> 阶段 4.2 合法目标高亮和按钮状态 -> 阶段 4.3 炉石式信息层级 -> 阶段 4.4 卡牌和随从显示拆分 -> 阶段 4.5 基础动画和音效 -> 阶段 4.6 View 复用和性能意识
后续路线：英雄技能最小闭环 -> UI / 交互 / 表现重点打磨 -> 主流程与套牌选择 -> 移动端 / 性能意识补强 -> 数据配置和测试工具 -> 求职包装
下一步：进入阶段 4.5，先列基础动画和音效的学习点与属性清单；目标是让出牌、攻击、受伤、死亡和回合切换有基础表现节奏，但动画和音效只表现 Core 已完成的规则结果
学习节奏：每个小任务开始前先列必学点和属性清单，确认后再写代码
```

冲锋的最小链路已经测试通过：`CardData` 配置关键词，`Minion` 复制关键词，`GameManager` 在召唤后让冲锋随从立刻可以攻击，`CardView` 可以在手牌描述区显示“冲锋”。

嘲讽的代码链路已经写入：`KeywordType` 增加 `Taunt`，`GameManager` 在攻击随从和攻击英雄前检查防守方是否有活着的嘲讽随从，`CardView` 和 `MinionView` 可以显示“嘲讽”。

战吼的最小链路已经测试通过：`BattlecryType` 定义战吼类型，`CardData` 支持配置战吼类型和通用数值，`GameManager` 在随从召唤成功后调用 `ResolveAfterSummon()` 处理冲锋和战吼，`CardView` 可以在手牌描述区显示战吼文字。

事件系统已经精简为只保留规则需要的 `MinionDied`。历史上的 `CardPlayed` / `MinionSummoned` 调试事件和 `GameEvent:` Console 日志已删除，避免干扰 AI 行动日志。

亡语的第一版代码链路已经写入：`DeathrattleType` 定义亡语类型，`CardData` 支持配置亡语类型和通用数值，`CardView` 和 `MinionView` 可以显示亡语文字，`GameManager` 监听 `MinionDied` 并在死亡随从有亡语时结算“对敌方英雄造成伤害”。
Unity Play 模式已确认：“亡语炸弹人”死亡后会触发亡语，敌方英雄生命减少 1。

圣盾的第一版代码链路已经写入并通过 Unity Play 模式验证：`KeywordType` 增加 `DivineShield`，`Minion.TakeDamage()` 在随从第一次受到正数伤害时移除圣盾并抵消该次伤害，`CardView` 和 `MinionView` 可以显示“圣盾”。

阶段 2 收尾复盘已经完成：`README.md` 已同步当前功能，`Docs/06_Stage2Review.md` 已整理阶段 2 成果、演示脚本、架构取舍和进入 AI 前检查点。

阶段 2.9 代码链路已经写入：新增 `BattleLogEntry` 和 `BattleLogger`，`GameManager` 通过 `GameManager.BattleLog.cs` 记录回合、出牌、召唤、攻击、伤害、圣盾抵消、死亡和游戏结束；`GameUIController` 的法术反馈优先读取最近一次结算日志，避免火花打到圣盾随从时误报实际伤害；`KeywordTextFormatter` 已抽出 UI 层关键词文本格式化逻辑。

阶段 2.10 第一轮 Core 操作结果标准化已经写入：新增 `GameActionFailureReason` 和 `GameActionResult`，`GameManager` 已为随从出牌和法术释放提供详细结果方法，旧 `bool Try...` 方法保留为兼容入口；`GameUIController` 的随从出牌和法术释放反馈已改为读取 Core 返回的 `GameActionResult`。

阶段 2.10 第二轮 Player 状态封装已经写入：`Player` 内部继续用 `List<Card>` 管理手牌和牌库，对外通过 `IReadOnlyList<Card>` 暴露 `Hand` / `Deck`；`GameManager` 和 `GameUIController` 已改为通过 `HasCardInHand(card)` 判断手牌归属。

阶段 2.10 第三轮动作建模已经写入：新增 `GameActionType`、`GameAction` 和 `GameActionGenerator`。`GameActionGenerator` 只读取当前局面并枚举合法动作，不执行动作、不做 AI 决策。`GameManager` 新增 `logLegalActionsOnTurnStart` 调试开关，可在回合开始后打印当前玩家合法动作列表。

阶段 2.10 第四轮动作执行闭环已经写入：`GameManager` 新增 `ExecuteAction(GameAction)`，可统一执行出牌、施法、攻击和结束回合；攻击入口新增 `TryAttackMinionDetailed()` / `TryAttackHeroDetailed()`；`GameActionGenerator` 已改为复用 `GameManager` 的出牌、施法目标、攻击和嘲讽验证，避免 AI 动作生成与 Core 执行规则重复。

阶段 3.0 / 3.1 基础 AI 行动已经写入：新增 `AIController` 和 `ActionSelector`，Enemy 回合开始后会复用 `GameActionGenerator` 生成合法动作，再通过 `GameManager.ExecuteAction(GameAction)` 执行动作，直到结束回合、游戏结束或达到行动上限。

阶段 3.2 第一版动作选择策略已经写入：`ActionSelector` 会优先选择可击杀敌方英雄的动作，其次选择可击杀敌方随从的动作，再选择可出牌动作，最后按固定优先级兜底。`AIActionSelection` 和 `AIActionSelectionReason` 会记录 AI 选择原因，`AIController` 在 Console 中输出单行 AI 行动日志，包含行动、理由和结果。

阶段 3.3 AI 策略验证入口已经写入：`GameManager` 增加 `Disable Deck Shuffle For Debug` 和 `Log AI Hand On Turn Start` 调试开关；`Player` 支持关闭创建时洗牌，方便用 Inspector 中的 Enemy Deck Data 稳定复现 AI 起手。`ActionSelector` 已调整为避免主动伤害自己、优先打高费牌、非击杀法术优先打敌方英雄，并在多个可击杀随从中优先处理高攻击力、低伤害溢出的目标。

阶段 3.4 第一版评估函数已经写入：新增 `Evaluator` 和 `EvaluationResult`，评分由英雄血量、手牌数量和场面随从三部分组成。`AIController` 的行动日志会输出行动前后总分变化和评分明细，方便观察 AI 动作是否真的让局面对自己变好。

阶段 3.5 快照模拟基础链路已经写入：新增 `GameStateSnapshot`、`PlayerSnapshot`、`MinionSnapshot`、`BoardSnapshot`、`SnapshotAction`、`SnapshotActionMapper` 和 `SnapshotSimulator`。`GameManager` 新增真实评分 vs 快照评分验证入口，以及合法动作快照模拟后评分日志入口；Unity 编译检查已通过。

阶段 3.6 评分优先级动作选择已经收口：`ActionSelector` 先保留斩杀硬规则，其余动作通过 `SnapshotSimulator` 单步模拟并由 `Evaluator` 评分；`AIActionSelection` 会携带被选动作的模拟评分，`AIController` 日志会同时显示模拟评分和真实执行前后评分。`SnapshotSimulator` 的 `EndTurn` 已补齐对手回合开始的关键状态变化，包括抽牌、法力刷新和随从恢复攻击。阶段 3.6 时 AI 只主动执行不降低评分的动作；阶段 3.7 已放宽为允许小幅亏分。

阶段 3.7 AI 保守度调校入口已经写入：`ActionSelector` 增加 `AllowedScoreLoss = 3`，允许主动动作在快照模拟后小幅亏分；如果被选动作低于当前评分但没有超过阈值，`AIActionSelectionReason.AcceptableScoreLoss` 会让 `AIController` 在日志里说明“允许小亏换节奏”。当前没有改 `Evaluator` 权重，方便先单独观察选择门槛的影响。

阶段 3.8 出随从快照模拟补强已经写入：`SnapshotAction` 会记录打出随从牌的嘲讽、圣盾、冲锋和战吼信息；`SnapshotActionMapper` 会从 `CardData` 读取这些信息；`SnapshotSimulator` 在模拟打出随从时会创建带关键词的 `MinionSnapshot`，冲锋随从会立即可攻击，并会模拟“对敌方英雄造成伤害”和“为出牌者抽牌”两类无目标战吼。

阶段 3.9 亡语伤害快照模拟已经写入：`MinionSnapshot` 会记录亡语类型和数值；`SnapshotAction` / `SnapshotActionMapper` 会把打出随从牌的亡语配置带入新随从；`SnapshotSimulator` 在法术击杀随从、随从攻击随从后，会在移除死亡随从前模拟一层“对死亡随从拥有者的敌方英雄造成伤害”的亡语。当前仍不模拟亡语触发新的亡语连锁。

阶段 3.10 同回合后续攻击预估已经写入：新增 `SnapshotFollowUpEvaluator`，在候选动作模拟后继续粗略模拟最多 2 次同回合随从攻击；当前只生成已有随从的攻击随从/攻击英雄动作，遵守嘲讽限制，不模拟后续出牌和对手回合。`ActionSelector` 仍保留斩杀硬规则、允许小亏阈值和原有选择原因，但候选动作排序使用“当前动作 + 后续攻击预估”后的最高评分。

阶段 3.11 具体手牌快照已经写入：新增 `CardSnapshot`，复制手牌卡牌的类型、费用、攻血、法术目标、关键词、战吼和亡语信息；`PlayerSnapshot` 新增 `HandCards`，会从真实 `Player.Hand` 复制已知手牌快照。当前 `SnapshotSimulator` 在抽牌时只增加手牌数量，因为快照还不知道牌库顶是哪张；在当前模拟层花费卡牌时会丢弃具体手牌列表，只保留数量，避免没有手牌索引时误用已经打出的牌。

阶段 3.12 继续出牌模拟已经写入：`SnapshotAction` 新增 `HandCardIndex`，`SnapshotActionMapper` 会尽量把真实动作中的手牌引用映射成手牌索引；`SnapshotSimulator` 在有具体手牌索引时会移除对应 `CardSnapshot`，找不到索引时只保留手牌数量；`SnapshotFollowUpEvaluator` 现在会基于 `PlayerSnapshot.HandCards` 生成后续出随从和伤害法术动作，并继续和攻击动作一起做最多 2 层同回合预估。
Play 模式已观察：AI 连续出牌和连续攻击选择基本合理；当前不急着增加搜索日志或扩大搜索深度。
阶段 3 AI v1 回归结果已整理到 `Docs/08_AIReview.md`。其中圣盾处理没有在 AI 自然对局中出现，因为 AI 一般不会主动攻击圣盾随从；该项记录为“未自然覆盖”，不视为失败。

阶段 3.13 英雄技能最小闭环已完成：`Player` 记录英雄技能费用、伤害和本回合使用状态；`GameActionType` / `GameAction` 支持英雄技能打随从和打英雄；`GameManager` 支持 2 费、每回合一次、对敌方角色造成 1 点伤害的英雄技能结算；`GameActionGenerator` 会把合法英雄技能动作加入动作列表；`BattleLogEntryType` 支持 `HeroSkill` 日志类型；AI 快照链路已能映射、模拟和后续预估英雄技能，`ActionSelector` 可把英雄技能纳入斩杀和评分选择，`AIController` 日志能显示“使用英雄技能”；`GameUIController` 已新增英雄技能按钮入口和“选择英雄技能目标”的 UI 状态。
Unity Play 模式已确认：玩家和 AI 都能正常使用英雄技能。

阶段 4.1 反馈文本和操作状态整理已写入：`GameUIController` 新增 `feedbackText`，普通操作反馈与 `gameOverText` 分离；`gameOverText` 未结束时保持隐藏，只在对局结束时显示胜负。`ClearOperationSelection()` 集中清空攻击、法术和英雄技能三种 UI 操作状态；`GetCurrentOperationText()` 会在当前玩家文本中显示“攻击 / 法术 / 英雄技能”操作状态。法术选牌、攻击者选择、法术目标结算、英雄技能目标结算、攻击目标结算等主要规则失败反馈已改为读取 `GameActionResult.Message`；UI 只保留“请选择目标”“请先选择攻击者 / 法术 / 英雄技能”等操作状态文案。英雄技能按钮当前在游戏未结束时保持可点击，以便费用不足或本回合已使用时能显示 Core 返回的失败原因；不可用视觉表现留到阶段 4.2 和按钮状态一起处理。

阶段 4.2 合法目标高亮和按钮状态已完成：新增 `TargetHighlightState` 作为 UI 高亮状态枚举；`MinionView` 支持普通、选中、合法目标、非法目标颜色；`BoardView` 支持由 `GameUIController` 为每个随从传入高亮状态；`GameUIController` 在攻击、法术和英雄技能选目标时复用 `GameManager.Validate...` 结果显示合法/非法高亮。英雄按钮、英雄技能按钮和结束回合按钮已有基础视觉状态；英雄技能按钮在游戏未结束时仍保持可点击，用于显示费用不足或本回合已使用等 Core 返回的失败原因。

阶段 4.3 炉石式战斗界面信息层级已完成：`BattlePrototype` 的 Canvas 信息区域已在 Unity Editor 中整理，敌方区、战场区、玩家操作区、反馈区和胜负提示区的职责更清楚。Play 模式已确认：出牌、攻击、法术、英雄技能、结束回合、AI 行动、普通反馈和胜负显示均正常。阶段 4.3 不新增 C# 类，位置、字号、颜色和边距继续作为 Editor / Prefab 配置管理。

后续计划已经按求职导向重新排序：先补英雄技能，让对局完整度更像炉石类项目；再把 UI、交互反馈、动画和主流程作为重点；随后补移动端性能意识、数据配置工具和求职包装。这个路线的目标是让项目从“规则系统原型”转向“可展示的商业项目 Demo”。

## 阶段 4 执行计划摘要

阶段 4 第一目标是演示清晰度，视觉信息层级参考炉石，但不复刻商业美术资源。第一轮继续使用当前 UGUI 和 `Text`，暂不整体迁移 TextMeshPro。

执行顺序：

```text
阶段 4.1：反馈文本和操作状态整理
阶段 4.2：合法目标高亮和按钮状态
阶段 4.3：炉石式战斗界面信息层级
阶段 4.4：卡牌和随从显示拆分
阶段 4.5：基础动画和音效
阶段 4.6：View 复用和性能意识
```

阶段 4.1 第一刀：

```text
已写入：拆分 FeedbackText 和 GameOverText
已写入：普通反馈显示费用不足、目标非法、请选择目标、圣盾抵消等操作结果
已写入：GameOverText 只显示胜负结果
已写入：整理 selectedAttacker、selectedSpellCard、isSelectingHeroSkillTarget 三种 UI 操作状态
已写入：保持 UI 只显示和转发输入，不直接修改 Core 规则状态
```

学习节奏：

```text
每个小任务开始前，先列“做之前先学什么”
每个要动的 C# 类，先列属性清单和故意不写的内容
用户确认后再写代码
Prefab、Scene、ScriptableObject、图片、音频和 .meta 默认不由 Codex 直接改
视觉布局、字号、颜色、边距优先由 Unity Editor / Prefab 调整
```

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
玩家英雄技能对敌方随从或敌方英雄造成伤害
费用不足和非法操作提示
攻击、法术和英雄技能选目标时显示合法/非法目标高亮
英雄技能、英雄和结束回合按钮显示基础视觉状态
随从攻击随从
随从攻击英雄
结束回合
胜负判定
Enemy AI 自动行动
AI 行动原因日志
AI 评分明细日志
快照评分验证日志
合法动作快照模拟评分日志
AI 被选动作模拟评分日志
AI 评分优先级动作选择
AI 允许小亏换节奏的选择原因日志
AI 出随从时的关键词和无目标战吼快照模拟
AI 法术和随从攻击导致死亡时的一层亡语伤害快照模拟
AI 同回合后续攻击收益预估
AI 具体手牌快照数据
AI 同回合继续出牌收益预估
AI 使用英雄技能
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
| `Core/Events/GameEventType.cs` | 游戏事件类型：当前只保留 `MinionDied` |
| `Core/Events/GameEvent.cs` | 游戏事件数据：当前只记录事件类型和死亡随从 |
| `Core/Events/GameEventBus.cs` | 事件总线：管理事件订阅和发布，当前用于亡语 |
| `Core/Logging/BattleLogEntry.cs` | 单条战斗日志快照，记录类型、来源、目标、尝试数值、实际数值和文本；当前已包含英雄技能日志类型 |
| `Core/Logging/BattleLogger.cs` | 本局战斗日志记录器，支持追加、查询最近日志和简单统计 |
| `Core/Actions/GameActionFailureReason.cs` | 游戏操作失败原因枚举，例如费用不足、目标非法、战场已满、英雄技能本回合已使用 |
| `Core/Actions/GameActionResult.cs` | 游戏操作结果，包含成功状态、失败原因、反馈文本和可选日志 |
| `Core/Actions/GameActionType.cs` | 游戏动作类型：出牌、施法、攻击、英雄技能、结束回合 |
| `Core/Actions/GameAction.cs` | 单条游戏动作数据，只记录动作意图，不执行规则；当前支持英雄技能打随从和打英雄 |
| `Core/Actions/GameActionGenerator.cs` | 合法动作生成器，只读取局面并创建 `GameAction` 列表；当前会生成合法英雄技能动作 |
| `Core/Entities/Hero.cs` | 英雄生命、受伤、治疗、死亡判断 |
| `Core/Entities/Player.cs` | 手牌、牌库、法力水晶、抽牌、出牌和英雄技能使用状态；对外只读暴露手牌和牌库 |
| `Core/Entities/Board.cs` | 双方战场随从列表和召唤位置限制 |
| `Core/Entities/Minion.cs` | 场上随从的攻击、生命、所属玩家、攻击权限、关键词和圣盾消耗 |
| `GameManager.cs` | 当前阶段的对局流程调度，包含 AI 回合触发、冲锋召唤处理、嘲讽攻击目标检查、最小战吼结算、亡语结算、英雄技能结算和战斗日志入口 |
| `GameManager.BattleLog.cs` | `GameManager` 的日志与伤害记录 helper，拆文件但不拆新系统 |

### AI 层

| 文件 | 职责 |
|------|------|
| `AI/AIController.cs` | AI 回合控制器，负责生成合法动作、选择动作、执行动作并打印 AI 日志；当前能显示英雄技能动作 |
| `AI/ActionSelector.cs` | AI 动作选择器，保留斩杀硬规则，其余动作按快照模拟评分选择，并允许小幅亏分换节奏；当前会把英雄技能纳入斩杀和评分选择 |
| `AI/AIActionSelection.cs` | AI 动作选择结果，包含最终动作、选择原因和可选模拟评分 |
| `AI/AIActionSelectionReason.cs` | AI 选择原因枚举，用于说明斩杀、评分最高动作、允许小亏、无收益结束回合或兜底选择 |
| `AI/Evaluator.cs` | AI 局面评估函数，可以评分真实局面和快照局面 |
| `AI/EvaluationResult.cs` | 评分明细结果，记录英雄血量、手牌、场面和总分 |
| `AI/Simulation/GameStateSnapshot.cs` | 对局快照根对象，用于脱离真实局面做模拟 |
| `AI/Simulation/PlayerSnapshot.cs` | 玩家快照，记录英雄血量、法力、手牌数量和本回合英雄技能使用状态 |
| `AI/Simulation/CardSnapshot.cs` | 手牌卡牌快照，记录模拟出牌所需的卡牌类型、费用、数值、关键词、战吼和亡语 |
| `AI/Simulation/MinionSnapshot.cs` | 随从快照，记录随从攻血、攻击权限、关键词和亡语数据 |
| `AI/Simulation/BoardSnapshot.cs` | 战场快照，保存双方随从列表 |
| `AI/Simulation/SnapshotAction.cs` | 快照动作，描述可以在快照上模拟的动作；出随从动作会携带关键词、战吼和亡语数据，英雄技能动作会携带技能伤害 |
| `AI/Simulation/SnapshotActionMapper.cs` | 将真实 `GameAction` 映射成快照动作，并从 `CardData` 读取出随从模拟需要的数据；当前支持英雄技能动作映射 |
| `AI/Simulation/SnapshotSimulator.cs` | 单步快照模拟器，执行快照动作并返回新快照；`EndTurn` 会模拟回合开始状态，出随从会模拟关键词和无目标战吼，随从死亡会模拟一层亡语伤害，英雄技能会模拟扣费、伤害和使用状态 |
| `AI/Simulation/SnapshotFollowUpEvaluator.cs` | 同回合后续预估器，在快照层继续模拟少量已有随从攻击、继续出牌和英雄技能，用于减少 AI 单步评分短视 |

### UI 层

| 文件 | 职责 |
|------|------|
| `UI/Views/CardView.cs` | 显示一张手牌、关键词文字、战吼文字、亡语文字并转发点击 |
| `UI/Views/HandView.cs` | 根据手牌列表生成多个 `CardView` |
| `UI/Views/TargetHighlightState.cs` | UI 目标高亮状态枚举：普通、合法、非法、选中 |
| `UI/Views/MinionView.cs` | 显示一个场上随从、关键词文字、亡语文字、点击和目标高亮 |
| `UI/Views/BoardView.cs` | 根据一方战场列表生成多个 `MinionView`，并接收上层传入的目标高亮状态 |
| `UI/Controllers/GameUIController.cs` | 连接 UI 和 `GameManager`，处理点击、选择状态、英雄技能目标选择、反馈、目标高亮和按钮状态刷新 |
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
| `Docs/08_AIReview.md` | 阶段 3 AI v1 回归清单、演示观察点和阶段性简化 |
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
- Unity Play 模式已确认：玩家和 AI 都能正常使用英雄技能。
- 阶段 2.9 新增脚本已通过 Unity 编译检查，未出现编译错误。
- `GameManager` 已不再忽略 `TakeDamage()` 的返回值，伤害 helper 会记录尝试伤害和实际伤害。
- `GameUIController` 法术成功反馈已改为显示 Core 返回的 `GameActionResult.Message`，避免 UI 直接猜测实际结算结果。
- `CardView` 和 `MinionView` 已复用 `KeywordTextFormatter` 显示关键词。
- `GameManager.TryPlayMinionCardDetailed()`、`TryPlaySpellCardOnMinionDetailed()`、`TryPlaySpellCardOnHeroDetailed()` 已接入详细操作结果。
- 阶段 4.1 已确认：`FeedbackText` 和 `GameOverText` 分离，普通反馈不再占用游戏结束文本。
- 阶段 4.1 已确认：法术、攻击和英雄技能的主要规则失败反馈由 `GameActionResult.Message` 提供，UI 不再自己重复判断费用不足、目标非法、嘲讽限制或游戏结束等规则结果。
- 阶段 4.1 已确认：UI 仍然自己处理“已选择 X，请选择目标”“请先选择攻击者 / 法术 / 英雄技能”等操作状态文案。
- 阶段 4.2 已确认：攻击、法术和英雄技能选目标时，随从和英雄目标会基于 Core 验证结果显示合法/非法高亮。
- 阶段 4.2 已确认：有嘲讽随从时，攻击目标高亮能体现嘲讽限制；点击非法目标时仍由 `GameManager` 返回失败原因。
- 阶段 4.2 已确认：英雄技能按钮在费用不足或本回合已使用时仍可点击，并继续显示 Core 返回的失败原因。
- 阶段 4.3 已确认：`BattlePrototype` 的 Canvas 信息层级已调整，敌方区、战场区、玩家操作区、反馈区和胜负提示区一眼可分。
- 阶段 4.3 已确认：Play 模式中出牌、攻击、法术、英雄技能、结束回合、AI 行动、普通反馈和胜负显示均正常。
- `Player.Hand` / `Player.Deck` 已改为只读列表，外部不能直接 `Add` / `Remove` 手牌或牌库。
- `GameManager` 和 `GameUIController` 已通过 `Player.HasCardInHand(card)` 判断手牌归属。
- `GameActionGenerator.GenerateLegalActions(gameManager)` 已能枚举当前玩家的出牌、施法、攻击和结束回合动作。
- `GameActionGenerator` 已复用 `GameManager` 验证方法，不再重复维护出牌、法术目标、攻击和嘲讽规则。
- `GameManager.ExecuteAction(GameAction)` 已能统一执行出牌、施法、攻击和结束回合动作。
- `GameManager` 已新增 `Log Legal Actions On Turn Start` 调试开关，用于 Play Mode Console 验证动作生成结果。
- `AIController` 已能在 Enemy 回合自动连续执行动作，直到结束回合、游戏结束或达到行动上限。
- `ActionSelector` 已能按基础策略选择动作，并通过 `AIActionSelectionReason` 输出选择原因。
- `GameManager` 已提供 AI 调试开关，可以关闭洗牌并在 Enemy 回合开始打印 AI 手牌和当前法力。
- `ActionSelector` 已完成阶段 3.3 第一轮策略微调：避免自伤、优先高费出牌、普通伤害优先打英雄、解场优先高攻击力目标。
- `Evaluator` 已能输出真实局面和快照局面的评分明细。
- `GameStateSnapshot` 已能从 `GameManager` 当前真实局面复制出可模拟快照。
- `SnapshotActionMapper` 和 `SnapshotSimulator` 已能完成真实动作到快照动作的映射和单步模拟。
- `GameManager` 已提供快照评分验证和合法动作模拟后评分日志开关。
- 阶段 3.5 相关脚本已通过 Unity 编译检查。
- `ActionSelector` 已从规则优先级过渡到评分优先级：除斩杀外，会根据快照模拟后评分选择动作。
- `AIActionSelection` 已能携带被选动作的模拟评分，`AIController` 日志会显示模拟评分和真实评分变化。
- `SnapshotSimulator` 的 `EndTurn` 已补齐对手回合开始的抽牌、法力刷新和随从恢复攻击。
- AI 当前允许主动执行小幅降低评分的动作；如果没有进入允许亏分范围的主动动作，会选择结束回合。
- `AIActionSelectionReason.AcceptableScoreLoss` 已加入，用于在 Console 日志中标记“允许小亏换节奏”的选择。
- 快照模拟打出随从时已能复制嘲讽、圣盾、冲锋，并模拟当前两类无目标战吼。
- 快照模拟已能在随从死亡移除前结算一层“对敌方英雄造成伤害”的亡语。
- `ActionSelector` 已接入同回合后续攻击预估：候选动作模拟后，会用 `SnapshotFollowUpEvaluator` 继续看最多 2 次已有随从攻击收益。
- `PlayerSnapshot` 已能保存具体 `CardSnapshot` 手牌列表；当前抽牌和花费卡牌后的具体手牌仍是阶段性简化。
- `SnapshotFollowUpEvaluator` 已能基于 `CardSnapshot` 生成继续出随从和伤害法术动作，并和攻击动作一起参与同回合预估。
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
- 历史上曾用 `CardPlayed` / `MinionSummoned` 验证事件总线和 Console 调试日志。
- 进入阶段 3 后，为避免干扰 AI 日志，出牌和召唤调试事件已删除。
- 当前事件系统只保留真正影响规则的 `MinionDied`。

## 阶段 2.5.1 已验证

- `GameManager` 在随从死亡清理时发布 `MinionDied`。
- `MinionDied` 事件把死亡随从写入 `TargetMinion`。
- `GameManager` 订阅 `MinionDied`，用于触发亡语。
- 事件系统不再输出 `GameEvent:` Console 调试日志，避免影响 AI 行动日志阅读。

## 阶段 2.6 已验证

- 已新增 `DeathrattleType.cs`，当前包含 `None` 和 `DealDamageToEnemyHero`。
- `CardData` 已支持配置 `Deathrattle Type` 和 `Deathrattle Value`。
- `CardView` 已支持显示“亡语：对敌方英雄造成 X 点伤害”。
- `MinionView` 已支持显示“亡语:X”。
- `GameManager` 已注册规则事件监听，收到 `MinionDied` 后会尝试结算死亡随从的亡语。
- 当前第一个亡语效果：对死亡随从拥有者的敌方英雄造成 `DeathrattleValue` 点伤害。
- Unity Play 模式已确认：亡语炸弹人死亡后，敌方英雄生命减少 1，死亡随从从战场移除。

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
- 当前冲锋、嘲讽和前两个无目标战吼不急着迁移到事件系统。事件系统已经精简为只保留 `MinionDied`，用于触发亡语。

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

阶段 2.10 本轮核心优化已经完成，阶段 3 已进入评估和模拟链路：

```text
阶段 3.0 / 3.1：AI 基础行动和自动回合已写入
阶段 3.2：基础动作选择和选择原因日志已写入
阶段 3.3：AI 调试验证入口和第一轮策略微调已写入
阶段 3.4：评估函数和评分明细已写入
阶段 3.5：快照数据、动作映射、单步模拟和调试验证入口已写入
阶段 3.6：评分优先级选择、被选动作模拟评分日志、EndTurn 快照修正和最小收益门槛已写入
阶段 3.7：允许小亏换节奏阈值和选择原因日志已写入
阶段 3.8：出随从关键词和无目标战吼快照模拟已写入
阶段 3.9：亡语伤害快照模拟已写入
阶段 3.10：同回合后续攻击预估已写入
阶段 3.11：具体手牌快照已写入
阶段 3.12：基于 CardSnapshot 的继续出牌模拟已写入
阶段 3.12 Play 模式观察：AI 连续出牌和连续攻击行为基本合理
阶段 3.13：英雄技能最小闭环已完成，玩家和 AI 都能正常使用英雄技能
阶段 3 AI v1 回归清单：已整理到 Docs/08_AIReview.md
后续求职导向路线：英雄技能最小闭环 -> UI / 交互 / 表现重点打磨 -> 主流程与套牌选择 -> 移动端 / 性能意识补强 -> 数据配置和测试工具 -> 求职包装
下一步：阶段 4.4 卡牌和随从显示拆分，优先让手牌中随从牌和法术牌一眼可区分，并把场上随从的 Ready、关键词、亡语显示拆得更清楚

UI 拆分 / UI 复用刷新：
暂缓到功能更完整后统一整理。
```

继续写代码前，仍然先写属性清单，再动代码。
