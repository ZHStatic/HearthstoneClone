# 逐行读代码检查表

这份文档是你复盘阶段 1 时可以反复使用的“读代码模板”。

当你感觉“每一句语法都看懂，但合起来又散了”时，通常不是语法问题，而是没有把代码放回这几个问题里：

```text
它属于谁？
它什么时候执行？
它读了哪些状态？
它改了哪些状态？
它把结果交给了谁？
```

## 读一个类之前

先不要看方法体，先填这个表。

```text
类名：
它代表什么：
它属于 Core 还是 UI：
它被谁创建：
它被谁引用：
它引用了谁：
它保存哪些状态：
它负责哪些行为：
它故意不负责什么：
```

示例：`CardView`

```text
类名：CardView
它代表什么：屏幕上的一张手牌 UI
它属于 Core 还是 UI：UI
它被谁创建：HandView 通过 Instantiate 创建
它被谁引用：HandView 临时保存到 cardViews 列表
它引用了谁：Text、Button、Card、Action<Card>
它保存哪些状态：当前显示的 card，点击回调 onClicked
它负责哪些行为：显示卡牌文本，按钮点击后通知上层
它故意不负责什么：不判断能不能出牌，不扣法力，不召唤随从
```

## 读一个字段时

每看到一个字段，问：

```text
这个字段保存什么？
谁给它赋值？
谁会读取它？
谁可以修改它？
为什么不是局部变量？
```

例子：

```csharp
private Minion selectedAttacker;
```

回答：

```text
保存当前选中的攻击者。
SelectAttacker 会给它赋值。
TryAttackSelectedTarget / TryAttackSelectedHero 会读取它。
ClearSelectedAttacker 会清空它。
它需要跨越两次点击存在，所以不能只是局部变量。
```

这个字段特别适合理解“UI 状态”：

```text
第一次点击随从，只是记录 selectedAttacker。
第二次点击目标，才拿 selectedAttacker 去攻击。
```

## 读一个属性时

看到属性，尤其是 `private set`，问：

```text
外部能不能读？
外部能不能改？
如果不能改，应该通过哪个方法改变？
```

例子：

```csharp
public int CurrentHealth { get; private set; }
```

回答：

```text
外部能读 CurrentHealth。
外部不能直接改 CurrentHealth。
要通过 TakeDamage 或 Heal 改。
```

这是一种封装：

```text
状态可以被观察，但不能被随便改。
```

## 读一个方法时

用这个模板：

```text
方法名：
入口是谁：
输入参数：
返回值：
提前结束条件：
正常流程：
改变了哪些状态：
调用了哪些别的方法：
谁依赖这个方法的结果：
```

示例：`GameManager.TryPlayMinionCard`

```text
方法名：TryPlayMinionCard
入口是谁：GameUIController.HandleCardClicked
输入参数：Card card
返回值：bool，表示是否成功出牌并召唤
提前结束条件：card 为空；游戏结束；当前玩家为空；战场满
正常流程：让 CurrentPlayer.PlayCard；成功后创建 Minion；交给 Board.SummonMinion
改变了哪些状态：玩家法力、手牌、战场随从列表
调用了哪些别的方法：Board.CanSummon、Player.PlayCard、Board.SummonMinion
谁依赖这个方法的结果：当前 UI 没有直接用返回值，但之后会 RefreshAll 重新显示状态
```

## 读 if return

本项目大量使用守卫语句。

```csharp
if (card == null) return false;
if (IsGameOver) return false;
if (CurrentPlayer == null) return false;
```

固定读法：

```text
先排除不能继续的情况。
只要命中任何一个 return，后面的成功流程就不会执行。
```

你可以在纸上把方法分成两块：

```text
失败出口区
成功流程区
```

这样会比强行找 `else` 更清楚。

## 读 Unity 生命周期方法

本项目里最常见的是：

```text
Awake
Start
OnDestroy
```

可以先粗略理解为：

```text
Awake：对象很早创建时调用，适合准备基础数据。
Start：对象启用后、第一帧前调用，适合找引用、注册按钮、刷新 UI。
OnDestroy：对象销毁时调用，适合取消按钮监听。
```

项目中的例子：

```text
GameManager.Awake -> StartNewGame，先准备 Core 数据。
GameUIController.Start -> 找 GameManager、注册按钮、RefreshAll。
CardView.Awake -> 注册自己的 Button 点击。
CardView.OnDestroy -> 移除自己的 Button 点击。
```

## 读 Instantiate

看到：

```csharp
CardView cardView = Instantiate(cardViewPrefab, cardContainer);
```

不要只翻译成“实例化”。要读成：

