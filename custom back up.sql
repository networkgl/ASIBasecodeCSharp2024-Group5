use EcoFridgeDB
go

select RoleName, *  from VwUsersRoleView


select * from [User]

insert into UserRole
values(18, 1)

select * from Role

BACKUP DATABASE EcoFridgeDB 
TO DISK = 'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\EcoFridgeDB.bak' 
WITH BLOCKSIZE = 4096;

BACKUP DATABASE TicketingSystemDB 
TO DISK = 'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\TicketingSystemDB.bak' 
WITH BLOCKSIZE = 4096;