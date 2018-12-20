IF OBJECT_ID('[rules].[Staff]') IS NULL
	BEGIN
		EXEC('CREATE VIEW [rules].[Staff] AS
				SELECT StaffUSI AS Id, FirstName, MiddleName, LastSurname, SexTypeId, HispanicLatinoEthnicity
				FROM edfi.Staff')
	END

IF OBJECT_ID('[rules].[Student]') IS NULL
	BEGIN
		EXEC('CREATE VIEW [rules].[Student] AS
				SELECT [StudentUSI] as Id, [FirstName], [LastSurname]
				FROM [edfi].[Student]')
	END

IF EXISTS(SELECT *
	FROM   sys.objects
	WHERE  object_id = OBJECT_ID(N'[rules].[CheckUppercase]')
			AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
	DROP FUNCTION [rules].[CheckUppercase]

EXEC('CREATE FUNCTION [rules].[CheckUppercase] (@STRING VARCHAR(250))
RETURNS nvarchar(4)
BEGIN
IF (UNICODE(SUBSTRING(@STRING, 1, 1)) <> UNICODE(LOWER(SUBSTRING(@STRING, 1, 1))) OR @STRING IS NULL)
	RETURN 1
RETURN 0
END')