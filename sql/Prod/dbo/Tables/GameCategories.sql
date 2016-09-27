CREATE TABLE [dbo].[GameCategories] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [Code]      NVARCHAR (100) NOT NULL,
    [Title]     NVARCHAR (MAX) NOT NULL,
    [GameSlugs] NVARCHAR (MAX) NOT NULL,
    [Updated]   DATETIME       DEFAULT ('1900-01-01T00:00:00.000') NOT NULL,
    CONSTRAINT [PK_dbo.GameCategories] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [GameCategory_Code]
    ON [dbo].[GameCategories]([Code] ASC);

