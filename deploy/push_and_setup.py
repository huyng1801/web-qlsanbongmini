#!/usr/bin/env python3
"""Push GitHub (neu can) + SSH setup VPS moi."""
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
SETUP = Path(__file__).parent / "setup_vps.sh"


def run_git_push():
    os.chdir(ROOT)
    if not (ROOT / ".git").exists():
        subprocess.check_call(["git", "init"])
    # remote
    remotes = subprocess.run(["git", "remote"], capture_output=True, text=True)
    if "origin" not in (remotes.stdout or ""):
        subprocess.check_call([
            "git", "remote", "add", "origin",
            "https://github.com/huyng1801/web-qlsanbongmini.git",
        ])
    else:
        subprocess.check_call([
            "git", "remote", "set-url", "origin",
            "https://github.com/huyng1801/web-qlsanbongmini.git",
        ])
    subprocess.check_call(["git", "add", "-A"])
    status = subprocess.run(["git", "status", "--porcelain"], capture_output=True, text=True)
    if status.stdout.strip():
        subprocess.check_call(["git", "commit", "-m", "Initial commit: QL San Bong Mini ASP.NET Core"])
    subprocess.check_call(["git", "branch", "-M", "master"])
    # push — HTTPS (public repo)
    r = subprocess.run(
        ["git", "push", "-u", "origin", "master", "--force"],
        capture_output=True, text=True,
    )
    print(r.stdout)
    if r.returncode != 0:
        print("git push stderr:", r.stderr)
        # try gh
        gr = subprocess.run(["gh", "repo", "view", "huyng1801/web-qlsanbongmini"], capture_output=True)
        if gr.returncode != 0:
            subprocess.run([
                "gh", "repo", "create", "huyng1801/web-qlsanbongmini",
                "--public", "--source", ".", "--remote", "origin", "--push",
            ], check=False)
        else:
            subprocess.check_call(["git", "push", "-u", "origin", "master", "--force"])
    print("GitHub push OK")


def setup_vps():
    if not PASSWORD:
        print("Set VPS_PASSWORD"); sys.exit(1)
    if not SETUP.exists():
        print("Missing setup_vps.sh"); sys.exit(1)
    script = SETUP.read_text(encoding="utf-8")
    for attempt in range(3):
        try:
            print(f"SSH {HOST}:{PORT} attempt {attempt + 1}...")
            ssh = paramiko.SSHClient()
            ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
            ssh.connect(HOST, port=PORT, username=USER, password=PASSWORD, timeout=90, banner_timeout=90)
            sftp = ssh.open_sftp()
            # upload with LF line endings
            content = SETUP.read_text(encoding="utf-8").replace("\r\n", "\n")
            with sftp.open("/tmp/setup_vps.sh", "w") as f:
                f.write(content)
            sftp.chmod("/tmp/setup_vps.sh", 0o755)
            sftp.close()
            cmd = "bash /tmp/setup_vps.sh 2>&1"
            stdin, stdout, stderr = ssh.exec_command(cmd, timeout=900)
            out = stdout.read().decode()
            err = stderr.read().decode()
            code = stdout.channel.recv_exit_status()
            print(out)
            if err:
                print("stderr:", err)
            ssh.close()
            if code == 0:
                print("VPS SETUP OK")
                return True
            print("Setup exit code:", code)
        except Exception as e:
            print("SSH error:", e)
            time.sleep(10)
    return False


def test_http():
    import re
    import requests
    base = f"http://{HOST}"
    s = requests.Session()
    r = s.get(f"{base}/Account/Login", timeout=30)
    m = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', r.text)
    print(f"Login page: {r.status_code}, token={bool(m)}")
    if not m:
        return False
    r2 = s.post(f"{base}/Account/Login", data={
        "__RequestVerificationToken": m.group(1),
        "UserName": "admin", "Password": "Admin@123", "RememberMe": "false",
    }, allow_redirects=False, timeout=30)
    print(f"POST login: {r2.status_code}")
    if r2.status_code not in (302, 303):
        return False
    for p in ["/Dashboard", "/LichSan", "/DatSan"]:
        r3 = s.get(base + p, timeout=20)
        print(f"  {p}: {r3.status_code}")
    return True


def main():
    print("=== GIT PUSH ===")
    try:
        run_git_push()
    except Exception as e:
        print("Git push warning:", e)

    print("\n=== VPS SETUP ===")
    if not setup_vps():
        sys.exit(1)

    print("\n=== HTTP TEST ===")
    time.sleep(5)
    ok = test_http()
    print("HTTP test:", "OK" if ok else "FAIL")
    print(f"\nURL: http://{HOST}/Account/Login")
    print("admin / Admin@123")
    sys.exit(0 if ok else 1)


if __name__ == "__main__":
    main()
