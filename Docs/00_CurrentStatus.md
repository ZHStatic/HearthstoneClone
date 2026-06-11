# Current Status

最后更新：2026-06-11

## 当前阶段

阶段 1：最小可玩原型。

当前状态：

```text
底层 Core 逻辑骨架已完成。
第一版 UGUI 显示和点击出牌流程已完成。
下一步准备接入随从攻击 UI 交互。
```

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
- `MinionView.cs`：显示单个场上随从。
- `BoardView.cs`：显示一方战场上的随从列表。
- `GameUIController.cs`：连接 UI 和 `GameManager`，处理点击出牌、结束回合和刷新。

### Unity 资源

- `Assets/Prefabs/UI/CardViewPrefab.prefab`
- `Assets/Prefabs/UI/MinionViewPrefab.prefab`
- 测试卡牌：`Assets/Test_1Cost_1_1.asset`
- 当前已有测试场景：`SampleScene`、`test1`、`step6`

## 已确认

- Unity Play 模式可以运行。
- 牌库中配置有效 `CardData` 后，手牌可以显示。
- 点击手牌可以调用 `GameManager.TryPlayMinionCard(card)`。
- 成功出牌后，手牌减少、法力减少、战场出现随从。
- 点击结束回合后，当前行动者切换，UI 会刷新。
- 已定位并修复一次 `NullReferenceException`：牌库列表中存在空的 `CardData` 项时，`Player` 现在会跳过空项。

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

## 下一步

先暂停继续堆功能，优先消化当前 UI 和文档。

之后进入随从攻击 UI：

1. 点击己方可攻击随从，记录为攻击者。
2. 点击敌方随从，调用 `GameManager.TryAttackMinion(attacker, target)`。
3. 点击敌方英雄，调用 `GameManager.TryAttackHero(attacker, targetHero)`。
4. 攻击后刷新战场、英雄血量和可攻击状态。

按照协作规则，开始写新类或改交互前，先列属性和方法清单，确认后再写。
