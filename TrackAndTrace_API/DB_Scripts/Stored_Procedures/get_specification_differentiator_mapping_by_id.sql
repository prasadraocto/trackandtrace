CREATE PROCEDURE [dbo].[get_specification_differentiator_mapping_by_id]
    @specification_id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
                    a.id, b.id as differentiator_id, b.name, a.value, c.code as specification_code
                FROM 
                    specification_differentiator_mapping a
                    INNER JOIN differentiator b ON a.differentiator_id = b.id AND b.active_flag = 1 AND b.delete_flag = 0
                    INNER JOIN specification c ON a.specification_id = c.id AND c.active_flag = 1 AND c.delete_flag = 0
                WHERE 
                    a.specification_id = @specification_id
                ORDER BY
                    a.id;';

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, 
        N'@specification_id INT',
        @specification_id;
END;