# Unity Notes

本文档记录当前项目用到的 Unity 概念和编辑器操作。

目标不是系统背 Unity，而是把项目中遇到的东西逐个讲清楚。

## 常用面板

### Hierarchy

`Hierarchy`：层级面板。

它显示当前场景里有哪些 `GameObject`。

例如：

```text
Canvas
EventSystem
GameManager
GameUIController
HandView
PlayerBoardView
EnemyBoardView
```

### Project

`Project`：项目资源面板。

它显示 `Assets` 文件夹里的资源。

例如：

```text
Scripts
Prefabs
Scenes
CardData asset
```

### Inspector

`Inspector`：检查器。

选中一个对象后，右侧会显示它身上的组件和可配置字段。

本项目中经常在 Inspector 里做：

```text
给 GameObject 挂脚本
给脚本字段拖引用
配置 CardData 卡牌数据
配置 GameManager 的牌库列表
```

### Console

`Console`：控制台。

它显示报错、警告和日志。

红色是错误，通常必须先解决。

## 基础概念

### GameObject

`GameObject`：游戏物体。

Unity 场景中的任何对象基本都是 GameObject。

例如：

```text
GameManager 是一个空 GameObject
Canvas 是一个 UI GameObject
EndTurnButton 是一个 UI GameObject
```

### Component

`Component`：组件。

GameObject 本身只是一个容器，功能来自组件。

例如：

```text
Button 组件让对象可以点击
Text 组件让对象显示文字
GameManager 脚本组件让对象拥有对局管理逻辑
```

### Prefab

`Prefab`：预制体。

可以理解为可以反复复制的模板。

本项目例子：

```text
CardViewPrefab：一张卡牌 UI 模板
MinionViewPrefab：一个随从 UI 模板
```

为什么用 Prefab：

```text
手牌和战场都是重复元素。
运行时根据数据动态生成多个 UI，比手动摆几十个对象更合理。
```

### ScriptableObject

`ScriptableObject`：Unity 里的数据资源。

本项目用它保存卡牌模板：

```text
卡牌名
费用
攻击
生命
描述
```

它适合做静态配置，不适合直接保存对局中变化的状态。

## UGUI 相关

### Canvas

`Canvas`：画布。

UGUI 的按钮、文字、图片一般都放在 Canvas 下面。

如果没有 Canvas，UI 元素通常不会正常显示。

### EventSystem

`EventSystem`：事件系统。

按钮点击、鼠标交互等 UI 输入需要它。

如果按钮点了没反应，要检查场景里有没有 EventSystem。

### Rect Transform

`Rect Transform`：UI 用的矩形变换。

普通物体用 `Transform`。

UI 物体用 `Rect Transform` 控制：

```text
位置
大小
锚点
宽高
```

常见字段：

```text
Pos X：横向偏移
Pos Y：纵向偏移
Width：宽度
Height：高度
Anchor：锚点
```

### Anchor

`Anchor`：锚点。

它决定 UI 元素贴着屏幕或父物体的哪里。

常用：

```text
Top Left：左上
Middle Center：中间
Bottom Center：底部居中
Bottom Right：右下
```

本项目当前布局建议：

```text
HandView：Bottom Center
PlayerBoardView：Middle Center，Pos Y 为负
EnemyBoardView：Middle Center，Pos Y 为正
EndTurnButton：Bottom Right
HUD 文本：Top Left
```

### Horizontal Layout Group

`Horizontal Layout Group`：水平布局组。

它会把子物体横向排列。

本项目中用于：

```text
手牌区横向排列卡牌
战场区横向排列随从
```

常用设置：

```text
Spacing：元素间距
Child Alignment：子物体对齐
Control Child Size：是否由布局组控制子物体大小
Child Force Expand：是否强制拉伸子物体
```

## 脚本字段拖引用

脚本中带 `[SerializeField]` 的字段会显示在 Inspector 中。

例如：

```csharp
[SerializeField] private HandView handView;
```

在 Inspector 中看到 `Hand View` 字段后，需要把场景里的 `HandView` 对象拖进去。

这叫“拖引用”。

如果忘记拖引用，运行时很容易出现：

```text
NullReferenceException
```

## NullReferenceException

`NullReferenceException`：空引用异常。

意思是：

```text
代码想使用一个对象，但这个对象其实是 null。
```

本项目遇到过的例子：

```text
Player Deck Data 里有空的 CardData。
Player 创建 Card 时传入了 null。
Card 构造函数访问 data.Cost，于是报错。
```

排查方式：

```text
1. 看 Console 红色报错。
2. 看报错调用链。
3. 找到最上面属于自己代码的文件和行号。
4. 检查那一行使用的对象有没有可能是 null。
5. 回到 Inspector 检查引用有没有拖、列表有没有 None。
```

## 当前项目的 Unity 操作清单

创建卡牌数据：

```text
Project 面板右键
Create > HearthstoneClone > Card Data
填写卡牌名、费用、攻击、生命、描述
拖到 GameManager 的 Player Deck Data / Enemy Deck Data
```

创建 UI prefab：

```text
在 Canvas 下创建 UI 元素
挂对应 View 脚本
拖好 Text / Button 字段
从 Hierarchy 拖到 Project 的 Assets/Prefabs/UI
```

运行前检查：

```text
场景中有 GameManager
场景中有 Canvas
场景中有 EventSystem
GameUIController 的字段已拖好
GameManager 的牌库列表不是全空
Console 没有红色错误
```
