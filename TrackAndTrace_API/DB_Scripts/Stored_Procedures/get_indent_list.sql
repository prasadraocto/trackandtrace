CREATE PROCEDURE [dbo].[get_indent_list]
    @project_id INT,
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
	    indent a
	    INNER JOIN project b ON a.project_id = b.id AND b.delete_flag = 0
	    INNER JOIN users c ON a.created_by = c.id AND c.delete_flag = 0
    WHERE
        a.project_id = @project_id AND
        (
            @search_query IS NULL OR 
            (a.indent_type LIKE '%' + @search_query + '%') OR
		    (a.indent_no LIKE '%' + @search_query + '%') OR
		    (c.name LIKE '%' + @search_query + '%')
        );

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
        a.id, a.request_id, a.project_id, b.name AS project_name, 
        a.indent_type, a.indent_no, a.indent_date, a.status AS indent_status,
        c.id AS raised_by_id, c.name AS raised_by_name,
        COUNT(CASE WHEN d.type = ''B'' THEN 1 END) AS build_count,
        COUNT(CASE WHEN d.type = ''UB'' THEN 1 END) AS unbuild_count
    FROM 
        indent a
        INNER JOIN project b ON a.project_id = b.id AND b.delete_flag = 0
        INNER JOIN users c ON a.created_by = c.id AND c.delete_flag = 0
        INNER JOIN indent_material im ON a.id = im.indent_id
        INNER JOIN material d ON im.material_id = d.id AND d.delete_flag = 0
    WHERE
        a.project_id = @project_id AND
	    (
		    @search_query IS NULL OR 
		    (a.indent_type LIKE ''%'' + @search_query + ''%'') OR
		    (a.indent_no LIKE ''%'' + @search_query + ''%'') OR
		    (c.name LIKE ''%'' + @search_query + ''%'')
	    )
    GROUP BY 
        a.id, a.request_id, a.project_id, b.name, 
        a.indent_type, a.indent_no, a.indent_date, 
        a.status, c.id, c.name
    ORDER BY ';

    BEGIN
        SET @SQL = @SQL + QUOTENAME(@sort_column) + ' ' + @sort_direction;
    END

    SET @SQL = @SQL + ' 
    OFFSET (@page - 1) * @page_size ROWS
    FETCH NEXT @page_size ROWS ONLY;';

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, 
        N'@page INT, @page_size INT, @search_query NVARCHAR(100), @sort_direction NVARCHAR(4), @project_id INT',
        @page, @page_size, @search_query, @sort_direction, @project_id;
END;