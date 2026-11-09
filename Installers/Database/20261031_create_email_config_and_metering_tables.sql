-- ====================================================================
-- Script tao bang Cau Hinh Email, Mau Thong Bao va Nghiep Vu Do Dem cho MDMSv2
-- Ngay tao: 31/10/2026
-- ====================================================================

SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;

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
(1, 'STATEMENT_NOTIFICATION', 'Phiếu chỉ số', 
 'THÔNG BÁO CHỈ SỐ ĐIỆN HÀNG THÁNG', 
 '<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Thông Báo Chỉ Số & Sản Lượng Điện Năng</title></head><body style=\"margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,\'Segoe UI\',Roboto,Helvetica,Arial,sans-serif;color:#334155;\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"background-color:#f5f7fc;padding:24px 10px;width:100%;max-width:100%;\"><tr><td align=\"center\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"max-width:640px;width:100%;background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;\"><tr><td style=\"background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:24px 30px;text-align:center;\"><h2 style=\"margin:0;color:#ffffff;font-size:20px;font-weight:700;\">{{CompanyName}}</h2><p style=\"margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;\">HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM (MDMS)</p><p style=\"margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;\">THÔNG BÁO CHỈ SỐ & SẢN LƯỢNG ĐIỆN NĂNG</p></td></tr><tr><td style=\"padding:24px 30px 10px 30px;\"><p style=\"margin:0 0 14px 0;font-size:15px;color:#0f172a;\">Kính gửi Quý khách hàng: <strong>{{CustomerName}}</strong>,</p><p style=\"margin:0 0 16px 0;font-size:13.5px;line-height:1.6;color:#475569;\">Chúng tôi xin trân trọng gửi tới Quý khách hàng bảng kê chỉ số công tơ và sản lượng điện năng tiêu thụ trong kỳ ghi chỉ số: {{Period}}.</p><table width=\"100%\" cellpadding=\"10\" cellspacing=\"0\" border=\"0\" style=\"background-color:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;margin-bottom:18px;font-size:13.5px;\"><tr><td width=\"22%\" style=\"color:#64748b;font-weight:500;\">Tên điểm đo:</td><td style=\"font-weight:700;color:#0f172a;\">{{PointName}}</td></tr></table></td></tr><tr><td style=\"padding:0 30px 15px 30px;\"><table width=\"100%\" cellpadding=\"8\" cellspacing=\"0\" border=\"1\" style=\"border-collapse:collapse;border-color:#e2e8f0;font-size:13px;\"><tr style=\"background-color:#f1f5f9;color:#0f172a;font-weight:600;text-align:center;\"><td width=\"32%\">Biểu giá</td><td width=\"22%\">Chỉ số cũ</td><td width=\"22%\">Chỉ số mới</td><td width=\"24%\">Sản lượng (kWh)</td></tr><tr><td style=\"padding-left:12px;\">Bình thường (BT)</td><td align=\"right\" style=\"color:#475569;\">{{PreviousBt}}</td><td align=\"right\" style=\"color:#475569;\">{{Bt}}</td><td align=\"right\" style=\"font-weight:700;color:#0f172a;\">{{ConsumtionBt}}</td></tr><tr><td style=\"padding-left:12px;\">Cao điểm (CD)</td><td align=\"right\" style=\"color:#475569;\">{{PreviousCd}}</td><td align=\"right\" style=\"color:#475569;\">{{Cd}}</td><td align=\"right\" style=\"font-weight:700;color:#0f172a;\">{{ConsumtionCd}}</td></tr><tr><td style=\"padding-left:12px;\">Thấp điểm (TD)</td><td align=\"right\" style=\"color:#475569;\">{{PreviousTd}}</td><td align=\"right\" style=\"color:#475569;\">{{Td}}</td><td align=\"right\" style=\"font-weight:700;color:#0f172a;\">{{ConsumtionTd}}</td></tr><tr style=\"background-color:#fef2f2;font-weight:700;\"><td style=\"padding-left:12px;color:#991b1b;\">Tổng điện năng hữu công (SG)</td><td align=\"right\" style=\"color:#991b1b;\">{{PreviousSg}}</td><td align=\"right\" style=\"color:#991b1b;\">{{Sg}}</td><td align=\"right\" style=\"color:#b22222;font-size:14px;\">{{ConsumtionTotal}}</td></tr><tr><td style=\"padding-left:12px;color:#64748b;\">Tổng điện năng phản kháng (VC)</td><td align=\"right\" style=\"color:#64748b;\">{{PreviousVc}}</td><td align=\"right\" style=\"color:#64748b;\">{{Vc}}</td><td align=\"right\" style=\"font-weight:600;color:#64748b;\">{{ConsumtionVc}}</td></tr></table></td></tr><tr><td style=\"padding:0 30px 15px 30px;\"><table width=\"100%\" cellpadding=\"8\" cellspacing=\"0\" border=\"0\" style=\"background-color:#eff6ff;border:1px solid #bfdbfe;border-radius:6px;font-size:13px;color:#1e40af;\"><tr><td width=\"55%\" style=\"vertical-align:top;\">• Công suất cực đại <strong>Pmax</strong>: <strong>{{Pmax}}</strong><br>• Thời điểm ghi nhận hồi: <strong>{{PmaxTime}}</strong></td><td width=\"45%\" style=\"vertical-align:top;\">• Hệ số công suất <strong>Cos &phi;</strong>: <strong>{{CosPhi}}</strong></td></tr></table></td></tr><tr><td style=\"padding:0 30px 15px 30px;\"><div style=\"background-color:#f8fafc;border-left:4px solid #b22222;padding:12px 16px;border-radius:4px;font-size:13px;color:#334155;line-height:1.6;\">📎 Chi tiết Phiếu chỉ số điện hàng tháng được đính kèm <strong>PhieuBaoSo_{{SerialNumber}}.pdf</strong></div></td></tr><tr><td style=\"padding:0 30px 24px 30px;\"><div style=\"background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;\">Mọi thắc mắc hoặc yêu cầu đối soát số liệu, Quý khách vui lòng liên hệ: {{CareFullname}}<br>📞 Điện thoại: {{ContactPhone}}<br>✉️ Email: {{CareEmail}}</div><p style=\"margin:16px 0 0 0;font-size:13.5px;color:#334155;\">Trân trọng cảm ơn sự hợp tác của Quý khách hàng!</p></td></tr><tr><td style=\"background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;\">Email tự động phát hành từ HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM (MDMS) - {{CompanyName}}. Vui lòng không phản hồi trực tiếp vào địa chỉ thư này.</td></tr></table></td></tr></table></body></html>', 
 'Mẫu email thông báo sản lượng điện hàng tháng gửi khách hàng đính kèm file PDF', 1, 1, NOW()),

