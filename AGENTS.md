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
| `Docs/06_Stage2Review.md` | 阶段 2 收尾复盘 |
| `Docs/07_CodeReviewReport.md` | 历史代码审查报告和修复记录 |
| `Docs/08_AIReview.md` | 阶段 3 AI 回归清单 |
| `Docs/Learning/CSharpNotes.md` | C# 和代码阅读笔记 |
| `Docs/Learning/UnityNotes.md` | Unity 编辑器、UGUI、Prefab、Inspector 操作笔记 |
| `Docs/00_CurrentStatus.md` | 当前进度快照，方便下次接着做 |
| `Docs/ProjectOverview.html` | 项目总览网页 |
| `README.md` | 项目介绍（面试官第一眼看） |
| `.gitignore` | 基于 GitHub 官方 Unity 模板 |

## 当前进度

当前进度、当前停靠点和下一步以 `Docs/00_CurrentStatus.md` 为准；本文件只保留协作规则，不重复维护阶段清单。

开始工作前先读取：
- `Docs/00_CurrentStatus.md`
- 必要时再读 `Docs/01_ProjectPlan.md`
