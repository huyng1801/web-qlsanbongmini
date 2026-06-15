#!/bin/bash
# Cap nhat nhanh: git pull + publish + restart
set -eu
APP_DIR="${APP_DIR:-/var/www/qlsanbongmini}"
export DOTNET_ROOT=/usr/share/dotnet
export PATH="/usr/share/dotnet:${PATH}"

cd "$APP_DIR"
git pull origin master

cd QLSanBongMini.Web
dotnet publish -c Release -r linux-x64 --self-contained true -o "$APP_DIR/publish" /p:UseAppHost=true
cp -r Views "$APP_DIR/publish/Views"
chown -R www-data:www-data "$APP_DIR/publish"
systemctl restart qlsanbongmini
sleep 8
curl -s -o /dev/null -w "HTTP %{http_code}\n" http://127.0.0.1/Account/Login
echo "Quick update done"
