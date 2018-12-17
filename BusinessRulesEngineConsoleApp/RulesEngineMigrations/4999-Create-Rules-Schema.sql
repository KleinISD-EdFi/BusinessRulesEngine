IF NOT EXISTS (SELECT name FROM sys.schemas WHERE name = N'rules')
	BEGIN
		EXEC ('CREATE SCHEMA [rules]')
	END