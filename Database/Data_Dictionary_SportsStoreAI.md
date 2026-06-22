# DATA DICTIONARY – SPORTS STORE AI

## 1. Nhóm tài khoản và phân quyền

### Users

| Trường | Kiểu dữ liệu | Null | Khóa/Ràng buộc | Mô tả |
|---|---|---:|---|---|
| Id | int identity | Không | PK | Mã người dùng |
| FullName | nvarchar(120) | Không |  | Họ tên |
| Status | nvarchar(20) | Không | CHECK | Active, Locked hoặc Inactive |
| CreatedAt | datetime2 | Không | DEFAULT | Thời điểm tạo |
| UpdatedAt | datetime2 | Có |  | Thời điểm cập nhật |
| UserName | nvarchar(256) | Có |  | Tên đăng nhập Identity |
| NormalizedUserName | nvarchar(256) | Có | UNIQUE INDEX | Tên đăng nhập chuẩn hóa |
| Email | nvarchar(256) | Có |  | Email |
| NormalizedEmail | nvarchar(256) | Có | INDEX | Email chuẩn hóa |
| EmailConfirmed | bit | Không | DEFAULT 0 | Xác nhận email |
| PasswordHash | nvarchar(max) | Có |  | Mật khẩu đã băm |
| SecurityStamp | nvarchar(max) | Có |  | Mã bảo mật Identity |
| ConcurrencyStamp | nvarchar(max) | Có |  | Kiểm soát đồng thời |
| PhoneNumber | nvarchar(30) | Có |  | Số điện thoại |
| PhoneNumberConfirmed | bit | Không | DEFAULT 0 | Xác nhận số điện thoại |
| TwoFactorEnabled | bit | Không | DEFAULT 0 | Xác thực hai lớp |
| LockoutEnd | datetimeoffset | Có |  | Thời điểm hết khóa |
| LockoutEnabled | bit | Không | DEFAULT 1 | Cho phép khóa |
| AccessFailedCount | int | Không | CHECK >= 0 | Số lần đăng nhập sai |

### Roles

| Trường | Kiểu | Null | Khóa | Mô tả |
|---|---|---:|---|---|
| Id | int identity | Không | PK | Mã vai trò |
| Name | nvarchar(256) | Có |  | Customer hoặc Admin |
| NormalizedName | nvarchar(256) | Có | UNIQUE INDEX | Tên chuẩn hóa |
| ConcurrencyStamp | nvarchar(max) | Có |  | Kiểm soát đồng thời |

### UserRoles

| Trường | Kiểu | Null | Khóa | Mô tả |
|---|---|---:|---|---|
| UserId | int | Không | PK, FK | Người dùng |
| RoleId | int | Không | PK, FK | Vai trò |

### UserAddresses

| Trường | Kiểu | Null | Khóa | Mô tả |
|---|---|---:|---|---|
| Id | int identity | Không | PK | Mã địa chỉ |
| UserId | int | Không | FK | Chủ sở hữu |
| ReceiverName | nvarchar(120) | Không |  | Tên người nhận |
| ReceiverPhone | nvarchar(20) | Không |  | Điện thoại người nhận |
| AddressLine | nvarchar(300) | Không |  | Địa chỉ chi tiết |
| Ward | nvarchar(100) | Không |  | Phường/xã |
| District | nvarchar(100) | Không |  | Quận/huyện |
| Province | nvarchar(100) | Không |  | Tỉnh/thành phố |
| IsDefault | bit | Không | DEFAULT 0 | Địa chỉ mặc định |
| CreatedAt | datetime2 | Không | DEFAULT | Ngày tạo |
| UpdatedAt | datetime2 | Có |  | Ngày cập nhật |

Các bảng `UserClaims`, `RoleClaims`, `UserLogins`, `UserTokens` phục vụ ASP.NET Core Identity.

---

## 2. Nhóm sản phẩm và biến thể

### Categories

| Trường | Kiểu | Null | Khóa/Ràng buộc | Mô tả |
|---|---|---:|---|---|
| Id | int identity | Không | PK | Mã danh mục |
| Name | nvarchar(120) | Không | UNIQUE | Tên danh mục |
| Description | nvarchar(500) | Có |  | Mô tả |
| IsActive | bit | Không | DEFAULT 1 | Trạng thái |
| CreatedAt | datetime2 | Không | DEFAULT | Ngày tạo |
| UpdatedAt | datetime2 | Có |  | Ngày cập nhật |

