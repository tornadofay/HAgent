from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def build_directory(source_dir: Path, output: Path, title: str) -> None:
    part = source_dir / "00-overview.md"
    if not part.exists():
        raise SystemExit(f"Missing roadmap source: {part}")

    text = part.read_text(encoding="utf-8").strip()
    lines = text.splitlines()
    if lines and lines[0].startswith("# "):
        lines[0] = "## " + lines[0][2:]
        text = "\n".join(lines)

    output.write_text(
        "\n".join(
            [
                f"# {title}",
                "",
                "> This file is generated from the active roadmap source. Do not edit it directly.",
                "> Source: `docs/roadmap/00-overview.md`.",
                "",
                text,
            ]
        ).rstrip()
        + "\n",
        encoding="utf-8",
    )


def build_plan() -> None:
    source_dir = ROOT / "docs" / "plan"
    parts = sorted(source_dir.glob("*.md"))
    if not parts:
        raise SystemExit(f"No documentation sources found: {source_dir}")

    sections = [
        "# HAgent Development Plan",
        "",
        "> This file is generated from smaller source documents. Do not edit it directly.",
        "> Source directory: `docs/plan`.",
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

    (ROOT / "plan.md").write_text("\n".join(sections).rstrip() + "\n", encoding="utf-8")


build_plan()
build_directory(ROOT / "docs" / "roadmap", ROOT / "roadmap.md", "HAgent Roadmap")
print("Generated plan.md and roadmap.md")
