# 阶段 1 复盘路线

这份文档的目标不是让你一次性背下阶段 1 的所有代码，而是给你一条可以反复走的路线。

你现在遇到的困难很正常：语法看懂，不等于能在脑子里稳定还原运行流程。尤其是 UI 回调、Unity Inspector 绑定、Prefab 动态生成这些内容，本来就需要多轮“追踪一次完整操作”才能真正熟。

## 先建立正确预期

阶段 1 要理解到什么程度才算够？

可以用三个层次判断：

```text
第一层：这一行语法我知道是什么意思。
第二层：这个方法被谁调用，调用后会改变哪些状态。
第三层：玩家点一下 UI 时，我能从 Unity 对象一路追到 Core 规则，再追到 UI 刷新。
```

你现在大概已经在第一层和第二层之间。接下来重点不是继续硬啃语法，而是训练第三层。

## 推荐复盘顺序

不要按文件夹从上到下读。推荐按“依赖越来越多”的顺序读。

```text
1. CardData.cs
2. Card.cs
3. Hero.cs
4. Minion.cs
5. Player.cs
6. Board.cs
7. GameManager.cs
8. CardView.cs
9. HandView.cs
10. MinionView.cs
11. BoardView.cs
12. GameUIController.cs
```

原因：

- `CardData` / `Card` / `Hero` / `Minion` 是比较小的对象，先建立信心。
- `Player` 和 `Board` 开始管理列表与资源。
- `GameManager` 负责把前面的对象串成规则流程。
- UI 文件最后读，因为它们依赖 Core，并且有 Unity 生命周期、Inspector 绑定、回调。

## 第一轮：只问“这个类是谁”

每个类先只回答四个问题，不要急着逐行钻进去。

```text
1. 这个类代表游戏里的什么东西？
2. 它保存了哪些状态？
3. 它提供了哪些操作？
4. 它不应该负责什么？
```

例如 `Player`：

```text
它代表一名玩家。
它保存 Hero、Hand、Deck、Mana。
它提供 StartTurn、DrawCard、PlayCard、ShuffleDeck。
它不负责战场站位，不负责攻击结算，不负责 UI 显示。
```

这一轮的目的：先把职责边界装进脑子里。职责不清楚时，看任何代码都会像散的。

## 第二轮：按“状态变化”读

游戏代码最重要的不是“这行语法是什么”，而是“这行改变了什么状态”。

读方法时，用这个格式做笔记：

```text
方法名：
输入：
会提前失败的情况：
成功后改变的状态：
返回值代表什么：
谁会调用它：
```

例如 `Player.PlayCard(card)`：

```text
输入：一张 Card
会提前失败的情况：card 是 null；card 不在手牌；费用不够
成功后改变的状态：CurrentMana 减少；Hand 移除这张牌
返回值：true 表示成功打出，false 表示失败
谁会调用它：GameManager.TryPlayMinionCard
```

这种读法比“逐字翻译语法”更接近工程实际。

## 第三轮：追踪完整玩家操作

阶段 1 最重要的四条链路是：

```text
开局
点击手牌出随从
点击结束回合
点击随从攻击随从 / 英雄
```

每次只追一条链路。不要同时追所有代码。

### 链路 1：开局

入口：

```csharp
GameManager.Awake()
```

你要能讲出：

```text
Unity 进入 Play 模式
GameManager.Awake 被 Unity 调用
StartNewGame 创建 Player / Enemy / Board
双方抽起手牌
StartTurn 进入 Player 的第一回合
GameUIController.Start 后刷新 UI
```

关键理解：

```text
GameManager 先在 Awake 准备 Core 状态。
GameUIController 后在 Start 读取这些状态并显示。
```

### 链路 2：点击手牌出随从

入口：

```text
玩家点击 CardViewPrefab 实例上的 Button
```

完整链路：

```text
Button.onClick
-> CardView.HandleClick()
-> onClicked?.Invoke(card)
-> GameUIController.HandleCardClicked(card)
-> GameManager.TryPlayMinionCard(card)
-> CurrentPlayer.PlayCard(card)
-> new Minion(card.CardData, CurrentPlayer)
-> Board.SummonMinion(minion)
-> GameUIController.RefreshAll()
-> HandView / BoardView 重新生成 UI
```

这一条链路是理解 UI 回调的核心。

### 链路 3：点击结束回合

入口：

```text
玩家点击 EndTurnButton
```

完整链路：

