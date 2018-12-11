
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 03/06/2018 12:11:33
-- Generated from EDMX file: D:\Projects\VK\VKApi\VKApiModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [VKApi];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_UserActionLimits_Profile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserActionLimits] DROP CONSTRAINT [FK_UserActionLimits_Profile];
GO
IF OBJECT_ID(N'[dbo].[FK_UserActions_Profile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserActions] DROP CONSTRAINT [FK_UserActions_Profile];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[GroupActionLimits]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GroupActionLimits];
GO
IF OBJECT_ID(N'[dbo].[GroupActions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GroupActions];
GO
IF OBJECT_ID(N'[dbo].[GroupActionTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GroupActionTypes];
GO
IF OBJECT_ID(N'[dbo].[Groups]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Groups];
GO
IF OBJECT_ID(N'[dbo].[Posts]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Posts];
GO
IF OBJECT_ID(N'[dbo].[Profile]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Profile];
GO
IF OBJECT_ID(N'[dbo].[UserActionLimits]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserActionLimits];
GO
IF OBJECT_ID(N'[dbo].[UserActions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserActions];
GO
IF OBJECT_ID(N'[dbo].[UserActionTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserActionTypes];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'GroupActionLimits'
CREATE TABLE [dbo].[GroupActionLimits] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ActionTypeId] int  NOT NULL,
    [LimitDateTime] datetime  NOT NULL
);
GO

-- Creating table 'GroupActions'
CREATE TABLE [dbo].[GroupActions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [GroupId] int  NOT NULL,
    [PostId] int  NOT NULL,
    [UpdateDateTime] datetime  NOT NULL
);
GO

-- Creating table 'GroupActionTypes'
CREATE TABLE [dbo].[GroupActionTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ActionType] varchar(10)  NOT NULL
);
GO

-- Creating table 'Groups'
CREATE TABLE [dbo].[Groups] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Url] bigint  NOT NULL,
    [GroupName] nvarchar(255)  NOT NULL
);
GO

-- Creating table 'Posts'
CREATE TABLE [dbo].[Posts] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Post] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'UserActionLimits'
CREATE TABLE [dbo].[UserActionLimits] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ActionTypeId] int  NOT NULL,
    [LimitDateTime] datetime  NOT NULL,
    [ProfileId] int  NOT NULL
);
GO

-- Creating table 'UserActions'
CREATE TABLE [dbo].[UserActions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] int  NOT NULL,
    [ActionTypeId] int  NOT NULL,
    [UpdateDateTime] datetime  NOT NULL,
    [ProfileId] int  NOT NULL
);
GO

-- Creating table 'UserActionTypes'
CREATE TABLE [dbo].[UserActionTypes] (
    [Id] int  NOT NULL,
    [ActionType] varchar(10)  NOT NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [FullName] nvarchar(255)  NOT NULL,
    [Url] bigint  NOT NULL,
    [Status] nvarchar(255)  NOT NULL,
    [Friends] int  NOT NULL,
    [Followers] int  NOT NULL,
    [Common] int  NOT NULL,
    [UpdateDate] datetime  NOT NULL
);
GO

-- Creating table 'Profile'
CREATE TABLE [dbo].[Profile] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ProfileName] nvarchar(10)  NOT NULL,
    [Login] nvarchar(255)  NOT NULL,
    [Password] nvarchar(255)  NOT NULL,
    [AppId] bigint  NOT NULL,
    [TelegramToken] nvarchar(255)  NULL,
    [TelegramChatId] bigint  NULL,
    [AntiCaptchaApiKey] nvarchar(255)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'GroupActionLimits'
