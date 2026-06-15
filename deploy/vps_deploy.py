#!/usr/bin/env python3
"""Push to GitHub and run full VPS setup on Ubuntu 22."""
from __future__ import annotations

import os
import subprocess
import sys
from pathlib import Path

import paramiko

ROOT = Path(__file__).resolve().parents[1]
SETUP = Path(__file__).parent / "setup_vps.sh"
HOST = os.environ.get("VPS_HOST", "103.72.97.87")
PORT = int(os.environ.get("VPS_SSH_PORT", "24700"))
USER = os.environ.get("VPS_USER", "root")
PASSWORD = os.environ.get("VPS_PASSWORD", "")
REMOTE = os.environ.get("GITHUB_REPO", "https://github.com/huyng1801/tim-tro.git")


def git_push() -> None:
    os.chdir(ROOT)
    subprocess.check_call(["git", "add", "-A"])
    st = subprocess.run(["git", "status", "--porcelain"], capture_output=True, text=True)
    if st.stdout.strip():
        msg = os.environ.get("COMMIT_MSG", "Clean project and add VPS deploy scripts")
        subprocess.check_call(["git", "commit", "-m", msg])
    remote = subprocess.run(["git", "remote", "get-url", "origin"], capture_output=True, text=True)
    if "tim-tro" not in (remote.stdout or ""):
        subprocess.check_call(["git", "remote", "set-url", "origin", REMOTE])
    subprocess.check_call(["git", "push", "-u", "origin", "master"])


def run_setup_on_vps() -> int:
    if not PASSWORD:
        print("Set VPS_PASSWORD environment variable")
        return 1

    content = SETUP.read_text(encoding="utf-8").replace("\r\n", "\n")
    ssh = paramiko.SSHClient()
    ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    ssh.connect(HOST, port=PORT, username=USER, password=PASSWORD, timeout=120)
    sftp = ssh.open_sftp()
    with sftp.open("/tmp/setup_vps.sh", "w") as f:
        f.write(content)
    sftp.chmod("/tmp/setup_vps.sh", 0o755)
    sftp.close()

    cmd = f"REPO_URL={REMOTE} bash /tmp/setup_vps.sh 2>&1"
    stdin, stdout, stderr = ssh.exec_command(cmd, timeout=1800)
    out = stdout.read().decode(errors="replace")
    code = stdout.channel.recv_exit_status()
    ssh.close()
    print(out)
    return code


if __name__ == "__main__":
    print("=== Git push ===")
    git_push()
    print("=== VPS setup ===")
    sys.exit(run_setup_on_vps())
