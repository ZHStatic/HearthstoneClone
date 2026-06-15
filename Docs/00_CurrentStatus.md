# Current Status

## 阶段 1 复盘学习入口

为了帮助重新理解阶段 1 的代码、Unity 关联和 UI 回调，新增以下学习文档：

- `Docs/Learning/Stage1ReviewGuide.md`：阶段 1 推荐复盘顺序、核心链路和练习方式。
- `Docs/Learning/UICallbacksAndButtonGuide.md`：专门解释 `Button.onClick`、`Action<T>`、`AddListener` 和点击回调链。
- `Docs/Learning/CodeReadingChecklist.md`：逐行读代码时可反复使用的检查表。

最后更新：2026-06-15

## 当前阶段

阶段 1 已完成，阶段 1.5：最小原型展示打磨已验收通过。

当前状态：

```text
阶段 1 最小对战闭环已完成。
阶段 1.5 已补充基础随从卡、操作反馈、选中高亮和基础 UI 可读性调整。
当前可以完成：抽牌 → 出牌 → 召唤随从 → 操作反馈 → 随从攻击随从/英雄 → 胜负判定。
```

下一步进入阶段 2：法术牌、关键词和事件系统。

## 已完成

### Core 层

- `CardData.cs`：卡牌模板数据，使用 `ScriptableObject`。
- `Card.cs`：运行时卡牌实例，引用 `CardData`，保存当前费用。
- `Hero.cs`：英雄血量、受伤、治疗、死亡判断。
- `Player.cs`：玩家手牌、牌库、法力水晶、抽牌、出牌。
- `Board.cs`：战场，管理双方场上的随从列表。
- `Minion.cs`：场上随从实例，管理攻击、生命、所属玩家、能否攻击。
- `GameManager.cs`：对局流程，管理开局、回合、出牌、攻击、死亡清理、胜负判断。

### UI 层

- `CardView.cs`：显示单张手牌，点击后通知上层。
- `HandView.cs`：显示当前行动者的手牌列表。
- `MinionView.cs`：显示单个场上随从，点击后通知上层，并支持选中高亮。
- `BoardView.cs`：显示一方战场上的随从列表，并传递随从点击回调和选中状态。
- `GameUIController.cs`：连接 UI 和 `GameManager`，处理点击出牌、随从攻击随从、随从攻击英雄、结束回合、操作反馈和刷新。

### Unity 资源

- `Assets/Prefabs/UI/CardViewPrefab.prefab`
- `Assets/Prefabs/UI/MinionViewPrefab.prefab`
- 卡牌数据位于 `Assets/ScriptableObjects/Cards/`
- 当前主测试场景：`Assets/Scenes/BattlePrototype.unity`
- 阶段 1.5 基础随从卡：训练新兵、河湾猎手、城墙守卫、战场斗士、岩石巨人。

## 已确认

- Unity Play 模式可以运行。
- 牌库中配置有效 `CardData` 后，手牌可以显示。
- 点击手牌可以调用 `GameManager.TryPlayMinionCard(card)`。
- 成功出牌后，手牌减少、法力减少、战场出现随从。
- 点击结束回合后，当前行动者切换，UI 会刷新。
- 随从攻击随从 UI 已测试通过。
- 随从攻击英雄 UI 已测试通过。
- 已定位并修复一次 `NullReferenceException`：牌库列表中存在空的 `CardData` 项时，`Player` 现在会跳过空项。
- 阶段 1.5 Play 模式验收通过：费用不足、出牌成功、选中/取消选中、攻击随从、攻击英雄和 UI 刷新均可正常展示。
- UI 可读性已做基础调整：左上角 HUD、卡牌文字、随从文字和操作提示比第一版更容易阅读。

## 当前设计理解

当前项目分为两层：

```text
Core：规则层
UI：表现和输入层
```

核心原则：

- `CardData` 是静态模板，`Card` / `Minion` 是运行时状态。
- `Player` 管理手牌、牌库、法力。
- `Board` 管理战场随从列表。
- `GameManager` 负责当前阶段的对局流程调度。
- UI 不直接改规则状态，只读取 Core 状态，并调用 `GameManager` 方法。

## 当前文档结构

```text
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

## 下一步：阶段 2

- 开始设计法术牌的数据结构和出牌流程。
- 引入事件系统前，先梳理阶段 2 的最小目标和边界。
- 第一批关键词建议从冲锋、嘲讽开始，再进入战吼、亡语、圣盾。
- 继续保持 Core 不依赖 UI，UI 只负责显示、输入和反馈。
