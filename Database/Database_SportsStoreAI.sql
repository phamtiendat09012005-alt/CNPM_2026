/*
===============================================================================
 DATABASE: SportsStoreAI
 DBMS    : Microsoft SQL Server
 SCOPE   :
   1. Tài khoản và phân quyền (chức năng nền tảng)
   2. Quản lý sản phẩm và biến thể
   3. Tra cứu sản phẩm và quản lý giỏ hàng
   4. Đặt hàng, thanh toán và xử lý đơn hàng
   5. AI gợi ý sản phẩm và kích thước

 LƯU Ý:
 - Không có bán hàng tại quầy/POS.
 - Không có voucher, tích điểm, đổi trả, nhà cung cấp, nhiều chi nhánh.
 - Bảng Users/Roles tương thích cấu trúc ASP.NET Core Identity dùng khóa INT.
 - Không chèn tài khoản Admin bằng SQL vì PasswordHash cần được Identity tạo.
===============================================================================
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF DB_ID(N'SportsStoreAI') IS NULL
BEGIN
    CREATE DATABASE SportsStoreAI;
END
GO

USE SportsStoreAI;
GO

/*=============================================================================
  1. ASP.NET CORE IDENTITY
=============================================================================*/

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id                      INT IDENTITY(1,1) NOT NULL,
        FullName                NVARCHAR(120) NOT NULL,
        Status                  NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Users_Status DEFAULT N'Active',
        CreatedAt               DATETIME2(0) NOT NULL
            CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt               DATETIME2(0) NULL,

        UserName                NVARCHAR(256) NULL,
        NormalizedUserName      NVARCHAR(256) NULL,
        Email                   NVARCHAR(256) NULL,
        NormalizedEmail         NVARCHAR(256) NULL,
        EmailConfirmed          BIT NOT NULL
            CONSTRAINT DF_Users_EmailConfirmed DEFAULT 0,
        PasswordHash            NVARCHAR(MAX) NULL,
        SecurityStamp           NVARCHAR(MAX) NULL,
        ConcurrencyStamp        NVARCHAR(MAX) NULL,
        PhoneNumber             NVARCHAR(30) NULL,
        PhoneNumberConfirmed    BIT NOT NULL
            CONSTRAINT DF_Users_PhoneConfirmed DEFAULT 0,
        TwoFactorEnabled        BIT NOT NULL
            CONSTRAINT DF_Users_TwoFactor DEFAULT 0,
        LockoutEnd              DATETIMEOFFSET NULL,
        LockoutEnabled          BIT NOT NULL
            CONSTRAINT DF_Users_LockoutEnabled DEFAULT 1,
        AccessFailedCount       INT NOT NULL
            CONSTRAINT DF_Users_AccessFailedCount DEFAULT 0,

        CONSTRAINT PK_Users PRIMARY KEY (Id),
        CONSTRAINT CK_Users_Status
            CHECK (Status IN (N'Active', N'Locked', N'Inactive')),
        CONSTRAINT CK_Users_AccessFailedCount
            CHECK (AccessFailedCount >= 0)
    );

    CREATE UNIQUE INDEX UserNameIndex
        ON dbo.Users(NormalizedUserName)
        WHERE NormalizedUserName IS NOT NULL;

    CREATE INDEX EmailIndex
        ON dbo.Users(NormalizedEmail);
END
GO

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        Id                  INT IDENTITY(1,1) NOT NULL,
        Name                NVARCHAR(256) NULL,
        NormalizedName      NVARCHAR(256) NULL,
        ConcurrencyStamp    NVARCHAR(MAX) NULL,

        CONSTRAINT PK_Roles PRIMARY KEY (Id)
    );

    CREATE UNIQUE INDEX RoleNameIndex
        ON dbo.Roles(NormalizedName)
        WHERE NormalizedName IS NOT NULL;
END
GO

IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRoles
    (
        UserId INT NOT NULL,
        RoleId INT NOT NULL,

        CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UserRoles_Users_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserRoles_Roles_RoleId
            FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_UserRoles_RoleId ON dbo.UserRoles(RoleId);
END
GO

IF OBJECT_ID(N'dbo.UserClaims', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserClaims
    (
        Id          INT IDENTITY(1,1) NOT NULL,
        UserId      INT NOT NULL,
        ClaimType   NVARCHAR(MAX) NULL,
        ClaimValue  NVARCHAR(MAX) NULL,

        CONSTRAINT PK_UserClaims PRIMARY KEY (Id),
        CONSTRAINT FK_UserClaims_Users_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_UserClaims_UserId ON dbo.UserClaims(UserId);
END
GO

IF OBJECT_ID(N'dbo.RoleClaims', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RoleClaims
    (
        Id          INT IDENTITY(1,1) NOT NULL,
        RoleId      INT NOT NULL,
        ClaimType   NVARCHAR(MAX) NULL,
        ClaimValue  NVARCHAR(MAX) NULL,

        CONSTRAINT PK_RoleClaims PRIMARY KEY (Id),
        CONSTRAINT FK_RoleClaims_Roles_RoleId
            FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_RoleClaims_RoleId ON dbo.RoleClaims(RoleId);
END
GO

IF OBJECT_ID(N'dbo.UserLogins', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserLogins
    (
        LoginProvider       NVARCHAR(450) NOT NULL,
        ProviderKey         NVARCHAR(450) NOT NULL,
        ProviderDisplayName NVARCHAR(MAX) NULL,
        UserId              INT NOT NULL,

        CONSTRAINT PK_UserLogins PRIMARY KEY (LoginProvider, ProviderKey),
        CONSTRAINT FK_UserLogins_Users_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_UserLogins_UserId ON dbo.UserLogins(UserId);
END
GO

IF OBJECT_ID(N'dbo.UserTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserTokens
    (
        UserId         INT NOT NULL,
        LoginProvider  NVARCHAR(450) NOT NULL,
        Name           NVARCHAR(450) NOT NULL,
        Value          NVARCHAR(MAX) NULL,

        CONSTRAINT PK_UserTokens PRIMARY KEY (UserId, LoginProvider, Name),
        CONSTRAINT FK_UserTokens_Users_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.UserAddresses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserAddresses
    (
        Id              INT IDENTITY(1,1) NOT NULL,
        UserId          INT NOT NULL,
        ReceiverName    NVARCHAR(120) NOT NULL,
        ReceiverPhone   NVARCHAR(20) NOT NULL,
        AddressLine     NVARCHAR(300) NOT NULL,
        Ward            NVARCHAR(100) NOT NULL,
        District        NVARCHAR(100) NOT NULL,
        Province        NVARCHAR(100) NOT NULL,
        IsDefault       BIT NOT NULL
            CONSTRAINT DF_UserAddresses_IsDefault DEFAULT 0,
        CreatedAt       DATETIME2(0) NOT NULL
            CONSTRAINT DF_UserAddresses_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt       DATETIME2(0) NULL,

        CONSTRAINT PK_UserAddresses PRIMARY KEY (Id),
        CONSTRAINT FK_UserAddresses_Users_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_UserAddresses_UserId ON dbo.UserAddresses(UserId);
END
GO

/*=============================================================================
  2. DANH MỤC, THƯƠNG HIỆU, SẢN PHẨM VÀ BIẾN THỂ
=============================================================================*/

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        Id          INT IDENTITY(1,1) NOT NULL,
        Name        NVARCHAR(120) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive    BIT NOT NULL
            CONSTRAINT DF_Categories_IsActive DEFAULT 1,
        CreatedAt   DATETIME2(0) NOT NULL
            CONSTRAINT DF_Categories_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt   DATETIME2(0) NULL,

        CONSTRAINT PK_Categories PRIMARY KEY (Id),
        CONSTRAINT UQ_Categories_Name UNIQUE (Name)
    );
END
GO

IF OBJECT_ID(N'dbo.Brands', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Brands
    (
        Id          INT IDENTITY(1,1) NOT NULL,
        Name        NVARCHAR(100) NOT NULL,
        LogoUrl     NVARCHAR(500) NULL,
        IsActive    BIT NOT NULL
            CONSTRAINT DF_Brands_IsActive DEFAULT 1,
        CreatedAt   DATETIME2(0) NOT NULL
            CONSTRAINT DF_Brands_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt   DATETIME2(0) NULL,

        CONSTRAINT PK_Brands PRIMARY KEY (Id),
        CONSTRAINT UQ_Brands_Name UNIQUE (Name)
    );
END
GO

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id            INT IDENTITY(1,1) NOT NULL,
        CategoryId    INT NOT NULL,
        BrandId       INT NULL,
        Name          NVARCHAR(180) NOT NULL,
        Slug          NVARCHAR(220) NOT NULL,
        Description   NVARCHAR(MAX) NULL,
        BasePrice     DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_Products_BasePrice DEFAULT 0,
        SportType     NVARCHAR(80) NULL,
        ProductType   NVARCHAR(30) NOT NULL,
        IsActive      BIT NOT NULL
            CONSTRAINT DF_Products_IsActive DEFAULT 1,
        CreatedAt     DATETIME2(0) NOT NULL
            CONSTRAINT DF_Products_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt     DATETIME2(0) NULL,

        CONSTRAINT PK_Products PRIMARY KEY (Id),
        CONSTRAINT UQ_Products_Slug UNIQUE (Slug),
        CONSTRAINT CK_Products_BasePrice CHECK (BasePrice >= 0),
        CONSTRAINT CK_Products_ProductType
            CHECK (ProductType IN
                (N'Shoes', N'Clothing', N'Equipment', N'Accessory')),
        CONSTRAINT FK_Products_Categories_CategoryId
            FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id),
        CONSTRAINT FK_Products_Brands_BrandId
            FOREIGN KEY (BrandId) REFERENCES dbo.Brands(Id)
    );

    CREATE INDEX IX_Products_CategoryId_IsActive
        ON dbo.Products(CategoryId, IsActive);

    CREATE INDEX IX_Products_BrandId
        ON dbo.Products(BrandId);

    CREATE INDEX IX_Products_Name
        ON dbo.Products(Name);
END
GO

IF OBJECT_ID(N'dbo.ProductImages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductImages
    (
        Id          INT IDENTITY(1,1) NOT NULL,
        ProductId   INT NOT NULL,
        Url         NVARCHAR(500) NOT NULL,
        IsPrimary   BIT NOT NULL
            CONSTRAINT DF_ProductImages_IsPrimary DEFAULT 0,
        SortOrder   INT NOT NULL
            CONSTRAINT DF_ProductImages_SortOrder DEFAULT 0,

        CONSTRAINT PK_ProductImages PRIMARY KEY (Id),
        CONSTRAINT CK_ProductImages_SortOrder CHECK (SortOrder >= 0),
        CONSTRAINT FK_ProductImages_Products_ProductId
            FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_ProductImages_ProductId
        ON dbo.ProductImages(ProductId);

    CREATE UNIQUE INDEX UX_ProductImages_OnePrimaryPerProduct
        ON dbo.ProductImages(ProductId)
        WHERE IsPrimary = 1;
END
GO

IF OBJECT_ID(N'dbo.ProductVariants', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductVariants
    (
        Id                  INT IDENTITY(1,1) NOT NULL,
        ProductId           INT NOT NULL,
        SKU                 NVARCHAR(60) NOT NULL,
        Size                NVARCHAR(30) NULL,
        Color               NVARCHAR(50) NULL,
        Price               DECIMAL(18,2) NOT NULL,
        StockQuantity       INT NOT NULL
            CONSTRAINT DF_ProductVariants_Stock DEFAULT 0,
        LowStockThreshold   INT NOT NULL
            CONSTRAINT DF_ProductVariants_LowStock DEFAULT 5,
        IsActive            BIT NOT NULL
            CONSTRAINT DF_ProductVariants_IsActive DEFAULT 1,
        CreatedAt           DATETIME2(0) NOT NULL
            CONSTRAINT DF_ProductVariants_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt           DATETIME2(0) NULL,

        CONSTRAINT PK_ProductVariants PRIMARY KEY (Id),
        CONSTRAINT UQ_ProductVariants_SKU UNIQUE (SKU),
        CONSTRAINT CK_ProductVariants_Price CHECK (Price >= 0),
        CONSTRAINT CK_ProductVariants_Stock CHECK (StockQuantity >= 0),
        CONSTRAINT CK_ProductVariants_LowStock CHECK (LowStockThreshold >= 0),
        CONSTRAINT FK_ProductVariants_Products_ProductId
            FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
    );

    CREATE INDEX IX_ProductVariants_ProductId_IsActive
        ON dbo.ProductVariants(ProductId, IsActive);

    CREATE INDEX IX_ProductVariants_Size_Color
        ON dbo.ProductVariants(Size, Color);
END
GO

/*=============================================================================
  3. GIỎ HÀNG
=============================================================================*/

IF OBJECT_ID(N'dbo.Carts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Carts
    (
        Id          INT IDENTITY(1,1) NOT NULL,
        UserId      INT NOT NULL,
        CreatedAt   DATETIME2(0) NOT NULL
            CONSTRAINT DF_Carts_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt   DATETIME2(0) NOT NULL
            CONSTRAINT DF_Carts_UpdatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Carts PRIMARY KEY (Id),
        CONSTRAINT UQ_Carts_UserId UNIQUE (UserId),
        CONSTRAINT FK_Carts_Users_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.CartItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CartItems
    (
        Id                  INT IDENTITY(1,1) NOT NULL,
        CartId              INT NOT NULL,
        ProductVariantId    INT NOT NULL,
        Quantity            INT NOT NULL,
        IsSelected          BIT NOT NULL
            CONSTRAINT DF_CartItems_IsSelected DEFAULT 1,
        AddedAt             DATETIME2(0) NOT NULL
            CONSTRAINT DF_CartItems_AddedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt           DATETIME2(0) NULL,

        CONSTRAINT PK_CartItems PRIMARY KEY (Id),
        CONSTRAINT UQ_CartItems_Cart_Variant
            UNIQUE (CartId, ProductVariantId),
        CONSTRAINT CK_CartItems_Quantity CHECK (Quantity > 0),
        CONSTRAINT FK_CartItems_Carts_CartId
            FOREIGN KEY (CartId) REFERENCES dbo.Carts(Id) ON DELETE CASCADE,
        CONSTRAINT FK_CartItems_ProductVariants_ProductVariantId
            FOREIGN KEY (ProductVariantId) REFERENCES dbo.ProductVariants(Id)
    );

    CREATE INDEX IX_CartItems_ProductVariantId
        ON dbo.CartItems(ProductVariantId);
END
GO

/*=============================================================================
  4. ĐƠN HÀNG, THANH TOÁN VÀ XỬ LÝ ĐƠN
=============================================================================*/

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        Id                BIGINT IDENTITY(1,1) NOT NULL,
        UserId            INT NOT NULL,
        OrderCode         NVARCHAR(30) NOT NULL,

        ReceiverName      NVARCHAR(120) NOT NULL,
        ReceiverPhone     NVARCHAR(20) NOT NULL,
        ShippingAddress   NVARCHAR(500) NOT NULL,

        Subtotal          DECIMAL(18,2) NOT NULL,
        ShippingFee       DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_Orders_ShippingFee DEFAULT 0,
        TotalAmount       DECIMAL(18,2) NOT NULL,

        Status            NVARCHAR(30) NOT NULL,
        Note              NVARCHAR(500) NULL,
        CreatedAt         DATETIME2(0) NOT NULL
            CONSTRAINT DF_Orders_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt         DATETIME2(0) NULL,
        CancelledAt       DATETIME2(0) NULL,

        CONSTRAINT PK_Orders PRIMARY KEY (Id),
        CONSTRAINT UQ_Orders_OrderCode UNIQUE (OrderCode),
        CONSTRAINT CK_Orders_Subtotal CHECK (Subtotal >= 0),
        CONSTRAINT CK_Orders_ShippingFee CHECK (ShippingFee >= 0),
        CONSTRAINT CK_Orders_TotalAmount CHECK (TotalAmount >= 0),
        CONSTRAINT CK_Orders_TotalFormula
            CHECK (TotalAmount = Subtotal + ShippingFee),
        CONSTRAINT CK_Orders_Status
            CHECK (Status IN
                (N'AwaitingPayment', N'Pending', N'Confirmed',
                 N'Processing', N'Shipping', N'Completed', N'Cancelled')),
        CONSTRAINT FK_Orders_Users_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );

    CREATE INDEX IX_Orders_UserId_CreatedAt
        ON dbo.Orders(UserId, CreatedAt DESC);

    CREATE INDEX IX_Orders_Status_CreatedAt
        ON dbo.Orders(Status, CreatedAt DESC);
END
GO

IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems
    (
        Id                  BIGINT IDENTITY(1,1) NOT NULL,
        OrderId             BIGINT NOT NULL,
        ProductVariantId    INT NOT NULL,

        ProductName         NVARCHAR(180) NOT NULL,
        SKU                 NVARCHAR(60) NOT NULL,
        Size                NVARCHAR(30) NULL,
        Color               NVARCHAR(50) NULL,
        UnitPrice           DECIMAL(18,2) NOT NULL,
        Quantity            INT NOT NULL,
        TotalPrice          AS (UnitPrice * Quantity) PERSISTED,

        CONSTRAINT PK_OrderItems PRIMARY KEY (Id),
        CONSTRAINT CK_OrderItems_UnitPrice CHECK (UnitPrice >= 0),
        CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0),
        CONSTRAINT FK_OrderItems_Orders_OrderId
            FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderItems_ProductVariants_ProductVariantId
            FOREIGN KEY (ProductVariantId) REFERENCES dbo.ProductVariants(Id)
    );

    CREATE INDEX IX_OrderItems_OrderId
        ON dbo.OrderItems(OrderId);

    CREATE INDEX IX_OrderItems_ProductVariantId
        ON dbo.OrderItems(ProductVariantId);
END
GO

IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments
    (
        Id                      BIGINT IDENTITY(1,1) NOT NULL,
        OrderId                 BIGINT NOT NULL,
        Method                  NVARCHAR(30) NOT NULL,
        Status                  NVARCHAR(20) NOT NULL,
        Amount                  DECIMAL(18,2) NOT NULL,
        Provider                NVARCHAR(60) NULL,
        ProviderTransactionId   NVARCHAR(120) NULL,
        PaymentUrl              NVARCHAR(1000) NULL,
        FailureReason           NVARCHAR(500) NULL,
        PaidAt                  DATETIME2(0) NULL,
        CreatedAt               DATETIME2(0) NOT NULL
            CONSTRAINT DF_Payments_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt               DATETIME2(0) NULL,

        CONSTRAINT PK_Payments PRIMARY KEY (Id),
        CONSTRAINT CK_Payments_Method
            CHECK (Method IN (N'COD', N'OnlineGateway')),
        CONSTRAINT CK_Payments_Status
            CHECK (Status IN
                (N'Unpaid', N'Pending', N'Paid', N'Failed', N'Refunded')),
        CONSTRAINT CK_Payments_Amount CHECK (Amount >= 0),
        CONSTRAINT FK_Payments_Orders_OrderId
            FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_Payments_OrderId
        ON dbo.Payments(OrderId);

    CREATE UNIQUE INDEX UX_Payments_ProviderTransactionId
        ON dbo.Payments(ProviderTransactionId)
        WHERE ProviderTransactionId IS NOT NULL;
END
GO

IF OBJECT_ID(N'dbo.OrderStatusHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderStatusHistories
    (
        Id                  BIGINT IDENTITY(1,1) NOT NULL,
        OrderId             BIGINT NOT NULL,
        Status              NVARCHAR(30) NOT NULL,
        Note                NVARCHAR(500) NULL,
        ChangedByUserId     INT NULL,
        ChangedAt           DATETIME2(0) NOT NULL
            CONSTRAINT DF_OrderStatusHistories_ChangedAt
            DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_OrderStatusHistories PRIMARY KEY (Id),
        CONSTRAINT CK_OrderStatusHistories_Status
            CHECK (Status IN
                (N'AwaitingPayment', N'Pending', N'Confirmed',
                 N'Processing', N'Shipping', N'Completed', N'Cancelled')),
        CONSTRAINT FK_OrderStatusHistories_Orders_OrderId
            FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderStatusHistories_Users_ChangedByUserId
            FOREIGN KEY (ChangedByUserId) REFERENCES dbo.Users(Id)
    );

    CREATE INDEX IX_OrderStatusHistories_OrderId_ChangedAt
        ON dbo.OrderStatusHistories(OrderId, ChangedAt);
END
GO

/*=============================================================================
  5. AI GỢI Ý SẢN PHẨM VÀ KÍCH THƯỚC
=============================================================================*/

IF OBJECT_ID(N'dbo.ProductViewHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductViewHistories
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL,
        UserId      INT NULL,
        ProductId   INT NOT NULL,
        SessionId   NVARCHAR(100) NULL,
        ViewedAt    DATETIME2(0) NOT NULL
            CONSTRAINT DF_ProductViewHistories_ViewedAt
            DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_ProductViewHistories PRIMARY KEY (Id),
        CONSTRAINT CK_ProductViewHistories_Identity
            CHECK (UserId IS NOT NULL OR SessionId IS NOT NULL),
        CONSTRAINT FK_ProductViewHistories_Users_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
        CONSTRAINT FK_ProductViewHistories_Products_ProductId
            FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_ProductViewHistories_UserId_ViewedAt
        ON dbo.ProductViewHistories(UserId, ViewedAt DESC);

    CREATE INDEX IX_ProductViewHistories_ProductId_ViewedAt
        ON dbo.ProductViewHistories(ProductId, ViewedAt DESC);

    CREATE INDEX IX_ProductViewHistories_SessionId_ViewedAt
        ON dbo.ProductViewHistories(SessionId, ViewedAt DESC);
END
GO

IF OBJECT_ID(N'dbo.SizeCharts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SizeCharts
    (
        Id                  INT IDENTITY(1,1) NOT NULL,
        BrandId             INT NULL,
        ProductType         NVARCHAR(30) NOT NULL,
        SizeLabel           NVARCHAR(30) NOT NULL,

        MinFootLengthCm     DECIMAL(5,2) NULL,
        MaxFootLengthCm     DECIMAL(5,2) NULL,
        MinHeightCm         DECIMAL(5,2) NULL,
        MaxHeightCm         DECIMAL(5,2) NULL,
        MinWeightKg         DECIMAL(5,2) NULL,
        MaxWeightKg         DECIMAL(5,2) NULL,
        MinChestCm          DECIMAL(5,2) NULL,
        MaxChestCm          DECIMAL(5,2) NULL,
        MinWaistCm          DECIMAL(5,2) NULL,
        MaxWaistCm          DECIMAL(5,2) NULL,

        FitNote             NVARCHAR(300) NULL,
        IsActive            BIT NOT NULL
            CONSTRAINT DF_SizeCharts_IsActive DEFAULT 1,
        CreatedAt           DATETIME2(0) NOT NULL
            CONSTRAINT DF_SizeCharts_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt           DATETIME2(0) NULL,

        CONSTRAINT PK_SizeCharts PRIMARY KEY (Id),
        CONSTRAINT CK_SizeCharts_ProductType
            CHECK (ProductType IN (N'Shoes', N'Clothing')),
        CONSTRAINT CK_SizeCharts_FootRange
            CHECK (
                MinFootLengthCm IS NULL OR MaxFootLengthCm IS NULL
                OR MinFootLengthCm <= MaxFootLengthCm
            ),
        CONSTRAINT CK_SizeCharts_HeightRange
            CHECK (
                MinHeightCm IS NULL OR MaxHeightCm IS NULL
                OR MinHeightCm <= MaxHeightCm
            ),
        CONSTRAINT CK_SizeCharts_WeightRange
            CHECK (
                MinWeightKg IS NULL OR MaxWeightKg IS NULL
                OR MinWeightKg <= MaxWeightKg
            ),
        CONSTRAINT CK_SizeCharts_ChestRange
            CHECK (
                MinChestCm IS NULL OR MaxChestCm IS NULL
                OR MinChestCm <= MaxChestCm
            ),
        CONSTRAINT CK_SizeCharts_WaistRange
            CHECK (
                MinWaistCm IS NULL OR MaxWaistCm IS NULL
                OR MinWaistCm <= MaxWaistCm
            ),
        CONSTRAINT FK_SizeCharts_Brands_BrandId
            FOREIGN KEY (BrandId) REFERENCES dbo.Brands(Id)
    );

    CREATE INDEX IX_SizeCharts_Brand_ProductType
        ON dbo.SizeCharts(BrandId, ProductType, IsActive);
END
GO

IF OBJECT_ID(N'dbo.RecommendationLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RecommendationLogs
    (
        Id                      BIGINT IDENTITY(1,1) NOT NULL,
        UserId                  INT NULL,
        SessionId               NVARCHAR(100) NULL,
        RecommendationType      NVARCHAR(30) NOT NULL,
        SourceProductId         INT NULL,
        RecommendedProductId    INT NULL,
        RecommendedSize         NVARCHAR(30) NULL,
        Score                   DECIMAL(6,5) NOT NULL,
        Reason                  NVARCHAR(500) NOT NULL,
        IsClicked               BIT NOT NULL
            CONSTRAINT DF_RecommendationLogs_IsClicked DEFAULT 0,
        IsPurchased             BIT NOT NULL
            CONSTRAINT DF_RecommendationLogs_IsPurchased DEFAULT 0,
        CreatedAt               DATETIME2(0) NOT NULL
            CONSTRAINT DF_RecommendationLogs_CreatedAt
            DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_RecommendationLogs PRIMARY KEY (Id),
        CONSTRAINT CK_RecommendationLogs_Type
            CHECK (RecommendationType IN
                (N'Related', N'Personalized', N'Popular', N'Size')),
        CONSTRAINT CK_RecommendationLogs_Score
            CHECK (Score >= 0 AND Score <= 1),
        CONSTRAINT CK_RecommendationLogs_Target
            CHECK (
                RecommendedProductId IS NOT NULL
                OR RecommendedSize IS NOT NULL
            ),
        CONSTRAINT FK_RecommendationLogs_Users_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
        CONSTRAINT FK_RecommendationLogs_Products_SourceProductId
            FOREIGN KEY (SourceProductId) REFERENCES dbo.Products(Id),
        CONSTRAINT FK_RecommendationLogs_Products_RecommendedProductId
            FOREIGN KEY (RecommendedProductId) REFERENCES dbo.Products(Id)
    );

    CREATE INDEX IX_RecommendationLogs_UserId_CreatedAt
        ON dbo.RecommendationLogs(UserId, CreatedAt DESC);

    CREATE INDEX IX_RecommendationLogs_SessionId_CreatedAt
        ON dbo.RecommendationLogs(SessionId, CreatedAt DESC);

    CREATE INDEX IX_RecommendationLogs_SourceProductId
        ON dbo.RecommendationLogs(SourceProductId);
END
GO

/*=============================================================================
  6. VIEW HỖ TRỢ
=============================================================================*/

CREATE OR ALTER VIEW dbo.vw_ProductCatalog
AS
SELECT
    p.Id AS ProductId,
    p.Name AS ProductName,
    p.Slug,
    p.ProductType,
    p.SportType,
    c.Id AS CategoryId,
    c.Name AS CategoryName,
    b.Id AS BrandId,
    b.Name AS BrandName,
    MIN(v.Price) AS MinPrice,
    MAX(v.Price) AS MaxPrice,
    SUM(v.StockQuantity) AS TotalStock,
    MAX(CASE WHEN i.IsPrimary = 1 THEN i.Url END) AS PrimaryImageUrl,
    p.IsActive
FROM dbo.Products p
INNER JOIN dbo.Categories c ON c.Id = p.CategoryId
LEFT JOIN dbo.Brands b ON b.Id = p.BrandId
INNER JOIN dbo.ProductVariants v
    ON v.ProductId = p.Id AND v.IsActive = 1
LEFT JOIN dbo.ProductImages i
    ON i.ProductId = p.Id
GROUP BY
    p.Id, p.Name, p.Slug, p.ProductType, p.SportType,
    c.Id, c.Name, b.Id, b.Name, p.IsActive;
GO

CREATE OR ALTER VIEW dbo.vw_OrderSummary
AS
SELECT
    o.Id AS OrderId,
    o.OrderCode,
    o.UserId,
    u.FullName AS CustomerName,
    u.Email AS CustomerEmail,
    o.TotalAmount,
    o.Status AS OrderStatus,
    pay.Method AS PaymentMethod,
    pay.Status AS PaymentStatus,
    o.CreatedAt
FROM dbo.Orders o
INNER JOIN dbo.Users u ON u.Id = o.UserId
OUTER APPLY
(
    SELECT TOP (1)
        p.Method,
        p.Status
    FROM dbo.Payments p
    WHERE p.OrderId = o.Id
    ORDER BY p.CreatedAt DESC, p.Id DESC
) pay;
GO

/*=============================================================================
  7. DỮ LIỆU MẪU
=============================================================================*/

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE NormalizedName = N'CUSTOMER')
BEGIN
    INSERT INTO dbo.Roles(Name, NormalizedName, ConcurrencyStamp)
    VALUES (N'Customer', N'CUSTOMER', CONVERT(NVARCHAR(36), NEWID()));
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE NormalizedName = N'ADMIN')
BEGIN
    INSERT INTO dbo.Roles(Name, NormalizedName, ConcurrencyStamp)
    VALUES (N'Admin', N'ADMIN', CONVERT(NVARCHAR(36), NEWID()));
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = N'Giày thể thao')
    INSERT INTO dbo.Categories(Name, Description)
    VALUES (N'Giày thể thao', N'Giày chạy bộ, bóng đá và luyện tập.');

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = N'Quần áo thể thao')
    INSERT INTO dbo.Categories(Name, Description)
    VALUES (N'Quần áo thể thao', N'Áo, quần và bộ đồ thể thao.');

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = N'Dụng cụ thể thao')
    INSERT INTO dbo.Categories(Name, Description)
    VALUES (N'Dụng cụ thể thao', N'Bóng, vợt và dụng cụ luyện tập.');

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Name = N'Phụ kiện')
    INSERT INTO dbo.Categories(Name, Description)
    VALUES (N'Phụ kiện', N'Tất, balo, bình nước và phụ kiện.');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE Name = N'Nike')
    INSERT INTO dbo.Brands(Name) VALUES (N'Nike');

IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE Name = N'Adidas')
    INSERT INTO dbo.Brands(Name) VALUES (N'Adidas');

IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE Name = N'Puma')
    INSERT INTO dbo.Brands(Name) VALUES (N'Puma');

IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE Name = N'Yonex')
    INSERT INTO dbo.Brands(Name) VALUES (N'Yonex');
GO

DECLARE
    @CategoryShoes INT = (SELECT Id FROM dbo.Categories WHERE Name = N'Giày thể thao'),
    @CategoryClothing INT = (SELECT Id FROM dbo.Categories WHERE Name = N'Quần áo thể thao'),
    @CategoryEquipment INT = (SELECT Id FROM dbo.Categories WHERE Name = N'Dụng cụ thể thao'),
    @CategoryAccessory INT = (SELECT Id FROM dbo.Categories WHERE Name = N'Phụ kiện'),
    @NikeId INT = (SELECT Id FROM dbo.Brands WHERE Name = N'Nike'),
    @AdidasId INT = (SELECT Id FROM dbo.Brands WHERE Name = N'Adidas'),
    @PumaId INT = (SELECT Id FROM dbo.Brands WHERE Name = N'Puma'),
    @YonexId INT = (SELECT Id FROM dbo.Brands WHERE Name = N'Yonex');

DECLARE @ProductId INT;

-- Sản phẩm 1
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Slug = N'nike-air-zoom-pegasus-41')
BEGIN
    INSERT INTO dbo.Products
    (
        CategoryId, BrandId, Name, Slug, Description,
        BasePrice, SportType, ProductType
    )
    VALUES
    (
        @CategoryShoes, @NikeId, N'Nike Air Zoom Pegasus 41',
        N'nike-air-zoom-pegasus-41',
        N'Giày chạy bộ hằng ngày, phù hợp luyện tập và di chuyển.',
        2490000, N'Running', N'Shoes'
    );

    SET @ProductId = SCOPE_IDENTITY();

    INSERT INTO dbo.ProductImages(ProductId, Url, IsPrimary, SortOrder)
    VALUES
    (@ProductId, N'https://placehold.co/800x800?text=Nike+Pegasus+41', 1, 0);

    INSERT INTO dbo.ProductVariants
    (ProductId, SKU, Size, Color, Price, StockQuantity, LowStockThreshold)
    VALUES
    (@ProductId, N'NIKE-PEG41-40-BLK', N'40', N'Đen', 2490000, 20, 5),
    (@ProductId, N'NIKE-PEG41-41-BLK', N'41', N'Đen', 2490000, 18, 5),
    (@ProductId, N'NIKE-PEG41-42-BLK', N'42', N'Đen', 2490000, 15, 5);
END

-- Sản phẩm 2
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Slug = N'adidas-own-the-run-shirt')
BEGIN
    INSERT INTO dbo.Products
    (
        CategoryId, BrandId, Name, Slug, Description,
        BasePrice, SportType, ProductType
    )
    VALUES
    (
        @CategoryClothing, @AdidasId, N'Adidas Own The Run Shirt',
        N'adidas-own-the-run-shirt',
        N'Áo chạy bộ thoáng khí, phù hợp luyện tập hằng ngày.',
        890000, N'Running', N'Clothing'
    );

    SET @ProductId = SCOPE_IDENTITY();

    INSERT INTO dbo.ProductImages(ProductId, Url, IsPrimary, SortOrder)
    VALUES
    (@ProductId, N'https://placehold.co/800x800?text=Adidas+Running+Shirt', 1, 0);

    INSERT INTO dbo.ProductVariants
    (ProductId, SKU, Size, Color, Price, StockQuantity, LowStockThreshold)
    VALUES
    (@ProductId, N'ADI-RUN-S-BLU', N'S', N'Xanh', 890000, 12, 3),
    (@ProductId, N'ADI-RUN-M-BLU', N'M', N'Xanh', 890000, 15, 3),
    (@ProductId, N'ADI-RUN-L-BLU', N'L', N'Xanh', 890000, 10, 3);
END

-- Sản phẩm 3
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Slug = N'puma-orbita-football')
BEGIN
    INSERT INTO dbo.Products
    (
        CategoryId, BrandId, Name, Slug, Description,
        BasePrice, SportType, ProductType
    )
    VALUES
    (
        @CategoryEquipment, @PumaId, N'Puma Orbita Football',
        N'puma-orbita-football',
        N'Bóng đá luyện tập kích thước tiêu chuẩn.',
        650000, N'Football', N'Equipment'
    );

    SET @ProductId = SCOPE_IDENTITY();

    INSERT INTO dbo.ProductImages(ProductId, Url, IsPrimary, SortOrder)
    VALUES
    (@ProductId, N'https://placehold.co/800x800?text=Puma+Football', 1, 0);

    INSERT INTO dbo.ProductVariants
    (ProductId, SKU, Size, Color, Price, StockQuantity, LowStockThreshold)
    VALUES
    (@ProductId, N'PUMA-ORBITA-5-WHT', N'5', N'Trắng', 650000, 25, 5);
END

-- Sản phẩm 4
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Slug = N'yonex-pro-racket-bag')
BEGIN
    INSERT INTO dbo.Products
    (
        CategoryId, BrandId, Name, Slug, Description,
        BasePrice, SportType, ProductType
    )
    VALUES
    (
        @CategoryAccessory, @YonexId, N'Yonex Pro Racket Bag',
        N'yonex-pro-racket-bag',
        N'Túi đựng vợt cầu lông chuyên dụng.',
        1250000, N'Badminton', N'Accessory'
    );

    SET @ProductId = SCOPE_IDENTITY();

    INSERT INTO dbo.ProductImages(ProductId, Url, IsPrimary, SortOrder)
    VALUES
    (@ProductId, N'https://placehold.co/800x800?text=Yonex+Racket+Bag', 1, 0);

    INSERT INTO dbo.ProductVariants
    (ProductId, SKU, Size, Color, Price, StockQuantity, LowStockThreshold)
    VALUES
    (@ProductId, N'YONEX-BAG-STD-BLK', N'Tiêu chuẩn', N'Đen', 1250000, 8, 2);
END
GO

DECLARE
    @Nike INT = (SELECT Id FROM dbo.Brands WHERE Name = N'Nike'),
    @Adidas INT = (SELECT Id FROM dbo.Brands WHERE Name = N'Adidas');

IF NOT EXISTS
(
    SELECT 1 FROM dbo.SizeCharts
    WHERE BrandId = @Nike AND ProductType = N'Shoes' AND SizeLabel = N'40'
)
    INSERT INTO dbo.SizeCharts
    (BrandId, ProductType, SizeLabel, MinFootLengthCm, MaxFootLengthCm, FitNote)
    VALUES (@Nike, N'Shoes', N'40', 24.60, 25.00, N'Form tiêu chuẩn.');

IF NOT EXISTS
(
    SELECT 1 FROM dbo.SizeCharts
    WHERE BrandId = @Nike AND ProductType = N'Shoes' AND SizeLabel = N'41'
)
    INSERT INTO dbo.SizeCharts
    (BrandId, ProductType, SizeLabel, MinFootLengthCm, MaxFootLengthCm, FitNote)
    VALUES (@Nike, N'Shoes', N'41', 25.10, 25.80, N'Form tiêu chuẩn.');

IF NOT EXISTS
(
    SELECT 1 FROM dbo.SizeCharts
    WHERE BrandId = @Nike AND ProductType = N'Shoes' AND SizeLabel = N'42'
)
    INSERT INTO dbo.SizeCharts
    (BrandId, ProductType, SizeLabel, MinFootLengthCm, MaxFootLengthCm, FitNote)
    VALUES (@Nike, N'Shoes', N'42', 25.90, 26.70, N'Form tiêu chuẩn.');

IF NOT EXISTS
(
    SELECT 1 FROM dbo.SizeCharts
    WHERE BrandId = @Adidas AND ProductType = N'Shoes' AND SizeLabel = N'42'
)
    INSERT INTO dbo.SizeCharts
    (BrandId, ProductType, SizeLabel, MinFootLengthCm, MaxFootLengthCm, FitNote)
    VALUES (@Adidas, N'Shoes', N'42', 26.00, 26.50, N'Nên tăng nửa size nếu chân bè.');

IF NOT EXISTS
(
    SELECT 1 FROM dbo.SizeCharts
    WHERE BrandId IS NULL AND ProductType = N'Clothing' AND SizeLabel = N'S'
)
    INSERT INTO dbo.SizeCharts
    (
        BrandId, ProductType, SizeLabel,
        MinHeightCm, MaxHeightCm,
        MinWeightKg, MaxWeightKg,
        MinChestCm, MaxChestCm,
        MinWaistCm, MaxWaistCm
    )
    VALUES
    (NULL, N'Clothing', N'S', 155, 165, 45, 55, 80, 88, 64, 72);

IF NOT EXISTS
(
    SELECT 1 FROM dbo.SizeCharts
    WHERE BrandId IS NULL AND ProductType = N'Clothing' AND SizeLabel = N'M'
)
    INSERT INTO dbo.SizeCharts
    (
        BrandId, ProductType, SizeLabel,
        MinHeightCm, MaxHeightCm,
        MinWeightKg, MaxWeightKg,
        MinChestCm, MaxChestCm,
        MinWaistCm, MaxWaistCm
    )
    VALUES
    (NULL, N'Clothing', N'M', 165, 172, 55, 65, 88, 96, 72, 80);

IF NOT EXISTS
(
    SELECT 1 FROM dbo.SizeCharts
    WHERE BrandId IS NULL AND ProductType = N'Clothing' AND SizeLabel = N'L'
)
    INSERT INTO dbo.SizeCharts
    (
        BrandId, ProductType, SizeLabel,
        MinHeightCm, MaxHeightCm,
        MinWeightKg, MaxWeightKg,
        MinChestCm, MaxChestCm,
        MinWaistCm, MaxWaistCm
    )
    VALUES
    (NULL, N'Clothing', N'L', 172, 180, 65, 78, 96, 104, 80, 90);
GO

PRINT N'Đã tạo hoặc cập nhật database SportsStoreAI thành công.';
GO
