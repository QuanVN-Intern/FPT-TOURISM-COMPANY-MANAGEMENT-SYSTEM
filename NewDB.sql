-- ============================================
-- TRAVEL COMPANY DATABASE - FULL SQL SCRIPT
-- SQL Server | Normalized to 3NF
-- ============================================

USE master;
GO
IF DB_ID('TravelCompanyDB') IS NOT NULL DROP DATABASE TravelCompanyDB;
CREATE DATABASE TravelCompanyDB;
GO
USE TravelCompanyDB;
GO

-- ============================================
-- TABLE: Roles
-- ============================================
CREATE TABLE Roles (
    RoleId      INT           PRIMARY KEY IDENTITY(1,1),
    RoleName    NVARCHAR(50)  NOT NULL UNIQUE,
    Description NVARCHAR(255),
    CreatedAt   DATETIME2     NOT NULL DEFAULT GETDATE(),
    UpdatedAt   DATETIME2     NOT NULL DEFAULT GETDATE(),
    IsDeleted   BIT           NOT NULL DEFAULT 0
);

-- ============================================
-- TABLE: Permissions
-- ============================================
CREATE TABLE Permissions (
    PermissionId   INT           PRIMARY KEY IDENTITY(1,1),
    PermissionName NVARCHAR(100) NOT NULL UNIQUE,
    Module         NVARCHAR(50)  NOT NULL,
    Action         NVARCHAR(50)  NOT NULL,
    Description    NVARCHAR(255),
    CONSTRAINT UQ_Perm_Module_Action UNIQUE (Module, Action)
);

-- ============================================
-- TABLE: RolePermissions (RBAC junction)
-- ============================================
CREATE TABLE RolePermissions (
    RoleId       INT NOT NULL,
    PermissionId INT NOT NULL,
    CONSTRAINT PK_RolePerm PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RP_Role  FOREIGN KEY (RoleId)       REFERENCES Roles(RoleId),
    CONSTRAINT FK_RP_Perm  FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId)
);

-- ============================================
-- TABLE: Accounts
-- ============================================
CREATE TABLE Accounts (
    AccountId    INT            PRIMARY KEY IDENTITY(1,1),
    Username     NVARCHAR(50)   NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256)  NOT NULL,
    FullName     NVARCHAR(150)  NOT NULL,
    Email        NVARCHAR(150)  NOT NULL
                 UNIQUE
                 CHECK (Email LIKE '%_@_%._%'),
    RoleId       INT            NOT NULL,
    IsActive     BIT            NOT NULL DEFAULT 1,
    LastLoginAt  DATETIME2,
    CreatedAt    DATETIME2      NOT NULL DEFAULT GETDATE(),
    UpdatedAt    DATETIME2      NOT NULL DEFAULT GETDATE(),
    IsDeleted    BIT            NOT NULL DEFAULT 0,
    CONSTRAINT FK_Acc_Role FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);
CREATE INDEX IX_Accounts_RoleId ON Accounts(RoleId);
CREATE INDEX IX_Accounts_Email  ON Accounts(Email);

-- ============================================
-- TABLE: Customers
-- ============================================
CREATE TABLE Customers (
    CustomerId  INT            PRIMARY KEY IDENTITY(1,1),
    FullName    NVARCHAR(150)  NOT NULL,
    Phone       VARCHAR(15)    NOT NULL UNIQUE
                               CHECK (Phone NOT LIKE '%[^0-9+]%'),
    Email       NVARCHAR(150)
                CHECK (Email LIKE '%_@_%._%'),
    DateOfBirth DATE,
    Address     NVARCHAR(500),
    PassportNo  NVARCHAR(50),
    Notes       NVARCHAR(1000),
    CreatedAt   DATETIME2      NOT NULL DEFAULT GETDATE(),
    UpdatedAt   DATETIME2      NOT NULL DEFAULT GETDATE(),
    IsDeleted   BIT            NOT NULL DEFAULT 0
);
CREATE INDEX IX_Customers_Phone    ON Customers(Phone);
CREATE INDEX IX_Customers_FullName ON Customers(FullName);

