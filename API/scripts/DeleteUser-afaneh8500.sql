-- Remove test user afaneh8500@gmail.com so you can re-test registration.
-- Run this in SQL Server Management Studio or: sqlcmd -S . -d MabaDb -i DeleteUser-afaneh8500.sql
-- (Adjust -S and -d if your server/database name differs.)

BEGIN TRANSACTION;

DECLARE @UserId UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Email = 'afaneh8500@gmail.com');

IF @UserId IS NULL
BEGIN
    PRINT 'No user found with email afaneh8500@gmail.com.';
    ROLLBACK;
    RETURN;
END

-- Remove role assignments first (FK from UserRoles to Users)
DELETE FROM UserRoles WHERE UserId = @UserId;

-- Remove the user
DELETE FROM Users WHERE Id = @UserId;

PRINT 'User afaneh8500@gmail.com and their role assignments have been deleted.';

COMMIT;
