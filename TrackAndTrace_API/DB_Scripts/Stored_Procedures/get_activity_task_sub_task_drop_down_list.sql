CREATE PROCEDURE [dbo].[get_activity_task_sub_task_drop_down_list]
    @company_id INT = 0,
    @activity_id INT = 0,
    @task_id INT = 0,
    @project_id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    -- Declare a variable to hold the SQL query
    DECLARE @SQL NVARCHAR(MAX);

    -- Build the dynamic SQL based on the input parameters
    IF @activity_id != 0
    BEGIN
        -- Task Drop Down Query
        SET @SQL = N'SELECT 
                        DISTINCT ''Task'' AS drop_down_name, b.id, b.name, c.id AS uom_id, c.code AS uom_name,
                        NULL AS start_date, NULL AS end_date, NULL AS cost
                    FROM 
                        task_project_mapping a
                        INNER JOIN task b ON a.task_id = b.id AND b.active_flag = 1 AND b.delete_flag = 0
                        INNER JOIN uom c ON b.uom_id = c.id AND c.active_flag = 1 AND c.delete_flag = 0
                    WHERE 
                        b.company_id = @company_id AND b.activity_id = @activity_id' +
                        CASE WHEN @project_id != 0 THEN N' AND a.project_id = @project_id' ELSE N'' END +
                    N' ORDER BY
                        b.id;';

        -- Execute the query
        EXEC sp_executesql @SQL, N'@company_id INT, @activity_id INT, @project_id INT', @company_id, @activity_id, @project_id;
    END
    ELSE IF @task_id != 0
    BEGIN

        IF @project_id = 0
            -- Sub_Task Drop Down Query for mapped project
            SET @SQL = N'SELECT 
                            DISTINCT ''Sub_Task'' AS drop_down_name, a.id, a.name, b.id AS uom_id, b.code AS uom_name,
                            NULL AS start_date, NULL AS end_date, NULL AS cost
                        FROM 
                            sub_task a
                            INNER JOIN uom b ON a.uom_id = b.id AND b.active_flag = 1 AND b.delete_flag = 0
                        WHERE 
                            a.active_flag = 1 AND a.delete_flag = 0 AND a.company_id = @company_id AND a.task_id = @task_id
                        ORDER BY
                            a.id;';
        ELSE
            -- Sub_Task Drop Down Query for mapped project
            SET @SQL = N'SELECT 
                            DISTINCT ''Sub_Task'' AS drop_down_name, b.id, b.name, c.id AS uom_id, c.code AS uom_name,
                            a.start_date, a.end_date, a.cost
                        FROM 
                            sub_task_project_mapping a
                            INNER JOIN sub_task b ON a.sub_task_id = b.id AND b.active_flag = 1 AND b.delete_flag = 0
                            INNER JOIN uom c ON b.uom_id = c.id AND c.active_flag = 1 AND c.delete_flag = 0
                        WHERE 
                            b.company_id = @company_id AND b.task_id = @task_id' +
                            CASE WHEN @project_id != 0 THEN N' AND a.project_id = @project_id' ELSE N'' END +
                        N' ORDER BY
                            b.id;';
        -- Execute the query
        EXEC sp_executesql @SQL, N'@company_id INT, @task_id INT, @project_id INT', @company_id, @task_id, @project_id;
    END
    ELSE IF @company_id != 0
    BEGIN
        -- Activity Drop Down Query
        SET @SQL = N'SELECT 
                        DISTINCT ''Activity'' AS drop_down_name, b.id, b.name, NULL AS uom_id, NULL AS uom_name,
                        NULL AS start_date, NULL AS end_date, NULL AS cost
                    FROM 
                        activity_project_mapping a
                        INNER JOIN activity b ON a.activity_id = b.id AND b.active_flag = 1 AND b.delete_flag = 0
                    WHERE 
                        b.company_id = @company_id' +
                        CASE WHEN @project_id != 0 THEN N' AND a.project_id = @project_id' ELSE N'' END +
                    N' ORDER BY
                        b.id;';

        -- Execute the query
        EXEC sp_executesql @SQL, N'@company_id INT, @project_id INT', @company_id, @project_id;
    END
    ELSE
    BEGIN
        -- If no parameters are provided, return an empty result set
        SET @SQL = N'SELECT TOP 0 NULL AS drop_down_name, NULL AS id, NULL AS name;';
        EXEC sp_executesql @SQL;
    END
END;