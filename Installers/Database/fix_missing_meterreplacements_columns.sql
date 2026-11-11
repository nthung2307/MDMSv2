-- ===================================================================================
-- MDMSv2: Bổ sung và đồng bộ toàn diện các cột cho bảng meterreplacements
-- Hỗ trợ thực thi an toàn trên mọi phiên bản CSDL hiện hữu (không phụ thuộc thứ tự cột)
-- Thực thi trên database 'amr' qua MySQL Workbench hoặc PowerShell:
--   mysql -u root -p amr < fix_missing_meterreplacements_columns.sql
-- ===================================================================================

USE `amr`;

SET @dbname = DATABASE();

-- 1. Reason
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'Reason') > 0, 'SELECT 1', 'ALTER TABLE `meterreplacements` ADD COLUMN `Reason` varchar(500) DEFAULT NULL'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 2. OldMeterMultiplier
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'OldMeterMultiplier') > 0, 'ALTER TABLE `meterreplacements` MODIFY COLUMN `OldMeterMultiplier` decimal(18,4) NOT NULL DEFAULT 1.0000', 'ALTER TABLE `meterreplacements` ADD COLUMN `OldMeterMultiplier` decimal(18,4) NOT NULL DEFAULT 1.0000'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 3. OldBt
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'OldBt') > 0, 'ALTER TABLE `meterreplacements` MODIFY COLUMN `OldBt` decimal(18,4) NOT NULL DEFAULT 0.0000', 'ALTER TABLE `meterreplacements` ADD COLUMN `OldBt` decimal(18,4) NOT NULL DEFAULT 0.0000'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 4. OldCd
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'OldCd') > 0, 'ALTER TABLE `meterreplacements` MODIFY COLUMN `OldCd` decimal(18,4) NOT NULL DEFAULT 0.0000', 'ALTER TABLE `meterreplacements` ADD COLUMN `OldCd` decimal(18,4) NOT NULL DEFAULT 0.0000'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 5. OldTd
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'OldTd') > 0, 'ALTER TABLE `meterreplacements` MODIFY COLUMN `OldTd` decimal(18,4) NOT NULL DEFAULT 0.0000', 'ALTER TABLE `meterreplacements` ADD COLUMN `OldTd` decimal(18,4) NOT NULL DEFAULT 0.0000'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 6. OldSg
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'OldSg') > 0, 'ALTER TABLE `meterreplacements` MODIFY COLUMN `OldSg` decimal(18,4) NOT NULL DEFAULT 0.0000', 'ALTER TABLE `meterreplacements` ADD COLUMN `OldSg` decimal(18,4) NOT NULL DEFAULT 0.0000'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 7. OldVc
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'OldVc') > 0, 'ALTER TABLE `meterreplacements` MODIFY COLUMN `OldVc` decimal(18,4) NOT NULL DEFAULT 0.0000', 'ALTER TABLE `meterreplacements` ADD COLUMN `OldVc` decimal(18,4) NOT NULL DEFAULT 0.0000'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 8. OldPmax
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'OldPmax') > 0, 'ALTER TABLE `meterreplacements` MODIFY COLUMN `OldPmax` decimal(18,4) NOT NULL DEFAULT 0.0000', 'ALTER TABLE `meterreplacements` ADD COLUMN `OldPmax` decimal(18,4) NOT NULL DEFAULT 0.0000'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 9. NewMeterMultiplier
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'NewMeterMultiplier') > 0, 'ALTER TABLE `meterreplacements` MODIFY COLUMN `NewMeterMultiplier` decimal(18,4) NOT NULL DEFAULT 1.0000', 'ALTER TABLE `meterreplacements` ADD COLUMN `NewMeterMultiplier` decimal(18,4) NOT NULL DEFAULT 1.0000'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 10. NewBt
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'NewBt') > 0, 'ALTER TABLE `meterreplacements` MODIFY COLUMN `NewBt` decimal(18,4) NOT NULL DEFAULT 0.0000', 'ALTER TABLE `meterreplacements` ADD COLUMN `NewBt` decimal(18,4) NOT NULL DEFAULT 0.0000'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 11. NewCd
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'NewCd') > 0, 'ALTER TABLE `meterreplacements` MODIFY COLUMN `NewCd` decimal(18,4) NOT NULL DEFAULT 0.0000', 'ALTER TABLE `meterreplacements` ADD COLUMN `NewCd` decimal(18,4) NOT NULL DEFAULT 0.0000'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 12. NewTd
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'NewTd') > 0, 'ALTER TABLE `meterreplacements` MODIFY COLUMN `NewTd` decimal(18,4) NOT NULL DEFAULT 0.0000', 'ALTER TABLE `meterreplacements` ADD COLUMN `NewTd` decimal(18,4) NOT NULL DEFAULT 0.0000'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 13. NewSg
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'NewSg') > 0, 'ALTER TABLE `meterreplacements` MODIFY COLUMN `NewSg` decimal(18,4) NOT NULL DEFAULT 0.0000', 'ALTER TABLE `meterreplacements` ADD COLUMN `NewSg` decimal(18,4) NOT NULL DEFAULT 0.0000'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 14. NewVc
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'NewVc') > 0, 'ALTER TABLE `meterreplacements` MODIFY COLUMN `NewVc` decimal(18,4) NOT NULL DEFAULT 0.0000', 'ALTER TABLE `meterreplacements` ADD COLUMN `NewVc` decimal(18,4) NOT NULL DEFAULT 0.0000'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 15. NewPmax
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'NewPmax') > 0, 'ALTER TABLE `meterreplacements` MODIFY COLUMN `NewPmax` decimal(18,4) NOT NULL DEFAULT 0.0000', 'ALTER TABLE `meterreplacements` ADD COLUMN `NewPmax` decimal(18,4) NOT NULL DEFAULT 0.0000'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 16. DocumentUrl
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'DocumentUrl') > 0, 'SELECT 1', 'ALTER TABLE `meterreplacements` ADD COLUMN `DocumentUrl` varchar(1000) DEFAULT NULL'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 17. Status
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'Status') > 0, 'SELECT 1', 'ALTER TABLE `meterreplacements` ADD COLUMN `Status` varchar(50) NOT NULL DEFAULT \'APPROVED\''));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 18. CreatedBy
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'CreatedBy') > 0, 'SELECT 1', 'ALTER TABLE `meterreplacements` ADD COLUMN `CreatedBy` bigint DEFAULT NULL'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 19. CreatedName
SET @sql = (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND LOWER(TABLE_NAME) = 'meterreplacements' AND COLUMN_NAME = 'CreatedName') > 0, 'SELECT 1', 'ALTER TABLE `meterreplacements` ADD COLUMN `CreatedName` varchar(255) DEFAULT NULL'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
