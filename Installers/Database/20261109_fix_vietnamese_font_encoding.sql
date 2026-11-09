-- =====================================================================================
-- SCRIPT SQL: KHẮC PHỤC TRIỆT ĐỂ LỖI FONT TIẾNG VIỆT CHO EMAIL VÀ PHIẾU BÁO CHỈ SỐ
-- Dự án: MDMSv2 (Hệ thống Quản lý Dữ liệu Đo đếm Công tơ Điện tử)
-- Mục đích: Khôi phục chuỗi văn bản tiếng Việt UTF-8 chuẩn xác 100% cho bảng:
--          1. StatementPdfTemplates (Mẫu phiếu báo chỉ số điện PDF A5/A4)
--          2. NotificationTemplates (Mẫu email thông báo sản lượng điện hàng tháng)
--          3. SystemEmailConfigs (Tên người gửi: VSIP Hải Phòng - Phòng Cơ Điện)
-- =====================================================================================

SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;

USE `amr`;

-- 1. Tắt tạm thời Safe Updates
ROLLBACK;
SET SQL_SAFE_UPDATES = 0;

START TRANSACTION;

-- =====================================================================================
-- BƯỚC 1: SỬA LỖI FONT TRONG BẢNG MẪU PHIẾU BÁO PDF (StatementPdfTemplates)
-- =====================================================================================

-- 1.1. Cập nhật Mẫu A5 Ngang - Tiêu chuẩn VSIP Hải Phòng (Id = 1)
UPDATE `StatementPdfTemplates`
SET 
  `Name` = 'Mẫu phiếu báo A5 Ngang - Chuẩn VSIP Hải Phòng',
  `HeaderTitle` = 'MONTHLY POWER METER READING',
  `SubHeaderTitle` = 'PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG',
  `CompanyName` = 'VSIP HAI PHONG CO., LTD',
  `CompanyAddress` = 'VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam',
  `ContactPhone` = 'Tel: 84-225-3959868   Fax: 84-225-3959886',
  `PaperSize` = 'A5_LANDSCAPE',
  `EngineType` = 'QUEST_PDF',
  `DisplayColumnsJson` = '{"columns":["BT","CD","TD","SG","VC","COSPHI","REMARKS","PMAX","MULTIPLIER"],"documentCode":"M&E-1/01 PR04","copyLabel":"(M&E Copy – Liên cho Phòng Cơ Điện)","partyAName":"Party A (VSIP Hai Phong Co., Ltd)\\nBên A (Công ty TNHH VSIP Hải Phòng)","partyBName":"Party B (Tenant)\\nBên B (Khách hàng)","headerLeftMode":"LOGO","headerCenterMode":"LOGO","headerRightMode":"DECOR","footerLeftMode":"LOGO","footerCenterMode":"LOGO","footerRightMode":"LOGO"}',
  `RequireDualSignatures` = 1,
  `FooterNotes` = 'Quý khách vui lòng kiểm tra và phản hồi số liệu trong vòng 03 ngày làm việc kể từ ngày nhận phiếu báo.'
WHERE `Code` = 'A5_VSIP_STANDARD' OR `Code` = 'STATEMENT_PDF_A5_STANDARD' OR `Id` = 1;

UPDATE `statementpdftemplates`
SET 
  `Name` = 'Mẫu phiếu báo A5 Ngang - Chuẩn VSIP Hải Phòng',
  `HeaderTitle` = 'MONTHLY POWER METER READING',
  `SubHeaderTitle` = 'PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG',
  `CompanyName` = 'VSIP HAI PHONG CO., LTD',
  `CompanyAddress` = 'VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam',
  `ContactPhone` = 'Tel: 84-225-3959868   Fax: 84-225-3959886',
  `PaperSize` = 'A5_LANDSCAPE',
  `EngineType` = 'QUEST_PDF',
  `DisplayColumnsJson` = '{"columns":["BT","CD","TD","SG","VC","COSPHI","REMARKS","PMAX","MULTIPLIER"],"documentCode":"M&E-1/01 PR04","copyLabel":"(M&E Copy – Liên cho Phòng Cơ Điện)","partyAName":"Party A (VSIP Hai Phong Co., Ltd)\\nBên A (Công ty TNHH VSIP Hải Phòng)","partyBName":"Party B (Tenant)\\nBên B (Khách hàng)","headerLeftMode":"LOGO","headerCenterMode":"LOGO","headerRightMode":"DECOR","footerLeftMode":"LOGO","footerCenterMode":"LOGO","footerRightMode":"LOGO"}',
  `RequireDualSignatures` = 1,
  `FooterNotes` = 'Quý khách vui lòng kiểm tra và phản hồi số liệu trong vòng 03 ngày làm việc kể từ ngày nhận phiếu báo.'
