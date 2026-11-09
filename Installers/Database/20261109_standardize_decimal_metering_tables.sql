-- ==============================================================================
-- MDMSv2 Migration: Standardize Energy & Metering Data Types to DECIMAL(18, 4) & DECIMAL(5, 2)
-- Date: 2026-11-09
-- Purpose:
--   1. Chuyển đổi toàn bộ số liệu đo đếm, chỉ số công tơ, hệ số nhân và sản lượng
--      tính tiền từ kiểu số thực (DOUBLE) sang số thập phân chuẩn xác cao DECIMAL(18, 4).
--   2. Chuẩn hóa hệ số công suất trung bình Cosφ (PF) sang DECIMAL(5, 2).
--   3. Đảm bảo tính toàn vẹn 100% của dữ liệu đo đếm và triệt tiêu sai số dấu phẩy động
--      (floating-point drift) trong lập hóa đơn tiền điện EVN.
-- ==============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ------------------------------------------------------------------------------
-- 1. Bảng `measurementpoints` (Điểm đo)
-- ------------------------------------------------------------------------------
ALTER TABLE `measurementpoints` 
    MODIFY COLUMN `MultiplierFactor` DECIMAL(18, 4) NOT NULL DEFAULT 1.0000;

-- ------------------------------------------------------------------------------
-- 2. Bảng `gcstemporary` (Số liệu tạm ghi chỉ số trước chốt)
-- ------------------------------------------------------------------------------
ALTER TABLE `gcstemporary`
    MODIFY COLUMN `PreviousReading` DECIMAL(18, 4) DEFAULT NULL,
    MODIFY COLUMN `CurrentReading` DECIMAL(18, 4) DEFAULT NULL,
    MODIFY COLUMN `NewCurrentReading` DECIMAL(18, 4) DEFAULT NULL,
    MODIFY COLUMN `NewPreviousReading` DECIMAL(18, 4) DEFAULT NULL,
    MODIFY COLUMN `MultiplierFactor` DECIMAL(18, 4) NOT NULL DEFAULT 1.0000,
    MODIFY COLUMN `Consumption` DECIMAL(18, 4) DEFAULT NULL,
    MODIFY COLUMN `CosPhi` DECIMAL(5, 2) DEFAULT NULL;

-- ------------------------------------------------------------------------------
-- 3. Bảng `gcsmonthlydetail` (Chi tiết chỉ số kỳ chốt đã lưu trữ)
-- ------------------------------------------------------------------------------
ALTER TABLE `gcsmonthlydetail`
    MODIFY COLUMN `PreviousReading` DECIMAL(18, 4) DEFAULT NULL,
    MODIFY COLUMN `CurrentReading` DECIMAL(18, 4) DEFAULT NULL,
    MODIFY COLUMN `MultiplierFactor` DECIMAL(18, 4) NOT NULL DEFAULT 1.0000,
    MODIFY COLUMN `Consumption` DECIMAL(18, 4) DEFAULT NULL,
    MODIFY COLUMN `CosPhi` DECIMAL(5, 2) DEFAULT NULL;

-- ------------------------------------------------------------------------------
-- 4. Bảng `periodicenergystatement` (Phiếu báo chỉ số điện năng & sản lượng định kỳ)
-- ------------------------------------------------------------------------------
ALTER TABLE `periodicenergystatement`
    MODIFY COLUMN `Bt` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `Cd` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `Td` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `Sg` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `Vc` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `Pmax` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `PreviousBt` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `PreviousCd` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `PreviousTd` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `PreviousSg` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `PreviousVc` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `PreviousPmax` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `ConsumtionBt` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `ConsumtionCd` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `ConsumtionTd` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `ConsumtionSg` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `ConsumtionVc` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `Multiplier` DECIMAL(18, 4) NOT NULL DEFAULT 1.0000,
    MODIFY COLUMN `CosPhi` DECIMAL(5, 2) DEFAULT NULL;

-- ------------------------------------------------------------------------------
-- 5. Bảng `meterreplacements` (Biên bản treo tháo & thay thế công tơ)
-- ------------------------------------------------------------------------------
ALTER TABLE `meterreplacements`
    MODIFY COLUMN `OldMeterMultiplier` DECIMAL(18, 4) NOT NULL DEFAULT 1.0000,
    MODIFY COLUMN `OldBt` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `OldCd` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `OldTd` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `OldSg` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `OldVc` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `OldPmax` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `NewMeterMultiplier` DECIMAL(18, 4) NOT NULL DEFAULT 1.0000,
    MODIFY COLUMN `NewBt` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `NewCd` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `NewTd` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `NewSg` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `NewVc` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000,
    MODIFY COLUMN `NewPmax` DECIMAL(18, 4) NOT NULL DEFAULT 0.0000;

SET FOREIGN_KEY_CHECKS = 1;

