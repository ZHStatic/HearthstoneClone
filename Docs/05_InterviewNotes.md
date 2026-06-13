# Interview Notes

本文档用于沉淀求职时可以讲清楚的项目要点。

它不是学习流水账，而是面试官视角下的“我做了什么、为什么这样做、后续如何扩展”。

## 项目一句话介绍

```text
这是一个使用 Unity 2D 和 C# 实现的单人版炉石核心对战原型。
当前阶段完成了卡牌数据、玩家资源、战场、回合流程、随从召唤、随从攻击随从、随从攻击英雄、胜负判定和第一版 UGUI 显示交互。
```

## 当前完成度

已完成：

- 卡牌模板数据：`CardData`
- 运行时卡牌：`Card`
- 英雄生命：`Hero`
- 玩家资源：`Player`
- 场上随从：`Minion`
- 战场列表：`Board`
- 对局流程：`GameManager`
- 第一版 UI：手牌、战场、法力、血量、结束回合、随从攻击随从、随从攻击英雄

暂未完成：

- 法术牌
- 关键词
- 事件系统
- AI 对手
- 动画、音效和最终美术

## 架构亮点

### 1. 静态数据和运行时状态分离

项目中把卡牌拆成：

```text
CardData：静态模板
Card：手牌/牌库里的运行时卡牌
Minion：场上的运行时随从
```

面试表达：

```text
我没有直接修改 ScriptableObject 里的模板数据，而是用 Card 和 Minion 保存对局中的运行时状态。
这样同一张卡可以在牌库里出现多份，后续也可以支持减费、受伤、Buff 等运行时变化。
```

### 2. Core 和 UI 分层

当前 Core 层不依赖 UI。

```text
Core 负责规则。
UI 负责显示和输入。
```

面试表达：

```text
UI 不直接扣法力、不删除手牌、不修改战场。
玩家点击后，UI 只调用 GameManager 的规则方法，然后重新读取 Core 状态刷新显示。
这样规则层更容易测试和扩展，也避免 UI 和规则混在一起。
```

### 3. GameManager 作为阶段 1 调度器

当前 `GameManager` 负责：

```text
开局
回合切换
出牌
攻击
死亡清理
胜负判断
```

面试表达：

```text
阶段 1 为了快速完成最小可玩原型，我先让 GameManager 承担主要流程调度。
后续加入法术和关键词后，会逐渐拆分出事件系统、效果系统、战斗结算系统。
```

### 4. 使用 ScriptableObject 管理卡牌模板

面试表达：

```text
卡牌基础属性使用 ScriptableObject 配置，方便在 Unity Inspector 中创建和调整卡牌数据。
程序运行时会根据这些模板创建 Card 实例，避免直接修改模板。
```

### 5. 使用 Prefab 复用 UI

面试表达：

```text
手牌和战场都是重复元素，所以我用 CardViewPrefab 和 MinionViewPrefab 作为模板。
HandView 和 BoardView 根据运行时数据动态生成 UI，避免为每张牌手动摆一个对象。
```

## 遇到的问题和解决

### NullReferenceException

问题：

```text
GameManager 开局时报 NullReferenceException。
调用链显示错误发生在 Card 构造函数中。
```

定位：

```text
Player 根据 Inspector 中的牌库列表创建 Card。
列表里有空的 CardData 槽位，导致 new Card(data) 时 data 为 null。
```

解决：

```text
在 Player 构造函数中检查 deckCards 和每个 CardData。
如果某个 CardData 是空的，就跳过。
同时在 Unity Inspector 中检查 Player Deck Data 和 Enemy Deck Data 是否有 None。
```

面试表达：

```text
这次问题让我意识到 Inspector 配置也需要防御性检查。
运行时代码不能默认所有编辑器配置都正确，所以我给 Player 创建牌库的流程加了空值保护。
```

## 当前可以演示的流程

演示顺序：

```text
1. 打开对战场景。
2. 展示 GameManager 上配置的卡牌数据。
3. Play 后展示手牌、法力、英雄血量。
4. 点击 1 费手牌，召唤随从。
5. 展示手牌减少、法力减少、战场出现随从。
6. 点击 End Turn，展示当前行动者切换。
7. 双方各有随从后，点击己方 Ready 随从，再点击敌方随从，展示随从互相造成伤害。
8. 点击己方 Ready 随从，再点击敌方英雄，展示英雄血量减少。
9. 英雄血量归零后，展示 Game Over。
```

讲解重点：

```text
每次 UI 操作都不是 UI 自己改状态，而是调用 GameManager。
状态变化发生在 Core，UI 只是刷新显示。
```

## 下一步计划

短期：阶段 1.5 最小原型展示打磨

- 补充更多随从卡数据。
- 做基础操作反馈，例如选中高亮和非法操作提示。
- 进入阶段 2 前整理一次 README、截图和演示说明。

中期：

- 做法术牌。
- 引入事件系统。
- 实现冲锋、嘲讽、战吼、亡语、圣盾等关键词。

长期：

- 做 AI 对手。
- 做套牌选择。
- 打磨 UI、动画、音效。
- 完善 README、视频和技术博客。
