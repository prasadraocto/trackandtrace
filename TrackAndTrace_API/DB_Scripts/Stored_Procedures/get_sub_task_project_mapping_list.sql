CREATE PROCEDURE [dbo].[get_sub_task_project_mapping_list]
    @project_id INT,
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

    -- Get the total count of records that meet the filtering criteria
    SELECT 
        @total_count = COUNT(*)
    FROM 
        sub_task_project_mapping a
        INNER JOIN sub_task b ON a.sub_task_id = b.id AND b.delete_flag = 0
        INNER JOIN activity c ON b.activity_id = c.id AND c.delete_flag = 0
        INNER JOIN task d ON b.task_id = d.id AND d.delete_flag = 0
        INNER JOIN uom e ON b.uom_id = e.id AND e.active_flag = 1 AND e.delete_flag = 0
    WHERE 
        a.project_id = @project_id AND b.company_id = @company_id AND
        (
            @search_query IS NULL OR 
            (b.code LIKE '%' + @search_query + '%') OR
            (b.name LIKE '%' + @search_query + '%') OR
            (c.name LIKE '%' + @search_query + '%') OR
            (d.name LIKE '%' + @search_query + '%') OR
            (e.name LIKE '%' + @search_query + '%')
        );

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
        a.id, a.sub_task_id, b.code, b.name, b.is_prime, a.cost, a.start_date, a.end_date, 
        c.id as activity_id, c.name as activity_name, 
        d.id as task_id, d.name as task_name, e.id as uom_id, e.name as uom_name
    FROM 
        sub_task_project_mapping a
        INNER JOIN sub_task b ON a.sub_task_id = b.id AND b.delete_flag = 0
        INNER JOIN activity c ON b.activity_id = c.id AND c.delete_flag = 0
        INNER JOIN task d ON b.task_id = d.id AND d.delete_flag = 0
        INNER JOIN uom e ON b.uom_id = e.id AND e.active_flag = 1 AND e.delete_flag = 0
    WHERE 
        a.project_id = @project_id AND b.company_id = @company_id AND
        (
            @search_query IS NULL OR 
            (b.code LIKE ''%'' + @search_query + ''%'') OR
            (b.name LIKE ''%'' + @search_query + ''%'') OR
            (c.name LIKE ''%'' + @search_query + ''%'') OR
            (d.name LIKE ''%'' + @search_query + ''%'') OR
            (e.name LIKE ''%'' + @search_query + ''%'')
        )
    ORDER BY ';

    BEGIN
        SET @SQL = @SQL + QUOTENAME(@sort_column) + ' ' + @sort_direction;
    END

    SET @SQL = @SQL + ' 
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;';

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, 
        N'@page INT, @page_size INT, @search_query NVARCHAR(100), @sort_direction NVARCHAR(4), @project_id INT, @company_id INT',
        @page, @page_size, @search_query, @sort_direction, @project_id, @company_id;
END;