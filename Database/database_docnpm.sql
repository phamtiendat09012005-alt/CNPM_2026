SET NOCOUNT ON;

DELETE FROM Payments;
DELETE FROM OrderStatusHistories;
DELETE FROM OrderItems;
DELETE FROM Orders;
DELETE FROM CartItems;
DELETE FROM Carts;
DELETE FROM ProductViewHistories;
DELETE FROM RecommendationLogs;
DELETE FROM ProductVariants;
DELETE FROM ProductImages;
DELETE FROM Products;
DELETE FROM SizeCharts;
DELETE FROM Brands;
DELETE FROM Categories;

DBCC CHECKIDENT ('ProductVariants', RESEED, 0);
DBCC CHECKIDENT ('ProductImages', RESEED, 0);
DBCC CHECKIDENT ('Products', RESEED, 0);
DBCC CHECKIDENT ('SizeCharts', RESEED, 0);
DBCC CHECKIDENT ('Brands', RESEED, 0);
DBCC CHECKIDENT ('Categories', RESEED, 0);

SET IDENTITY_INSERT Categories ON;
INSERT INTO Categories (Id, Name, Description, IsActive, CreatedAt) VALUES
(1, N'Giày Chạy Bộ', N'Giày chạy bộ chuyên nghiệp, siêu nhẹ, đệm êm', 1, GETUTCDATE()),
(2, N'Giày Bóng Đá', N'Giày đinh sân cỏ nhân tạo và tự nhiên', 1, GETUTCDATE()),
(3, N'Giày Bóng Rổ', N'Giày bóng rổ cổ cao bảo vệ mắt cá chân', 1, GETUTCDATE()),
(4, N'Áo Thể Thao Nam', N'Áo thun nam co giãn, thoáng khí, thấm hút', 1, GETUTCDATE()),
(5, N'Quần Thể Thao Nam', N'Quần short, quần jogger thể thao nam', 1, GETUTCDATE()),
(6, N'Áo Thể Thao Nữ', N'Áo croptop, bra, tanktop thể thao nữ', 1, GETUTCDATE()),
(7, N'Đồ Tập Yoga Nữ', N'Quần legging, đồ tập Yoga co giãn đa chiều', 1, GETUTCDATE()),
(8, N'Vợt Cầu Lông', N'Vợt cầu lông carbon công thủ toàn diện', 1, GETUTCDATE()),
(9, N'Bóng Thể Thao', N'Quả bóng đá, bóng rổ thi đấu tiêu chuẩn', 1, GETUTCDATE()),
(10, N'Balo & Túi Thể Thao', N'Balo, túi trống đựng đồ thể thao tiện dụng', 1, GETUTCDATE());
SET IDENTITY_INSERT Categories OFF;

SET IDENTITY_INSERT Brands ON;
INSERT INTO Brands (Id, Name, LogoUrl, IsActive, CreatedAt) VALUES
(1, N'Nike', 'https://upload.wikimedia.org/wikipedia/commons/a/a6/Logo_NIKE.svg', 1, GETUTCDATE()),
(2, N'Adidas', 'https://upload.wikimedia.org/wikipedia/commons/2/20/Adidas_Logo.svg', 1, GETUTCDATE()),
(3, N'Puma', 'https://upload.wikimedia.org/wikipedia/commons/8/88/Puma_Logo.svg', 1, GETUTCDATE()),
(4, N'Under Armour', 'https://upload.wikimedia.org/wikipedia/commons/4/44/Under_armour_logo.svg', 1, GETUTCDATE()),
(5, N'Yonex', 'https://upload.wikimedia.org/wikipedia/commons/2/21/Yonex_logo.svg', 1, GETUTCDATE()),
(6, N'Asics', 'https://upload.wikimedia.org/wikipedia/commons/b/b1/Asics_Logo.svg', 1, GETUTCDATE()),
(7, N'Mizuno', 'https://upload.wikimedia.org/wikipedia/commons/d/df/Mizuno_logo.svg', 1, GETUTCDATE()),
(8, N'Lining', 'https://upload.wikimedia.org/wikipedia/commons/8/8d/Li-Ning_logo.svg', 1, GETUTCDATE());
SET IDENTITY_INSERT Brands OFF;

