CREATE PROCEDURE [dbo].[get_manpower_project_mapping_by_id]
    @manpower_id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
                    a.id, a.project_id, b.name
                FROM 
                    manpower_project_mapping a
                    INNER JOIN project b ON a.project_id = b.id AND b.active_flag = 1 AND b.delete_flag = 0
                WHERE 
                    a.manpower_id = @manpower_id AND a.delete_flag = 0
                ORDER BY
                    a.id;';

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, 
        N'@manpower_id INT',
        @manpower_id;
END;