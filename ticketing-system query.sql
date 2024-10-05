create database TicketingSystemDB
go

use TicketingSystemDB
go

create table [User] 
(
	UserId int identity(1,1) primary key,
	[Name] varchar(100) unique,
	Email varchar(100) unique,
	[Password] varchar(50)
)
go

create table [Role]
(
	RoleId int identity(1,1) primary key,
	RoleName varchar(100) unique
)
go

create table UserRole
(
	UserRoleId int identity(1,1) primary key,
	UserId int,
	RoleId int,
	foreign key(UserId) references [User](UserId) on delete cascade,
	foreign key(RoleId) references [Role](RoleId) on delete cascade
)
go

alter table UserRole
add primary key (UserRoleId)
go

create table Ticket
(
	TicketId int identity(1,1) primary key,
	CategoryId int,
	PriorityId int,
	StatusId int,
	IssueDescription varchar(max),
	AttachmentPath varchar(max),
	CreateAt datetime default CURRENT_TIMESTAMP,
	LastModified datetime default CURRENT_TIMESTAMP,
	foreign key(CategoryId) references Category(CategoryId) on delete cascade,
	foreign key(PriorityId) references [Priority](PriorityId) on delete cascade,
	foreign key([StatusId]) references [Status](StatusId) on delete cascade,
)
go

create table [Priority]
(
	PriorityId int identity(1,1) primary key,
	PriorityName varchar(100) unique,
)
go

create table Category
(
	CategoryId int identity(1,1) primary key,
	CategoryName varchar(100) unique,
)
go

create table [Status]
(
	StatusId int identity(1,1) primary key,
	StatusName varchar(100) unique
)
go

create table Article
(
	ArticleId int identity(1,1) primary key,
	UserId int,
	Content varchar(max),
	foreign key(UserId) references [User](UserId) on delete cascade
)
go

create table Feedback
(
	FeedbackId int identity(1,1) primary key,
	UserId int,
	FeedbackText varchar(max),
	foreign key(UserId) references  [User](UserId) on delete cascade
)
go

create table UserTicket
(
	UserTicketId int identity(1,1) primary key,
	UserId int,
	TicketId int,
	foreign key(UserId) references [User](UserId) on delete cascade,
	foreign key(TicketId) references Ticket(TicketId) on delete cascade,
)
go

create table Resolve
(
	ResolveId int identity(1,1) primary key,
	UserTicketId int,
	AdminId int,
	AgentId int,
	DateAssigned datetime default current_timestamp,
	LastModified datetime default current_timestamp,
	foreign key (UserTicketId) references UserTicket(UserTicketId) on delete cascade,
	foreign key (AdminId) references [User](UserId),
	foreign key (AgentId) references [User](UserId),
)
go

create table Notification
(
	NotificationId int identity(1,1) primary key,
	CreatorId int,
	UserTicketId int,
	Content varchar(max),
	CreatedAt datetime default current_timestamp,
	LastModified datetime default current_timestamp,
	foreign key (CreatorId) references [User](UserId) on delete cascade,
	foreign key (UserTicketId) references [UserTicket](UserTicketId),
)
go

select * from [User]
select * from [Role]
select * from UserRole

insert into [User]
values('admin', 'admin@gmail.com', 'admin123')

update [User] set [Name] = 'superadmin', Email = 'superadmin@gmail.com', [Password] = 'superadmin123' where UserId = 1

insert into UserRole
values(1, 4)

insert into UserRole
values(3, 3)

insert into [User]
values('lagare', 'lagare@gmail.com', 'lagare123')

insert into UserRole
values(2, 1)
go

create view vw_UserRoleView
as
select ur.UserRoleId, u.*, r.RoleId, r.RoleName
from [User] u
inner join UserRole ur
on u.UserId = ur.UserId
inner join [Role] r
on r.RoleId = ur.UserRoleId

create view vw_AdminUsersView
as
select u.*, ur.RoleId, ur.UserRoleId, r.RoleName
from [User] u
inner join UserRole ur
on u.UserId = ur.UserId
inner join [Role] r
on r.RoleId = ur.RoleId
where ur.RoleId = 3

create view vw_UsersAndAgentsView
as
select u.*, ur.RoleId, ur.UserRoleId, r.RoleName
from [User] u
inner join UserRole ur
on u.UserId = ur.UserId
inner join [Role] r
on r.RoleId = ur.RoleId
where ur.RoleId in(2, 1)
go

insert into [User]
values('testagent', 'testagent@gmail.com', 'lord1212')
select * from [User]

insert into UserRole
values(4, 2)
select * from vw_UsersAndAgentsView

select * from [Role]

update UserRole
set RoleId = 3 where UserId = 3

select * from [User]
go;

alter view vw_UserRoleView
as
select u.*, r.RoleId, r.RoleName, ur.UserRoleId
from [User] u
inner join UserRole ur
on u.UserId = ur.UserId
inner join [Role] r
on r.RoleId = ur.RoleId
go;

select * from Ticket
go;

select * from [Priority]
select * from [Status]

