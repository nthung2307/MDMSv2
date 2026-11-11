-- ==============================================================================
-- MDMSv2 Phase 1 Migration: IoT Wide-Table Snapshot, Partitioned TimeSeries &
-- Numeric Energy/Event/Operating Optimizations (Zero-Data-Loss Migration)
-- Date: 2026-11-11
-- Purpose:
--   1. Create MeasurementPointCurrentSnapshots (1 row/point wide snapshot).
--   2. Create MeasurementPointTimeSeries (monthly RANGE(PartitionYm) partitioned,
--      12-month retention target + 3 pre-created future months + p_max).
--   3. Backfill MeasurementPointCurrentSnapshots from latest records across
--      measurementpointmonitoring, measurementpointmonitoringbystatus,
--      and measurementpointmonitoringbydatetime.
--   4. Backfill MeasurementPointTimeSeries month-by-month from merged EAV
--      rows in measurementpointmonitoring + archivedatameasurementpointmonitoring.
--   5. Backfill missing hourly rows from measurementpointmonitoringhourly into
--      MeasurementPointHalfHourOperatingReports (numeric values only, dropping redundant *Unit columns),
--      deduplicate, and create UNIQUE INDEX on (MeasurementPointId, CreatedTime).
--   6. Convert MeasurementPointHalfHourEnergyReports & MeasurementPointDailyEnergyReports
--      value columns from LONGTEXT -> DECIMAL(18,4) + SuspectedMask (bits 0..9)
--      + IsSuspected TINYINT(1), deduplicate, and enforce UNIQUE INDEX.
--   7. Add ParameterCode VARCHAR(64) and nullable TagNameId (ON DELETE SET NULL)
--      on MeasurementPointEvents & MeasurementPointAlerts.
--   8. Run strict Verification Gate assertions (SIGNAL SQLSTATE '45000' on mismatch)
--      BEFORE dropping the 5 obsolete legacy tables and old string/unit columns.
-- ==============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

DROP FUNCTION IF EXISTS `fn_phase1_is_suspected`;
DROP FUNCTION IF EXISTS `fn_phase1_parse_decimal`;
DROP FUNCTION IF EXISTS `fn_phase1_parse_double`;
DROP FUNCTION IF EXISTS `fn_phase1_parse_smallint`;
DROP PROCEDURE IF EXISTS `sp_phase1_add_monthly_partitions`;
DROP PROCEDURE IF EXISTS `sp_phase1_migrate_energy_table`;
DROP PROCEDURE IF EXISTS `sp_phase1_migrate_event_alert_table`;
DROP PROCEDURE IF EXISTS `sp_phase1_migrate`;

DELIMITER $$

-- ------------------------------------------------------------------------------
-- Deterministic Parsing & Flag Extraction Helpers (No SUPER privilege required)
-- ------------------------------------------------------------------------------
CREATE FUNCTION `fn_phase1_is_suspected`(p_raw TEXT)
RETURNS TINYINT(1)
DETERMINISTIC
NO SQL
BEGIN
    IF p_raw IS NULL THEN
        RETURN 0;
    END IF;
    IF LOCATE('*', p_raw) > 0 OR LOCATE('NOT TOPICAL', UPPER(p_raw)) > 0 THEN
        RETURN 1;
    END IF;
    RETURN 0;
END$$

CREATE FUNCTION `fn_phase1_parse_decimal`(p_raw TEXT)
RETURNS DECIMAL(18,4)
DETERMINISTIC
NO SQL
BEGIN
    DECLARE v_clean VARCHAR(255);
    DECLARE v_norm VARCHAR(255);
    DECLARE v_dot_pos INT DEFAULT 0;
    DECLARE v_comma_pos INT DEFAULT 0;
    DECLARE v_dot_count INT DEFAULT 0;
    DECLARE v_comma_count INT DEFAULT 0;
    DECLARE v_int_len INT DEFAULT 0;

    IF p_raw IS NULL THEN
        RETURN NULL;
    END IF;

    SET v_clean = p_raw;
    SET v_clean = REPLACE(v_clean, '*', '');
    SET v_clean = REPLACE(v_clean, '(Not Topical)', '');
    SET v_clean = REPLACE(v_clean, '(not topical)', '');
    SET v_clean = REPLACE(v_clean, '(NOT TOPICAL)', '');
    SET v_clean = REPLACE(v_clean, ' ', '');
    SET v_clean = TRIM(v_clean);

    IF v_clean = '' OR LOWER(v_clean) = 'null' OR LOWER(v_clean) = 'n/a' OR v_clean = '-' THEN
        RETURN NULL;
    END IF;

    SET v_dot_pos = LOCATE('.', v_clean);
    SET v_comma_pos = LOCATE(',', v_clean);
    SET v_dot_count = CHAR_LENGTH(v_clean) - CHAR_LENGTH(REPLACE(v_clean, '.', ''));
    SET v_comma_count = CHAR_LENGTH(v_clean) - CHAR_LENGTH(REPLACE(v_clean, ',', ''));

    IF v_dot_pos > 0 AND v_comma_pos > 0 THEN
        IF v_comma_pos > v_dot_pos THEN
            -- Vietnamese format: 1.234,56 -> 1234.56
            SET v_norm = REPLACE(REPLACE(v_clean, '.', ''), ',', '.');
        ELSE
            -- US/Invariant format: 1,234.56 -> 1234.56
            SET v_norm = REPLACE(v_clean, ',', '');
        END IF;
    ELSEIF v_comma_count > 0 THEN
        IF v_comma_count > 1 THEN
            -- Thousands commas only: 1,234,567 -> 1234567
            SET v_norm = REPLACE(v_clean, ',', '');
        ELSE
            -- Single comma treated as decimal separator: 123,45 -> 123.45
            SET v_norm = REPLACE(v_clean, ',', '.');
        END IF;
    ELSEIF v_dot_count > 1 THEN
        -- Multiple dots: Vietnamese thousands 1.234.567 -> 1234567
        SET v_norm = REPLACE(v_clean, '.', '');
    ELSE
        SET v_norm = v_clean;
    END IF;

    IF v_norm NOT REGEXP '^[+-]?[0-9]+(\\.[0-9]+)?$' THEN
        RETURN NULL;
    END IF;

    SET v_dot_pos = LOCATE('.', v_norm);
    IF v_dot_pos > 0 THEN
        SET v_int_len = CHAR_LENGTH(REPLACE(SUBSTRING(v_norm, 1, v_dot_pos - 1), '-', ''));
    ELSE
        SET v_int_len = CHAR_LENGTH(REPLACE(v_norm, '-', ''));
    END IF;

    -- Prevent DECIMAL(18,4) integer part overflow (max 14 integer digits)
    IF v_int_len > 14 THEN
        RETURN NULL;
    END IF;

    RETURN ROUND(CAST(v_norm AS DECIMAL(28,6)), 4);
END$$

CREATE FUNCTION `fn_phase1_parse_double`(p_raw TEXT)
RETURNS DOUBLE
DETERMINISTIC
NO SQL
BEGIN
    DECLARE v_dec DECIMAL(18,4);
    SET v_dec = `fn_phase1_parse_decimal`(p_raw);
    IF v_dec IS NULL THEN
        RETURN NULL;
    END IF;
    RETURN CAST(v_dec AS DOUBLE);
END$$

CREATE FUNCTION `fn_phase1_parse_smallint`(p_raw TEXT)
RETURNS SMALLINT
DETERMINISTIC
NO SQL
BEGIN
    DECLARE v_dec DECIMAL(18,4);
    DECLARE v_int BIGINT;
    SET v_dec = `fn_phase1_parse_decimal`(p_raw);
    IF v_dec IS NULL THEN
        RETURN NULL;
    END IF;
    SET v_int = ROUND(v_dec, 0);
    IF v_int < -32768 OR v_int > 32767 THEN
        RETURN NULL;
    END IF;
    RETURN CAST(v_int AS SIGNED);
END$$

-- ------------------------------------------------------------------------------
-- Partition Creation Helper for MeasurementPointTimeSeries (Monthly RANGE)
-- ------------------------------------------------------------------------------
CREATE PROCEDURE `sp_phase1_add_monthly_partitions`(IN p_start_ym INT, IN p_end_ym INT)
BEGIN
    DECLARE v_cur_date DATE;
    DECLARE v_end_date DATE;
    DECLARE v_next_date DATE;
    DECLARE v_part_ym INT;
    DECLARE v_less_ym INT;
    DECLARE v_part_name VARCHAR(32);
    DECLARE v_exists INT DEFAULT 0;
    DECLARE v_has_max INT DEFAULT 0;
    DECLARE v_max_bound INT DEFAULT 0;

    IF p_start_ym IS NOT NULL AND p_end_ym IS NOT NULL AND p_start_ym <= p_end_ym THEN
        SET v_cur_date = STR_TO_DATE(CONCAT(p_start_ym, '01'), '%Y%m%d');
        SET v_end_date = STR_TO_DATE(CONCAT(p_end_ym, '01'), '%Y%m%d');

        WHILE v_cur_date <= v_end_date DO
            SET v_part_ym = YEAR(v_cur_date) * 100 + MONTH(v_cur_date);
            SET v_next_date = DATE_ADD(v_cur_date, INTERVAL 1 MONTH);
            SET v_less_ym = YEAR(v_next_date) * 100 + MONTH(v_next_date);
            SET v_part_name = CONCAT('p_', v_part_ym);

            SELECT COUNT(*) INTO v_exists
            FROM INFORMATION_SCHEMA.PARTITIONS
            WHERE TABLE_SCHEMA = DATABASE()
              AND LOWER(TABLE_NAME) = 'measurementpointtimeseries'
              AND PARTITION_NAME = v_part_name;

            SELECT COUNT(*) INTO v_has_max
            FROM INFORMATION_SCHEMA.PARTITIONS
            WHERE TABLE_SCHEMA = DATABASE()
              AND LOWER(TABLE_NAME) = 'measurementpointtimeseries'
              AND (PARTITION_NAME = 'p_max' OR PARTITION_DESCRIPTION = 'MAXVALUE');

            SELECT COALESCE(MAX(CAST(PARTITION_DESCRIPTION AS UNSIGNED)), 0) INTO v_max_bound
            FROM INFORMATION_SCHEMA.PARTITIONS
            WHERE TABLE_SCHEMA = DATABASE()
              AND LOWER(TABLE_NAME) = 'measurementpointtimeseries'
              AND PARTITION_DESCRIPTION REGEXP '^[0-9]+$';

            IF v_exists = 0 AND v_less_ym > v_max_bound THEN
                IF v_has_max > 0 THEN
                    SET @ddl = CONCAT(
                        'ALTER TABLE `MeasurementPointTimeSeries` REORGANIZE PARTITION `p_max` INTO (',
                        'PARTITION `', v_part_name, '` VALUES LESS THAN (', v_less_ym, '), ',
                        'PARTITION `p_max` VALUES LESS THAN MAXVALUE)'
                    );
                ELSE
                    SET @ddl = CONCAT(
                        'ALTER TABLE `MeasurementPointTimeSeries` ADD PARTITION (',
                        'PARTITION `', v_part_name, '` VALUES LESS THAN (', v_less_ym, '))'
                    );
                END IF;
                PREPARE stmt FROM @ddl;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            END IF;

            SET v_cur_date = v_next_date;
        END WHILE;
    END IF;
END$$