(2, 'FORGOT_PASSWORD', 'Đặt lại mật khẩu', 
 '[MDMS] Yêu cầu đặt lại mật khẩu', 
 '<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Yêu Cầu Đặt Lại Mật Khẩu</title></head><body style=\"margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,\'Segoe UI\',Roboto,Helvetica,Arial,sans-serif;color:#334155;\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"padding:40px 15px;\"><tr><td align=\"center\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"max-width:580px;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;\"><tr><td style=\"background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:28px 32px;text-align:center;\"><h2 style=\"margin:0;color:#ffffff;font-size:20px;font-weight:700;\">{{CompanyName}}</h2><p style=\"margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;\">HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM ĐIỆN TỬ (MDMS)</p><p style=\"margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;\">YÊU CẦU ĐẶT LẠI MẬT KHẨU</p></td></tr><tr><td style=\"padding:32px 32px 20px 32px;\"><p style=\"margin:0 0 14px 0;font-size:15px;color:#0f172a;\">Kính gửi <strong>{{Username}}</strong>,</p><p style=\"margin:0 0 20px 0;font-size:14px;line-height:1.7;color:#475569;\">Chúng tôi nhận được yêu cầu thiết lập lại mật khẩu đăng nhập cho tài khoản liên kết với địa chỉ email này trên hệ thống MDMS. Để hoàn tất quy trình, Quý vị vui lòng nhấp vào nút thao tác bên dưới:</p><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"margin:24px 0;\"><tr><td align=\"center\"><a href=\"{{ResetLink}}\" target=\"_blank\" style=\"display:inline-block;background-color:#b22222;color:#ffffff;font-size:14px;font-weight:600;text-decoration:none;padding:13px 36px;border-radius:6px;box-shadow:0 2px 8px rgba(178,34,34,0.3);\">Thiết Lập Lại Mật Khẩu</a></td></tr></table><div style=\"background-color:#fffbeb;border-left:4px solid #f59e0b;padding:12px 16px;margin-bottom:20px;border-radius:4px;\"><p style=\"margin:0;font-size:12.5px;color:#92400e;line-height:1.5;\">⏱️ <strong>Lưu ý bảo mật:</strong> Liên kết đặt lại mật khẩu có hiệu lực trong vòng <strong>10 phút</strong> kể từ khi phát hành. Nếu Quý vị không gửi yêu cầu này, vui lòng bỏ qua thư hoặc thông báo ngay cho quản trị viên.</p></div><div style=\"background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;\">Nếu Quý vị cần hỗ trợ kỹ thuật, vui lòng liên hệ: {{CareFullname}}<br>📞 Hotline: {{SupportPhone}}<br>✉️ Email: {{SystemEmail}}</div></td></tr><tr><td style=\"background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;\">Email tự động phát hành từ Hệ thống Quản trị Đo đếm Điện tử MDMS.</td></tr></table></td></tr></table></body></html>', 
 'Mẫu email gửi link yêu cầu thiết lập lại mật khẩu người dùng', 1, 1, NOW()),

