USE TravelCompanyDB;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Accounts') AND name = 'LicenseNumber')
BEGIN
    ALTER TABLE Accounts ADD LicenseNumber NVARCHAR(50);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('TourAssignments') AND type = 'U')
BEGIN
    CREATE TABLE TourAssignments (
        AssignmentId INT PRIMARY KEY IDENTITY(1,1),
        TourId INT NOT NULL,
        AccountId INT NOT NULL, -- The Driver
        VehicleId INT NOT NULL,
        FOREIGN KEY (TourId) REFERENCES Tours(TourId),
        FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
        FOREIGN KEY (VehicleId) REFERENCES Vehicles(VehicleId),
        CONSTRAINT UQ_Tour_Driver UNIQUE (TourId, AccountId)
    );
END
GO
