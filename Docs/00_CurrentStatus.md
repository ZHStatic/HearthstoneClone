# Current Status

最后更新：2026-06-13

## 当前阶段

阶段 1：最小可玩原型。

当前状态：

```text
阶段 1 最小对战闭环已完成代码接入。
当前可以完成：抽牌 → 出牌 → 召唤随从 → 随从攻击随从/英雄 → 胜负判定。
```

Unity 中还需要确认英雄按钮引用是否已经绑定，详见“下一步验证”。

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
- `MinionView.cs`：显示单个场上随从，点击后通知上层。
- `BoardView.cs`：显示一方战场上的随从列表，并传递随从点击回调。
- `GameUIController.cs`：连接 UI 和 `GameManager`，处理点击出牌、随从攻击随从、随从攻击英雄、结束回合和刷新。

### Unity 资源

- `Assets/Prefabs/UI/CardViewPrefab.prefab`
- `Assets/Prefabs/UI/MinionViewPrefab.prefab`
- 卡牌数据位于 `Assets/ScriptableObjects/Cards/`
- 当前主测试场景：`Assets/Scenes/BattlePrototype.unity`

## 已确认

- Unity Play 模式可以运行。
- 牌库中配置有效 `CardData` 后，手牌可以显示。
- 点击手牌可以调用 `GameManager.TryPlayMinionCard(card)`。
- 成功出牌后，手牌减少、法力减少、战场出现随从。
- 点击结束回合后，当前行动者切换，UI 会刷新。
- 随从攻击随从 UI 已测试通过。
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

## 下一步验证

在 Unity 中完成以下检查：

1. 给 `PlayerHeroText` 和 `EnemyHeroText` 所在 UI 物体添加 `Button` 组件。
2. 在 `GameUIController` 中绑定 `Player Hero Button` 和 `Enemy Hero Button`。
3. Play 后双方各召唤至少一个随从。
4. 选中己方 `Ready` 随从后点击敌方随从，确认随从互相造成伤害。
5. 选中己方 `Ready` 随从后点击敌方英雄，确认英雄血量减少。
6. 英雄血量降到 0 或以下时，确认显示 `Game Over`，结束回合按钮不可点击。

验证通过后，可以提交：

```text
feat: 完成阶段1最小对战闭环
```

## 阶段 1 后续可选打磨

- 补充更多随从卡数据。
- 做选中高亮和非法操作提示。
- 做简单攻击动画或攻击线。
- 进入阶段 2：法术牌、关键词和事件系统。
