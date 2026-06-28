# AGENTS.md — HearthstoneClone 项目上下文

## 用户画像

- **ZHStatic** — 目标 Unity 游戏开发/策划岗位
- 编程基础不多，无游戏开发经验，Unity 只会基本操作
- 时间：周内晚上 1-2h + 周末 2-10h（约 10-20h/周）
- **学习方式：项目驱动，边做边学**，不提前系统学完再动手
- 后续也考虑转向虚幻引擎（本项目先用 Unity 打基础）

## 项目定位

单人版炉石传说核心对战体验 | Unity 2D Built-In RP | 求职 Demo

**原则：精致的核心体验 > 粗糙的大而全。** 面试官 5 分钟看架构和玩法，不看卡牌数量。

### 范围：做 / 不做

| 做 | 不做 |
|----|------|
| 随从/法术/武器卡 | 联网对战 / 服务器 |
| 5-10 个核心关键词 | 天梯 / 竞技场 / 冒险模式 |
| 英雄技能 | 商店 / 充值 / 任务系统 |
| 5 套预制套牌 | 自定义美术（色块 + 免费素材即可） |
| 单人 vs AI 对战 | 100+ 卡牌（50-100 即可） |

## 技术架构（已确定）

- **Unity 2020.3.48f1c1**，2D Built-In Render Pipeline
- **事件驱动的效果结算系统** — GameEventBus 模式，解耦卡牌/关键词/UI
- **ScriptableObject** 存储卡牌模板数据
- **UGUI** 做 UI
- 目录结构见 `Docs/01_ProjectPlan.md`

## Git 规范

```
feat: 新功能        → feat: 实现法力水晶系统
fix: 修 bug        → fix: 修复随从死亡后仍能攻击
refactor: 重构     → refactor: 提取攻击结算为独立方法
chore: 杂项配置    → chore: 更新 .gitignore
docs: 文档更新     → docs: 补充架构说明
```

- 每完成一个小功能 commit 一次
- commit message 用中文写也可以
- 仓库：https://github.com/ZHStatic/HearthstoneClone

## 代码规范

- 类名 PascalCase，方法名 PascalCase，变量名 camelCase
- 每个文件有清晰的职责边界：它负责什么、依赖谁、被谁依赖
- 公开字段在 Inspector 中可配置的用 `[SerializeField]`，不直接 `public`
- 逻辑层（Core/Events/Effects/Keywords/AI）不直接依赖 UI 层

## Unity 资源协作规则

- Codex 新增 C# 脚本时，只创建 `.cs` 文件，**不要手动生成 `.meta` 文件**。
- 写完脚本后，提醒用户回到 Unity，让 Unity 自动导入脚本并生成对应 `.meta`。
- Prefab、Scene、ScriptableObject、图片、音频等 Unity 资源，优先让用户在 Unity Editor 的 Project 面板中创建、移动或重命名。
- 如果必须由 Codex 调整 Unity 资源文件，先说明风险并等待用户确认，避免破坏 `.meta` GUID 和场景/Prefab 引用。

## AI 协作原则

> **每行代码都必须自己读懂。**

- 不是让用户自己写，而是 Codex 写完之后，用户必须能解释它在做什么、为什么这样写
- 每次写完一个类，用户确认理解后再进入下一个
- 遇到不懂的语法/概念立刻追问，不要跳过
- 用户可以随时要求放慢节奏、换个角度解释、或者重写
- 遇到 Unity、游戏行业工程实践、UI/资源制作流程等问题时，先说明业内或成熟项目通常怎么做，再说明本项目当前阶段可以如何简化，以及这种简化的代价。
- 明确区分哪些事情应该在 Unity Editor / Prefab / ScriptableObject 里配置，哪些事情应该写进代码。UI 布局、字号、颜色、边距等视觉参数优先放在 Editor/Prefab 中调整；运行时状态、规则判断、交互反馈内容才由代码控制。
- 如果为了原型速度建议临时写法，必须明确标注“这是阶段性简化，不是成熟项目最终做法”，并补充以后求职或面试时可以如何解释这个取舍。

### 固定巡检方法

每次做较大范围代码整理、架构调整或 UI 反馈修正前，先按这个顺序检查：

