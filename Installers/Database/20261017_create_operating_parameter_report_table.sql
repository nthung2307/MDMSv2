-- ====================================================================
-- Script tao bang MeasurementPointHalfHourOperatingReports cho MDMSv2
-- Dung de luu tru thong so van hanh chu ky 30 phut: Ua, Ub, Uc, Ia, Ib, Ic, P, Q, PF, F
-- ====================================================================

USE `amr`;

CREATE TABLE IF NOT EXISTS `MeasurementPointHalfHourOperatingReports` (
  `Id` bigint NOT NULL,
  `MeasurementPointId` bigint NOT NULL,
  `CreatedTime` datetime(6) DEFAULT NULL,
  `DienApPhaA` double DEFAULT NULL,
  `DienApPhaB` double DEFAULT NULL,
  `DienApPhaC` double DEFAULT NULL,
  `DongDienPhaA` double DEFAULT NULL,
  `DongDienPhaB` double DEFAULT NULL,
  `DongDienPhaC` double DEFAULT NULL,
  `CongSuatTacDungTong3Pha` double DEFAULT NULL,
  `CongSuatPhanKhangTong3Pha` double DEFAULT NULL,
  `HeSoCongSuatPF` double DEFAULT NULL,
  `TanSoHeThong` double DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MeasurementPointHalfHourOperatingReports_MeasurementPointId` (`MeasurementPointId`),
  KEY `IX_MeasurementPointHalfHourOperatingReports_CreatedTime` (`CreatedTime`),
  CONSTRAINT `FK_MeasurementPointHalfHourOperatingReports_MeasurementPoints` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
