-- Create UserRoles table for storing synchronized data from OneIDM
CREATE TABLE UserRoles (
    Id int IDENTITY(1,1) NOT NULL,
    DomainId nvarchar(255) NOT NULL,
    Role nvarchar(255) NOT NULL,
    Module nvarchar(255) NOT NULL,
    CreatedAt datetime2 NOT NULL,
    UpdatedAt datetime2 NOT NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (Id)
);

-- Create indexes for better query performance
CREATE INDEX IX_UserRoles_Module ON UserRoles (Module);
CREATE INDEX IX_UserRoles_DomainId ON UserRoles (DomainId);
CREATE INDEX IX_UserRoles_Role ON UserRoles (Role);

-- Create unique constraint to prevent duplicate entries
CREATE UNIQUE INDEX IX_UserRoles_Unique ON UserRoles (DomainId, Role, Module);