-- =====================================================================================
-- SCRIPT SQL: HỦY / XÓA SỔ GHI CHỈ SỐ (GCS) ĐÃ CHỐT ĐỂ THỰC HIỆN KIỂM THỬ LẠI
-- Dự án: MDMSv2 (Hệ thống Quản lý Dữ liệu Đo đếm Công tơ Điện tử)
-- Mục đích: Hủy kỳ chốt sổ và phiếu báo tương ứng, làm sạch bảng tạm để người dùng
--          có thể thực hiện lại tác vụ "Ghi chỉ số" & "Chốt sổ" từ đầu.
-- =====================================================================================

USE `amr`;

-- 1. Hủy transaction treo trước đó (nếu có)
ROLLBACK;

-- 2. Tắt chế độ Safe Updates tạm thời để cho phép DELETE/UPDATE theo điều kiện
SET SQL_SAFE_UPDATES = 0;

-- =====================================================================================
-- CẤU HÌNH KỲ CẦN XÓA (Điều chỉnh theo nhu cầu):
--   - Nếu muốn xóa kỳ cụ thể: Đặt tháng và năm (ví dụ: SET @target_month = 10; SET @target_year = 2026;)
--   - Nếu muốn xóa TOÀN BỘ tất cả các kỳ đã chốt để kiểm thử lại từ đầu: Đặt @target_month = NULL;
-- =====================================================================================
SET @target_month = NULL;  -- NULL = Xóa toàn bộ tất cả các kỳ; Hoặc nhập số tháng: 10, 11,...
SET @target_year  = NULL;  -- NULL = Xóa toàn bộ các năm; Hoặc nhập số năm: 2026,...

-- -------------------------------------------------------------------------------------
-- BƯỚC 1: KIỂM TRA SỐ LƯỢNG BẢN GHI TRƯỚC KHI XÓA
-- -------------------------------------------------------------------------------------
SELECT '1. GcsCutoffCycle (Kỳ chốt)' AS Bang, COUNT(*) AS SoLuongBanGhi 
FROM `gcscutoffcycle` 
WHERE (@target_month IS NULL OR `Month` = @target_month)
  AND (@target_year  IS NULL OR `Year`  = @target_year)
UNION ALL
SELECT '2. GcsMonthlyDetail (Chi tiết chốt)', COUNT(*) 
FROM `gcsmonthlydetail` 
WHERE `GcsCutoffCycleId` IN (
    SELECT `Id` FROM `gcscutoffcycle` 
    WHERE (@target_month IS NULL OR `Month` = @target_month)
      AND (@target_year  IS NULL OR `Year`  = @target_year)
)
UNION ALL
SELECT '3. PeriodicEnergyStatement (Phiếu báo)', COUNT(*) 
FROM `periodicenergystatement` 
WHERE (@target_month IS NULL OR `Month` = @target_month)
  AND (@target_year  IS NULL OR `Year`  = @target_year)
UNION ALL
SELECT '4. GcsTemporary (Bản ghi tạm)', COUNT(*) 
FROM `gcstemporary` 
WHERE (@target_month IS NULL OR `Month` = @target_month)
  AND (@target_year  IS NULL OR `Year`  = @target_year);

-- -------------------------------------------------------------------------------------
-- BƯỚC 2: TIẾN HÀNH XÓA DỮ LIỆU TRONG TRANSACTION AN TOÀN
-- -------------------------------------------------------------------------------------
START TRANSACTION;

-- 1. Xóa chi tiết sổ ghi chỉ số đã chốt (gcsmonthlydetail) theo chu kỳ chốt
DELETE md FROM `gcsmonthlydetail` md
INNER JOIN `gcscutoffcycle` cc ON md.`GcsCutoffCycleId` = cc.`Id`
WHERE (@target_month IS NULL OR cc.`Month` = @target_month)
  AND (@target_year  IS NULL OR cc.`Year`  = @target_year);

-- 2. Xóa phiếu báo điện năng định kỳ đã sinh (periodicenergystatement)
DELETE FROM `periodicenergystatement`
WHERE (@target_month IS NULL OR `Month` = @target_month)
  AND (@target_year  IS NULL OR `Year`  = @target_year);

-- 3. Xóa thông tin các kỳ chốt sổ (gcscutoffcycle)
DELETE FROM `gcscutoffcycle`
WHERE (@target_month IS NULL OR `Month` = @target_month)
  AND (@target_year  IS NULL OR `Year`  = @target_year);

-- 4. Xóa toàn bộ dữ liệu tạm thời (gcstemporary) của kỳ tương ứng (hoặc tất cả nếu NULL)
--    để khi mở lại màn hình "Ghi chỉ số", hệ thống sẽ tự động nạp mới từ dữ liệu công tơ IoT
DELETE FROM `gcstemporary`
WHERE (@target_month IS NULL OR `Month` = @target_month)
  AND (@target_year  IS NULL OR `Year`  = @target_year);

-- 5. Đặt lại cờ IsNewChange = 1 cho các sổ GCS và Điểm đo
--    giúp hệ thống nhận diện đây là kỳ chốt ban đầu hoặc cần tái tạo dữ liệu đo đếm mới
UPDATE `gcs` SET `IsNewChange` = 1 WHERE `Status` = 1;
UPDATE `measurementpoints` SET `IsNewChange` = 1 WHERE `Status` = 1;

COMMIT;

-- Bật lại chế độ Safe Updates sau khi thao tác xong
SET SQL_SAFE_UPDATES = 1;

-- -------------------------------------------------------------------------------------
-- BƯỚC 3: KIỂM TRA LẠI KẾT QUẢ SAU KHI XÓA (KẾT QUẢ PHẢI VỀ 0)
-- -------------------------------------------------------------------------------------
SELECT '1. GcsCutoffCycle (Kỳ chốt)' AS Bang, COUNT(*) AS SoLuongConLai 
FROM `gcscutoffcycle` 
WHERE (@target_month IS NULL OR `Month` = @target_month)
  AND (@target_year  IS NULL OR `Year`  = @target_year)
UNION ALL
SELECT '2. GcsMonthlyDetail (Chi tiết chốt)', COUNT(*) 
FROM `gcsmonthlydetail` 
WHERE `GcsCutoffCycleId` IN (
    SELECT `Id` FROM `gcscutoffcycle` 
    WHERE (@target_month IS NULL OR `Month` = @target_month)
      AND (@target_year  IS NULL OR `Year`  = @target_year)
)
UNION ALL
SELECT '3. PeriodicEnergyStatement (Phiếu báo)', COUNT(*) 
FROM `periodicenergystatement` 
WHERE (@target_month IS NULL OR `Month` = @target_month)
  AND (@target_year  IS NULL OR `Year`  = @target_year)
UNION ALL
SELECT '4. GcsTemporary (Bản ghi tạm)', COUNT(*) 
FROM `gcstemporary` 
WHERE (@target_month IS NULL OR `Month` = @target_month)
  AND (@target_year  IS NULL OR `Year`  = @target_year);

