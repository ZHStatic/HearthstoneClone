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
2. 创建内部空 hand。
3. 创建内部空 deck。
4. 遍历 Inspector 里配置的 CardData 列表。
5. 如果某个 CardData 是空的，就跳过。
6. 如果 CardData 有效，就创建 Card 实例并加入内部 deck。
7. 洗牌。
8. 对外通过只读的 Hand / Deck 属性暴露当前手牌和牌库。
9. 法力初始化为 0。
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
10. BattleLogger 记录回合开始。
11. 当前玩家场上的随从变成可以攻击。
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
4. GameUIController 先判断卡牌类型；具体规则检查交给 GameManager。
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
2. GameUIController 调用 GameManager.TryPlayMinionCardDetailed(card)。
3. GameManager 通过 ValidatePlayMinionCard(card) 检查游戏状态、当前玩家、手牌归属、法力、卡牌类型和战场空间。
4. 如果校验失败，GameManager 返回 GameActionResult，UI 显示其中的失败反馈。
5. 如果校验成功，进入随从召唤流程。
6. CurrentPlayer.PlayCard(card) 检查手牌和法力。
7. 如果成功，Player 扣除法力并从手牌移除卡牌。
8. GameManager 创建 Minion。
9. Board.SummonMinion(minion) 把随从加入战场。
10. BattleLogger 记录出牌和召唤。
11. GameManager 调用 ResolveAfterSummon(minion) 处理召唤后结算。
12. 如果随从拥有 `Charge`，GameManager 让它立刻可以攻击。
13. 如果这张牌配置了战吼，GameManager 结算战吼效果。
14. 如果这张牌有伤害战吼，BattleLogger 记录尝试伤害和实际伤害。
15. 如果随从拥有 `Taunt`，它会在后续攻击目标判断中被识别为嘲讽随从。
16. GameUIController 读取 GameActionResult.Message，设置操作反馈并调用 RefreshAll() 刷新手牌、战场和 HUD。
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

### 嘲讽随从补充流程

入口：

```text
玩家尝试攻击对方随从或英雄
```

流程：

```text
1. CardData 中的 Keywords 配置了 Taunt。
2. GameManager 创建 Minion。
3. Minion 从 CardData 复制关键词。
4. UI 刷新后，CardView 和 MinionView 可以显示“嘲讽”。
5. 玩家选择一个己方可攻击随从。
6. 玩家点击敌方随从或敌方英雄。
7. GameManager 先确认攻击者是否可以攻击。
8. 如果目标是敌方英雄，GameManager 检查敌方是否有活着的嘲讽随从。
9. 如果敌方有活着的嘲讽随从，攻击英雄失败。
10. 如果目标是敌方随从，GameManager 检查敌方是否有活着的嘲讽随从。
11. 如果敌方有活着的嘲讽随从，目标必须也是嘲讽随从，否则攻击失败。
12. 如果目标合法，才进入正常伤害结算。
```

当前阶段性简化：

```text
嘲讽不使用事件系统。
嘲讽只限制随从/英雄攻击目标，不限制法术选目标。
```

### 战吼随从补充流程

入口：

```text
玩家点击一张带有 BattlecryType 的随从牌
```

当前测试战吼：

```text
火焰学徒：2 费，2/2，战吼：对敌方英雄造成 1 点伤害
```

流程：

```text
1. CardData 中配置 BattlecryType = DealDamageToEnemyHero。
2. CardData 中配置 BattlecryValue = 1。
3. CardView 刷新手牌时显示“战吼：对敌方英雄造成 1 点伤害”。
4. 玩家点击火焰学徒。
5. GameUIController 调用 GameManager.TryPlayMinionCard(card)。
6. GameManager 检查出牌条件并让 Player 扣法力、移除手牌。
7. GameManager 创建 Minion。
8. Board.SummonMinion(minion) 把随从加入战场。
9. GameManager.ResolveAfterSummon(minion) 被调用。
10. ResolveAfterSummon 先处理冲锋，再处理战吼。
11. ResolveBattlecry 识别 DealDamageToEnemyHero。
12. GameManager 找到出牌者的对手。
13. 敌方 Hero 承受 BattlecryValue 点伤害。
14. BattleLogger 记录战吼来源、目标、尝试伤害和实际伤害。
15. GameManager.CheckGameOver() 检查战吼是否已经打死英雄。
16. GameUIController 刷新 UI，显示新的英雄血量和战场状态。
```