WHERE `Code` = 'A5_VSIP_STANDARD' OR `Code` = 'STATEMENT_PDF_A5_STANDARD' OR `Id` = 1;

-- 1.2. Cập nhật Mẫu A4 Đứng - Chuẩn Kế toán & Lưu trữ (Id = 2)
UPDATE `StatementPdfTemplates`
SET 
  `Name` = 'Mẫu phiếu báo A4 Đứng - Chuẩn Kế toán & Lưu trữ',
  `HeaderTitle` = 'MONTHLY POWER METER READING',
  `SubHeaderTitle` = 'PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG',
  `CompanyName` = 'VSIP HAI PHONG CO., LTD',
  `CompanyAddress` = 'VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam',
  `ContactPhone` = 'Tel: 84-225-3959868   Fax: 84-225-3959886',
  `PaperSize` = 'A4_PORTRAIT',
  `EngineType` = 'QUEST_PDF',
  `DisplayColumnsJson` = '{"columns":["BT","CD","TD","SG","VC","COSPHI","REMARKS","PMAX","MULTIPLIER"],"documentCode":"M&E-1/01 PR04","copyLabel":"(M&E Copy – Liên cho Phòng Cơ Điện)","partyAName":"Party A (VSIP Hai Phong Co., Ltd)\\nBên A (Công ty TNHH VSIP Hải Phòng)","partyBName":"Party B (Tenant)\\nBên B (Khách hàng)","headerLeftMode":"LOGO","headerCenterMode":"LOGO","headerRightMode":"DECOR","footerLeftMode":"LOGO","footerCenterMode":"LOGO","footerRightMode":"LOGO"}',
  `RequireDualSignatures` = 1,
  `FooterNotes` = 'Phiếu báo này được kết xuất tự động từ hệ thống đo đếm từ xa AMR/MDMS. Mọi yêu cầu đối soát số liệu kỹ thuật xin liên hệ Bộ phận Vận hành Đo đếm.'
WHERE `Code` = 'A4_PORTRAIT_STANDARD' OR `Code` = 'STATEMENT_PDF_A4_FULL' OR `Id` = 2;

UPDATE `statementpdftemplates`
SET 
  `Name` = 'Mẫu phiếu báo A4 Đứng - Chuẩn Kế toán & Lưu trữ',
  `HeaderTitle` = 'MONTHLY POWER METER READING',
  `SubHeaderTitle` = 'PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG',
  `CompanyName` = 'VSIP HAI PHONG CO., LTD',
  `CompanyAddress` = 'VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam',
  `ContactPhone` = 'Tel: 84-225-3959868   Fax: 84-225-3959886',
  `PaperSize` = 'A4_PORTRAIT',
  `EngineType` = 'QUEST_PDF',
  `DisplayColumnsJson` = '{"columns":["BT","CD","TD","SG","VC","COSPHI","REMARKS","PMAX","MULTIPLIER"],"documentCode":"M&E-1/01 PR04","copyLabel":"(M&E Copy – Liên cho Phòng Cơ Điện)","partyAName":"Party A (VSIP Hai Phong Co., Ltd)\\nBên A (Công ty TNHH VSIP Hải Phòng)","partyBName":"Party B (Tenant)\\nBên B (Khách hàng)","headerLeftMode":"LOGO","headerCenterMode":"LOGO","headerRightMode":"DECOR","footerLeftMode":"LOGO","footerCenterMode":"LOGO","footerRightMode":"LOGO"}',
  `RequireDualSignatures` = 1,
  `FooterNotes` = 'Phiếu báo này được kết xuất tự động từ hệ thống đo đếm từ xa AMR/MDMS. Mọi yêu cầu đối soát số liệu kỹ thuật xin liên hệ Bộ phận Vận hành Đo đếm.'
WHERE `Code` = 'A4_PORTRAIT_STANDARD' OR `Code` = 'STATEMENT_PDF_A4_FULL' OR `Id` = 2;


-- =====================================================================================
-- BƯỚC 2: SỬA LỖI FONT TRONG BẢNG CẤU HÌNH GỬI EMAIL (SystemEmailConfigs)
-- =====================================================================================

