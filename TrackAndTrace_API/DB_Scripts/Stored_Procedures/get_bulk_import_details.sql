CREATE PROCEDURE [dbo].[get_bulk_import_details]
    @company_id INT,
    @import_name NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
                    a.id, a.name, a.total, a.success, a.failed, a.failed_records, a.status, a.created_at, a.updated_at
                FROM 
                    bulk_import_details a
                WHERE 
                    a.company_id = @company_id AND a.import_name = @import_name
                ORDER BY
                    a.id;';

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, 
        N'@company_id INT, @import_name NVARCHAR(100)',
        @company_id, @import_name;
END;