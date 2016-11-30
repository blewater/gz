CREATE TABLE [dbo].[GameCategories] (
    [Id]        INT             IDENTITY (1, 1) NOT NULL,
    [Code]      NVARCHAR (100)  NOT NULL,
    [Title]     NVARCHAR (255)  NOT NULL,
    [GameSlugs] NVARCHAR (2048) NOT NULL,
    [Updated]   DATETIME        DEFAULT ('1900-01-01T00:00:00.000') NOT NULL,
    [IsMobile]  BIT             DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_dbo.GameCategories] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [GameCategory_Code]
    ON [dbo].[GameCategories]([Code] ASC);


GO
GRANT UPDATE
    ON OBJECT::[dbo].[GameCategories] TO [gzAdminUser]
    AS [dbo];




GO
GRANT SELECT
    ON OBJECT::[dbo].[GameCategories] TO [gzAdminUser]
    AS [dbo];




GO
GRANT INSERT
    ON OBJECT::[dbo].[GameCategories] TO [gzAdminUser]
    AS [dbo];




GO
GRANT DELETE
    ON OBJECT::[dbo].[GameCategories] TO [gzAdminUser]
    AS [dbo];



