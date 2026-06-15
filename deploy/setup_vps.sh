#!/bin/bash
# Setup VPS Ubuntu — clone repo, SQL Server Docker, .NET 8, Nginx, systemd
set -eu

REPO_URL="${REPO_URL:-https://github.com/huyng1801/web-qlsanbongmini.git}"
APP_DIR="${APP_DIR:-/var/www/qlsanbongmini}"
SA_PASSWORD="${SA_PASSWORD:-QlSb@Vps2026!Strong}"
BRANCH="${BRANCH:-master}"

echo "=== 1. System packages ==="
export DEBIAN_FRONTEND=noninteractive
apt-get update -qq
apt-get install -y -qq git nginx curl ca-certificates docker.io

echo "=== 2. .NET 8 SDK ==="
if ! command -v dotnet &>/dev/null; then
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet
  ln -sf /usr/share/dotnet/dotnet /usr/local/bin/dotnet || true
fi
export DOTNET_ROOT=/usr/share/dotnet
export PATH="/usr/share/dotnet:${PATH}"
dotnet --version

echo "=== 3. SQL Server (Docker) ==="
systemctl enable docker
systemctl start docker
if ! docker ps -a --format '{{.Names}}' | grep -qx sqlserver; then
  docker run -d --name sqlserver --restart unless-stopped \
    -e "ACCEPT_EULA=Y" \
    -e "MSSQL_SA_PASSWORD=${SA_PASSWORD}" \
    -p 127.0.0.1:1433:1433 \
    mcr.microsoft.com/mssql/server:2022-latest
  echo "Waiting SQL Server..."
  sleep 25
else
  docker start sqlserver || true
  sleep 5
fi

echo "=== 4. Clone / update source ==="
mkdir -p "$(dirname "$APP_DIR")"
if [ -d "$APP_DIR/.git" ]; then
  cd "$APP_DIR"
  git fetch origin
  git reset --hard "origin/${BRANCH}"
else
  rm -rf "$APP_DIR"
  git clone --branch "$BRANCH" --depth 1 "$REPO_URL" "$APP_DIR"
  cd "$APP_DIR"
fi

echo "=== 5. Production config ==="
PROD_CFG="QLSanBongMini.Web/appsettings.Production.json"
cat > "$PROD_CFG" <<EOF
{
  "Database": { "Provider": "SqlServer" },
  "ConnectionStrings": {
    "SqlServer": "Server=127.0.0.1,1433;Database=QLSanBongMini;User Id=sa;Password=${SA_PASSWORD};TrustServerCertificate=True;Encrypt=False;"
  },
  "Logging": {
    "LogLevel": { "Default": "Warning", "Microsoft.AspNetCore": "Warning" }
  }
}
EOF

echo "=== 6. Publish app ==="
cd QLSanBongMini.Web
export DOTNET_ROOT=/usr/share/dotnet
export PATH="/usr/share/dotnet:${PATH}"
dotnet publish -c Release -r linux-x64 --self-contained true -o "$APP_DIR/publish" /p:UseAppHost=true
cp -r Views "$APP_DIR/publish/Views"

echo "=== 7. systemd service ==="
cat > /etc/systemd/system/qlsanbongmini.service <<EOF
[Unit]
Description=QL San Bong Mini Web
After=network.target docker.service

[Service]
WorkingDirectory=${APP_DIR}/publish
ExecStart=${APP_DIR}/publish/QLSanBongMini.Web
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5000
Restart=always
RestartSec=5
User=www-data

[Install]
WantedBy=multi-user.target
EOF

chown -R www-data:www-data "$APP_DIR/publish"

echo "=== 8. Nginx ==="
cat > /etc/nginx/sites-available/qlsanbongmini <<'NGX'
server {
    listen 80 default_server;
    listen [::]:80 default_server;
    server_name _;
    client_max_body_size 20M;

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
NGX
ln -sf /etc/nginx/sites-available/qlsanbongmini /etc/nginx/sites-enabled/qlsanbongmini
rm -f /etc/nginx/sites-enabled/default
nginx -t

systemctl daemon-reload
systemctl enable qlsanbongmini nginx
systemctl restart qlsanbongmini nginx

echo "=== 9. Health check ==="
sleep 12
HTTP=$(curl -s -o /dev/null -w '%{http_code}' http://127.0.0.1/Account/Login)
TOKEN=$(curl -s http://127.0.0.1/Account/Login | grep -c RequestVerificationToken || true)
echo "HTTP /Account/Login: $HTTP, token=$TOKEN"
systemctl is-active qlsanbongmini nginx docker
echo "DONE — http://$(curl -s ifconfig.me 2>/dev/null || hostname -I | awk '{print $1}')/Account/Login"