UPDATE `SystemEmailConfigs`
SET `SenderDisplayName` = 'VSIP Hải Phòng - Phòng Cơ Điện'
WHERE `Id` = 1;

UPDATE `systememailconfigs`
SET `SenderDisplayName` = 'VSIP Hải Phòng - Phòng Cơ Điện'
WHERE `Id` = 1;


-- =====================================================================================
-- BƯỚC 3: SỬA LỖI FONT TRONG BẢNG MẪU EMAIL THÔNG BÁO (NotificationTemplates)
-- =====================================================================================

-- 3.1. Mẫu Thông báo chỉ số điện hàng tháng (STATEMENT_NOTIFICATION)
UPDATE `NotificationTemplates`
SET 
  `Name` = 'Phiếu chỉ số',
  `SubjectTemplate` = 'THÔNG BÁO CHỈ SỐ ĐIỆN HÀNG THÁNG',
  `Description` = 'Mẫu email thông báo sản lượng điện hàng tháng gửi khách hàng đính kèm file PDF',
  `BodyHtmlTemplate` = '<!DOCTYPE html><html><head><meta charset="utf-8"><title>Thông Báo Chỉ Số & Sản Lượng Điện Năng</title></head><body style="margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,\'Segoe UI\',Roboto,Helvetica,Arial,sans-serif;color:#334155;"><table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f5f7fc;padding:24px 10px;width:100%;max-width:100%;"><tr><td align="center"><table width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:640px;width:100%;background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;"><tr><td style="background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:24px 30px;text-align:center;"><h2 style="margin:0;color:#ffffff;font-size:20px;font-weight:700;">{{CompanyName}}</h2><p style="margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;">HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM (MDMS)</p><p style="margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;">THÔNG BÁO CHỈ SỐ & SẢN LƯỢNG ĐIỆN NĂNG</p></td></tr><tr><td style="padding:24px 30px 10px 30px;"><p style="margin:0 0 14px 0;font-size:15px;color:#0f172a;">Kính gửi Quý khách hàng: <strong>{{CustomerName}}</strong>,</p><p style="margin:0 0 16px 0;font-size:13.5px;line-height:1.6;color:#475569;">Chúng tôi xin trân trọng gửi tới Quý khách hàng bảng kê chỉ số công tơ và sản lượng điện năng tiêu thụ trong kỳ ghi chỉ số: {{Period}}.</p><table width="100%" cellpadding="10" cellspacing="0" border="0" style="background-color:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;margin-bottom:18px;font-size:13.5px;"><tr><td width="22%" style="color:#64748b;font-weight:500;">Tên điểm đo:</td><td style="font-weight:700;color:#0f172a;">{{PointName}}</td></tr></table></td></tr><tr><td style="padding:0 30px 15px 30px;"><table width="100%" cellpadding="8" cellspacing="0" border="1" style="border-collapse:collapse;border-color:#e2e8f0;font-size:13px;"><tr style="background-color:#f1f5f9;color:#0f172a;font-weight:600;text-align:center;"><td width="32%">Biểu giá</td><td width="22%">Chỉ số cũ</td><td width="22%">Chỉ số mới</td><td width="24%">Sản lượng (kWh)</td></tr><tr><td style="padding-left:12px;">Bình thường (BT)</td><td align="right" style="color:#475569;">{{PreviousBt}}</td><td align="right" style="color:#475569;">{{Bt}}</td><td align="right" style="font-weight:700;color:#0f172a;">{{ConsumtionBt}}</td></tr><tr><td style="padding-left:12px;">Cao điểm (CD)</td><td align="right" style="color:#475569;">{{PreviousCd}}</td><td align="right" style="color:#475569;">{{Cd}}</td><td align="right" style="font-weight:700;color:#0f172a;">{{ConsumtionCd}}</td></tr><tr><td style="padding-left:12px;">Thấp điểm (TD)</td><td align="right" style="color:#475569;">{{PreviousTd}}</td><td align="right" style="color:#475569;">{{Td}}</td><td align="right" style="font-weight:700;color:#0f172a;">{{ConsumtionTd}}</td></tr><tr style="background-color:#fef2f2;font-weight:700;"><td style="padding-left:12px;color:#991b1b;">Tổng điện năng hữu công (SG)</td><td align="right" style="color:#991b1b;">{{PreviousSg}}</td><td align="right" style="color:#991b1b;">{{Sg}}</td><td align="right" style="color:#b22222;font-size:14px;">{{ConsumtionTotal}}</td></tr><tr><td style="padding-left:12px;color:#64748b;">Tổng điện năng phản kháng (VC)</td><td align="right" style="color:#64748b;">{{PreviousVc}}</td><td align="right" style="color:#64748b;">{{Vc}}</td><td align="right" style="font-weight:600;color:#64748b;">{{ConsumtionVc}}</td></tr></table></td></tr><tr><td style="padding:0 30px 15px 30px;"><table width="100%" cellpadding="8" cellspacing="0" border="0" style="background-color:#eff6ff;border:1px solid #bfdbfe;border-radius:6px;font-size:13px;color:#1e40af;"><tr><td width="55%" style="vertical-align:top;">• Công suất cực đại <strong>Pmax</strong>: <strong>{{Pmax}}</strong><br>• Thời điểm ghi nhận hồi: <strong>{{PmaxTime}}</strong></td><td width="45%" style="vertical-align:top;">• Hệ số công suất <strong>Cos &phi;</strong>: <strong>{{CosPhi}}</strong></td></tr></table></td></tr><tr><td style="padding:0 30px 15px 30px;"><div style="background-color:#f8fafc;border-left:4px solid #b22222;padding:12px 16px;border-radius:4px;font-size:13px;color:#334155;line-height:1.6;">📎 Chi tiết Phiếu chỉ số điện hàng tháng được đính kèm <strong>PhieuBaoSo_{{SerialNumber}}.pdf</strong></div></td></tr><tr><td style="padding:0 30px 24px 30px;"><div style="background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;">Mọi thắc mắc hoặc yêu cầu đối soát số liệu, Quý khách vui lòng liên hệ: {{CareFullname}}<br>📞 Điện thoại: {{ContactPhone}}<br>✉️ Email: {{CareEmail}}</div><p style="margin:16px 0 0 0;font-size:13.5px;color:#334155;">Trân trọng cảm ơn sự hợp tác của Quý khách hàng!</p></td></tr><tr><td style="background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;">Email tự động phát hành từ HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM (MDMS) - {{CompanyName}}. Vui lòng không phản hồi trực tiếp vào địa chỉ thư này.</td></tr></table></td></tr></table></body></html>'
