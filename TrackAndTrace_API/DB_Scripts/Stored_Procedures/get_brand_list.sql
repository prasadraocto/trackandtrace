CREATE PROCEDURE [dbo].[get_brand_list]
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
        brand a
    WHERE 
        a.company_id = @company_id AND a.delete_flag = 0 AND
        (
            @search_query IS NULL OR 
            (a.code LIKE '%' + @search_query + '%') OR
            (a.name LIKE '%' + @search_query + '%')
        );

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
        a.id, a.code, a.name, a.active_flag
    FROM 
        brand a
    WHERE 
        a.company_id = @company_id AND a.delete_flag = 0 AND
        (
            @search_query IS NULL OR 
            (a.code LIKE ''%'' + @search_query + ''%'') OR
            (a.name LIKE ''%'' + @search_query + ''%'')
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