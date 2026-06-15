# web-qlsanbongmini

# Website Quản Lý Sân Bóng Đá Mini

Đồ án môn **Lập trình Web với ASP.NET** — Bộ môn Công nghệ thông tin, Trường Đại học Trà Vinh.

Hệ thống hỗ trợ quản lý sân, đặt lịch, khách hàng và thanh toán cơ bản. Source code chạy được trên **Visual Studio + SQL Server** (máy khách) và **VPS** (demo online).

---

## Công nghệ sử dụng

| Thành phần | Công nghệ |
|------------|-----------|
| Framework | ASP.NET Core 8 MVC |
| Ngôn ngữ | C# |
| ORM | Entity Framework Core |
| Cơ sở dữ liệu | Microsoft SQL Server |
| Xác thực | ASP.NET Core Identity |
| Giao diện | Razor View + Bootstrap 5 |

---

## Chức năng chính

- **Đăng nhập / phân quyền:** Admin, Nhân viên
- **Quản lý sân:** Thêm, sửa, xóa sân; trạng thái hoạt động / bảo trì
- **Bảng giá:** Giá theo khung giờ (sáng, chiều, tối, cuối tuần)
- **Đặt sân:** Chọn sân, ngày, khung giờ; kiểm tra trùng lịch
- **Khách hàng:** Lưu thông tin và lịch sử đặt sân
- **Thanh toán:** Tiền mặt / chuyển khoản; trạng thái cọc / thanh toán đủ
- **Lịch sân:** Xem lưới theo ngày (trống / đã đặt / bảo trì)
- **Dashboard:** Thống kê lịch đặt và doanh thu cơ bản

---

## Yêu cầu hệ thống

### Máy phát triển / cài đặt cho khách hàng

- Windows 10/11
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (workload **ASP.NET and web development**)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) hoặc SQL Server Developer
- [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) (khuyến nghị)

### VPS demo

