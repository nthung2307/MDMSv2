-- ==============================================================================
-- MDMSv2 Migration: Standardize Energy & Metering Data Types to DECIMAL(18, 4) & DECIMAL(5, 2)
-- Date: 2026-11-09 (Idempotent & Fresh-Install Safe)
-- Purpose:
--   1. Chuyển đổi toàn bộ số liệu đo đếm, chỉ số công tơ, hệ số nhân và sản lượng
--      tính tiền từ kiểu số thực (DOUBLE) sang số thập phân chuẩn xác cao DECIMAL(18, 4).
--   2. Chuẩn hóa hệ số công suất trung bình Cosφ (PF) sang DECIMAL(5, 2).
--   3. Tự động bổ sung nếu thiếu các cột mở rộng (NewPreviousReading, CosPhi, PmaxTime...)
--      để chạy an toàn 100% trên cả môi trường cài mới (amr_full.sql) và nâng cấp VM.
-- ==============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

DROP PROCEDURE IF EXISTS `sp_phase3_ensure_column`;

DELIMITER $$

CREATE PROCEDURE `sp_phase3_ensure_column`(
    IN p_target_table VARCHAR(128),
    IN p_column_name VARCHAR(128),
    IN p_column_def VARCHAR(255)
)
BEGIN
    DECLARE v_tbl VARCHAR(128) DEFAULT NULL;
    DECLARE v_col_exists INT DEFAULT 0;

    SELECT TABLE_NAME INTO v_tbl
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = DATABASE()
      AND LOWER(TABLE_NAME) = LOWER(p_target_table)
    LIMIT 1;

    IF v_tbl IS NOT NULL THEN
        SELECT COUNT(*) INTO v_col_exists
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = v_tbl
          AND COLUMN_NAME = p_column_name;

        IF v_col_exists > 0 THEN
            SET @sql = CONCAT('ALTER TABLE `', v_tbl, '` MODIFY COLUMN `', p_column_name, '` ', p_column_def);
        ELSE
            SET @sql = CONCAT('ALTER TABLE `', v_tbl, '` ADD COLUMN `', p_column_name, '` ', p_column_def);
        END IF;

        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

DELIMITER ;

-- ------------------------------------------------------------------------------
-- 1. Bảng `measurementpoints` (Điểm đo)
-- ------------------------------------------------------------------------------
CALL `sp_phase3_ensure_column`('measurementpoints', 'MultiplierFactor', 'DECIMAL(18, 4) NOT NULL DEFAULT 1.0000');

-- ------------------------------------------------------------------------------
-- 2. Bảng `gcstemporary` (Số liệu tạm ghi chỉ số trước chốt)
-- ------------------------------------------------------------------------------
CALL `sp_phase3_ensure_column`('gcstemporary', 'PreviousReading', 'DECIMAL(18, 4) DEFAULT NULL');
CALL `sp_phase3_ensure_column`('gcstemporary', 'CurrentReading', 'DECIMAL(18, 4) DEFAULT NULL');
CALL `sp_phase3_ensure_column`('gcstemporary', 'NewCurrentReading', 'DECIMAL(18, 4) DEFAULT NULL');
CALL `sp_phase3_ensure_column`('gcstemporary', 'NewPreviousReading', 'DECIMAL(18, 4) DEFAULT NULL');
CALL `sp_phase3_ensure_column`('gcstemporary', 'MultiplierFactor', 'DECIMAL(18, 4) NOT NULL DEFAULT 1.0000');
CALL `sp_phase3_ensure_column`('gcstemporary', 'Consumption', 'DECIMAL(18, 4) DEFAULT NULL');
CALL `sp_phase3_ensure_column`('gcstemporary', 'CosPhi', 'DECIMAL(5, 2) DEFAULT NULL');
CALL `sp_phase3_ensure_column`('gcstemporary', 'PmaxTime', 'DATETIME(6) DEFAULT NULL');

-- ------------------------------------------------------------------------------
-- 3. Bảng `gcsmonthlydetail` (Chi tiết chỉ số kỳ chốt đã lưu trữ)
-- ------------------------------------------------------------------------------
CALL `sp_phase3_ensure_column`('gcsmonthlydetail', 'PreviousReading', 'DECIMAL(18, 4) DEFAULT NULL');
CALL `sp_phase3_ensure_column`('gcsmonthlydetail', 'CurrentReading', 'DECIMAL(18, 4) DEFAULT NULL');
CALL `sp_phase3_ensure_column`('gcsmonthlydetail', 'MultiplierFactor', 'DECIMAL(18, 4) NOT NULL DEFAULT 1.0000');
CALL `sp_phase3_ensure_column`('gcsmonthlydetail', 'Consumption', 'DECIMAL(18, 4) DEFAULT NULL');
CALL `sp_phase3_ensure_column`('gcsmonthlydetail', 'CosPhi', 'DECIMAL(5, 2) DEFAULT NULL');
CALL `sp_phase3_ensure_column`('gcsmonthlydetail', 'ReadingTime', 'DATETIME(6) DEFAULT NULL');
CALL `sp_phase3_ensure_column`('gcsmonthlydetail', 'PmaxTime', 'DATETIME(6) DEFAULT NULL');

-- ------------------------------------------------------------------------------
-- 4. Bảng `periodicenergystatement` (Phiếu báo chỉ số điện năng & sản lượng định kỳ)
-- ------------------------------------------------------------------------------
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'Bt', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'Cd', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'Td', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'Sg', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'Vc', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'Pmax', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'PreviousBt', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'PreviousCd', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'PreviousTd', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'PreviousSg', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'PreviousVc', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'PreviousPmax', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'ConsumtionBt', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'ConsumtionCd', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'ConsumtionTd', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'ConsumtionSg', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'ConsumtionVc', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'Multiplier', 'DECIMAL(18, 4) NOT NULL DEFAULT 1.0000');
CALL `sp_phase3_ensure_column`('periodicenergystatement', 'CosPhi', 'DECIMAL(5, 2) DEFAULT NULL');

-- ------------------------------------------------------------------------------
-- 5. Bảng `meterreplacements` (Biên bản treo tháo & thay thế công tơ)
-- ------------------------------------------------------------------------------
CALL `sp_phase3_ensure_column`('meterreplacements', 'OldMeterMultiplier', 'DECIMAL(18, 4) NOT NULL DEFAULT 1.0000');
CALL `sp_phase3_ensure_column`('meterreplacements', 'OldBt', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('meterreplacements', 'OldCd', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('meterreplacements', 'OldTd', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('meterreplacements', 'OldSg', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('meterreplacements', 'OldVc', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('meterreplacements', 'OldPmax', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('meterreplacements', 'NewMeterMultiplier', 'DECIMAL(18, 4) NOT NULL DEFAULT 1.0000');
CALL `sp_phase3_ensure_column`('meterreplacements', 'NewBt', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('meterreplacements', 'NewCd', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('meterreplacements', 'NewTd', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('meterreplacements', 'NewSg', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('meterreplacements', 'NewVc', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');
CALL `sp_phase3_ensure_column`('meterreplacements', 'NewPmax', 'DECIMAL(18, 4) NOT NULL DEFAULT 0.0000');

DROP PROCEDURE IF EXISTS `sp_phase3_ensure_column`;
SET FOREIGN_KEY_CHECKS = 1;
