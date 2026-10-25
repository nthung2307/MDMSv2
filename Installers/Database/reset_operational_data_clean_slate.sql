-- =====================================================================================================
-- SCRIPT RESET DỮ LIỆU VẬN HÀNH, CHỈ SỐ, SẢN LƯỢNG & PHIẾU BÁO ĐỂ KIỂM THỬ MÔI TRƯỜNG "TRẮNG"
-- Dự án: MDMSv2 (Meter Data Management System v2)
-- Cơ sở dữ liệu: amr (MySQL)
-- 
-- MỤC ĐÍCH:
-- 1. GIỮ LẠI NGUYÊN VẸN CÁC DỮ LIỆU CỐ ĐỊNH / CẤU HÌNH (MASTER DATA):
--    - Khách hàng (`customers`)
--    - Sổ GCS (`gcs`)
--    - Điểm đo, Hệ số nhân, Liên kết Khách hàng & Sổ GCS (`measurementpoints`)
--    - Cấu hình liên kết dữ liệu / Danh mục thanh ghi (`tagname`)
--    - Tài khoản người dùng, phân quyền (`users`, `roles`, `userroles`, `rolepermissions`, `usercustomers`)
--    - Cấu hình hệ thống & phiên đăng nhập (`systemconfig`, `usertokens`, `__efmigrationshistory`)
--
-- 2. XÓA TOÀN BỘ DỮ LIỆU PHÁT SINH / VẬN HÀNH / TÍNH TOÁN:
--    - Báo cáo Chỉ số điện chu kỳ 30 phút (`measurementpointhalfhourenergyreports`)
--    - Báo cáo Thông số vận hành chu kỳ 30 phút (`measurementpointhalfhouroperatingreports`)
--    - Báo cáo Sản lượng điện / chỉ số ngày (`measurementpointdailyenergyreports`)
--    - Dữ liệu đo đếm theo giờ (`measurementpointmonitoringhourly`)
--    - Dữ liệu đo đếm tức thời / đệm / trạng thái (`measurementpointmonitoring`, `measurementpointmonitoringbydatetime`, `measurementpointmonitoringbystatus`, `archivedatameasurementpointmonitoring`)
--    - Quản lý Phiếu báo điện năng (`periodicenergystatement`)
--    - Dữ liệu tính toán & Kỳ chốt Sổ GCS cũ (`gcscutoffcycle`, `gcsmonthlydetail`, `gcstemporary`)
--    - Sự kiện & Cảnh báo (`measurementpointevents`, `measurementpointalerts`)
--    - Lịch sử tiến trình Hangfire jobs (tùy chọn reset sạch sẽ hàng đợi và log job cũ)
-- =====================================================================================================

USE `amr`;

-- 1. Tạm thời vô hiệu hóa kiểm tra khóa ngoại (Foreign Key Checks)
SET @OLD_FOREIGN_KEY_CHECKS = @@FOREIGN_KEY_CHECKS;
SET FOREIGN_KEY_CHECKS = 0;

SET @OLD_SQL_SAFE_UPDATES = @@SQL_SAFE_UPDATES;
SET SQL_SAFE_UPDATES = 0;

START TRANSACTION;

-- =====================================================================================================
-- BƯỚC 1: XÓA DỮ LIỆU BÁO CÁO ĐO ĐẾM & VẬN HÀNH
-- =====================================================================================================

-- 1.1. Xóa dữ liệu Chỉ số điện nửa giờ (Báo cáo "Chỉ số điện" & "Chỉ số điện các điểm đo")
TRUNCATE TABLE `measurementpointhalfhourenergyreports`;

-- 1.2. Xóa dữ liệu Thông số vận hành nửa giờ (Báo cáo "Thông số vận hành")
TRUNCATE TABLE `measurementpointhalfhouroperatingreports`;

-- 1.3. Xóa dữ liệu Sản lượng điện & Chỉ số ngày chốt 0h (Báo cáo "Sản lượng điện")
TRUNCATE TABLE `measurementpointdailyenergyreports`;

-- 1.4. Xóa dữ liệu đo đếm tổng hợp theo giờ
TRUNCATE TABLE `measurementpointmonitoringhourly`;

-- 1.5. Xóa dữ liệu đo đếm tức thời, đệm thời gian, trạng thái kết nối & dữ liệu lưu trữ
TRUNCATE TABLE `measurementpointmonitoring`;
TRUNCATE TABLE `measurementpointmonitoringbydatetime`;
TRUNCATE TABLE `measurementpointmonitoringbystatus`;
TRUNCATE TABLE `archivedatameasurementpointmonitoring`;

-- =====================================================================================================
-- BƯỚC 2: XÓA DỮ LIỆU PHIẾU BÁO & DỮ LIỆU TÍNH TOÁN SỔ GCS
-- =====================================================================================================

-- 2.1. Xóa toàn bộ phiếu báo điện năng (Trang "Quản lý phiếu báo")
TRUNCATE TABLE `periodicenergystatement`;

-- 2.2. Xóa chi tiết chốt chỉ số sổ GCS hàng tháng
TRUNCATE TABLE `gcsmonthlydetail`;

-- 2.3. Xóa dữ liệu tạm tính sổ GCS
TRUNCATE TABLE `gcstemporary`;

-- 2.4. Xóa toàn bộ kỳ chốt sổ GCS cũ (để người dùng tạo mới trên môi trường trắng)
TRUNCATE TABLE `gcscutoffcycle`;

