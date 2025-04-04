CREATE PROCEDURE [dbo].[get_project_user_mapping_by_id]
    @project_id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
                    a.id, a.user_id, b.name
                FROM 
                    project_user_mapping a
                    INNER JOIN users b ON a.user_id = b.id AND b.active_flag = 1 AND b.delete_flag = 0
                WHERE 
                    a.project_id = @project_id AND a.delete_flag = 0
                ORDER BY
                    a.id;';

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, 
        N'@project_id INT',
        @project_id;
END;