```powershell
& 'C:/Users/Static/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe' '.codex/skills/hearthstone-code-review/scripts/find_review_candidates.py'
```

- 优先使用项目专用 `hearthstone-code-review` skill。
- 把脚本输出当作候选，不直接当作结论；需要打开相关代码确认。
- 重点看：忽略 `TakeDamage()` / `Heal()` 返回值、UI 误报实际伤害、重复 formatter、手动字符串拼接、Core/UI 反向依赖、`GameManager` / `GameUIController` 继续膨胀。
- 中文文档在 PowerShell 中读取时使用 `-Encoding UTF8`，避免误判为乱码。
- Prefab、Scene、ScriptableObject、图片、音频和 `.meta` 默认不由 Codex 直接改；视觉布局、字号、颜色、边距优先在 Unity Editor / Prefab 里调整。

### 写代码前的"属性清单"流程

**每个类动笔前，先列清单，用户确认后再写代码，不要直接跳到下一步。**

清单格式：

```
写 XXX.cs 之前：

  要写的：
  - 属性 A —（一句话描述用途）
  - 属性 B
  - 方法 C()

  故意不写的（说明原因）：
  - 属性 D → 阶段 X 才需要
  - 属性 E → 目前没有使用场景

  可以吗？
```

**清单检查维度（用户和 AI 都对照着看）：**

| 维度 | 问自己 | 覆盖什么 |
|------|--------|---------|
| **身份** | 这个东西在屏幕上怎么被认出来？需要名字/图标吗？ | 属性是否遗漏 |
| **数值边界** | 有什么能改、什么不能改？0 和负数合法吗？默认值是什么？ | 字段约束 |
| **关系** | 这个类被谁引用？引用了谁？关系合理吗？ | 依赖方向 |
| **玩家视角** | 玩家在 UI 上能看到这个信息吗？需要显示还是隐藏？ | UI 需求 |
| **行为完整性** | 每个方法：正常输入返回什么？边界值呢？非法输入（null、负数）呢？ | 防御性编程 |
| **时序/生命周期** | 构造时做什么？每回合开始/结束做什么？死亡/销毁时做什么？ | 状态机 |

**节奏控制：**
- 写完一个类 → 讲解 → 等用户确认 → **停下来**，等用户说"继续"再写下一个
- 除非用户明确说"继续写 XXX.cs"或"一口气写完阶段 1 所有类"，否则**不要主动进入下一个文件的编写**

## 项目文件说明

| 文件 | 用途 |
|------|------|
| `Docs/01_ProjectPlan.md` | 完整开发计划（Markdown） |
| `Docs/02_CoreArchitecture.md` | 当前 Core 层架构说明（类职责/引用关系/核心流程） |
| `Docs/03_UIArchitecture.md` | 当前 UI 层架构说明（CardView/HandView/BoardView/GameUIController） |
| `Docs/04_FeatureFlows.md` | 核心功能流程拆解（开局/出牌/结束回合/攻击） |
| `Docs/05_InterviewNotes.md` | 求职面试讲解要点 |
| `Docs/Learning/CSharpNotes.md` | C# 和代码阅读笔记 |
| `Docs/Learning/UnityNotes.md` | Unity 编辑器、UGUI、Prefab、Inspector 操作笔记 |
| `Docs/00_CurrentStatus.md` | 当前进度快照，方便下次接着做 |
| `PROJECT_PLAN.html` | 开发计划（浏览器查看，gitignore 已排除） |
| `README.md` | 项目介绍（面试官第一眼看） |
| `.gitignore` | 基于 GitHub 官方 Unity 模板 |

## 当前进度

- [x] 项目初始化（Git、Unity 2D、GitHub）
- [x] .gitignore、README、PROJECT_PLAN
- [x] 文档目录整理 — `Docs/`
- [x] 阶段 1：最小可玩原型
  - [x] CardData.cs — ScriptableObject 卡牌模板（名称/费用/攻击/血量）
  - [x] Card.cs — 运行时卡牌实例（引用 CardData + 动态 CurrentCost）
  - [x] Hero.cs — 英雄（血量/受伤/治疗/死亡判定）
  - [x] Player.cs — 玩家（手牌/牌库/法力水晶/抽牌/出牌/洗牌）
  - [x] Board.cs — 战场（管理双方随从站位）
  - [x] Minion.cs — 随从运行时实例
  - [x] GameManager.cs — 回合流程/出牌/攻击/死亡清理/胜负判定
  - [x] 核心架构梳理 — `Docs/02_CoreArchitecture.md`
  - [x] 代码阅读笔记 — `Docs/Learning/CSharpNotes.md`
  - [x] UI 架构梳理 — `Docs/03_UIArchitecture.md`
  - [x] 手牌区 UI
  - [x] 出牌交互
  - [x] 随从攻击交互
  - [x] 法力水晶 UI
  - [x] 回合流程 UI（结束回合按钮）
