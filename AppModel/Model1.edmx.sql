
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 11/27/2016 14:44:30
-- Generated from EDMX file: C:\Users\Dmitriy\Source\Repos\NoodleD\AppModel\Model1.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [NoodleD];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_StringValue_FieldType]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[StringValue] DROP CONSTRAINT [FK_StringValue_FieldType];
GO
IF OBJECT_ID(N'[dbo].[FK_StringValue_Language]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[StringValue] DROP CONSTRAINT [FK_StringValue_Language];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Dish]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Dish];
GO
IF OBJECT_ID(N'[dbo].[FieldType]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FieldType];
GO
IF OBJECT_ID(N'[dbo].[Language]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Language];
GO
IF OBJECT_ID(N'[dbo].[MenuFolder]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MenuFolder];
GO
IF OBJECT_ID(N'[dbo].[StringValue]', 'U') IS NOT NULL
    DROP TABLE [dbo].[StringValue];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Dish'
CREATE TABLE [dbo].[Dish] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [MenuFolderGUID] uniqueidentifier  NOT NULL,
    [RowGUID] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'FieldType'
CREATE TABLE [dbo].[FieldType] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Type] int  NOT NULL,
    [TableName] nvarchar(100)  NOT NULL,
    [FieldName] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'Language'
CREATE TABLE [dbo].[Language] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] char(2)  NOT NULL
);
GO

-- Creating table 'MenuFolder'
CREATE TABLE [dbo].[MenuFolder] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RowGUID] uniqueidentifier  NOT NULL,
    [ParentId] int  NOT NULL,
    [HasIngredients] bit  NOT NULL
);
GO

-- Creating table 'StringValue'
CREATE TABLE [dbo].[StringValue] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RowGUID] uniqueidentifier  NOT NULL,
    [FieldTypeId] int  NOT NULL,
    [LangId] int  NOT NULL,
    [Value] nvarchar(512)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Dish'
ALTER TABLE [dbo].[Dish]
ADD CONSTRAINT [PK_Dish]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'FieldType'
ALTER TABLE [dbo].[FieldType]
ADD CONSTRAINT [PK_FieldType]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Language'
ALTER TABLE [dbo].[Language]
ADD CONSTRAINT [PK_Language]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'MenuFolder'
ALTER TABLE [dbo].[MenuFolder]
ADD CONSTRAINT [PK_MenuFolder]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'StringValue'
ALTER TABLE [dbo].[StringValue]
ADD CONSTRAINT [PK_StringValue]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [FieldTypeId] in table 'StringValue'
ALTER TABLE [dbo].[StringValue]
ADD CONSTRAINT [FK_StringValue_FieldType]
    FOREIGN KEY ([FieldTypeId])
    REFERENCES [dbo].[FieldType]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_StringValue_FieldType'
CREATE INDEX [IX_FK_StringValue_FieldType]
ON [dbo].[StringValue]
    ([FieldTypeId]);
GO

-- Creating foreign key on [LangId] in table 'StringValue'
ALTER TABLE [dbo].[StringValue]
ADD CONSTRAINT [FK_StringValue_Language]
    FOREIGN KEY ([LangId])
    REFERENCES [dbo].[Language]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_StringValue_Language'
CREATE INDEX [IX_FK_StringValue_Language]
ON [dbo].[StringValue]
    ([LangId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------