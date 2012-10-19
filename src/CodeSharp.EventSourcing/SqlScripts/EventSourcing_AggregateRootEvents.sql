--Copyright (c) CodeSharp.  All rights reserved.

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EventSourcing_AggregateRootEvents](
    [AggregateRootName] [nvarchar](512) NOT NULL,
    [AggregateRootId] [uniqueidentifier] NOT NULL,
    [Version] [bigint] NOT NULL,
    [Name] [nvarchar](512) NOT NULL,
    [OccurredTime] [datetime] NOT NULL,
    [Data] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_EventSourcing_AggregateRootEvents] PRIMARY KEY CLUSTERED 
(
    [AggregateRootName] ASC,
    [AggregateRootId] ASC,
    [Version] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
