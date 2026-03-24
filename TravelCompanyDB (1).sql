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
    CONSTRAINT PK_RolePerm   PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RP_Role    FOREIGN KEY (RoleId)
                             REFERENCES Roles(RoleId),
    CONSTRAINT FK_RP_Perm    FOREIGN KEY (PermissionId)
                             REFERENCES Permissions(PermissionId)
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
    CONSTRAINT FK_Acc_Role FOREIGN KEY (RoleId)
                           REFERENCES Roles(RoleId)
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
    TourId          INT             PRIMARY KEY IDENTITY(1,1),
    TourCode        VARCHAR(20)     NOT NULL UNIQUE,
    TourName        NVARCHAR(200)   NOT NULL,
    DestinationId   INT             NOT NULL,
    DurationDays    INT             NOT NULL
                    CHECK (DurationDays > 0),
    PricePerPerson  DECIMAL(18,2)   NOT NULL
                    CHECK (PricePerPerson > 0),
    MaxCapacity     INT             NOT NULL
                    CHECK (MaxCapacity > 0),
    AvailableSlots  INT             NOT NULL
                    CHECK (AvailableSlots >= 0),
    DepartureDate   DATE            NOT NULL,
    ReturnDate      AS (DATEADD(DAY, DurationDays - 1, DepartureDate)) PERSISTED,
    Description     NVARCHAR(2000),
    ThumbnailUrl    NVARCHAR(500),
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Active'
                    CHECK (Status IN ('Active','Inactive','Completed','Cancelled')),
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    IsDeleted       BIT             NOT NULL DEFAULT 0,
    CONSTRAINT FK_Tour_Dest FOREIGN KEY (DestinationId)
                            REFERENCES Destinations(DestinationId),
    CONSTRAINT CK_Slots_Cap CHECK (AvailableSlots <= MaxCapacity)
);
CREATE INDEX IX_Tours_Code          ON Tours(TourCode);
CREATE INDEX IX_Tours_Name          ON Tours(TourName);
CREATE INDEX IX_Tours_Destination   ON Tours(DestinationId);
CREATE INDEX IX_Tours_DepartureDate ON Tours(DepartureDate);
CREATE INDEX IX_Tours_Price         ON Tours(PricePerPerson);
CREATE INDEX IX_Tours_Status        ON Tours(Status)
             WHERE IsDeleted = 0;

-- ============================================
-- TABLE: Bookings
-- ============================================
CREATE TABLE Bookings (
    BookingId       INT             PRIMARY KEY IDENTITY(1,1),
    BookingCode     VARCHAR(20)     NOT NULL UNIQUE,
    CustomerId      INT             NOT NULL,
    TourId          INT             NOT NULL,
    AccountId       INT             NOT NULL,
    NumPersons      INT             NOT NULL
                    CHECK (NumPersons > 0),
    TotalAmount     DECIMAL(18,2)   NOT NULL
                    CHECK (TotalAmount > 0),
    BookingDate     DATETIME2       NOT NULL DEFAULT GETDATE(),
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Confirmed'
                    CHECK (Status IN
                          ('Confirmed','Cancelled','Completed')),
    CancelledAt     DATETIME2,
    CancelReason    NVARCHAR(500),
    Notes           NVARCHAR(1000),
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),
    IsDeleted       BIT             NOT NULL DEFAULT 0,
    CONSTRAINT FK_Book_Customer FOREIGN KEY (CustomerId)
                                REFERENCES Customers(CustomerId),
    CONSTRAINT FK_Book_Tour     FOREIGN KEY (TourId)
                                REFERENCES Tours(TourId),
    CONSTRAINT FK_Book_Account  FOREIGN KEY (AccountId)
                                REFERENCES Accounts(AccountId)
);
CREATE INDEX IX_Bookings_Customer ON Bookings(CustomerId);
CREATE INDEX IX_Bookings_Tour     ON Bookings(TourId);
CREATE INDEX IX_Bookings_Status   ON Bookings(Status);
CREATE INDEX IX_Bookings_Date     ON Bookings(BookingDate);


