CREATE PROCEDURE import_injury
    @CompanyID INT,
    @CreatedBy INT,
    @ImportName NVARCHAR(100),
    @CodeNameList type_code_name READONLY
AS
BEGIN
    DECLARE @ImportID INT;
    DECLARE @TotalCount INT;
    DECLARE @SuccessCount INT = 0;
    DECLARE @FailedCount INT = 0;
    DECLARE @FailedRecords NVARCHAR(MAX) = NULL;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Step 1: Calculate Total Records
        SELECT @TotalCount = COUNT(*) FROM @CodeNameList;

        -- Step 2: Check if the ImportName already exists
        SELECT @ImportID = id
        FROM bulk_import_details
        WHERE name = @ImportName AND company_id = @CompanyID;

        IF @ImportID IS NULL
        BEGIN
            RAISERROR('ImportName does not exist for the specified CompanyID.', 16, 1);
        END

        -- Step 3: Check for duplicates in the injury table
        DECLARE @DuplicateRecords TABLE (
            code VARCHAR(50),
            name VARCHAR(100)
        );

        INSERT INTO @DuplicateRecords (code, name)
        SELECT c.code, c.name
        FROM @CodeNameList c
        INNER JOIN injury i
        ON i.company_id = @CompanyID AND (i.code = c.code OR i.name = c.name) AND i.delete_flag = 0;

        -- Step 4: Handle Failed Records
        IF (SELECT COUNT(*) FROM @DuplicateRecords) > 0
        BEGIN
            -- Create JSON for failed records using STRING_AGG (SQL Server 2017+)
            SELECT @FailedRecords = 
            '[' + STRING_AGG(
                '{"code":"' + code + '","name":"' + name + '","remark":"Record already exists in the database"}', 
                ',') + ']'
            FROM @DuplicateRecords;

            -- Update FailedCount
            SELECT @FailedCount = COUNT(*) FROM @DuplicateRecords;
        END

        -- Step 5: Insert non-duplicate records into the injury table
        INSERT INTO injury (
            code,
            name,
            company_id,
            active_flag,
            delete_flag,
            created_by,
            created_date
        )
        SELECT c.code, c.name, @CompanyID, 1, 0, @CreatedBy, GETDATE()
        FROM @CodeNameList c
        LEFT JOIN @DuplicateRecords d
        ON c.code = d.code AND c.name = d.name
        WHERE d.code IS NULL;

        -- Update SuccessCount
        SET @SuccessCount = @TotalCount - @FailedCount;

        -- Step 6: Update the bulk_import_details record
        UPDATE bulk_import_details
        SET 
            total = @TotalCount,
            success = @SuccessCount,
            failed = @FailedCount,
            failed_records = @FailedRecords,
            status = 'completed',
            updated_by = @CreatedBy,
            updated_date = GETDATE()
        WHERE id = @ImportID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;

        -- Update the bulk_import_details table in case of failure
        IF @ImportID IS NOT NULL
        BEGIN
            UPDATE bulk_import_details
            SET 
                status = 'failed',
                updated_by = @CreatedBy,
                updated_date = GETDATE()
            WHERE id = @ImportID;
        END

        -- Replace THROW with RAISERROR for older versions
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