```text
Button.onClick
-> GameUIController.HandleEndTurnClicked()
-> GameManager.EndTurn()
-> GameManager.GetOpponent(CurrentPlayer)
-> GameManager.StartTurn(nextPlayer)
-> Player.StartTurn()
-> 当前玩家切换、法力刷新、抽牌、随从恢复攻击
-> GameUIController.RefreshAll()
```

关键理解：

```text
Button 不知道什么叫回合。
GameUIController 知道这个按钮应该调用 EndTurn。
GameManager 才真正改变回合状态。
```

### 链路 4：随从攻击

入口：

```text
玩家先点己方 Ready 随从，再点敌方随从或敌方英雄
```

随从攻击随从：

```text
MinionView.HandleClick()
-> onClicked?.Invoke(minion)
-> GameUIController.HandleMinionClicked(clickedMinion)
-> 第一次点击：SelectAttacker(clickedMinion)
-> 第二次点击：TryAttackSelectedTarget(target)
-> GameManager.TryAttackMinion(selectedAttacker, target)
-> 双方 TakeDamage
-> attacker.SetCanAttack(false)
-> CleanupDeadMinions()
-> CheckGameOver()
-> ClearSelectedAttacker()
-> RefreshAll()
```

随从攻击英雄：

```text
先选中 selectedAttacker
-> 点击英雄 Button
-> GameUIController.HandleEnemyHeroClicked()
-> TryAttackSelectedHero(enemy.Hero)
-> GameManager.TryAttackHero(selectedAttacker, enemy.Hero)
-> Hero.TakeDamage
-> attacker.SetCanAttack(false)
-> CheckGameOver()
-> ClearSelectedAttacker()
-> RefreshAll()
```

关键理解：

```text
GameUIController 负责记住 selectedAttacker。
GameManager 负责判断攻击是否合法并结算。
```

## Unity 和代码怎么关联

你可以把 Unity 场景理解成“对象和引用的装配现场”。

代码里这些字段：

```csharp
[SerializeField] private HandView handView;
[SerializeField] private Button endTurnButton;
[SerializeField] private Text manaText;
```

意思是：

```text
代码需要这些对象。
Unity Inspector 负责把场景里的对象拖给这些字段。
运行时脚本就可以通过字段操作它们。
```

所以 Unity 关联重点不是背窗口操作，而是理解这句话：

```text
脚本定义需要什么引用，Inspector 把实际场景对象填进去。
```

Prefab 的作用：

```text
CardViewPrefab 是一张卡牌 UI 模板。
HandView 根据手牌数据 Instantiate 多个 CardViewPrefab。
每个 CardView 再通过 SetCard 绑定一张具体 Card。
```

这就是“数据驱动 UI”。

## 每次复盘只做一个小任务

推荐你之后按这样的节奏复盘：

```text
第 1 次：只读 Core 小类，整理每个类的职责。
第 2 次：只追开局流程。
第 3 次：只追出牌流程。
第 4 次：只追结束回合流程。
第 5 次：只追攻击流程。
第 6 次：只看 UI 回调，不看规则细节。
第 7 次：打开 Unity，逐个 Inspector 字段对照脚本。
```

一次只解决一个问题，会比从头到尾硬读更有效。

## 建议的主动练习

不要只看代码。每次读完一条流程，做一个小实验。

```text
1. 在某个关键方法里加 Debug.Log。
2. 进入 Play 模式。
3. 点击一次 UI。
4. 看 Console 输出顺序是否和你预测一致。
```

例如出牌流程可以临时加：

```csharp
Debug.Log("CardView.HandleClick");
Debug.Log("GameUIController.HandleCardClicked");
Debug.Log("GameManager.TryPlayMinionCard");
Debug.Log("Player.PlayCard");
Debug.Log("Board.SummonMinion");
```

实验结束后再删掉这些日志。

这比单纯读文档更容易把回调链固定在脑子里。

## 什么时候算真正理解阶段 1

你不需要背出所有代码，但最好能做到：

```text
看到一张手牌被点击，能说出它会一路走到 TryPlayMinionCard。
看到一个随从被点击，能说出第一次点击是选攻击者，第二次点击是找目标。
看到 RefreshAll，知道它不是改变规则，而是重新读取 Core 状态显示到 UI。
看到 [SerializeField]，知道它需要在 Unity Inspector 里拖引用。
看到 Action<Card>，知道它保存的是“卡牌被点击后要通知谁”。
```

如果能讲清楚这些，阶段 1 的主干你就抓住了。