-- LƯU Ý: Danh mục Sổ GCS (`gcs`) và liên kết `measurementpoints.GcsId` được GIỮ LẠI NGUYÊN VẸN.

-- =====================================================================================================
-- BƯỚC 3: XÓA DỮ LIỆU SỰ KIỆN & CẢNH BÁO
-- =====================================================================================================

-- 3.1. Xóa toàn bộ bảng sự kiện điểm đo (Trang "Bảng sự kiện")
TRUNCATE TABLE `measurementpointevents`;

-- 3.2. Xóa toàn bộ bảng cảnh báo điểm đo (Trang "Bảng cảnh báo")
TRUNCATE TABLE `measurementpointalerts`;

-- =====================================================================================================
-- BƯỚC 4: RESET HÀNG ĐỢI & LỊCH SỬ HANGFIRE JOBS CŨ (TÙY CHỌN DỌN SẠCH)
-- =====================================================================================================
TRUNCATE TABLE `hangfirejobstate`;
TRUNCATE TABLE `hangfirejobparameter`;
TRUNCATE TABLE `hangfirejobqueue`;
TRUNCATE TABLE `hangfirestate`;
TRUNCATE TABLE `hangfirejob`;
TRUNCATE TABLE `hangfirecounter`;
TRUNCATE TABLE `hangfireaggregatedcounter`;
TRUNCATE TABLE `hangfirelist`;
TRUNCATE TABLE `hangfireset`;
TRUNCATE TABLE `hangfirehash`;
TRUNCATE TABLE `hangfiredistributedlock`;
TRUNCATE TABLE `hangfireserver`;

COMMIT;

-- 5. Bật lại kiểm tra khóa ngoại và Safe Updates
SET FOREIGN_KEY_CHECKS = @OLD_FOREIGN_KEY_CHECKS;
SET SQL_SAFE_UPDATES = @OLD_SQL_SAFE_UPDATES;

-- =====================================================================================================
-- BƯỚC 5: KIỂM TRA LẠI SỐ LƯỢNG BẢN GHI SAU KHI DỌN DẸP
-- =====================================================================================================

SELECT '=== 1. DỮ LIỆU ĐƯỢC BẢO TOÀN (PHẢI CÒN NGUYÊN) ===' AS `KetQuaKiemTra`;
SELECT 'customers (Khách hàng)' AS `TenBang`, COUNT(*) AS `SoLuongBanGhi` FROM `customers`
UNION ALL
SELECT 'gcs (Sổ GCS)', COUNT(*) FROM `gcs`
UNION ALL
SELECT 'measurementpoints (Tổng điểm đo)', COUNT(*) FROM `measurementpoints`
UNION ALL
SELECT '  -> Điểm đo đã liên kết Khách hàng (CustomerId)', COUNT(*) FROM `measurementpoints` WHERE `CustomerId` IS NOT NULL AND `CustomerId` > 0
UNION ALL
SELECT '  -> Điểm đo đã liên kết Sổ GCS (GcsId)', COUNT(*) FROM `measurementpoints` WHERE `GcsId` IS NOT NULL AND `GcsId` > 0
UNION ALL
SELECT 'tagname (Liên kết thông số / tags)', COUNT(*) FROM `tagname`
UNION ALL
SELECT 'users (Tài khoản người dùng)', COUNT(*) FROM `users`
UNION ALL
SELECT 'usercustomers (Phân quyền người dùng)', COUNT(*) FROM `usercustomers`
UNION ALL
SELECT 'systemconfig (Cấu hình hệ thống)', COUNT(*) FROM `systemconfig`;

SELECT '=== 2. DỮ LIỆU VẬN HÀNH & BÁO CÁO ĐÃ XÓA (TẤT CẢ PHẢI = 0) ===' AS `KetQuaKiemTra`;
SELECT 'measurementpointhalfhourenergyreports (Chỉ số nửa giờ)' AS `TenBang`, COUNT(*) AS `SoLuong` FROM `measurementpointhalfhourenergyreports`
UNION ALL
SELECT 'measurementpointhalfhouroperatingreports (Thông số vận hành)', COUNT(*) FROM `measurementpointhalfhouroperatingreports`
UNION ALL
SELECT 'measurementpointdailyenergyreports (Sản lượng / chỉ số ngày)', COUNT(*) FROM `measurementpointdailyenergyreports`
UNION ALL
SELECT 'measurementpointmonitoringhourly (Đo đếm giờ)', COUNT(*) FROM `measurementpointmonitoringhourly`
UNION ALL
SELECT 'measurementpointmonitoring (Đo đếm tức thời)', COUNT(*) FROM `measurementpointmonitoring`
UNION ALL
SELECT 'periodicenergystatement (Phiếu báo)', COUNT(*) FROM `periodicenergystatement`
UNION ALL
SELECT 'gcscutoffcycle (Kỳ chốt GCS cũ)', COUNT(*) FROM `gcscutoffcycle`
UNION ALL
SELECT 'gcsmonthlydetail (Chi tiết GCS tháng)', COUNT(*) FROM `gcsmonthlydetail`
UNION ALL
SELECT 'gcstemporary (Tạm tính GCS)', COUNT(*) FROM `gcstemporary`
UNION ALL
SELECT 'measurementpointevents (Sự kiện)', COUNT(*) FROM `measurementpointevents`
UNION ALL
SELECT 'measurementpointalerts (Cảnh báo)', COUNT(*) FROM `measurementpointalerts`;
