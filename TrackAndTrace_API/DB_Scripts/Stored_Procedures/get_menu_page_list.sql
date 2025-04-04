CREATE PROCEDURE [dbo].[get_menu_page_list]
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
        menu_page_mapping a
		INNER JOIN menu b ON a.menu_id = b.id
		INNER JOIN page c ON a.page_id = c.id
    WHERE 
        (
            @search_query IS NULL OR 
            (b.name LIKE '%' + @search_query + '%') OR 
            (c.name LIKE '%' + @search_query + '%')
        );

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
        a.id,
        b.id AS menu_id,
        b.name AS menu_name,
        c.id AS page_id,
        c.name AS page_name,
        c.url,
        a.mapping_order
    FROM 
        menu_page_mapping a
		INNER JOIN menu b ON a.menu_id = b.id
		INNER JOIN page c ON a.page_id = c.id
    WHERE 
        (
            @search_query IS NULL OR 
            (b.name LIKE ''%'' + @search_query + ''%'') OR 
            (c.name LIKE ''%'' + @search_query + ''%'')
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
        N'@page INT, @page_size INT, @search_query NVARCHAR(100), @sort_direction NVARCHAR(4)',
        @page, @page_size, @search_query, @sort_direction;
END