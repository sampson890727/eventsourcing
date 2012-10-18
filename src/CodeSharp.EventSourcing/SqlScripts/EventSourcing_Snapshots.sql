SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EventSourcing_Snapshots](
    [AggregateRootName] [nvarchar](512) NOT NULL,
    [AggregateRootId] [uniqueidentifier] NOT NULL,
    [Version] [bigint] NOT NULL,
    [Name] [nvarchar](512) NOT NULL,
    [SerializedData] [nvarchar](max) NOT NULL,
    [CreatedTime] [datetime] NOT NULL,
 CONSTRAINT [PK_EventSourcing_Snapshots] PRIMARY KEY CLUSTERED 
(
    [AggregateRootName] ASC,
    [AggregateRootId] ASC,
    [Version] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