### Brands

| Trường | Kiểu | Null | Khóa/Ràng buộc | Mô tả |
|---|---|---:|---|---|
| Id | int identity | Không | PK | Mã thương hiệu |
| Name | nvarchar(100) | Không | UNIQUE | Tên thương hiệu |
| LogoUrl | nvarchar(500) | Có |  | Logo |
| IsActive | bit | Không | DEFAULT 1 | Trạng thái |
| CreatedAt | datetime2 | Không | DEFAULT | Ngày tạo |
| UpdatedAt | datetime2 | Có |  | Ngày cập nhật |

### Products

| Trường | Kiểu | Null | Khóa/Ràng buộc | Mô tả |
|---|---|---:|---|---|
| Id | int identity | Không | PK | Mã sản phẩm |
| CategoryId | int | Không | FK | Danh mục |
| BrandId | int | Có | FK | Thương hiệu |
| Name | nvarchar(180) | Không | INDEX | Tên sản phẩm |
| Slug | nvarchar(220) | Không | UNIQUE | Đường dẫn thân thiện |
| Description | nvarchar(max) | Có |  | Mô tả |
| BasePrice | decimal(18,2) | Không | CHECK >= 0 | Giá cơ bản |
| SportType | nvarchar(80) | Có |  | Running, Football... |
| ProductType | nvarchar(30) | Không | CHECK | Shoes, Clothing, Equipment, Accessory |
| IsActive | bit | Không | DEFAULT 1 | Hiển thị sản phẩm |
| CreatedAt | datetime2 | Không | DEFAULT | Ngày tạo |
| UpdatedAt | datetime2 | Có |  | Ngày cập nhật |

### ProductImages

| Trường | Kiểu | Null | Khóa/Ràng buộc | Mô tả |
|---|---|---:|---|---|
| Id | int identity | Không | PK | Mã ảnh |
| ProductId | int | Không | FK | Sản phẩm |
| Url | nvarchar(500) | Không |  | Đường dẫn ảnh |
| IsPrimary | bit | Không | UNIQUE FILTERED | Ảnh đại diện |
| SortOrder | int | Không | CHECK >= 0 | Thứ tự hiển thị |

### ProductVariants

| Trường | Kiểu | Null | Khóa/Ràng buộc | Mô tả |
|---|---|---:|---|---|
| Id | int identity | Không | PK | Mã biến thể |
| ProductId | int | Không | FK | Sản phẩm |
| SKU | nvarchar(60) | Không | UNIQUE | Mã hàng |
| Size | nvarchar(30) | Có | INDEX | Kích thước |
| Color | nvarchar(50) | Có | INDEX | Màu sắc |
| Price | decimal(18,2) | Không | CHECK >= 0 | Giá biến thể |
| StockQuantity | int | Không | CHECK >= 0 | Số lượng tồn |
| LowStockThreshold | int | Không | CHECK >= 0 | Ngưỡng sắp hết |
| IsActive | bit | Không | DEFAULT 1 | Trạng thái |
| CreatedAt | datetime2 | Không | DEFAULT | Ngày tạo |
| UpdatedAt | datetime2 | Có |  | Ngày cập nhật |

---

## 3. Nhóm giỏ hàng

### Carts

| Trường | Kiểu | Null | Khóa/Ràng buộc | Mô tả |
|---|---|---:|---|---|
| Id | int identity | Không | PK | Mã giỏ |
| UserId | int | Không | FK, UNIQUE | Mỗi người dùng một giỏ |
| CreatedAt | datetime2 | Không | DEFAULT | Ngày tạo |
| UpdatedAt | datetime2 | Không | DEFAULT | Ngày cập nhật |

### CartItems

| Trường | Kiểu | Null | Khóa/Ràng buộc | Mô tả |
|---|---|---:|---|---|
| Id | int identity | Không | PK | Mã dòng giỏ |
| CartId | int | Không | FK | Giỏ hàng |
| ProductVariantId | int | Không | FK | Biến thể |
| Quantity | int | Không | CHECK > 0 | Số lượng |
| IsSelected | bit | Không | DEFAULT 1 | Có được chọn thanh toán |
| AddedAt | datetime2 | Không | DEFAULT | Ngày thêm |
| UpdatedAt | datetime2 | Có |  | Ngày cập nhật |