INSERT INTO SizeCharts (ProductType, BrandId, SizeLabel, MinFootLengthCm, MaxFootLengthCm, IsActive, CreatedAt) VALUES
('Shoes', NULL, '39', 24.1, 24.5, 1, GETUTCDATE()),
('Shoes', NULL, '40', 24.6, 25.0, 1, GETUTCDATE()),
('Shoes', NULL, '41', 25.1, 25.5, 1, GETUTCDATE()),
('Shoes', NULL, '42', 25.6, 26.0, 1, GETUTCDATE()),
('Shoes', NULL, '43', 26.1, 26.5, 1, GETUTCDATE());

INSERT INTO SizeCharts (ProductType, BrandId, SizeLabel, MinHeightCm, MaxHeightCm, MinWeightKg, MaxWeightKg, MinChestCm, MaxChestCm, MinWaistCm, MaxWaistCm, IsActive, CreatedAt) VALUES
('Clothing', NULL, 'S', 150, 160, 45, 55, 80, 86, 68, 74, 1, GETUTCDATE()),
('Clothing', NULL, 'M', 161, 170, 56, 65, 87, 92, 75, 80, 1, GETUTCDATE()),
('Clothing', NULL, 'L', 171, 178, 66, 75, 93, 98, 81, 86, 1, GETUTCDATE()),
('Clothing', NULL, 'XL', 179, 185, 76, 85, 99, 104, 87, 92, 1, GETUTCDATE()),
('Clothing', NULL, 'XXL', 186, 192, 86, 95, 105, 110, 93, 98, 1, GETUTCDATE());