-- ============================================
-- TABLE: Destinations
-- ============================================
CREATE TABLE Destinations (
    DestinationId INT            PRIMARY KEY IDENTITY(1,1),
    Name          NVARCHAR(150)  NOT NULL,
    Country       NVARCHAR(100)  NOT NULL,
    Region        NVARCHAR(100),
    Description   NVARCHAR(1000),
    CreatedAt     DATETIME2      NOT NULL DEFAULT GETDATE(),
    UpdatedAt     DATETIME2      NOT NULL DEFAULT GETDATE(),
    IsDeleted     BIT            NOT NULL DEFAULT 0
);

-- ============================================
-- TABLE: Tours
-- ============================================
CREATE TABLE Tours (
    TourId         INT             PRIMARY KEY IDENTITY(1,1),
    TourCode       VARCHAR(20)     NOT NULL UNIQUE,
    TourName       NVARCHAR(200)   NOT NULL,
    DestinationId  INT             NOT NULL,
    DurationDays   INT             NOT NULL CHECK (DurationDays > 0),
    PricePerPerson DECIMAL(18,2)   NOT NULL CHECK (PricePerPerson > 0),
    MaxCapacity    INT             NOT NULL CHECK (MaxCapacity > 0),
    AvailableSlots INT             NOT NULL CHECK (AvailableSlots >= 0),
    DepartureDate  DATE            NOT NULL,
    ReturnDate     AS (DATEADD(DAY, DurationDays - 1, DepartureDate)) PERSISTED,
    Description    NVARCHAR(2000),
    ThumbnailUrl   NVARCHAR(500),
    Status         NVARCHAR(20)    NOT NULL DEFAULT 'Active'
                   CHECK (Status IN ('Active','Inactive','Completed','Cancelled')),
    CreatedAt      DATETIME2       NOT NULL DEFAULT GETDATE(),
    UpdatedAt      DATETIME2       NOT NULL DEFAULT GETDATE(),
    IsDeleted      BIT             NOT NULL DEFAULT 0,
    CONSTRAINT FK_Tour_Dest FOREIGN KEY (DestinationId) REFERENCES Destinations(DestinationId),
    CONSTRAINT CK_Slots_Cap CHECK (AvailableSlots <= MaxCapacity)
);
CREATE INDEX IX_Tours_Code          ON Tours(TourCode);
CREATE INDEX IX_Tours_Name          ON Tours(TourName);
CREATE INDEX IX_Tours_Destination   ON Tours(DestinationId);
CREATE INDEX IX_Tours_DepartureDate ON Tours(DepartureDate);
CREATE INDEX IX_Tours_Price         ON Tours(PricePerPerson);
CREATE INDEX IX_Tours_Status        ON Tours(Status) WHERE IsDeleted = 0;

-- ============================================
-- TABLE: Bookings
-- ============================================
CREATE TABLE Bookings (
    BookingId    INT            PRIMARY KEY IDENTITY(1,1),
    BookingCode  VARCHAR(20)    NOT NULL UNIQUE,
    CustomerId   INT            NOT NULL,
    TourId       INT            NOT NULL,
    AccountId    INT            NOT NULL,
    NumPersons   INT            NOT NULL CHECK (NumPersons > 0),
    TotalAmount  DECIMAL(18,2)  NOT NULL CHECK (TotalAmount > 0),
    BookingDate  DATETIME2      NOT NULL DEFAULT GETDATE(),
    Status       NVARCHAR(20)   NOT NULL DEFAULT 'Confirmed'
                 CHECK (Status IN ('Confirmed','Cancelled','Completed')),
    CancelledAt  DATETIME2,
    CancelReason NVARCHAR(500),
    Notes        NVARCHAR(1000),
    CreatedAt    DATETIME2      NOT NULL DEFAULT GETDATE(),
    UpdatedAt    DATETIME2      NOT NULL DEFAULT GETDATE(),
    IsDeleted    BIT            NOT NULL DEFAULT 0,
    CONSTRAINT FK_Book_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_Book_Tour     FOREIGN KEY (TourId)     REFERENCES Tours(TourId),
    CONSTRAINT FK_Book_Account  FOREIGN KEY (AccountId)  REFERENCES Accounts(AccountId)
);
CREATE INDEX IX_Bookings_Customer ON Bookings(CustomerId);
CREATE INDEX IX_Bookings_Tour     ON Bookings(TourId);
CREATE INDEX IX_Bookings_Status   ON Bookings(Status);
CREATE INDEX IX_Bookings_Date     ON Bookings(BookingDate);

