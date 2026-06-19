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

## 构造函数

构造函数是在 `new` 一个对象时自动执行的方法。

例子：

```csharp
public Card(CardData data)
{
    CardData = data;
    CurrentCost = data.Cost;
}
```

读作：

```text
创建一张 Card 时，需要传入 CardData。
这张 Card 会记住自己的模板，并把当前费用设置成模板费用。
```

调用方式：

```csharp
Card card = new Card(data);
```

本项目里常见的构造函数：

```text
new Card(data)
new Hero(heroName, heroHealth)
new Player(deckCards, "Player")
new Minion(card.CardData, CurrentPlayer)
new Board(Player, Enemy)
```

## 属性和 private set

例子：

```csharp
public int CurrentHealth { get; private set; }
```

读作：

```text
外部可以读取 CurrentHealth。
但只有这个类内部可以修改 CurrentHealth。
```

这样做的目的：

```text
避免外部代码随便改核心状态。
```

例如英雄血量不应该被 UI 直接改。

正确方式是调用：

```csharp
hero.TakeDamage(amount);
hero.Heal(amount);
```

## SerializeField

例子：

```csharp
[SerializeField] private Text nameText;
```

读作：

```text
这个字段在代码里是 private。
但 Unity Inspector 可以看到并配置它。
```

为什么不用 `public`：

```text
public 表示任何代码都可以访问和修改。
SerializeField 可以让 Inspector 配置，同时保持代码层面的封装。
```

本项目例子：

```text
CardView 里的 Text 和 Button 引用
HandView 里的 CardViewPrefab
GameUIController 里的各种 UI 引用
GameManager 里的 Player Deck Data / Enemy Deck Data
```

## null

`null` 表示“这里没有对象”。

例子：

```csharp
if (card == null) return false;
```

读作：

```text
如果没有传入有效卡牌，操作失败。
```

本项目遇到过一次真实问题：

```text
牌库列表里某个 CardData 是 None。
代码把这个空对象传给 new Card(data)。
Card 构造函数访问 data.Cost 时发生 NullReferenceException。
```

解决方式：

```csharp
if (data == null) continue;
```

读作：

```text
如果这个卡牌模板是空的，就跳过这一次循环。
```

## Action 回调

`Action<T>` 可以理解为“保存一个以后要调用的方法”。

本项目例子：

```csharp
private Action<Card> onClicked;
```

读作：

```text
onClicked 是一个方法引用。
这个方法需要接收一张 Card。
```

在 `CardView` 中：

```csharp
onClicked?.Invoke(card);
```

读作：

```text
如果 onClicked 不为空，就调用它，并把当前这张 card 传出去。
```

为什么这样写：

```text
CardView 只负责显示和点击。
它不应该知道点击后是出牌、查看详情，还是拖拽。
所以它把“被点击的 Card”通知给上层，让上层决定做什么。
```

这就是 UI 分层中常见的回调写法。

## HashSet

`HashSet<T>` 是一种集合，特点是：

```text
只保存唯一值。
同一个值不能重复出现。
适合判断“这个东西之前有没有出现过”。
```

它和 `List<T>` 的区别：

| 类型 | 特点 | 适合场景 |
|------|------|----------|
| `List<T>` | 有顺序，可以重复 | 手牌、牌库、战场随从、Inspector 配置列表 |
| `HashSet<T>` | 不关心顺序，不允许重复 | 去重、快速判断某个值是否已经出现 |

本项目中，关键词字段仍然使用：

```csharp
[SerializeField] private List<KeywordType> keywords = new List<KeywordType>();
```

原因是 Unity Inspector 对 `List<T>` 的显示和编辑更直观。

但是清理关键词时，会临时使用：

```csharp
HashSet<KeywordType> seenKeywords = new HashSet<KeywordType>();
```

原因是关键词不能重复。例如：

```text
Charge
Charge
```

只应该被保留成：

```text
Charge
```

关键代码：

```csharp
bool isNewKeyword = seenKeywords.Add(keyword);
```

`HashSet.Add()` 的返回值表示：

```text
true  = 之前没有这个值，这次成功加入
false = 之前已经有这个值，这次没有加入
```

所以这句代码同时做了两件事：

```text
记录这个 keyword 已经出现过。
判断这个 keyword 是不是重复项。
```

本项目当前写法：

```csharp
private void CleanKeywords()
{
    if (keywords == null)
    {
        keywords = new List<KeywordType>();
        return;
    }

    HashSet<KeywordType> seenKeywords = new HashSet<KeywordType>();
    List<KeywordType> cleanedKeywords = new List<KeywordType>();

    foreach (KeywordType keyword in keywords)
    {
        if (keyword == KeywordType.None)
        {
            cleanedKeywords.Add(keyword);
            continue;
        }

        bool isNewKeyword = seenKeywords.Add(keyword);
        if (!isNewKeyword) continue;

        cleanedKeywords.Add(keyword);
    }

    keywords = cleanedKeywords;
}
```

