-- =========================================================================
-- SCRIPT SQL: DỌN DẸP CÁC BẢN GHI RÁC DO IMPORT NHẦM TIÊU ĐỀ FILE BÁO CÁO
-- Dự án: MDMSv2 (Hệ thống Quản lý Dữ liệu Đo đếm Công tơ Điện tử)
-- Cơ sở dữ liệu: amr
-- Mục đích: Xóa triệt để các bản ghi tiêu đề banner bị insert nhầm vào bảng Customers và MeasurementPoints
-- =========================================================================

USE amr;

-- Tắt chế độ Safe Update trong MySQL Workbench
SET SQL_SAFE_UPDATES = 0;

-- 1. Xem trước các bản ghi rác trong bảng Customers (Khách hàng)
SELECT Id, CustomerCode, FullName, PhoneNumber, Email, Address, CreatedTime
FROM Customers
WHERE CustomerCode IN ('Mã KH', 'QUẢN LÝ KHÁCH HÀNG')
   OR CustomerCode LIKE 'Thời gian xuất%'
   OR CustomerCode LIKE 'HỆ THỐNG QUẢN LÝ%';

-- 2. Xóa các bản ghi rác trong bảng Customers
DELETE FROM Customers
WHERE CustomerCode IN ('Mã KH', 'QUẢN LÝ KHÁCH HÀNG')
   OR CustomerCode LIKE 'Thời gian xuất%'
   OR CustomerCode LIKE 'HỆ THỐNG QUẢN LÝ%';

-- 3. Xem trước các bản ghi rác trong bảng MeasurementPoints (Điểm đo) nếu có
SELECT Id, Code, Description, Type, CustomerId, CreatedTime
FROM MeasurementPoints
WHERE Code IN ('Mã điểm đo', 'QUẢN LÝ ĐIỂM ĐO')
   OR Code LIKE 'Thời gian xuất%'
   OR Code LIKE 'HỆ THỐNG QUẢN LÝ%';

-- 4. Xóa các bản ghi rác trong bảng MeasurementPoints nếu có
DELETE FROM MeasurementPoints
WHERE Code IN ('Mã điểm đo', 'QUẢN LÝ ĐIỂM ĐO')
   OR Code LIKE 'Thời gian xuất%'
   OR Code LIKE 'HỆ THỐNG QUẢN LÝ%';

-- Bật lại chế độ Safe Update
SET SQL_SAFE_UPDATES = 1;

-- Kiểm tra lại số lượng bản ghi sau khi dọn dẹp
SELECT 'Customers' AS Bang, COUNT(1) AS TongSoBanGhi FROM Customers WHERE Deleted = 0
UNION ALL
SELECT 'MeasurementPoints' AS Bang, COUNT(1) AS TongSoBanGhi FROM MeasurementPoints WHERE Deleted = 0;
