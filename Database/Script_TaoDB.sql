-- Script tạo database QLSanBongMini
-- Chạy trên SQL Server Management Studio (SSMS)
-- Lưu ý: Nếu chạy ứng dụng lần đầu, EF Core có thể tự tạo bảng qua EnsureCreated.
-- Script này dùng khi muốn tạo database thủ công trước.

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'QLSanBongMini')
BEGIN
    CREATE DATABASE QLSanBongMini;
END
GO

USE QLSanBongMini;
GO

-- Sau khi tạo database, chạy ứng dụng ASP.NET Core một lần để EF tạo bảng và seed dữ liệu.
-- Hoặc dùng lệnh (khi đã cài dotnet-ef tương thích):
--   cd QLSanBongMini.Web
--   dotnet ef database update

PRINT N'Database QLSanBongMini đã sẵn sàng. Hãy chạy ứng dụng để khởi tạo bảng và dữ liệu mẫu.';
