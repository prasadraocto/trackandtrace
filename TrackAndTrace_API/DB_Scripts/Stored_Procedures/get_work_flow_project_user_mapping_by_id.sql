CREATE PROCEDURE [dbo].[get_work_flow_project_user_mapping_by_id]
    @work_flow_id INT,
    @project_id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
                     b.id, b.name, a.order_id, a.is_supersede
                FROM 
                    work_flow_project_user_mapping a
                    INNER JOIN users b ON a.user_id = b.id AND b.active_flag = 1 AND b.delete_flag = 0
                WHERE 
                    a.work_flow_id = @work_flow_id AND a.project_id = @project_id AND a.delete_flag = 0
                ORDER BY
                    a.order_id;';

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, 
        N'@work_flow_id INT, @project_id INT',
        @work_flow_id, @project_id;
END;