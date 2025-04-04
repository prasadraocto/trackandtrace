CREATE PROCEDURE [dbo].[get_work_flow_pending_request]
    @user_id INT,
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
        @total_count = COUNT(DISTINCT a.request_id)
    FROM
        trx_work_flow_approval_status a
        INNER JOIN work_flow_project_user_mapping b ON a.wf_project_user_id = b.id AND b.delete_flag = 0
        INNER JOIN work_flow c ON b.work_flow_id = c.id AND c.delete_flag = 0
        INNER JOIN project d ON b.project_id = d.id AND d.delete_flag = 0
        INNER JOIN bulk_import_details e ON a.request_id = e.id
        INNER JOIN users f ON b.user_id = f.id
		LEFT JOIN boq_request_hdr g ON a.request_id = g.request_id
		LEFT JOIN indent i ON a.request_id = i.request_id
    WHERE
        c.company_id = @company_id
        -- Exclude rejected requests
        AND NOT EXISTS (
            SELECT 1
            FROM trx_work_flow_approval_status a2
            WHERE a2.request_id = a.request_id
              AND a2.status IN ('rejected', 'cancelled')
        )
        -- Only include pending requests
        AND a.status = 'pending'
        -- Ensure @user_id is the first user in the workflow for that request
        AND EXISTS (
            SELECT 1
            FROM trx_work_flow_approval_status a3
            INNER JOIN work_flow_project_user_mapping b3 ON a3.wf_project_user_id = b3.id
            WHERE a3.request_id = a.request_id
              AND b3.user_id = @user_id
            ORDER BY a3.order_id ASC
            OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY
        )
        AND (
            @search_query IS NULL OR 
            (c.name LIKE '%' + @search_query + '%') OR 
            (d.name LIKE '%' + @search_query + '%') OR 
            (e.name LIKE '%' + @search_query + '%') OR 
            (f.name LIKE '%' + @search_query + '%') OR
            (g.boq_request_no LIKE '%' + @search_query + '%') OR
            (i.indent_no LIKE '%' + @search_query + '%')
        );


    -- Build the dynamic SQL for sorting and filtering
    DECLARE @SQL NVARCHAR(MAX);
    SET @SQL = N'
        WITH RankedRequests AS (
            SELECT
                a.id, 
                c.name AS request_type, 
                b.project_id, 
                d.name AS project_name, 
                a.request_id, 
                COALESCE(g.boq_request_no, i.indent_no, e.name) AS import_name, 
                a.order_id,
                b.user_id AS current_user_id,
                f.name AS current_user_name,
                -- Next User Calculation
                LEAD(b.user_id) OVER (PARTITION BY a.request_id ORDER BY a.order_id ASC) AS next_user_id,
                LEAD(f.name) OVER (PARTITION BY a.request_id ORDER BY a.order_id ASC) AS next_user_name,
                ROW_NUMBER() OVER (PARTITION BY a.request_id ORDER BY a.order_id ASC) AS rn
            FROM
                trx_work_flow_approval_status a
                INNER JOIN work_flow_project_user_mapping b ON a.wf_project_user_id = b.id AND b.delete_flag = 0
                INNER JOIN work_flow c ON b.work_flow_id = c.id AND c.delete_flag = 0
                INNER JOIN project d ON b.project_id = d.id AND d.delete_flag = 0
                INNER JOIN bulk_import_details e ON a.request_id = e.id
                INNER JOIN users f ON b.user_id = f.id
				LEFT JOIN boq_request_hdr g ON a.request_id = g.request_id
				LEFT JOIN indent i ON a.request_id = i.request_id
            WHERE
                c.company_id = @company_id
                -- Only consider rows that have pending status
                AND a.status = ''pending''
                AND NOT EXISTS (
                    SELECT 1
                    FROM trx_work_flow_approval_status a2
                    WHERE a2.request_id = a.request_id
                      AND a2.status IN (''rejected'', ''cancelled'')
                )
        )
        SELECT
            id, 
            request_type, 
            project_id, 
            project_name, 
            request_id, 
            import_name, 
            current_user_id, 
            current_user_name, 
            next_user_id, 
            next_user_name, 
            order_id
        FROM
            RankedRequests
        WHERE
            rn = 1 -- Get the first row per request_id
        AND (
            @search_query IS NULL OR 
            (request_type LIKE ''%'' + @search_query + ''%'') OR 
            (project_name LIKE ''%'' + @search_query + ''%'') OR 
            (import_name LIKE ''%'' + @search_query + ''%'') OR 
            (current_user_name LIKE ''%'' + @search_query + ''%'')
        )
        -- Filter by user_id for the first user in the approval workflow
        AND current_user_id = @user_id
        ORDER BY ' + QUOTENAME(@sort_column) + ' ' + @sort_direction + '
        OFFSET (@page - 1) * @page_size ROWS
        FETCH NEXT @page_size ROWS ONLY;
    ';

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL,
        N'@page INT, @page_size INT, @search_query NVARCHAR(100), @user_id INT, @company_id INT',
        @page, @page_size, @search_query, @user_id, @company_id;
END;