-- ============================================
-- TABLE: Payments
-- ============================================
CREATE TABLE Payments (
    PaymentId      INT            PRIMARY KEY IDENTITY(1,1),
    BookingId      INT            NOT NULL,
    Amount         DECIMAL(18,2)  NOT NULL CHECK (Amount > 0),
    PaymentDate    DATETIME2      NOT NULL DEFAULT GETDATE(),
    PaymentMethod  NVARCHAR(50)   NOT NULL DEFAULT 'Cash'
                   CHECK (PaymentMethod IN ('Cash','BankTransfer','Card','EWallet')),
    Status         NVARCHAR(20)   NOT NULL DEFAULT 'Pending'
                   CHECK (Status IN ('Pending','Paid','Refunded','Failed')),
    TransactionRef NVARCHAR(100),
    PaidAt         DATETIME2,
    Notes          NVARCHAR(500),
    CreatedAt      DATETIME2      NOT NULL DEFAULT GETDATE(),
    UpdatedAt      DATETIME2      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Pay_Booking FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId)
);
CREATE INDEX IX_Payments_Booking ON Payments(BookingId);
CREATE INDEX IX_Payments_Status  ON Payments(Status);
CREATE INDEX IX_Payments_Date    ON Payments(PaymentDate);

-- ============================================
-- TABLE: Vehicles
-- ============================================
CREATE TABLE Vehicles (
    VehicleId   INT          PRIMARY KEY IDENTITY,
    PlateNumber VARCHAR(20)  UNIQUE NOT NULL,
    Capacity    INT          NOT NULL,
    Status      NVARCHAR(20) DEFAULT 'Available',
    Notes       NVARCHAR(255)
);

-- ============================================
-- TABLE: TourVehicles
-- ============================================
CREATE TABLE TourVehicles (
    Id        INT PRIMARY KEY IDENTITY,
    TourId    INT NOT NULL,
    VehicleId INT NOT NULL,
    FOREIGN KEY (TourId)    REFERENCES Tours(TourId),
    FOREIGN KEY (VehicleId) REFERENCES Vehicles(VehicleId)
);

GO

-- ============================================
-- SEED DATA: Roles
-- RoleId 1=Admin  2=Manager  3=Driver
--         4=Receptionist  5=Guide
-- Note: Staff role removed
-- ============================================
INSERT INTO Roles (RoleName, Description) VALUES
('Admin',        N'Toàn quyền hệ thống'),
('Manager',      N'Quản lý tour, khách hàng, báo cáo'),
('Driver',       N'Xem lịch tour được phân công'),
('Receptionist', N'Lễ tân, xem booking và khách hàng'),
('Guide',        N'Hướng dẫn viên, xem tour và khách hàng');

-- ============================================
-- SEED DATA: Permissions
-- ============================================
INSERT INTO Permissions (PermissionName, Module, Action) VALUES
('Tour_Create',    'Tour',     'Create'),
('Tour_Read',      'Tour',     'Read'),
('Tour_Update',    'Tour',     'Update'),
('Tour_Delete',    'Tour',     'Delete'),
('Customer_Create','Customer', 'Create'),
('Customer_Read',  'Customer', 'Read'),
('Customer_Update','Customer', 'Update'),
('Customer_Delete','Customer', 'Delete'),
('Booking_Create', 'Booking',  'Create'),
('Booking_Cancel', 'Booking',  'Cancel'),
('Payment_Manage', 'Payment',  'Manage'),
('Report_View',    'Report',   'View'),
('Account_Manage', 'Account',  'Manage');

-- Admin gets all permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 1, PermissionId FROM Permissions;

-- Manager permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 2, PermissionId FROM Permissions
WHERE PermissionName IN (
    'Tour_Create','Tour_Read','Tour_Update','Tour_Delete',
    'Customer_Create','Customer_Read','Customer_Update',
    'Booking_Create','Booking_Cancel',
    'Payment_Manage','Report_View'
);

-- Driver permissions (view tours/schedules only)
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 3, PermissionId FROM Permissions
WHERE PermissionName IN ('Tour_Read');

-- Receptionist permissions (view customers and bookings)
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 4, PermissionId FROM Permissions
WHERE PermissionName IN (
    'Tour_Read','Customer_Read','Booking_Create','Booking_Cancel'
);

-- Guide permissions (view tours and customers)
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 5, PermissionId FROM Permissions
WHERE PermissionName IN ('Tour_Read','Customer_Read');

GO

