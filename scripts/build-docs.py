from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]


# Roadmap source files intentionally use a stable, human-readable filename scheme.
# Keep assembly order explicit here so adding a new roadmap source does not depend
# on lexical filename ordering (for example, 0.10 vs 0.97 or auxiliary 0.97 phases).
ROADMAP_ORDER = [
    "00-overview.md",
    "10-foundation.md",
    "20-data-access.md",
    "30-agent-runtime.md",
    "35-generic-host-integration.md",
    "951-identity-tenancy-user-context.md",
    "952-event-subsystem.md",
    "953-unified-policy-engine.md",
    "954-prompt-instruction-governance.md",
    "955-context-engineering.md",
    "956-observability-tracing.md",
    "957-evaluation-quality-measurement.md",
    "9575-resource-governance-learning.md",
    "958-agent-lifecycle-health.md",
    "9591-goal-plan-persistence-recovery.md",
    "959-human-intervention.md",
    "9592-provider-ecosystem-adapter-lifecycle.md",
    "38-configuration-storage-and-portability.md",
    "36-capability-aware-execution.md",
    "37-persistent-cognitive-runtime.md",
    "cognitive-workbench.md",
    "cognitive-workbench-controls.md",
    "cognitive-workbench-learning.md",
    "40-workspaces-chat.md",
    "50-collaboration-workflows.md",
    "60-platform-and-release.md",
]


def roadmap_sort_key(path: Path) -> tuple[int, int, str]:
    try:
        return (ROADMAP_ORDER.index(path.name), 0, path.name)
    except ValueError:
        # New unregistered roadmap files are placed at the end rather than silently
        # changing the established order. This makes roadmap additions deliberate.
        return (len(ROADMAP_ORDER), 0, path.name)


def build(source_dir: Path, output: Path, title: str, *, roadmap: bool = False) -> None:
    if roadmap:
        parts = sorted(source_dir.glob("*.md"), key=roadmap_sort_key)
    else:
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
build(ROOT / "docs" / "roadmap", ROOT / "roadmap.md", "HAgent Roadmap", roadmap=True)
print("Generated plan.md and roadmap.md")
