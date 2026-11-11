-- ===================================================================================
-- MDMSv2: Kịch bản SQL dọn dẹp toàn bộ dữ liệu biên bản treo tháo thử nghiệm
-- Thực thi trên database 'amr' thông qua MySQL Workbench hoặc dòng lệnh:
--   mysql -u root -p amr < clean_test_meter_replacements.sql
-- ===================================================================================

USE `amr`;

-- Tạm thời tắt Safe Update Mode trong MySQL Workbench (Error Code: 1175)
SET SQL_SAFE_UPDATES = 0;

-- Bước 1: Khôi phục lại Hệ số nhân (MultiplierFactor) của các Điểm đo về giá trị
-- hệ số nhân cũ (OldMeterMultiplier) trước khi thay thế nếu có biên bản đã được duyệt (APPROVED)
UPDATE `measurementpoints` p
JOIN (
    SELECT mr.MeasurementPointId, mr.OldMeterMultiplier
    FROM `meterreplacements` mr
    INNER JOIN (
        SELECT MeasurementPointId, MIN(ReplacementDate) AS MinDate
        FROM `meterreplacements`
        WHERE `Status` = 'APPROVED'
        GROUP BY MeasurementPointId
    ) first_rep ON mr.MeasurementPointId = first_rep.MeasurementPointId AND mr.ReplacementDate = first_rep.MinDate
) orig ON p.Id = orig.MeasurementPointId
SET p.MultiplierFactor = orig.OldMeterMultiplier,
    p.IsNewChange = 0
WHERE p.Id > 0;

-- Bước 2: Xóa toàn bộ bản ghi biên bản treo tháo phát sinh trong quá trình kiểm thử
DELETE FROM `meterreplacements` WHERE `Id` > 0;

-- Bước 3: Đảm bảo reset cờ IsNewChange của các điểm đo về 0
UPDATE `measurementpoints` SET `IsNewChange` = 0 WHERE `Id` > 0;

-- Bật lại Safe Update Mode
SET SQL_SAFE_UPDATES = 1;

-- Bước 4: Kiểm tra lại số lượng bản ghi còn lại (kết quả mong đợi: 0)
SELECT COUNT(*) AS `TotalReplacementsRemaining` FROM `meterreplacements`;
