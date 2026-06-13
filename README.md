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
- UI 和 Core 分层：UI 只负责显示和输入，规则由 Core 处理

进行中：

- 阶段 1.5 最小原型展示打磨
- 下一步：补充基础随从卡、操作反馈和演示说明

暂未完成：

- 法术牌
- 关键词
- 事件系统
- AI 对手
- 动画、音效和最终 UI 打磨

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
│   ├── Core/      # 核心规则：玩家、卡牌、战场、回合流程
│   └── UI/        # 表现层：手牌、战场、HUD、点击输入
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
└── Learning/
    ├── CSharpNotes.md
    └── UnityNotes.md
```

## 文档入口

- [当前进度](Docs/00_CurrentStatus.md)
- [项目计划](Docs/01_ProjectPlan.md)
- [Core 架构](Docs/02_CoreArchitecture.md)
- [UI 架构](Docs/03_UIArchitecture.md)
- [功能流程](Docs/04_FeatureFlows.md)
- [面试讲解要点](Docs/05_InterviewNotes.md)
- [C# 学习笔记](Docs/Learning/CSharpNotes.md)
- [Unity 学习笔记](Docs/Learning/UnityNotes.md)

## 运行方式

1. 使用 Unity Hub 打开本项目。
2. Unity 版本：`2020.3.48f1c1`。
3. 打开 `Assets/Scenes/` 下当前用于测试的场景。
4. 确认 `GameManager` 中的 `Player Deck Data` 和 `Enemy Deck Data` 配置了有效 `CardData`。
5. 点击 Play 运行。

## 架构重点

当前项目重点展示以下设计：

- `CardData`、`Card`、`Minion` 分离静态模板和运行时状态。
- `Player`、`Board`、`GameManager` 拆分玩家资源、战场状态和对局流程。
- Core 层不依赖 UI 层。
- UI 点击只调用 `GameManager` 方法，不直接修改规则数据。
- 后续法术和关键词会通过事件系统扩展。

## License

MIT
