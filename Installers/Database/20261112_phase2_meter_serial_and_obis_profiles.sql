-- ==============================================================================
-- MDMSv2 Phase 2 Migration: MeterSerial Identification, 2-Tier ApiKey,
-- Standard MeterProfiles (MV_KV_KW, LV_V_KW), 60-Tag OBIS Auto-Mapping,
-- and Kepware Legacy Tag Routing Flag
-- Date: 2026-11-12
-- ==============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

CREATE TABLE IF NOT EXISTS `MeterProfiles` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `Code` VARCHAR(64) NOT NULL,
    `Name` VARCHAR(200) NOT NULL,
    `Description` VARCHAR(500) NULL,
    `VoltageLevel` VARCHAR(32) NOT NULL DEFAULT 'LV',
    `DefaultVoltageUnit` VARCHAR(16) NOT NULL DEFAULT 'V',
    `DefaultPowerUnit` VARCHAR(16) NOT NULL DEFAULT 'kW',
    `IsDefault` TINYINT(1) NOT NULL DEFAULT 0,
    `IsSystem` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedTime` DATETIME(6) NULL,
    `CreatedBy` BIGINT NULL,
    `CreatedName` LONGTEXT NULL,
    `ModifiedTime` DATETIME(6) NULL,
    `ModifiedBy` BIGINT NULL,
    `ModifiedName` LONGTEXT NULL,
    `Deleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedTime` DATETIME(6) NULL,
    `DeletedBy` BIGINT NULL,
    `DeletedName` LONGTEXT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_MeterProfiles_Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `MeterProfileTagMappings` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `MeterProfileId` BIGINT NOT NULL,
    `ObisCode` VARCHAR(64) NOT NULL,
    `ParameterCode` VARCHAR(128) NOT NULL,
    `Unit` VARCHAR(32) NULL,
    `CustomName` VARCHAR(255) NULL,
    `LegacyAliases` VARCHAR(1000) NULL,
    `DataCategory` VARCHAR(64) NOT NULL DEFAULT 'ELECTRICAL',
    `DisplayOrder` INT NOT NULL DEFAULT 0,
    `CreatedTime` DATETIME(6) NULL,
    `CreatedBy` BIGINT NULL,
    `CreatedName` LONGTEXT NULL,
    `ModifiedTime` DATETIME(6) NULL,
    `ModifiedBy` BIGINT NULL,
    `ModifiedName` LONGTEXT NULL,
    `Deleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedTime` DATETIME(6) NULL,
    `DeletedBy` BIGINT NULL,
    `DeletedName` LONGTEXT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_MeterProfileTagMappings_Profile_Obis` (`MeterProfileId`, `ObisCode`),
    INDEX `IX_MeterProfileTagMappings_Profile_Param` (`MeterProfileId`, `ParameterCode`),
    CONSTRAINT `FK_MeterProfileTagMappings_MeterProfiles`
        FOREIGN KEY (`MeterProfileId`) REFERENCES `MeterProfiles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP PROCEDURE IF EXISTS `sp_phase2_add_mp_column`;
DROP PROCEDURE IF EXISTS `sp_phase2_seed_profile_mappings`;
DROP PROCEDURE IF EXISTS `sp_phase2_migrate`;

DELIMITER $$

CREATE PROCEDURE `sp_phase2_add_mp_column`(
    IN p_col VARCHAR(64),
    IN p_def VARCHAR(255)
)
BEGIN
    DECLARE v_table VARCHAR(64) DEFAULT 'MeasurementPoints';
    SELECT TABLE_NAME INTO v_table
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = DATABASE() AND LOWER(TABLE_NAME) = 'measurementpoints'
    LIMIT 1;

    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND LOWER(TABLE_NAME) = 'measurementpoints'
          AND COLUMN_NAME = p_col
    ) THEN
        SET @ddl = CONCAT('ALTER TABLE `', v_table, '` ADD COLUMN `', p_col, '` ', p_def);
        PREPARE stmt FROM @ddl;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

CREATE PROCEDURE `sp_phase2_seed_profile_mappings`(
    IN p_profile_id BIGINT,
    IN p_voltage_unit VARCHAR(16)
)
BEGIN
    INSERT INTO `MeterProfileTagMappings`
        (`MeterProfileId`, `ObisCode`, `ParameterCode`, `Unit`, `CustomName`, `LegacyAliases`, `DataCategory`, `DisplayOrder`, `CreatedTime`, `Deleted`)
    SELECT p_profile_id, v.ObisCode, v.ParameterCode, v.Unit, v.CustomName, v.LegacyAliases, v.DataCategory, v.DisplayOrder, UTC_TIMESTAMP(6), 0
    FROM (
        SELECT '31_7_0' AS ObisCode, 'DONG_DIEN_PHA_A' AS ParameterCode, 'A' AS Unit, NULL AS CustomName, 'Meas.Ia,Ia' AS LegacyAliases, 'ELECTRICAL' AS DataCategory, 1 AS DisplayOrder
        UNION ALL SELECT '51_7_0', 'DONG_DIEN_PHA_B', 'A', NULL, 'Meas.Ib,Ib', 'ELECTRICAL', 2
        UNION ALL SELECT '71_7_0', 'DONG_DIEN_PHA_C', 'A', NULL, 'Meas.Ic,Ic', 'ELECTRICAL', 3
        UNION ALL SELECT '32_7_0', 'DIEN_AP_PHA_A', p_voltage_unit, NULL, 'Meas.Ua,Meas.Va,Ua,Va', 'ELECTRICAL', 4
        UNION ALL SELECT '52_7_0', 'DIEN_AP_PHA_B', p_voltage_unit, NULL, 'Meas.Ub,Meas.Vb,Ub,Vb', 'ELECTRICAL', 5
        UNION ALL SELECT '72_7_0', 'DIEN_AP_PHA_C', p_voltage_unit, NULL, 'Meas.Uc,Meas.Vc,Uc,Vc', 'ELECTRICAL', 6
        UNION ALL SELECT '21_7_0', 'CONG_SUAT_TAC_DUNG_PHA_A', 'kW', NULL, 'Meas.Pa,Pa', 'ELECTRICAL', 7
        UNION ALL SELECT '41_7_0', 'CONG_SUAT_TAC_DUNG_PHA_B', 'kW', NULL, 'Meas.Pb,Pb', 'ELECTRICAL', 8
        UNION ALL SELECT '61_7_0', 'CONG_SUAT_TAC_DUNG_PHA_C', 'kW', NULL, 'Meas.Pc,Pc', 'ELECTRICAL', 9
        UNION ALL SELECT '1_7_0',  'CONG_SUAT_TAC_DUNG_TONG_3_PHA', 'kW', NULL, 'Meas.Pt,Meas.Ptotal,15_7_0,16_7_0', 'ELECTRICAL', 10
        UNION ALL SELECT '23_7_0', 'CONG_SUAT_PHAN_KHANG_PHA_A', 'kVAr', NULL, 'Meas.Qa,Qa', 'ELECTRICAL', 11
        UNION ALL SELECT '43_7_0', 'CONG_SUAT_PHAN_KHANG_PHA_B', 'kVAr', NULL, 'Meas.Qb,Qb', 'ELECTRICAL', 12
        UNION ALL SELECT '63_7_0', 'CONG_SUAT_PHAN_KHANG_PHA_C', 'kVAr', NULL, 'Meas.Qc,Qc', 'ELECTRICAL', 13
        UNION ALL SELECT '3_7_0',  'CONG_SUAT_PHAN_KHANG_TONG_3_PHA', 'kVAr', NULL, 'Meas.Qt,Meas.Qtotal', 'ELECTRICAL', 14
        UNION ALL SELECT '33_7_0', 'HE_SO_CONG_SUAT_PF_PHA_A', NULL, NULL, 'Meas.PFa,Meas.CosPhiA', 'ELECTRICAL', 15
        UNION ALL SELECT '53_7_0', 'HE_SO_CONG_SUAT_PF_PHA_B', NULL, NULL, 'Meas.PFb,Meas.CosPhiB', 'ELECTRICAL', 16
        UNION ALL SELECT '73_7_0', 'HE_SO_CONG_SUAT_PF_PHA_C', NULL, NULL, 'Meas.PFc,Meas.CosPhiC', 'ELECTRICAL', 17
        UNION ALL SELECT '13_7_0', 'HE_SO_CONG_SUAT_PF_TRUNG_BINH_3_PHA', NULL, NULL, 'Meas.PF,Meas.PFt,Meas.PFavg', 'ELECTRICAL', 18
        UNION ALL SELECT '14_7_0', 'TAN_SO_HE_THONG', 'Hz', NULL, 'Meas.Hz,Meas.Freq', 'ELECTRICAL', 19
        UNION ALL SELECT '1_8_0',  'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG', 'kWh', NULL, 'Energy.PTRU,TOU.PTOT,Energy.PTOT', 'ENERGY', 20
        UNION ALL SELECT '1_8_1',  'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT', 'kWh', NULL, 'Energy.PTR1,TOU.PTR1', 'ENERGY', 21
        UNION ALL SELECT '1_8_2',  'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD', 'kWh', NULL, 'Energy.PTR2,TOU.PTR2', 'ENERGY', 22
        UNION ALL SELECT '1_8_3',  'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD', 'kWh', NULL, 'Energy.PTR3,TOU.PTR3', 'ENERGY', 23
        UNION ALL SELECT '3_8_0',  'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG', 'kVArh', NULL, 'Energy.QTRU,TOU.QTOT,5_8_0', 'ENERGY', 24
        UNION ALL SELECT '2_8_0',  'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_TONG_CHIEU_NHAN', 'kWh', NULL, 'Energy.NTRU,TOU.PTOT_N,Energy.PImTOT', 'ENERGY', 25
        UNION ALL SELECT '2_8_1',  'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_BT_CHIEU_NHAN', 'kWh', NULL, 'Energy.NTR1,TOU.PTR1_N', 'ENERGY', 26
        UNION ALL SELECT '2_8_2',  'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_CD_CHIEU_NHAN', 'kWh', NULL, 'Energy.NTR2,TOU.PTR2_N', 'ENERGY', 27
        UNION ALL SELECT '2_8_3',  'CHI_SO_TICH_LUY_DIEN_NANG_TAC_DUNG_AP_BIEU_GIO_TD_CHIEU_NHAN', 'kWh', NULL, 'Energy.NTR3,TOU.PTR3_N', 'ENERGY', 28
        UNION ALL SELECT '4_8_0',  'CHI_SO_TICH_LUY_DIEN_NANG_PHAN_KHANG_AQ_TONG_CHIEU_NHAN', 'kVArh', NULL, 'Energy.QImTOT,TOU.QTOT_N,8_8_0', 'ENERGY', 29
        UNION ALL SELECT '1_8_0_1', 'CHI_SO_CHOT_DIEN_NANG_AP_TONG', 'kWh', NULL, 'Energy.PBRU,Billing.PTOT,TOU.PBTOT', 'ENERGY', 30
        UNION ALL SELECT '1_8_1_1', 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT', 'kWh', NULL, 'Energy.PBR1,TOU.PBR1,Billing.PBR1', 'ENERGY', 31
        UNION ALL SELECT '1_8_2_1', 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD', 'kWh', NULL, 'Energy.PBR2,TOU.PBR2,Billing.PBR2', 'ENERGY', 32
        UNION ALL SELECT '1_8_3_1', 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD', 'kWh', NULL, 'Energy.PBR3,TOU.PBR3,Billing.PBR3', 'ENERGY', 33
        UNION ALL SELECT '3_8_0_1', 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG', 'kVArh', NULL, 'Energy.QBRU,Billing.QTOT,TOU.QBTOT', 'ENERGY', 34
        UNION ALL SELECT '2_8_0_1', 'CHI_SO_CHOT_DIEN_NANG_AP_TONG_CHIEU_NHAN', 'kWh', NULL, 'Energy.NBRU,Billing.PTOT_N', 'ENERGY', 35
        UNION ALL SELECT '2_8_1_1', 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT_CHIEU_NHAN', 'kWh', NULL, 'Energy.NBR1,Billing.PBR1_N', 'ENERGY', 36
        UNION ALL SELECT '2_8_2_1', 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD_CHIEU_NHAN', 'kWh', NULL, 'Energy.NBR2,Billing.PBR2_N', 'ENERGY', 37
        UNION ALL SELECT '2_8_3_1', 'CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD_CHIEU_NHAN', 'kWh', NULL, 'Energy.NBR3,Billing.PBR3_N', 'ENERGY', 38
        UNION ALL SELECT '4_8_0_1', 'CHI_SO_CHOT_DIEN_NANG_AQ_TONG_CHIEU_NHAN', 'kVArh', NULL, 'Billing.QTOT_N', 'ENERGY', 39
        UNION ALL SELECT '1_6_0',   'PMAX_KY_HIEN_TAI', 'kW', NULL, 'MD.Current,MaxDemand.MDCU', 'ENERGY', 40
        UNION ALL SELECT '1_6_0_1', 'PMAX_KY_TRUOC', 'kW', NULL, 'MD.Previous,MaxDemand.MDPR', 'ENERGY', 41
        UNION ALL SELECT '0_9_1', 'TIME_METER_CLOCK', NULL, NULL, '', 'TIME', 42
        UNION ALL SELECT '0_9_2', 'DATE_METER_CLOCK', NULL, NULL, '', 'TIME', 43
        UNION ALL SELECT '0_9_3', 'DATE_CLOSING_LAST', NULL, NULL, '0_1_2_D', 'TIME', 44
        UNION ALL SELECT '0_9_4', 'TIME_CLOSING_LAST', NULL, NULL, '0_1_2_T', 'TIME', 45
        UNION ALL SELECT '0_9_5', 'DATE_PMAX_CURRENT', NULL, NULL, '1_6_0_D', 'TIME', 46
        UNION ALL SELECT '0_9_6', 'TIME_PMAX_CURRENT', NULL, NULL, '1_6_0_T', 'TIME', 47
        UNION ALL SELECT '0_9_7', 'DATE_PMAX_PREVIOUS', NULL, NULL, '1_6_0_1_D', 'TIME', 48
        UNION ALL SELECT '0_9_8', 'TIME_PMAX_PREVIOUS', NULL, NULL, '1_6_0_1_T', 'TIME', 49
        UNION ALL SELECT '96_5_0',  'MAT_KET_NOI', NULL, 'Mất kết nối công tơ', 'Status.Error,Disconnected,_System._Error', 'ALERT', 50
        UNION ALL SELECT '97_97_1', 'CANH_BAO_01', NULL, 'Cảnh báo 01', 'Status.Efa01,Efa.Efa01', 'ALERT', 51
        UNION ALL SELECT '97_97_2', 'CANH_BAO_02', NULL, 'Cảnh báo 02', 'Status.Efa02,Efa.Efa02', 'ALERT', 52
        UNION ALL SELECT '97_97_3', 'CANH_BAO_03', NULL, 'Cảnh báo 03', 'Status.Efa03,Efa.Efa03', 'ALERT', 53
        UNION ALL SELECT '97_97_4', 'CANH_BAO_04', NULL, 'Cảnh báo 04', 'Status.Efa04,Efa.Efa04', 'ALERT', 54
        UNION ALL SELECT '97_97_5', 'CANH_BAO_05', NULL, 'Cảnh báo 05', 'Status.Efa05,Efa.Efa05', 'ALERT', 55
        UNION ALL SELECT '97_97_6', 'CANH_BAO_06', NULL, 'Cảnh báo 06', 'Status.Efa06,Efa.Efa06', 'ALERT', 56
        UNION ALL SELECT '97_97_7', 'CANH_BAO_07', NULL, 'Cảnh báo 07', 'Status.Efa07,Efa.Efa07', 'ALERT', 57
        UNION ALL SELECT '97_97_8', 'CANH_BAO_08', NULL, 'Cảnh báo 08', 'Status.Efa08,Efa.Efa08', 'ALERT', 58
        UNION ALL SELECT '97_97_9', 'CANH_BAO_09', NULL, 'Cảnh báo 09', 'Status.Efa09,Efa.Efa09', 'ALERT', 59
        UNION ALL SELECT '97_97_10','CANH_BAO_10', NULL, 'Cảnh báo 10', 'Status.Efa10,Efa.Efa10', 'ALERT', 60
    ) AS v
    WHERE NOT EXISTS (
        SELECT 1 FROM `MeterProfileTagMappings` m
        WHERE m.MeterProfileId = p_profile_id AND m.ObisCode = v.ObisCode AND m.Deleted = 0
    );
END$$

CREATE PROCEDURE `sp_phase2_migrate`()
BEGIN
    DECLARE v_mp_table VARCHAR(64) DEFAULT 'MeasurementPoints';
    DECLARE v_sys_table VARCHAR(64) DEFAULT 'SystemConfig';
    DECLARE v_mv_id BIGINT DEFAULT NULL;
    DECLARE v_lv_id BIGINT DEFAULT NULL;

    SELECT TABLE_NAME INTO v_mp_table
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = DATABASE() AND LOWER(TABLE_NAME) = 'measurementpoints'
    LIMIT 1;

    SELECT TABLE_NAME INTO v_sys_table
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = DATABASE() AND LOWER(TABLE_NAME) = 'systemconfig'
    LIMIT 1;

    CALL `sp_phase2_add_mp_column`('MeterSerial', 'VARCHAR(100) NULL');
    CALL `sp_phase2_add_mp_column`('ApiKeyHash', 'VARCHAR(128) NULL');
    CALL `sp_phase2_add_mp_column`('ApiKeyEncrypted', 'VARCHAR(512) NULL');
    CALL `sp_phase2_add_mp_column`('KeyPrefix', 'VARCHAR(32) NULL');
    CALL `sp_phase2_add_mp_column`('MeterProfileId', 'BIGINT NULL');
    CALL `sp_phase2_add_mp_column`('MappingMode', 'VARCHAR(20) NOT NULL DEFAULT ''HYBRID''');

    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND LOWER(TABLE_NAME) = 'measurementpoints'
          AND INDEX_NAME = 'IX_MeasurementPoints_MeterSerial'
    ) THEN
        SET @idx_sql = CONCAT('CREATE INDEX `IX_MeasurementPoints_MeterSerial` ON `', v_mp_table, '` (`MeterSerial`)');
        PREPARE stmt FROM @idx_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
        WHERE TABLE_SCHEMA = DATABASE()
          AND LOWER(TABLE_NAME) = 'measurementpoints'
          AND CONSTRAINT_NAME = 'FK_MeasurementPoints_MeterProfiles'
    ) THEN
        SET @fk_sql = CONCAT(
            'ALTER TABLE `', v_mp_table, '` ADD CONSTRAINT `FK_MeasurementPoints_MeterProfiles` ',
            'FOREIGN KEY (`MeterProfileId`) REFERENCES `MeterProfiles` (`Id`) ON DELETE SET NULL'
        );
        PREPARE stmt FROM @fk_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;

    -- Seed Standard Profile 1: Medium Voltage (kV / kW)
    INSERT IGNORE INTO `MeterProfiles`
        (`Code`, `Name`, `Description`, `VoltageLevel`, `DefaultVoltageUnit`, `DefaultPowerUnit`, `IsDefault`, `IsSystem`, `CreatedTime`, `Deleted`)
    VALUES
        ('MV_KV_KW', 'Công tơ Trung thế chuẩn (kV / kW)', 'Hồ sơ ánh xạ OBIS IEC 62056-61 mặc định cho điểm đo Trung thế (U: kV, P: kW)', 'MV', 'kV', 'kW', 0, 1, UTC_TIMESTAMP(6), 0),
        ('LV_V_KW', 'Công tơ Hạ thế chuẩn (V / kW)', 'Hồ sơ ánh xạ OBIS IEC 62056-61 mặc định cho điểm đo Hạ thế (U: V, P: kW)', 'LV', 'V', 'kW', 1, 1, UTC_TIMESTAMP(6), 0);

    SELECT `Id` INTO v_mv_id FROM `MeterProfiles` WHERE `Code` = 'MV_KV_KW' LIMIT 1;
    SELECT `Id` INTO v_lv_id FROM `MeterProfiles` WHERE `Code` = 'LV_V_KW' LIMIT 1;

    IF v_mv_id IS NOT NULL THEN
        CALL `sp_phase2_seed_profile_mappings`(v_mv_id, 'kV');
    END IF;
    IF v_lv_id IS NOT NULL THEN
        CALL `sp_phase2_seed_profile_mappings`(v_lv_id, 'V');
    END IF;

    -- Ensure IOT_ALLOW_LEGACY_TAG_ROUTING config exists and defaults to 'true' for zero-downtime upgrade
    IF v_sys_table IS NOT NULL THEN
        SET @cfg_sql = CONCAT(
            'INSERT INTO `', v_sys_table, '` (`Key`, `Value`) ',
            'SELECT ''IOT_ALLOW_LEGACY_TAG_ROUTING'', ''true'' ',
            'WHERE NOT EXISTS (SELECT 1 FROM `', v_sys_table, '` WHERE `Key` = ''IOT_ALLOW_LEGACY_TAG_ROUTING'')'
        );
        PREPARE stmt FROM @cfg_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

DELIMITER ;

CALL `sp_phase2_migrate`();

DROP PROCEDURE IF EXISTS `sp_phase2_add_mp_column`;
DROP PROCEDURE IF EXISTS `sp_phase2_seed_profile_mappings`;
DROP PROCEDURE IF EXISTS `sp_phase2_migrate`;

SET FOREIGN_KEY_CHECKS = 1;
