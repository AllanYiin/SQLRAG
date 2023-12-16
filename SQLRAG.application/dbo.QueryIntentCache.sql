CREATE TABLE [dbo].[QueryIntentCache](
	[id] [uniqueidentifier] NOT NULL,
	[QueryIntent] [nvarchar](max) NULL,
	[QueryIntentArray]  AS (dbo.SqlArray::Parse([VectorizedQueryIntent])),
	[VectorizedQueryIntent] [varchar](max) NULL,
	[GeneratedTSQL] [nvarchar](max)  NULL,
	[GeneratedData] [nvarchar](max) NULL,
	[CreateDate] [datetime] NULL,
	[ExecStatus] [nvarchar](256) NULL,
 CONSTRAINT [PK_QueryIntentCache] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[QueryIntentCache] ADD  CONSTRAINT [DF_QueryIntentCache_id]  DEFAULT (newid()) FOR [id]
GO

ALTER TABLE [dbo].[QueryIntentCache] ADD  CONSTRAINT [DF_QueryIntentCache_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