WHERE `Code` = 'STATEMENT_NOTIFICATION' OR `Id` = 1;

UPDATE `notificationtemplates`
SET 
  `Name` = 'Phiếu chỉ số',
  `SubjectTemplate` = 'THÔNG BÁO CHỈ SỐ ĐIỆN HÀNG THÁNG',
  `Description` = 'Mẫu email thông báo sản lượng điện hàng tháng gửi khách hàng đính kèm file PDF',
  `BodyHtmlTemplate` = '<!DOCTYPE html><html><head><meta charset="utf-8"><title>Thông Báo Chỉ Số & Sản Lượng Điện Năng</title></head><body style="margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,\'Segoe UI\',Roboto,Helvetica,Arial,sans-serif;color:#334155;"><table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f5f7fc;padding:24px 10px;width:100%;max-width:100%;"><tr><td align="center"><table width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:640px;width:100%;background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;"><tr><td style="background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:24px 30px;text-align:center;"><h2 style="margin:0;color:#ffffff;font-size:20px;font-weight:700;">{{CompanyName}}</h2><p style="margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;">HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM (MDMS)</p><p style="margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;">THÔNG BÁO CHỈ SỐ & SẢN LƯỢNG ĐIỆN NĂNG</p></td></tr><tr><td style="padding:24px 30px 10px 30px;"><p style="margin:0 0 14px 0;font-size:15px;color:#0f172a;">Kính gửi Quý khách hàng: <strong>{{CustomerName}}</strong>,</p><p style="margin:0 0 16px 0;font-size:13.5px;line-height:1.6;color:#475569;">Chúng tôi xin trân trọng gửi tới Quý khách hàng bảng kê chỉ số công tơ và sản lượng điện năng tiêu thụ trong kỳ ghi chỉ số: {{Period}}.</p><table width="100%" cellpadding="10" cellspacing="0" border="0" style="background-color:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;margin-bottom:18px;font-size:13.5px;"><tr><td width="22%" style="color:#64748b;font-weight:500;">Tên điểm đo:</td><td style="font-weight:700;color:#0f172a;">{{PointName}}</td></tr></table></td></tr><tr><td style="padding:0 30px 15px 30px;"><table width="100%" cellpadding="8" cellspacing="0" border="1" style="border-collapse:collapse;border-color:#e2e8f0;font-size:13px;"><tr style="background-color:#f1f5f9;color:#0f172a;font-weight:600;text-align:center;"><td width="32%">Biểu giá</td><td width="22%">Chỉ số cũ</td><td width="22%">Chỉ số mới</td><td width="24%">Sản lượng (kWh)</td></tr><tr><td style="padding-left:12px;">Bình thường (BT)</td><td align="right" style="color:#475569;">{{PreviousBt}}</td><td align="right" style="color:#475569;">{{Bt}}</td><td align="right" style="font-weight:700;color:#0f172a;">{{ConsumtionBt}}</td></tr><tr><td style="padding-left:12px;">Cao điểm (CD)</td><td align="right" style="color:#475569;">{{PreviousCd}}</td><td align="right" style="color:#475569;">{{Cd}}</td><td align="right" style="font-weight:700;color:#0f172a;">{{ConsumtionCd}}</td></tr><tr><td style="padding-left:12px;">Thấp điểm (TD)</td><td align="right" style="color:#475569;">{{PreviousTd}}</td><td align="right" style="color:#475569;">{{Td}}</td><td align="right" style="font-weight:700;color:#0f172a;">{{ConsumtionTd}}</td></tr><tr style="background-color:#fef2f2;font-weight:700;"><td style="padding-left:12px;color:#991b1b;">Tổng điện năng hữu công (SG)</td><td align="right" style="color:#991b1b;">{{PreviousSg}}</td><td align="right" style="color:#991b1b;">{{Sg}}</td><td align="right" style="color:#b22222;font-size:14px;">{{ConsumtionTotal}}</td></tr><tr><td style="padding-left:12px;color:#64748b;">Tổng điện năng phản kháng (VC)</td><td align="right" style="color:#64748b;">{{PreviousVc}}</td><td align="right" style="color:#64748b;">{{Vc}}</td><td align="right" style="font-weight:600;color:#64748b;">{{ConsumtionVc}}</td></tr></table></td></tr><tr><td style="padding:0 30px 15px 30px;"><table width="100%" cellpadding="8" cellspacing="0" border="0" style="background-color:#eff6ff;border:1px solid #bfdbfe;border-radius:6px;font-size:13px;color:#1e40af;"><tr><td width="55%" style="vertical-align:top;">• Công suất cực đại <strong>Pmax</strong>: <strong>{{Pmax}}</strong><br>• Thời điểm ghi nhận hồi: <strong>{{PmaxTime}}</strong></td><td width="45%" style="vertical-align:top;">• Hệ số công suất <strong>Cos &phi;</strong>: <strong>{{CosPhi}}</strong></td></tr></table></td></tr><tr><td style="padding:0 30px 15px 30px;"><div style="background-color:#f8fafc;border-left:4px solid #b22222;padding:12px 16px;border-radius:4px;font-size:13px;color:#334155;line-height:1.6;">📎 Chi tiết Phiếu chỉ số điện hàng tháng được đính kèm <strong>PhieuBaoSo_{{SerialNumber}}.pdf</strong></div></td></tr><tr><td style="padding:0 30px 24px 30px;"><div style="background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;">Mọi thắc mắc hoặc yêu cầu đối soát số liệu, Quý khách vui lòng liên hệ: {{CareFullname}}<br>📞 Điện thoại: {{ContactPhone}}<br>✉️ Email: {{CareEmail}}</div><p style="margin:16px 0 0 0;font-size:13.5px;color:#334155;">Trân trọng cảm ơn sự hợp tác của Quý khách hàng!</p></td></tr><tr><td style="background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;">Email tự động phát hành từ HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM (MDMS) - {{CompanyName}}. Vui lòng không phản hồi trực tiếp vào địa chỉ thư này.</td></tr></table></td></tr></table></body></html>'
WHERE `Code` = 'STATEMENT_NOTIFICATION' OR `Id` = 1;

COMMIT;

SET SQL_SAFE_UPDATES = 1;

-- Kiểm tra lại sau khi cập nhật
SELECT 'StatementPdfTemplates' AS Bang, `Code`, `CompanyName`, `HeaderTitle` FROM `statementpdftemplates` WHERE `Id` IN (1, 2)
UNION ALL
SELECT 'NotificationTemplates', `Code`, `Name`, `SubjectTemplate` FROM `notificationtemplates` WHERE `Id` = 1
UNION ALL
SELECT 'SystemEmailConfigs', `ProviderType`, `SenderEmail`, `SenderDisplayName` FROM `systememailconfigs` WHERE `Id` = 1;

