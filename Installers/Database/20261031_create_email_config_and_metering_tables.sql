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

-- 7. Insert mau khoi tao NotificationTemplates neu chua co
INSERT IGNORE INTO `NotificationTemplates` 
(`Id`, `Code`, `Name`, `SubjectTemplate`, `BodyHtmlTemplate`, `Description`, `IsDefault`, `IsActive`, `CreatedTime`)
VALUES
(1, 'STATEMENT_NOTIFICATION', 'Mẫu thông báo phiếu báo điện năng (Mặc định)', 
 '[VSIP Hải Phòng] Phiếu báo chỉ số & sản lượng điện kỳ {{CycleName}} - Điểm đo {{PointName}}', 
 '<div style=\"font-family: Arial, sans-serif; line-height: 1.6; color: #333;\"><h2 style=\"color: #003366;\">Kính gửi Quý khách hàng: {{CustomerName}}</h2><p>Phòng Cơ Điện - <b>{{CompanyName}}</b> xin trân trọng gửi tới Quý khách hàng thông tin chỉ số và sản lượng điện năng tiêu thụ kỳ <b>{{CycleName}}</b>.</p><table style=\"border-collapse: collapse; width: 100%; max-width: 600px; margin: 15px 0;\"><tr style=\"background-color: #f2f2f2;\"><td style=\"padding: 8px; border: 1px solid #ddd;\"><b>Điểm đo:</b></td><td style=\"padding: 8px; border: 1px solid #ddd;\">{{PointName}} ({{PointCode}})</td></tr><tr><td style=\"padding: 8px; border: 1px solid #ddd;\"><b>Mã khách hàng:</b></td><td style=\"padding: 8px; border: 1px solid #ddd;\">{{CustomerCode}}</td></tr><tr style=\"background-color: #f2f2f2;\"><td style=\"padding: 8px; border: 1px solid #ddd;\"><b>Số chế tạo công tơ:</b></td><td style=\"padding: 8px; border: 1px solid #ddd;\">{{MeterSerial}}</td></tr><tr><td style=\"padding: 8px; border: 1px solid #ddd;\"><b>Hệ số nhân (HSN):</b></td><td style=\"padding: 8px; border: 1px solid #ddd;\">{{Multiplier}}</td></tr><tr style=\"background-color: #f2f2f2;\"><td style=\"padding: 8px; border: 1px solid #ddd;\"><b>Tổng điện năng hữu công:</b></td><td style=\"padding: 8px; border: 1px solid #ddd; font-weight: bold; color: #d9534f;\">{{OutputSg}} kWh</td></tr><tr><td style=\"padding: 8px; border: 1px solid #ddd;\"><b>Điện năng vô công:</b></td><td style=\"padding: 8px; border: 1px solid #ddd;\">{{OutputVc}} kVARh</td></tr><tr style=\"background-color: #f2f2f2;\"><td style=\"padding: 8px; border: 1px solid #ddd;\"><b>Công suất Pmax:</b></td><td style=\"padding: 8px; border: 1px solid #ddd;\">{{Pmax}} kW</td></tr><tr><td style=\"padding: 8px; border: 1px solid #ddd;\"><b>Hệ số Cos φ:</b></td><td style=\"padding: 8px; border: 1px solid #ddd;\">{{CosPhi}}</td></tr></table><p>Chi tiết thông số chỉ số đầu kỳ, cuối kỳ và sản lượng cụ thể được đính kèm trong file PDF <b>Phiếu Báo Chỉ Số Điện Năng</b> kèm theo email này.</p><p>Trân trọng cảm ơn sự hợp tác của Quý khách hàng!</p><hr style=\"border: none; border-top: 1px solid #eee; margin: 20px 0;\"/><p style=\"font-size: 12px; color: #777;\"><i>Email này được gửi tự động từ Hệ thống Quản trị Đo đếm Năng lượng MDMSv2. Vui lòng không phản hồi trực tiếp vào địa chỉ này.</i></p></div>', 
 'Mẫu email thông báo sản lượng điện hàng tháng gửi khách hàng đính kèm file PDF', 1, 1, NOW()),

