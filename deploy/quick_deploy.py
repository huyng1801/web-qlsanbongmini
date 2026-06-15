#!/usr/bin/env python3
"""Git push + quick update VPS."""
import os
import subprocess
import sys
import time
from pathlib import Path

import paramiko

ROOT = Path(__file__).resolve().parents[1]
HOST = os.environ.get("VPS_HOST", "103.72.97.87")
PORT = int(os.environ.get("VPS_SSH_PORT", "24700"))
USER = "root"
PASSWORD = os.environ.get("VPS_PASSWORD", "")
QUICK = Path(__file__).parent / "quick_update.sh"


def git_push():
    os.chdir(ROOT)
    subprocess.check_call(["git", "add", "-A"])
    st = subprocess.run(["git", "status", "--porcelain"], capture_output=True, text=True)
    if st.stdout.strip():
        subprocess.check_call(["git", "commit", "-m", os.environ.get("COMMIT_MSG", "Update")])
    subprocess.check_call(["git", "push", "origin", "master"])


def quick_deploy():
    content = QUICK.read_text(encoding="utf-8").replace("\r\n", "\n")
    ssh = paramiko.SSHClient()
    ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    ssh.connect(HOST, port=PORT, username=USER, password=PASSWORD, timeout=90)
    sftp = ssh.open_sftp()
    with sftp.open("/tmp/quick_update.sh", "w") as f:
        f.write(content)
    sftp.chmod("/tmp/quick_update.sh", 0o755)
    sftp.close()
    stdin, stdout, stderr = ssh.exec_command("bash /tmp/quick_update.sh 2>&1", timeout=600)
    out = stdout.read().decode()
    code = stdout.channel.recv_exit_status()
    ssh.close()
    print(out)
    return code == 0


if __name__ == "__main__":
    if not PASSWORD:
        print("Set VPS_PASSWORD"); sys.exit(1)
    print("=== Git push ===")
    git_push()
    print("=== VPS quick update ===")
    ok = quick_deploy()
    sys.exit(0 if ok else 1)
