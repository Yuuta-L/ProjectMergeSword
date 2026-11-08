#!/usr/bin/env python3
"""Make sure this Unity project has its own, working Locus workspace.

Locus identifies a workspace by ``<root>/Locus/config.json`` -> ``workspace_id`` and
keeps one ``workspace_checkouts`` row per root in the app-data ``locus.db``. Copying a
project directory copies that identity too, so the copy gets registered as a sibling
checkout of the original project instead of becoming a workspace of its own - and that
binding never self-heals.

Run this once in a project copied from the template, before adding the folder in Locus:

    python locus-workspace-identity.py          # repair/allocate this project's identity
    python locus-workspace-identity.py --check  # same, but report only
    python locus-workspace-identity.py --all    # health check of every registered checkout

It is idempotent: on a healthy project it changes nothing. Only rows belonging to this
root are ever written. When it had to touch the registry, restart Locus - the registry is
read at startup only.
"""

from __future__ import annotations

import argparse
import json
import os
import sqlite3
import time
import uuid
from pathlib import Path

CHECKOUTS_SQL = """
select c.checkout_id, c.project_id, c.root_path, c.normalized_root, p.project_id, p.is_visible
from workspace_checkouts c left join workspace_projects p on p.project_id = c.project_id
order by c.root_path
"""


def data_dir() -> Path:
    """Locus app-data directory, as recorded by the storage location choice."""
    appdata = os.environ.get("APPDATA")
    if appdata:
        cfg = Path(appdata) / "locus" / "storage_last_default.json"
        if cfg.is_file():
            return Path(json.loads(cfg.read_text(encoding="utf-8"))["path"])
    return Path("F:/locus/data")


def normalize(root: Path) -> str:
    """Same shape Locus stores in workspace_checkouts.normalized_root."""
    return str(root.resolve()).replace("\\", "/").rstrip("/").lower()


def read_workspace_id(root: Path) -> str | None:
    config = root / "Locus" / "config.json"
    if not config.is_file():
        return None
    return json.loads(config.read_text(encoding="utf-8")).get("workspace_id")


