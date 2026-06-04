USE [BarInventoryDB]
GO

/****** Object:  Table [dbo].[Cocktails] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cocktails](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[CocktailIngredients] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CocktailIngredients](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CocktailId] [int] NOT NULL,
	[IngredientId] [int] NOT NULL,
	[Quantity] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CocktailIngredients]  WITH CHECK ADD  CONSTRAINT [FK_CocktailIngredients_Cocktails] FOREIGN KEY([CocktailId])
REFERENCES [dbo].[Cocktails] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CocktailIngredients] CHECK CONSTRAINT [FK_CocktailIngredients_Cocktails]
GO

ALTER TABLE [dbo].[CocktailIngredients]  WITH CHECK ADD  CONSTRAINT [FK_CocktailIngredients_Ingredients] FOREIGN KEY([IngredientId])
REFERENCES [dbo].[Ingredients] ([Id])
GO
ALTER TABLE [dbo].[CocktailIngredients] CHECK CONSTRAINT [FK_CocktailIngredients_Ingredients]
GO

/****** Updating ReceiptItems to support Cocktails ******/
ALTER TABLE [dbo].[ReceiptItems] ALTER COLUMN [IngredientId] [int] NULL;
GO
ALTER TABLE [dbo].[ReceiptItems] ADD [CocktailId] [int] NULL;
GO
ALTER TABLE [dbo].[ReceiptItems]  WITH CHECK ADD  CONSTRAINT [FK_ReceiptItems_Cocktails] FOREIGN KEY([CocktailId])
REFERENCES [dbo].[Cocktails] ([Id])
GO
ALTER TABLE [dbo].[ReceiptItems] CHECK CONSTRAINT [FK_ReceiptItems_Cocktails]
GO
