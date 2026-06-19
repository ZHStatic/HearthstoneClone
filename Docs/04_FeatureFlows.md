# Feature Flows

本文档记录当前项目中已经实现的核心功能流程。

目标是把代码流程翻译成人话，方便复盘、调试和面试讲解。

类职责和依赖关系放在 `Docs/02_CoreArchitecture.md`。
UI 层结构放在 `Docs/03_UIArchitecture.md`。

## 开局流程

入口：

```csharp
GameManager.Awake()
```

流程：

```text
1. Unity 进入 Play 模式。
2. GameManager.Awake() 被调用。
3. Awake() 调用 StartNewGame()。
4. 创建 Player。
5. 创建 Enemy。
6. 创建 Board。
7. 双方抽起手牌。
8. 进入玩家第一个回合。
```

为什么用 `Awake`：

```text
Awake 会早于其他脚本的 Start 执行。
这样 UI 脚本在 Start 里刷新时，GameManager 已经准备好 Player、Enemy 和 Board。
```

## 创建牌库流程

入口：

```csharp
new Player(deckCards, heroName)
```

流程：

```text
1. 创建 Hero。
2. 创建空 Deck。
3. 遍历 Inspector 里配置的 CardData 列表。
4. 如果某个 CardData 是空的，就跳过。
5. 如果 CardData 有效，就创建 Card 实例并加入 Deck。
6. 洗牌。
7. 创建空 Hand。
8. 法力初始化为 0。
```

这里处理过一次真实问题：

```text
如果牌库列表里有 None (Card Data)，以前会触发 NullReferenceException。
现在 Player 会跳过空卡牌数据，避免开局直接崩溃。
```

## 回合开始流程

入口：

```csharp
GameManager.StartTurn(targetPlayer)
```

流程：

```text
1. 如果 targetPlayer 是空，直接结束。
2. 如果游戏已经结束，直接结束。
3. 设置 CurrentPlayer。
4. TurnNumber + 1。
5. 调用 targetPlayer.StartTurn()。
6. 玩家最大法力 +1，最多 10。
7. 当前法力补满。
8. 手牌费用重置。
9. 抽一张牌。
10. 当前玩家场上的随从变成可以攻击。
```

## 点击手牌出牌流程

入口：

```text
玩家点击 CardView
```

流程：

```text
1. CardView 接收到 Button 点击。
2. CardView 调用 onClicked(card)。
3. GameUIController.HandleCardClicked(card) 被调用。
4. GameUIController 先做轻量 UI 检查，用于显示卡牌无效、费用不足等提示。
5. 如果 card.CardData.CardType 是 Minion，进入随从召唤流程。
6. 如果 card.CardData.CardType 是 Spell，记录 selectedSpellCard，并提示玩家选择法术目标。
```

## 随从牌召唤流程

入口：

```text
玩家点击一张随从牌
```

流程：

```text
1. GameUIController.HandleCardClicked(card) 确认这是一张 Minion 卡。
2. GameUIController 检查战场是否存在、当前玩家战场是否已满。
3. GameUIController 调用 GameManager.TryPlayMinionCard(card)。
4. GameManager 通过 CanPlayCard(card) 检查通用出牌条件。
5. GameManager 确认卡牌类型是 Minion。
6. CurrentPlayer.PlayCard(card) 检查手牌和法力。
7. 如果成功，Player 扣除法力并从手牌移除卡牌。
8. GameManager 创建 Minion。
9. Board.SummonMinion(minion) 把随从加入战场。
10. 如果随从拥有 `Charge`，GameManager 让它立刻可以攻击。
11. GameUIController 设置操作反馈并调用 RefreshAll() 刷新手牌、战场和 HUD。
```

这条流程体现的分层：

```text
CardView 只知道自己被点了。
GameUIController 负责把点击交给 GameManager。
GameManager 负责规则流程。
Player 负责手牌和法力。
Board 负责战场随从列表。
UI 最后重新读取 Core 状态并显示操作结果。
```

### 冲锋随从补充流程

入口：

```text
玩家点击一张带有 Charge 关键词的随从牌
```

流程：

```text
1. CardData 中的 Keywords 配置了 Charge。
2. GameManager 创建 Minion。
3. Minion 从 CardData 复制关键词。
4. Board.SummonMinion(minion) 召唤成功。
5. GameManager.ApplySummonKeywords(minion) 检查 minion.HasKeyword(KeywordType.Charge)。
6. 如果有冲锋，调用 minion.SetCanAttack(true)。
7. UI 刷新后，新随从会直接显示 Ready。
8. 玩家可以在召唤当回合用它攻击敌方随从或英雄。
```

当前阶段性简化：

```text
冲锋不使用事件系统。
冲锋只改变召唤后的攻击权限，不影响攻击目标和伤害结算。
```

## 法术牌施放流程

入口：

```text
玩家点击一张法术牌，再点击一个随从或英雄
```

当前测试法术：

```text
火花：1 费，造成 2 点伤害，目标类型 AnyCharacter
```

流程：

