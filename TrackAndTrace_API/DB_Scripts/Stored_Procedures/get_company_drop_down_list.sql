CREATE PROCEDURE [dbo].[get_company_drop_down_list]
    @user_id INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @ROLE_NAME NVARCHAR(50);
    DECLARE @ORG_ID INT;

    -- Get the role of the user
    SELECT 
        @ROLE_NAME = r.name, 
        @ORG_ID = c.organization_id
    FROM 
        users u
    JOIN designation d ON u.designation_id = d.id
    JOIN roles r ON d.role_id = r.id
    JOIN company c ON u.company_id = c.id
    WHERE 
        u.id = @user_id;

    -- Build SQL query based on role
    SET @SQL = N'SELECT a.id, a.name
        FROM company a
        WHERE a.active_flag = 1 
          AND a.delete_flag = 0 
          AND NOT EXISTS 
          (
              SELECT 1 
              FROM designation d
              JOIN roles r ON d.role_id = r.id
              WHERE r.name = ''SUPER_ADMIN'' 
                AND d.company_id = a.id
          )';

    -- Apply additional filter if the user is not a SUPER_ADMIN
    IF @ROLE_NAME <> 'SUPER_ADMIN'
    BEGIN
        SET @SQL = @SQL + N' AND a.organization_id = @ORG_ID';
    END

    -- Append ORDER BY clause
    SET @SQL = @SQL + N' ORDER BY a.name';

    -- Execute the dynamic SQL
    EXEC sp_executesql @SQL, 
        N'@user_id INT, @ORG_ID INT', 
        @user_id, @ORG_ID;
END;