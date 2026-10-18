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
  `DienApPhaAUnit` longtext,
  `DienApPhaB` double DEFAULT NULL,
  `DienApPhaBUnit` longtext,
  `DienApPhaC` double DEFAULT NULL,
  `DienApPhaCUnit` longtext,
  `DongDienPhaA` double DEFAULT NULL,
  `DongDienPhaAUnit` longtext,
  `DongDienPhaB` double DEFAULT NULL,
  `DongDienPhaBUnit` longtext,
  `DongDienPhaC` double DEFAULT NULL,
  `DongDienPhaCUnit` longtext,
  `CongSuatTacDungTong3Pha` double DEFAULT NULL,
  `CongSuatTacDungTong3PhaUnit` longtext,
  `CongSuatPhanKhangTong3Pha` double DEFAULT NULL,
  `CongSuatPhanKhangTong3PhaUnit` longtext,
  `HeSoCongSuatPF` double DEFAULT NULL,
  `TanSoHeThong` double DEFAULT NULL,
  `TanSoHeThongUnit` longtext,
  PRIMARY KEY (`Id`),
  KEY `IX_MeasurementPointHalfHourOperatingReports_MeasurementPointId` (`MeasurementPointId`),
  KEY `IX_MeasurementPointHalfHourOperatingReports_CreatedTime` (`CreatedTime`),
  CONSTRAINT `FK_MeasurementPointHalfHourOperatingReports_MeasurementPoints` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
