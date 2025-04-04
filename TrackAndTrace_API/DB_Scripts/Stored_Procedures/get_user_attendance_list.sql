CREATE PROCEDURE [dbo].[get_user_attendance_list]
    @user_id INT,
    @company_id INT,
    @page INT = 1,
    @page_size INT = 10,
    @search_query NVARCHAR(100) = NULL,
    @sort_column NVARCHAR(50) = 'id',
    @sort_direction NVARCHAR(4) = 'desc',
    @is_export BIT = 0,
    @from_date DATE = NULL,
    @to_date DATE = NULL,
    @total_count INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Assign default values if NULL
    IF @from_date IS NULL SET @from_date = CAST(GETDATE() AS DATE);
    IF @to_date IS NULL SET @to_date = CAST(GETDATE() AS DATE);

    DECLARE @Role_Name VARCHAR(50);

    -- Get the user role
    SELECT @Role_Name = c.name
    FROM users a 
        INNER JOIN designation b ON a.designation_id = b.id AND b.delete_flag = 0
        INNER JOIN roles c ON b.role_id = c.id
    WHERE a.id = @user_id AND a.active_flag = 1 AND a.delete_flag = 0;

    -- Get total count of records
    SELECT @total_count = COUNT(*)
    FROM user_attendance a
        INNER JOIN users b ON a.user_id = b.id AND b.active_flag = 1 AND b.delete_flag = 0
        INNER JOIN designation c ON b.designation_id = c.id AND c.delete_flag = 0
        INNER JOIN roles d ON c.role_id = d.id
    WHERE 
        (CASE WHEN @Role_Name IN ('COMPANY_ADMIN', 'CHAIRMAN') THEN a.user_id ELSE @user_id END) = a.user_id
        AND a.company_id = @company_id 
        AND a.delete_flag = 0
        AND (
            TRY_CONVERT(DATETIME2, a.attendance_timestamp, 127) BETWEEN @from_date AND DATEADD(DAY, 1, @to_date)
        )
        AND (
            @search_query IS NULL OR 
            a.address LIKE '%' + @search_query + '%' OR
            a.attendance_type LIKE '%' + @search_query + '%' OR
            b.code LIKE '%' + @search_query + '%' OR
            b.name LIKE '%' + @search_query + '%' OR
            c.name LIKE '%' + @search_query + '%'
        );

    -- Build the dynamic SQL query
    DECLARE @SQL NVARCHAR(MAX);

    SET @SQL = N'SELECT 
        a.id, a.user_id, b.code AS user_code, b.name AS user_name, b.designation_id, c.name AS designation_name,
        a.latitude, a.longitude, a.address, a.image, a.attendance_type, 
        
        -- Convert attendance_timestamp
        CASE 
            -- If the timestamp contains 7 fractional digits, convert and subtract 5:30 hours
            WHEN a.attendance_timestamp LIKE ''%.0000000Z'' THEN 
                FORMAT(DATEADD(MINUTE, -330, TRY_CONVERT(DATETIME2, a.attendance_timestamp, 127)), ''yyyy-MM-ddTHH:mm:ss.fffZ'')
            
            -- If its already in the correct format, return as is
            ELSE a.attendance_timestamp 
        END AS attendance_date

    FROM user_attendance a
        INNER JOIN users b ON a.user_id = b.id AND b.active_flag = 1 AND b.delete_flag = 0
        INNER JOIN designation c ON b.designation_id = c.id AND c.delete_flag = 0
        INNER JOIN roles d ON c.role_id = d.id
    WHERE 
        (CASE WHEN @Role_Name IN (''COMPANY_ADMIN'', ''CHAIRMAN'') THEN a.user_id ELSE @user_id END) = a.user_id
        AND a.company_id = @company_id
        AND a.delete_flag = 0
        AND (TRY_CONVERT(DATETIME2, a.attendance_timestamp, 127) BETWEEN @from_date AND DATEADD(DAY, 1, @to_date))
        AND (
            @search_query IS NULL OR 
            a.address LIKE ''%'' + @search_query + ''%'' OR
            a.attendance_type LIKE ''%'' + @search_query + ''%'' OR
            b.code LIKE ''%'' + @search_query + ''%'' OR
            b.name LIKE ''%'' + @search_query + ''%'' OR
            c.name LIKE ''%'' + @search_query + ''%''
        )
    ORDER BY ' + QUOTENAME(@sort_column) + ' ' + @sort_direction;

    -- Apply pagination only if @is_export = 0
    IF @is_export = 0
    BEGIN
        SET @SQL = @SQL + ' 
        OFFSET (@page - 1) * @page_size ROWS
        FETCH NEXT @page_size ROWS ONLY;';
    END;

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, 
        N'@page INT, @page_size INT, @search_query NVARCHAR(100), @sort_direction NVARCHAR(4), 
        @is_export BIT, @Role_Name VARCHAR(50), @user_id INT, @company_id INT, 
        @from_date DATE, @to_date DATE',
        @page, @page_size, @search_query, @sort_direction, @is_export, @Role_Name, 
        @user_id, @company_id, @from_date, @to_date;
END;