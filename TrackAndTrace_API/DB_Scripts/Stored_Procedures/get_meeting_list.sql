CREATE PROCEDURE [dbo].[get_meeting_list]
    @company_id INT,
    @start_date VARCHAR(50),
    @end_date VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Build the dynamic SQL for filtering and sorting
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'SELECT 
                    a.id, a.title, a.agenda, a.meeting_date, a.start_time,
                    a.end_time, a.meeting_url, a.color, a.status
                FROM 
                    meeting a
                WHERE 
                    a.company_id = @company_id 
                    AND a.delete_flag = 0
                    AND a.meeting_date >= CAST(@start_date AS DATETIME)
                    AND a.meeting_date <= CAST(@end_date AS DATETIME)
                ORDER BY
                    a.id;';

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, 
        N'@company_id INT, @start_date VARCHAR(50), @end_date VARCHAR(50)',
        @company_id, @start_date, @end_date;
END;