#!/bin/bash
# Full setup: Ubuntu 22 + .NET 8 + Docker SQL Server + Nginx + systemd
set -eu

APP_DIR="${APP_DIR:-/var/www/tim-tro}"
REPO_URL="${REPO_URL:-https://github.com/huyng1801/tim-tro.git}"
SA_PASSWORD="${SA_PASSWORD:-ThayDoiMatKhauManh}"
SERVICE_NAME="${SERVICE_NAME:-tim-tro}"

export DEBIAN_FRONTEND=noninteractive

echo "=== [1/8] Install packages ==="
apt-get update -y
apt-get install -y git curl nginx ufw docker.io

echo "=== [2/8] Install .NET 8 ==="
if ! command -v dotnet >/dev/null 2>&1; then
  curl -fsSL https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -o /tmp/packages-microsoft-prod.deb
  dpkg -i /tmp/packages-microsoft-prod.deb
  apt-get update -y
  apt-get install -y dotnet-sdk-8.0 aspnetcore-runtime-8.0
fi
dotnet --info | head -n 5

echo "=== [3/8] Start Docker SQL Server ==="
systemctl enable docker
systemctl start docker

if ! docker ps -a --format '{{.Names}}' | grep -qx sqlserver; then
  docker run -d --name sqlserver \
    -e "ACCEPT_EULA=Y" \
    -e "MSSQL_SA_PASSWORD=${SA_PASSWORD}" \
    -p 1433:1433 \
    --restart unless-stopped \
    mcr.microsoft.com/mssql/server:2022-latest
else
  docker start sqlserver || true
fi

echo "Waiting SQL Server..."
sleep 25

echo "=== [4/8] Clone / update source ==="
mkdir -p "$(dirname "$APP_DIR")"
if [ -d "$APP_DIR/.git" ]; then
  cd "$APP_DIR"
  git fetch origin master
  git reset --hard origin/master
else
  rm -rf "$APP_DIR"
  git clone "$REPO_URL" "$APP_DIR"
fi

echo "=== [5/8] Publish app ==="
cd "$APP_DIR/QLSanBongMini.Web"
dotnet publish -c Release -r linux-x64 --self-contained true \
  -o "$APP_DIR/publish" /p:UseAppHost=true
cp -r Views "$APP_DIR/publish/Views"
chown -R www-data:www-data "$APP_DIR/publish"

echo "=== [6/8] systemd service ==="
cat > "/etc/systemd/system/${SERVICE_NAME}.service" <<EOF
[Unit]
Description=Tim Tro / QLSanBongMini Web App
After=network.target docker.service

[Service]
WorkingDirectory=${APP_DIR}/publish
ExecStart=${APP_DIR}/publish/QLSanBongMini.Web
Restart=always
RestartSec=5
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5000
Environment=DOTNET_ROOT=/usr/share/dotnet

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
systemctl enable "${SERVICE_NAME}"
systemctl restart "${SERVICE_NAME}"

echo "=== [7/8] Nginx reverse proxy ==="
cat > /etc/nginx/sites-available/tim-tro <<'EOF'
server {
    listen 80 default_server;
    listen [::]:80 default_server;
    server_name _;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
EOF

ln -sf /etc/nginx/sites-available/tim-tro /etc/nginx/sites-enabled/tim-tro
rm -f /etc/nginx/sites-enabled/default
nginx -t
systemctl restart nginx

echo "=== [8/8] Firewall ==="
ufw allow OpenSSH || true
ufw allow 80/tcp || true
ufw --force enable || true

sleep 8
curl -s -o /dev/null -w "Login HTTP %{http_code}\n" http://127.0.0.1/Account/Login || true
systemctl is-active "${SERVICE_NAME}" || true
echo "Setup done: http://$(curl -s ifconfig.me 2>/dev/null || echo VPS_IP)/Account/Login"
