# 代码阅读笔记

这份文档专门记录项目中常见的代码写法，以及应该怎样读懂它们。

目标不是背语法，而是把代码翻译成人话：

- 这段代码在尝试做什么？
- 成功和失败分别是什么？
- 哪些情况会提前结束？
- 后面的代码为什么可以继续执行？

## 守卫语句

守卫语句，也叫提前返回，是成熟项目里非常常见的写法。

它的基本形式是：

```csharp
if (失败情况) return;

正常流程;
```

如果方法需要返回 `bool`，常见形式是：

```csharp
if (失败情况) return false;

正常流程;
return true;
```

### 怎样读

看到这种代码时，先不要急着找 `else`。

固定读法：

```text
如果发生这个情况，这个方法到此结束。
否则，代码继续往下走。
```

也就是说：

```csharp
if (!played) return false;

Minion minion = new Minion(card.CardData, CurrentPlayer);
return Board.SummonMinion(minion);
```

可以读成：

```text
如果没有成功打出这张牌，就返回失败。
否则继续往下走，创建随从并召唤到战场。
```

它逻辑上等价于：

```csharp
if (!played)
{
    return false;
}
else
{
    Minion minion = new Minion(card.CardData, CurrentPlayer);
    return Board.SummonMinion(minion);
}
```

工程里常省略 `else`，因为前面的 `return` 已经结束方法了。

可以记住一句：

```text
前面已经 return 了，后面就天然是 else。
```

## 例子：出牌失败就不召唤

来自 `GameManager.TryPlayMinionCard`：

```csharp
bool played = CurrentPlayer.PlayCard(card);
if (!played) return false;

Minion minion = new Minion(card.CardData, CurrentPlayer);
return Board.SummonMinion(minion);
```

逐句翻译：

```text
1. 让当前玩家尝试打出这张牌。
2. played 保存尝试结果：true 表示成功，false 表示失败。
3. 如果 played 是 false，说明没有成功打出，直接返回 false。
4. 如果能走到下面，说明已经成功支付费用，并且牌已经从手牌移除。
5. 创建一个随从。
6. 把随从召唤到战场。
```

这里的 `!played` 表示：

```text
played 的反面
```

所以：

```text
played == true   -> !played == false
played == false  -> !played == true
```

## 例子：双方都没死就不结算胜负

来自 `GameManager.CheckGameOver`：

```csharp
if (!playerDead && !enemyDead) return;

IsGameOver = true;
```

逐句翻译：

```text
如果玩家没死，并且敌人也没死：
    游戏还没有结束，直接退出。

否则：
    至少有一方死了，游戏结束。
```

表格理解：

```text
playerDead   enemyDead   结果
false        false       双方都活着，return
true         false       玩家死了，继续执行，游戏结束
false        true        敌人死了，继续执行，游戏结束
true         true        双方都死了，继续执行，游戏结束
```

## 为什么成熟项目喜欢这样写

守卫语句的好处：

- 先排除非法情况，正常流程更明显。
- 避免一层又一层的 `else` 嵌套。
- 后面的代码默认处于“前置条件已经满足”的状态。
- 方法越复杂，这种写法越能减少阅读负担。

新手刚看会吃力，主要是因为代码没有明显写出 `else`。

练习方法：

```text
看到 if (...) return;
先在脑子里补一句：
如果满足这个条件，这个方法已经结束了。
```

## 常见守卫语句读法

```csharp
if (card == null) return false;
```

读作：

```text
如果没有传入有效卡牌，操作失败。
```

```csharp
if (!Hand.Contains(card)) return false;
```

读作：

```text
如果这张牌不在手牌里，不能打出。
```

```csharp
if (card.CurrentCost > CurrentMana) return false;
```

读作：

```text
如果卡牌费用大于当前法力，费用不够，不能打出。
```

```csharp
if (IsGameOver) return;
```

读作：

```text
如果游戏已经结束，就不要继续执行后面的逻辑。
```

```csharp
if (target == null) return false;
```

读作：

```text
如果没有目标，操作失败。
```

## 阅读布尔表达式的小技巧

### `!`

`!` 表示取反。

```csharp
!played
```

读作：

```text
没有成功打出
```

### `&&`

`&&` 表示并且。

```csharp
!playerDead && !enemyDead
```

读作：

```text
玩家没死，并且敌人也没死
```

### `||`

`||` 表示或者。

```csharp
playerDead || enemyDead
```

读作：

```text
玩家死了，或者敌人死了
```

