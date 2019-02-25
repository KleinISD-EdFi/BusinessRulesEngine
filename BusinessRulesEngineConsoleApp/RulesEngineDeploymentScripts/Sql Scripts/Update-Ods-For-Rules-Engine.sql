IF NOT EXISTS (
	SELECT  schema_name
	FROM    information_schema.schemata
	WHERE   schema_name = 'rules' )
BEGIN
EXEC sp_executesql N'CREATE SCHEMA rules'
END
GO

IF EXISTS(select * FROM sys.views where name = 'Staff')
	DROP VIEW [rules].[Staff];
GO

CREATE VIEW [rules].[Staff] AS
SELECT StaffUSI AS Id, FirstName, MiddleName, LastSurname, SexTypeId, HispanicLatinoEthnicity
FROM edfi.Staff
GO

IF EXISTS(select * FROM sys.views where name = 'Student')
	DROP VIEW [rules].[Student];
GO

CREATE VIEW [rules].[Student] AS
SELECT [StudentUSI] as Id, [FirstName], [LastSurname]
FROM [edfi].[Student]
GO

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'[rules].[CheckUppercase]')
                    AND type IN (N'FN', N'IF', N'TF', N'FS', N'FT') ) 
    DROP FUNCTION [rules].[CheckUppercase] ;
GO

CREATE FUNCTION [rules].[CheckUppercase] (@STRING VARCHAR(250))
	RETURNS nvarchar(4)
	BEGIN
	IF (UNICODE(SUBSTRING(@STRING, 1, 1)) <> UNICODE(LOWER(SUBSTRING(@STRING, 1, 1))) OR @STRING IS NULL)
		RETURN 1
	RETURN 0
	END
GO
