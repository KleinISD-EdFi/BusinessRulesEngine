CREATE VIEW [rules].[Staff] AS
SELECT StaffUSI AS Id, FirstName, MiddleName, LastSurname, SexTypeId, HispanicLatinoEthnicity
FROM edfi.Staff
GO

CREATE VIEW [rules].[Student] AS
SELECT [StudentUSI] as Id, [FirstName], [LastSurname]
FROM [edfi].[Student]
GO

CREATE FUNCTION [rules].[CheckUppercase] (@STRING VARCHAR(250))
RETURNS nvarchar(4)
BEGIN
IF (UNICODE(SUBSTRING(@STRING, 1, 1)) <> UNICODE(LOWER(SUBSTRING(@STRING, 1, 1))) OR @STRING IS NULL)
	RETURN 1
RETURN 0
END
GO