-- ============================================
-- TABLE: Payments
-- ============================================
CREATE TABLE Payments (
    PaymentId     INT            PRIMARY KEY IDENTITY(1,1),
    BookingId     INT            NOT NULL,
    Amount        DECIMAL(18,2)  NOT NULL
                  CHECK (Amount > 0),
    PaymentDate   DATETIME2      NOT NULL DEFAULT GETDATE(),
    PaymentMethod NVARCHAR(50)   NOT NULL DEFAULT 'Cash'
                  CHECK (PaymentMethod IN
                        ('Cash','BankTransfer','Card','EWallet')),
    Status        NVARCHAR(20)   NOT NULL DEFAULT 'Pending'
                  CHECK (Status IN ('Pending','Paid','Refunded','Failed')),
    TransactionRef NVARCHAR(100),
    PaidAt        DATETIME2,
    Notes         NVARCHAR(500),
    CreatedAt     DATETIME2      NOT NULL DEFAULT GETDATE(),
    UpdatedAt     DATETIME2      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Pay_Booking FOREIGN KEY (BookingId)
                              REFERENCES Bookings(BookingId)
);
CREATE INDEX IX_Payments_Booking ON Payments(BookingId);
CREATE INDEX IX_Payments_Status  ON Payments(Status);
CREATE INDEX IX_Payments_Date    ON Payments(PaymentDate);

-- == Verhicles ===
             CREATE TABLE Vehicles (
    VehicleId INT PRIMARY KEY IDENTITY,
    PlateNumber VARCHAR(20) UNIQUE NOT NULL,
    Capacity INT NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Available',
    Notes NVARCHAR(255)
);

-- == Tour Verhicles ==
    CREATE TABLE TourVehicles (
    Id INT PRIMARY KEY IDENTITY,
    TourId INT NOT NULL,
    VehicleId INT NOT NULL,

    FOREIGN KEY (TourId) REFERENCES Tours(TourId),
    FOREIGN KEY (VehicleId) REFERENCES Vehicles(VehicleId)
);

ALTER TABLE Accounts ADD LicenseNumber NVARCHAR(50);

CREATE TABLE TourAssignments (
    AssignmentId INT PRIMARY KEY IDENTITY(1,1),
    TourId INT NOT NULL,
    AccountId INT NOT NULL, -- The Driver
    VehicleId INT NOT NULL,
    FOREIGN KEY (TourId) REFERENCES Tours(TourId),
    FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
    FOREIGN KEY (VehicleId) REFERENCES Vehicles(VehicleId),
    CONSTRAINT UQ_Tour_Driver UNIQUE (TourId, AccountId) -- Prevent duplicate assignment
);





-- ============================================
-- SEED DATA
-- ============================================
INSERT INTO Roles (RoleName, Description) VALUES
('Admin',    N'Toàn quyền hệ thống'),
('Manager',  N'Quản lý khách hàng, xem báo cáo'),
('Staff',    N'Tạo booking, không xóa tour'),
('Driver',   N'Xem lịch tour được phân công'),
('Receptionist', N'Lễ tân, tạo booking cơ bản');


INSERT INTO Permissions (PermissionName, Module, Action) VALUES
('Tour_Create',    'Tour',    'Create'),
('Tour_Read',      'Tour',    'Read'),
('Tour_Update',    'Tour',    'Update'),
('Tour_Delete',    'Tour',    'Delete'),
('Customer_Create','Customer','Create'),
('Customer_Read',  'Customer','Read'),
('Customer_Update','Customer','Update'),
('Customer_Delete','Customer','Delete'),
('Booking_Create', 'Booking', 'Create'),
('Booking_Cancel', 'Booking', 'Cancel'),
('Payment_Manage', 'Payment', 'Manage'),
('Report_View',    'Report',  'View'),
('Account_Manage', 'Account', 'Manage');


-- Admin gets all permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 1, PermissionId FROM Permissions;
-- ============================================
-- VIEW: Booking Summary
-- ============================================
GO

INSERT INTO Customers (FullName, Phone, Email, DateOfBirth, Address, PassportNo, Notes)
VALUES (
    N'Trần Minh Nam',
    '0987654321',
    'namtran@gmail.com',
    '1998-05-20',
    N'Hà Nội, Việt Nam',
    'P12345678',
    N'Khách VIP'
);


INSERT INTO Destinations (Name, Country, Region, Description)
VALUES (
    N'Đà Nẵng',
    N'Việt Nam',
    N'Miền Trung',
    N'Thành phố biển đẹp nổi tiếng'
);


INSERT INTO Tours (
    TourCode, TourName, DestinationId,
    DurationDays, PricePerPerson,
    MaxCapacity, AvailableSlots,
    DepartureDate, Description, ThumbnailUrl
)
VALUES (
    'TOUR001',
    N'Tour Đà Nẵng 3N2Đ',
    1,                -- DestinationId
    3,
    3500000,
    20,
    20,
    '2026-04-01',
    N'Tour nghỉ dưỡng biển Đà Nẵng',
    'https://example.com/danang.jpg'
);
INSERT INTO Destinations (Name, Country, Region, Description) VALUES
('Da Nang', 'Vietnam', 'Central', 'Famous coastal city'),
('Phu Quoc', 'Vietnam', 'South', 'Beautiful island destination'),
('Sapa', 'Vietnam', 'North', 'Mountainous region with rice terraces'),
('Bangkok', 'Thailand', 'Southeast Asia', 'Vibrant capital city'),
('Singapore', 'Singapore', 'Southeast Asia', 'Modern city-state');



