CREATE PROCEDURE [dbo].[get_common_drop_down_list]
    @company_id INT,
    @name VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
                    a.id, a.code, a.name
                FROM 
                    ' + @name + ' a
                WHERE 
                    a.company_id = @company_id AND a.active_flag = 1 AND a.delete_flag = 0
                ORDER BY
                    a.id;';

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, 
        N'@company_id INT, @name VARCHAR(50)',
        @company_id, @name;
END;