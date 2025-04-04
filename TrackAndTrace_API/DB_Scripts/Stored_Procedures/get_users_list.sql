CREATE PROCEDURE [dbo].[get_users_list]
    @user_id INT,
    @company_id INT,
    @page INT = 1,
    @page_size INT = 10,
    @search_query NVARCHAR(100) = NULL,
    @sort_column NVARCHAR(50) = 'id',
    @sort_direction NVARCHAR(4) = 'desc',
    @total_count INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Declare the variable for role_name
    DECLARE @role_name NVARCHAR(50);

    -- Set the role_name variable based on the user_id
    SELECT
        @role_name = c.name
    FROM
        users a
        INNER JOIN designation b ON a.designation_id = b.id AND b.active_flag = 1 AND b.delete_flag = 0
        INNER JOIN roles c ON b.role_id = c.id
    WHERE
        a.id = @user_id;

    -- Get the total count of records that meet the filtering criteria
    SELECT 
        @total_count = COUNT(*)
    FROM 
        users a
        INNER JOIN designation b ON a.designation_id = b.id AND b.active_flag = 1 AND b.delete_flag = 0
        INNER JOIN roles c ON b.role_id = c.id
        INNER JOIN company d ON a.company_id = d.id AND d.active_flag = 1 AND d.delete_flag = 0
    WHERE 
        (
            -- If the user's role is SUPER_ADMIN, get all COMPANY_ADMIN users without considering company_id
            @role_name = 'SUPER_ADMIN' AND c.name = 'COMPANY_ADMIN'
            OR
            -- For other roles, filter by company_id
            a.company_id = @company_id
        )
        AND (
            @search_query IS NULL OR 
            (a.code LIKE '%' + @search_query + '%') OR
            (a.name LIKE '%' + @search_query + '%') OR
            (a.email LIKE '%' + @search_query + '%') OR
            (a.phone LIKE '%' + @search_query + '%') OR
            (b.name LIKE '%' + @search_query + '%') OR
            (c.name LIKE '%' + @search_query + '%')
        )
        -- Exclude SUPER_ADMIN users from the result
        AND a.delete_flag = 0 AND c.name <> 'SUPER_ADMIN';

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
        a.id, a.code, a.name, a.email, a.password, a.phone, b.id as designation_id, 
        b.name as designation_name, c.id as role_id, c.name as role_name, 
		d.id as company_id, d.name as company_name, a.active_flag
    FROM 
        users a
        INNER JOIN designation b ON a.designation_id = b.id AND b.delete_flag = 0
        INNER JOIN roles c ON b.role_id = c.id
        INNER JOIN company d ON a.company_id = d.id AND d.active_flag = 1 AND d.delete_flag = 0
    WHERE 
        (
            -- If the users role is SUPER_ADMIN, get all COMPANY_ADMIN users without considering company_id
            @role_name = ''SUPER_ADMIN'' AND c.name = ''COMPANY_ADMIN''
            OR
            -- For other roles, filter by company_id
            a.company_id = @company_id AND a.delete_flag = 0
        )
        AND (
            @search_query IS NULL OR 
            (a.code LIKE ''%'' + @search_query + ''%'') OR
            (a.name LIKE ''%'' + @search_query + ''%'') OR
            (a.email LIKE ''%'' + @search_query + ''%'') OR
            (a.phone LIKE ''%'' + @search_query + ''%'') OR
            (b.name LIKE ''%'' + @search_query + ''%'') OR
            (c.name LIKE ''%'' + @search_query + ''%'')
        )
        -- Exclude SUPER_ADMIN users from the result
        AND a.delete_flag = 0 AND c.name <> ''SUPER_ADMIN''
    ORDER BY ';

    BEGIN
        SET @SQL = @SQL + QUOTENAME(@sort_column) + ' ' + @sort_direction;
    END

    SET @SQL = @SQL + ' 
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;';

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, 
        N'@page INT, @page_size INT, @search_query NVARCHAR(100), @sort_direction NVARCHAR(4), @user_id INT, @company_id INT, @role_name NVARCHAR(50)',
        @page, @page_size, @search_query, @sort_direction, @user_id, @company_id, @role_name;
END;