Ràng buộc duy nhất `(CartId, ProductVariantId)` giúp một biến thể chỉ xuất hiện một lần trong giỏ.

---

## 4. Nhóm đơn hàng và thanh toán

### Orders

| Trường | Kiểu | Null | Khóa/Ràng buộc | Mô tả |
|---|---|---:|---|---|
| Id | bigint identity | Không | PK | Mã nội bộ |
| UserId | int | Không | FK | Khách hàng |
| OrderCode | nvarchar(30) | Không | UNIQUE | Mã đơn hiển thị |
| ReceiverName | nvarchar(120) | Không |  | Người nhận |
| ReceiverPhone | nvarchar(20) | Không |  | Điện thoại |
| ShippingAddress | nvarchar(500) | Không |  | Địa chỉ giao |
| Subtotal | decimal(18,2) | Không | CHECK >= 0 | Tiền hàng |
| ShippingFee | decimal(18,2) | Không | CHECK >= 0 | Phí giao hàng |
| TotalAmount | decimal(18,2) | Không | CHECK | Tổng cộng |
| Status | nvarchar(30) | Không | CHECK | Trạng thái đơn |
| Note | nvarchar(500) | Có |  | Ghi chú |
| CreatedAt | datetime2 | Không | DEFAULT | Ngày đặt |
| UpdatedAt | datetime2 | Có |  | Ngày cập nhật |
| CancelledAt | datetime2 | Có |  | Ngày hủy |

Trạng thái: `AwaitingPayment`, `Pending`, `Confirmed`, `Processing`, `Shipping`, `Completed`, `Cancelled`.

### OrderItems

| Trường | Kiểu | Null | Khóa/Ràng buộc | Mô tả |
|---|---|---:|---|---|
| Id | bigint identity | Không | PK | Mã chi tiết |
| OrderId | bigint | Không | FK | Đơn hàng |
| ProductVariantId | int | Không | FK | Biến thể gốc |
| ProductName | nvarchar(180) | Không | Snapshot | Tên khi mua |
| SKU | nvarchar(60) | Không | Snapshot | SKU khi mua |
| Size | nvarchar(30) | Có | Snapshot | Size khi mua |
| Color | nvarchar(50) | Có | Snapshot | Màu khi mua |
| UnitPrice | decimal(18,2) | Không | CHECK >= 0 | Đơn giá |
| Quantity | int | Không | CHECK > 0 | Số lượng |
| TotalPrice | computed | Không | UnitPrice × Quantity | Thành tiền |

### Payments

| Trường | Kiểu | Null | Khóa/Ràng buộc | Mô tả |
|---|---|---:|---|---|
| Id | bigint identity | Không | PK | Mã thanh toán |
| OrderId | bigint | Không | FK | Đơn hàng |
| Method | nvarchar(30) | Không | CHECK | COD hoặc OnlineGateway |
| Status | nvarchar(20) | Không | CHECK | Unpaid, Pending, Paid, Failed, Refunded |
| Amount | decimal(18,2) | Không | CHECK >= 0 | Số tiền |
| Provider | nvarchar(60) | Có |  | VNPay/MoMo... |
| ProviderTransactionId | nvarchar(120) | Có | UNIQUE FILTERED | Mã giao dịch |
| PaymentUrl | nvarchar(1000) | Có |  | URL thanh toán |
| FailureReason | nvarchar(500) | Có |  | Lý do lỗi |
| PaidAt | datetime2 | Có |  | Ngày thanh toán |
| CreatedAt | datetime2 | Không | DEFAULT | Ngày tạo |
| UpdatedAt | datetime2 | Có |  | Ngày cập nhật |

### OrderStatusHistories

| Trường | Kiểu | Null | Khóa/Ràng buộc | Mô tả |
|---|---|---:|---|---|
| Id | bigint identity | Không | PK | Mã lịch sử |
| OrderId | bigint | Không | FK | Đơn hàng |
| Status | nvarchar(30) | Không | CHECK | Trạng thái mới |
| Note | nvarchar(500) | Có |  | Ghi chú |
| ChangedByUserId | int | Có | FK | Người thay đổi |
| ChangedAt | datetime2 | Không | DEFAULT | Thời điểm thay đổi |

