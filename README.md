# HearthstoneClone

复刻炉石传说的卡牌对战游戏 | Unity 2D 练手项目

## 项目简介

本项目的目标是实现炉石传说核心对战玩法的完整复刻，包括随从召唤、法术施放、关键词机制（嘲讽/冲锋/战吼/亡语/圣盾）、AI 对手对战等功能。

**开发目的：**
- 系统学习 Unity 2D 游戏开发流程
- 深入理解卡牌游戏核心架构（事件系统、回合制、状态管理）
- 积累可展示的求职作品

## 技术栈

| 技术 | 说明 |
|------|------|
| Unity 2022 LTS / 6000 LTS | 2D Built-In Render Pipeline |
| C# | 游戏逻辑与架构 |
| UGUI | UI 系统（手牌、战场、菜单） |
| ScriptableObject | 卡牌数据管理 |

## 开发阶段

- [ ] **阶段 0**：Unity 基础学习（C# 基础、GameObject、Prefab、ScriptableObject）
- [ ] **阶段 1**：最小可玩原型（随从召唤与攻击、法力水晶、胜负判定）
- [ ] **阶段 2**：法术牌与关键词系统（嘲讽/冲锋/战吼/亡语/圣盾、事件驱动效果结算）
- [ ] **阶段 3**：AI 对手（博弈树搜索 + 评估函数）
- [ ] **阶段 4**：打磨与包装（UI 动画、套牌选择、音效）

## 项目结构（规划）

```
Assets/
├── Scripts/
│   ├── Core/            # 核心游戏逻辑（回合、回合流程、游戏状态）
│   ├── Cards/           # 卡牌系统（数据定义、效果系统、关键词）
│   ├── Board/           # 战场管理（随从站位、攻击交互）
│   ├── AI/              # AI 对手
│   └── UI/              # UI 控制器
├── Prefabs/             # 预制体（卡牌、随从 Token）
├── ScriptableObjects/   # 卡牌数据、配置表
├── Scenes/              # 场景
├── Sprites/             # 图片资源（临时占位 → 最终素材）
└── Audio/               # 音效
```

## 快速开始

1. 克隆仓库：`git clone https://github.com/ZHStatic/HearthstoneClone.git`
2. 在 Unity Hub 中通过 `Add Project from Disk` 打开项目
3. 打开 `Assets/Scenes/` 下的主场景
4. 点击 Play 运行

## License

MIT
