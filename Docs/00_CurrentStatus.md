# Current Status

最后更新：2026-06-19

## 当前阶段

阶段 1、阶段 1.5、阶段 2.1、阶段 2.1.5、阶段 2.2 已完成。

当前停靠点：

```text
阶段 2.2 已完成：第一个关键词“冲锋”
```

冲锋的最小链路已经测试通过：`CardData` 配置关键词，`Minion` 复制关键词，`GameManager` 在召唤后让冲锋随从立刻可以攻击。

## 当前可玩内容

当前 Play 模式已能完成：

```text
抽牌
出牌
召唤随从
召唤冲锋随从并立即攻击
施放单目标伤害法术
费用不足和非法操作提示
随从攻击随从
随从攻击英雄
结束回合
胜负判定
```

当前测试卡牌：

- 基础随从：训练新兵、河湾猎手、城墙守卫、战场斗士、岩石巨人。
- 冲锋随从：疾风斥候，2 费，2/1，关键词为 `Charge`。
- 基础法术：火花，1 费，造成 2 点伤害，目标为任意角色。

## 当前代码结构

### Core 层

| 文件 | 职责 |
|------|------|
| `CardData.cs` | ScriptableObject 卡牌模板数据 |
| `CardType.cs` | 卡牌类型：随从、法术 |
| `SpellTargetType.cs` | 单目标法术可选择的目标范围 |
| `KeywordType.cs` | 关键词类型：当前支持 `Charge` |
| `Card.cs` | 手牌/牌库中的运行时卡牌实例 |
| `Hero.cs` | 英雄生命、受伤、治疗、死亡判断 |
| `Player.cs` | 手牌、牌库、法力水晶、抽牌、出牌 |
| `Board.cs` | 双方战场随从列表和召唤位置限制 |
| `Minion.cs` | 场上随从的攻击、生命、所属玩家、攻击权限和关键词 |
| `GameManager.cs` | 当前阶段的对局流程调度，包含冲锋召唤处理 |

### UI 层

| 文件 | 职责 |
|------|------|
| `CardView.cs` | 显示一张手牌并转发点击 |
| `HandView.cs` | 根据手牌列表生成多个 `CardView` |
| `MinionView.cs` | 显示一个场上随从、点击和选中高亮 |
| `BoardView.cs` | 根据一方战场列表生成多个 `MinionView` |
| `GameUIController.cs` | 连接 UI 和 `GameManager`，处理点击、选择状态、反馈和刷新 |

## 文档分工

以后按这个分工维护文档，避免重复：

| 文档 | 只负责 |
|------|--------|
| `Docs/00_CurrentStatus.md` | 当前进度、当前停靠点、下一步 |
| `Docs/01_ProjectPlan.md` | 项目长期路线和阶段目标 |
| `Docs/02_CoreArchitecture.md` | Core 层职责、依赖、边界和后续拆分点 |
| `Docs/03_UIArchitecture.md` | UI 层职责、点击输入、刷新方式 |
| `Docs/04_FeatureFlows.md` | 玩家操作到代码调用的流程 |
| `Docs/05_InterviewNotes.md` | 面试时怎么讲这个项目 |
| `Docs/Learning/` | 学习笔记，不要求和正式架构文档完全同步 |

## 已确认

- Unity Play 模式可以运行。
- 牌库中配置有效 `CardData` 后，手牌可以显示。
- 空的 `CardData` 会被 `Player` 跳过，避免开局空引用。
- 随从牌可以通过 `GameManager.TryPlayMinionCard(card)` 召唤。
- 配置了 `Charge` 的随从召唤后会立刻进入可攻击状态。
- 法术牌可以进入选目标状态，并通过 `TryPlaySpellCardOnMinion` / `TryPlaySpellCardOnHero` 结算。
- 出牌成功后，手牌减少、法力减少、战场或目标血量刷新。
- 结束回合后，当前行动者切换，UI 刷新。
- 随从攻击随从、随从攻击英雄和胜负判定已测试通过。

## 阶段 2.2 结论

当前代码不需要推倒重来，冲锋可以作为最小关键词验证保留在现有结构中。

已验证链路：

```text
CardData 配置 Charge
-> Minion 复制关键词
-> GameManager 召唤后识别 Charge
-> 新随从 CanAttack = true
-> UI 显示 Ready
```

需要记住的风险点：

- `GameManager` 已经负责回合、出牌、法术、攻击、死亡清理和胜负判断，后续不能无限加规则特判。
- `GameUIController` 已经负责攻击选择、法术选择、英雄点击、操作反馈和刷新，后续 UI 状态复杂时需要拆分。
- 当前法术直接由 `GameManager` 结算，这是阶段性简化，不是成熟项目最终做法。
- 当前冲锋不需要事件系统，战吼、亡语、圣盾这类机制不要急着硬写。

下一步判断：

```text
UI 可以补充关键词文字显示，让演示更直观。
嘲讽开始会影响攻击目标选择。
战吼和亡语开始需要更认真地引入事件或效果系统。
```

## 下一步

优先做一个小收尾：

```text
让 CardView / MinionView 显示关键词文字，例如“冲锋”。
```

之后进入阶段 2.3：嘲讽。
