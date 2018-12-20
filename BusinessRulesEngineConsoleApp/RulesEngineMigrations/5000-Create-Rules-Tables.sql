IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[rules].[RuleValidation]'))
	CREATE TABLE [rules].[RuleValidation](
		[RuleValidationId] [bigint] IDENTITY(1,1) NOT NULL,
		[CollectionId] [nvarchar](50) NULL,
		[RunDateTime] [datetime] NOT NULL,
	 CONSTRAINT [PK_rules.RuleValidation] PRIMARY KEY CLUSTERED 
	(
		[RuleValidationId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	GO

IF OBJECT_ID('[rules].[RuleValidationRecipients]', 'U') IS NOT NULL 
  DROP TABLE [rules].[RuleValidationRecipients];

CREATE TABLE [rules].[RuleValidationRecipients](
	[EmailAddress] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_rules.RuleValidationRecipients] PRIMARY KEY CLUSTERED 
(
	[EmailAddress] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF OBJECT_ID('[rules].[RuleValidationDetail]', 'U') IS NOT NULL 
  DROP TABLE [rules].[RuleValidationDetail];

CREATE TABLE [rules].[RuleValidationDetail](
[RuleValidationId] [bigint] NOT NULL,
[Id] [bigint] NOT NULL,
[RuleId] [nvarchar](50) NOT NULL,
[IsError] [bit] NOT NULL,
[Message] [nvarchar](max) NULL,
CONSTRAINT [PK_rules.RuleValidationDetail] PRIMARY KEY CLUSTERED 
(
	[RuleValidationId] ASC,
	[Id] ASC,
	[RuleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [rules].[RuleValidationDetail]  WITH CHECK ADD  CONSTRAINT [FK_rules.RuleValidationDetail_rules.RuleValidation_RuleValidationId] FOREIGN KEY([RuleValidationId])
REFERENCES [rules].[RuleValidation] ([RuleValidationId])
ON DELETE CASCADE
GO

ALTER TABLE [rules].[RuleValidationDetail] CHECK CONSTRAINT [FK_rules.RuleValidationDetail_rules.RuleValidation_RuleValidationId]
GO

IF OBJECT_ID('[rules].[RuleValidationRuleComponent]', 'U') IS NOT NULL 
  DROP TABLE [rules].[RuleValidationRuleComponent];

CREATE TABLE [rules].[RuleValidationRuleComponent](
	[RuleValidationId] [bigint] NOT NULL,
	[RulesetId] [nvarchar](50) NOT NULL,
	[RuleId] [nvarchar](50) NOT NULL,
	[Component] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_rules.RuleValidationRuleComponent] PRIMARY KEY CLUSTERED 
(
	[RuleValidationId] ASC,
	[RulesetId] ASC,
	[RuleId] ASC,
	[Component] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [rules].[RuleValidationRuleComponent]  WITH CHECK ADD  CONSTRAINT [FK_rules.RuleValidationRuleComponent_rules.RuleValidation_RuleValidationId] FOREIGN KEY([RuleValidationId])
REFERENCES [rules].[RuleValidation] ([RuleValidationId])
ON DELETE CASCADE
GO

ALTER TABLE [rules].[RuleValidationRuleComponent] CHECK CONSTRAINT [FK_rules.RuleValidationRuleComponent_rules.RuleValidation_RuleValidationId]
GO