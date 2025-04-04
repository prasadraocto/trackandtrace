CREATE PROCEDURE [dbo].[get_daily_activity_machinery_drop_down_list]
    @company_id INT,
    @daily_activity_id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'
        SELECT 
            a.id AS machinery_id, 
            a.code AS machinery_code, 
            a.name AS machinery_name, 
            a.quantity - ISNULL(used_machinery.total_used_quantity, 0) AS available_quantity
        FROM 
            machinery a
            LEFT JOIN (
                SELECT 
                    machinery_id, 
                    SUM(quantity) AS total_used_quantity
                FROM 
                    trx_daily_activity_machinery
                WHERE 
                    company_id = @company_id 
                    AND delete_flag = 0 
                    AND CONVERT(VARCHAR(10), created_date, 120) = CONVERT(VARCHAR(10), GETDATE(), 120)
                    AND daily_activity_id <> @daily_activity_id
                GROUP BY 
                    machinery_id
            ) used_machinery ON a.id = used_machinery.machinery_id
        WHERE 
            a.company_id = @company_id 
            AND a.delete_flag = 0
        ORDER BY
            a.id;';

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, 
        N'@company_id INT, @daily_activity_id INT', 
        @company_id, @daily_activity_id;
END;