-- Remove test users so you can re-test registration with the same emails.
-- Run: sqlcmd -S . -d MabaDb -E -i DeleteTestUsers.sql

BEGIN TRANSACTION;

-- Delete UserRoles then Users for cfrctrcfr2004@gmail.com
DELETE FROM UserRoles WHERE UserId IN (SELECT Id FROM Users WHERE Email = 'cfrctrcfr2004@gmail.com');
DELETE FROM Users WHERE Email = 'cfrctrcfr2004@gmail.com';

-- Delete UserRoles then Users for afaneh8500@gmail.com
DELETE FROM UserRoles WHERE UserId IN (SELECT Id FROM Users WHERE Email = 'afaneh8500@gmail.com');
DELETE FROM Users WHERE Email = 'afaneh8500@gmail.com';

COMMIT;