SET IDENTITY_INSERT Products ON;
INSERT INTO Products (Id, CategoryId, BrandId, Name, Slug, Description, BasePrice, SportType, ProductType, IsActive, CreatedAt) VALUES
(1, 1, 1, N'Nike Air Zoom Pegasus 40', 'nike-pegasus-40', N'Thiết kế lưới thoáng khí tối ưu cho cự ly trung bình.', 3200000, N'Chạy bộ', 'Shoes', 1, GETUTCDATE()),
(2, 1, 2, N'Adidas Ultraboost Light', 'adidas-ultraboost', N'Công nghệ Boost siêu nhẹ hoàn trả năng lượng vượt trội.', 4500000, N'Chạy bộ', 'Shoes', 1, GETUTCDATE()),
(3, 1, 6, N'Asics Gel-Kayano 30', 'asics-gel-kayano-30', N'Hỗ trợ chống lật cổ chân tuyệt đối cho người chạy.', 3900000, N'Chạy bộ', 'Shoes', 1, GETUTCDATE()),
(4, 1, 3, N'Puma Velocity Nitro 2', 'puma-velocity-nitro', N'Lớp đệm Nitro Foam bám đường cực tốt.', 2800000, N'Chạy bộ', 'Shoes', 1, GETUTCDATE()),
(5, 2, 1, N'Nike Mercurial Vapor 15', 'nike-mercurial-vapor', N'Giày đá bóng sân cỏ nhân tạo tốc độ cao.', 2600000, N'Bóng đá', 'Shoes', 1, GETUTCDATE()),
(6, 2, 2, N'Adidas Predator Accuracy', 'adidas-predator-accuracy', N'Tối ưu khả năng kiểm soát và chuyền bóng chính xác.', 2700000, N'Bóng đá', 'Shoes', 1, GETUTCDATE()),
(7, 2, 7, N'Mizuno Morelia Neo III', 'mizuno-morelia-neo', N'Da Kangaroo tự nhiên ôm chân hoàn hảo.', 3500000, N'Bóng đá', 'Shoes', 1, GETUTCDATE()),
(8, 2, 3, N'Puma Future Ultimate TF', 'puma-future-ultimate', N'Dải thun nén linh hoạt theo mọi chuyển động.', 2900000, N'Bóng đá', 'Shoes', 1, GETUTCDATE()),
(9, 3, 1, N'Nike LeBron 21 Premium', 'nike-lebron-21', N'Bộ đệm air tối ưu lực bật nhảy.', 4200000, N'Bóng rổ', 'Shoes', 1, GETUTCDATE()),
(10, 3, 4, N'Under Armour Curry 11', 'ua-curry-11', N'Đế cao su chuyên dụng bám sân cực tốt.', 3800000, N'Bóng rổ', 'Shoes', 1, GETUTCDATE()),
(11, 3, 2, N'Adidas Trae Young 3', 'adidas-trae-young-3', N'Giày cổ thấp linh hoạt dành cho các hậu vệ.', 3100000, N'Bóng rổ', 'Shoes', 1, GETUTCDATE()),
(12, 3, 8, N'Lining Way of Wade 10', 'lining-way-of-wade-10', N'Thiết kế độc đáo cùng trọng lượng siêu nhẹ.', 4500000, N'Bóng rổ', 'Shoes', 1, GETUTCDATE()),
(13, 4, 1, N'Áo Thun Nam Nike Dri-FIT', 'ao-nike-dri-fit', N'Công nghệ tản nhiệt Dri-FIT giúp cơ thể luôn khô thoáng.', 750000, N'Tập luyện', 'Clothing', 1, GETUTCDATE()),
(14, 4, 2, N'Áo Polo Nam Adidas AeroReady', 'ao-polo-adidas', N'Chất vải sang trọng, phù hợp tập luyện và dạo phố.', 950000, N'Tập luyện', 'Clothing', 1, GETUTCDATE()),
(15, 4, 4, N'Áo Thể Thao UA Tech 2.0', 'ao-ua-tech-2', N'Rộng rãi, chất vải mát lạnh chống mùi hôi.', 650000, N'Tập luyện', 'Clothing', 1, GETUTCDATE()),
(16, 4, 3, N'Áo Thun Puma Performance', 'ao-puma-performance', N'Có dải phản quang an toàn khi chạy bộ ban đêm.', 600000, N'Tập luyện', 'Clothing', 1, GETUTCDATE()),
(17, 5, 1, N'Quần Short Nam Nike Challenger', 'quan-short-nike-challenger', N'Quần short dài 7 inch kèm lớp quần lót lưới bên trong.', 850000, N'Chạy bộ', 'Clothing', 1, GETUTCDATE()),
(18, 5, 2, N'Quần Jogger Adidas Tiro 23', 'quan-jogger-adidas-tiro', N'Dáng ôm sát, có ống khóa kéo tiện lợi thay đồ.', 1200000, N'Tập luyện', 'Clothing', 1, GETUTCDATE()),
(19, 5, 4, N'Quần Tập Nam UA Woven', 'quan-ua-woven', N'Chất liệu vải dệt siêu nhẹ không cản trở chuyển động.', 950000, N'Gym', 'Clothing', 1, GETUTCDATE()),
(20, 5, 3, N'Quần Short Puma Essentials', 'quan-short-puma', N'Chất vải cotton pha mềm mại cho ngày dài năng động.', 550000, N'Tập luyện', 'Clothing', 1, GETUTCDATE()),
(21, 6, 1, N'Áo Bra Thể Thao Nike Swoosh', 'bra-nike-swoosh', N'Nâng đỡ mức độ vừa phải, dây chéo lưng cá tính.', 850000, N'Gym', 'Clothing', 1, GETUTCDATE()),
(22, 6, 2, N'Áo Tanktop Nữ Adidas Train', 'tanktop-adidas-train', N'Thiết kế khoét nách rộng mát mẻ, dễ dàng phối đồ.', 650000, N'Gym', 'Clothing', 1, GETUTCDATE()),
(23, 6, 4, N'Áo Thun Nữ UA HeatGear', 'ao-nu-ua-heatgear', N'Ôm sát cơ thể, đẩy mồ hôi ra ngoài bề mặt cực nhanh.', 700000, N'Gym', 'Clothing', 1, GETUTCDATE()),
(24, 6, 3, N'Áo Croptop Puma Studio', 'croptop-puma-studio', N'Dáng ngắn năng động, bo chun ôm sát cơ thể tôn dáng.', 600000, N'Gym', 'Clothing', 1, GETUTCDATE()),
(25, 7, 1, N'Quần Legging Nike Yoga Luxe', 'legging-nike-yoga', N'Chất vải Infinalon siêu mềm mượt như làn da thứ hai.', 1800000, N'Yoga', 'Clothing', 1, GETUTCDATE()),
(26, 7, 2, N'Quần Yoga Adidas Studio', 'quan-yoga-adidas', N'Cạp quần cao gen bụng, định hình vóc dáng thon gọn.', 1500000, N'Yoga', 'Clothing', 1, GETUTCDATE()),
(27, 7, 4, N'Áo Bra Yoga UA Seamless', 'bra-yoga-ua-seamless', N'Cấu trúc không đường may triệt tiêu hoàn toàn ma sát.', 900000, N'Yoga', 'Clothing', 1, GETUTCDATE()),
(28, 7, 3, N'Bộ Đồ Tập Yoga Puma FormKnit', 'bo-tap-yoga-puma', N'Đồng bộ áo quần vải thun gân co giãn 4 chiều.', 2100000, N'Yoga', 'Clothing', 1, GETUTCDATE()),
(29, 8, 5, N'Vợt Cầu Lông Yonex Astrox 99 Pro', 'vot-yonex-astrox-99', N'Dòng vợt nặng đầu chuyên công mạnh mẽ, đập cầu uy lực.', 4500000, N'Cầu lông', 'Equipment', 1, GETUTCDATE()),
(30, 8, 8, N'Vợt Cầu Lông Lining Aeronaut 9000', 'vot-lining-aeronaut', N'Cấu trúc rãnh thoát khí vành vợt giảm tối đa lực cản.', 4200000, N'Cầu lông', 'Equipment', 1, GETUTCDATE()),
(31, 8, 5, N'Vợt Cầu Lông Yonex Nanoflare 1000Z', 'vot-yonex-nanoflare', N'Phản tạt tốc độ cực cao, khung vợt khí động học siêu bén.', 4800000, N'Cầu lông', 'Equipment', 1, GETUTCDATE()),
(32, 8, 8, N'Vợt Cầu Lông Lining Halbertec 8000', 'vot-lining-halbertec', N'Thiết kế thân đũa cân bằng công thủ toàn diện.', 3900000, N'Cầu lông', 'Equipment', 1, GETUTCDATE()),
(33, 9, 1, N'Quả Bóng Rổ Nike Elite Championship', 'bong-ro-nike-elite', N'Chất liệu da tổng hợp có chiều sâu bám tay cực tốt.', 1200000, N'Bóng rổ', 'Equipment', 1, GETUTCDATE()),
(34, 9, 2, N'Quả Bóng Đá Adidas Oceaunz', 'bong-da-adidas-oceaunz', N'Bóng thi đấu chính thức ép nhiệt không đường may.', 2500000, N'Bóng đá', 'Equipment', 1, GETUTCDATE()),
(35, 9, 3, N'Quả Bóng Đá Puma Orbita', 'bong-da-puma-orbita', N'Độ nảy chuẩn xác, giữ hơi tốt suốt quá trình thi đấu.', 950000, N'Bóng đá', 'Equipment', 1, GETUTCDATE()),
(36, 9, 4, N'Quả Bóng Rổ UA 495', 'bong-ro-ua-495', N'Bóng rổ đa năng chơi tốt ở cả sân gạch và sân gỗ.', 850000, N'Bóng rổ', 'Equipment', 1, GETUTCDATE()),
(37, 10, 1, N'Balo Thể Thao Nike Brasilia', 'balo-nike-brasilia', N'Nhiều ngăn khóa kéo đa năng, có ngăn đựng giày riêng.', 1100000, N'Tập luyện', 'Accessory', 1, GETUTCDATE()),
(38, 10, 2, N'Túi Trống Adidas Essentials', 'tui-trong-adidas', N'Quai đeo vai êm ái, ngăn chứa rộng rãi đi gym cực tiện.', 950000, N'Gym', 'Accessory', 1, GETUTCDATE()),
(39, 10, 4, N'Balo Under Armour Hustle 5.0', 'balo-ua-hustle', N'Tích hợp ngăn đựng laptop chống sốc chống thấm nước.', 1400000, N'Tập luyện', 'Accessory', 1, GETUTCDATE()),
(40, 10, 3, N'Túi Đeo Chéo Puma Phase', 'tui-cheo-puma', N'Kích thước nhỏ gọn lý tưởng đựng điện thoại, ví tiền.', 450000, N'Thể thao', 'Accessory', 1, GETUTCDATE());
SET IDENTITY_INSERT Products OFF;