```text
根据 cardViewPrefab 这个 UI 模板，
在 cardContainer 下面创建一个新的 CardView 对象，
并把这个新对象保存到 cardView 变量里。
```

然后下一句通常会绑定数据：

```csharp
cardView.SetCard(card, onCardClicked);
```

合起来就是：

```text
创建一个卡牌 UI，并告诉它显示哪张 Card、被点后通知谁。
```

## 读 RefreshAll

`RefreshAll` 很容易误解成“它驱动游戏规则”。其实不是。

固定理解：

```text
Core 规则先改变数据。
RefreshAll 再读取最新数据，重新显示 UI。
```

出牌时：

```text
GameManager.TryPlayMinionCard 改变手牌、法力、战场。
RefreshAll 重新显示手牌、战场、法力。
```

结束回合时：

```text
GameManager.EndTurn 改变 CurrentPlayer、法力、抽牌、随从攻击状态。
RefreshAll 重新显示当前玩家、手牌、战场、法力。
```

攻击时：

```text
GameManager.TryAttackMinion / TryAttackHero 改变生命、攻击状态、死亡清理、胜负。
RefreshAll 重新显示随从、英雄血量、游戏结束文本。
```

一句话：

```text
RefreshAll 是显示层同步，不是规则层结算。
```

## 读 Inspector 绑定字段

看到：

```csharp
[SerializeField] private Text manaText;
```

问：

```text
这个字段要在 Inspector 里拖什么对象？
如果没拖，会不会报错？
代码里有没有 null 防护？
这个字段负责显示什么？
```

示例：

```text
manaText 要拖显示法力的 Text。
如果没拖，SetText 里有 null 防护，不会报错，但也不会显示。
它负责显示 CurrentMana / MaxMana。
```

## 读一条 UI 点击链

把点击流程写成“谁调用谁”。

模板：

```text
Unity Button
-> View.HandleClick
-> onClicked.Invoke(data)
-> GameUIController.HandleXxxClicked(data)
-> GameManager.TryXxx(data)
-> Core 对象改变状态
-> GameUIController.RefreshAll
-> View 重新显示
```

本项目里：

```text
CardView 点击传 Card。
MinionView 点击传 Minion。
HeroButton 点击不需要传 UI 对象，GameUIController 直接找到 Player.Hero / Enemy.Hero。
EndTurnButton 点击不需要传数据，直接调用 EndTurn。
```

## 读完一个方法后的自测

读完方法后，不看代码回答：

```text
1. 这个方法最早可能在哪个玩家操作后执行？
2. 它有几个提前 return？
3. 它成功时至少改变一个什么状态？
4. 它有没有调用 Core 方法？
5. 它有没有调用 UI 刷新？
6. 如果传入 null 会怎样？
```

如果答不上来，不代表你笨，只代表需要从“语法阅读”切回“流程阅读”。

## 推荐手写练习

挑一个方法，手写三行：

```text
之前：
执行：
之后：
```

例子：`Player.DrawCard`

```text
之前：Deck 有 5 张，Hand 有 3 张。
执行：从 Deck 最后一张取出，移除它，加入 Hand。
之后：Deck 有 4 张，Hand 有 4 张。
```

例子：`GameManager.TryAttackMinion`

```text
之前：A 可以攻击，B 是敌方随从。
执行：A 和 B 互相扣血，A 失去攻击权，清理死亡随从。
之后：A 不能再攻击，死亡随从离开 Board。
```

这种练习能把代码从“文字”变成“状态变化”。

## 当前阶段最该掌握的概念

优先级从高到低：

```text
1. 类的职责边界
2. 字段 / 属性 / 方法的区别
3. 构造函数和 new
4. List 的 Add / Remove / Count / 索引
5. if return 守卫语句
6. private set 封装
7. [SerializeField] 和 Inspector 拖引用
8. Unity Awake / Start / OnDestroy
9. Button.onClick.AddListener
10. Action<T> 回调
11. Instantiate 动态生成 UI
12. RefreshAll 手动刷新
```

不要平均用力。阶段 1 的主干是前 8 个，UI 迷雾主要是后 4 个。

## 卡住时应该怎么问

比起问“我不懂回调”，更推荐这样问：

```text
CardView.SetCard 里的 onClicked 是从哪里来的？
CardView.HandleClick 里的 Invoke 最后会进入哪个方法？
HandView.Refresh 创建出来的 CardView 存在哪里？
selectedAttacker 为什么不能写成局部变量？
RefreshAll 是不是会重新创建所有手牌 UI？
```

问题越具体，我就越能帮你把那一段拆到你真正能握住。
