# Current Status

最后更新：2026-06-07

## 当前阶段

阶段 1：最小可玩原型。

当前已完成的是第一版底层核心逻辑骨架。

## 已完成

核心类：

- `CardData.cs`：卡牌模板数据，使用 `ScriptableObject`。
- `Card.cs`：运行时卡牌实例，引用 `CardData`，保存当前费用。
- `Hero.cs`：英雄血量、受伤、治疗、死亡判断。
- `Player.cs`：玩家手牌、牌库、法力水晶、抽牌、出牌。
- `Board.cs`：战场，管理双方场上的随从列表。
- `Minion.cs`：场上随从实例，管理攻击、生命、所属玩家、能否攻击。
- `GameManager.cs`：对局流程，管理开局、回合、出牌、攻击、死亡清理、胜负判断。

学习/辅助文档：

- `CODE_READING_NOTES.md`：记录守卫语句、倒序遍历删除元素、`List` 索引等代码阅读点。
- `CORE_ARCHITECTURE.md`：记录当前 Core 层架构、类职责、引用关系、核心流程和后续扩展方向。

## 已确认

- `GameManager.cs` 已在 Unity 中确认没有 Console 报错。
- `GameManager` 可以挂到场景物体上。
- Inspector 能看到 `Player Deck Data`、`Enemy Deck Data`、`Starting Hand Count`。
- `GameManager.cs` 代码已梳理过一遍，大体理解没问题。
- 已讨论并理解：
  - 守卫语句 / 提前返回。
  - `bool played = CurrentPlayer.PlayCard(card)` 的含义。
  - `if (!played) return false;` 为什么不是“成功就返回 false”。
  - `CheckGameOver` 中双方都没死就提前返回的逻辑。
  - `RemoveDeadMinions` 为什么倒序遍历删除。

## 当前设计理解

当前 Core 层的基本分工：

```text
CardData     静态卡牌模板
Card         手牌/牌库中的运行时卡牌
Hero         英雄生命主体
Player       玩家资源：英雄、手牌、牌库、法力
Minion       场上的随从
Board        战场随从列表
GameManager  对局裁判和流程调度
```

核心原则：

- 静态数据和运行时状态分离。
- 玩家资源、战场、对局流程分开管理。
- Core 层不依赖 UI。
- UI 以后只读取 Core 状态，并调用 `GameManager` 方法。

## 下一步

下一步进入 UI 和交互，让底层逻辑在 Unity 中可见、可操作。

建议顺序：

1. `CardView.cs`：显示单张卡牌。
2. `HandView.cs`：显示玩家手牌列表。
3. 点击手牌调用 `GameManager.TryPlayMinionCard(card)`。
4. `MinionView.cs` / `BoardView.cs`：显示场上随从。
5. 结束回合按钮调用 `GameManager.EndTurn()`。
6. 法力和英雄血量 UI。

按照协作规则，下一次开始写新类前，先列属性清单，让用户确认后再写。

## 当前建议提交

如果 `CORE_ARCHITECTURE.md` 和 `CURRENT_STATUS.md` 还未提交，建议下一次提交：

```text
docs: 补充核心架构与当前进度说明
```