INSERT INTO Accounts (Username, PasswordHash, FullName, Email, RoleId)
VALUES
('admin', '123456', 'System Admin', 'admin@travel.com', 1),
('manager1', '123456', 'John Manager', 'manager@travel.com', 2),
('staff1', '123456', 'Alice Staff', 'staff@travel.com', 3);

INSERT INTO Customers (FullName, Phone, Email, DateOfBirth, Address)
VALUES
('Michael Nguyen', '0911111111', 'michael@gmail.com', '1995-01-01', 'Hanoi'),
('Sarah Tran', '0922222222', 'sarah@gmail.com', '1996-02-02', 'Ho Chi Minh City'),
('David Le', '0933333333', 'david@gmail.com', '1997-03-03', 'Da Nang'),
('Emma Pham', '0944444444', 'emma@gmail.com', '1998-04-04', 'Hue');


INSERT INTO Tours (
    TourCode, TourName, DestinationId,
    DurationDays, PricePerPerson,
    MaxCapacity, AvailableSlots,
    DepartureDate, Description
)
VALUES
('TOUR002', 'Phu Quoc Island Tour 4D3N', 2, 4, 5500000, 25, 25, '2026-04-05', 'Island exploration'),
('TOUR003', 'Sapa Mountain Tour 2D1N', 3, 2, 2500000, 15, 15, '2026-04-10', 'Trekking and nature'),
('TOUR004', 'Bangkok City Tour 5D4N', 4, 5, 8500000, 30, 30, '2026-04-15', 'City and culture'),
('TOUR005', 'Singapore Premium Tour 3D2N', 5, 3, 9000000, 18, 18, '2026-04-20', 'Luxury travel experience');


INSERT INTO Bookings (
    BookingCode, CustomerId, TourId, AccountId,
    NumPersons, TotalAmount, Status
)
VALUES
('BK002', 2, 2, 3, 3, 16500000, 'Confirmed'),
('BK003', 3, 3, 3, 1, 2500000, 'Cancelled'),
('BK004', 4, 1, 3, 4, 14000000, 'Confirmed');

-- ============================================
-- SEED DATA: Test accounts for all roles
-- Password for all accounts: 123456
-- SHA-256 hash of '123456'
-- ============================================
DECLARE @pwd NVARCHAR(256) = '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92';

-- Update existing seeded accounts to use hashed password
UPDATE Accounts SET PasswordHash = @pwd WHERE PasswordHash = '123456';

-- Manager
INSERT INTO Accounts (Username, PasswordHash, FullName, Email, RoleId, IsActive, IsDeleted)
VALUES ('manager_test', @pwd, 'Mary Manager', 'manager_test@travel.com', 2, 1, 0);

-- Staff
INSERT INTO Accounts (Username, PasswordHash, FullName, Email, RoleId, IsActive, IsDeleted)
VALUES ('staff_test', @pwd, 'Steve Staff', 'staff_test@travel.com', 3, 1, 0);

-- Driver
INSERT INTO Accounts (Username, PasswordHash, FullName, Email, RoleId, IsActive, IsDeleted)
VALUES ('driver_test', @pwd, 'Dave Driver', 'driver_test@travel.com', 4, 1, 0);

-- Receptionist
INSERT INTO Accounts (Username, PasswordHash, FullName, Email, RoleId, IsActive, IsDeleted)
VALUES ('recept_test', @pwd, 'Rachel Recept', 'recept_test@travel.com', 5, 1, 0);

INSERT INTO Vehicles (PlateNumber, Capacity, Status, Notes)
VALUES
('29A-12345', 4, 'Available', N'Toyota Vios'),

('30A-67890', 7, 'Available', N'Toyota Innova'),

('30B-22222', 16, 'Available', N'Ford Transit'),

('29B-99999', 29, 'Maintenance', N'Hyundai County'),

('30C-55555', 45, 'Available', N'Hyundai Universe'),

('29D-11111', 7, 'Busy', N'Used for HaLong tour'),

('30E-88888', 16, 'Available', N'New vehicle'),

('29F-77777', 4, 'Available', NULL),

('30G-33333', 29, 'Busy', N'Sapa trip'),

('30H-44444', 45, 'Maintenance', N'Engine repair');