- [x] 阶段 1.5：最小原型展示打磨
  - [x] 5 张基础随从卡
  - [x] 费用不足、目标非法、不能攻击等操作反馈
  - [x] 随从选中高亮
  - [x] 基础 UI 可读性调整
- [x] 阶段 2.1：基础伤害法术
  - [x] `CardType`
  - [x] `SpellTargetType`
  - [x] 火花测试法术
  - [x] 法术选目标和伤害结算
- [x] 阶段 2.1.5：架构复盘与文档整理
- [x] 阶段 2.2：第一个关键词“冲锋”
  - [x] `KeywordType.cs`
  - [x] `CardData` 支持关键词配置
  - [x] `Minion` 复制关键词
  - [x] `GameManager` 召唤后处理冲锋
  - [x] `CardView` 显示手牌关键词文字
  - [x] 疾风斥候测试通过
- [x] 阶段 2.3：关键词“嘲讽”
  - [x] `KeywordType` 增加 `Taunt`
  - [x] 攻击随从和攻击英雄时检查嘲讽限制
  - [x] 手牌和场上随从可以显示“嘲讽”
- [x] 阶段 2.4：第一个战吼
  - [x] `BattlecryType`
  - [x] `CardData` 支持战吼类型和通用数值
  - [x] 召唤后结算“对敌方英雄造成伤害”
- [x] 阶段 2.4.5：战吼抽牌
  - [x] 战吼通用数值复用为抽牌数量
  - [x] 书卷侍从测试通过
- [x] 阶段 2.5.0：事件系统基础链路
  - [x] `GameEventType`
  - [x] `GameEvent`
  - [x] `GameEventBus`
  - [x] 曾用出牌和召唤调试事件验证事件总线，阶段 3 已删除调试事件
- [x] 阶段 2.5.1：死亡事件
  - [x] 随从死亡时发布 `MinionDied`
  - [x] 事件携带死亡随从
- [x] 阶段 2.6：第一个亡语
  - [x] `DeathrattleType`
  - [x] `CardData` 支持亡语类型和通用数值
  - [x] `MinionDied` 事件触发亡语伤害
- [x] 阶段 2.7：关键词“圣盾”
  - [x] `KeywordType` 增加 `DivineShield`
  - [x] `Minion.TakeDamage()` 支持首次正数伤害抵消并移除圣盾
  - [x] 手牌和场上随从显示“圣盾”
- [x] 阶段 2.8：阶段 2 收尾复盘
  - [x] 更新 README
  - [x] 新增 `Docs/06_Stage2Review.md`
  - [x] 整理阶段 2 演示脚本、架构取舍和进入 AI 前检查点
- [x] 阶段 2.9：战斗日志与代码整理
  - [x] `BattleLogEntry.cs`
  - [x] `BattleLogger.cs`
  - [x] `GameManager.BattleLog.cs`
  - [x] 伤害 helper 记录尝试伤害和实际伤害
  - [x] 圣盾抵消时记录 `DivineShieldPrevented`
  - [x] `GameUIController` 法术反馈优先读取 Core 最近结算日志
  - [x] `KeywordTextFormatter.cs` 统一关键词文本
- [x] 阶段 2.10.1：Core 操作结果标准化第一轮
  - [x] `GameActionFailureReason.cs`
  - [x] `GameActionResult.cs`
  - [x] 随从出牌和法术释放返回详细操作结果
  - [x] UI 读取 Core 返回的反馈文本
- [x] 阶段 2.10.2：Player 状态封装
  - [x] `Player.Hand` / `Player.Deck` 对外只读
  - [x] `Player.HasCardInHand(card)` 替代外部直接 `Hand.Contains(card)`