(3, 'USER_CREDENTIAL', 'Cấp mới tài khoản & mật khẩu', 
 '[MDMS] Thông tin tài khoản - {{FullName}}', 
 '<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Thông Tin Cấp Tài Khoản</title></head><body style=\"margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,\'Segoe UI\',Roboto,Helvetica,Arial,sans-serif;color:#334155;\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"padding:40px 15px;\"><tr><td align=\"center\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"max-width:600px;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;\"><tr><td style=\"background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:28px 32px;text-align:center;\"><h2 style=\"margin:0;color:#ffffff;font-size:20px;font-weight:700;\">{{CompanyName}}</h2><p style=\"margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;\">HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM ĐIỆN TỬ (MDMS)</p><p style=\"margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;\">THÔNG TIN TÀI KHOẢN</p></td></tr><tr><td style=\"padding:32px 32px 20px 32px;\"><p style=\"margin:0 0 14px 0;font-size:15px;color:#0f172a;\">Kính gửi Quý người dùng: <strong>{{FullName}}</strong>,</p><p style=\"margin:0 0 20px 0;font-size:14px;line-height:1.7;color:#475569;\">Tài khoản truy cập Hệ thống Quản trị Dữ liệu Đo đếm Điện tử (MDMS) của Quý vị đã được cấp thành công. Thông tin tài khoản đăng nhập chi tiết như sau:</p><table width=\"100%\" cellpadding=\"12\" cellspacing=\"0\" border=\"0\" style=\"background-color:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;margin-bottom:24px;font-size:14px;\"><tr><td width=\"35%\" style=\"color:#64748b;font-weight:500;\">Tên đăng nhập:</td><td width=\"65%\" style=\"color:#0f172a;font-weight:700;font-family:monospace;font-size:15px;\">{{Username}}</td></tr><tr><td style=\"color:#64748b;font-weight:500;\">Mật khẩu khởi tạo:</td><td style=\"color:#b22222;font-weight:700;font-family:monospace;font-size:15px;\">{{Password}}</td></tr></table><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"margin:24px 0;\"><tr><td align=\"center\"><a href=\"{{LoginUrl}}\" target=\"_blank\" style=\"display:inline-block;background-color:#b22222;color:#ffffff;font-size:14px;font-weight:600;text-decoration:none;padding:13px 36px;border-radius:6px;box-shadow:0 2px 8px rgba(178,34,34,0.3);\">Đăng Nhập</a></td></tr></table><div style=\"background-color:#fffbeb;border-left:4px solid #f59e0b;padding:12px 16px;margin-bottom:20px;border-radius:4px;\"><p style=\"margin:0;font-size:13px;color:#92400e;line-height:1.5;\">⚠️ <strong>Khuyến cáo an toàn:</strong> Vì lý do bảo mật thông tin, Quý vị vui lòng thực hiện đổi mật khẩu ngay trong lần đăng nhập đầu tiên và tuyệt đối không chia sẻ mật khẩu này cho bất kỳ ai.</p></div><div style=\"background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;\">Nếu Quý vị cần hỗ trợ kỹ thuật, vui lòng liên hệ: {{CareFullname}}<br>📞 Hotline: {{SupportPhone}}<br>✉️ Email: {{SystemEmail}}</div></td></tr><tr><td style=\"background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;\">Email tự động phát hành từ Hệ thống Quản trị Đo đếm Điện tử MDMS.</td></tr></table></td></tr></table></body></html>', 
 'Mẫu email gửi thông tin tên đăng nhập và mật khẩu khi tài khoản được cấp mới', 1, 1, NOW());

