# CLAUDE.md — HearthstoneClone 项目上下文

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

- **Unity 2022 LTS / 6000 LTS**，2D Built-In Render Pipeline
- **事件驱动的效果结算系统** — GameEventBus 模式，解耦卡牌/关键词/UI
- **ScriptableObject** 存储卡牌模板数据
- **UGUI** 做 UI
- 目录结构见 `PROJECT_PLAN.md`

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

## AI 协作原则

> **每行代码都必须自己读懂。**

- 不是让用户自己写，而是 Claude 写完之后，用户必须能解释它在做什么、为什么这样写
- 每次写完一个类，用户确认理解后再进入下一个
- 遇到不懂的语法/概念立刻追问，不要跳过
- 用户可以随时要求放慢节奏、换个角度解释、或者重写

## 项目文件说明

| 文件 | 用途 |
|------|------|
| `PROJECT_PLAN.md` | 完整开发计划（Markdown） |
| `PROJECT_PLAN.html` | 开发计划（浏览器查看，gitignore 已排除） |
| `README.md` | 项目介绍（面试官第一眼看） |
| `.gitignore` | 基于 GitHub 官方 Unity 模板 |

## 当前进度

- [x] 项目初始化（Git、Unity 2D、GitHub）
- [x] .gitignore、README、PROJECT_PLAN
- [ ] 阶段 1：最小可玩原型

## 开发阶段速览

| 阶段 | 内容 | 时间 |
|------|------|------|
| 1 | 最小原型：随从召唤/攻击/胜负 | 5-7 周 |
| 2 | 法术 + 5 个关键词 + 事件系统 | 6-8 周 |
| 3 | AI 对手（博弈树 + 评估函数） | 4-6 周 |
| 4 | 套牌构筑 + UI 打磨 | 4-6 周 |
| 5 | 求职包装（视频/博客） | 2-3 周 |