当前阶段性简化：

```text
战吼暂时不通过 GameEventBus 结算。
当前只支持不需要选目标的战吼。
前两个战吼分别固定打敌方英雄和为出牌者抽牌，避免同时引入出牌选目标 UI。
事件系统基础链路和死亡事件已接入，亡语通过 MinionDied 事件触发。
```

### 阶段 2.4 测试步骤

在 Unity Editor 中操作：

```text
1. 等 Unity 自动导入 BattlecryType.cs，并确认 Console 没有编译错误。
2. 在 Project 面板中创建一张新的 CardData，命名为“火焰学徒”。
3. 设置 Card Type = Minion。
4. 设置 Cost = 2。
5. 设置 Attack = 2。
6. 设置 Health = 2。
7. 设置 Battlecry Type = DealDamageToEnemyHero。
8. 设置 Battlecry Value = 1。
9. 把“火焰学徒”加入 GameManager 的 Player Deck Data。
10. 进入 Play 模式。
11. 抽到火焰学徒后，确认手牌描述显示战吼文字。
12. 打出火焰学徒。
13. 确认火焰学徒进入战场。
14. 确认敌方英雄生命立刻减少 1。
15. 如果测试高伤害战吼，确认敌方英雄生命归零后会 Game Over。
```

### 阶段 2.4.5 抽牌战吼测试步骤

在 Unity Editor 中操作：

```text
1. 等 Unity 自动导入脚本，并确认 Console 没有编译错误。
2. 创建一张新的 CardData，例如“书卷侍从”。
3. 设置 Card Type = Minion。
4. 设置 Cost = 2。
5. 设置 Attack = 1。
6. 设置 Health = 2。
7. 设置 Battlecry Type = DrawCard。
8. 设置 Battlecry Value = 1。
9. 把“书卷侍从”加入 GameManager 的 Player Deck Data。
10. 进入 Play 模式。
11. 抽到书卷侍从后，确认手牌描述显示“战吼：抽 1 张牌”。
12. 打出书卷侍从。
13. 确认书卷侍从进入战场。
14. 确认己方手牌数量先因打出书卷侍从减少 1，再因战吼抽牌增加 1。
15. 如果手牌满，当前规则会调用 Player.DrawCard() 烧掉牌库顶牌，不额外显示提示。
```

### 阶段 2.5.0 事件系统基础链路测试步骤

在 Unity Editor 中操作：

```text
1. 等 Unity 自动导入 Assets/Scripts/Core/Events 下的新脚本，并确认 Console 没有编译错误。
2. 进入 Play 模式。
3. 让一个随从死亡。
4. 确认亡语等依赖死亡事件的规则可以正常触发。
```

说明：阶段 2.5.0 曾经用 `CardPlayed` / `MinionSummoned` 和 Console 日志验证事件总线。
进入阶段 3 后，这些只用于调试输出的事件已经删除，避免干扰 AI 行动日志。
当前事件系统只保留真正影响规则的 `MinionDied`。

### 阶段 2.5.1 死亡事件测试步骤

在 Unity Editor 中操作：

```text
1. 进入 Play 模式。
2. 让一个随从被攻击打死，或用火花打死一个随从。
3. 如果死亡随从配置了亡语，确认亡语效果会结算。
4. 如果死亡随从没有亡语，确认随从会从战场移除，且没有额外 GameEvent 调试日志刷屏。
```

### 亡语随从补充流程

入口：

```text
一个配置了 DeathrattleType 的随从死亡
```

当前测试亡语：

```text
亡语炸弹人：2 费，1/1，亡语：对敌方英雄造成 1 点伤害
```

流程：

