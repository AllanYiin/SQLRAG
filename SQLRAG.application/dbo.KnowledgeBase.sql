CREATE TABLE [dbo].[KnowledgeBase](
	[PartId] [uniqueidentifier] NOT NULL,
	[ParentId] [uniqueidentifier] NULL,
	[Ordinal] int NULL,
	[IsRewrite] [bit] NULL DEFAULT 0,
	[SourceType] [smallint] NULL,
	[Url] [nvarchar](512) NULL,
	[TextContent] [nvarchar](max) NOT NULL,
	[Embeddings] [dbo].[SqlArray] NULL,
	[Raw] [nvarchar](max) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_TextContent] PRIMARY KEY CLUSTERED 
(
	[PartId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[KnowledgeBase] ADD  CONSTRAINT [DF_KnowledgeBase_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