---

## 5. Nhóm AI

### ProductViewHistories

| Trường | Kiểu | Null | Khóa/Ràng buộc | Mô tả |
|---|---|---:|---|---|
| Id | bigint identity | Không | PK | Mã lượt xem |
| UserId | int | Có | FK | Người dùng đã đăng nhập |
| ProductId | int | Không | FK | Sản phẩm |
| SessionId | nvarchar(100) | Có | INDEX | Phiên khách vãng lai |
| ViewedAt | datetime2 | Không | DEFAULT | Thời điểm xem |

Phải có ít nhất `UserId` hoặc `SessionId`.

### SizeCharts

| Trường | Kiểu | Null | Khóa/Ràng buộc | Mô tả |
|---|---|---:|---|---|
| Id | int identity | Không | PK | Mã bảng size |
| BrandId | int | Có | FK | Thương hiệu; null là mặc định |
| ProductType | nvarchar(30) | Không | CHECK | Shoes hoặc Clothing |
| SizeLabel | nvarchar(30) | Không |  | 40, 41, S, M, L... |
| MinFootLengthCm | decimal(5,2) | Có | CHECK RANGE | Chiều dài chân nhỏ nhất |
| MaxFootLengthCm | decimal(5,2) | Có | CHECK RANGE | Chiều dài chân lớn nhất |
| MinHeightCm | decimal(5,2) | Có | CHECK RANGE | Chiều cao nhỏ nhất |
| MaxHeightCm | decimal(5,2) | Có | CHECK RANGE | Chiều cao lớn nhất |
| MinWeightKg | decimal(5,2) | Có | CHECK RANGE | Cân nặng nhỏ nhất |
| MaxWeightKg | decimal(5,2) | Có | CHECK RANGE | Cân nặng lớn nhất |
| MinChestCm | decimal(5,2) | Có | CHECK RANGE | Vòng ngực nhỏ nhất |
| MaxChestCm | decimal(5,2) | Có | CHECK RANGE | Vòng ngực lớn nhất |
| MinWaistCm | decimal(5,2) | Có | CHECK RANGE | Vòng eo nhỏ nhất |
| MaxWaistCm | decimal(5,2) | Có | CHECK RANGE | Vòng eo lớn nhất |
| FitNote | nvarchar(300) | Có |  | Ghi chú form |
| IsActive | bit | Không | DEFAULT 1 | Trạng thái |
| CreatedAt | datetime2 | Không | DEFAULT | Ngày tạo |
| UpdatedAt | datetime2 | Có |  | Ngày cập nhật |

### RecommendationLogs

| Trường | Kiểu | Null | Khóa/Ràng buộc | Mô tả |
|---|---|---:|---|---|
| Id | bigint identity | Không | PK | Mã log |
| UserId | int | Có | FK | Người dùng |
| SessionId | nvarchar(100) | Có | INDEX | Phiên truy cập |
| RecommendationType | nvarchar(30) | Không | CHECK | Related, Personalized, Popular hoặc Size |
| SourceProductId | int | Có | FK | Sản phẩm nguồn |
| RecommendedProductId | int | Có | FK | Sản phẩm được gợi ý |
| RecommendedSize | nvarchar(30) | Có |  | Kích thước gợi ý |
| Score | decimal(6,5) | Không | CHECK 0–1 | Điểm phù hợp |
| Reason | nvarchar(500) | Không |  | Lý do gợi ý |
| IsClicked | bit | Không | DEFAULT 0 | Có nhấn vào gợi ý |
| IsPurchased | bit | Không | DEFAULT 0 | Có mua sau gợi ý |
| CreatedAt | datetime2 | Không | DEFAULT | Ngày tạo |

---

## 6. View

### vw_ProductCatalog

Tổng hợp sản phẩm, danh mục, thương hiệu, giá nhỏ nhất/lớn nhất, tổng tồn và ảnh đại diện.

### vw_OrderSummary

Tổng hợp đơn hàng, người mua, trạng thái đơn và trạng thái thanh toán mới nhất.