-- 8. Insert mau khoi tao StatementPdfTemplates neu chua co
INSERT IGNORE INTO `StatementPdfTemplates` 
(`Id`, `Code`, `Name`, `HeaderTitle`, `SubHeaderTitle`, `CompanyName`, `CompanyAddress`, `ContactPhone`, `PaperSize`, `EngineType`, `DisplayColumnsJson`, `FooterNotes`, `RequireDualSignatures`, `IsDefault`, `IsActive`, `CreatedTime`)
VALUES
(1, 'A5_VSIP_STANDARD', 'Mẫu phiếu báo A5 Ngang - Chuẩn VSIP Hải Phòng', 
 'MONTHLY POWER METER READING', 'PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG', 
 'VSIP HAI PHONG CO., LTD', 'VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam', 'Tel: 84-225-3959868   Fax: 84-225-3959886', 
 'A5_LANDSCAPE', 'QUEST_PDF', '{"columns":["BT","CD","TD","SG","VC","COSPHI","REMARKS","PMAX","MULTIPLIER"],"documentCode":"M&E-1/01 PR04","copyLabel":"(M&E Copy – Liên cho Phòng Cơ Điện)","partyAName":"Party A (VSIP Hai Phong Co., Ltd)\\nBên A (Công ty TNHH VSIP Hải Phòng)","partyBName":"Party B (Tenant)\\nBên B (Khách hàng)","headerLeftMode":"LOGO","headerCenterMode":"LOGO","headerRightMode":"DECOR","footerLeftMode":"LOGO","footerCenterMode":"LOGO","footerRightMode":"LOGO"}', 
 'Quý khách vui lòng kiểm tra và phản hồi số liệu trong vòng 03 ngày làm việc kể từ ngày nhận phiếu báo.', 
 1, 1, 1, NOW()),

(2, 'A4_PORTRAIT_STANDARD', 'Mẫu phiếu báo A4 Đứng - Chuẩn Kế toán & Lưu trữ', 
 'MONTHLY POWER METER READING', 'PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG', 
 'VSIP HAI PHONG CO., LTD', 'VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam', 'Tel: 84-225-3959868   Fax: 84-225-3959886', 
 'A4_PORTRAIT', 'QUEST_PDF', '{"columns":["BT","CD","TD","SG","VC","COSPHI","REMARKS","PMAX","MULTIPLIER"],"documentCode":"M&E-1/01 PR04","copyLabel":"(M&E Copy – Liên cho Phòng Cơ Điện)","partyAName":"Party A (VSIP Hai Phong Co., Ltd)\\nBên A (Công ty TNHH VSIP Hải Phòng)","partyBName":"Party B (Tenant)\\nBên B (Khách hàng)","headerLeftMode":"LOGO","headerCenterMode":"LOGO","headerRightMode":"DECOR","footerLeftMode":"LOGO","footerCenterMode":"LOGO","footerRightMode":"LOGO"}', 
 'Phiếu báo này được kết xuất tự động từ hệ thống đo đếm từ xa AMR/MDMS. Mọi yêu cầu đối soát số liệu kỹ thuật xin liên hệ Bộ phận Vận hành Đo đếm.', 
 1, 0, 1, NOW());

