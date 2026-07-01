# 阶段 4 前文档修复记录

最后更新：2026-07-01

本文记录阶段 4 前的文档同步结果。它不是新的开发计划；当前进度仍以 `Docs/00_CurrentStatus.md` 为准，后续任务仍以 `Docs/01_ProjectPlan.md` 的阶段路线为准。

## 已修复

| 文件 | 修复内容 |
|------|----------|
| `README.md` | 当前完成度补到阶段 3.13；当前阶段改为 UI / 交互 / 展示打磨；测试卡牌同步 `疾风斥候` 为 1 费 1/1；演示路径加入英雄技能；文档入口补齐 07/08 和项目总览网页 |
| `Docs/02_CoreArchitecture.md` | AI 类表补齐 `CardSnapshot`、`SnapshotAction`、`SnapshotFollowUpEvaluator` 等快照类；AI 流程同步到同回合后续预估和英雄技能；修正嘲讽链路里的旧方法名 |
| `Docs/03_UIArchitecture.md` | `GameUIController` 职责加入英雄技能选目标；攻击流程改为详细结果入口；补充英雄技能 UI 选择状态 |
| `Docs/04_FeatureFlows.md` | 补英雄技能流程；修正过时的 AI 状态和旧 Console 验证说法；攻击和出随从流程改为详细结果入口 |
| `Docs/05_InterviewNotes.md` | 完成度补到阶段 3.13；AI 说明从单步模拟改为少量同回合后续预估；补英雄技能面试讲稿；下一步改为阶段 4 UI / 展示打磨 |
| `Docs/08_AIReview.md` | 日期更新到 2026-07-01；补英雄技能 AI 回归项；当前简化和下一步判断标准同步 |
| `AGENTS.md` / `CLAUDE.md` | 文件说明移除不存在的根目录旧 HTML 入口，补齐 06/07/08 和 `Docs/ProjectOverview.html` |
| `Docs/07_CodeReviewReport.md` | 增加历史报告说明，避免把 2026-06-27 的旧问题误认为当前状态 |

## 当前保留的文档取舍

- `AGENTS.md` 和 `CLAUDE.md` 内容相同是有意保留：它们分别服务 Codex 和 Claude 的项目协作规则，不在本轮删除。
- `Docs/07_CodeReviewReport.md` 保留为历史审查记录，不作为当前 bug 清单。
- `Docs/00_CurrentStatus.md` 内容偏长，但仍承担“完整接续上下文”的作用；阶段 4 前可以再单独精简。
- HTML 学习页只做事实同步，不在本轮重做视觉和结构。

## 后续文档待办

| 优先级 | 项目 | 说明 |
|--------|------|------|
| P1 | 精简 `Docs/00_CurrentStatus.md` | 建议改成“当前状态 + 下一步 + 关键历史链接”，把历史流水账移到归档 |
| P1 | 统一文档入口 | README 面向面试官，`00_CurrentStatus` 面向接续开发，`01_ProjectPlan` 面向路线规划，避免重复维护同一阶段清单 |
| P2 | HTML 页面维护策略 | 如果后续继续保留 HTML 学习页，需要每个阶段收尾时同步更新覆盖阶段和核心事实 |
