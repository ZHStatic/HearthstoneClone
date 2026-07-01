# HearthstoneClone

单人版炉石核心对战原型 | Unity 2D | 求职 Demo

## 项目简介

本项目使用 Unity 2020.3 和 C# 实现一个炉石类卡牌对战原型。

当前目标不是追求卡牌数量和美术规模，而是做出一个结构清晰、可解释、可扩展的核心对战体验，用于学习和求职展示。

## 当前完成度

已完成：

- 卡牌模板数据：`ScriptableObject`
- 运行时卡牌、英雄、玩家、战场、随从
- 回合开始、抽牌、法力水晶、出牌召唤
- 随从攻击随从、随从攻击英雄和胜负判断
- 第一版 UGUI：手牌、战场、法力、英雄血量、结束回合、攻击交互
- 阶段 1.5 展示打磨：基础随从卡、操作反馈、选中高亮和基础 UI 可读性调整
- 阶段 2.1 基础伤害法术：卡牌类型、法术目标类型、火花测试法术、法术选目标和伤害结算
- 阶段 2.2 第一个关键词：冲锋，召唤后可以立即攻击，手牌 UI 显示“冲锋”
- 阶段 2.3 第二个关键词：嘲讽，攻击目标选择会受嘲讽限制
- 阶段 2.4 第一个战吼：对敌方英雄造成伤害的最小链路
- 阶段 2.4.5 第二个战吼：抽牌
- 阶段 2.5 第一版事件系统：出牌、召唤和死亡事件
- 阶段 2.6 第一个亡语：死亡后对敌方英雄造成伤害
- 阶段 2.7 圣盾：第一次受到伤害时抵消该次伤害并失去圣盾
- 阶段 2.8 收尾复盘：整理阶段 2 架构、演示脚本和进入 AI 前检查点
- 阶段 3.0 / 3.1 基础 AI 行动：Enemy 回合自动枚举合法动作并执行
- 阶段 3.2 第一版 AI 选择策略：优先斩杀、解场、出牌，并输出选择理由
- 阶段 3.3 AI 策略验证入口：稳定起手、打印 AI 手牌、调整解场和出牌优先级
- 阶段 3.4 第一版 AI 评估函数：按英雄血量、手牌和场面输出评分明细
- 阶段 3.5 快照模拟基础链路：复制当前局面，模拟合法动作，并输出模拟后评分
- 阶段 3.6 / 3.7 评分优先级动作选择：按模拟评分选动作，并允许小幅亏分换节奏
- 阶段 3.8 / 3.9 快照规则补强：模拟出随从关键词、无目标战吼和一层亡语伤害
- 阶段 3.10 / 3.12 同回合后续预估：AI 会评估攻击、出牌、法术和英雄技能的少量后续动作
- 阶段 3.11 手牌快照：AI 模拟出牌时保留具体手牌信息，不只记录手牌数量
- 阶段 3.13 英雄技能闭环：玩家和 AI 都可以使用 2 费 1 点伤害英雄技能
- UI 和 Core 分层：UI 只负责显示和输入，规则由 Core 处理

当前阶段：

- 阶段 2 最小目标已完成：基础法术、冲锋、嘲讽、战吼、事件系统、亡语和圣盾都已通过 Play 模式验证
- 阶段 3 已完成基础 AI、快照模拟、同回合后续预估和英雄技能最小闭环
- 下一阶段重点转向 UI 交互、表现反馈和求职展示打磨

暂未完成：

- 更完整的 AI 搜索和玩家反击模拟
- 套牌选择/构筑
- 动画、音效和最终美术打磨

## 技术栈

| 技术 | 说明 |
|------|------|
| Unity 2020.3.48f1c1 | 2D Built-In Render Pipeline |
| C# | 游戏逻辑与架构 |
| UGUI | 手牌、战场和 HUD |
| ScriptableObject | 卡牌模板数据 |
| Prefab | 复用卡牌和随从 UI |

## 当前项目结构

```text
Assets/
├── Scripts/
│   ├── Core/      # 核心规则：卡牌、实体、动作、事件、日志
│   ├── AI/        # AI 控制、动作选择、评估函数、快照模拟
│   └── UI/        # 表现层：控制器、视图、文本格式化
├── Prefabs/
│   └── UI/        # CardViewPrefab、MinionViewPrefab
└── Scenes/        # 当前测试场景

Docs/
├── 00_CurrentStatus.md
├── 01_ProjectPlan.md
├── 02_CoreArchitecture.md
├── 03_UIArchitecture.md
├── 04_FeatureFlows.md
├── 05_InterviewNotes.md
├── 06_Stage2Review.md
├── 07_CodeReviewReport.md
├── 08_AIReview.md
├── ProjectOverview.html
└── Learning/
    ├── CodeReadingChecklist.md
    ├── CSharpNotes.md
    ├── Stage1ReviewGuide.md
    ├── UICallbacksAndButtonGuide.md
    └── UnityNotes.md
```

## 文档入口

