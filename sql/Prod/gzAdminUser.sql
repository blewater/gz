--in master
CREATE LOGIN gzAdminUser WITH PASSWORD = 'xxxx'; -- saved elsewhere
go
CREATE USER [gzAdminUser] FOR LOGIN [gzAdminUser]
go

--in gzDbProd
GRANT SELECT, INSERT, UPDATE, DELETE ON AspNetRoles To gzAdminUser
go
GRANT SELECT ON AspNetUserClaims To gzAdminUser
go
GRANT SELECT ON AspNetUserLogins To gzAdminUser
go
GRANT SELECT, INSERT, UPDATE, DELETE ON AspNetUserRoles TO gzAdminUser
go
GRANT SELECT, UPDATE, DELETE ON AspNetUsers To gzAdminUser
go
GRANT SELECT, INSERT, UPDATE, DELETE ON CarouselEntries To gzAdminUser
go
GRANT SELECT, INSERT, UPDATE, DELETE ON DynamicPageDatas To gzAdminUser
go
GRANT SELECT, INSERT, UPDATE, DELETE ON DynamicPages To gzAdminUser
go
GRANT SELECT, INSERT, UPDATE, DELETE ON DynamicPageTemplates To gzAdminUser
go
GRANT SELECT, INSERT, UPDATE, DELETE ON EmailTemplates  To gzAdminUser
go
GRANT SELECT, INSERT, UPDATE, DELETE ON GameCategories   To gzAdminUser
go
GRANT SELECT ON LogEntries To gzAdminUser
go
