-- ====================================================================
-- Script tao bang Cau Hinh Email, Mau Thong Bao va Nghiep Vu Do Dem cho MDMSv2
-- Ngay tao: 31/10/2026
-- ====================================================================

-- 1. Tao bang SystemEmailConfigs
CREATE TABLE IF NOT EXISTS `SystemEmailConfigs` (
  `Id` bigint NOT NULL,
  `ProviderType` varchar(50) NOT NULL,
  `SenderEmail` varchar(255) NOT NULL,
  `SenderDisplayName` varchar(255) DEFAULT NULL,
  `TenantId` varchar(100) DEFAULT NULL,
  `ClientId` varchar(100) DEFAULT NULL,
  `EncryptedClientSecret` varchar(1000) DEFAULT NULL,
  `SmtpHost` varchar(255) DEFAULT NULL,
  `SmtpPort` int DEFAULT NULL,
  `SecurityMode` varchar(50) DEFAULT NULL,
  `RequireAuth` tinyint(1) NOT NULL DEFAULT '1',
  `SmtpUsername` varchar(255) DEFAULT NULL,
  `EncryptedSmtpPassword` varchar(1000) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedTime` datetime(6) NOT NULL,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 2. Tao bang NotificationTemplates
CREATE TABLE IF NOT EXISTS `NotificationTemplates` (
  `Id` bigint NOT NULL,
  `Code` varchar(100) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `SubjectTemplate` varchar(500) NOT NULL,
  `BodyHtmlTemplate` longtext NOT NULL,
  `Description` varchar(1000) DEFAULT NULL,
  `IsDefault` tinyint(1) NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedTime` datetime(6) NOT NULL,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_NotificationTemplates_Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 3. Tao bang StatementPdfTemplates
CREATE TABLE IF NOT EXISTS `StatementPdfTemplates` (
  `Id` bigint NOT NULL,
  `Code` varchar(100) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `HeaderTitle` varchar(255) DEFAULT NULL,
  `SubHeaderTitle` varchar(255) DEFAULT NULL,
  `CompanyName` varchar(255) DEFAULT NULL,
  `CompanyAddress` varchar(500) DEFAULT NULL,
  `ContactPhone` varchar(255) DEFAULT NULL,
  `LogoBase64` longtext,
  `PaperSize` varchar(50) DEFAULT NULL,
  `DisplayColumnsJson` varchar(1000) DEFAULT NULL,
  `FooterNotes` varchar(2000) DEFAULT NULL,
  `RequireDualSignatures` tinyint(1) NOT NULL DEFAULT '1',
  `IsDefault` tinyint(1) NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedTime` datetime(6) NOT NULL,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_StatementPdfTemplates_Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 4. Tao bang MeterReplacements
CREATE TABLE IF NOT EXISTS `MeterReplacements` (
  `Id` bigint NOT NULL,
  `MeasurementPointId` bigint NOT NULL,
  `ReplacementDate` datetime(6) NOT NULL,
  `Reason` varchar(500) DEFAULT NULL,
  `OldMeterSerial` varchar(100) NOT NULL,
  `OldMeterMultiplier` double NOT NULL DEFAULT '1',
  `OldBt` double NOT NULL DEFAULT '0',
  `OldCd` double NOT NULL DEFAULT '0',
  `OldTd` double NOT NULL DEFAULT '0',
  `OldSg` double NOT NULL DEFAULT '0',
  `OldVc` double NOT NULL DEFAULT '0',
  `OldPmax` double NOT NULL DEFAULT '0',
  `NewMeterSerial` varchar(100) NOT NULL,
  `NewMeterMultiplier` double NOT NULL DEFAULT '1',
  `NewBt` double NOT NULL DEFAULT '0',
  `NewCd` double NOT NULL DEFAULT '0',
  `NewTd` double NOT NULL DEFAULT '0',
  `NewSg` double NOT NULL DEFAULT '0',
  `NewVc` double NOT NULL DEFAULT '0',
  `NewPmax` double NOT NULL DEFAULT '0',
  `DocumentUrl` varchar(1000) DEFAULT NULL,
  `Status` varchar(50) NOT NULL DEFAULT 'APPROVED',
  `CreatedTime` datetime(6) NOT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `CreatedName` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MeterReplacements_MeasurementPointId` (`MeasurementPointId`),
  KEY `IX_MeterReplacements_ReplacementDate` (`ReplacementDate`),
  CONSTRAINT `FK_MeterReplacements_MeasurementPoints` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 5. Bo sung cac cot moi vao bang PeriodicEnergyStatement (kiem tra truoc khi add de tranh trung lap)
SET @dbname = DATABASE();
SET @tablename = 'periodicenergystatement';

-- PmaxTime
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'PmaxTime') > 0,
  'SELECT 1',
  'ALTER TABLE `periodicenergystatement` ADD COLUMN `PmaxTime` datetime(6) DEFAULT NULL'
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- PreviousPmaxTime
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'PreviousPmaxTime') > 0,
  'SELECT 1',
  'ALTER TABLE `periodicenergystatement` ADD COLUMN `PreviousPmaxTime` datetime(6) DEFAULT NULL'
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- BillingTimestamp
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'BillingTimestamp') > 0,
  'SELECT 1',
  'ALTER TABLE `periodicenergystatement` ADD COLUMN `BillingTimestamp` datetime(6) DEFAULT NULL'
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- HasMeterReplacement
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'HasMeterReplacement') > 0,
  'SELECT 1',
  'ALTER TABLE `periodicenergystatement` ADD COLUMN `HasMeterReplacement` tinyint(1) NOT NULL DEFAULT \'0\''
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- MeterReplacementId
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'MeterReplacementId') > 0,
  'SELECT 1',
  'ALTER TABLE `periodicenergystatement` ADD COLUMN `MeterReplacementId` bigint DEFAULT NULL'
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- StatementPdfTemplateId
SET @preparedStatement = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = 'StatementPdfTemplateId') > 0,
  'SELECT 1',
  'ALTER TABLE `periodicenergystatement` ADD COLUMN `StatementPdfTemplateId` bigint DEFAULT NULL'
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- 6. Insert mau khoi tao SystemEmailConfigs neu chua co
INSERT IGNORE INTO `SystemEmailConfigs` 
(`Id`, `ProviderType`, `SenderEmail`, `SenderDisplayName`, `TenantId`, `ClientId`, `EncryptedClientSecret`, `SmtpHost`, `SmtpPort`, `SecurityMode`, `RequireAuth`, `SmtpUsername`, `EncryptedSmtpPassword`, `IsActive`, `CreatedTime`)
VALUES
(1, 'MICROSOFT_GRAPH', 'noreply.hoadondientu.hp@vsip.com.vn', 'VSIP Hải Phòng - Phòng Cơ Điện', '5e5bd7f8-2adb-4595-956a-a03d68ea32a6', '6a86904e-3153-496f-8c1b-986ae398747a', '[AZURE_CLIENT_SECRET]', 'smtp.gmail.com', 587, 'StartTls', 1, '', '', 1, NOW());