(2, 'FORGOT_PASSWORD', 'Mẫu email lấy lại mật khẩu (Mặc định)', 
 '[MDMSv2] Yêu cầu cấp lại mật khẩu truy cập hệ thống', 
 '<div style=\"font-family: Arial, sans-serif; line-height: 1.6; color: #333;\"><h2 style=\"color: #003366;\">Xin chào {{FullName}},</h2><p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản <b>{{Username}}</b> của bạn trên Hệ thống MDMSv2.</p><p>Mã xác thực (OTP) hoặc mật khẩu tạm thời mới của bạn là:</p><div style=\"font-size: 24px; font-weight: bold; color: #d9534f; background: #f9f9f9; padding: 12px; border-radius: 4px; display: inline-block; letter-spacing: 2px;\">{{Password}}</div><p>Vui lòng đăng nhập và đổi lại mật khẩu ngay sau khi truy cập để đảm bảo tính an toàn bảo mật thông tin.</p><hr style=\"border: none; border-top: 1px solid #eee; margin: 20px 0;\"/><p style=\"font-size: 12px; color: #777;\"><i>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email hoặc liên hệ với Quản trị viên hệ thống.</i></p></div>', 
 'Mẫu email gửi mã xác nhận hoặc mật khẩu tạm thời khi quên mật khẩu', 1, 1, NOW()),

(3, 'USER_CREDENTIAL', 'Mẫu email cấp thông tin tài khoản mới (Mặc định)', 
 '[MDMSv2] Thông tin kích hoạt tài khoản hệ thống', 
 '<div style=\"font-family: Arial, sans-serif; line-height: 1.6; color: #333;\"><h2 style=\"color: #003366;\">Kính chào {{FullName}},</h2><p>Tài khoản truy cập Hệ thống Quản trị Đo đếm Năng lượng MDMSv2 của bạn đã được khởi tạo thành công với thông tin như sau:</p><ul><li><b>Tên đăng nhập:</b> {{Username}}</li><li><b>Mật khẩu đăng nhập:</b> {{Password}}</li></ul><p>Vui lòng truy cập đường dẫn hệ thống để đăng nhập và thay đổi mật khẩu lần đầu.</p><hr style=\"border: none; border-top: 1px solid #eee; margin: 20px 0;\"/><p style=\"font-size: 12px; color: #777;\"><i>Email tự động từ hệ thống MDMSv2. Trân trọng cảm ơn!</i></p></div>', 
 'Mẫu email gửi thông tin tên đăng nhập và mật khẩu khi tài khoản được cấp mới', 1, 1, NOW());

-- 8. Insert mau khoi tao StatementPdfTemplates neu chua co
INSERT IGNORE INTO `StatementPdfTemplates` 
(`Id`, `Code`, `Name`, `HeaderTitle`, `SubHeaderTitle`, `CompanyName`, `CompanyAddress`, `ContactPhone`, `PaperSize`, `DisplayColumnsJson`, `FooterNotes`, `RequireDualSignatures`, `IsDefault`, `IsActive`, `CreatedTime`)
VALUES
(1, 'A5_VSIP_STANDARD', 'Mẫu phiếu báo A5 Ngang - Chuẩn VSIP Hải Phòng', 
 'PHIẾU BÁO CHỈ SỐ ĐIỆN NĂNG', 'DÀNH CHO KHÁCH HÀNG SỬ DỤNG ĐIỆN', 
 'CÔNG TY TNHH VSIP HẢI PHÒNG', 'Khu Đô thị, Công nghiệp và Dịch vụ VSIP Hải Phòng, Thủy Nguyên, Hải Phòng', '(0225) 3959 888', 
 'A5_LANDSCAPE', '[\"BT\",\"CD\",\"TD\",\"SG\",\"VC\",\"PMAX\",\"COSPHI\"]', 
 'Ghi chú: Phiếu báo chỉ số điện năng này là cơ sở để phát hành hóa đơn tiền điện hàng tháng. Mọi thắc mắc xin vui lòng liên hệ Phòng Cơ Điện trong vòng 24 giờ kể từ khi nhận được phiếu báo.', 
 1, 1, 1, NOW()),

(2, 'A4_PORTRAIT_STANDARD', 'Mẫu phiếu báo A4 Đứng - Chuẩn Kế toán & Lưu trữ', 
 'PHIẾU XÁC NHẬN CHỈ SỐ ĐO ĐẾM ĐIỆN NĂNG', 'KỲ GIAO NHẬN ĐIỆN THÁNG', 
 'CÔNG TY TNHH VSIP HẢI PHÒNG', 'Khu Đô thị, Công nghiệp và Dịch vụ VSIP Hải Phòng, Thủy Nguyên, Hải Phòng', '(0225) 3959 888', 
 'A4_PORTRAIT', '[\"BT\",\"CD\",\"TD\",\"SG\",\"VC\",\"PMAX\",\"COSPHI\"]', 
 'Ghi chú: Phiếu báo có giá trị xác nhận chỉ số đo đếm thực tế giữa bên giao điện và bên nhận điện.', 
 1, 0, 1, NOW());

