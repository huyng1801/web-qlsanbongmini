#!/usr/bin/env python3
import re, requests, sys
HOST = "103.72.97.87"
BASE = f"http://{HOST}"

def main():
    s = requests.Session()
    r = s.get(f"{BASE}/Account/Login", timeout=20)
    t = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', r.text)
    assert t, "no token"
    r2 = s.post(f"{BASE}/Account/Login", data={
        "__RequestVerificationToken": t.group(1),
        "UserName": "admin", "Password": "Admin@123", "RememberMe": "false",
    }, allow_redirects=False, timeout=20)
    assert r2.status_code in (302, 303), f"login {r2.status_code}"
    pages = ["/Dashboard", "/LichSan", "/DatSan", "/DatSan/Create", "/KhachHang",
             "/San", "/KhungGio", "/BangGia", "/NguoiDung"]
    ok = 0
    for p in pages:
        r3 = s.get(BASE + p, timeout=20)
        good = r3.status_code == 200
        print(f"[{'OK' if good else 'FAIL'}] {p} {r3.status_code}")
        if good: ok += 1
    print(f"Result: {ok}/{len(pages)}")
    return 0 if ok == len(pages) else 1

if __name__ == "__main__":
    sys.exit(main())