- [x] 阶段 2.10.3：动作建模与验证入口
  - [x] `GameActionType.cs`
  - [x] `GameAction.cs`
  - [x] `GameActionGenerator.cs`
  - [x] `GameManager` 支持回合开始时打印合法动作，默认关闭
- [x] 阶段 3.0 / 3.1：AI 基础行动
  - [x] `AIController.cs`
  - [x] `ActionSelector.cs`
  - [x] Enemy 回合自动生成合法动作并通过 `GameManager.ExecuteAction(GameAction)` 执行
- [x] 阶段 3.2：第一版动作选择策略
  - [x] `AIActionSelection.cs`
  - [x] `AIActionSelectionReason.cs`
  - [x] AI 行动日志输出行动、理由和结果

## 当前停靠点

- 阶段 1 最小对战闭环已完成。
- 阶段 1.5 展示打磨已完成：基础卡牌、操作反馈、选中高亮和基础 UI 可读性。
- 阶段 2.1 基础伤害法术已完成：火花可以选择随从或英雄并造成伤害。
- 阶段 2.1.5 已完成：文档边界重新整理，Core 架构文档不再重复详细流程。
- 阶段 2.2 已完成：冲锋随从召唤后可以立即攻击，手牌 UI 可以显示“冲锋”。
- 阶段 2.3 已完成：嘲讽可以限制攻击目标，手牌和场上随从可以显示“嘲讽”。
- 阶段 2.4 / 2.4.5 已完成：战吼可以造成敌方英雄伤害，也可以为出牌者抽牌。
- 阶段 2.5.0 / 2.5.1 已完成：事件总线基础链路已验证，当前只保留规则需要的 `MinionDied`。
- 阶段 2.6 已完成：亡语炸弹人死亡后可以通过 `MinionDied` 事件触发伤害。
- 阶段 2.7 已完成：圣盾随从第一次受到正数伤害时抵消伤害并失去圣盾。
- 阶段 2.8 已完成：阶段 2 文档复盘和演示脚本已整理。
- 阶段 2.9 已收口：战斗日志、圣盾反馈修正、关键词 formatter 复用已完成。
- 阶段 2.10.1 已完成：随从出牌和法术释放已接入 `GameActionResult`。
- 阶段 2.10.2 已完成：`Player` 手牌和牌库已改为只读暴露。
- 阶段 2.10.3 已完成：动作类型、动作数据和合法动作生成器已写入，Console 验证入口已加入。
- 阶段 3.0 / 3.1 已完成：Enemy AI 可以在自己的回合自动枚举并执行合法动作。
- 阶段 3.2 已完成：AI 第一版动作选择策略和选择原因日志已写入。
- UI 拆分和 UI 复用刷新暂时延后，等功能更完整后统一大改。
- 下一步：继续打磨 AI 出牌和攻击顺序，再进入评估函数。

## 开发阶段速览

| 阶段 | 内容 | 时间 |
|------|------|------|
| 1 | 最小原型：随从召唤/攻击/胜负 | 已完成 |
| 1.5 | 最小原型展示打磨：卡牌、反馈、演示整理 | 已完成 |
| 2.1 | 基础伤害法术 | 已完成 |
| 2.1.5 | 架构复盘与文档整理 | 已完成 |
| 2.2 | 第一个关键词：冲锋 | 已完成 |
| 2.3 | 第二个关键词：嘲讽 | 已完成 |
| 2.4 | 第一个战吼：伤害敌方英雄 | 已完成 |
| 2.4.5 | 战吼抽牌 | 已完成 |
| 2.5 | 第一版事件系统和死亡事件 | 已完成 |
| 2.6 | 第一个亡语 | 已完成 |
| 2.7 | 第三个关键词：圣盾 | 已完成 |
| 2.8 | 阶段 2 收尾复盘 | 已完成 |
| 2.9 | 战斗日志与代码整理 | 已完成 |
| 2.10 | 进入 AI 前的结构和 UI 优化 | 已完成 |
| 2 | 法术 + 5 个关键词 + 事件系统 | 6-8 周 |
| 3 | AI 对手（博弈树 + 评估函数） | 进行中 |
| 4 | 套牌构筑 + UI 打磨 | 4-6 周 |
| 5 | 求职包装（视频/博客） | 2-3 周 |
