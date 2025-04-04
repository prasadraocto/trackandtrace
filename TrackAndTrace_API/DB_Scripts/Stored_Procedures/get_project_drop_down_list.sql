CREATE PROCEDURE [dbo].[get_project_drop_down_list]
    @company_id INT = 0,
    @activity_id INT = 0,
    @task_id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    -- Declare a variable to hold the SQL query
    DECLARE @SQL NVARCHAR(MAX);

    -- Build the dynamic SQL based on the input parameters
    IF @activity_id != 0
    BEGIN
        -- Activity Query for Task creation
        SET @SQL = N'SELECT 
                        b.id, b.name
                    FROM 
                        activity_project_mapping a
                        INNER JOIN project b ON a.project_id = b.id AND b.active_flag = 1 AND b.delete_flag = 0
                    WHERE 
                        a.activity_id = @activity_id
                    ORDER BY
                        a.id;';

        -- Execute the query
        EXEC sp_executesql @SQL, N'@activity_id INT', @activity_id;
    END
    ELSE IF @task_id != 0
    BEGIN
        -- Task Query for Sub-Task creation
        SET @SQL = N'SELECT 
                        b.id, b.name
                    FROM 
                        task_project_mapping a
                        INNER JOIN project b ON a.project_id = b.id AND b.active_flag = 1 AND b.delete_flag = 0
                    WHERE 
                        a.task_id = @task_id
                    ORDER BY
                        a.id;';

        -- Execute the query
        EXEC sp_executesql @SQL, N'@task_id INT', @task_id;
    END
    ELSE IF @company_id != 0
    BEGIN
        -- All project query
        SET @SQL = N'SELECT 
                        a.id, a.name
                    FROM 
                        project a
                    WHERE 
                        a.company_id = @company_id AND a.active_flag = 1 AND a.delete_flag = 0
                    ORDER BY
                        a.id;';

        -- Execute the query
        EXEC sp_executesql @SQL, N'@company_id INT', @company_id;
    END
    ELSE
    BEGIN
        -- If no parameters are provided, return an empty result set
        SET @SQL = N'SELECT TOP 0 NULL AS id, NULL AS name;';
        EXEC sp_executesql @SQL;
    END
END;