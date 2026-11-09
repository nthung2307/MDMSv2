-- =====================================================================================
-- SCRIPT SQL: XÓA DỮ LIỆU SỔ GHI CHỈ SỐ (GCS) VÀ PHIẾU BÁO ĐỊNH KỲ THÁNG 10/2026 ĐỂ KIỂM THỬ LẠI
-- Dự án: MDMSv2 (Hệ thống Quản lý Dữ liệu Đo đếm Công tơ Điện tử)
-- Mục đích: Hủy kỳ chốt sổ Tháng 10/2026 để người dùng có thể thực hiện lại quy trình Ghi chỉ số
-- =====================================================================================

-- 1. Chọn cơ sở dữ liệu amr
USE `amr`;

-- 2. Hủy transaction trước đó nếu đang bị treo dở dang do lỗi
ROLLBACK;

-- 3. TẮT CHẾ ĐỘ SAFE UPDATES CỦA MYSQL WORKBENCH (Tránh lỗi Error Code: 1175)
SET SQL_SAFE_UPDATES = 0;

-- -------------------------------------------------------------------------------------
-- BƯỚC 1: KIỂM TRA SỐ LƯỢNG BẢN GHI SẼ BỊ XÓA TRƯỚC KHI THỰC HIỆN
-- -------------------------------------------------------------------------------------
SELECT 'GcsCutoffCycle' AS TableName, COUNT(*) AS TotalRecords 
FROM `GcsCutoffCycle` 
WHERE `Month` = 10 AND `Year` = 2026;

SELECT 'GcsMonthlyDetail' AS TableName, COUNT(*) AS TotalRecords 
FROM `GcsMonthlyDetail` 
WHERE `GcsCutoffCycleId` IN (
    SELECT `Id` FROM `GcsCutoffCycle` WHERE `Month` = 10 AND `Year` = 2026
);

SELECT 'PeriodicEnergyStatement' AS TableName, COUNT(*) AS TotalRecords 
FROM `PeriodicEnergyStatement` 
WHERE `Month` = 10 AND `Year` = 2026;

SELECT 'GcsTemporary' AS TableName, COUNT(*) AS TotalRecords 
FROM `GcsTemporary` 
WHERE `Month` = 10 AND `Year` = 2026;


-- -------------------------------------------------------------------------------------
-- BƯỚC 2: THỰC HIỆN XÓA DỮ LIỆU TRONG TRANSACTION AN TOÀN
-- -------------------------------------------------------------------------------------
START TRANSACTION;

-- 1. Xóa chi tiết sổ ghi chỉ số đã chốt (GcsMonthlyDetail) của Tháng 10/2026
DELETE md FROM `GcsMonthlyDetail` md
INNER JOIN `GcsCutoffCycle` cc ON md.`GcsCutoffCycleId` = cc.`Id`
WHERE cc.`Month` = 10 AND cc.`Year` = 2026;

-- 2. Xóa phiếu báo năng lượng định kỳ (PeriodicEnergyStatement) của Tháng 10/2026
DELETE FROM `PeriodicEnergyStatement`
WHERE `Month` = 10 AND `Year` = 2026;

-- 3. Xóa thông tin kỳ chốt sổ (GcsCutoffCycle) của Tháng 10/2026
DELETE FROM `GcsCutoffCycle`
WHERE `Month` = 10 AND `Year` = 2026;

-- 4. Xóa dữ liệu tạm thời (GcsTemporary) của Tháng 10/2026 (nếu còn tồn tại)
--    để khi người dùng mở lại trang "Ghi chỉ số", hệ thống sẽ đọc mới từ bảng IoT/công tơ
DELETE FROM `GcsTemporary`
WHERE `Month` = 10 AND `Year` = 2026;

-- 5. Đặt lại cờ IsNewChange của các sổ GCS và điểm đo để hệ thống sẵn sàng ghi nhận mới
UPDATE `Gcs` SET `IsNewChange` = 1 WHERE `Status` = 1;

COMMIT;

-- BẬT LẠI CHẾ ĐỘ SAFE UPDATES SAU KHI XÓA XONG
SET SQL_SAFE_UPDATES = 1;

-- -------------------------------------------------------------------------------------
-- BƯỚC 3: KIỂM TRA LẠI KẾT QUẢ SAU KHI XÓA (KẾT QUẢ TẤT CẢ PHẢI BẰNG 0)
-- -------------------------------------------------------------------------------------
SELECT 'GcsCutoffCycle' AS TableName, COUNT(*) AS RemainingRecords 
FROM `GcsCutoffCycle` WHERE `Month` = 10 AND `Year` = 2026
UNION ALL
SELECT 'GcsMonthlyDetail', COUNT(*) 
FROM `GcsMonthlyDetail` WHERE `GcsCutoffCycleId` IN (
    SELECT `Id` FROM `GcsCutoffCycle` WHERE `Month` = 10 AND `Year` = 2026
)
UNION ALL
SELECT 'PeriodicEnergyStatement', COUNT(*) 
FROM `PeriodicEnergyStatement` WHERE `Month` = 10 AND `Year` = 2026
UNION ALL
SELECT 'GcsTemporary', COUNT(*) 
FROM `GcsTemporary` WHERE `Month` = 10 AND `Year` = 2026;
