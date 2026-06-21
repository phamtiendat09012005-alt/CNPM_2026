# Danh sách nội dung đã sửa

## Frontend

- Viết lại toàn bộ trang Figma tĩnh thành React component hợp lệ.
- Loại bỏ inline style sai cú pháp và bố cục cố định 1440px.
- Thêm giao diện responsive bằng CSS thuần.
- Thêm Header, ProductCard, ProtectedRoute và AuthContext.
- Hoàn thiện các service gọi API.
- Hoàn thiện đăng ký, đăng nhập, danh sách/chi tiết sản phẩm, giỏ hàng, checkout, đơn hàng và Admin.
- Loại bỏ Staff, POS, Invoice, Inventory, Revenue và User Management ngoài phạm vi.
- Bảo vệ route Admin theo vai trò.
- `npm run lint` không có lỗi/cảnh báo.
- `npm run build` thành công.

## Backend

- Thay lưu mật khẩu dạng rõ bằng ASP.NET Core Identity.
- Thêm JWT authentication và role claim.
- Thêm phân quyền Customer/Admin.
- Loại bỏ connection string viết cứng trong DbContext.
- Sửa Product - ProductImage thành quan hệ một-nhiều.
- Không nhận UserId từ URL khi tạo đơn; lấy từ JWT.
- Backend tự tính giá và phí giao hàng.
- Thêm giao dịch Serializable khi tạo đơn để hạn chế bán vượt tồn.
- Thêm Cart API, Order API, Payment mô phỏng, Admin API và Recommendation API.
- Thêm xử lý hủy đơn và hoàn lại tồn kho.
- Thêm middleware xử lý lỗi, không trả lỗi SQL trực tiếp.
- Đổi mã đơn hàng để giảm nguy cơ trùng.
- Thêm Unit Test và Integration Test cơ bản.

## Database và phạm vi

- Giữ database SQL Server đã thiết kế.
- Không có POS/bán hàng tại quầy.
- Có Data Dictionary, ERD và 20 test case.