INSERT INTO ProductImages (ProductId, Url, IsPrimary, SortOrder) VALUES
(1, 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcToOonO3nXZWOH-xMyAz0dcdzQc4AgomcuJzg&s', 1, 0),
(2, 'https://bizweb.dktcdn.net/100/340/361/products/ultrarun5runningshoesblueie879-bb73a166-f509-4760-b41f-75de0c49d45a.jpg?v=1742786279653', 1, 0),
(3, 'https://product.hstatic.net/200000456065/product/a2_3c50178d63554c68b46f1c337b2d4b1e.png', 1, 0),
(4, 'https://bizweb.dktcdn.net/100/531/710/products/img-5369-png.jpg?v=1734716624030', 1, 0),
(5, 'https://sumstore.vn/wp-content/uploads/2023/06/xspeed-ii-ngoc.jpg', 1, 0),
(6, 'https://www.sport9.vn/images/uploaded/102367710_677737919735625_7246313792324099474_n.jpg', 1, 0),
(7, 'https://bizweb.dktcdn.net/100/301/479/products/svhms-me-woran38.png?v=1597219302113', 1, 0),
(8, 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTmJHfs5BfRbHRlH_NuWO0s7DvKlMM5LW1pig&s', 1, 0),
(9, 'https://stepback.vn/wp-content/uploads/2024/04/giay-bong-ro-stepback-attack-ace.jpg', 1, 0),
(10, 'https://cdn.hstatic.net/products/1000312752/e11005f64d7e58d3e7e8da0dbad9a56580b7335f47c14323558e9eb38624078f777ed3_9071c19246f64a9597635bf88a6823ce.jpg', 1, 0),
(11, 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR4_n_pQIOy2qXwTyaoX8IT9ghKLZpGLN7PJw&s', 1, 0),
(12, 'https://cdn.choibongro.vn/wp-content/uploads/2024/04/giay-bong-ro-tre-em-c111-c112-choibongro-vn-13.webp', 1, 0),
(13, 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSVwbIANMY7atz85kaXf86nwWn71sa2h-QJHg&s', 1, 0),
(14, 'https://hidosport.vn/wp-content/uploads/2025/10/ao-the-thao-nam-nu-egan-air-chinh-hang-2025-2.webp', 1, 0),
(15, 'https://bizweb.dktcdn.net/100/017/070/products/ao-pickleball-nam-forehand-mau-do-3.jpg?v=1753804647410', 1, 0),
(16, 'https://n7media.coolmate.me/uploads/May2025/ao-thun-the-thao-nam-phoi-bo-co-thumb-1.png', 1, 0),
(17, 'https://www.sporter.vn/wp-content/uploads/2017/06/quan-jogger-nam-360s-lx-den-5.jpg', 1, 0),
(18, 'https://pos.nvncdn.com/822bfa-13829/ps/20190723_gDxw2W4F7LnYdhKIGkoCDzTE.jpg?v=1676339531', 1, 0),
(19, 'https://www.sporter.vn/wp-content/uploads/2017/06/quan-jogger-nam-360s-lx-xanh-3-750x800.jpg', 1, 0),
(20, 'https://justplay.vn/wp-content/uploads/2025/02/quan-the-thao-just-play-victry-mau-trang.jpg', 1, 0),
(21, 'https://deltasport.vn/wp-content/uploads/2025/05/PO056W1-polo-the-thao-xam-dam-429K-4.jpg', 1, 0),
(22, 'https://product.hstatic.net/200000365171/product/6_40ef2ad3cad84212a44b8f035f4e9e24_master.png', 1, 0),
(23, 'https://www.donex.vn/upload/image/product/38011919_1.jpg', 1, 0),
(24, 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ813W9w7v9VNAJfIdwsI8AU8Jp5zUIhvc1wA&s', 1, 0),
(25, 'https://fsfcenter.vn/upload/sanpham/large/set-do-tap-the-thao-gym-yoga-aerobic-phoi-mau-004-1668655974-e00da0.jpg', 1, 0),
(26, 'https://thegioidotap.vn/wp-content/uploads/2024/03/Do-tap-Yoga-nu-thegioidotap.vn-3.jpg', 1, 0),
(27, 'https://xuatnhapkhautheoyeucau.com/wp-content/uploads/2020/01/O1CN01Slvx1n1dJyL7Wkhfy_1128833716.jpg_430x430q90.jpg', 1, 0),
(28, 'https://nugym.vn/wp-content/uploads/2025/12/43.jpg.jpg', 1, 0),
(29, 'https://saigonbadminton.vn/wp-content/uploads/2025/04/kiotviet_e01be1b79ef1f87d889629479a30edde.jpg', 1, 0),
(30, 'https://cdn0921.cdn4s1.com/media/san-pham/cau-long/vot-cau-long/vcl-ht-7728/vcl-doi-ln3.jpg', 1, 0),
(31, 'https://images.elipsport.vn/anh-seo-tin-tuc/2024/4/24/nhung-hang-vot-cau-long-noi-tieng-the-gioi-12.jpg', 1, 0),
(32, 'https://bizweb.dktcdn.net/thumb/large/100/301/479/files/victor-870b0073-e7c9-4740-b6f1-1ce583a8d596.jpg?v=1529041126713', 1, 0),
(33, 'https://product.hstatic.net/200000174771/product/6db845d05af54ecf893ae82e1b65e912_7003e436a8c64e268a862a0d87312c0d_large.jpg', 1, 0),
(34, 'https://cdn.shopify.com/s/files/1/0456/5070/6581/files/FB2983-103-1_540x.jpg?v=1721988904', 1, 0),
(35, 'https://cdnmedia.baotintuc.vn/Upload/rGkvwNpj74Z1EcpzQ6ltA/files/2022/10/tuan7/bong-301022.jpg', 1, 0),
(36, 'https://cdn.hstatic.net/products/1000061481/dsc001231231125_56a75cd7b75c4d6eaa61c731f25dfeed_large.jpg', 1, 0),
(37, 'https://hidola.com/upload/sanpham/tui-the-thao-6147.jpg', 1, 0),
(38, 'https://justplay.vn/wp-content/uploads/2026/03/Bang-dau-just-play-jp-4-2.jpg', 1, 0),
(39, 'https://xbags.vn/data/uploads/132304-02.jpg', 1, 0),
(40, 'https://product.hstatic.net/1000281067/product/balo-keep-fly-backpack-sky-mau-den_6e1f7d4f3a344c41a47ed376ea3946a5.jpg', 1, 0);

INSERT INTO ProductVariants (ProductId, Sku, Size, Color, Price, StockQuantity, LowStockThreshold, IsActive, CreatedAt)
SELECT Id, CONCAT('SKU-', Id, '-39'), '39', N'Tiêu chuẩn', BasePrice, 20, 5, 1, GETUTCDATE() FROM Products WHERE Id BETWEEN 1 AND 12;

INSERT INTO ProductVariants (ProductId, Sku, Size, Color, Price, StockQuantity, LowStockThreshold, IsActive, CreatedAt)
SELECT Id, CONCAT('SKU-', Id, '-40'), '40', N'Tiêu chuẩn', BasePrice, 35, 5, 1, GETUTCDATE() FROM Products WHERE Id BETWEEN 1 AND 12;

INSERT INTO ProductVariants (ProductId, Sku, Size, Color, Price, StockQuantity, LowStockThreshold, IsActive, CreatedAt)
SELECT Id, CONCAT('SKU-', Id, '-41'), '41', N'Tiêu chuẩn', BasePrice, 25, 5, 1, GETUTCDATE() FROM Products WHERE Id BETWEEN 1 AND 12;

INSERT INTO ProductVariants (ProductId, Sku, Size, Color, Price, StockQuantity, LowStockThreshold, IsActive, CreatedAt)
SELECT Id, CONCAT('SKU-', Id, '-42'), '42', N'Tiêu chuẩn', BasePrice, 15, 5, 1, GETUTCDATE() FROM Products WHERE Id BETWEEN 1 AND 12;

INSERT INTO ProductVariants (ProductId, Sku, Size, Color, Price, StockQuantity, LowStockThreshold, IsActive, CreatedAt)
SELECT Id, CONCAT('SKU-', Id, '-43'), '43', N'Tiêu chuẩn', BasePrice, 10, 5, 1, GETUTCDATE() FROM Products WHERE Id BETWEEN 1 AND 12;

INSERT INTO ProductVariants (ProductId, Sku, Size, Color, Price, StockQuantity, LowStockThreshold, IsActive, CreatedAt)
SELECT Id, CONCAT('SKU-', Id, '-S'), 'S', N'Tiêu chuẩn', BasePrice, 40, 5, 1, GETUTCDATE() FROM Products WHERE Id BETWEEN 13 AND 28;

INSERT INTO ProductVariants (ProductId, Sku, Size, Color, Price, StockQuantity, LowStockThreshold, IsActive, CreatedAt)
SELECT Id, CONCAT('SKU-', Id, '-M'), 'M', N'Tiêu chuẩn', BasePrice, 50, 5, 1, GETUTCDATE() FROM Products WHERE Id BETWEEN 13 AND 28;

INSERT INTO ProductVariants (ProductId, Sku, Size, Color, Price, StockQuantity, LowStockThreshold, IsActive, CreatedAt)
SELECT Id, CONCAT('SKU-', Id, '-L'), 'L', N'Tiêu chuẩn', BasePrice, 45, 5, 1, GETUTCDATE() FROM Products WHERE Id BETWEEN 13 AND 28;

INSERT INTO ProductVariants (ProductId, Sku, Size, Color, Price, StockQuantity, LowStockThreshold, IsActive, CreatedAt)
SELECT Id, CONCAT('SKU-', Id, '-XL'), 'XL', N'Tiêu chuẩn', BasePrice, 30, 5, 1, GETUTCDATE() FROM Products WHERE Id BETWEEN 13 AND 28;

INSERT INTO ProductVariants (ProductId, Sku, Size, Color, Price, StockQuantity, LowStockThreshold, IsActive, CreatedAt)
SELECT Id, CONCAT('SKU-', Id, '-XXL'), 'XXL', N'Tiêu chuẩn', BasePrice, 20, 5, 1, GETUTCDATE() FROM Products WHERE Id BETWEEN 13 AND 28;

INSERT INTO ProductVariants (ProductId, Sku, Size, Color, Price, StockQuantity, LowStockThreshold, IsActive, CreatedAt)
SELECT Id, CONCAT('SKU-', Id, '-FS'), 'Free Size', N'Tiêu chuẩn', BasePrice, 100, 5, 1, GETUTCDATE() FROM Products WHERE Id BETWEEN 29 AND 40;