## 本项目的阅读习惯

以后遇到看不懂的代码，可以按这个顺序拆：

```text
1. 这个方法的目标是什么？
2. 前面的 if return 在排除哪些失败情况？
3. bool 变量 true / false 分别代表什么？
4. 哪一行开始进入真正的成功流程？
5. 这个方法最后返回什么结果？
```

这份文档后续可以继续补充：

- 构造函数
- 属性和字段
- 只读属性
- `private set`
- `SerializeField`
- `IReadOnlyList`
- 倒序遍历删除元素
- 事件系统

## 倒序遍历删除元素

当代码一边遍历 `List`，一边删除里面的元素时，经常会使用倒序遍历。

本项目例子：

```csharp
for (int i = minions.Count - 1; i >= 0; i--)
{
    Minion minion = minions[i];
    if (minion.IsDead)
    {
        Board.RemoveMinion(minion);
    }
}
```

### 怎样读

固定读法：

```text
从列表最后一个元素开始检查。
如果当前元素需要删除，就删掉它。
然后继续检查前一个元素。
```

这里不是因为“后上场的随从更重要”，而是因为：

```text
删除 List 中的元素后，后面的元素索引会往前移动。
```

### 为什么正序可能出问题

假设列表是：

```text
索引 0：A，死亡
索引 1：B，死亡
索引 2：C，存活
```

如果正序遍历：

```text
i = 0，检查 A，A 死亡，删除 A
```

删除后列表变成：

```text
索引 0：B
索引 1：C
```

然后循环执行 `i++`，`i` 变成 1。

接下来检查的是：

```text
索引 1：C
```

问题是：

```text
B 从索引 1 移到了索引 0，但已经被跳过了。
```

所以正序删除容易漏检查元素。

### 为什么倒序安全

还是这个列表：

```text
索引 0：A，死亡
索引 1：B，死亡
索引 2：C，存活
```

倒序遍历：

```text
i = 2，检查 C，存活，不删
i = 1，检查 B，死亡，删除 B
i = 0，检查 A，死亡，删除 A
```

删除 B 时，受影响的是 B 后面的元素索引。

但是 B 后面的元素 C 已经检查过了，所以不会漏。

可以记住一句：

```text
遍历 List 时如果要删除元素，倒序通常更安全。
```

## 类似的 List 阅读点

### `Add` 会加到末尾

```csharp
minions.Add(minion);
```

读作：

```text
把这个随从加到列表最后。
```

所以当前 `Board` 里的随从顺序是：

```text
索引 0：最早上场的随从
索引 1：第二个上场的随从
索引 2：第三个上场的随从
```

### 索引从 0 开始

```csharp
minions[0]
```

读作：

```text
列表里的第一个元素。
```

```csharp
minions[minions.Count - 1]
```

读作：

```text
列表里的最后一个元素。
```

因为索引从 0 开始，所以最后一个元素不是 `Count`，而是 `Count - 1`。

### `Count` 是当前数量

```csharp
minions.Count
```

读作：

```text
这个列表当前有多少个元素。
```

如果列表里有 3 个元素：

```text
Count = 3
有效索引 = 0、1、2
最后一个索引 = Count - 1 = 2
```

### 不要在 `foreach` 里直接删除当前列表元素

这类代码要小心：

```csharp
foreach (Minion minion in minions)
{
    if (minion.IsDead)
    {
        Board.RemoveMinion(minion);
    }
}
```

读代码时要警惕：

```text
foreach 正在遍历一个列表时，如果同时修改这个列表，通常会出问题。
```

常见替代方案：

```text
1. 倒序 for 遍历删除。
2. 先记录要删除的元素，再统一删除。
3. 创建一个新列表，只保留不需要删除的元素。
```

本项目现在用的是第一种：

```text
倒序 for 遍历删除。
```

### `IReadOnlyList` 表示外部只读

```csharp
IReadOnlyList<Minion> minions = Board.GetMinions(owner);
```

读作：

```text
拿到这个玩家场上的随从列表，但这里只按“只读列表”来使用。
```

这表示调用方可以查看：

```text
minions.Count
minions[i]
```

但不应该直接做：

```text
Add
Remove
Clear
```

本项目里真正添加/移除随从，要通过 `Board` 的方法：

```csharp
Board.SummonMinion(minion);
Board.RemoveMinion(minion);
```

这样做的目的：

```text
战场列表由 Board 统一管理，外部代码不要随便改列表。
```
