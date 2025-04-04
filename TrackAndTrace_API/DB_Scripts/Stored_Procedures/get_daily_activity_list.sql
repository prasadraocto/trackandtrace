CREATE PROCEDURE [dbo].[get_daily_activity_list]
    @company_id INT,
    @project_id INT = 0,
    @from_date DATE = NULL,
    @to_date DATE = NULL,
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
        @total_count = COUNT(DISTINCT a.id)
    FROM 
        trx_daily_activity_details a
        INNER JOIN trx_daily_activity_item b ON a.activity_item_id = b.id
        INNER JOIN activity c ON b.activity_id = c.id AND c.delete_flag = 0
        INNER JOIN task d ON b.task_id = d.id AND d.delete_flag = 0
        INNER JOIN sub_task e ON b.sub_task_id = e.id AND e.delete_flag = 0
        INNER JOIN project f ON a.project_id = f.id AND f.delete_flag = 0
        INNER JOIN uom g ON e.uom_id = g.id AND g.delete_flag = 0
        INNER JOIN shift h ON a.shift_id = h.id AND h.delete_flag = 0
        INNER JOIN project_level_mapping i ON a.project_level_id = i.id AND i.delete_flag = 0
    WHERE 
        a.project_id = CASE WHEN @project_id = 0 THEN a.project_id ELSE @project_id END AND 
        a.company_id = @company_id AND a.delete_flag = 0 AND
        (
			(@from_date IS NULL OR a.created_date >= @from_date) AND
			(@to_date IS NULL OR a.created_date <= @to_date) AND
			(@search_query IS NULL OR 
				(c.name LIKE '%' + @search_query + '%') OR
				(d.name LIKE '%' + @search_query + '%') OR
				(e.name LIKE '%' + @search_query + '%') OR
                (f.name LIKE '%' + @search_query + '%') OR
				(h.name LIKE '%' + @search_query + '%') OR
				(i.name LIKE '%' + @search_query + '%')
			)
		);

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
       DISTINCT a.id, CONCAT(c.name, '' - '', d.name, '' - '', e.name) AS daily_activity_name, 
       a.created_date as activity_date, a.project_id, f.name as project_name, a.quantity, g.name as uom_name,
       a.shift_id, h.name as shift_name, a.hrs_spent, i.name as project_level_name, a.is_draft, a.status
    FROM 
        trx_daily_activity_details a
        INNER JOIN trx_daily_activity_item b ON a.activity_item_id = b.id
        INNER JOIN activity c ON b.activity_id = c.id AND c.delete_flag = 0
        INNER JOIN task d ON b.task_id = d.id AND d.delete_flag = 0
        INNER JOIN sub_task e ON b.sub_task_id = e.id AND e.delete_flag = 0
        INNER JOIN project f ON a.project_id = f.id AND f.delete_flag = 0
        INNER JOIN uom g ON e.uom_id = g.id AND g.delete_flag = 0
        INNER JOIN shift h ON a.shift_id = h.id AND h.delete_flag = 0
        INNER JOIN project_level_mapping i ON a.project_level_id = i.id AND i.delete_flag = 0
    WHERE 
        a.project_id = CASE WHEN @project_id = 0 THEN a.project_id ELSE @project_id END AND 
        a.company_id = @company_id AND a.delete_flag = 0 AND
        (
			(@from_date IS NULL OR a.created_date >= @from_date) AND
			(@to_date IS NULL OR a.created_date <= @to_date) AND
			(@search_query IS NULL OR 
				(c.name LIKE ''%'' + @search_query + ''%'') OR
				(d.name LIKE ''%'' + @search_query + ''%'') OR
				(e.name LIKE ''%'' + @search_query + ''%'') OR
                (f.name LIKE ''%'' + @search_query + ''%'') OR
                (h.name LIKE ''%'' + @search_query + ''%'') OR
                (i.name LIKE ''%'' + @search_query + ''%'')
			)
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
        N'@page INT, @page_size INT, @search_query NVARCHAR(100), @sort_direction NVARCHAR(4), @company_id INT, @project_id INT, @from_date DATE, @to_date DATE',
        @page, @page_size, @search_query, @sort_direction, @company_id, @project_id, @from_date, @to_date;
END;