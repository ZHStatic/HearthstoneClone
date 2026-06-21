---
name: hearthstone-code-review
description: Project-specific HearthstoneClone code review workflow. Use when working in D:\demo\HearthstoneClone and the user asks to find similar issues, do small code cleanup, review repeated patterns, inspect misleading UI feedback, check ignored actual rule results, or verify Core/UI dependency boundaries in the Unity C# card-battle project.
---

# HearthstoneClone Code Review

Use this skill only for the local `HearthstoneClone` Unity project. It is for finding small, teachable code-quality issues before changing code.

## Workflow

1. Read `AGENTS.md` and the current files involved in the request.
2. Run `scripts/find_review_candidates.py` from the repository root when the user asks for "similar issues" or broad cleanup candidates.
3. Treat script output as candidates, not proof. Open each relevant file and confirm the issue from code.
4. Prioritize findings that affect correctness, player feedback, architecture clarity, or repeated learning value.
5. Before modifying game C# classes, follow the project rule: list the class/change checklist and wait for user confirmation unless the user explicitly asked for a batch implementation.

## What To Check

- Ignored rule results: calls such as `TakeDamage()` or `Heal()` where the return value is discarded even though the actual result matters.
- Misleading UI feedback: UI text that reports intended damage or action result instead of Core's actual resolved result.
- Repeated formatter logic: duplicated keyword, battlecry, deathrattle, or status text conversion between UI classes.
- Manual string assembly: loops that build line-separated text with `+=` when `string.Join` would be clearer.
- Core/UI boundary: Core code must not depend on `UnityEngine.UI`, prefabs, buttons, text widgets, or view classes.
- Overgrown coordinator methods: `GameManager` and `GameUIController` can stay pragmatic, but new logic should not hide important rule outcomes.

## Project Rules

- Core owns rules and state. UI reads Core state and calls `GameManager.Try...`; UI must not directly change hand, mana, board, health, or keywords.
- Unity assets are editor-owned. Do not edit prefab, scene, ScriptableObject, image, audio, or `.meta` files unless the user explicitly confirms.
- New C# scripts should be `.cs` only; let Unity generate `.meta`.
- Prefer small, explainable refactors over broad architecture rewrites.
- If a temporary prototype shortcut is kept, state that it is a staged simplification and explain when it should be replaced.

## Reporting Style

When reporting candidates:

- Lead with confirmed findings, not raw script output.
- Include file links and line numbers.
- Separate "should fix now" from "keep in mind later".
- Explain why the issue matters in gameplay or learning terms.
- If proposing code edits, keep them scoped to the current phase.

## Script

Run from the repository root:

```powershell
& 'C:/Users/Static/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe' '.codex/skills/hearthstone-code-review/scripts/find_review_candidates.py'
```

The script scans `Assets/Scripts/**/*.cs` and prints candidate locations for manual review.