读法：

```text
1. 如果关键词列表为空，就创建一个空列表。
2. 准备 seenKeywords，用来记录已经见过的关键词。
3. 准备 cleanedKeywords，用来保存清理后的结果。
4. 遍历原来的 keywords。
5. 遇到 None 就保留。None 是 Unity Inspector 新增元素时的临时占位。
6. 如果 HashSet 说这是重复值，就跳过。
7. 如果是新关键词，就加入 cleanedKeywords。
8. 最后用 cleanedKeywords 替换原列表。
```

这里不删除 `None` 的原因：

```text
Unity Inspector 给 enum 列表点 + 时，新元素默认是枚举的第一个值。
KeywordType 的第一个值是 None。
如果 OnValidate 立刻删除 None，新元素会刚加上就被清掉，看起来像“点 + 没反应”。
所以 None 允许作为编辑期占位存在。
真正判断关键词时，HasKeyword(KeywordType.None) 仍然会返回 false。
```

为什么不直接把字段写成 `HashSet<KeywordType>`：

```text
Unity Inspector 更适合编辑 List。
HashSet 更适合运行时代码做去重和查询。
所以这里是：对外配置用 List，内部清洗用 HashSet。
```

这是一个比较常见的工程取舍：用适合编辑器的数据结构保存配置，用适合算法意图的数据结构处理数据。

## ref 参数

`ref` 是 C# 的参数修饰符。

普通参数传递时，可以先理解成：

```text
方法拿到的是传入值的一份可用数据。
方法内部怎么改，不一定会改到外面的变量本身。
```

`ref` 的意思是：

```text
把变量本身交给方法。
方法内部可以直接修改调用者传进来的那个变量。
```

例子：

```csharp
private void Change(string value)
{
    value = "新内容";
}

string text = "旧内容";
Change(text);

// text 仍然是 "旧内容"
```

加上 `ref`：

```csharp
private void Change(ref string value)
{
    value = "新内容";
}

string text = "旧内容";
Change(ref text);

// text 变成 "新内容"
```

注意两边都要写 `ref`：

```csharp
Change(ref text);
```

这样做是为了让调用处一眼看出来：

```text
这个方法可能会改掉我传进去的变量。
```

### 什么时候适合用 ref

`ref` 适合少数比较明确的场景：

```text
方法需要直接修改一个外部变量。
这个修改是方法的核心目的。
调用者也能清楚接受这种副作用。
```

例如一些底层性能优化、数学计算、需要同时读写同一个结构体的场景。

但在普通业务逻辑和 UI 文本拼接里，`ref` 往往会增加阅读负担。

### 这次 CardView 的取舍

阶段 2.4 中，`CardView` 需要把几类文本拼成手牌描述：

```text
关键词
战吼
手写描述
```

一开始曾经写成：

```csharp
string text = "";

AppendDescriptionLine(ref text, GetKeywordsText(cardData));
AppendDescriptionLine(ref text, GetBattlecryText(cardData));
AppendDescriptionLine(ref text, cardData.Description);
```

这种写法能跑，但不是这里最合适的工程写法。

问题是：

```text
1. ref 对初学者阅读负担较高。
2. 这里并不是真的需要“修改外部变量本身”这种能力。
3. 文本拼接更常见的写法是先收集多行，再统一组合。
```

所以当前改成了：

```csharp
List<string> lines = new List<string>();

AddDescriptionLine(lines, GetKeywordsText(cardData));
AddDescriptionLine(lines, GetBattlecryText(cardData));
AddDescriptionLine(lines, cardData.Description);

return string.Join("\n", lines);
```

读法：

```text
1. 准备一个字符串列表 lines。
2. 把非空描述逐行加入 lines。
3. 用 string.Join 把这些行用换行符连接起来。
```

`string.Join("\n", lines)` 的意思是：

```text
把 lines 里的每个字符串拼起来，中间用换行符分隔。
```

这个写法更符合当前项目的学习目标：

```text
优先使用清楚、常见、容易维护的写法。
不要为了“少写几行”引入不必要的高级语法。
```

### 当前规则

以后遇到 `ref`、`out`、`delegate`、`event`、泛型接口、反射这类会增加认知负担的语法时，先问：

```text
这个语法是不是解决当前问题的常见做法？
不用它会不会更清楚？
它带来的复杂度值不值得？
```

如果只是为了临时方便，就优先不用。
