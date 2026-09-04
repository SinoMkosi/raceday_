/* ============================================================
   RaceDay System - Database Schema & Seed Data
   Target: Microsoft SQL Server (SSMS)
   This script matches /docs/erd.png exactly (see README for notes)
   ============================================================ */

IF DB_ID('RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END
GO

USE RaceDayDB;
GO

/* ------------------------------------------------------------
   Drop tables if they already exist (child -> parent order)
   ------------------------------------------------------------ */
IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID('dbo.Enrolments', 'U') IS NOT NULL DROP TABLE dbo.Enrolments;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL DROP TABLE dbo.Roles;
GO

/* ------------------------------------------------------------
   1. Roles
   ------------------------------------------------------------ */
CREATE TABLE dbo.Roles (
    RoleID      INT IDENTITY(1,1) PRIMARY KEY,
    RoleName    VARCHAR(20) NOT NULL UNIQUE
        CONSTRAINT CK_Roles_RoleName CHECK (RoleName IN ('Admin', 'Organiser', 'Participant'))
);
GO

/* ------------------------------------------------------------
   2. Users
   ------------------------------------------------------------ */
CREATE TABLE dbo.Users (
    UserID          INT IDENTITY(1,1) PRIMARY KEY,
    RoleID          INT NOT NULL,
    FullName        VARCHAR(100) NOT NULL,
    Email           VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash    VARCHAR(255) NOT NULL,
    PhoneNumber     VARCHAR(20)  NULL,
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES dbo.Roles(RoleID)
);
GO

/* ------------------------------------------------------------
   3. Events
   ------------------------------------------------------------ */
CREATE TABLE dbo.Events (
    EventID         INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID     INT NOT NULL,
    EventName       VARCHAR(150) NOT NULL,
    Description     VARCHAR(1000) NULL,
    EventDate       DATE NOT NULL,
    Location        VARCHAR(150) NOT NULL,
    Status          VARCHAR(20) NOT NULL DEFAULT 'Planned'
        CONSTRAINT CK_Events_Status CHECK (Status IN ('Planned', 'Open', 'Closed', 'Cancelled', 'Completed')),
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Events_Organiser FOREIGN KEY (OrganiserID) REFERENCES dbo.Users(UserID)
);
GO

/* ------------------------------------------------------------
   4. Categories
   ------------------------------------------------------------ */
CREATE TABLE dbo.Categories (
    CategoryID      INT IDENTITY(1,1) PRIMARY KEY,
    EventID         INT NOT NULL,
    CategoryName    VARCHAR(100) NOT NULL,
    DistanceKM      DECIMAL(6,2) NULL,
    MaxParticipants INT NOT NULL,
    EntryFee        DECIMAL(8,2) NOT NULL DEFAULT 0,
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventID) REFERENCES dbo.Events(EventID),
    CONSTRAINT CK_Categories_MaxParticipants CHECK (MaxParticipants > 0)
);
GO

/* ------------------------------------------------------------
   5. Enrolments
   ------------------------------------------------------------ */
CREATE TABLE dbo.Enrolments (
    EnrolmentID     INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID   INT NOT NULL,
    CategoryID      INT NOT NULL,
    BibNumber       VARCHAR(10) NULL UNIQUE,
    EnrolmentDate   DATETIME NOT NULL DEFAULT GETDATE(),
    Status          VARCHAR(20) NOT NULL DEFAULT 'Pending'
        CONSTRAINT CK_Enrolments_Status CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled')),
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryID) REFERENCES dbo.Categories(CategoryID),
    CONSTRAINT UQ_Enrolments_Participant_Category UNIQUE (ParticipantID, CategoryID)
);
GO

/* ------------------------------------------------------------
   6. Results  (1:1 with Enrolments)
   ------------------------------------------------------------ */
CREATE TABLE dbo.Results (
    ResultID        INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID     INT NOT NULL UNIQUE,
    FinishTime      TIME NULL,
    Position        INT NULL,
    Status          VARCHAR(20) NOT NULL DEFAULT 'Not Started'
        CONSTRAINT CK_Results_Status CHECK (Status IN ('Not Started', 'DNF', 'DSQ', 'Finished')),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentID) REFERENCES dbo.Enrolments(EnrolmentID)
);
GO

/* ============================================================
   SEED DATA
   ============================================================ */

-- Roles
INSERT INTO dbo.Roles (RoleName) VALUES ('Admin'), ('Organiser'), ('Participant');
GO

