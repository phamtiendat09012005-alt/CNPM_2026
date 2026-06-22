# Sports Store AI – Bản đã sửa

Dự án gồm:

- `Backend/SportsStoreAI.sln`: ASP.NET Core Web API 8.
- `Frontend/SportsStoreUI`: React + Vite.
- `Database/Database_SportsStoreAI.sql`: SQL Server script.
- `Backend/SportsStoreAI.Tests`: Unit Test và Integration Test cơ bản.

## Phạm vi chức năng

1. Tài khoản và phân quyền Customer/Admin.
2. Quản lý, tìm kiếm sản phẩm và biến thể.
3. Giỏ hàng, đặt hàng, thanh toán mô phỏng và xử lý đơn.
4. AI gợi ý sản phẩm và kích thước.

Đã loại bỏ POS và khu vực Staff.

## 1. Tạo database

Mở SQL Server Management Studio và chạy:

```text
Database/Database_SportsStoreAI.sql
```

Database được tạo với tên `SportsStoreAI`.

## 2. Cấu hình Backend

Mở:

```text
Backend/SportsStoreAI.API/appsettings.Development.json
```

Chuỗi kết nối mặc định:

```text
Server=localhost;Database=SportsStoreAI;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

SQL Server Express:

```text
Server=localhost\SQLEXPRESS;Database=SportsStoreAI;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Mở `Backend/SportsStoreAI.sln`, restore NuGet và chạy project `SportsStoreAI.API`.

Swagger:

```text
https://localhost:7188/swagger
```

Tài khoản Admin được seed khi Backend kết nối được database:

```text
Email: admin@sportsstoreai.local
Password: Admin@123456
```

## 3. Chạy Frontend

Mở Terminal:

```bash
cd Frontend/SportsStoreUI
npm install
npm run dev
```

Frontend:

```text
http://localhost:5173
```

Frontend mặc định gọi API:

```text
https://localhost:7188/api
```

Đổi API URL bằng cách tạo `.env`:

```text
VITE_API_URL=https://localhost:7188/api
```

## 4. Chạy kiểm thử Backend

```bash
cd Backend
dotnet test
```

## Lưu ý HTTPS

Lần đầu gọi API HTTPS từ React, hãy mở `https://localhost:7188/swagger` trên trình duyệt và chấp nhận chứng chỉ phát triển.

## Thanh toán online

Đồ án dùng `OnlineGateway` ở chế độ mô phỏng để kiểm thử luồng thanh toán, không kết nối cổng thanh toán thật.
