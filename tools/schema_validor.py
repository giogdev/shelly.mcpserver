#!/usr/bin/env python3
"""Validate a JSON/JSONC file against a JSON Schema.

Usage:
    python schema_validor.py <schema_path> <data_path>
"""

from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
from pathlib import Path


def load_json_allow_comments(path: Path) -> object:
    text = path.read_text(encoding="utf-8")
    # Support JSONC-like files by stripping block and line comments.
    text = re.sub(r"/\*.*?\*/", "", text, flags=re.S)
    text = re.sub(r"(^|\s)//.*?$", "", text, flags=re.M)
    return json.loads(text)


def ensure_jsonschema_installed() -> None:
    try:
        import jsonschema  # noqa: F401
    except Exception:
        subprocess.check_call(
            [sys.executable, "-m", "pip", "install", "jsonschema"],
            stdout=subprocess.DEVNULL,
        )


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Validate a JSON/JSONC data file against a JSON Schema"
    )
    parser.add_argument("schema_path", help="Path to JSON Schema file")
    parser.add_argument("data_path", help="Path to JSON or JSONC file to validate")
    return parser


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()

    schema_path = Path(args.schema_path)
    data_path = Path(args.data_path)

    if not schema_path.exists():
        print(f"ERROR: schema file not found: {schema_path}")
        return 3
    if not data_path.exists():
        print(f"ERROR: data file not found: {data_path}")
        return 3

    try:
        schema = load_json_allow_comments(schema_path)
    except Exception as exc:
        print(f"ERROR: unable to parse schema file '{schema_path}': {exc}")
        return 4

    try:
        data = load_json_allow_comments(data_path)
    except Exception as exc:
        print(f"ERROR: unable to parse data file '{data_path}': {exc}")
        return 4

    ensure_jsonschema_installed()
    import jsonschema

    validator = jsonschema.Draft202012Validator(schema)
    errors = sorted(validator.iter_errors(data), key=lambda e: list(e.path))

    if not errors:
        print("VALID: file matches schema")
        return 0

    print(f"INVALID: found {len(errors)} error(s)")
    for idx, err in enumerate(errors[:20], start=1):
        path = "".join(
            [
                f"[{repr(p)}]" if isinstance(p, str) else f"[{p}]"
                for p in err.path
            ]
        )
        printable_path = path if path else "<root>"
        print(f"{idx}. path={printable_path} | message={err.message}")

    if len(errors) > 20:
        print(f"... {len(errors) - 20} additional error(s) omitted")

    return 2


if __name__ == "__main__":
    raise SystemExit(main())
