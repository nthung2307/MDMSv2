-- =====================================================================================================
-- SCRIPT XÓA BỎ TOÀN BỘ SỰ KIỆN & CẢNH BÁO ĐỂ KIỂM THỬ (CLEAN SLATE TESTING)
-- Dự án: MDMSv2 (Meter Data Management System v2)
-- Cơ sở dữ liệu: amr (MySQL)
-- 
-- MỤC ĐÍCH:
-- 1. Xóa toàn bộ dữ liệu lịch sử sự kiện trong bảng `measurementpointevents` (Trang "Bảng sự kiện")
-- 2. Xóa toàn bộ dữ liệu cảnh báo hiện hữu trong bảng `measurementpointalerts` (Trang "Bảng cảnh báo")
-- 3. Reset cờ cảnh báo `WarningValue` trong bảng `tagname` về 0 để đồng bộ trạng thái ban đầu của điểm đo.
-- =====================================================================================================

USE `amr`;

-- 1. Tạm thời vô hiệu hóa kiểm tra khóa ngoại (Foreign Key Checks)
SET @OLD_FOREIGN_KEY_CHECKS = @@FOREIGN_KEY_CHECKS;
SET FOREIGN_KEY_CHECKS = 0;

SET @OLD_SQL_SAFE_UPDATES = @@SQL_SAFE_UPDATES;
SET SQL_SAFE_UPDATES = 0;

START TRANSACTION;

-- 2. Xóa toàn bộ dữ liệu bảng sự kiện điểm đo (Trang "Bảng sự kiện")
TRUNCATE TABLE `measurementpointevents`;

-- 3. Xóa toàn bộ dữ liệu bảng cảnh báo điểm đo (Trang "Bảng cảnh báo")
TRUNCATE TABLE `measurementpointalerts`;

-- 4. Reset cờ trạng thái cảnh báo trên danh mục thanh ghi / thẻ dữ liệu
UPDATE `tagname` 
SET `WarningValue` = 0 
WHERE `WarningValue` IS NOT NULL AND `WarningValue` != 0;

COMMIT;

SET FOREIGN_KEY_CHECKS = @OLD_FOREIGN_KEY_CHECKS;
SET SQL_SAFE_UPDATES = @OLD_SQL_SAFE_UPDATES;

-- 5. Kiểm tra lại số lượng bản ghi sau khi xóa:
SELECT 'measurementpointevents' AS `Table_Name`, COUNT(*) AS `Total_Remaining_Records` FROM `measurementpointevents`
UNION ALL
SELECT 'measurementpointalerts' AS `Table_Name`, COUNT(*) AS `Total_Remaining_Records` FROM `measurementpointalerts`;