- Windows Server hoặc Windows VPS (khuyến nghị cho SQL Server + IIS)
- RAM tối thiểu 2 GB
- SQL Server Express
- IIS + [.NET 8 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## Cấu trúc thư mục

```
web-qlsanbongmini/
├── QLSanBongMini.sln
├── QLSanBongMini.Web/       # Project MVC chính
│   ├── Controllers/
│   ├── Models/
│   ├── Views/
│   ├── Data/
│   ├── Services/
│   └── wwwroot/
├── Database/
│   ├── Script_TaoDB.sql     # Script tạo database (nếu không dùng migration)
│   └── SeedData.sql         # Dữ liệu mẫu
└── README.md
```

---

## Cài đặt trên máy local (Visual Studio + SQL Server hoặc SQLite)

### Cơ sở dữ liệu tự chọn (mặc định)

Trong `appsettings.json`:

```json
"Database": {
  "Provider": "Auto"
}
```

| Giá trị | Hành vi |
|---------|---------|
| `Auto` | Có SQL Server → dùng SQL Server; không có → **tự chuyển SQLite** (`qlsanbongmini.db`) |
| `SqlServer` | Bắt buộc SQL Server |
| `Sqlite` | Luôn dùng SQLite (phát triển / máy không cài SQL Server) |

> **.NET 8.0.28** — project ghim `RuntimeFrameworkVersion` 8.0.28, chạy ổn trên máy đã cài .NET 8 Runtime 8.0.28.

### Bước 1: Clone hoặc copy source code

```powershell
git clone <url-repo> web-qlsanbongmini
cd web-qlsanbongmini
```

### Bước 2: Cấu hình connection string

Mở file `QLSanBongMini.Web/appsettings.json`:

```json
"Database": { "Provider": "Auto" },
"ConnectionStrings": {
  "SqlServer": "Server=.;Database=QLSanBongMini;Trusted_Connection=True;TrustServerCertificate=True;",
  "Sqlite": "Data Source=qlsanbongmini.db"
}
```

| Tham số | Ý nghĩa |
|---------|---------|
| `Server=.` | SQL Server trên cùng máy |
| `Database=QLSanBongMini` | Tên database |
| `Trusted_Connection=True` | Đăng nhập Windows |

Nếu dùng SQL Server Authentication:

```json
"DefaultConnection": "Server=.;Database=QLSanBongMini;User Id=sa;Password=MatKhauCuaBan;TrustServerCertificate=True;"
```

### Bước 3: Tạo database

**Cách 1 — Tự động (khuyến nghị):**

Chạy ứng dụng lần đầu — hệ thống tự tạo bảng và seed dữ liệu mẫu qua Entity Framework (`EnsureCreated`).

**Cách 2 — Script SQL:**

1. Mở SSMS, kết nối SQL Server
2. Chạy `Database/Script_TaoDB.sql` để tạo database
3. Chạy ứng dụng để EF tạo bảng và seed data

**Cách 3 — EF Migration (tùy chọn):**

```powershell
cd QLSanBongMini.Web
dotnet ef database update
```

### Bước 4: Chạy ứng dụng

**Visual Studio:**

1. Mở `QLSanBongMini.sln`
2. Đặt `QLSanBongMini.Web` làm Startup Project
3. Nhấn **F5** hoặc **Ctrl + F5**

**Dòng lệnh:**

```powershell
cd QLSanBongMini.Web
dotnet run
```

Truy cập URL hiển thị trên terminal (thường `https://localhost:7xxx`).

---

## Tài khoản demo mặc định

| Vai trò | Tên đăng nhập | Mật khẩu |
|---------|---------------|----------|
| Admin | `admin` | `Admin@123` |
| Nhân viên | `nhanvien` | `Nhanvien@123` |

> Đổi mật khẩu sau khi cài đặt trên môi trường thật.

---

## Triển khai VPS (đã cấu hình mẫu)

Script deploy: `deploy/deploy_vps.py` (đặt biến môi trường `VPS_PASSWORD` trước khi chạy).

```powershell
cd QLSanBongMini.Web
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish-linux
cd publish-linux; tar -czf ../qlsb-app.tar.gz .
cd ../../deploy
$env:VPS_PASSWORD='mat-khau-ssh'
python deploy_vps.py
```

**Demo online:** http://103.166.182.48/Account/Login

---

## Triển khai lên VPS (demo cho khách)

### Bước 1: Publish ứng dụng

Trên máy phát triển:

```powershell
cd QLSanBongMini.Web
dotnet publish -c Release -o ./publish
```

Copy thư mục `publish` lên VPS.

### Bước 2: Cài đặt trên VPS Windows

1. Cài **SQL Server Express** và tạo database `QLSanBongMini`
2. Chạy script `Database/Script_TaoDB.sql` và `SeedData.sql`
3. Cài **[.NET 8 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0)**
4. Cài và cấu hình **IIS**:
   - Tạo Application Pool (.NET CLR: **No Managed Code**)
   - Tạo Website trỏ tới thư mục `publish`
   - Gán quyền đọc cho IIS AppPool
5. Sửa `appsettings.Production.json` (hoặc biến môi trường) với connection string VPS

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=QLSanBongMini;User Id=qlsb;Password=MatKhauManh;TrustServerCertificate=True;"
}
```

6. Mở port **80** / **443** trên firewall VPS
7. (Khuyến nghị) Cấu hình HTTPS với Let's Encrypt hoặc chứng chỉ SSL

### Bước 3: Kiểm tra

- Truy cập `http://<IP-VPS>` hoặc tên miền
- Đăng nhập bằng tài khoản demo
- Kiểm tra đặt sân, lịch sân và dashboard

---

## Biến môi trường

| Biến | Mô tả |
|------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Development` (local) hoặc `Production` (VPS) |
| `ConnectionStrings__DefaultConnection` | Ghi đè connection string khi deploy |

Ví dụ trên IIS: thêm biến môi trường trong Application Pool hoặc `web.config`.

---

## Phát triển tiếp

```powershell
# Thêm migration sau khi đổi model
dotnet ef migrations add TenMigration --project QLSanBongMini.Web

# Cập nhật database
dotnet ef database update --project QLSanBongMini.Web

# Build release
dotnet build -c Release
```

---

## Xử lý lỗi thường gặp

| Lỗi | Cách xử lý |
|-----|------------|
| Không kết nối được SQL Server | Kiểm tra SQL Server đang chạy; bật TCP/IP trong SQL Server Configuration Manager |
| `Login failed for user` | Kiểm tra user/password trong connection string |
| Lỗi migration | Xóa DB và chạy lại `dotnet ef database update`, hoặc dùng script SQL |
| IIS 500.30 / 500.31 | Cài đúng .NET 8 Hosting Bundle; kiểm tra Application Pool |
| HTTPS certificate | Dùng `TrustServerCertificate=True` khi dev; cấu hình SSL thật trên VPS |

---

## Tác giả

- **Sinh viên:** _(điền họ tên, MSSV)_
- **Giảng viên hướng dẫn:** _(điền họ tên)_
- **Trường:** Đại học Trà Vinh — Khoa Kỹ thuật và Công nghệ

---

## Giấy phép

Dự án học tập — sử dụng cho mục đích đồ án và triển khai nội bộ.