-- ------------------------------------------------------------------------------
-- Energy Reports Upgrade Helper (HalfHour & Daily): LONGTEXT -> DECIMAL(18,4)
-- + SuspectedMask (bits 0..9) + IsSuspected TINYINT(1) + UNIQUE INDEX
-- ------------------------------------------------------------------------------
CREATE PROCEDURE `sp_phase1_migrate_energy_table`(IN p_target_table VARCHAR(128), IN p_ux_name VARCHAR(128))
BEGIN
    DECLARE v_tbl VARCHAR(128) DEFAULT NULL;
    DECLARE v_has_suspected_mask INT DEFAULT 0;
    DECLARE v_has_tmp_col INT DEFAULT 0;
    DECLARE v_col_type VARCHAR(64) DEFAULT '';
    DECLARE v_before_rows BIGINT DEFAULT 0;
    DECLARE v_before_distinct BIGINT DEFAULT 0;
    DECLARE v_after_rows BIGINT DEFAULT 0;
    DECLARE v_before_sum DECIMAL(32,4) DEFAULT 0.0000;
    DECLARE v_after_sum DECIMAL(32,4) DEFAULT 0.0000;
    DECLARE v_before_suspected_rows BIGINT DEFAULT 0;
    DECLARE v_after_suspected_rows BIGINT DEFAULT 0;
    DECLARE v_dup_count BIGINT DEFAULT 0;
    DECLARE v_ux_exists INT DEFAULT 0;
    DECLARE v_col VARCHAR(128);
    DECLARE v_unit_col VARCHAR(128);
    DECLARE v_idx INT DEFAULT 1;

    SELECT TABLE_NAME INTO v_tbl
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = DATABASE()
      AND LOWER(TABLE_NAME) = LOWER(p_target_table)
    LIMIT 1;

    IF v_tbl IS NOT NULL THEN
        SELECT COUNT(*) INTO v_has_suspected_mask
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = v_tbl
          AND COLUMN_NAME = 'SuspectedMask';

        SELECT DATA_TYPE INTO v_col_type
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = v_tbl
          AND COLUMN_NAME = 'ChiSoTichLuyDienNangTacDungAPTongKenhGiao'
        LIMIT 1;

        IF v_has_suspected_mask = 0 OR LOWER(COALESCE(v_col_type, '')) <> 'decimal' THEN
            IF v_has_suspected_mask = 0 THEN
                SET @sql = CONCAT('ALTER TABLE `', v_tbl, '` ADD COLUMN `SuspectedMask` INT NOT NULL DEFAULT 0, ADD COLUMN `IsSuspected` TINYINT(1) NOT NULL DEFAULT 0');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
            END IF;

            SELECT COUNT(*) INTO v_has_tmp_col
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = v_tbl
              AND COLUMN_NAME = 'Tmp_Dec_01';

            IF v_has_tmp_col = 0 THEN
                SET @sql = CONCAT(
                    'ALTER TABLE `', v_tbl, '` ',
                    'ADD COLUMN `Tmp_Dec_01` DECIMAL(18,4) DEFAULT NULL, ',
                    'ADD COLUMN `Tmp_Dec_02` DECIMAL(18,4) DEFAULT NULL, ',
                    'ADD COLUMN `Tmp_Dec_03` DECIMAL(18,4) DEFAULT NULL, ',
                    'ADD COLUMN `Tmp_Dec_04` DECIMAL(18,4) DEFAULT NULL, ',
                    'ADD COLUMN `Tmp_Dec_05` DECIMAL(18,4) DEFAULT NULL, ',
                    'ADD COLUMN `Tmp_Dec_06` DECIMAL(18,4) DEFAULT NULL, ',
                    'ADD COLUMN `Tmp_Dec_07` DECIMAL(18,4) DEFAULT NULL, ',
                    'ADD COLUMN `Tmp_Dec_08` DECIMAL(18,4) DEFAULT NULL, ',
                    'ADD COLUMN `Tmp_Dec_09` DECIMAL(18,4) DEFAULT NULL, ',
                    'ADD COLUMN `Tmp_Dec_10` DECIMAL(18,4) DEFAULT NULL'
                );
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
            END IF;

            -- Convert existing strings into typed decimals + 10-bit SuspectedMask
            -- Bit positions match Domain.Enums.EnergySuspectedFlags exactly (0..9)
            SET @sql = CONCAT(
                'UPDATE `', v_tbl, '` SET ',
                '`Tmp_Dec_01` = `fn_phase1_parse_decimal`(`ChiSoTichLuyDienNangTacDungAPTongKenhGiao`), ',
                '`Tmp_Dec_02` = `fn_phase1_parse_decimal`(`ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao`), ',
                '`Tmp_Dec_03` = `fn_phase1_parse_decimal`(`ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao`), ',
                '`Tmp_Dec_04` = `fn_phase1_parse_decimal`(`ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao`), ',
                '`Tmp_Dec_05` = `fn_phase1_parse_decimal`(`ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao`), ',
                '`Tmp_Dec_06` = `fn_phase1_parse_decimal`(`ChiSoTichLuyDienNangTacDungAPTongKenhNhan`), ',
                '`Tmp_Dec_07` = `fn_phase1_parse_decimal`(`ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan`), ',
                '`Tmp_Dec_08` = `fn_phase1_parse_decimal`(`ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan`), ',
                '`Tmp_Dec_09` = `fn_phase1_parse_decimal`(`ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan`), ',
                '`Tmp_Dec_10` = `fn_phase1_parse_decimal`(`ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan`), ',
                '`SuspectedMask` = (',
                '  (`fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPTongKenhGiao`) * 1) + ',
                '  (`fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao`) * 2) + ',
                '  (`fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao`) * 4) + ',
                '  (`fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao`) * 8) + ',
                '  (`fn_phase1_is_suspected`(`ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao`) * 16) + ',
                '  (`fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPTongKenhNhan`) * 32) + ',
                '  (`fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan`) * 64) + ',
                '  (`fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan`) * 128) + ',
                '  (`fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan`) * 256) + ',
                '  (`fn_phase1_is_suspected`(`ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan`) * 512)',
                '), ',
                '`IsSuspected` = IF(',
                '  (`fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPTongKenhGiao`) + ',
                '   `fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao`) + ',
                '   `fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao`) + ',
                '   `fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao`) + ',
                '   `fn_phase1_is_suspected`(`ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao`) + ',
                '   `fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPTongKenhNhan`) + ',
                '   `fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan`) + ',
                '   `fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan`) + ',
                '   `fn_phase1_is_suspected`(`ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan`) + ',
                '   `fn_phase1_is_suspected`(`ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan`)) > 0, 1, 0)'
            );
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

            -- Pre-drop baseline check metrics
            SET @v_cnt = 0; SET @v_sum = 0.0000; SET @v_susp = 0;
            SET @sql = CONCAT(
                'SELECT COUNT(*), ',
                'COALESCE(SUM(COALESCE(`Tmp_Dec_01`,0)+COALESCE(`Tmp_Dec_02`,0)+COALESCE(`Tmp_Dec_03`,0)+COALESCE(`Tmp_Dec_04`,0)+COALESCE(`Tmp_Dec_05`,0)+',
                'COALESCE(`Tmp_Dec_06`,0)+COALESCE(`Tmp_Dec_07`,0)+COALESCE(`Tmp_Dec_08`,0)+COALESCE(`Tmp_Dec_09`,0)+COALESCE(`Tmp_Dec_10`,0)), 0), ',
                'COALESCE(SUM(CASE WHEN `IsSuspected` = 1 THEN 1 ELSE 0 END), 0) ',
                'INTO @v_cnt, @v_sum, @v_susp FROM `', v_tbl, '`'
            );
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
            SET v_before_rows = @v_cnt;
            SET v_before_sum = @v_sum;
            SET v_before_suspected_rows = @v_susp;

            -- Swap columns: drop legacy string & redundant *Unit columns, rename Tmp_Dec_*
            SET @sql = CONCAT(
                'ALTER TABLE `', v_tbl, '` ',
                'DROP COLUMN `ChiSoTichLuyDienNangTacDungAPTongKenhGiao`, ',
                'DROP COLUMN `ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao`, ',
                'DROP COLUMN `ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao`, ',
                'DROP COLUMN `ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao`, ',
                'DROP COLUMN `ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao`, ',
                'DROP COLUMN `ChiSoTichLuyDienNangTacDungAPTongKenhNhan`, ',
                'DROP COLUMN `ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan`, ',
                'DROP COLUMN `ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan`, ',
                'DROP COLUMN `ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan`, ',
                'DROP COLUMN `ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan`'
            );
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

            -- Drop legacy *Unit columns if present
            SET v_idx = 1;
            WHILE v_idx <= 10 DO
                SET v_unit_col = ELT(v_idx,
                    'ChiSoTichLuyDienNangTacDungAPTongKenhGiaoUnit',
                    'ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiaoUnit',
                    'ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiaoUnit',
                    'ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiaoUnit',
                    'ChiSoTichLuyDienNangPhanKhangAQTongKenhGiaoUnit',
                    'ChiSoTichLuyDienNangTacDungAPTongKenhNhanUnit',
                    'ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhanUnit',
                    'ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhanUnit',
                    'ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhanUnit',
                    'ChiSoTichLuyDienNangPhanKhangAQTongKenhNhanUnit'
                );
                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = v_tbl AND COLUMN_NAME = v_unit_col
                ) THEN
                    SET @sql = CONCAT('ALTER TABLE `', v_tbl, '` DROP COLUMN `', v_unit_col, '`');
                    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
                END IF;
                SET v_idx = v_idx + 1;
            END WHILE;

            -- Rename Tmp_Dec_01..10 -> exact domain property column names
            SET @sql = CONCAT(
                'ALTER TABLE `', v_tbl, '` ',
                'CHANGE COLUMN `Tmp_Dec_01` `ChiSoTichLuyDienNangTacDungAPTongKenhGiao` DECIMAL(18,4) DEFAULT NULL, ',
                'CHANGE COLUMN `Tmp_Dec_02` `ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao` DECIMAL(18,4) DEFAULT NULL, ',
                'CHANGE COLUMN `Tmp_Dec_03` `ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao` DECIMAL(18,4) DEFAULT NULL, ',
                'CHANGE COLUMN `Tmp_Dec_04` `ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao` DECIMAL(18,4) DEFAULT NULL, ',
                'CHANGE COLUMN `Tmp_Dec_05` `ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao` DECIMAL(18,4) DEFAULT NULL, ',
                'CHANGE COLUMN `Tmp_Dec_06` `ChiSoTichLuyDienNangTacDungAPTongKenhNhan` DECIMAL(18,4) DEFAULT NULL, ',
                'CHANGE COLUMN `Tmp_Dec_07` `ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan` DECIMAL(18,4) DEFAULT NULL, ',
                'CHANGE COLUMN `Tmp_Dec_08` `ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan` DECIMAL(18,4) DEFAULT NULL, ',
                'CHANGE COLUMN `Tmp_Dec_09` `ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan` DECIMAL(18,4) DEFAULT NULL, ',
                'CHANGE COLUMN `Tmp_Dec_10` `ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan` DECIMAL(18,4) DEFAULT NULL'
            );
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

            -- Verification Gate: exact row/sum/suspected parity BEFORE deduplication
            SET @v_cnt = 0; SET @v_sum = 0.0000; SET @v_susp = 0;
            SET @sql = CONCAT(
                'SELECT COUNT(*), ',
                'COALESCE(SUM(COALESCE(`ChiSoTichLuyDienNangTacDungAPTongKenhGiao`,0)+COALESCE(`ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao`,0)+COALESCE(`ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao`,0)+COALESCE(`ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao`,0)+COALESCE(`ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao`,0)+',
                'COALESCE(`ChiSoTichLuyDienNangTacDungAPTongKenhNhan`,0)+COALESCE(`ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan`,0)+COALESCE(`ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan`,0)+COALESCE(`ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan`,0)+COALESCE(`ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan`,0)), 0), ',
                'COALESCE(SUM(CASE WHEN `IsSuspected` = 1 THEN 1 ELSE 0 END), 0) ',
                'INTO @v_cnt, @v_sum, @v_susp FROM `', v_tbl, '`'
            );
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
            SET v_after_rows = @v_cnt;
            SET v_after_sum = @v_sum;
            SET v_after_suspected_rows = @v_susp;

            IF v_after_rows <> v_before_rows OR v_after_sum <> v_before_sum OR v_after_suspected_rows <> v_before_suspected_rows THEN
                SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Verification Gate Failed: Energy report numeric/suspected parity mismatch during column swap!';
            END IF;
        END IF;

        -- Deduplicate rows sharing identical (MeasurementPointId, CreatedTime) keeping latest Id
        SET @v_distinct_cnt = 0;
        SET @sql = CONCAT(
            'SELECT COUNT(*) INTO @v_distinct_cnt FROM (SELECT `MeasurementPointId`, `CreatedTime` FROM `', v_tbl, '` WHERE `CreatedTime` IS NOT NULL GROUP BY `MeasurementPointId`, `CreatedTime`) d'
        );
        PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
        SET v_before_distinct = @v_distinct_cnt;

        SET @sql = CONCAT(
            'DELETE t1 FROM `', v_tbl, '` t1 ',
            'INNER JOIN `', v_tbl, '` t2 ',
            'ON t1.`MeasurementPointId` = t2.`MeasurementPointId` ',
            'AND t1.`CreatedTime` = t2.`CreatedTime` ',
            'AND t1.`CreatedTime` IS NOT NULL ',
            'AND t1.`Id` < t2.`Id`'
        );
        PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

        -- Ensure unique constraint on (MeasurementPointId, CreatedTime)
        SELECT COUNT(*) INTO v_ux_exists
        FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = v_tbl
          AND NON_UNIQUE = 0
          AND INDEX_NAME = p_ux_name;

        IF v_ux_exists = 0 THEN
            SET @sql = CONCAT('ALTER TABLE `', v_tbl, '` ADD UNIQUE INDEX `', p_ux_name, '` (`MeasurementPointId`, `CreatedTime`)');
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
        END IF;

        SET @v_after_distinct = 0;
        SET @sql = CONCAT('SELECT COUNT(*) INTO @v_after_distinct FROM `', v_tbl, '` WHERE `CreatedTime` IS NOT NULL');
        PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
        IF @v_after_distinct <> v_before_distinct THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Verification Gate Failed: Distinct energy slot count mismatch after deduplication!';
        END IF;
    END IF;
END$$

-- ------------------------------------------------------------------------------
-- Events & Alerts Upgrade Helper: ParameterCode VARCHAR(64) + Nullable TagNameId
-- ------------------------------------------------------------------------------
CREATE PROCEDURE `sp_phase1_migrate_event_alert_table`(IN p_target_table VARCHAR(128), IN p_idx_name VARCHAR(128))
BEGIN
    DECLARE v_tbl VARCHAR(128) DEFAULT NULL;
    DECLARE v_tag_tbl VARCHAR(128) DEFAULT NULL;
    DECLARE v_col_exists INT DEFAULT 0;
    DECLARE v_is_nullable VARCHAR(10) DEFAULT 'NO';
    DECLARE v_fk_name VARCHAR(128);
    DECLARE v_idx_exists INT DEFAULT 0;
    DECLARE v_done INT DEFAULT 0;

    DECLARE cur_fks CURSOR FOR
        SELECT DISTINCT CONSTRAINT_NAME
        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = v_tbl
          AND COLUMN_NAME = 'TagNameId'
          AND REFERENCED_TABLE_NAME IS NOT NULL;
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_done = 1;

    SELECT TABLE_NAME INTO v_tbl
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = DATABASE()
      AND LOWER(TABLE_NAME) = LOWER(p_target_table)
    LIMIT 1;

    SELECT TABLE_NAME INTO v_tag_tbl
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = DATABASE()
      AND LOWER(TABLE_NAME) = 'tagname'
    LIMIT 1;

    IF v_tbl IS NOT NULL THEN
        SELECT COUNT(*) INTO v_col_exists
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = v_tbl
          AND COLUMN_NAME = 'ParameterCode';

        IF v_col_exists = 0 THEN
            SET @sql = CONCAT('ALTER TABLE `', v_tbl, '` ADD COLUMN `ParameterCode` VARCHAR(64) DEFAULT NULL');
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
        END IF;

        IF v_tag_tbl IS NOT NULL THEN
            SET @sql = CONCAT(
                'UPDATE `', v_tbl, '` t ',
                'INNER JOIN `', v_tag_tbl, '` tg ON t.`TagNameId` = tg.`Id` ',
                'SET t.`ParameterCode` = LEFT(tg.`Description`, 64) ',
                'WHERE t.`ParameterCode` IS NULL AND tg.`Description` IS NOT NULL'
            );
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
        END IF;

        SELECT IS_NULLABLE INTO v_is_nullable
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = v_tbl
          AND COLUMN_NAME = 'TagNameId'
        LIMIT 1;

        IF v_is_nullable = 'NO' THEN
            SET v_done = 0;
            OPEN cur_fks;
            fk_loop: LOOP
                FETCH cur_fks INTO v_fk_name;
                IF v_done = 1 THEN LEAVE fk_loop; END IF;
                SET @sql = CONCAT('ALTER TABLE `', v_tbl, '` DROP FOREIGN KEY `', v_fk_name, '`');
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
            END LOOP;
            CLOSE cur_fks;

            SET @sql = CONCAT('ALTER TABLE `', v_tbl, '` MODIFY COLUMN `TagNameId` BIGINT DEFAULT NULL');
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

            IF v_tag_tbl IS NOT NULL THEN
                SET @sql = CONCAT(
                    'ALTER TABLE `', v_tbl, '` ADD CONSTRAINT `FK_', v_tbl, '_TagName_TagNameId` ',
                    'FOREIGN KEY (`TagNameId`) REFERENCES `', v_tag_tbl, '` (`Id`) ON DELETE SET NULL'
                );
                PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
            END IF;
        END IF;

        SELECT COUNT(*) INTO v_idx_exists
        FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = v_tbl
          AND INDEX_NAME = p_idx_name;

        IF v_idx_exists = 0 THEN
            SET @sql = CONCAT('ALTER TABLE `', v_tbl, '` ADD INDEX `', p_idx_name, '` (`MeasurementPointId`, `CreatedTime`)');
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
        END IF;
    END IF;
