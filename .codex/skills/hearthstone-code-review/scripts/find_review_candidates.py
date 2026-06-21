from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
import re
import sys

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")


@dataclass
class Candidate:
    rule: str
    path: Path
    line_number: int
    line: str


RULES = {
    "ignored_take_damage": "TakeDamage return value is ignored; actual damage may matter.",
    "ignored_heal": "Heal return value is ignored; actual heal may matter.",
    "manual_string_concat": "String is appended in a loop; string.Join may be clearer.",
    "keyword_formatter_duplication": "Keyword text formatting appears in a UI view; consider shared formatter.",
    "ui_dependency_in_core": "Core file references Unity UI namespace.",
    "feedback_intended_damage": "UI feedback may use intended damage instead of resolved damage.",
}


def iter_cs_files(root: Path) -> list[Path]:
    scripts_root = root / "Assets" / "Scripts"
    if not scripts_root.exists():
        return []

    return sorted(scripts_root.rglob("*.cs"))


def scan_file(root: Path, path: Path) -> list[Candidate]:
    candidates: list[Candidate] = []
    relative_path = path.relative_to(root)
    text = path.read_text(encoding="utf-8-sig")
    lines = text.splitlines()
    loop_window = 0

    for index, line in enumerate(lines, start=1):
        stripped = line.strip()

        if re.search(r"^\s*(for|foreach)\s*\(", line):
            loop_window = 25

        if re.search(r"^\s*[A-Za-z0-9_\.]+\s*\.\s*TakeDamage\s*\(", line):
            candidates.append(Candidate("ignored_take_damage", relative_path, index, stripped))

        if re.search(r"^\s*[A-Za-z0-9_\.]+\s*\.\s*Heal\s*\(", line):
            candidates.append(Candidate("ignored_heal", relative_path, index, stripped))

        if loop_window > 0 and "+=" in line and ("text" in stripped or "statusText" in stripped):
            candidates.append(Candidate("manual_string_concat", relative_path, index, stripped))

        if "GetKeywordText" in line and path.name in {"CardView.cs", "MinionView.cs"}:
            candidates.append(Candidate("keyword_formatter_duplication", relative_path, index, stripped))

        normalized_path = str(relative_path).replace("\\", "/")
        if "using UnityEngine.UI;" in line and normalized_path.startswith("Assets/Scripts/Core/"):
            candidates.append(Candidate("ui_dependency_in_core", relative_path, index, stripped))

        if path.name == "GameUIController.cs" and ("造成 {damage}" in line or "SpellDamage" in line):
            candidates.append(Candidate("feedback_intended_damage", relative_path, index, stripped))

        if loop_window > 0:
            loop_window -= 1

    return candidates


def print_candidates(candidates: list[Candidate]) -> None:
    if not candidates:
        print("No candidates found.")
        return

    grouped: dict[str, list[Candidate]] = {}
    for candidate in candidates:
        grouped.setdefault(candidate.rule, []).append(candidate)

    for rule, items in grouped.items():
        print(f"\n[{rule}] {RULES[rule]}")
        for item in items:
            print(f"  {item.path}:{item.line_number}: {item.line}")


def main() -> int:
    root = Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else Path.cwd().resolve()
    files = iter_cs_files(root)
    if not files:
        print("No C# files found under Assets/Scripts.")
        return 1

    candidates: list[Candidate] = []
    for path in files:
        candidates.extend(scan_file(root, path))

    print_candidates(candidates)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
