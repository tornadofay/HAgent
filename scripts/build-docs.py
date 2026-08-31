from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def build(source_dir: Path, output: Path, title: str) -> None:
    parts = sorted(source_dir.glob("*.md"))
    if not parts:
        raise SystemExit(f"No documentation sources found: {source_dir}")

    sections = [
        f"# {title}",
        "",
        "> This file is generated from smaller source documents. Do not edit it directly.",
        "> Source directory: `" + str(source_dir.relative_to(ROOT)).replace('\\', '/') + "`.",
        "",
    ]

    for part in parts:
        text = part.read_text(encoding="utf-8").strip()
        lines = text.splitlines()
        if lines and lines[0].startswith("# "):
            lines[0] = "## " + lines[0][2:]
            text = "\n".join(lines)
        if text:
            sections.append(text)
            sections.append("")

    output.write_text("\n".join(sections).rstrip() + "\n", encoding="utf-8")


build(ROOT / "docs" / "plan", ROOT / "plan.md", "HAgent Development Plan")
build(ROOT / "docs" / "roadmap", ROOT / "roadmap.md", "HAgent Roadmap")
print("Generated plan.md and roadmap.md")
