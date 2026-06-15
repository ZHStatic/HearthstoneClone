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
- UI 和 Core 分层：UI 只负责显示和输入，规则由 Core 处理

当前阶段：

- 阶段 1.5 最小原型展示打磨已验收通过
- 下一步：进入阶段 2，制作法术牌、关键词和事件系统

暂未完成：

- 法术牌
- 关键词
- 事件系统
- AI 对手
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

## 当前基础卡牌

阶段 1.5 使用 5 张无关键词随从卡覆盖低费、中费和高费测试场景：

| 卡牌 | 费用 | 攻击 | 生命 | 用途 |
|------|------|------|------|------|
| 训练新兵 | 1 | 1 | 2 | 测试第一回合出牌和基础召唤 |
| 河湾猎手 | 2 | 3 | 2 | 测试高攻击低生命随从交换 |
| 城墙守卫 | 3 | 2 | 5 | 测试高生命随从存活和反击 |
| 战场斗士 | 4 | 4 | 4 | 测试中费标准身材 |
| 岩石巨人 | 5 | 6 | 6 | 测试费用不足提示和后期攻击 |

## 演示路径

推荐 2-3 分钟演示顺序：

1. 打开 `Assets/Scenes/BattlePrototype.unity`。
2. 展示 `GameManager` 上配置的双方牌库。
3. Play 后展示手牌、法力、英雄血量和左上角 HUD。
4. 点击费用不足的卡牌，展示操作提示。
5. 点击可打出的低费随从，展示手牌减少、法力减少、战场出现随从。
6. 结束回合，让双方各召唤随从。
7. 点击 Ready 随从，展示选中高亮和提示。
8. 攻击敌方随从或英雄，展示伤害结算、血量变化和胜负提示。

## 架构重点

当前项目重点展示以下设计：

- `CardData`、`Card`、`Minion` 分离静态模板和运行时状态。
- `Player`、`Board`、`GameManager` 拆分玩家资源、战场状态和对局流程。
- Core 层不依赖 UI 层。
- UI 点击只调用 `GameManager` 方法，不直接修改规则数据。
- 后续法术和关键词会通过事件系统扩展。

## 学习复盘文档

- [阶段 1 类图](Docs/Diagrams/Stage1_ClassDiagram.drawio)
- [阶段 1 复盘路线](Docs/Learning/Stage1ReviewGuide.md)
- [UI 回调和 Button 理解指南](Docs/Learning/UICallbacksAndButtonGuide.md)
- [逐行读代码检查表](Docs/Learning/CodeReadingChecklist.md)

## License

MIT
