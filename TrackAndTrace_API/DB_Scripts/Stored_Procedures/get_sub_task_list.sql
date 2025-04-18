CREATE PROCEDURE [dbo].[get_sub_task_list]
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
        sub_task a
        INNER JOIN activity b ON a.activity_id = b.id AND b.delete_flag = 0
        INNER JOIN task c ON a.task_id = c.id AND c.delete_flag = 0
        INNER JOIN uom d ON a.uom_id = d.id AND d.active_flag = 1 AND d.delete_flag = 0
    WHERE 
        a.company_id = @company_id AND a.delete_flag = 0 AND
        (
            @search_query IS NULL OR 
            (a.code LIKE '%' + @search_query + '%') OR
            (a.name LIKE '%' + @search_query + '%') OR
            (b.name LIKE '%' + @search_query + '%') OR
            (c.name LIKE '%' + @search_query + '%') OR
            (d.name LIKE '%' + @search_query + '%')
        );

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
        a.id, a.code, a.name, a.is_prime, 
        b.id as activity_id, b.name as activity_name, 
        c.id as task_id, c.name as task_name, d.id as uom_id, d.name as uom_name, 
        a.estimated_days, a.active_flag
    FROM 
        sub_task a
        INNER JOIN activity b ON a.activity_id = b.id AND b.delete_flag = 0
        INNER JOIN task c ON a.task_id = c.id AND c.delete_flag = 0
        INNER JOIN uom d ON a.uom_id = d.id AND d.active_flag = 1 AND d.delete_flag = 0
    WHERE 
        a.company_id = @company_id AND a.delete_flag = 0 AND
        (
            @search_query IS NULL OR 
            (a.code LIKE ''%'' + @search_query + ''%'') OR
            (a.name LIKE ''%'' + @search_query + ''%'') OR
            (b.name LIKE ''%'' + @search_query + ''%'') OR
            (c.name LIKE ''%'' + @search_query + ''%'') OR
            (d.name LIKE ''%'' + @search_query + ''%'')
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
        N'@page INT, @page_size INT, @search_query NVARCHAR(100), @sort_direction NVARCHAR(4), @company_id INT',
        @page, @page_size, @search_query, @sort_direction, @company_id;
END;