- [当前进度](Docs/00_CurrentStatus.md)
- [项目计划](Docs/01_ProjectPlan.md)
- [Core 架构](Docs/02_CoreArchitecture.md)
- [UI 架构](Docs/03_UIArchitecture.md)
- [功能流程](Docs/04_FeatureFlows.md)
- [面试讲解要点](Docs/05_InterviewNotes.md)
- [阶段 2 收尾复盘](Docs/06_Stage2Review.md)
- [历史代码审查报告](Docs/07_CodeReviewReport.md)
- [AI 回归清单](Docs/08_AIReview.md)
- [C# 学习笔记](Docs/Learning/CSharpNotes.md)
- [Unity 学习笔记](Docs/Learning/UnityNotes.md)

## 运行方式

1. 使用 Unity Hub 打开本项目。
2. Unity 版本：`2020.3.48f1c1`。
3. 打开 `Assets/Scenes/` 下当前用于测试的场景。
4. 确认 `GameManager` 中的 `Player Deck Data` 和 `Enemy Deck Data` 配置了有效 `CardData`。
5. 点击 Play 运行。

## 当前测试卡牌

当前测试牌覆盖基础随从、关键词随从、法术、战吼、亡语和圣盾：

| 卡牌 | 费用 | 类型 | 数值/效果 | 用途 |
|------|------|------|-----------|------|
| 训练新兵 | 1 | 随从 | 1/2 | 测试第一回合出牌和基础召唤 |
| 河湾猎手 | 2 | 随从 | 3/2 | 测试高攻击低生命随从交换 |
| 疾风斥候 | 1 | 随从 | 1/1，冲锋 | 测试召唤当回合攻击 |
| 城墙守卫 | 3 | 随从 | 2/5，嘲讽 | 测试嘲讽目标限制 |
| 战场斗士 | 4 | 随从 | 4/4 | 测试中费标准身材 |
| 岩石巨人 | 5 | 随从 | 6/6 | 测试费用不足提示和后期攻击 |
| 火花 | 1 | 法术 | 造成 2 点伤害，目标为任意角色 | 测试基础法术选目标和伤害 |
| 火焰学徒 | 2 | 随从 | 战吼：对敌方英雄造成 1 点伤害 | 测试战吼伤害 |
| 书卷侍从 | 2 | 随从 | 战吼：抽 1 张牌 | 测试战吼抽牌 |
| 亡语炸弹人 | 2 | 随从 | 亡语：对敌方英雄造成 1 点伤害 | 测试死亡事件和亡语 |
| 圣盾卫士 | 2 | 随从 | 圣盾 | 测试第一次伤害抵消 |

## 演示路径

推荐 3-5 分钟演示顺序：

1. 打开 `Assets/Scenes/BattlePrototype.unity`。
2. 展示 `GameManager` 上配置的双方牌库。
3. Play 后展示手牌、法力、英雄血量和左上角 HUD。
4. 点击费用不足的卡牌，展示操作提示。
5. 点击可打出的低费随从，展示手牌减少、法力减少、战场出现随从。
6. 点击火花，展示法术选目标提示；点击随从或英雄，展示 2 点伤害结算。
7. 打出疾风斥候，展示冲锋随从召唤当回合可以攻击。
8. 展示城墙守卫的嘲讽，让攻击英雄或非嘲讽目标失败。
9. 点击英雄技能按钮并选择敌方随从或英雄，展示 2 费 1 点伤害的目标选择和结算。
10. 打出火焰学徒或书卷侍从，展示战吼文字和即时效果。
11. 打出亡语炸弹人并让它死亡，展示死亡事件触发亡语。
12. 打出圣盾卫士，展示第一次受到伤害不掉血并失去圣盾，第二次正常掉血。

## 架构重点

当前项目重点展示以下设计：

- `CardData`、`Card`、`Minion` 分离静态模板和运行时状态。
- `CardType`、`SpellTargetType` 让卡牌模板可以表达随从牌和基础法术牌。
- `KeywordType`、`CardData.Keywords`、`Minion.Keywords` 支持第一版关键词数据链路。
- `BattlecryType`、`CardData.BattlecryType`、`CardData.BattlecryValue` 支持第一版战吼数据链路。
- `DeathrattleType` 配合 `MinionDied` 事件验证死亡后触发效果。
- `CardView` 和 `MinionView` 会把关键词显示为中文，例如“冲锋”“嘲讽”“圣盾”。
- `CardView` 会显示战吼和亡语文字。
- `Player`、`Board`、`GameManager` 拆分玩家资源、战场状态和对局流程。
- `GameActionGenerator`、`AIController`、`ActionSelector` 组成第一版 AI 行动链路。
- `Evaluator`、`AI/Simulation` 和 `SnapshotFollowUpEvaluator` 让 AI 可以先模拟动作结果和少量同回合后续动作，再观察评分变化。
- `GameActionType.UseHeroSkillOnMinion` / `UseHeroSkillOnHero` 把英雄技能接入玩家和 AI 共用的动作链路。
- Core 层不依赖 UI 层。
- UI 点击只调用 `GameManager` 方法，不直接修改规则数据。
- 当前基础伤害法术、冲锋、嘲讽、战吼和圣盾保留阶段性简化；亡语已开始通过事件系统触发。
- 后续如果出现更多伤害修改、攻击规则和死亡连锁，会逐步抽出 `DamageResolver`、`CombatResolver`、`DeathProcessor` 或 `EffectSystem`。
- 阶段 2.8 已整理文档边界：状态、计划、Core 架构、UI 架构、功能流程、阶段复盘和面试笔记各自维护不同内容。

## 学习复盘文档

- [阶段 1 类图](Docs/Diagrams/Stage1_ClassDiagram.drawio)
- [阶段 2 收尾复盘](Docs/06_Stage2Review.md)
- [阶段 1 复盘路线](Docs/Learning/Stage1ReviewGuide.md)
- [UI 回调和 Button 理解指南](Docs/Learning/UICallbacksAndButtonGuide.md)
- [逐行读代码检查表](Docs/Learning/CodeReadingChecklist.md)

## License

MIT