END$$

-- ------------------------------------------------------------------------------
-- Main Phase 1 Migration Procedure (Idempotent + Verified)
-- ------------------------------------------------------------------------------
CREATE PROCEDURE `sp_phase1_migrate`()
BEGIN
    DECLARE v_tbl_monitoring VARCHAR(128) DEFAULT NULL;
    DECLARE v_tbl_archive VARCHAR(128) DEFAULT NULL;
    DECLARE v_tbl_datetime VARCHAR(128) DEFAULT NULL;
    DECLARE v_tbl_status VARCHAR(128) DEFAULT NULL;
    DECLARE v_tbl_hourly VARCHAR(128) DEFAULT NULL;
    DECLARE v_tbl_operating VARCHAR(128) DEFAULT NULL;
    DECLARE v_tbl_tagname VARCHAR(128) DEFAULT NULL;
    DECLARE v_tbl_points VARCHAR(128) DEFAULT NULL;

    DECLARE v_vn_now DATETIME;
    DECLARE v_cur_ym INT;
    DECLARE v_future3_ym INT;
    DECLARE v_cutoff12_ym INT;
    DECLARE v_data_min_ym INT DEFAULT NULL;
    DECLARE v_data_max_ym INT DEFAULT NULL;
    DECLARE v_min_dt DATETIME DEFAULT NULL;
    DECLARE v_max_dt DATETIME DEFAULT NULL;
    DECLARE v_cur_m DATE;
    DECLARE v_end_m DATE;
    DECLARE v_next_m DATE;
    DECLARE v_m_ym INT;

    DECLARE v_snap_cnt BIGINT DEFAULT 0;
    DECLARE v_expected_snap_pts BIGINT DEFAULT 0;
    DECLARE v_actual_snap_pts BIGINT DEFAULT 0;
    DECLARE v_expected_ts_cnt BIGINT DEFAULT 0;
    DECLARE v_actual_ts_cnt BIGINT DEFAULT 0;
    DECLARE v_op_before BIGINT DEFAULT 0;
    DECLARE v_op_after BIGINT DEFAULT 0;
    DECLARE v_hourly_missing BIGINT DEFAULT 0;
    DECLARE v_op_ux_exists INT DEFAULT 0;
    DECLARE v_op_distinct BIGINT DEFAULT 0;

    -- Discover exact physical table names (safe under both lower_case_table_names=0 and 1)
    SELECT TABLE_NAME INTO v_tbl_monitoring FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND LOWER(TABLE_NAME) = 'measurementpointmonitoring' LIMIT 1;
    SELECT TABLE_NAME INTO v_tbl_archive FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND LOWER(TABLE_NAME) = 'archivedatameasurementpointmonitoring' LIMIT 1;
    SELECT TABLE_NAME INTO v_tbl_datetime FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND LOWER(TABLE_NAME) = 'measurementpointmonitoringbydatetime' LIMIT 1;
    SELECT TABLE_NAME INTO v_tbl_status FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND LOWER(TABLE_NAME) = 'measurementpointmonitoringbystatus' LIMIT 1;
    SELECT TABLE_NAME INTO v_tbl_hourly FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND LOWER(TABLE_NAME) = 'measurementpointmonitoringhourly' LIMIT 1;
    SELECT TABLE_NAME INTO v_tbl_operating FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND LOWER(TABLE_NAME) = 'measurementpointhalfhouroperatingreports' LIMIT 1;
    SELECT TABLE_NAME INTO v_tbl_tagname FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND LOWER(TABLE_NAME) = 'tagname' LIMIT 1;
    SELECT TABLE_NAME INTO v_tbl_points FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND LOWER(TABLE_NAME) = 'measurementpoints' LIMIT 1;

    -- --------------------------------------------------------------------------
    -- STEP 1: Create MeasurementPointCurrentSnapshots & MeasurementPointTimeSeries
    -- --------------------------------------------------------------------------
    CREATE TABLE IF NOT EXISTS `MeasurementPointCurrentSnapshots` (
        `Id` bigint NOT NULL,
        `MeasurementPointId` bigint NOT NULL,
        `LastTelemetryAt` datetime(6) DEFAULT NULL,
        `LastStatusAt` datetime(6) DEFAULT NULL,
        `LastDateTimeAt` datetime(6) DEFAULT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        `DongDienPhaA` decimal(18,4) DEFAULT NULL, `DongDienPhaAUnit` varchar(32) DEFAULT NULL, `DongDienPhaAQuality` tinyint(1) DEFAULT NULL, `DongDienPhaAUpdatedAt` datetime(6) DEFAULT NULL,
        `DongDienPhaB` decimal(18,4) DEFAULT NULL, `DongDienPhaBUnit` varchar(32) DEFAULT NULL, `DongDienPhaBQuality` tinyint(1) DEFAULT NULL, `DongDienPhaBUpdatedAt` datetime(6) DEFAULT NULL,
        `DongDienPhaC` decimal(18,4) DEFAULT NULL, `DongDienPhaCUnit` varchar(32) DEFAULT NULL, `DongDienPhaCQuality` tinyint(1) DEFAULT NULL, `DongDienPhaCUpdatedAt` datetime(6) DEFAULT NULL,
        `DienApPhaA` decimal(18,4) DEFAULT NULL, `DienApPhaAUnit` varchar(32) DEFAULT NULL, `DienApPhaAQuality` tinyint(1) DEFAULT NULL, `DienApPhaAUpdatedAt` datetime(6) DEFAULT NULL,
        `DienApPhaB` decimal(18,4) DEFAULT NULL, `DienApPhaBUnit` varchar(32) DEFAULT NULL, `DienApPhaBQuality` tinyint(1) DEFAULT NULL, `DienApPhaBUpdatedAt` datetime(6) DEFAULT NULL,
        `DienApPhaC` decimal(18,4) DEFAULT NULL, `DienApPhaCUnit` varchar(32) DEFAULT NULL, `DienApPhaCQuality` tinyint(1) DEFAULT NULL, `DienApPhaCUpdatedAt` datetime(6) DEFAULT NULL,
        `CongSuatTacDungPhaA` decimal(18,4) DEFAULT NULL, `CongSuatTacDungPhaAUnit` varchar(32) DEFAULT NULL, `CongSuatTacDungPhaAQuality` tinyint(1) DEFAULT NULL, `CongSuatTacDungPhaAUpdatedAt` datetime(6) DEFAULT NULL,
        `CongSuatTacDungPhaB` decimal(18,4) DEFAULT NULL, `CongSuatTacDungPhaBUnit` varchar(32) DEFAULT NULL, `CongSuatTacDungPhaBQuality` tinyint(1) DEFAULT NULL, `CongSuatTacDungPhaBUpdatedAt` datetime(6) DEFAULT NULL,
        `CongSuatTacDungPhaC` decimal(18,4) DEFAULT NULL, `CongSuatTacDungPhaCUnit` varchar(32) DEFAULT NULL, `CongSuatTacDungPhaCQuality` tinyint(1) DEFAULT NULL, `CongSuatTacDungPhaCUpdatedAt` datetime(6) DEFAULT NULL,
        `CongSuatTacDungTong3Pha` decimal(18,4) DEFAULT NULL, `CongSuatTacDungTong3PhaUnit` varchar(32) DEFAULT NULL, `CongSuatTacDungTong3PhaQuality` tinyint(1) DEFAULT NULL, `CongSuatTacDungTong3PhaUpdatedAt` datetime(6) DEFAULT NULL,
        `CongSuatPhanKhangPhaA` decimal(18,4) DEFAULT NULL, `CongSuatPhanKhangPhaAUnit` varchar(32) DEFAULT NULL, `CongSuatPhanKhangPhaAQuality` tinyint(1) DEFAULT NULL, `CongSuatPhanKhangPhaAUpdatedAt` datetime(6) DEFAULT NULL,
        `CongSuatPhanKhangPhaB` decimal(18,4) DEFAULT NULL, `CongSuatPhanKhangPhaBUnit` varchar(32) DEFAULT NULL, `CongSuatPhanKhangPhaBQuality` tinyint(1) DEFAULT NULL, `CongSuatPhanKhangPhaBUpdatedAt` datetime(6) DEFAULT NULL,
        `CongSuatPhanKhangPhaC` decimal(18,4) DEFAULT NULL, `CongSuatPhanKhangPhaCUnit` varchar(32) DEFAULT NULL, `CongSuatPhanKhangPhaCQuality` tinyint(1) DEFAULT NULL, `CongSuatPhanKhangPhaCUpdatedAt` datetime(6) DEFAULT NULL,
        `CongSuatPhanKhangTong3Pha` decimal(18,4) DEFAULT NULL, `CongSuatPhanKhangTong3PhaUnit` varchar(32) DEFAULT NULL, `CongSuatPhanKhangTong3PhaQuality` tinyint(1) DEFAULT NULL, `CongSuatPhanKhangTong3PhaUpdatedAt` datetime(6) DEFAULT NULL,
        `HeSoCongSuatPFPhaA` decimal(18,4) DEFAULT NULL, `HeSoCongSuatPFPhaAQuality` tinyint(1) DEFAULT NULL, `HeSoCongSuatPFPhaAUpdatedAt` datetime(6) DEFAULT NULL,
        `HeSoCongSuatPFPhaB` decimal(18,4) DEFAULT NULL, `HeSoCongSuatPFPhaBQuality` tinyint(1) DEFAULT NULL, `HeSoCongSuatPFPhaBUpdatedAt` datetime(6) DEFAULT NULL,
        `HeSoCongSuatPFPhaC` decimal(18,4) DEFAULT NULL, `HeSoCongSuatPFPhaCQuality` tinyint(1) DEFAULT NULL, `HeSoCongSuatPFPhaCUpdatedAt` datetime(6) DEFAULT NULL,
        `HeSoCongSuatPFTrungBinh3Pha` decimal(18,4) DEFAULT NULL, `HeSoCongSuatPFTrungBinh3PhaQuality` tinyint(1) DEFAULT NULL, `HeSoCongSuatPFTrungBinh3PhaUpdatedAt` datetime(6) DEFAULT NULL,
        `TanSoHeThong` decimal(18,4) DEFAULT NULL, `TanSoHeThongUnit` varchar(32) DEFAULT NULL, `TanSoHeThongQuality` tinyint(1) DEFAULT NULL, `TanSoHeThongUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPBieuGioBT` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioBTUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioBTQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioBTUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPBieuGioCD` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioCDUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioCDQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioCDUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPBieuGioTD` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioTDUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioTDQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioTDUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPTong` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPTongUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPTongQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPTongUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoTichLuyDienNangPhanKhangAQTong` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangPhanKhangAQTongUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangPhanKhangAQTongQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangPhanKhangAQTongUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPTongChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPTongChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPTongChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPTongChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoChotDienNangAPGioBT` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPGioBTUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPGioBTQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPGioBTUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoChotDienNangAPGioCD` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPGioCDUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPGioCDQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPGioCDUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoChotDienNangAPGioTD` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPGioTDUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPGioTDQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPGioTDUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoChotDienNangAPTong` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPTongUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPTongQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPTongUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoChotDienNangAQTong` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAQTongUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAQTongQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAQTongUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoChotDienNangAPGioBTChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPGioBTChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPGioBTChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPGioBTChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoChotDienNangAPGioCDChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPGioCDChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPGioCDChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPGioCDChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoChotDienNangAPGioTDChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPGioTDChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPGioTDChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPGioTDChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoChotDienNangAPTongChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPTongChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPTongChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPTongChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
        `ChiSoChotDienNangAQTongChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAQTongChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAQTongChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAQTongChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
        `PmaxKyHienTai` decimal(18,4) DEFAULT NULL, `PmaxKyHienTaiUnit` varchar(32) DEFAULT NULL, `PmaxKyHienTaiQuality` tinyint(1) DEFAULT NULL, `PmaxKyHienTaiUpdatedAt` datetime(6) DEFAULT NULL,
        `PmaxKyTruoc` decimal(18,4) DEFAULT NULL, `PmaxKyTruocUnit` varchar(32) DEFAULT NULL, `PmaxKyTruocQuality` tinyint(1) DEFAULT NULL, `PmaxKyTruocUpdatedAt` datetime(6) DEFAULT NULL,
        `MatKetNoi` tinyint(1) DEFAULT NULL, `MatKetNoiQuality` tinyint(1) DEFAULT NULL, `MatKetNoiUpdatedAt` datetime(6) DEFAULT NULL,
        `CanhBao01` tinyint(1) DEFAULT NULL, `CanhBao01Quality` tinyint(1) DEFAULT NULL, `CanhBao01UpdatedAt` datetime(6) DEFAULT NULL,
        `CanhBao02` tinyint(1) DEFAULT NULL, `CanhBao02Quality` tinyint(1) DEFAULT NULL, `CanhBao02UpdatedAt` datetime(6) DEFAULT NULL,
        `CanhBao03` tinyint(1) DEFAULT NULL, `CanhBao03Quality` tinyint(1) DEFAULT NULL, `CanhBao03UpdatedAt` datetime(6) DEFAULT NULL,
        `CanhBao04` tinyint(1) DEFAULT NULL, `CanhBao04Quality` tinyint(1) DEFAULT NULL, `CanhBao04UpdatedAt` datetime(6) DEFAULT NULL,
        `CanhBao05` tinyint(1) DEFAULT NULL, `CanhBao05Quality` tinyint(1) DEFAULT NULL, `CanhBao05UpdatedAt` datetime(6) DEFAULT NULL,
        `CanhBao06` tinyint(1) DEFAULT NULL, `CanhBao06Quality` tinyint(1) DEFAULT NULL, `CanhBao06UpdatedAt` datetime(6) DEFAULT NULL,
        `CanhBao07` tinyint(1) DEFAULT NULL, `CanhBao07Quality` tinyint(1) DEFAULT NULL, `CanhBao07UpdatedAt` datetime(6) DEFAULT NULL,
        `CanhBao08` tinyint(1) DEFAULT NULL, `CanhBao08Quality` tinyint(1) DEFAULT NULL, `CanhBao08UpdatedAt` datetime(6) DEFAULT NULL,
        `CanhBao09` tinyint(1) DEFAULT NULL, `CanhBao09Quality` tinyint(1) DEFAULT NULL, `CanhBao09UpdatedAt` datetime(6) DEFAULT NULL,
        `CanhBao10` tinyint(1) DEFAULT NULL, `CanhBao10Quality` tinyint(1) DEFAULT NULL, `CanhBao10UpdatedAt` datetime(6) DEFAULT NULL,
        `MeterYear` smallint DEFAULT NULL, `MeterYearQuality` tinyint(1) DEFAULT NULL,
        `MeterMonth` smallint DEFAULT NULL, `MeterMonthQuality` tinyint(1) DEFAULT NULL,
        `MeterDay` smallint DEFAULT NULL, `MeterDayQuality` tinyint(1) DEFAULT NULL,
        `MeterHour` smallint DEFAULT NULL, `MeterHourQuality` tinyint(1) DEFAULT NULL,
        `MeterMinute` smallint DEFAULT NULL, `MeterMinuteQuality` tinyint(1) DEFAULT NULL,
        `MeterSecond` smallint DEFAULT NULL, `MeterSecondQuality` tinyint(1) DEFAULT NULL,
        `ClosingYear` smallint DEFAULT NULL, `ClosingYearQuality` tinyint(1) DEFAULT NULL,
        `ClosingMonth` smallint DEFAULT NULL, `ClosingMonthQuality` tinyint(1) DEFAULT NULL,
        `ClosingDay` smallint DEFAULT NULL, `ClosingDayQuality` tinyint(1) DEFAULT NULL,
        `ClosingHour` smallint DEFAULT NULL, `ClosingHourQuality` tinyint(1) DEFAULT NULL,
        `ClosingMinute` smallint DEFAULT NULL, `ClosingMinuteQuality` tinyint(1) DEFAULT NULL,
        `ClosingSecond` smallint DEFAULT NULL, `ClosingSecondQuality` tinyint(1) DEFAULT NULL,
        `PmaxCurrentYear` smallint DEFAULT NULL, `PmaxCurrentYearQuality` tinyint(1) DEFAULT NULL,
        `PmaxCurrentMonth` smallint DEFAULT NULL, `PmaxCurrentMonthQuality` tinyint(1) DEFAULT NULL,
        `PmaxCurrentDay` smallint DEFAULT NULL, `PmaxCurrentDayQuality` tinyint(1) DEFAULT NULL,
        `PmaxCurrentHour` smallint DEFAULT NULL, `PmaxCurrentHourQuality` tinyint(1) DEFAULT NULL,
        `PmaxCurrentMinute` smallint DEFAULT NULL, `PmaxCurrentMinuteQuality` tinyint(1) DEFAULT NULL,
        `PmaxCurrentSecond` smallint DEFAULT NULL, `PmaxCurrentSecondQuality` tinyint(1) DEFAULT NULL,
        `PmaxPrevYear` smallint DEFAULT NULL, `PmaxPrevYearQuality` tinyint(1) DEFAULT NULL,
        `PmaxPrevMonth` smallint DEFAULT NULL, `PmaxPrevMonthQuality` tinyint(1) DEFAULT NULL,
        `PmaxPrevDay` smallint DEFAULT NULL, `PmaxPrevDayQuality` tinyint(1) DEFAULT NULL,
        `PmaxPrevHour` smallint DEFAULT NULL, `PmaxPrevHourQuality` tinyint(1) DEFAULT NULL,
        `PmaxPrevMinute` smallint DEFAULT NULL, `PmaxPrevMinuteQuality` tinyint(1) DEFAULT NULL,
        `PmaxPrevSecond` smallint DEFAULT NULL, `PmaxPrevSecondQuality` tinyint(1) DEFAULT NULL,
        PRIMARY KEY (`Id`),
        UNIQUE KEY `IX_MeasurementPointCurrentSnapshots_MeasurementPointId` (`MeasurementPointId`)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

    CREATE TABLE IF NOT EXISTS `MeasurementPointTimeSeries` (
        `Id` bigint NOT NULL,
        `MeasurementPointId` bigint NOT NULL,
        `PartitionYm` int NOT NULL,
        `RecordedAt` datetime(6) NOT NULL,
        `IngestedAt` datetime(6) NOT NULL,
        `QualityFlag` tinyint(1) NOT NULL DEFAULT '1',
        `SourcePacketType` tinyint unsigned NOT NULL DEFAULT '1',
        `DongDienPhaA` decimal(18,4) DEFAULT NULL,
        `DongDienPhaB` decimal(18,4) DEFAULT NULL,
        `DongDienPhaC` decimal(18,4) DEFAULT NULL,
        `DienApPhaA` decimal(18,4) DEFAULT NULL,
        `DienApPhaB` decimal(18,4) DEFAULT NULL,
        `DienApPhaC` decimal(18,4) DEFAULT NULL,
        `CongSuatTacDungPhaA` decimal(18,4) DEFAULT NULL,
        `CongSuatTacDungPhaB` decimal(18,4) DEFAULT NULL,
        `CongSuatTacDungPhaC` decimal(18,4) DEFAULT NULL,
        `CongSuatTacDungTong3Pha` decimal(18,4) DEFAULT NULL,
        `CongSuatPhanKhangPhaA` decimal(18,4) DEFAULT NULL,
        `CongSuatPhanKhangPhaB` decimal(18,4) DEFAULT NULL,
        `CongSuatPhanKhangPhaC` decimal(18,4) DEFAULT NULL,
        `CongSuatPhanKhangTong3Pha` decimal(18,4) DEFAULT NULL,
        `HeSoCongSuatPFPhaA` decimal(18,4) DEFAULT NULL,
        `HeSoCongSuatPFPhaB` decimal(18,4) DEFAULT NULL,
        `HeSoCongSuatPFPhaC` decimal(18,4) DEFAULT NULL,
        `HeSoCongSuatPFTrungBinh3Pha` decimal(18,4) DEFAULT NULL,
        `TanSoHeThong` decimal(18,4) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPBieuGioBT` decimal(18,4) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPBieuGioCD` decimal(18,4) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPBieuGioTD` decimal(18,4) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPTong` decimal(18,4) DEFAULT NULL,
        `ChiSoTichLuyDienNangPhanKhangAQTong` decimal(18,4) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan` decimal(18,4) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan` decimal(18,4) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan` decimal(18,4) DEFAULT NULL,
        `ChiSoTichLuyDienNangTacDungAPTongChieuNhan` decimal(18,4) DEFAULT NULL,
        `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan` decimal(18,4) DEFAULT NULL,
        `ChiSoChotDienNangAPGioBT` decimal(18,4) DEFAULT NULL,
        `ChiSoChotDienNangAPGioCD` decimal(18,4) DEFAULT NULL,
        `ChiSoChotDienNangAPGioTD` decimal(18,4) DEFAULT NULL,
        `ChiSoChotDienNangAPTong` decimal(18,4) DEFAULT NULL,
        `ChiSoChotDienNangAQTong` decimal(18,4) DEFAULT NULL,
        `ChiSoChotDienNangAPGioBTChieuNhan` decimal(18,4) DEFAULT NULL,
        `ChiSoChotDienNangAPGioCDChieuNhan` decimal(18,4) DEFAULT NULL,
        `ChiSoChotDienNangAPGioTDChieuNhan` decimal(18,4) DEFAULT NULL,
        `ChiSoChotDienNangAPTongChieuNhan` decimal(18,4) DEFAULT NULL,
        `ChiSoChotDienNangAQTongChieuNhan` decimal(18,4) DEFAULT NULL,
        `PmaxKyHienTai` decimal(18,4) DEFAULT NULL,
        `PmaxKyTruoc` decimal(18,4) DEFAULT NULL,
        PRIMARY KEY (`MeasurementPointId`, `PartitionYm`, `RecordedAt`, `Id`),
        KEY `IX_MeasurementPointTimeSeries_MeasurementPointId_RecordedAt` (`MeasurementPointId`, `RecordedAt`),
        KEY `IX_MeasurementPointTimeSeries_RecordedAt` (`RecordedAt`)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
    PARTITION BY RANGE (`PartitionYm`) (
        PARTITION `p_max` VALUES LESS THAN MAXVALUE
    );

    -- Partition calendar window in UTC+7
    SET v_vn_now = DATE_ADD(UTC_TIMESTAMP(), INTERVAL 7 HOUR);
    SET v_cur_ym = YEAR(v_vn_now) * 100 + MONTH(v_vn_now);
    SET v_future3_ym = YEAR(DATE_ADD(v_vn_now, INTERVAL 3 MONTH)) * 100 + MONTH(DATE_ADD(v_vn_now, INTERVAL 3 MONTH));
    SET v_cutoff12_ym = YEAR(DATE_SUB(v_vn_now, INTERVAL 11 MONTH)) * 100 + MONTH(DATE_SUB(v_vn_now, INTERVAL 11 MONTH));

    -- Inspect time bounds from legacy telemetry + archive tables if present
    IF v_tbl_monitoring IS NOT NULL THEN
        SET @dmin = NULL; SET @dmax = NULL;
        SET @sql = CONCAT('SELECT MIN(`CreatedTime`), MAX(`CreatedTime`) INTO @dmin, @dmax FROM `', v_tbl_monitoring, '` WHERE `CreatedTime` IS NOT NULL');
        PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
        IF @dmin IS NOT NULL AND (v_min_dt IS NULL OR @dmin < v_min_dt) THEN SET v_min_dt = @dmin; END IF;
        IF @dmax IS NOT NULL AND (v_max_dt IS NULL OR @dmax > v_max_dt) THEN SET v_max_dt = @dmax; END IF;
    END IF;

    IF v_tbl_archive IS NOT NULL THEN
        SET @dmin = NULL; SET @dmax = NULL;
        SET @sql = CONCAT('SELECT MIN(`CreatedTime`), MAX(`CreatedTime`) INTO @dmin, @dmax FROM `', v_tbl_archive, '` WHERE `CreatedTime` IS NOT NULL');
        PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
        IF @dmin IS NOT NULL AND (v_min_dt IS NULL OR @dmin < v_min_dt) THEN SET v_min_dt = @dmin; END IF;
        IF @dmax IS NOT NULL AND (v_max_dt IS NULL OR @dmax > v_max_dt) THEN SET v_max_dt = @dmax; END IF;
    END IF;

    IF v_min_dt IS NOT NULL THEN
        SET v_data_min_ym = YEAR(DATE_ADD(v_min_dt, INTERVAL 7 HOUR)) * 100 + MONTH(DATE_ADD(v_min_dt, INTERVAL 7 HOUR));
        IF v_data_min_ym < 202001 THEN SET v_data_min_ym = 202001; END IF;
    END IF;

    CALL `sp_phase1_add_monthly_partitions`(
        COALESCE(LEAST(COALESCE(v_data_min_ym, v_cutoff12_ym), v_cutoff12_ym), v_cutoff12_ym),
        v_future3_ym
    );

    -- --------------------------------------------------------------------------
    -- STEP 2: Populate MeasurementPointCurrentSnapshots (Wide 1 row/meter)
    -- --------------------------------------------------------------------------
    SELECT COUNT(*) INTO v_snap_cnt FROM `MeasurementPointCurrentSnapshots`;

    IF v_snap_cnt = 0 AND (v_tbl_monitoring IS NOT NULL OR v_tbl_status IS NOT NULL OR v_tbl_datetime IS NOT NULL) THEN
        DROP TEMPORARY TABLE IF EXISTS `tmp_phase1_snap_raw`;
        CREATE TEMPORARY TABLE `tmp_phase1_snap_raw` (
            `MeasurementPointId` bigint NOT NULL,
            `TagDesc` varchar(128) NOT NULL,
            `Val` text,
            `Unit` varchar(32) DEFAULT NULL,
            `Quality` tinyint(1) DEFAULT NULL,
            `CreatedTime` datetime(6) NOT NULL,
            `PktType` tinyint NOT NULL,
            `OrigId` bigint NOT NULL,
            KEY `IX_tmp_snap` (`MeasurementPointId`, `TagDesc`, `CreatedTime`, `OrigId`)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

        IF v_tbl_monitoring IS NOT NULL THEN
            SET @sql = CONCAT(
                'INSERT INTO `tmp_phase1_snap_raw` (`MeasurementPointId`, `TagDesc`, `Val`, `Unit`, `Quality`, `CreatedTime`, `PktType`, `OrigId`) ',
                'SELECT m.`MeasurementPointsId`, ',
                'UPPER(TRIM(COALESCE(NULLIF(m.`TagNameDescription`, ''''), tg.`Description`, ''''))), ',
                'm.`Value`, LEFT(tg.`Unit`, 32), m.`Quanlity`, COALESCE(m.`CreatedTime`, UTC_TIMESTAMP(6)), 1, m.`Id` ',
                'FROM `', v_tbl_monitoring, '` m ',
                IF(v_tbl_points IS NOT NULL, CONCAT('INNER JOIN `', v_tbl_points, '` mp ON m.`MeasurementPointsId` = mp.`Id` '), ''),
                IF(v_tbl_tagname IS NOT NULL, CONCAT('LEFT JOIN `', v_tbl_tagname, '` tg ON m.`TagNameId` = tg.`Id` '), ''),
                'WHERE m.`Value` IS NOT NULL AND UPPER(TRIM(COALESCE(NULLIF(m.`TagNameDescription`, ''''), tg.`Description`, ''''))) <> '''''
            );
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
        END IF;

        IF v_tbl_status IS NOT NULL THEN
            SET @sql = CONCAT(
                'INSERT INTO `tmp_phase1_snap_raw` (`MeasurementPointId`, `TagDesc`, `Val`, `Unit`, `Quality`, `CreatedTime`, `PktType`, `OrigId`) ',
                'SELECT s.`MeasurementPointId`, ',
                'UPPER(TRIM(COALESCE(NULLIF(s.`TagNameDescription`, ''''), tg.`Description`, ''''))), ',
                's.`Value`, NULL, s.`Quanlity`, COALESCE(s.`CreatedTime`, UTC_TIMESTAMP(6)), 2, s.`Id` ',
                'FROM `', v_tbl_status, '` s ',
                IF(v_tbl_points IS NOT NULL, CONCAT('INNER JOIN `', v_tbl_points, '` mp ON s.`MeasurementPointId` = mp.`Id` '), ''),
                IF(v_tbl_tagname IS NOT NULL, CONCAT('LEFT JOIN `', v_tbl_tagname, '` tg ON s.`TagNameId` = tg.`Id` '), ''),
                'WHERE s.`Value` IS NOT NULL AND UPPER(TRIM(COALESCE(NULLIF(s.`TagNameDescription`, ''''), tg.`Description`, ''''))) <> '''''
            );
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
        END IF;

        IF v_tbl_datetime IS NOT NULL THEN
            SET @sql = CONCAT(
                'INSERT INTO `tmp_phase1_snap_raw` (`MeasurementPointId`, `TagDesc`, `Val`, `Unit`, `Quality`, `CreatedTime`, `PktType`, `OrigId`) ',
                'SELECT d.`MeasurementPointId`, ',
                'UPPER(TRIM(COALESCE(NULLIF(d.`TagNameDescription`, ''''), tg.`Description`, ''''))), ',
                'd.`Value`, NULL, d.`Quanlity`, COALESCE(d.`CreatedTime`, UTC_TIMESTAMP(6)), 3, d.`Id` ',
                'FROM `', v_tbl_datetime, '` d ',
                IF(v_tbl_points IS NOT NULL, CONCAT('INNER JOIN `', v_tbl_points, '` mp ON d.`MeasurementPointId` = mp.`Id` '), ''),
                IF(v_tbl_tagname IS NOT NULL, CONCAT('LEFT JOIN `', v_tbl_tagname, '` tg ON d.`TagNameId` = tg.`Id` '), ''),
                'WHERE d.`Value` IS NOT NULL AND UPPER(TRIM(COALESCE(NULLIF(d.`TagNameDescription`, ''''), tg.`Description`, ''''))) <> '''''
            );
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
        END IF;

        SELECT COUNT(DISTINCT `MeasurementPointId`) INTO v_expected_snap_pts FROM `tmp_phase1_snap_raw`;

        INSERT INTO `MeasurementPointCurrentSnapshots` (
            `Id`, `MeasurementPointId`, `LastTelemetryAt`, `LastStatusAt`, `LastDateTimeAt`, `UpdatedAt`,
            `DongDienPhaA`, `DongDienPhaAUnit`, `DongDienPhaAQuality`, `DongDienPhaAUpdatedAt`,
            `DongDienPhaB`, `DongDienPhaBUnit`, `DongDienPhaBQuality`, `DongDienPhaBUpdatedAt`,
            `DongDienPhaC`, `DongDienPhaCUnit`, `DongDienPhaCQuality`, `DongDienPhaCUpdatedAt`,
            `DienApPhaA`, `DienApPhaAUnit`, `DienApPhaAQuality`, `DienApPhaAUpdatedAt`,
            `DienApPhaB`, `DienApPhaBUnit`, `DienApPhaBQuality`, `DienApPhaBUpdatedAt`,
            `DienApPhaC`, `DienApPhaCUnit`, `DienApPhaCQuality`, `DienApPhaCUpdatedAt`,
            `CongSuatTacDungPhaA`, `CongSuatTacDungPhaAUnit`, `CongSuatTacDungPhaAQuality`, `CongSuatTacDungPhaAUpdatedAt`,
            `CongSuatTacDungPhaB`, `CongSuatTacDungPhaBUnit`, `CongSuatTacDungPhaBQuality`, `CongSuatTacDungPhaBUpdatedAt`,
            `CongSuatTacDungPhaC`, `CongSuatTacDungPhaCUnit`, `CongSuatTacDungPhaCQuality`, `CongSuatTacDungPhaCUpdatedAt`,
            `CongSuatTacDungTong3Pha`, `CongSuatTacDungTong3PhaUnit`, `CongSuatTacDungTong3PhaQuality`, `CongSuatTacDungTong3PhaUpdatedAt`,
            `CongSuatPhanKhangPhaA`, `CongSuatPhanKhangPhaAUnit`, `CongSuatPhanKhangPhaAQuality`, `CongSuatPhanKhangPhaAUpdatedAt`,
            `CongSuatPhanKhangPhaB`, `CongSuatPhanKhangPhaBUnit`, `CongSuatPhanKhangPhaBQuality`, `CongSuatPhanKhangPhaBUpdatedAt`,
            `CongSuatPhanKhangPhaC`, `CongSuatPhanKhangPhaCUnit`, `CongSuatPhanKhangPhaCQuality`, `CongSuatPhanKhangPhaCUpdatedAt`,
            `CongSuatPhanKhangTong3Pha`, `CongSuatPhanKhangTong3PhaUnit`, `CongSuatPhanKhangTong3PhaQuality`, `CongSuatPhanKhangTong3PhaUpdatedAt`,
            `HeSoCongSuatPFPhaA`, `HeSoCongSuatPFPhaAQuality`, `HeSoCongSuatPFPhaAUpdatedAt`,
            `HeSoCongSuatPFPhaB`, `HeSoCongSuatPFPhaBQuality`, `HeSoCongSuatPFPhaBUpdatedAt`,
            `HeSoCongSuatPFPhaC`, `HeSoCongSuatPFPhaCQuality`, `HeSoCongSuatPFPhaCUpdatedAt`,
            `HeSoCongSuatPFTrungBinh3Pha`, `HeSoCongSuatPFTrungBinh3PhaQuality`, `HeSoCongSuatPFTrungBinh3PhaUpdatedAt`,
            `TanSoHeThong`, `TanSoHeThongUnit`, `TanSoHeThongQuality`, `TanSoHeThongUpdatedAt`,
            `ChiSoTichLuyDienNangTacDungAPBieuGioBT`, `ChiSoTichLuyDienNangTacDungAPBieuGioBTUnit`, `ChiSoTichLuyDienNangTacDungAPBieuGioBTQuality`, `ChiSoTichLuyDienNangTacDungAPBieuGioBTUpdatedAt`,
            `ChiSoTichLuyDienNangTacDungAPBieuGioCD`, `ChiSoTichLuyDienNangTacDungAPBieuGioCDUnit`, `ChiSoTichLuyDienNangTacDungAPBieuGioCDQuality`, `ChiSoTichLuyDienNangTacDungAPBieuGioCDUpdatedAt`,
            `ChiSoTichLuyDienNangTacDungAPBieuGioTD`, `ChiSoTichLuyDienNangTacDungAPBieuGioTDUnit`, `ChiSoTichLuyDienNangTacDungAPBieuGioTDQuality`, `ChiSoTichLuyDienNangTacDungAPBieuGioTDUpdatedAt`,
            `ChiSoTichLuyDienNangTacDungAPTong`, `ChiSoTichLuyDienNangTacDungAPTongUnit`, `ChiSoTichLuyDienNangTacDungAPTongQuality`, `ChiSoTichLuyDienNangTacDungAPTongUpdatedAt`,
            `ChiSoTichLuyDienNangPhanKhangAQTong`, `ChiSoTichLuyDienNangPhanKhangAQTongUnit`, `ChiSoTichLuyDienNangPhanKhangAQTongQuality`, `ChiSoTichLuyDienNangPhanKhangAQTongUpdatedAt`,
            `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan`, `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUnit`, `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanQuality`, `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUpdatedAt`,
            `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan`, `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUnit`, `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanQuality`, `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUpdatedAt`,
            `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan`, `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUnit`, `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanQuality`, `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUpdatedAt`,
            `ChiSoTichLuyDienNangTacDungAPTongChieuNhan`, `ChiSoTichLuyDienNangTacDungAPTongChieuNhanUnit`, `ChiSoTichLuyDienNangTacDungAPTongChieuNhanQuality`, `ChiSoTichLuyDienNangTacDungAPTongChieuNhanUpdatedAt`,
            `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan`, `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUnit`, `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanQuality`, `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUpdatedAt`,
            `ChiSoChotDienNangAPGioBT`, `ChiSoChotDienNangAPGioBTUnit`, `ChiSoChotDienNangAPGioBTQuality`, `ChiSoChotDienNangAPGioBTUpdatedAt`,
            `ChiSoChotDienNangAPGioCD`, `ChiSoChotDienNangAPGioCDUnit`, `ChiSoChotDienNangAPGioCDQuality`, `ChiSoChotDienNangAPGioCDUpdatedAt`,
            `ChiSoChotDienNangAPGioTD`, `ChiSoChotDienNangAPGioTDUnit`, `ChiSoChotDienNangAPGioTDQuality`, `ChiSoChotDienNangAPGioTDUpdatedAt`,
            `ChiSoChotDienNangAPTong`, `ChiSoChotDienNangAPTongUnit`, `ChiSoChotDienNangAPTongQuality`, `ChiSoChotDienNangAPTongUpdatedAt`,
            `ChiSoChotDienNangAQTong`, `ChiSoChotDienNangAQTongUnit`, `ChiSoChotDienNangAQTongQuality`, `ChiSoChotDienNangAQTongUpdatedAt`,
            `ChiSoChotDienNangAPGioBTChieuNhan`, `ChiSoChotDienNangAPGioBTChieuNhanUnit`, `ChiSoChotDienNangAPGioBTChieuNhanQuality`, `ChiSoChotDienNangAPGioBTChieuNhanUpdatedAt`,
            `ChiSoChotDienNangAPGioCDChieuNhan`, `ChiSoChotDienNangAPGioCDChieuNhanUnit`, `ChiSoChotDienNangAPGioCDChieuNhanQuality`, `ChiSoChotDienNangAPGioCDChieuNhanUpdatedAt`,
            `ChiSoChotDienNangAPGioTDChieuNhan`, `ChiSoChotDienNangAPGioTDChieuNhanUnit`, `ChiSoChotDienNangAPGioTDChieuNhanQuality`, `ChiSoChotDienNangAPGioTDChieuNhanUpdatedAt`,
            `ChiSoChotDienNangAPTongChieuNhan`, `ChiSoChotDienNangAPTongChieuNhanUnit`, `ChiSoChotDienNangAPTongChieuNhanQuality`, `ChiSoChotDienNangAPTongChieuNhanUpdatedAt`,
            `ChiSoChotDienNangAQTongChieuNhan`, `ChiSoChotDienNangAQTongChieuNhanUnit`, `ChiSoChotDienNangAQTongChieuNhanQuality`, `ChiSoChotDienNangAQTongChieuNhanUpdatedAt`,
            `PmaxKyHienTai`, `PmaxKyHienTaiUnit`, `PmaxKyHienTaiQuality`, `PmaxKyHienTaiUpdatedAt`,
            `PmaxKyTruoc`, `PmaxKyTruocUnit`, `PmaxKyTruocQuality`, `PmaxKyTruocUpdatedAt`,
            `MatKetNoi`, `MatKetNoiQuality`, `MatKetNoiUpdatedAt`,
            `CanhBao01`, `CanhBao01Quality`, `CanhBao01UpdatedAt`,
            `CanhBao02`, `CanhBao02Quality`, `CanhBao02UpdatedAt`,
            `CanhBao03`, `CanhBao03Quality`, `CanhBao03UpdatedAt`,
            `CanhBao04`, `CanhBao04Quality`, `CanhBao04UpdatedAt`,
            `CanhBao05`, `CanhBao05Quality`, `CanhBao05UpdatedAt`,
            `CanhBao06`, `CanhBao06Quality`, `CanhBao06UpdatedAt`,
            `CanhBao07`, `CanhBao07Quality`, `CanhBao07UpdatedAt`,
            `CanhBao08`, `CanhBao08Quality`, `CanhBao08UpdatedAt`,
            `CanhBao09`, `CanhBao09Quality`, `CanhBao09UpdatedAt`,
            `CanhBao10`, `CanhBao10Quality`, `CanhBao10UpdatedAt`,
            `MeterYear`, `MeterYearQuality`, `MeterMonth`, `MeterMonthQuality`, `MeterDay`, `MeterDayQuality`,
            `MeterHour`, `MeterHourQuality`, `MeterMinute`, `MeterMinuteQuality`, `MeterSecond`, `MeterSecondQuality`,
            `ClosingYear`, `ClosingYearQuality`, `ClosingMonth`, `ClosingMonthQuality`, `ClosingDay`, `ClosingDayQuality`,
            `ClosingHour`, `ClosingHourQuality`, `ClosingMinute`, `ClosingMinuteQuality`, `ClosingSecond`, `ClosingSecondQuality`,
            `PmaxCurrentYear`, `PmaxCurrentYearQuality`, `PmaxCurrentMonth`, `PmaxCurrentMonthQuality`, `PmaxCurrentDay`, `PmaxCurrentDayQuality`,
            `PmaxCurrentHour`, `PmaxCurrentHourQuality`, `PmaxCurrentMinute`, `PmaxCurrentMinuteQuality`, `PmaxCurrentSecond`, `PmaxCurrentSecondQuality`,
            `PmaxPrevYear`, `PmaxPrevYearQuality`, `PmaxPrevMonth`, `PmaxPrevMonthQuality`, `PmaxPrevDay`, `PmaxPrevDayQuality`,
            `PmaxPrevHour`, `PmaxPrevHourQuality`, `PmaxPrevMinute`, `PmaxPrevMinuteQuality`, `PmaxPrevSecond`, `PmaxPrevSecondQuality`
        )
        SELECT
            r.`MeasurementPointId` AS `Id`,
            r.`MeasurementPointId`,
            MAX(CASE WHEN r.`PktType` = 1 THEN r.`CreatedTime` END) AS `LastTelemetryAt`,
            MAX(CASE WHEN r.`PktType` = 2 THEN r.`CreatedTime` END) AS `LastStatusAt`,
            MAX(CASE WHEN r.`PktType` = 3 THEN r.`CreatedTime` END) AS `LastDateTimeAt`,
            COALESCE(MAX(r.`CreatedTime`), UTC_TIMESTAMP(6)) AS `UpdatedAt`,

            MAX(CASE WHEN r.`TagDesc` = 'DONG_DIEN_PHA_A' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'DONG_DIEN_PHA_A' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'DONG_DIEN_PHA_A' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'DONG_DIEN_PHA_A' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'DONG_DIEN_PHA_B' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'DONG_DIEN_PHA_B' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'DONG_DIEN_PHA_B' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'DONG_DIEN_PHA_B' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'DONG_DIEN_PHA_C' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'DONG_DIEN_PHA_C' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'DONG_DIEN_PHA_C' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'DONG_DIEN_PHA_C' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'DIEN_AP_PHA_A' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'DIEN_AP_PHA_A' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'DIEN_AP_PHA_A' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'DIEN_AP_PHA_A' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'DIEN_AP_PHA_B' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'DIEN_AP_PHA_B' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'DIEN_AP_PHA_B' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'DIEN_AP_PHA_B' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'DIEN_AP_PHA_C' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'DIEN_AP_PHA_C' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'DIEN_AP_PHA_C' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'DIEN_AP_PHA_C' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_PHA_A' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_PHA_A' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_PHA_A' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_PHA_A' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_PHA_B' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_PHA_B' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_PHA_B' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_PHA_B' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_PHA_C' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_PHA_C' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_PHA_C' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_PHA_C' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_TONG_3_PHA' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_TONG_3_PHA' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_TONG_3_PHA' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_TAC_DUNG_TONG_3_PHA' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_PHA_A' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_PHA_A' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_PHA_A' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_PHA_A' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_PHA_B' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_PHA_B' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_PHA_B' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_PHA_B' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_PHA_C' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_PHA_C' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_PHA_C' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_PHA_C' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_TONG_3_PHA' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_TONG_3_PHA' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_TONG_3_PHA' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_TONG_3_PHA' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'HE_SO_CONG_SUAT_PF_PHA_A' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'HE_SO_CONG_SUAT_PF_PHA_A' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'HE_SO_CONG_SUAT_PF_PHA_A' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'HE_SO_CONG_SUAT_PF_PHA_B' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'HE_SO_CONG_SUAT_PF_PHA_B' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'HE_SO_CONG_SUAT_PF_PHA_B' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'HE_SO_CONG_SUAT_PF_PHA_C' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'HE_SO_CONG_SUAT_PF_PHA_C' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'HE_SO_CONG_SUAT_PF_PHA_C' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'HE_SO_CONG_SUAT_PF_TRUNG_BINH_3_PHA' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'HE_SO_CONG_SUAT_PF_TRUNG_BINH_3_PHA' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'HE_SO_CONG_SUAT_PF_TRUNG_BINH_3_PHA' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'TAN_SO_HE_THONG' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'TAN_SO_HE_THONG' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'TAN_SO_HE_THONG' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'TAN_SO_HE_THONG' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT_CHIEU_NHAN' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT_CHIEU_NHAN' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT_CHIEU_NHAN' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT_CHIEU_NHAN' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD_CHIEU_NHAN' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD_CHIEU_NHAN' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD_CHIEU_NHAN' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD_CHIEU_NHAN' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD_CHIEU_NHAN' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD_CHIEU_NHAN' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD_CHIEU_NHAN' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD_CHIEU_NHAN' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG_CHIEU_NHAN' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG_CHIEU_NHAN' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG_CHIEU_NHAN' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG_CHIEU_NHAN' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG_CHIEU_NHAN' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG_CHIEU_NHAN' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG_CHIEU_NHAN' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG_CHIEU_NHAN' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_TONG' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_TONG' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_TONG' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_TONG' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT_CHIEU_NHAN' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT_CHIEU_NHAN' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT_CHIEU_NHAN' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT_CHIEU_NHAN' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD_CHIEU_NHAN' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD_CHIEU_NHAN' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD_CHIEU_NHAN' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD_CHIEU_NHAN' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD_CHIEU_NHAN' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD_CHIEU_NHAN' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD_CHIEU_NHAN' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD_CHIEU_NHAN' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_TONG_CHIEU_NHAN' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_TONG_CHIEU_NHAN' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_TONG_CHIEU_NHAN' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_TONG_CHIEU_NHAN' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG_CHIEU_NHAN' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG_CHIEU_NHAN' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG_CHIEU_NHAN' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG_CHIEU_NHAN' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'PMAX_KY_HIEN_TAI' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'PMAX_KY_HIEN_TAI' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'PMAX_KY_HIEN_TAI' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'PMAX_KY_HIEN_TAI' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'PMAX_KY_TRUOC' THEN `fn_phase1_parse_decimal`(r.`Val`) END),
            MAX(CASE WHEN r.`TagDesc` = 'PMAX_KY_TRUOC' THEN r.`Unit` END),
            MAX(CASE WHEN r.`TagDesc` = 'PMAX_KY_TRUOC' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'PMAX_KY_TRUOC' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'MAT_KET_NOI' THEN CASE WHEN LOWER(TRIM(r.`Val`)) IN ('1', 'true', 'lost') THEN 1 WHEN LOWER(TRIM(r.`Val`)) IN ('0', 'false', 'normal') THEN 0 ELSE NULL END END),
            MAX(CASE WHEN r.`TagDesc` = 'MAT_KET_NOI' THEN COALESCE(r.`Quality`, 1) END),
            MAX(CASE WHEN r.`TagDesc` = 'MAT_KET_NOI' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_01' THEN CASE WHEN LOWER(TRIM(r.`Val`)) IN ('1', 'true') THEN 1 WHEN LOWER(TRIM(r.`Val`)) IN ('0', 'false') THEN 0 ELSE NULL END END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_01' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_01' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_02' THEN CASE WHEN LOWER(TRIM(r.`Val`)) IN ('1', 'true') THEN 1 WHEN LOWER(TRIM(r.`Val`)) IN ('0', 'false') THEN 0 ELSE NULL END END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_02' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_02' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_03' THEN CASE WHEN LOWER(TRIM(r.`Val`)) IN ('1', 'true') THEN 1 WHEN LOWER(TRIM(r.`Val`)) IN ('0', 'false') THEN 0 ELSE NULL END END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_03' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_03' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_04' THEN CASE WHEN LOWER(TRIM(r.`Val`)) IN ('1', 'true') THEN 1 WHEN LOWER(TRIM(r.`Val`)) IN ('0', 'false') THEN 0 ELSE NULL END END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_04' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_04' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_05' THEN CASE WHEN LOWER(TRIM(r.`Val`)) IN ('1', 'true') THEN 1 WHEN LOWER(TRIM(r.`Val`)) IN ('0', 'false') THEN 0 ELSE NULL END END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_05' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_05' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_06' THEN CASE WHEN LOWER(TRIM(r.`Val`)) IN ('1', 'true') THEN 1 WHEN LOWER(TRIM(r.`Val`)) IN ('0', 'false') THEN 0 ELSE NULL END END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_06' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_06' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_07' THEN CASE WHEN LOWER(TRIM(r.`Val`)) IN ('1', 'true') THEN 1 WHEN LOWER(TRIM(r.`Val`)) IN ('0', 'false') THEN 0 ELSE NULL END END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_07' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_07' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_08' THEN CASE WHEN LOWER(TRIM(r.`Val`)) IN ('1', 'true') THEN 1 WHEN LOWER(TRIM(r.`Val`)) IN ('0', 'false') THEN 0 ELSE NULL END END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_08' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_08' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_09' THEN CASE WHEN LOWER(TRIM(r.`Val`)) IN ('1', 'true') THEN 1 WHEN LOWER(TRIM(r.`Val`)) IN ('0', 'false') THEN 0 ELSE NULL END END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_09' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_09' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_10' THEN CASE WHEN LOWER(TRIM(r.`Val`)) IN ('1', 'true') THEN 1 WHEN LOWER(TRIM(r.`Val`)) IN ('0', 'false') THEN 0 ELSE NULL END END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_10' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'CANH_BAO_10' THEN r.`CreatedTime` END),

            MAX(CASE WHEN r.`TagDesc` = 'NAM_HIEN_TAI' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'NAM_HIEN_TAI' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'THANG_HIEN_TAI' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'THANG_HIEN_TAI' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'NGAY_HIEN_TAI' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'NGAY_HIEN_TAI' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'GIO_HIEN_TAI' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'GIO_HIEN_TAI' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'PHUT_HIEN_TAI' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'PHUT_HIEN_TAI' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'GIAY_HIEN_TAI' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'GIAY_HIEN_TAI' THEN r.`Quality` END),

            MAX(CASE WHEN r.`TagDesc` = 'NAM_CHOT_GAN_NHAT' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'NAM_CHOT_GAN_NHAT' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'THANG_CHOT_GAN_NHAT' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'THANG_CHOT_GAN_NHAT' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'NGAY_CHOT_GAN_NHAT' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'NGAY_CHOT_GAN_NHAT' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'GIO_CHOT_GAN_NHAT' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'GIO_CHOT_GAN_NHAT' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'PHUT_CHOT_GAN_NHAT' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'PHUT_CHOT_GAN_NHAT' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'GIAY_CHOT_GAN_NHAT' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'GIAY_CHOT_GAN_NHAT' THEN r.`Quality` END),

            MAX(CASE WHEN r.`TagDesc` = 'NAM_PMAX_KY_HIEN_TAI' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'NAM_PMAX_KY_HIEN_TAI' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'THANG_PMAX_KY_HIEN_TAI' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'THANG_PMAX_KY_HIEN_TAI' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'NGAY_PMAX_KY_HIEN_TAI' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'NGAY_PMAX_KY_HIEN_TAI' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'GIO_PMAX_KY_HIEN_TAI' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'GIO_PMAX_KY_HIEN_TAI' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'PHUT_PMAX_KY_HIEN_TAI' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'PHUT_PMAX_KY_HIEN_TAI' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'GIAY_PMAX_KY_HIEN_TAI' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'GIAY_PMAX_KY_HIEN_TAI' THEN r.`Quality` END),

            MAX(CASE WHEN r.`TagDesc` = 'NAM_PMAX_KY_TRUOC' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'NAM_PMAX_KY_TRUOC' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'THANG_PMAX_KY_TRUOC' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'THANG_PMAX_KY_TRUOC' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'NGAY_PMAX_KY_TRUOC' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'NGAY_PMAX_KY_TRUOC' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'GIO_PMAX_KY_TRUOC' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'GIO_PMAX_KY_TRUOC' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'PHUT_PMAX_KY_TRUOC' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'PHUT_PMAX_KY_TRUOC' THEN r.`Quality` END),
            MAX(CASE WHEN r.`TagDesc` = 'GIAY_PMAX_KY_TRUOC' THEN `fn_phase1_parse_smallint`(r.`Val`) END), MAX(CASE WHEN r.`TagDesc` = 'GIAY_PMAX_KY_TRUOC' THEN r.`Quality` END)
        FROM (
            SELECT
                `MeasurementPointId`, `TagDesc`, `Val`, `Unit`, `Quality`, `CreatedTime`, `PktType`,
                ROW_NUMBER() OVER (PARTITION BY `MeasurementPointId`, `TagDesc` ORDER BY `CreatedTime` DESC, `OrigId` DESC) AS rn
            FROM `tmp_phase1_snap_raw`
        ) r
        WHERE r.rn = 1
        GROUP BY r.`MeasurementPointId`;

        DROP TEMPORARY TABLE IF EXISTS `tmp_phase1_snap_raw`;

        SELECT COUNT(*) INTO v_actual_snap_pts FROM `MeasurementPointCurrentSnapshots`;
        IF v_actual_snap_pts < v_expected_snap_pts THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Verification Gate Failed: MeasurementPointCurrentSnapshots missing measurement points!';
        END IF;
    END IF;

    -- --------------------------------------------------------------------------
    -- STEP 3: Populate MeasurementPointTimeSeries (Chunked by month, 1s buckets)
    -- --------------------------------------------------------------------------
    IF v_tbl_monitoring IS NOT NULL OR v_tbl_archive IS NOT NULL THEN
        SELECT COALESCE(MAX(`Id`), 100000000000000000) INTO @ts_seq FROM `MeasurementPointTimeSeries`;

        IF v_min_dt IS NOT NULL AND v_max_dt IS NOT NULL THEN
            IF YEAR(v_min_dt) < 2020 THEN SET v_min_dt = '2020-01-01 00:00:00'; END IF;
            SET v_cur_m = DATE_FORMAT(v_min_dt, '%Y-%m-01');
            SET v_end_m = DATE_FORMAT(v_max_dt, '%Y-%m-01');

            WHILE v_cur_m <= v_end_m DO
                SET v_next_m = DATE_ADD(v_cur_m, INTERVAL 1 MONTH);

                DROP TEMPORARY TABLE IF EXISTS `tmp_phase1_ts_month`;
                CREATE TEMPORARY TABLE `tmp_phase1_ts_month` (
                    `MeasurementPointId` bigint NOT NULL,
                    `RecordedAt` datetime(6) NOT NULL,
                    `TagDesc` varchar(128) NOT NULL,
                    `ValDec` decimal(18,4) DEFAULT NULL,
                    `Quality` tinyint(1) DEFAULT 1,
                    KEY `IX_tmp_ts_m` (`MeasurementPointId`, `RecordedAt`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                IF v_tbl_monitoring IS NOT NULL THEN
                    SET @sql = CONCAT(
                        'INSERT INTO `tmp_phase1_ts_month` (`MeasurementPointId`, `RecordedAt`, `TagDesc`, `ValDec`, `Quality`) ',
                        'SELECT m.`MeasurementPointsId`, ',
                        'CAST(DATE_FORMAT(m.`CreatedTime`, ''%Y-%m-%d %H:%i:%s'') AS DATETIME(6)), ',
                        'UPPER(TRIM(COALESCE(NULLIF(m.`TagNameDescription`, ''''), tg.`Description`, ''''))), ',
                        '`fn_phase1_parse_decimal`(m.`Value`), COALESCE(m.`Quanlity`, 1) ',
                        'FROM `', v_tbl_monitoring, '` m ',
                        IF(v_tbl_points IS NOT NULL, CONCAT('INNER JOIN `', v_tbl_points, '` mp ON m.`MeasurementPointsId` = mp.`Id` '), ''),
                        IF(v_tbl_tagname IS NOT NULL, CONCAT('LEFT JOIN `', v_tbl_tagname, '` tg ON m.`TagNameId` = tg.`Id` '), ''),
                        'WHERE m.`CreatedTime` >= ''', DATE_FORMAT(v_cur_m, '%Y-%m-%d 00:00:00'), ''' ',
                        'AND m.`CreatedTime` < ''', DATE_FORMAT(v_next_m, '%Y-%m-%d 00:00:00'), ''' ',
                        'AND `fn_phase1_parse_decimal`(m.`Value`) IS NOT NULL'
                    );
                    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
                END IF;

                IF v_tbl_archive IS NOT NULL THEN
                    SET @sql = CONCAT(
                        'INSERT INTO `tmp_phase1_ts_month` (`MeasurementPointId`, `RecordedAt`, `TagDesc`, `ValDec`, `Quality`) ',
                        'SELECT a.`MeasurementPointsId`, ',
                        'CAST(DATE_FORMAT(a.`CreatedTime`, ''%Y-%m-%d %H:%i:%s'') AS DATETIME(6)), ',
                        'UPPER(TRIM(COALESCE(NULLIF(a.`TagNameDescription`, ''''), tg.`Description`, ''''))), ',
                        '`fn_phase1_parse_decimal`(a.`Value`), COALESCE(a.`Quanlity`, 1) ',
                        'FROM `', v_tbl_archive, '` a ',
                        IF(v_tbl_points IS NOT NULL, CONCAT('INNER JOIN `', v_tbl_points, '` mp ON a.`MeasurementPointsId` = mp.`Id` '), ''),
                        IF(v_tbl_tagname IS NOT NULL, CONCAT('LEFT JOIN `', v_tbl_tagname, '` tg ON a.`TagNameId` = tg.`Id` '), ''),
                        'WHERE a.`CreatedTime` >= ''', DATE_FORMAT(v_cur_m, '%Y-%m-%d 00:00:00'), ''' ',
                        'AND a.`CreatedTime` < ''', DATE_FORMAT(v_next_m, '%Y-%m-%d 00:00:00'), ''' ',
                        'AND `fn_phase1_parse_decimal`(a.`Value`) IS NOT NULL'
                    );
                    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
                END IF;

                -- Keep only numeric telemetry/energy/latch descriptions mapped in MeasurementPointTimeSeries
                DELETE FROM `tmp_phase1_ts_month`
                WHERE `TagDesc` NOT IN (
                    'DONG_DIEN_PHA_A','DONG_DIEN_PHA_B','DONG_DIEN_PHA_C',
                    'DIEN_AP_PHA_A','DIEN_AP_PHA_B','DIEN_AP_PHA_C',
                    'CONG_SUAT_TAC_DUNG_PHA_A','CONG_SUAT_TAC_DUNG_PHA_B','CONG_SUAT_TAC_DUNG_PHA_C','CONG_SUAT_TAC_DUNG_TONG_3_PHA',
                    'CONG_SUAT_PHAN_KHANG_PHA_A','CONG_SUAT_PHAN_KHANG_PHA_B','CONG_SUAT_PHAN_KHANG_PHA_C','CONG_SUAT_PHAN_KHANG_TONG_3_PHA',
                    'HE_SO_CONG_SUAT_PF_PHA_A','HE_SO_CONG_SUAT_PF_PHA_B','HE_SO_CONG_SUAT_PF_PHA_C','HE_SO_CONG_SUAT_PF_TRUNG_BINH_3_PHA',
                    'TAN_SO_HE_THONG',
                    'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT','CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD',
                    'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD','CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG','CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG',
                    'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT_CHIEU_NHAN','CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD_CHIEU_NHAN',
                    'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD_CHIEU_NHAN','CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG_CHIEU_NHAN','CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG_CHIEU_NHAN',
                    'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT','CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD','CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD',
                    'CHI_SO_CHOT_DIEN_NANG_AP_TONG','CHI_SO_CHOT_DIEN_NANG_AQ_TONG',
                    'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT_CHIEU_NHAN','CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD_CHIEU_NHAN','CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD_CHIEU_NHAN',
                    'CHI_SO_CHOT_DIEN_NANG_AP_TONG_CHIEU_NHAN','CHI_SO_CHOT_DIEN_NANG_AQ_TONG_CHIEU_NHAN',
                    'PMAX_KY_HIEN_TAI','PMAX_KY_TRUOC'
                );

                SELECT COUNT(*) INTO v_expected_ts_cnt
                FROM (SELECT `MeasurementPointId`, `RecordedAt` FROM `tmp_phase1_ts_month` GROUP BY `MeasurementPointId`, `RecordedAt`) b;

                IF v_expected_ts_cnt > 0 THEN
                    INSERT INTO `MeasurementPointTimeSeries` (
                        `Id`, `MeasurementPointId`, `PartitionYm`, `RecordedAt`, `IngestedAt`, `QualityFlag`, `SourcePacketType`,
                        `DongDienPhaA`, `DongDienPhaB`, `DongDienPhaC`,
                        `DienApPhaA`, `DienApPhaB`, `DienApPhaC`,
                        `CongSuatTacDungPhaA`, `CongSuatTacDungPhaB`, `CongSuatTacDungPhaC`, `CongSuatTacDungTong3Pha`,
                        `CongSuatPhanKhangPhaA`, `CongSuatPhanKhangPhaB`, `CongSuatPhanKhangPhaC`, `CongSuatPhanKhangTong3Pha`,
                        `HeSoCongSuatPFPhaA`, `HeSoCongSuatPFPhaB`, `HeSoCongSuatPFPhaC`, `HeSoCongSuatPFTrungBinh3Pha`, `TanSoHeThong`,
                        `ChiSoTichLuyDienNangTacDungAPBieuGioBT`, `ChiSoTichLuyDienNangTacDungAPBieuGioCD`, `ChiSoTichLuyDienNangTacDungAPBieuGioTD`,
                        `ChiSoTichLuyDienNangTacDungAPTong`, `ChiSoTichLuyDienNangPhanKhangAQTong`,
                        `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan`, `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan`, `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan`,
                        `ChiSoTichLuyDienNangTacDungAPTongChieuNhan`, `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan`,
                        `ChiSoChotDienNangAPGioBT`, `ChiSoChotDienNangAPGioCD`, `ChiSoChotDienNangAPGioTD`,
                        `ChiSoChotDienNangAPTong`, `ChiSoChotDienNangAQTong`,
                        `ChiSoChotDienNangAPGioBTChieuNhan`, `ChiSoChotDienNangAPGioCDChieuNhan`, `ChiSoChotDienNangAPGioTDChieuNhan`,
                        `ChiSoChotDienNangAPTongChieuNhan`, `ChiSoChotDienNangAQTongChieuNhan`,
                        `PmaxKyHienTai`, `PmaxKyTruoc`
                    )
                    SELECT
                        (@ts_seq := @ts_seq + 1),
                        g.`MeasurementPointId`,
                        (YEAR(DATE_ADD(g.`RecordedAt`, INTERVAL 7 HOUR)) * 100 + MONTH(DATE_ADD(g.`RecordedAt`, INTERVAL 7 HOUR))),
                        g.`RecordedAt`,
                        UTC_TIMESTAMP(6),
                        g.`QualityFlag`,
                        1,
                        g.`DongDienPhaA`, g.`DongDienPhaB`, g.`DongDienPhaC`,
                        g.`DienApPhaA`, g.`DienApPhaB`, g.`DienApPhaC`,
                        g.`CongSuatTacDungPhaA`, g.`CongSuatTacDungPhaB`, g.`CongSuatTacDungPhaC`, g.`CongSuatTacDungTong3Pha`,
                        g.`CongSuatPhanKhangPhaA`, g.`CongSuatPhanKhangPhaB`, g.`CongSuatPhanKhangPhaC`, g.`CongSuatPhanKhangTong3Pha`,
                        g.`HeSoCongSuatPFPhaA`, g.`HeSoCongSuatPFPhaB`, g.`HeSoCongSuatPFPhaC`, g.`HeSoCongSuatPFTrungBinh3Pha`, g.`TanSoHeThong`,
                        g.`ChiSoTichLuyDienNangTacDungAPBieuGioBT`, g.`ChiSoTichLuyDienNangTacDungAPBieuGioCD`, g.`ChiSoTichLuyDienNangTacDungAPBieuGioTD`,
                        g.`ChiSoTichLuyDienNangTacDungAPTong`, g.`ChiSoTichLuyDienNangPhanKhangAQTong`,
                        g.`ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan`, g.`ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan`, g.`ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan`,
                        g.`ChiSoTichLuyDienNangTacDungAPTongChieuNhan`, g.`ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan`,
                        g.`ChiSoChotDienNangAPGioBT`, g.`ChiSoChotDienNangAPGioCD`, g.`ChiSoChotDienNangAPGioTD`,
                        g.`ChiSoChotDienNangAPTong`, g.`ChiSoChotDienNangAQTong`,
                        g.`ChiSoChotDienNangAPGioBTChieuNhan`, g.`ChiSoChotDienNangAPGioCDChieuNhan`, g.`ChiSoChotDienNangAPGioTDChieuNhan`,
                        g.`ChiSoChotDienNangAPTongChieuNhan`, g.`ChiSoChotDienNangAQTongChieuNhan`,
                        g.`PmaxKyHienTai`, g.`PmaxKyTruoc`
                    FROM (
                        SELECT
                            src.`MeasurementPointId`,
                            src.`RecordedAt`,
                            MIN(src.`Quality`) AS `QualityFlag`,
                            MAX(CASE WHEN src.`TagDesc` = 'DONG_DIEN_PHA_A' THEN src.`ValDec` END) AS `DongDienPhaA`,
                            MAX(CASE WHEN src.`TagDesc` = 'DONG_DIEN_PHA_B' THEN src.`ValDec` END) AS `DongDienPhaB`,
                            MAX(CASE WHEN src.`TagDesc` = 'DONG_DIEN_PHA_C' THEN src.`ValDec` END) AS `DongDienPhaC`,
                            MAX(CASE WHEN src.`TagDesc` = 'DIEN_AP_PHA_A' THEN src.`ValDec` END) AS `DienApPhaA`,
                            MAX(CASE WHEN src.`TagDesc` = 'DIEN_AP_PHA_B' THEN src.`ValDec` END) AS `DienApPhaB`,
                            MAX(CASE WHEN src.`TagDesc` = 'DIEN_AP_PHA_C' THEN src.`ValDec` END) AS `DienApPhaC`,
                            MAX(CASE WHEN src.`TagDesc` = 'CONG_SUAT_TAC_DUNG_PHA_A' THEN src.`ValDec` END) AS `CongSuatTacDungPhaA`,
                            MAX(CASE WHEN src.`TagDesc` = 'CONG_SUAT_TAC_DUNG_PHA_B' THEN src.`ValDec` END) AS `CongSuatTacDungPhaB`,
                            MAX(CASE WHEN src.`TagDesc` = 'CONG_SUAT_TAC_DUNG_PHA_C' THEN src.`ValDec` END) AS `CongSuatTacDungPhaC`,
                            MAX(CASE WHEN src.`TagDesc` = 'CONG_SUAT_TAC_DUNG_TONG_3_PHA' THEN src.`ValDec` END) AS `CongSuatTacDungTong3Pha`,
                            MAX(CASE WHEN src.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_PHA_A' THEN src.`ValDec` END) AS `CongSuatPhanKhangPhaA`,
                            MAX(CASE WHEN src.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_PHA_B' THEN src.`ValDec` END) AS `CongSuatPhanKhangPhaB`,
                            MAX(CASE WHEN src.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_PHA_C' THEN src.`ValDec` END) AS `CongSuatPhanKhangPhaC`,
                            MAX(CASE WHEN src.`TagDesc` = 'CONG_SUAT_PHAN_KHANG_TONG_3_PHA' THEN src.`ValDec` END) AS `CongSuatPhanKhangTong3Pha`,
                            MAX(CASE WHEN src.`TagDesc` = 'HE_SO_CONG_SUAT_PF_PHA_A' THEN src.`ValDec` END) AS `HeSoCongSuatPFPhaA`,
                            MAX(CASE WHEN src.`TagDesc` = 'HE_SO_CONG_SUAT_PF_PHA_B' THEN src.`ValDec` END) AS `HeSoCongSuatPFPhaB`,
                            MAX(CASE WHEN src.`TagDesc` = 'HE_SO_CONG_SUAT_PF_PHA_C' THEN src.`ValDec` END) AS `HeSoCongSuatPFPhaC`,
                            MAX(CASE WHEN src.`TagDesc` = 'HE_SO_CONG_SUAT_PF_TRUNG_BINH_3_PHA' THEN src.`ValDec` END) AS `HeSoCongSuatPFTrungBinh3Pha`,
                            MAX(CASE WHEN src.`TagDesc` = 'TAN_SO_HE_THONG' THEN src.`ValDec` END) AS `TanSoHeThong`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT' THEN src.`ValDec` END) AS `ChiSoTichLuyDienNangTacDungAPBieuGioBT`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD' THEN src.`ValDec` END) AS `ChiSoTichLuyDienNangTacDungAPBieuGioCD`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD' THEN src.`ValDec` END) AS `ChiSoTichLuyDienNangTacDungAPBieuGioTD`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG' THEN src.`ValDec` END) AS `ChiSoTichLuyDienNangTacDungAPTong`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG' THEN src.`ValDec` END) AS `ChiSoTichLuyDienNangPhanKhangAQTong`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT_CHIEU_NHAN' THEN src.`ValDec` END) AS `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD_CHIEU_NHAN' THEN src.`ValDec` END) AS `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD_CHIEU_NHAN' THEN src.`ValDec` END) AS `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG_CHIEU_NHAN' THEN src.`ValDec` END) AS `ChiSoTichLuyDienNangTacDungAPTongChieuNhan`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG_CHIEU_NHAN' THEN src.`ValDec` END) AS `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT' THEN src.`ValDec` END) AS `ChiSoChotDienNangAPGioBT`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD' THEN src.`ValDec` END) AS `ChiSoChotDienNangAPGioCD`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD' THEN src.`ValDec` END) AS `ChiSoChotDienNangAPGioTD`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_TONG' THEN src.`ValDec` END) AS `ChiSoChotDienNangAPTong`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG' THEN src.`ValDec` END) AS `ChiSoChotDienNangAQTong`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT_CHIEU_NHAN' THEN src.`ValDec` END) AS `ChiSoChotDienNangAPGioBTChieuNhan`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD_CHIEU_NHAN' THEN src.`ValDec` END) AS `ChiSoChotDienNangAPGioCDChieuNhan`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD_CHIEU_NHAN' THEN src.`ValDec` END) AS `ChiSoChotDienNangAPGioTDChieuNhan`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AP_TONG_CHIEU_NHAN' THEN src.`ValDec` END) AS `ChiSoChotDienNangAPTongChieuNhan`,
                            MAX(CASE WHEN src.`TagDesc` = 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG_CHIEU_NHAN' THEN src.`ValDec` END) AS `ChiSoChotDienNangAQTongChieuNhan`,
                            MAX(CASE WHEN src.`TagDesc` = 'PMAX_KY_HIEN_TAI' THEN src.`ValDec` END) AS `PmaxKyHienTai`,
                            MAX(CASE WHEN src.`TagDesc` = 'PMAX_KY_TRUOC' THEN src.`ValDec` END) AS `PmaxKyTruoc`
                        FROM `tmp_phase1_ts_month` src
                        GROUP BY src.`MeasurementPointId`, src.`RecordedAt`
                    ) g
                    WHERE NOT EXISTS (
                        SELECT 1 FROM `MeasurementPointTimeSeries` existing
                        WHERE existing.`MeasurementPointId` = g.`MeasurementPointId`
                          AND existing.`PartitionYm` = (YEAR(DATE_ADD(g.`RecordedAt`, INTERVAL 7 HOUR)) * 100 + MONTH(DATE_ADD(g.`RecordedAt`, INTERVAL 7 HOUR)))
                          AND existing.`RecordedAt` = g.`RecordedAt`
                    );

                    SELECT COUNT(*) INTO v_actual_ts_cnt
                    FROM (
                        SELECT DISTINCT t.`MeasurementPointId`, t.`RecordedAt`
                        FROM `MeasurementPointTimeSeries` t
                        WHERE t.`RecordedAt` >= v_cur_m AND t.`RecordedAt` < v_next_m
                    ) matched
                    INNER JOIN (
                        SELECT DISTINCT `MeasurementPointId`, `RecordedAt` FROM `tmp_phase1_ts_month`
                    ) exp_b
                      ON matched.`MeasurementPointId` = exp_b.`MeasurementPointId`
                     AND matched.`RecordedAt` = exp_b.`RecordedAt`;

                    IF v_actual_ts_cnt < v_expected_ts_cnt THEN
                        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Verification Gate Failed: MeasurementPointTimeSeries monthly bucket parity check failed!';
                    END IF;
                END IF;

                DROP TEMPORARY TABLE IF EXISTS `tmp_phase1_ts_month`;
                SET v_cur_m = v_next_m;
            END WHILE;
        END IF;
    END IF;

    -- --------------------------------------------------------------------------
    -- STEP 4: Backfill MeasurementPointMonitoringHourly -> HalfHourOperatingReports
    -- (Numeric values only; drops redundant legacy *Unit columns if present)
    -- --------------------------------------------------------------------------
    IF v_tbl_operating IS NOT NULL THEN
        SET @v_op_b = 0;
        SET @sql = CONCAT('SELECT COUNT(*) INTO @v_op_b FROM `', v_tbl_operating, '`');
        PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
        SET v_op_before = @v_op_b;

        IF v_tbl_hourly IS NOT NULL THEN
            SET @op_seq = 0;
            SET @sql = CONCAT('SELECT COALESCE(MAX(`Id`), 200000000000000000) INTO @op_seq FROM `', v_tbl_operating, '`');
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

            SET @sql = CONCAT(
                'INSERT INTO `', v_tbl_operating, '` (',
                '`Id`, `MeasurementPointId`, `CreatedTime`, ',
                '`DienApPhaA`, `DienApPhaB`, `DienApPhaC`, ',
                '`DongDienPhaA`, `DongDienPhaB`, `DongDienPhaC`, ',
                '`CongSuatTacDungTong3Pha`, `CongSuatPhanKhangTong3Pha`, ',
                '`HeSoCongSuatPF`, `TanSoHeThong`) ',
                'SELECT ',
                '(@op_seq := @op_seq + 1), h.`MeasurementPointId`, h.`CreatedTime`, ',
                '`fn_phase1_parse_double`(MAX(h.`TrungBinhDienApPhaA`)), ',
                '`fn_phase1_parse_double`(MAX(h.`TrungBinhDienApPhaB`)), ',
                '`fn_phase1_parse_double`(MAX(h.`TrungBinhDienApPhaC`)), ',
                '`fn_phase1_parse_double`(MAX(h.`TrungBinhDongDienPhaA`)), ',
                '`fn_phase1_parse_double`(MAX(h.`TrungBinhDongDienPhaB`)), ',
                '`fn_phase1_parse_double`(MAX(h.`TrungBinhDongDienPhaC`)), ',
                '`fn_phase1_parse_double`(MAX(h.`TrungBinhCongSuatTacDungTong3Pha`)), ',
                '`fn_phase1_parse_double`(MAX(h.`TrungBinhCongSuatPhanKhangTong3Pha`)), ',
                'NULL, NULL ',
                'FROM `', v_tbl_hourly, '` h ',
                IF(v_tbl_points IS NOT NULL, CONCAT('INNER JOIN `', v_tbl_points, '` mp ON h.`MeasurementPointId` = mp.`Id` '), ''),
                'WHERE h.`CreatedTime` IS NOT NULL ',
                'AND NOT EXISTS (SELECT 1 FROM `', v_tbl_operating, '` op WHERE op.`MeasurementPointId` = h.`MeasurementPointId` AND op.`CreatedTime` = h.`CreatedTime`) ',
                'GROUP BY h.`MeasurementPointId`, h.`CreatedTime`'
            );
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

            -- Verify zero missing hourly slots
            SET @v_miss = 0;
            SET @sql = CONCAT(
                'SELECT COUNT(*) INTO @v_miss FROM (',
                'SELECT DISTINCT h.`MeasurementPointId`, h.`CreatedTime` FROM `', v_tbl_hourly, '` h ',
                IF(v_tbl_points IS NOT NULL, CONCAT('INNER JOIN `', v_tbl_points, '` mp ON h.`MeasurementPointId` = mp.`Id` '), ''),
                'WHERE h.`CreatedTime` IS NOT NULL',
                ') req WHERE NOT EXISTS (',
                'SELECT 1 FROM `', v_tbl_operating, '` op WHERE op.`MeasurementPointId` = req.`MeasurementPointId` AND op.`CreatedTime` = req.`CreatedTime`)'
            );
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
            SET v_hourly_missing = @v_miss;
            IF v_hourly_missing > 0 THEN
                SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Verification Gate Failed: Some Hourly operating rows were not backfilled!';
            END IF;
        END IF;

        -- Deduplicate HalfHourOperatingReports before creating UNIQUE INDEX
        SET @sql = CONCAT(
            'DELETE op1 FROM `', v_tbl_operating, '` op1 ',
            'INNER JOIN `', v_tbl_operating, '` op2 ',
            'ON op1.`MeasurementPointId` = op2.`MeasurementPointId` ',
            'AND op1.`CreatedTime` = op2.`CreatedTime` ',
            'AND op1.`CreatedTime` IS NOT NULL ',
            'AND op1.`Id` < op2.`Id`'
        );
        PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

        SELECT COUNT(*) INTO v_op_ux_exists
        FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = v_tbl_operating
          AND NON_UNIQUE = 0
          AND INDEX_NAME = 'UX_MPHalfHourOp_Point_Time';

        IF v_op_ux_exists = 0 THEN
            SET @sql = CONCAT('ALTER TABLE `', v_tbl_operating, '` ADD UNIQUE INDEX `UX_MPHalfHourOp_Point_Time` (`MeasurementPointId`, `CreatedTime`)');
            PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
        END IF;

        -- Drop redundant *Unit columns (units resolve from TagName configuration)
        BEGIN
            DECLARE v_op_idx INT DEFAULT 1;
            DECLARE v_op_unit_col VARCHAR(128);
            WHILE v_op_idx <= 9 DO
                SET v_op_unit_col = ELT(v_op_idx,
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
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = v_tbl_operating AND COLUMN_NAME = v_op_unit_col
                ) THEN
                    SET @sql = CONCAT('ALTER TABLE `', v_tbl_operating, '` DROP COLUMN `', v_op_unit_col, '`');
                    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
                END IF;
                SET v_op_idx = v_op_idx + 1;
            END WHILE;
        END;
    END IF;

    -- --------------------------------------------------------------------------
    -- STEP 5: Upgrade HalfHourEnergyReports & DailyEnergyReports In-Place
    -- --------------------------------------------------------------------------
    CALL `sp_phase1_migrate_energy_table`('MeasurementPointHalfHourEnergyReports', 'UX_MPHalfHourEnergy_Point_Time');
    CALL `sp_phase1_migrate_energy_table`('MeasurementPointDailyEnergyReports', 'UX_MPDailyEnergy_Point_Time');

    -- --------------------------------------------------------------------------
    -- STEP 6: Upgrade MeasurementPointEvents & MeasurementPointAlerts
    -- --------------------------------------------------------------------------
    CALL `sp_phase1_migrate_event_alert_table`('MeasurementPointEvents', 'IX_MPEvents_PointId_CreatedTime');
    CALL `sp_phase1_migrate_event_alert_table`('MeasurementPointAlerts', 'IX_MPAlerts_PointId_CreatedTime');

    -- --------------------------------------------------------------------------
    -- STEP 7: Drop Legacy Tables ONLY AFTER All Verification Gates Passed
    -- --------------------------------------------------------------------------
    IF v_tbl_monitoring IS NOT NULL THEN
        SET @sql = CONCAT('DROP TABLE IF EXISTS `', v_tbl_monitoring, '`');
        PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
    END IF;
    IF v_tbl_archive IS NOT NULL THEN
        SET @sql = CONCAT('DROP TABLE IF EXISTS `', v_tbl_archive, '`');
        PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
    END IF;
    IF v_tbl_datetime IS NOT NULL THEN
        SET @sql = CONCAT('DROP TABLE IF EXISTS `', v_tbl_datetime, '`');
        PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
    END IF;
    IF v_tbl_status IS NOT NULL THEN
        SET @sql = CONCAT('DROP TABLE IF EXISTS `', v_tbl_status, '`');
        PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
    END IF;
    IF v_tbl_hourly IS NOT NULL THEN
        SET @sql = CONCAT('DROP TABLE IF EXISTS `', v_tbl_hourly, '`');
        PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
    END IF;

    -- Remove obsolete MySQL events & routines referencing dropped legacy EAV tables
    DROP EVENT IF EXISTS `evt_clean_measpointmonitoring`;
    DROP EVENT IF EXISTS `evt_clean_measpointmonitoringbystatus`;
    DROP PROCEDURE IF EXISTS `batch_delete_old_records`;
    DROP PROCEDURE IF EXISTS `clean_measurement_batch`;
    DROP PROCEDURE IF EXISTS `sp_clean_measurementpointmonitoring`;
    DROP PROCEDURE IF EXISTS `sp_clean_measurementpointmonitoringbystatus`;
END$$

DELIMITER ;

CALL `sp_phase1_migrate`();

DROP PROCEDURE IF EXISTS `sp_phase1_migrate`;
DROP PROCEDURE IF EXISTS `sp_phase1_migrate_event_alert_table`;
DROP PROCEDURE IF EXISTS `sp_phase1_migrate_energy_table`;
DROP PROCEDURE IF EXISTS `sp_phase1_add_monthly_partitions`;
DROP FUNCTION IF EXISTS `fn_phase1_parse_smallint`;
DROP FUNCTION IF EXISTS `fn_phase1_parse_double`;
DROP FUNCTION IF EXISTS `fn_phase1_parse_decimal`;
DROP FUNCTION IF EXISTS `fn_phase1_is_suspected`;

SET FOREIGN_KEY_CHECKS = 1;
