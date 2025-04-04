CREATE PROCEDURE [dbo].[get_activity_drop_down_milestone_list]
    @project_id INT = 0,
    @company_id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    -- Declare a variable to hold the SQL query
    DECLARE @SQL NVARCHAR(MAX);

    -- Sub_Task Drop Down Query for mapped project
    SET @SQL = N'
    SELECT 
        DISTINCT c.id AS activity_id, 
        c.name AS activity_name, 
        CONVERT(VARCHAR(50), MIN(CONVERT(DATE, a.start_date))) AS start_date, 
        CONVERT(VARCHAR(50), MAX(CONVERT(DATE, a.end_date))) AS end_date, 
        SUM(a.cost) AS cost
    FROM 
        sub_task_project_mapping a
        INNER JOIN sub_task b 
            ON a.sub_task_id = b.id 
            AND b.active_flag = 1 
            AND b.delete_flag = 0
        INNER JOIN activity c 
            ON b.activity_id = c.id 
            AND c.active_flag = 1 
            AND c.delete_flag = 0
    WHERE 
        NOT EXISTS (
            SELECT 1
            FROM 
                activity_milestone am
            INNER JOIN activity_milestone_mapping amm ON am.id = amm.activity_milestone_id
            WHERE 
                am.project_id = @project_id AND amm.activity_id = c.id AND am.delete_flag = 0
        )
        AND a.project_id = @project_id 
        AND b.company_id = @company_id 
    GROUP BY 
        c.id, c.name
    ORDER BY 
        c.id;';

    -- Execute the query
    EXEC sp_executesql @SQL, N'@project_id INT, @company_id INT', @project_id, @company_id;
END;
