CREATE PROCEDURE [dbo].[get_daily_activity_detail_by_id]
    @daily_activity_id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
       DISTINCT a.id, a.created_date as activity_date, b.activity_id, b.task_id, b.sub_task_id, 
       a.quantity, a.progress, a.shift_id, a.hrs_spent, a.labour_type_id, 
       a.subcontractor_id, a.weather_id, a.remarks, a.project_level_id, a.is_draft, a.status
    FROM 
        trx_daily_activity_details a
        INNER JOIN trx_daily_activity_item b ON a.activity_item_id = b.id
        INNER JOIN activity c ON b.activity_id = c.id AND c.delete_flag = 0
        INNER JOIN task d ON b.task_id = d.id AND d.delete_flag = 0
        INNER JOIN sub_task e ON b.sub_task_id = e.id AND e.delete_flag = 0
        INNER JOIN project f ON a.project_id = f.id AND f.delete_flag = 0
        INNER JOIN uom g ON e.uom_id = g.id AND g.delete_flag = 0
        INNER JOIN shift h ON a.shift_id = h.id AND h.delete_flag = 0
        LEFT JOIN labour_type i ON a.labour_type_id = i.id AND i.delete_flag = 0
        LEFT JOIN subcontractor j ON a.subcontractor_id = j.id AND j.delete_flag = 0
        LEFT JOIN weather k ON a.weather_id = k.id AND k.delete_flag = 0
        INNER JOIN project_level_mapping l ON a.project_level_id = l.id AND l.delete_flag = 0
    WHERE 
        a.id = @daily_activity_id;';

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, 
        N'@daily_activity_id INT',
        daily_activity_id;
END;