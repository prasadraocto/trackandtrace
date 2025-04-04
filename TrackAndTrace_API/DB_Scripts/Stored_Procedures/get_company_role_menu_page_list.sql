CREATE PROCEDURE [dbo].[get_company_role_menu_page_list]
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
        company_role_menu_page_mapping a
		INNER JOIN company b ON a.company_id = b.id AND b.delete_flag = 0
		INNER JOIN role c ON a.role_id = c.id
		INNER JOIN menu d ON a.menu_id = d.id
		INNER JOIN page e ON a.page_id = e.id
    WHERE 
        (
            @search_query IS NULL OR 
            (b.name LIKE '%' + @search_query + '%') OR 
            (c.name LIKE '%' + @search_query + '%') OR 
            (d.name LIKE '%' + @search_query + '%') OR 
            (e.name LIKE '%' + @search_query + '%')
        );

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
        a.id,
        b.id AS company_id,
        b.name AS company_name,
        c.id AS role_id,
        c.name AS role_name,
        d.id AS menu_id,
        d.name AS menu_name,
        e.id AS page_id,
        e.name AS page_name,
        a.mapping_order
    FROM 
        company_role_menu_page_mapping a
		INNER JOIN company b ON a.company_id = b.id AND b.delete_flag = 0
		INNER JOIN role c ON a.role_id = c.id
		INNER JOIN menu d ON a.menu_id = d.id
		INNER JOIN page e ON a.page_id = e.id
    WHERE 
        (
            @search_query IS NULL OR 
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
        N'@page INT, @page_size INT, @search_query NVARCHAR(100), @sort_direction NVARCHAR(4)',
        @page, @page_size, @search_query, @sort_direction;
END