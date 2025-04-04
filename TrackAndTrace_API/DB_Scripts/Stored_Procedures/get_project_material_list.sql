CREATE PROCEDURE [dbo].[get_project_material_list]
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
        project_material_mapping a
        INNER JOIN material b ON a.material_id = b.id AND b.delete_flag = 0
        INNER JOIN uom c ON b.uom_id = c.id AND c.delete_flag = 0
        INNER JOIN brand d ON b.brand_id = d.id AND d.delete_flag = 0
        INNER JOIN category e ON b.category_id = e.id AND e.delete_flag = 0
        INNER JOIN specification f ON b.specification_id = f.id AND f.delete_flag = 0
    WHERE 
        a.project_id = @project_id AND a.delete_flag = 0 AND
        (
            @search_query IS NULL OR 
            (b.code LIKE '%' + @search_query + '%') OR
            (b.name LIKE '%' + @search_query + '%') OR
            (c.name LIKE '%' + @search_query + '%') OR
            (d.name LIKE '%' + @search_query + '%') OR
            (e.name LIKE '%' + @search_query + '%') OR
            (f.name LIKE '%' + @search_query + '%')
        );

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
        a.id, a.material_id, b.code, b.name, b.description, a.quantity, c.id as uom_id, c.name as uom_name, 
		d.id as brand_id, d.name as brand_name, e.id as category_id, e.name as category_name, 
		f.id as specification_id, f.name as specification_name
    FROM 
        project_material_mapping a
        INNER JOIN material b ON a.material_id = b.id AND b.delete_flag = 0
        INNER JOIN uom c ON b.uom_id = c.id AND c.delete_flag = 0
        INNER JOIN brand d ON b.brand_id = d.id AND d.delete_flag = 0
        INNER JOIN category e ON b.category_id = e.id AND e.delete_flag = 0
        INNER JOIN specification f ON b.specification_id = f.id AND f.delete_flag = 0
    WHERE 
        a.project_id = @project_id AND a.delete_flag = 0 AND
        (
            @search_query IS NULL OR 
            (a.code LIKE ''%'' + @search_query + ''%'') OR
            (a.name LIKE ''%'' + @search_query + ''%'') OR
            (c.name LIKE ''%'' + @search_query + ''%'') OR
            (d.name LIKE ''%'' + @search_query + ''%'') OR
            (e.name LIKE ''%'' + @search_query + ''%'') OR
            (f.name LIKE ''%'' + @search_query + ''%'')
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
        N'@page INT, @page_size INT, @search_query NVARCHAR(100), @sort_direction NVARCHAR(4), @project_id INT',
        @page, @page_size, @search_query, @sort_direction, @project_id;
END;