from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


ROADMAP_SOURCES = [
    "00-overview.md",
    "10-foundation.md",
    "20-data-access.md",
    "30-agent-runtime.md",
    "35-generic-host-integration.md",
    "36-capability-aware-execution.md",
    "37-persistent-cognitive-runtime.md",
    "38-configuration-storage-and-portability.md",
    # These are subdocuments of 0.97, not separate roadmap phases.
    "cognitive-workbench.md",
    "cognitive-workbench-controls.md",
    "cognitive-workbench-learning.md",
    "40-workspaces-chat.md",
    "50-collaboration-workflows.md",
    "60-platform-and-release.md",
    "951-identity-tenancy-user-context.md",
    "952-event-subsystem.md",
    "953-unified-policy-engine.md",
    "954-prompt-instruction-governance.md",
    "955-context-engineering.md",
    "956-observability-tracing.md",
    "957-evaluation-quality-measurement.md",
    "9575-resource-governance-learning.md",
    "9576-learned-resource-reliability-adaptation-consolidation.md",
    "958-agent-lifecycle-health.md",
    "9591-goal-plan-persistence-recovery.md",
    "959-human-intervention.md",
    "9592-provider-ecosystem-adapter-lifecycle.md",
]


def normalize_heading(text: str) -> str:
    lines = text.splitlines()
    if lines and lines[0].startswith("# "):
        lines[0] = "## " + lines[0][2:]
    return "\n".join(lines)


def build_roadmap() -> None:
    source_dir = ROOT / "docs" / "roadmap"
    sections = [
        "# HAgent Roadmap",
        "",
        "> This file is generated from the complete active V1 roadmap source set. Do not edit it directly.",
        "> V2/research-only material belongs in `roadmapv2.md` and is intentionally excluded.",
        "> Source directory: `docs/roadmap`.",
        "",
    ]

    for filename in ROADMAP_SOURCES:
        part = source_dir / filename
        if not part.exists():
            raise SystemExit(f"Missing roadmap source: {part}")

        text = part.read_text(encoding="utf-8").strip()
        if text:
            sections.append(normalize_heading(text))
            sections.append("")

    (ROOT / "roadmap.md").write_text(
        "\n".join(sections).rstrip() + "\n",
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
        if text:
            sections.append(normalize_heading(text))
            sections.append("")

    (ROOT / "plan.md").write_text(
        "\n".join(sections).rstrip() + "\n",
        encoding="utf-8",
    )


build_plan()
build_roadmap()
print("Generated plan.md and roadmap.md")