def write_workspace_id(root: Path, workspace_id: str) -> None:
    config = root / "Locus" / "config.json"
    fields = {}
    if config.is_file():
        fields = json.loads(config.read_text(encoding="utf-8"))
    fields.setdefault("knowledge_enabled", True)
    fields.setdefault("unity_test_tools_enabled", False)
    fields = {k: v for k, v in fields.items() if k != "workspace_id"} | {"workspace_id": workspace_id}
    config.parent.mkdir(parents=True, exist_ok=True)
    config.write_text(json.dumps(fields, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def new_workspace_id() -> str:
    return "unity-" + uuid.uuid4().hex[:24]


def checkouts(con: sqlite3.Connection) -> list[dict]:
    return [
        {"checkout_id": r[0], "project_id": r[1], "root": Path(r[2]),
         "normalized_root": r[3], "registered_project_id": r[4], "visible": r[5]}
        for r in con.execute(CHECKOUTS_SQL)
    ]


def check(con: sqlite3.Connection) -> int:
    rows = checkouts(con)
    shared: dict[str, list[str]] = {}
    for row in rows:
        shared.setdefault(row["project_id"], []).append(str(row["root"]))

    bad = 0
    for row in rows:
        root, config_id = row["root"], read_workspace_id(row["root"])
        problems = []
        if config_id is None:
            problems.append("Locus/config.json missing")
        elif config_id != row["project_id"]:
            problems.append(f"config {config_id} != checkout {row['project_id']}")
        if len(shared[row["project_id"]]) > 1:
            problems.append("project id shared with " + ", ".join(
                p for p in shared[row["project_id"]] if p != str(root)))
        if row["registered_project_id"] is None:
            problems.append("no workspace_projects row")
        elif not row["visible"]:
            problems.append("project hidden (is_visible=0)")
        bad += bool(problems)
        print(("BAD " if problems else "OK  ") + f"{root}  {config_id or '-'}  {row['project_id']}"
              + ("  <- " + "; ".join(problems) if problems else ""))
    print(f"\n{len(rows)} checkout(s), {bad} with problems")
    return bad


def fix(con: sqlite3.Connection, root: Path, apply: bool) -> int:
    root = root.resolve()
    key = normalize(root)
    row = next((r for r in checkouts(con) if r["normalized_root"] == key), None)
    config_id = read_workspace_id(root)
    others = {r["project_id"] for r in checkouts(con) if r["normalized_root"] != key}

    # Prefer an identity no other root uses; config.json wins over a stale checkout row,
    # and an inherited (colliding) id is always rotated.
    if config_id and config_id not in others:
        target, reason = config_id, None
        if row and row["project_id"] != target:
            reason = "checkout row points at another project"
        elif row and not row["visible"]:
            reason = "project row missing or hidden"
    elif row and row["project_id"] not in others:
        target, reason = row["project_id"], "Locus/config.json missing"
    else:
        target = new_workspace_id()
        reason = ("config id is also used by another root (copied identity)"
                  if config_id else "Locus/config.json missing, checkout row unusable")

    print(f"root     {root}")
    print(f"config   {config_id or '-'}  ->  {target}")
    print(f"checkout {row['checkout_id'] + '  ' + row['project_id'] if row else 'not registered yet'}  ->  {target}")
    if row is None:
        print("note     not in the Locus registry yet - add this folder in Locus once")
    if reason is None:
        print("already consistent - nothing to do")
        return 0
    print(f"reason   {reason}")
    if not apply:
        print("\ndry run - nothing written")
        return 0
    if target != config_id:
        write_workspace_id(root, target)
    if row is None:
        print("done     identity written - add this folder in Locus once to get the workspace")
        return 0

    old = row["project_id"]
    with con:
        if old != target:
            con.execute("update workspace_checkouts set project_id=? where checkout_id=?",
                        (target, row["checkout_id"]))
        con.execute("insert or ignore into workspace_projects (project_id, last_opened_at, is_visible)"
                    " values (?,?,1)", (target, int(time.time())))
        con.execute("update workspace_projects set is_visible=1, last_opened_at=? where project_id=?",
                    (int(time.time()), target))
        if old != target:
            still_used = con.execute("select count(*) from workspace_checkouts where project_id=?",
                                     (old,)).fetchone()[0]
            sessions = con.execute("select count(*) from sessions where workspace_id=?",
                                   (old,)).fetchone()[0]
            if not still_used and not sessions:
                con.execute("delete from workspace_projects where project_id=?", (old,))
                print(f"cleaned orphan project row {old}")
            elif not still_used:
                print(f"kept     project row {old}: {sessions} session(s) still use it")
    print("done     binding repaired - restart Locus to load the registry")
    return 0


def self_test() -> int:
    """Exercise fix() against a scratch copy of the real registry."""
    import tempfile

    with tempfile.TemporaryDirectory() as tmp:
        tmp = Path(tmp)
        db = tmp / "locus.db"
        source, target_db = sqlite3.connect(data_dir() / "locus.db"), sqlite3.connect(db)
        source.backup(target_db)
        source.close()
        target_db.close()
        con = sqlite3.connect(db)

        def register(name: str, config_id: str | None, checkout_id: str, row_id: str) -> Path:
            root = tmp / name
            (root / "Locus").mkdir(parents=True)
            if config_id:
                (root / "Locus" / "config.json").write_text(
                    json.dumps({"workspace_id": config_id}), encoding="utf-8")
            con = sqlite3.connect(db)
            con.execute("insert into workspace_checkouts values (?,?,?,?,0)",
                        (checkout_id, row_id, str(root), normalize(root)))
            con.execute("insert or ignore into workspace_projects values (?,0,1)", (row_id,))
            con.commit()
            con.close()
            return root

        copied = register("Copied", "unity-aaaa", "checkout-copied", "unity-shared")
        register("Original", "unity-shared", "checkout-original", "unity-shared")
        stale = register("Stale", "unity-config", "checkout-stale", "unity-old")

        assert fix(con, copied, apply=True) == 0
        assert con.execute("select project_id from workspace_checkouts where checkout_id='checkout-copied'").fetchone()[0] == "unity-aaaa"
        assert con.execute("select is_visible from workspace_projects where project_id='unity-aaaa'").fetchone()[0] == 1
        assert con.execute("select count(*) from workspace_projects where project_id='unity-shared'").fetchone()[0] == 1, "other root keeps its row"

        assert fix(con, stale, apply=True) == 0
        assert con.execute("select project_id from workspace_checkouts where checkout_id='checkout-stale'").fetchone()[0] == "unity-config"
        assert con.execute("select count(*) from workspace_projects where project_id='unity-old'").fetchone()[0] == 0, "orphan row removed"

        before = con.execute("select * from workspace_checkouts order by checkout_id").fetchall()
        assert fix(con, copied, apply=True) == 0, "idempotent"
        assert con.execute("select * from workspace_checkouts order by checkout_id").fetchall() == before
        con.close()
    print("self-test passed")
    return 0


def project_root() -> Path:
    """Nearest Unity project above this script (the script ships at the template root)."""
    here = Path(__file__).resolve().parent
    for candidate in (here, *here.parents):
        if (candidate / "ProjectSettings" / "ProjectVersion.txt").is_file():
            return candidate
    return here


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("root", nargs="?", help="project root (default: the project this script lives in)")
    parser.add_argument("--check", action="store_true", help="report what would change, write nothing")
    parser.add_argument("--all", action="store_true", help="health check of every registered checkout")
    parser.add_argument("--db", help="locus.db to inspect (default: the app data directory)")
    parser.add_argument("--self-test", action="store_true", help="run the built-in check against a scratch registry copy")
    args = parser.parse_args()

    if args.self_test:
        return self_test()

    db = Path(args.db) if args.db else data_dir() / "locus.db"
    if not db.is_file():
        print(f"locus.db not found: {db}")
        return 2

    con = sqlite3.connect(db, timeout=15)
    con.execute("pragma busy_timeout=15000")
    if args.all:
        return 1 if check(con) else 0
    root = Path(args.root) if args.root else project_root()
    if not root.is_dir():
        print(f"not a directory: {root}")
        return 2
    return fix(con, root, apply=not args.check)


if __name__ == "__main__":
    raise SystemExit(main())
