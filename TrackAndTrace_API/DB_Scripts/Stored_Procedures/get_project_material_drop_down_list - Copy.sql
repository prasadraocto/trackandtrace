CREATE PROCEDURE [dbo].[get_project_material_drop_down_list]
    @project_id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
                    b.id as material_id, b.code as material_code, b.name as material_name, a.quantity, c.code as uom_name
                FROM 
                    project_material_mapping a
                    INNER JOIN material b ON a.material_id = b.id AND b.active_flag = 1 AND b.delete_flag = 0
                    INNER JOIN uom c ON b.uom_id = c.id AND c.active_flag = 1 AND c.delete_flag = 0
                WHERE 
                    a.project_id = @project_id AND a.delete_flag = 0
                ORDER BY
                    b.id;';

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, N'@project_id INT', @project_id;
END;