```text
1. CardData 中配置 DeathrattleType = DealDamageToEnemyHero。
2. CardData 中配置 DeathrattleValue = 1。
3. CardView 刷新手牌时显示“亡语：对敌方英雄造成 1 点伤害”。
4. 玩家打出亡语随从。
5. GameManager 创建 Minion，并把它召唤到 Board。
6. MinionView 刷新场上随从时显示“亡语:1”。
7. 这个随从被攻击或法术伤害打死。
8. GameManager.CleanupDeadMinions() 检查到 minion.IsDead。
9. GameManager.PublishMinionDied(minion) 发布死亡事件。
10. GameEventBus 通知 ResolveDeathrattleOnMinionDied(gameEvent)。
11. GameManager 从 gameEvent.TargetMinion 取出死亡随从。
12. ResolveDeathrattle(minion) 检查死亡随从是否有亡语。
13. 如果亡语类型是 DealDamageToEnemyHero，就找到死亡随从拥有者的对手。
14. 敌方 Hero 承受 DeathrattleValue 点伤害。
15. BattleLogger 记录亡语来源、目标、尝试伤害和实际伤害。
16. GameManager.CheckGameOver() 检查亡语是否已经打死英雄。
17. 死亡随从随后从 Board 移除。
18. GameUIController 刷新 UI，显示新的英雄血量和战场状态。
```

当前阶段性简化：

```text
当前只支持一个亡语类型：对敌方英雄造成伤害。
当前不支持亡语召唤、亡语抽牌、亡语打随从或 AOE。
当前亡语结算仍放在 GameManager，等亡语类型变多或出现死亡连锁时再拆 DeathProcessor。
```

### 阶段 2.6 亡语测试步骤

在 Unity Editor 中操作：

```text
1. 等 Unity 自动导入 DeathrattleType.cs，并确认 Console 没有编译错误。
2. 创建一张新的 CardData，例如“亡语炸弹人”。
3. 设置 Card Type = Minion。
4. 设置 Cost = 2。
5. 设置 Attack = 1。
6. 设置 Health = 1。
7. 设置 Deathrattle Type = DealDamageToEnemyHero。
8. 设置 Deathrattle Value = 1。
9. 把“亡语炸弹人”加入 GameManager 的 Player Deck Data。
10. 进入 Play 模式。
11. 抽到亡语炸弹人后，确认手牌描述显示“亡语：对敌方英雄造成 1 点伤害”。
12. 打出亡语炸弹人。
13. 确认场上随从状态区显示“亡语:1”。
14. 让亡语炸弹人被攻击或用火花打死。
15. 确认 Console 输出 MinionDied。
16. 确认敌方英雄生命减少 1。
17. 确认亡语炸弹人从战场移除。
18. 如果把 Deathrattle Value 临时调高到足以斩杀，确认敌方英雄生命归零后会 Game Over。
```

### 圣盾随从补充流程

入口：

```text
一个配置了 DivineShield 的随从受到正数伤害
```

当前测试圣盾：

```text
圣盾卫士：2 费，2/2，关键词：圣盾
```

流程：

```text
1. CardData 中配置 Keywords 包含 DivineShield。
2. CardView 刷新手牌时显示“圣盾”。
3. 玩家打出圣盾随从。
4. GameManager 创建 Minion。
5. Minion 从 CardData.Keywords 复制 DivineShield。
6. Board.SummonMinion(minion) 把随从加入战场。
7. MinionView 刷新场上随从时显示“圣盾”。
8. 这个随从第一次被攻击或被火花命中。
9. GameManager 调用 DamageMinion(target, amount, sourceName, sourcePlayer, entryType)。
10. DamageMinion 先记录目标是否有圣盾。
11. DamageMinion 调用 Minion.TakeDamage(amount)。
12. Minion.TakeDamage 检查到 HasDivineShield 为 true。
13. Minion.RemoveKeyword(DivineShield) 移除圣盾。
14. Minion.TakeDamage 返回 0，CurrentHealth 不减少。
15. BattleLogger 记录本次尝试伤害为正数、实际伤害为 0，并标记圣盾抵消。
16. GameUIController 刷新 UI，场上随从不再显示“圣盾”。
17. 这个随从第二次受到伤害。
18. Minion.TakeDamage 正常扣除 CurrentHealth。
19. 如果 CurrentHealth 小于等于 0，GameManager.CleanupDeadMinions() 会按已有死亡流程处理。
```

当前阶段性简化：

