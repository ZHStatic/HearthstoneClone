# UI 回调和 Button 理解指南

这份文档专门解释阶段 1 里最容易卡住的内容：`Button.onClick`、`Action<T>`、`AddListener`、`onClicked?.Invoke(...)`。

先记住一句话：

```text
回调就是：我现在先把一个方法存起来，等某件事发生时再调用它。
```

在本项目里，“某件事”通常就是玩家点击按钮。

## 为什么 UI 要用回调

以 `CardView` 为例。

`CardView` 只代表屏幕上的一张手牌 UI。它应该知道：

```text
怎么显示卡牌名字、费用、攻击、生命。
自己什么时候被点了。
```

它不应该知道：

```text
这张牌能不能出。
出牌要不要扣法力。
出牌后要不要召唤随从。
出牌后怎么判断胜负。
```

所以 `CardView` 被点击时，只做一件事：

```text
把“哪张 Card 被点击了”通知给上层。
```

这个“通知给上层”的方式，就是回调。

## Action<Card> 是什么

代码：

```csharp
private Action<Card> onClicked;
```

可以先翻译成：

```text
onClicked 是一个方法变量。
它保存的方法需要接收一个 Card 参数。
它没有返回值。
```

更口语一点：

```text
等这张牌被点击时，我要调用 onClicked，并把 card 交给它。
```

`Action<T>` 里的 `T` 表示这个方法需要接收什么类型的参数。

```text
Action<Card>   = 接收 Card 的方法
Action<Minion> = 接收 Minion 的方法
```

## SetCard 在做什么

`CardView.SetCard`：

```csharp
public void SetCard(Card card, Action<Card> onClicked)
{
    this.card = card;
    this.onClicked = onClicked;

    Refresh();
}
```

逐句读：

```text
this.card = card;
把外部传进来的 Card 保存到这个 CardView 里。
以后这个 UI 就知道自己显示的是哪张牌。

this.onClicked = onClicked;
把外部传进来的“点击后要调用的方法”保存起来。
以后按钮被点击时，就可以调用它。

Refresh();
根据 card 的数据刷新文字显示。
```

这里最重要的是：

```text
SetCard 不只是设置显示数据。
它还把“点击后通知谁”一起设置好了。
```

## Button.onClick.AddListener 是什么

`CardView.Awake`：

```csharp
private void Awake()
{
    if (button != null)
    {
        button.onClick.AddListener(HandleClick);
    }
}
```

翻译：

```text
当这个 CardView 被创建后，
如果 button 有绑定，
就告诉 Button：
以后你被点击时，请调用我的 HandleClick 方法。
```

这里的关系是：

```text
Unity Button 被点击
-> Button 调用 HandleClick
```

注意：这里还没有出牌。这里只是把 Unity 的按钮点击接到 `CardView.HandleClick`。

## HandleClick 在做什么

`CardView.HandleClick`：

```csharp
private void HandleClick()
{
    if (card == null) return;

    onClicked?.Invoke(card);
}
```

逐句读：

```text
if (card == null) return;
如果这个 UI 当前没有绑定有效卡牌，就什么也不做。

onClicked?.Invoke(card);
如果 onClicked 不为空，就调用它，并把当前 card 传出去。
```

`?.Invoke` 是安全调用。

```text
onClicked?.Invoke(card)
```

大致等价于：

```csharp
if (onClicked != null)
{
    onClicked(card);
}
```

所以它不是神秘语法，只是“如果有回调，就调用回调”。

## 出牌点击的完整时间线

这条链最重要，建议反复看。

```text
1. GameUIController.RefreshHand()
2. handView.SetHand(currentPlayer.Hand, HandleCardClicked)
3. HandView.Refresh(cards)
4. Instantiate(cardViewPrefab, cardContainer)
5. cardView.SetCard(card, onCardClicked)
6. CardView 保存 card
7. CardView 保存 onClicked，也就是 GameUIController.HandleCardClicked
8. 玩家点击这张 CardView 上的 Button
9. Button 调用 CardView.HandleClick
10. CardView 调用 onClicked?.Invoke(card)
11. 实际被调用的是 GameUIController.HandleCardClicked(card)
12. GameUIController 调用 gameManager.TryPlayMinionCard(card)
13. GameManager 执行真正的出牌规则
14. GameUIController.RefreshAll()
```

关键点：

```text
HandleCardClicked 不是玩家点击时才临时找出来的。
它早在 SetHand 的时候就被传给了 HandView，再传给 CardView 保存起来了。
```

## 为什么 CardView 不直接调用 GameManager

