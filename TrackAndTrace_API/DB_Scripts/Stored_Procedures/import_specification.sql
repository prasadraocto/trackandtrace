CREATE PROCEDURE import_specification
    @CompanyID INT,
    @CreatedBy INT,
    @ImportName NVARCHAR(100),
    @SpecificationList type_specification READONLY
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
        SELECT @TotalCount = COUNT(*) FROM @SpecificationList;

        -- Step 2: Check if the ImportName already exists
        SELECT @ImportID = id
        FROM bulk_import_details
        WHERE name = @ImportName AND company_id = @CompanyID;

        IF @ImportID IS NULL
        BEGIN
            RAISERROR('ImportName does not exist for the specified CompanyID.', 16, 1);
        END

        -- Step 3: Ensure specifications exist and retrieve IDs
        DECLARE @SpecificationMapping TABLE (
            code VARCHAR(50),
            specification_id INT
        );

        -- Insert pre-existing specifications into mapping
        INSERT INTO @SpecificationMapping (code, specification_id)
        SELECT DISTINCT
            sl.code,
            s.id AS specification_id
        FROM @SpecificationList sl
        LEFT JOIN specification s
            ON s.company_id = @CompanyID AND s.code = sl.code
        WHERE s.delete_flag = 0;

        -- Insert new specifications and capture their IDs
        INSERT INTO specification (
            code,
            name,
            company_id,
            active_flag,
            delete_flag,
            created_by,
            created_date
        )
        OUTPUT INSERTED.id, INSERTED.code INTO @SpecificationMapping (specification_id, code)
        SELECT DISTINCT
            sl.code,
            sl.name,
            @CompanyID,
            1,
            0,
            @CreatedBy,
            GETDATE()
        FROM @SpecificationList sl
        LEFT JOIN specification s
            ON s.company_id = @CompanyID AND s.code = sl.code
        WHERE s.id IS NULL;

        -- Step 4: Map differentiators and insert into specification_differentiator_mapping
        INSERT INTO specification_differentiator_mapping (
            specification_id,
            differentiator_id,
            value
        )
        SELECT 
            sm.specification_id,
            d.id AS differentiator_id,
            sl.value
        FROM @SpecificationList sl
        INNER JOIN @SpecificationMapping sm ON sl.code = sm.code
        INNER JOIN differentiator d ON d.company_id = @CompanyID AND d.code = sl.differentiator
        WHERE NOT EXISTS (
            SELECT 1
            FROM specification_differentiator_mapping m
            WHERE m.specification_id = sm.specification_id AND m.differentiator_id = d.id
        );

        -- Step 5: Update SuccessCount
        SET @SuccessCount = @TotalCount;

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