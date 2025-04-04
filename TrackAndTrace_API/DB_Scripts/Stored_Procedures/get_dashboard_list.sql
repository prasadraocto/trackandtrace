CREATE PROCEDURE [dbo].[get_dashboard_list]
    @user_id INT,
    @project_id INT,
    @company_id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Build the dynamic SQL for the dashboard list
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @ROLE NVARCHAR(50);

    -- Get the role of the user
    SELECT 
	    @ROLE = c.name
    FROM 
	    users a
	    INNER JOIN designation b ON a.designation_id = b.id
	    INNER JOIN roles c ON b.role_id = c.id
    WHERE
	    a.id = @user_id AND a.company_id = @company_id;


    -- Build the dynamic SQL based on the role
    
    IF @ROLE = 'COMPANY_ADMIN'
    BEGIN
        SET @SQL = N'
            SELECT ''Project'' AS name, ''/company-admin-transactions/project'' AS link, COUNT(*) AS count
            FROM project 
            WHERE company_id = @company_id AND active_flag = 1 AND delete_flag = 0
        
            UNION ALL
        
            SELECT ''User'' AS name, ''/company-admin-transactions/company-users'' AS link, COUNT(*) AS count
            FROM users 
            WHERE company_id = @company_id AND active_flag = 1 AND delete_flag = 0
        
            UNION ALL
        
            SELECT ''Designation'' AS name, ''/company-admin-masters/designation'' AS link, COUNT(*) AS count
            FROM designation 
            WHERE company_id = @company_id AND active_flag = 1 AND delete_flag = 0
        
            UNION ALL
        
            SELECT ''Manpower'' AS name, ''/company-admin-transactions/manpower'' AS link, COUNT(*) AS count
            FROM manpower 
            WHERE company_id = @company_id AND active_flag = 1 AND delete_flag = 0
            
            UNION ALL

            SELECT ''Activity'' AS name, ''/company-admin-transactions/activity'' AS link, COUNT(*) AS count
            FROM activity 
            WHERE company_id = @company_id AND active_flag = 1 AND delete_flag = 0

            UNION ALL

            SELECT ''Task'' AS name, ''/company-admin-transactions/task'' AS link, COUNT(*) AS count
            FROM task 
            WHERE company_id = @company_id AND active_flag = 1 AND delete_flag = 0

            UNION ALL

            SELECT ''Sub Task'' AS name, ''/company-admin-transactions/sub-task'' AS link, COUNT(*) AS count
            FROM sub_task 
            WHERE company_id = @company_id AND active_flag = 1 AND delete_flag = 0
            
            UNION ALL

            SELECT ''Material'' AS name, ''/company-admin-transactions/material'' AS link, COUNT(*) AS count
            FROM material 
            WHERE company_id = @company_id AND active_flag = 1 AND delete_flag = 0
            ';
    END
    ELSE IF @ROLE = 'PROJECT_ADMIN'
    BEGIN
        SET @SQL = N'
            SELECT ''Materials'' AS name, ''/project-admin-transactions/project-material'' AS link, COUNT(*) AS count
            FROM project_material_mapping 
            WHERE project_id = @project_id AND delete_flag = 0
        
            UNION ALL

            SELECT ''Manpower'' AS name, NULL AS link, COUNT(*) AS count
            FROM manpower_project_mapping 
            WHERE project_id = @project_id AND delete_flag = 0
            
            UNION ALL

            SELECT ''Activity'' AS name, NULL AS link, COUNT(*) AS count
            FROM activity_project_mapping 
            WHERE project_id = @project_id

            UNION ALL

            SELECT ''Task'' AS name, NULL AS link, COUNT(*) AS count
            FROM task_project_mapping 
            WHERE project_id = @project_id

            UNION ALL
            
            SELECT ''Sub-Task'' AS name, ''/project-admin-transactions/sub-task-mapping'' AS link, COUNT(*) AS count
            FROM sub_task_project_mapping 
            WHERE project_id = @project_id
            ';
    END

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, 
        N'@user_id INT, @project_id INT, @company_id INT',
        @user_id, @project_id, @company_id;
END;