alter view vw_UserTicketView
as
select c.CategoryName, s.StatusName, t.*, ut.UserTicketId, u.*
from Ticket t
inner join UserTicket ut
on t.TicketId = ut.TicketId
inner join [User] u
on u.UserId = ut.UserId
inner join Category c
on c.CategoryId = t.CategoryId
inner join [Status] s
on s.StatusId = t.StatusId
go

select * from vw_UserRoleView
order by RoleName
go

select * from vw_UserTicketView
go;

insert into [Priority]
values ('Critical'), ('High'), ('Normal'), ('Low')
go

insert into [Category]
values ('Technical Issues'), ('Billing and Payments'), ('Product Inquiries'), ('Complaints and Feedback'), ('Account Management'), ('Policy Questions')
go

select * from[Status]

insert into [Status]
values ('Open'), ('In Progress'), ('Resolved'), ('Closed'), ('Unopened')

select * from vw_UserRoleView
order by RoleName

alter view vw_UserTicketViewForAdminsAndAgents
as
select p.PriorityName, c.CategoryName, s.StatusName, t.*, ut.UserTicketId, u.*
from Ticket t
inner join UserTicket ut
on t.TicketId = ut.TicketId
inner join [User] u
on u.UserId = ut.UserId
inner join Category c
on c.CategoryId = t.CategoryId
left join [Priority] p
on p.PriorityId = t.PriorityId
inner join [Status] s
on s.StatusId = t.StatusId
go

select * from Ticket
go

select * from Resolve
go

alter view vw_TicketAssignment
as
select r.ResolveId, t.TicketId, t.IssueDescription, userRep.UserId 'ReporterId', userRep.Name 'Reporter', userRep.Email 'ReporterEmail', userRep.Password 'ReporterPassword', userAd.UserId 'AdminId', userAd.Email 'AdminEmail', userAd.Name 'AdminName', userAd.Password 'AdminPassword', userAg.UserId 'AgentId', userAg.Email 'AgentEmail', userAg.Name 'AgentName', userAg.Password 'AgentPassword', r.DateAssigned, r.LastModified 'ResolveLastModified', p.*, s.*, c.*, t.LastModified 'TicketLastModified', t.CreateAt, t.AttachmentPath, ut.UserTicketId
from Ticket t
inner join UserTicket ut
on ut.TicketId = t.TicketId
left join Resolve r
on r.UserTicketId = ut.UserTicketId
left join [User] userAd
on r.AdminId = r.AdminId
left join [User] userAg
on r.AgentId = userAg.UserId
left join [Priority] p
on p.PriorityId = t.PriorityId
left join [Status] s
on s.StatusId = t.StatusId
left join [Category] c
on c.CategoryId = t.CategoryId
left join [User] userRep
on userRep.UserId = ut.UserId
go

ALTER VIEW vw_TicketAssignment
AS
SELECT 
    r.ResolveId, 
    t.TicketId, 
    t.IssueDescription, 
    userRep.UserId AS ReporterId, 
    userRep.Name AS Reporter, 
    userRep.Email AS ReporterEmail, 
    userRep.Password AS ReporterPassword, 
    userAd.UserId AS AssignerId, 
    userAd.Email AS AssignerEmail, 
    userAd.Name AS AssignerName, 
    userAd.Password AS AssignerPassword, 
    userAg.UserId AS AgentId, 
    userAg.Email AS AgentEmail, 
    userAg.Name AS AgentName, 
    userAg.Password AS AgentPassword, 
    r.DateAssigned, 
    r.LastModified AS ResolveLastModified, 
    p.PriorityId, p.PriorityName, 
    s.StatusId, s.StatusName, 
    c.CategoryId, c.CategoryName, 
    t.LastModified AS TicketLastModified, 
    t.CreateAt, 
    t.AttachmentPath, 
    ut.UserTicketId
FROM 
    Ticket t
INNER JOIN 
    UserTicket ut ON ut.TicketId = t.TicketId
LEFT JOIN 
    Resolve r ON r.UserTicketId = ut.UserTicketId
LEFT JOIN 
    [User] userAd ON r.AssignerId = userAd.UserId
LEFT JOIN 
    [User] userAg ON r.AgentId = userAg.UserId
LEFT JOIN 
    [Priority] p ON p.PriorityId = t.PriorityId
LEFT JOIN 
    [Status] s ON s.StatusId = t.StatusId
LEFT JOIN 
    [Category] c ON c.CategoryId = t.CategoryId
LEFT JOIN 
    [User] userRep ON userRep.UserId = ut.UserId
GROUP BY 
    r.ResolveId, t.TicketId, t.IssueDescription, 
    userRep.UserId, userRep.Name, userRep.Email, userRep.Password, 
    userAd.UserId, userAd.Email, userAd.Name, userAd.Password, 
    userAg.UserId, userAg.Email, userAg.Name, userAg.Password, 
    r.DateAssigned, r.LastModified, 
    p.PriorityId, p.PriorityName, 
    s.StatusId, s.StatusName, 
    c.CategoryId, c.CategoryName, 
    t.LastModified, t.CreateAt, 
    t.AttachmentPath, ut.UserTicketId;
