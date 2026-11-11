-- ==============================================================================
-- MDMSv2 Phase 1 Follow-up: Remove redundant *Unit columns from
-- MeasurementPointHalfHourOperatingReports (units now resolve from TagName).
-- Date: 2026-11-11
-- Idempotent: safe to re-run on already-upgraded or clean databases.
-- ==============================================================================

SET NAMES utf8mb4;

DROP PROCEDURE IF EXISTS `sp_phase1_drop_operating_unit_columns`;

DELIMITER $$

CREATE PROCEDURE `sp_phase1_drop_operating_unit_columns`()
BEGIN
    DECLARE v_tbl VARCHAR(128) DEFAULT NULL;
    DECLARE v_idx INT DEFAULT 1;
    DECLARE v_unit_col VARCHAR(128);
    DECLARE v_remaining INT DEFAULT 0;

    SELECT TABLE_NAME INTO v_tbl
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = DATABASE()
      AND LOWER(TABLE_NAME) = 'measurementpointhalfhouroperatingreports'
    LIMIT 1;

    IF v_tbl IS NOT NULL THEN
        WHILE v_idx <= 9 DO
            SET v_unit_col = ELT(v_idx,
                'DienApPhaAUnit',
                'DienApPhaBUnit',
                'DienApPhaCUnit',
                'DongDienPhaAUnit',
                'DongDienPhaBUnit',
                'DongDienPhaCUnit',
                'CongSuatTacDungTong3PhaUnit',
                'CongSuatPhanKhangTong3PhaUnit',
                'TanSoHeThongUnit'
            );

            IF EXISTS (
                SELECT 1
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = v_tbl
                  AND COLUMN_NAME = v_unit_col
            ) THEN
                SET @sql = CONCAT('ALTER TABLE `', v_tbl, '` DROP COLUMN `', v_unit_col, '`');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            END IF;

            SET v_idx = v_idx + 1;
        END WHILE;

        SELECT COUNT(*) INTO v_remaining
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = v_tbl
          AND COLUMN_NAME IN (
              'DienApPhaAUnit',
              'DienApPhaBUnit',
              'DienApPhaCUnit',
              'DongDienPhaAUnit',
              'DongDienPhaBUnit',
              'DongDienPhaCUnit',
              'CongSuatTacDungTong3PhaUnit',
              'CongSuatPhanKhangTong3PhaUnit',
              'TanSoHeThongUnit'
          );

        IF v_remaining > 0 THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Verification Gate Failed: Some *Unit columns still remain on MeasurementPointHalfHourOperatingReports!';
        END IF;
    END IF;
END$$

DELIMITER ;

CALL `sp_phase1_drop_operating_unit_columns`();

DROP PROCEDURE IF EXISTS `sp_phase1_drop_operating_unit_columns`;

