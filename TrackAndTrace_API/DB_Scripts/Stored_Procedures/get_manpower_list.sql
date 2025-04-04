CREATE PROCEDURE [dbo].[get_manpower_list]
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
        manpower a
        INNER JOIN designation d ON a.designation_id = d.id AND d.delete_flag = 0
        INNER JOIN users eu ON a.engineer_id = eu.id AND eu.delete_flag = 0
        LEFT JOIN users cu ON a.charge_hand_id = cu.id AND cu.delete_flag = 0
        LEFT JOIN users gu ON a.gang_leader_id = gu.id AND gu.delete_flag = 0
        LEFT JOIN subcontractor s ON a.subcontractor_id = s.id AND s.delete_flag = 0
    WHERE 
        a.company_id = @company_id AND a.delete_flag = 0 AND
        (
            @search_query IS NULL OR 
            (a.code LIKE '%' + @search_query + '%') OR
            (a.name LIKE '%' + @search_query + '%') OR
            (eu.name LIKE '%' + @search_query + '%') OR
            (cu.name LIKE '%' + @search_query + '%') OR
            (gu.name LIKE '%' + @search_query + '%') OR
            (s.name LIKE '%' + @search_query + '%') OR
            (a.rating LIKE '%' + @search_query + '%')
        );

    -- Build the dynamic SQL for sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
        a.id, a.code, a.name, a.designation_id, d.name as designation_name, 
        a.engineer_id, eu.name as engineer_name, a.charge_hand_id, cu.name as charge_hand_name, 
        a.gang_leader_id, gu.name as gang_leader_name, 
        a.subcontractor_id, s.name as subcontractor_name, a.rating, a.active_flag
    FROM 
        manpower a
        INNER JOIN designation d ON a.designation_id = d.id AND d.delete_flag = 0
        INNER JOIN users eu ON a.engineer_id = eu.id AND eu.delete_flag = 0
        LEFT JOIN users cu ON a.charge_hand_id = cu.id AND cu.delete_flag = 0
        LEFT JOIN users gu ON a.gang_leader_id = gu.id AND gu.delete_flag = 0
        LEFT JOIN subcontractor s ON a.subcontractor_id = s.id AND s.delete_flag = 0
    WHERE 
        a.company_id = @company_id AND a.delete_flag = 0 AND
        (
            @search_query IS NULL OR 
            (a.code LIKE ''%'' + @search_query + ''%'') OR
            (a.name LIKE ''%'' + @search_query + ''%'') OR
            (eu.name LIKE ''%'' + @search_query + ''%'') OR
            (cu.name LIKE ''%'' + @search_query + ''%'') OR
            (gu.name LIKE ''%'' + @search_query + ''%'') OR
            (s.name LIKE ''%'' + @search_query + ''%'') OR
            (a.rating LIKE ''%'' + @search_query + ''%'')
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