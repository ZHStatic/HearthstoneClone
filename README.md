# HearthstoneClone

单人卡牌对战原型 | Unity 2D | Core + AI + UGUI

## 简介

HearthstoneClone 是一个使用 Unity 2020.3 和 C# 实现的炉石类单人卡牌对战项目。当前重点是完整的单局对战、清晰的规则层边界、可配置卡牌与套牌数据、基础 AI 行动和可读的 UI 反馈。

## 当前状态

已完成的核心能力：

- 对局流程：抽牌、法力、出牌、攻击、结束回合、胜负判断。
- 卡牌类型：随从牌、单目标伤害法术。
- 关键词与效果：冲锋、嘲讽、圣盾、战吼、亡语、英雄技能。
- 数据配置：`CardData` 管理卡牌模板，`DeckData` 管理预制套牌。
- AI：合法动作枚举、评分函数、快照模拟、同回合后续动作预估。
- UI：手牌、战场、英雄血量、法力、目标高亮、操作反馈、基础音效和目标脉冲。
- 性能整理：手牌和战场 View 已改为轻量复用，避免频繁销毁重建。

当前阶段：

- 阶段 4.7 已完成：预制套牌、套牌选择、胜负结算和演示模式入口。
- 阶段 4.8 已完成：轻量复用和横屏比例检查通过，无需额外布局或代码修改。
- 下一步进入阶段 5：数据配置和测试工具补强。

## 技术栈

| 技术 | 用途 |
|------|------|
| Unity 2020.3.48f1c1 | 2D Built-In Render Pipeline |
| C# | 核心规则、AI、UI 控制 |
| UGUI | 手牌、战场、HUD 和交互反馈 |
| ScriptableObject | 卡牌模板和预制套牌 |
| Prefab | 卡牌和随从 UI 复用 |

## 项目结构

```text
Assets/
├── Scripts/
│   ├── Core/      # 核心规则：卡牌、实体、动作、事件、日志
│   ├── AI/        # AI 控制、动作选择、评估函数、快照模拟
│   ├── UI/        # UI 控制器、视图、文本格式化
│   └── Editor/    # 编辑器工具
├── ScriptableObjects/
│   ├── Cards/     # CardData 卡牌模板
│   └── Decks/     # DeckData 预制套牌
├── Prefabs/
│   └── UI/
└── Scenes/
```

## 运行方式

1. 使用 Unity Hub 打开项目。
2. Unity 版本使用 `2020.3.48f1c1`。
3. 打开 `Assets/Scenes/BattlePrototype.unity`。
4. 确认 `GameManager` 的 `Default Player Deck` 和 `Default Enemy Deck` 已绑定有效 `DeckData`。
5. 点击 Play 运行。

可通过 Unity 菜单生成测试套牌：

```text
HearthstoneClone/AI Test Deck/Apply Comprehensive Deck To Both Players
HearthstoneClone/AI Test Deck/Apply Fixed Observation Deck
```

## 当前测试卡牌

| 卡牌 | 类型 | 费用 | 效果 |
|------|------|------|------|
| 训练新兵 | 随从 | 1 | 1/2 |
| 河湾猎手 | 随从 | 2 | 3/2 |
| 疾风斥候 | 随从 | 1 | 1/1，冲锋 |
| 城墙守卫 | 随从 | 3 | 2/5，嘲讽 |
| 战场斗士 | 随从 | 4 | 4/4 |
| 岩石巨人 | 随从 | 5 | 6/6 |
| 火花 | 法术 | 1 | 对任意角色造成 2 点伤害 |
| 火焰学徒 | 随从 | 2 | 战吼：对敌方英雄造成 1 点伤害 |
| 书卷侍从 | 随从 | 2 | 战吼：抽 1 张牌 |
| 亡语炸弹人 | 随从 | 2 | 亡语：对敌方英雄造成 1 点伤害 |
| 圣盾卫士 | 随从 | 2 | 圣盾 |

## 架构要点

- `CardData`、`Card`、`Minion` 分离静态模板、手牌/牌库实例和场上实体。
- `DeckData` 通过稳定 `DeckKey` 管理预制套牌，并向 `GameManager` 提供开局牌表。
- Core 层负责规则和状态，UI 层只负责显示和输入。
- 玩家输入和 AI 行动都复用 `GameAction`、`GameActionGenerator` 和 `GameManager.ExecuteAction()`。
- 战斗日志记录关键结算结果，UI 反馈优先读取 Core 返回的实际结果。
- 亡语通过 `MinionDied` 事件触发；基础法术、战吼和英雄技能仍保留直接结算的阶段性实现。

## 文档入口

- [当前进度](Docs/00_CurrentStatus.md)
- [项目计划](Docs/01_ProjectPlan.md)
- [Core 架构](Docs/02_CoreArchitecture.md)
- [UI 架构](Docs/03_UIArchitecture.md)
- [功能流程](Docs/04_FeatureFlows.md)
- [AI 回归清单](Docs/08_AIReview.md)
- [性能与移动端巡检](Docs/09_PerformanceMobileReview.md)

## License

MIT