如果 `CardView` 直接引用 `GameManager`，代码可能会变成：

```csharp
gameManager.TryPlayMinionCardDetailed(card);
```

看起来更简单，但问题是：

```text
CardView 就知道太多规则了。
以后如果点击卡牌不是立刻出牌，而是先显示详情、拖拽、选择目标，CardView 就得改。
```

现在用回调：

```text
CardView：我被点了，这是我的 Card。
GameUIController：我决定这次点击要出牌。
GameManager：我判断能不能出，并修改 Core 状态。
```

职责更清楚。

## EndTurnButton 为什么不用 Action

`GameUIController.Start`：

```csharp
endTurnButton.onClick.AddListener(HandleEndTurnClicked);
```

结束回合按钮比较简单：

```text
这个按钮固定属于 GameUIController。
它被点击后固定调用 HandleEndTurnClicked。
不需要动态生成，也不需要每个按钮携带不同数据。
```

所以它直接在 `GameUIController` 里注册就够了。

手牌和随从不同：

```text
手牌有很多张，每张 CardView 对应不同 Card。
随从有很多个，每个 MinionView 对应不同 Minion。
```

它们需要通过 `SetCard` / `SetMinion` 把各自的数据和点击回调绑定起来。

## MinionView 的回调和 CardView 一样

`MinionView` 的结构和 `CardView` 几乎一样：

```text
CardView 保存 Card，点击时 Invoke(Card)。
MinionView 保存 Minion，点击时 Invoke(Minion)。
```

对应代码关系：

```text
BoardView.Refresh(minions, HandleMinionClicked)
-> MinionView.SetMinion(minion, HandleMinionClicked)
-> 玩家点击 MinionView
-> MinionView.HandleClick()
-> onClicked?.Invoke(minion)
-> GameUIController.HandleMinionClicked(minion)
```

所以学会 `CardView` 后，`MinionView` 不需要重新学一套。

## AddListener 和 RemoveListener

`AddListener`：

```text
注册监听。
告诉 Button：以后点击时调用这个方法。
```

`RemoveListener`：

```text
取消监听。
当这个 UI 对象销毁时，不再让 Button 保存它的方法引用。
```

本项目里通常成对出现：

```csharp
button.onClick.AddListener(HandleClick);
button.onClick.RemoveListener(HandleClick);
```

可以先这样理解：

```text
Awake/Start 里订阅按钮点击。
OnDestroy 里取消订阅。
```

这是一个良好的 Unity UI 习惯。

## Inspector 绑定和代码回调的区别

项目里按钮点击主要有两种绑定方式。

第一种：代码绑定。

```csharp
button.onClick.AddListener(HandleClick);
```

意思是：

```text
运行时由脚本告诉 Button 点击后调用哪个方法。
```

第二种：Inspector 拖引用。

```csharp
[SerializeField] private Button endTurnButton;
```

意思是：

```text
在 Unity Inspector 里把场景中的 Button 对象拖给这个字段。
```

它们解决的是两个不同问题：

```text
Inspector 绑定：这个字段指向哪个 Unity 对象？
AddListener：这个 Button 被点击后调用哪个方法？
```

## 读回调代码的固定公式

以后看到回调代码，按这个顺序拆：

```text
1. 谁保存了回调？
2. 回调的参数是什么？
3. 谁把具体方法传进来？
4. 什么时候 Invoke？
5. Invoke 后实际进入哪个方法？
```

以 `CardView` 为例：

```text
1. CardView 保存 onClicked。
2. 参数是 Card。
3. GameUIController.RefreshHand 把 HandleCardClicked 传进来。
4. Button 点击后，CardView.HandleClick 里 Invoke。
5. 实际进入 GameUIController.HandleCardClicked(card)。
```

以 `MinionView` 为例：

```text
1. MinionView 保存 onClicked。
2. 参数是 Minion。
3. GameUIController.RefreshBoards 把 HandleMinionClicked 传进来。
4. Button 点击后，MinionView.HandleClick 里 Invoke。
5. 实际进入 GameUIController.HandleMinionClicked(minion)。
```

## 一个最小类比

可以把回调理解成留电话：

```text
GameUIController 对 CardView 说：
这是这张牌的数据。
另外，这是我的电话号码 HandleCardClicked。
如果这张牌被点了，你打这个电话，并告诉我是哪张牌。
```

于是点击发生时：

```text
CardView 不处理出牌。
CardView 只打电话。
GameUIController 接电话后，才找 GameManager 办规则。
```

这个类比虽然简单，但能帮你在代码里稳住方向。