GO


select * from vw_UserRoleView
order by RoleName
go

select * from [Priority]
go

insert into [Priority]
values ('Not Set')

insert into [Priority]
values ('Not Set')
go

select * from Ticket
go

select * from [Status]
go

select * from Resolve
go

select * from vw_TicketAssignment
go

select * from [Notification]

alter table [Notification]
add HasRead bit default 0

alter table [Notification]
add LastModified DateTime default getdate()
go;

select * from [Role]
go

create view vw_AdminCount
as
select Count(*) 'Total Admin Count'
from [User] u
inner join UserRole ur on u.UserId = ur.UserId
inner join [Role] r on r.RoleId = ur.RoleId
where ur.RoleId = 3
go

select * from AssignedTicket
go

select * from vw_UserRoleView
go

select * from vw_TicketAssignment
go

select * from vw_TicketCountForAgent
go

select * from AssignedTicket
go

select * from vw_TotalticketsResolved
go

select * from vw_UserCount
go

select * from vw_UserRoleView
go

select * from vw_UsersAndAgentsView
go

select * from vw_UserTicketView
go

select * from vw_UserTicketViewForAdminsAndAgents
go

CREATE PROCEDURE GetTotalAssignedTicketsCount
    @agentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        COUNT(*) AS TotalAssignedTicketsCount
    FROM 
        [UserTicket] ut
    INNER JOIN 
        AssignedTicket r ON r.UserTicketId = ut.UserTicketId
    WHERE 
        r.AgentId = @agentId;
END
GO

create view vw_AgentCount
as
select Count(*) 'Total Agent Count'
from [User] u
inner join UserRole ur on u.UserId = ur.UserId
inner join [Role] r on r.RoleId = ur.RoleId
where ur.RoleId = 2
go

create view vw_TotalTicketResolvedByAgent
as
select Count(*)
from AssignedTicket ast
inner join UserRole ur on u.UserId = ur.UserId
inner join [Role] r on r.RoleId = ur.RoleId
where ur.RoleId = 2
go

alter PROCEDURE GetMyTotalTicketsResolved
    @agentId INT, @result int output
AS
BEGIN
	SELECT 
       @result = COUNT(*)
    FROM 
        AssignedTicket ast
    WHERE 
		ast.AgentId = @agentId;
END
go

select * from AssignedTicket
go

create PROCEDURE GetTotalTicketsYouAssigned
    @AssignerId INT, @result int output
AS
BEGIN
	SELECT 
       @result = COUNT(*)
    FROM 
        AssignedTicket ast
    WHERE 
		ast.AssignerId = @AssignerId;
END
go

select * from AssignedTicket
go

select * from [Role]
go

declare @result as int
exec GetTotalTicketsYouAssigned 3, @result output;
select @result;

create view vw_AgentCount
as
select Count(*) 'Total Agent Count'
from vw_UserRoleView
where RoleId = 2
go

select * from [UserTicket]
go

create PROCEDURE GetTotalTicketsCreatedByMe
    @UserId INT, @result int output
AS
BEGIN
	SELECT 
       @result = COUNT(*)
    FROM 
        UserTicket ut
    WHERE 
		ut.UserId = @UserId;
END
go

declare @result as int
exec GetTotalTicketsCreatedByMe 2, @result output;
select @result;
go

select * from vw_UserRoleView
go


select * from vw_TicketAssignment
go
select * from [Status]
go

create PROCEDURE GetTotalTicketsResolvedForReporter
    @UserId INT, @result int output
AS
BEGIN
	SELECT 
       @result = COUNT(*)
    FROM 
        vw_TicketAssignment ta
    WHERE 
		ta.ReporterId = @UserId and StatusId = 3
END
go

create PROCEDURE GetTotalTicketsNotResolvedForReporter
    @UserId INT, @result int output
AS
BEGIN
	SELECT 
       @result = COUNT(*)
    FROM 
        vw_TicketAssignment ta
    WHERE 
		ta.ReporterId = @UserId and StatusId IN(1,2,4)
END
go

declare @result as int
exec GetTotalTicketsNotResolvedForReporter 2, @result output;
select @result
go

alter view vw_UserTicketView
as
SELECT        c.CategoryName, s.StatusName, t.TicketId, t.CategoryId, t.PriorityId, t.StatusId, t.IssueDescription, t.AttachmentPath, t.CreateAt, t.LastModified, t.ResolveAt, ut.UserTicketId, u.UserId, 
                         u.Name, u.Email, u.Password
FROM            dbo.Ticket AS t INNER JOIN
                         dbo.UserTicket AS ut ON t.TicketId = ut.TicketId INNER JOIN
                         dbo.[User] AS u ON u.UserId = ut.UserId INNER JOIN
                         dbo.Category AS c ON c.CategoryId = t.CategoryId INNER JOIN
                         dbo.Status AS s ON s.StatusId = t.StatusId
go;

		
select *
from vw_UserRoleView
go

select * from Notification