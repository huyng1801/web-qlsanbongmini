# Website Quản Lý Sân Bóng Mini

Dự án đồ án môn Lập trình Web với ASP.NET Core MVC.

## 1) Thành phần giữ lại để nộp

- Mã nguồn chính: `QLSanBongMini.Web/`
- Solution: `QLSanBongMini.sln`
- Script CSDL tham khảo: `Database/`
- Báo cáo theo mẫu chuẩn: `Docs/DoAn_QLSanBongMini_TheoMau_Chuan.docx`

## 2) Công nghệ sử dụng

- .NET 8 (ASP.NET Core MVC)
- C#
- Entity Framework Core
- SQL Server (production) / SQLite (local fallback)
- ASP.NET Core Identity
- Bootstrap 5

## 3) Yêu cầu môi trường chạy local

- Windows 10/11
- .NET 8 SDK
- SQL Server (khuyến nghị) **hoặc** không cần SQL Server nếu dùng SQLite
- Visual Studio 2022 (tùy chọn) hoặc CLI

## 4) Cấu hình cơ sở dữ liệu

File: `QLSanBongMini.Web/appsettings.json`

```json
"Database": {
  "Provider": "Auto"
},
"ConnectionStrings": {
  "SqlServer": "Server=.;Database=QLSanBongMini;Trusted_Connection=True;TrustServerCertificate=True;",
  "Sqlite": "Data Source=qlsanbongmini.db"
}
```

### Ý nghĩa `Database:Provider`

- `Auto`: Có SQL Server thì dùng SQL Server, không có thì tự động dùng SQLite.
- `SqlServer`: Bắt buộc SQL Server.
- `Sqlite`: Luôn dùng SQLite.

## 5) Cách chạy dự án trên máy local

### Cách 1: Visual Studio

1. Mở `QLSanBongMini.sln`
2. Đặt `QLSanBongMini.Web` làm Startup Project
3. Nhấn `Ctrl + F5`

### Cách 2: Dòng lệnh (CLI)

```powershell
cd QLSanBongMini.Web
dotnet restore
dotnet run
```

Sau khi chạy, mở URL hiển thị trên terminal (thường là `http://localhost:xxxx` hoặc `https://localhost:xxxx`).

## 6) Tài khoản demo mặc định

- Admin: `admin` / `Admin@123`
- Nhân viên: `nhanvien` / `Nhanvien@123`

> Khuyến nghị đổi mật khẩu nếu demo trên môi trường thật.

## 7) Tạo/Cài đặt dữ liệu

Ứng dụng sẽ tự seed dữ liệu mẫu khi chạy lần đầu (`EnsureCreated` + `DbInitializer`).

Nếu cần script:

- `Database/Script_TaoDB.sql`
- `Database/SeedData.sql`

## 8) Cấu trúc thư mục chính

```text
web-qlsanbongmini/
|-- QLSanBongMini.sln
|-- QLSanBongMini.Web/
|   |-- Controllers/
|   |-- Data/
|   |-- Models/
|   |-- Services/
|   |-- Views/
|   `-- wwwroot/
|-- Database/
|-- Docs/
|   `-- DoAn_QLSanBongMini_TheoMau_Chuan.docx
`-- README.md
```

## 9) Lỗi thường gặp

- Không mở được trang đăng nhập:
  - Kiểm tra ứng dụng đã chạy thành công chưa.
- Lỗi kết nối SQL Server:
  - Chuyển `Database:Provider` sang `Sqlite` để chạy nhanh local.
- Port đã bị chiếm:
  - Dừng ứng dụng đang dùng port đó, hoặc chạy lại `dotnet run` với port khác.

---

Khi đóng gói để nộp: chỉ cần zip mã nguồn + `Docs/DoAn_QLSanBongMini_TheoMau_Chuan.docx`.
