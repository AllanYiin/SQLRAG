﻿CREATE TABLE [dbo].[EncryptedKeys](
	[KeyName] [nvarchar](50) NOT NULL,
	[KeyDesc] [nvarchar](512) NULL,
	[KeyValue] VARBINARY(4000) NOT NULL,
 CONSTRAINT [PK_EncryptedKeys] PRIMARY KEY CLUSTERED 
(
	[KeyName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]