ALTER TABLE [dbo].[GroupActionLimits]
ADD CONSTRAINT [PK_GroupActionLimits]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GroupActions'
ALTER TABLE [dbo].[GroupActions]
ADD CONSTRAINT [PK_GroupActions]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GroupActionTypes'
ALTER TABLE [dbo].[GroupActionTypes]
ADD CONSTRAINT [PK_GroupActionTypes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Groups'
ALTER TABLE [dbo].[Groups]
ADD CONSTRAINT [PK_Groups]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Posts'
ALTER TABLE [dbo].[Posts]
ADD CONSTRAINT [PK_Posts]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserActionLimits'
ALTER TABLE [dbo].[UserActionLimits]
ADD CONSTRAINT [PK_UserActionLimits]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserActions'
ALTER TABLE [dbo].[UserActions]
ADD CONSTRAINT [PK_UserActions]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserActionTypes'
ALTER TABLE [dbo].[UserActionTypes]
ADD CONSTRAINT [PK_UserActionTypes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Profile'
ALTER TABLE [dbo].[Profile]
ADD CONSTRAINT [PK_Profile]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [ActionTypeId] in table 'GroupActionLimits'
ALTER TABLE [dbo].[GroupActionLimits]
ADD CONSTRAINT [FK_GroupActionLimitsGroupActionTypes]
    FOREIGN KEY ([ActionTypeId])
    REFERENCES [dbo].[GroupActionTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GroupActionLimitsGroupActionTypes'
CREATE INDEX [IX_FK_GroupActionLimitsGroupActionTypes]
ON [dbo].[GroupActionLimits]
    ([ActionTypeId]);
GO

-- Creating foreign key on [GroupId] in table 'GroupActions'
ALTER TABLE [dbo].[GroupActions]
ADD CONSTRAINT [FK_GroupsGroup_actions]
    FOREIGN KEY ([GroupId])
    REFERENCES [dbo].[Groups]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GroupsGroup_actions'
CREATE INDEX [IX_FK_GroupsGroup_actions]
ON [dbo].[GroupActions]
    ([GroupId]);
GO

-- Creating foreign key on [PostId] in table 'GroupActions'
ALTER TABLE [dbo].[GroupActions]
ADD CONSTRAINT [FK_PostsGroup_actions]
    FOREIGN KEY ([PostId])
    REFERENCES [dbo].[Posts]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PostsGroup_actions'
CREATE INDEX [IX_FK_PostsGroup_actions]
ON [dbo].[GroupActions]
    ([PostId]);
GO

-- Creating foreign key on [ActionTypeId] in table 'UserActionLimits'
ALTER TABLE [dbo].[UserActionLimits]
ADD CONSTRAINT [FK_UserActionTypesUserActionLimits]
    FOREIGN KEY ([ActionTypeId])
    REFERENCES [dbo].[UserActionTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserActionTypesUserActionLimits'
CREATE INDEX [IX_FK_UserActionTypesUserActionLimits]
ON [dbo].[UserActionLimits]
    ([ActionTypeId]);
GO

-- Creating foreign key on [ActionTypeId] in table 'UserActions'
ALTER TABLE [dbo].[UserActions]
ADD CONSTRAINT [FK_UserActionsUserActionTypes]
    FOREIGN KEY ([ActionTypeId])
    REFERENCES [dbo].[UserActionTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserActionsUserActionTypes'
CREATE INDEX [IX_FK_UserActionsUserActionTypes]
ON [dbo].[UserActions]
    ([ActionTypeId]);
GO

-- Creating foreign key on [UserId] in table 'UserActions'
ALTER TABLE [dbo].[UserActions]
ADD CONSTRAINT [FK_UsersUser_actions]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UsersUser_actions'
CREATE INDEX [IX_FK_UsersUser_actions]
ON [dbo].[UserActions]
    ([UserId]);
GO

-- Creating foreign key on [ProfileId] in table 'UserActionLimits'
ALTER TABLE [dbo].[UserActionLimits]
ADD CONSTRAINT [FK_UserActionLimits_Profiles1]
    FOREIGN KEY ([ProfileId])
    REFERENCES [dbo].[Profile]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserActionLimits_Profiles1'
CREATE INDEX [IX_FK_UserActionLimits_Profiles1]
ON [dbo].[UserActionLimits]
    ([ProfileId]);
GO

-- Creating foreign key on [ProfileId] in table 'UserActions'
ALTER TABLE [dbo].[UserActions]
ADD CONSTRAINT [FK_UserActions_Profiles1]
    FOREIGN KEY ([ProfileId])
    REFERENCES [dbo].[Profile]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserActions_Profiles1'
CREATE INDEX [IX_FK_UserActions_Profiles1]
ON [dbo].[UserActions]
    ([ProfileId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------