-- ============================================
-- SEED DATA: Accounts
-- All passwords = '123456' (SHA-256 hashed)
-- ============================================
DECLARE @pwd NVARCHAR(256) = '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92';

INSERT INTO Accounts (Username, PasswordHash, FullName, Email, RoleId, IsActive, IsDeleted)
VALUES
-- Main accounts
('admin',        @pwd, 'System Admin',  'admin@travel.com',        1, 1, 0),
('manager1',     @pwd, 'John Manager',  'manager1@travel.com',     2, 1, 0),
-- Test accounts (one per role)
('manager_test', @pwd, 'Mary Manager',  'manager_test@travel.com', 2, 1, 0),
('driver_test',  @pwd, 'Dave Driver',   'driver_test@travel.com',  3, 1, 0),
('recept_test',  @pwd, 'Rachel Recept', 'recept_test@travel.com',  4, 1, 0),
('guide_test',   @pwd, 'Gary Guide',    'guide_test@travel.com',   5, 1, 0);

GO

-- ============================================
-- SEED DATA: Destinations
-- ============================================
INSERT INTO Destinations (Name, Country, Region, Description) VALUES
(N'Đà Nẵng',  N'Việt Nam',  N'Miền Trung',   N'Thành phố biển đẹp nổi tiếng'),
('Da Nang',   'Vietnam',    'Central',        'Famous coastal city'),
('Phu Quoc',  'Vietnam',    'South',          'Beautiful island destination'),
('Sapa',      'Vietnam',    'North',          'Mountainous region with rice terraces'),
('Bangkok',   'Thailand',   'Southeast Asia', 'Vibrant capital city'),
('Singapore', 'Singapore',  'Southeast Asia', 'Modern city-state');

-- ============================================
-- SEED DATA: Customers
-- ============================================
INSERT INTO Customers (FullName, Phone, Email, DateOfBirth, Address, PassportNo, Notes) VALUES
(N'Trần Minh Nam', '0987654321', 'namtran@gmail.com', '1998-05-20', N'Hà Nội, Việt Nam', 'P12345678', N'Khách VIP'),
('Michael Nguyen', '0911111111', 'michael@gmail.com', '1995-01-01', 'Hanoi',             NULL, NULL),
('Sarah Tran',     '0922222222', 'sarah@gmail.com',   '1996-02-02', 'Ho Chi Minh City',  NULL, NULL),
('David Le',       '0933333333', 'david@gmail.com',   '1997-03-03', 'Da Nang',           NULL, NULL),
('Emma Pham',      '0944444444', 'emma@gmail.com',    '1998-04-04', 'Hue',               NULL, NULL);

-- ============================================
-- SEED DATA: Tours
-- ============================================
INSERT INTO Tours (
    TourCode, TourName, DestinationId,
    DurationDays, PricePerPerson,
    MaxCapacity, AvailableSlots,
    DepartureDate, Description, ThumbnailUrl
) VALUES
('TOUR001', N'Tour Đà Nẵng 3N2Đ',         1, 3, 3500000, 20, 20, '2026-07-01', N'Tour nghỉ dưỡng biển Đà Nẵng', 'https://example.com/danang.jpg'),
('TOUR002', 'Phu Quoc Island Tour 4D3N',   3, 4, 5500000, 25, 25, '2026-07-05', 'Island exploration',            NULL),
('TOUR003', 'Sapa Mountain Tour 2D1N',     4, 2, 2500000, 15, 15, '2026-07-10', 'Trekking and nature',           NULL),
('TOUR004', 'Bangkok City Tour 5D4N',      5, 5, 8500000, 30, 30, '2026-07-15', 'City and culture',              NULL),
('TOUR005', 'Singapore Premium Tour 3D2N', 6, 3, 9000000, 18, 18, '2026-07-20', 'Luxury travel experience',      NULL);

-- ============================================
-- SEED DATA: Bookings
-- ============================================
INSERT INTO Bookings (
    BookingCode, CustomerId, TourId, AccountId,
    NumPersons, TotalAmount, Status
) VALUES
('BK001', 1, 1, 2, 2,  7000000, 'Confirmed'),
('BK002', 2, 2, 2, 3, 16500000, 'Confirmed'),
('BK003', 3, 3, 2, 1,  2500000, 'Cancelled'),
('BK004', 4, 1, 2, 4, 14000000, 'Confirmed');