```text
1. 玩家点击火花。
2. CardView 调用 onClicked(card)。
3. GameUIController.HandleCardClicked(card) 被调用。
4. GameUIController 检查游戏状态、当前玩家、手牌归属和法力。
5. GameUIController 发现 card.CardData.CardType 是 Spell。
6. GameUIController 记录 selectedSpellCard = card。
7. UI 显示“已选择火花，请选择法术目标”。
8. 玩家点击一个随从或英雄。
9. 如果点击随从，GameUIController 调用 TryPlaySelectedSpellOnMinion(target)。
10. 如果点击英雄，GameUIController 调用 TryPlaySelectedSpellOnHero(targetHero)。
11. GameUIController 把请求交给 GameManager.TryPlaySpellCardOnMinion / OnHero。
12. GameManager 通过 CanPlayCard(card) 检查通用出牌条件。
13. GameManager 根据 SpellTargetType 判断目标是否合法。
14. CurrentPlayer.PlayCard(card) 扣除法力，并从手牌移除法术牌。
15. 目标受到 SpellDamage 点伤害。
16. GameManager 清理死亡随从并检查胜负。
17. GameUIController 清空 selectedSpellCard，显示反馈并刷新 UI。
```

这条流程的分层：

```text
CardView 只负责转发点击。
GameUIController 负责记录“当前选中的法术牌”和玩家选择的目标。
GameManager 负责判断法术能否打出、目标是否合法、伤害如何结算。
Player 负责扣法力和移除手牌。
Hero / Minion 负责承受伤害。
```

当前阶段性简化：

```text
只支持单目标伤害法术。
不支持治疗、Buff、抽牌、召唤、AOE 和随机目标。
还没有事件系统，法术伤害直接由 GameManager 调用 TakeDamage。
```

## 结束回合流程

入口：

```text
玩家点击 EndTurnButton
```

流程：

```text
1. Button 调用 GameUIController.HandleEndTurnClicked()。
2. GameUIController 调用 GameManager.EndTurn()。
3. GameManager 找到当前玩家的对手。
4. GameManager.StartTurn(nextPlayer)。
5. 新的当前玩家开始回合。
6. GameUIController.RefreshAll()。
7. UI 显示新的当前玩家手牌、法力和战场状态。
```

当前临时调试规则：

```text
AI 还没做，所以 UI 暂时显示当前行动者的手牌。
这样可以手动测试双方出牌和回合切换。
```

## 随从攻击随从流程

入口：

```text
玩家点击己方随从，再点击敌方随从
```

流程：

```text
1. MinionView 接收到随从点击。
2. BoardView 传递点击回调。
3. GameUIController.HandleMinionClicked(clickedMinion) 被调用。
4. 如果当前没有 selectedAttacker，尝试选择当前玩家的可攻击随从。
5. 选择成功后，GameUIController 记录 selectedAttacker，BoardView 刷新时让对应 MinionView 高亮。
6. 如果已经有 selectedAttacker，再点击敌方随从。
7. GameUIController 调用 GameManager.TryAttackMinion(selectedAttacker, target)。
8. GameManager 检查攻击者、目标、阵营和攻击权限。
9. 双方随从互相造成攻击力数值的伤害。
10. 攻击者 CanAttack = false。
11. 清理死亡随从。
12. 检查胜负。
13. GameUIController 清空 selectedAttacker，显示攻击结果提示并刷新 UI。
```

这条流程的分层：

```text
MinionView 只负责告诉上层哪个随从被点了。
GameUIController 负责记录攻击者和目标，并显示选中高亮和操作反馈。
GameManager 负责判断攻击是否合法并结算伤害。
```

## 随从攻击英雄流程

入口：

```text
玩家点击己方随从，再点击敌方英雄按钮
```

流程：

```text
1. 玩家先点击己方 Ready 随从。
2. GameUIController 把该随从记录为 selectedAttacker，并刷新选中高亮。
3. 玩家点击敌方英雄按钮。
4. GameUIController 调用 TryAttackSelectedHero(targetHero)。
5. TryAttackSelectedHero 调用 GameManager.TryAttackHero(selectedAttacker, targetHero)。
6. GameManager 检查攻击者、目标英雄和阵营。
7. 目标英雄受到攻击者攻击力数值的伤害。
8. 攻击者 CanAttack = false。
9. GameManager 检查胜负。
10. GameUIController 清空 selectedAttacker，显示攻击结果提示并刷新 UI。
```

补充规则：

```text
如果点击的是自己的英雄，GameManager.TryAttackHero 会返回失败。
UI 不自己判断这件事，规则仍然留在 Core 层。
```

## UI 刷新流程

入口：

```csharp
GameUIController.RefreshAll()
```

流程：

```text
1. 如果没有 GameManager，就清空 UI。
2. 刷新当前行动者手牌。
3. 刷新玩家战场。
4. 刷新敌方战场。
5. 刷新当前玩家、回合数、法力、英雄血量、操作反馈和游戏结束文字。
```

为什么当前用手动刷新：

```text
当前操作点还不多。
手动刷新更直观，方便学习和调试。
阶段 2 加入事件系统后，可以逐步改成事件驱动刷新。
```