```text
圣盾暂时直接写在 Minion.TakeDamage()。
当前不发布 DamagePrevented 或 ShieldBroken 事件，但会记录 `DivineShieldPrevented` 战斗日志。
当前没有圣盾破裂动画、音效或专用图标。
后续如果出现免疫、法术伤害加成、吸血、伤害翻倍等机制，再抽 DamageResolver 或 CombatResolver。
```

### 阶段 2.7 圣盾测试步骤

在 Unity Editor 中操作：

```text
1. 等 Unity 自动导入 KeywordType.cs、Minion.cs、CardView.cs 和 MinionView.cs，并确认 Console 没有编译错误。
2. 创建一张新的 CardData，例如“圣盾卫士”。
3. 设置 Card Type = Minion。
4. 设置 Cost = 2。
5. 设置 Attack = 2。
6. 设置 Health = 2。
7. 在 Keywords 列表中添加 DivineShield。
8. 把“圣盾卫士”加入 GameManager 的 Player Deck Data。
9. 进入 Play 模式。
10. 抽到圣盾卫士后，确认手牌描述显示“圣盾”。
11. 打出圣盾卫士。
12. 确认场上随从状态区显示“圣盾”。
13. 用火花打圣盾卫士，或让敌方随从攻击圣盾卫士。
14. 确认圣盾卫士第一次受到伤害后生命值不变。
15. 确认场上随从状态区不再显示“圣盾”。
16. 再次让圣盾卫士受到伤害。
17. 确认第二次伤害会正常扣除生命值。
18. 如果第二次伤害足以致死，确认随从会进入已有死亡清理流程。
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
11. GameUIController 把请求交给 GameManager.TryPlaySpellCardOnMinionDetailed / TryPlaySpellCardOnHeroDetailed。
12. GameManager 通过 ValidatePlaySpellCard(card) 检查游戏状态、当前玩家、手牌归属、法力和卡牌类型。
13. GameManager 通过 ValidateSpellTargetMinion / ValidateSpellTargetHero 根据 SpellTargetType 判断目标是否合法。
14. CurrentPlayer.PlayCard(card) 扣除法力，并从手牌移除法术牌。
15. GameManager 调用 DamageMinion 或 DamageHero 结算伤害。
16. BattleLogger 记录法术来源、目标、尝试伤害和实际伤害。
17. 如果目标随从用圣盾抵消伤害，BattleLogger 记录圣盾抵消。
18. GameManager 清理死亡随从并检查胜负。
19. GameUIController 清空 selectedSpellCard，显示 GameActionResult.Message，并刷新 UI。
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
当前已有出牌、召唤和死亡事件日志，也已有 BattleLogger 记录法术伤害结果。
伤害结算仍然不通过 GameEventBus；这是阶段性简化，不是成熟项目最终做法。
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
3. GameManager 用 BattleLogger 记录当前玩家回合结束。
4. GameManager 找到当前玩家的对手。
5. GameManager.StartTurn(nextPlayer)。
6. 新的当前玩家开始回合，并记录回合开始日志。
7. GameUIController.RefreshAll()。
8. UI 显示新的当前玩家手牌、法力和战场状态。
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
8. GameManager 检查攻击者、目标、阵营、攻击权限和嘲讽限制。
9. 如果目标合法，BattleLogger 记录攻击行为。
10. 双方随从通过 DamageMinion 互相造成攻击力数值的伤害。
11. BattleLogger 记录双方尝试伤害和实际伤害；圣盾抵消时实际伤害为 0。
12. 攻击者 CanAttack = false。
13. 清理死亡随从，并记录死亡日志。
14. 检查胜负。
15. GameUIController 清空 selectedAttacker，显示攻击结果提示并刷新 UI。
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
6. GameManager 检查攻击者、目标英雄、阵营和嘲讽限制。
7. 如果对方场上有活着的嘲讽随从，攻击英雄失败。
8. 如果目标合法，BattleLogger 记录攻击行为。
9. 目标英雄通过 DamageHero 受到攻击者攻击力数值的伤害。
10. BattleLogger 记录尝试伤害和实际伤害。
11. 攻击者 CanAttack = false。
12. GameManager 检查胜负。
13. GameUIController 清空 selectedAttacker，显示攻击结果提示并刷新 UI。
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
