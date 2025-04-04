CREATE PROCEDURE [dbo].[get_daily_activity_manpower_drop_down_list]
    @project_id INT,
    @user_id INT,
    @shift_id NVARCHAR(50),
    @hrs_spent DECIMAL(18, 2),
    @activity_date DATE,
    @old_hrs_spent DECIMAL(18, 2),
    @old_manpower_ids NVARCHAR(MAX),
    @response_message NVARCHAR(1000) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalWorkShiftHrs DECIMAL(18, 2);
    
    -- Use table variable with primary key for better performance
    DECLARE @ManpowerIds TABLE (
        manpower_id INT PRIMARY KEY
    );

    -- Cache the total work hours to avoid repeated lookups
    SELECT @TotalWorkShiftHrs = work_hours
    FROM project WITH (NOLOCK)
    WHERE id = @project_id;

    -- Handle Old Manpower IDs more efficiently
    IF @old_manpower_ids IS NOT NULL AND LEN(@old_manpower_ids) > 0
    BEGIN
        INSERT INTO @ManpowerIds (manpower_id)
        SELECT DISTINCT TRY_CAST(value AS INT)
        FROM STRING_SPLIT(@old_manpower_ids, ',')
        WHERE TRY_CAST(value AS INT) IS NOT NULL;
    END

    -- Use temp table for better performance with larger datasets
    CREATE TABLE #EligibleManpower (
        manpower_id INT PRIMARY KEY
    );

    -- Populate eligible manpower more efficiently
    INSERT INTO #EligibleManpower (manpower_id)
    SELECT DISTINCT m.id
    FROM manpower m WITH (NOLOCK)
    INNER JOIN manpower_project_mapping mp WITH (NOLOCK) 
        ON m.id = mp.manpower_id
    WHERE m.delete_flag = 0
        AND m.active_flag = 1
        AND mp.delete_flag = 0
        AND mp.project_id = @project_id
        AND m.engineer_id = @user_id;

    -- Merge ManpowerIds
    INSERT INTO @ManpowerIds (manpower_id)
    SELECT manpower_id 
    FROM #EligibleManpower em
    WHERE NOT EXISTS (
        SELECT 1 FROM @ManpowerIds mi 
        WHERE mi.manpower_id = em.manpower_id
    );

    -- Check if we have any records before proceeding
    IF EXISTS (SELECT 1 FROM @ManpowerIds)
    BEGIN
        -- Use temp table to store intermediate results
        CREATE TABLE #ManpowerDetails (
            manpower_id INT,
            manpower_code NVARCHAR(100),
            manpower_name NVARCHAR(100),
            designation_name NVARCHAR(100),
            total_spent_hrs DECIMAL(18, 2),
            remaining_work_hrs DECIMAL(18, 2)
        );

        -- Calculate spent hours more efficiently
        INSERT INTO #ManpowerDetails
        SELECT 
            a.id AS manpower_id,
            a.code AS manpower_code,
            a.name AS manpower_name,
            d.name AS designation_name,
            COALESCE(SUM(a2.hrs_spent), 0),
            0 -- placeholder for remainingWorkHrs
        FROM manpower a WITH (NOLOCK)
        INNER JOIN designation d WITH (NOLOCK) ON a.designation_id = d.id
        INNER JOIN @ManpowerIds m ON a.id = m.manpower_id
        LEFT JOIN trx_daily_activity_manpower b WITH (NOLOCK) ON a.id = b.manpower_id AND b.delete_flag = 0
        LEFT JOIN trx_daily_activity_details a2 WITH (NOLOCK) ON b.daily_activity_id = a2.id
            AND a2.shift_id = @shift_id AND a2.created_date = @activity_date AND a2.delete_flag = 0
        WHERE a.delete_flag = 0 AND a.active_flag = 1
        GROUP BY 
            a.id,
            a.code,
            a.name,
            d.name;

        -- Update remaining hours in a single operation
        UPDATE 
            #ManpowerDetails
        SET 
            remaining_work_hrs = CASE 
            WHEN @TotalWorkShiftHrs - total_spent_hrs < 0 THEN 0
            ELSE @TotalWorkShiftHrs - total_spent_hrs
        END;

        -- Final result set with query hint at the end
        SELECT 
            manpower_id,
            manpower_code,
            manpower_name,
            designation_name,
            remaining_work_hrs,
            CASE WHEN @hrs_spent <= remaining_work_hrs THEN 1 ELSE 0 END AS is_valid
        FROM #ManpowerDetails
        ORDER BY 
            CASE WHEN @hrs_spent <= remaining_work_hrs THEN 1 ELSE 0 END DESC,
            manpower_name
        OPTION (RECOMPILE);

        SET @response_message = 'Manpower List';
    END
    ELSE
    BEGIN
        SET @response_message = 'No Record Found!';
    END

    -- Clean up
    DROP TABLE IF EXISTS #EligibleManpower;
    DROP TABLE IF EXISTS #ManpowerDetails;
END