-- Users: 2 Organisers, 2 Participants (+1 admin for completeness)
INSERT INTO dbo.Users (RoleID, FullName, Email, PasswordHash, PhoneNumber)
VALUES
    ((SELECT RoleID FROM dbo.Roles WHERE RoleName = 'Admin'),       'System Admin',    'admin@raceday.co.za',     'HASH_ADMIN_0001',  '0110000000'),
    ((SELECT RoleID FROM dbo.Roles WHERE RoleName = 'Organiser'),   'Naledi Khumalo',  'naledi@raceday.co.za',    'HASH_ORG_0001',     '0821112222'),
    ((SELECT RoleID FROM dbo.Roles WHERE RoleName = 'Organiser'),   'Johan van Wyk',   'johan@raceday.co.za',     'HASH_ORG_0002',     '0823334444'),
    ((SELECT RoleID FROM dbo.Roles WHERE RoleName = 'Participant'), 'Sinoxolo Mkosi',  'sino@raceday.co.za',      'HASH_PART_0001',    '0835556666'),
    ((SELECT RoleID FROM dbo.Roles WHERE RoleName = 'Participant'), 'Priya Naidoo',    'priya@raceday.co.za',     'HASH_PART_0002',    '0837778888');
GO

-- Events: 3 events, owned by the 2 organisers
INSERT INTO dbo.Events (OrganiserID, EventName, Description, EventDate, Location, Status)
VALUES
    ((SELECT UserID FROM dbo.Users WHERE Email = 'naledi@raceday.co.za'), 'Johannesburg City Marathon', 'Annual road marathon through the city centre.', '2026-11-08', 'Johannesburg, Gauteng', 'Open'),
    ((SELECT UserID FROM dbo.Users WHERE Email = 'naledi@raceday.co.za'), 'Soweto 10K Fun Run',          'Community fun run supporting local schools.',   '2026-10-03', 'Soweto, Gauteng',       'Open'),
    ((SELECT UserID FROM dbo.Users WHERE Email = 'johan@raceday.co.za'),  'Cape Peninsula Cycle Tour',   'Scenic road cycling event around the peninsula.','2026-12-05', 'Cape Town, Western Cape','Planned');
GO

-- Categories: at least one per event
INSERT INTO dbo.Categories (EventID, CategoryName, DistanceKM, MaxParticipants, EntryFee)
VALUES
    ((SELECT EventID FROM dbo.Events WHERE EventName = 'Johannesburg City Marathon'), 'Full Marathon', 42.20, 2000, 350.00),
    ((SELECT EventID FROM dbo.Events WHERE EventName = 'Johannesburg City Marathon'), 'Half Marathon', 21.10, 3000, 250.00),
    ((SELECT EventID FROM dbo.Events WHERE EventName = 'Soweto 10K Fun Run'),         '10K Open',      10.00, 1500, 120.00),
    ((SELECT EventID FROM dbo.Events WHERE EventName = 'Soweto 10K Fun Run'),         '5K Fun Walk',    5.00, 1000,  60.00),
    ((SELECT EventID FROM dbo.Events WHERE EventName = 'Cape Peninsula Cycle Tour'),  '100K Race',    100.00,  800, 450.00);
GO

-- Enrolments: sample sign-ups
INSERT INTO dbo.Enrolments (ParticipantID, CategoryID, BibNumber, Status)
VALUES
    ((SELECT UserID FROM dbo.Users WHERE Email = 'sino@raceday.co.za'),
     (SELECT CategoryID FROM dbo.Categories WHERE CategoryName = 'Half Marathon'), 'JCM-1001', 'Confirmed'),
    ((SELECT UserID FROM dbo.Users WHERE Email = 'priya@raceday.co.za'),
     (SELECT CategoryID FROM dbo.Categories WHERE CategoryName = 'Full Marathon'), 'JCM-2001', 'Confirmed'),
    ((SELECT UserID FROM dbo.Users WHERE Email = 'sino@raceday.co.za'),
     (SELECT CategoryID FROM dbo.Categories WHERE CategoryName = '10K Open'),      'SOW-3001', 'Pending');
GO

-- Results: sample result for a confirmed enrolment
INSERT INTO dbo.Results (EnrolmentID, FinishTime, Position, Status)
VALUES
    ((SELECT EnrolmentID FROM dbo.Enrolments WHERE BibNumber = 'JCM-1001'), '01:42:37', 15, 'Finished');
GO
