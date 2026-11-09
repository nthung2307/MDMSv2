CREATE DATABASE  IF NOT EXISTS `amr` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `amr`;
-- MySQL dump 10.13  Distrib 8.0.41, for Win64 (x86_64)
--
-- Host: localhost    Database: amr
-- ------------------------------------------------------
-- Server version	8.0.41

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `archivedatameasurementpointmonitoring`
--

DROP TABLE IF EXISTS `archivedatameasurementpointmonitoring`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `archivedatameasurementpointmonitoring` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `MeasurementPointsId` bigint NOT NULL,
  `TagNameId` bigint DEFAULT NULL,
  `TagNameDescription` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Quanlity` tinyint(1) DEFAULT NULL,
  `GroupId` bigint DEFAULT NULL,
  `CreatedTime` datetime(6) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `CreatedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_ArchiveDataMeasurementPointMonitoring_MeasurementPointsId` (`MeasurementPointsId`),
  KEY `IX_ArchiveDataMeasurementPointMonitoring_TagNameId` (`TagNameId`),
  KEY `idx_archivedatameasurementpointmonitoring_CreatedTime` (`CreatedTime`),
  CONSTRAINT `FK_ArchiveDataMeasurementPointMonitoring_MeasurementPoints_Meas~` FOREIGN KEY (`MeasurementPointsId`) REFERENCES `measurementpoints` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ArchiveDataMeasurementPointMonitoring_TagName_TagNameId` FOREIGN KEY (`TagNameId`) REFERENCES `tagname` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `customers`
--

DROP TABLE IF EXISTS `customers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `customers` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `CustomerCode` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `FullName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `PhoneNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Address` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Email` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Status` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `CreatedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CreatedTime` datetime(6) DEFAULT NULL,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  `ModifiedBy` bigint DEFAULT NULL,
  `ModifiedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Deleted` tinyint(1) NOT NULL,
  `DeletedTime` datetime(6) DEFAULT NULL,
  `DeletedBy` bigint DEFAULT NULL,
  `DeletedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_Customers_ModifiedBy` (`ModifiedBy`),
  KEY `IX_Customers_Status` (`Status`)
) ENGINE=InnoDB AUTO_INCREMENT=1496298269417473 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `gcs`
--

DROP TABLE IF EXISTS `gcs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `gcs` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `Code` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `PeriodNumber` int NOT NULL,
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Status` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedTime` datetime(6) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `CreatedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  `ModifiedBy` bigint DEFAULT NULL,
  `ModifiedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Deleted` tinyint(1) NOT NULL,
  `DeletedTime` datetime(6) DEFAULT NULL,
  `DeletedBy` bigint DEFAULT NULL,
  `DeletedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `DayPeriod` json NOT NULL,
  `IsNewChange` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `IX_Gcs_ModifiedBy` (`ModifiedBy`)
) ENGINE=InnoDB AUTO_INCREMENT=1460300846120961 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `gcscutoffcycle`
--

DROP TABLE IF EXISTS `gcscutoffcycle`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `gcscutoffcycle` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `GcsId` bigint NOT NULL,
  `Month` int NOT NULL,
  `Year` int NOT NULL,
  `PeriodNumber` int NOT NULL,
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CreatedTime` datetime(6) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `CreatedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Day` int NOT NULL DEFAULT '0',
  `PreviousDay` int DEFAULT NULL,
  `PreviousMonth` int DEFAULT NULL,
  `PreviousPeriodNumber` int DEFAULT NULL,
  `PreviousYear` int DEFAULT NULL,
  `IsSendingMail` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `IX_GcsCutoffCycle_GcsId` (`GcsId`),
  CONSTRAINT `FK_GcsCutoffCycle_Gcs_GcsId` FOREIGN KEY (`GcsId`) REFERENCES `gcs` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1530019138371585 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `gcsmonthlydetail`
--

DROP TABLE IF EXISTS `gcsmonthlydetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `gcsmonthlydetail` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `GcsCutoffCycleId` bigint NOT NULL,
  `MeasurementPointId` bigint NOT NULL,
  `MeterReadingSet` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `PreviousReading` double DEFAULT NULL,
  `CurrentReading` double DEFAULT NULL,
  `MultiplierFactor` double NOT NULL,
  `Consumption` double DEFAULT NULL,
  `IsChange` tinyint(1) NOT NULL,
  `CreatedTime` datetime(6) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `CreatedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_GcsMonthlyDetail_GcsCutoffCycleId` (`GcsCutoffCycleId`),
  KEY `IX_GcsMonthlyDetail_MeasurementPointId` (`MeasurementPointId`),
  CONSTRAINT `FK_GcsMonthlyDetail_GcsCutoffCycle_GcsCutoffCycleId` FOREIGN KEY (`GcsCutoffCycleId`) REFERENCES `gcscutoffcycle` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_GcsMonthlyDetail_MeasurementPoints_MeasurementPointId` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1530019138543634 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `gcstemporary`
--

DROP TABLE IF EXISTS `gcstemporary`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `gcstemporary` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `GcsId` bigint NOT NULL,
  `MeasurementPointId` bigint NOT NULL,
  `Month` int NOT NULL,
  `Year` int NOT NULL,
  `PeriodNumber` int NOT NULL,
  `MeterReadingSet` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `PreviousReading` double DEFAULT NULL,
  `CurrentReading` double DEFAULT NULL,
  `Quanlity` tinyint(1) DEFAULT NULL,
  `NewCurrentReading` double DEFAULT NULL,
  `MultiplierFactor` double NOT NULL,
  `Consumption` double DEFAULT NULL,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  `ModifiedBy` bigint DEFAULT NULL,
  `ModifiedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ReadingTime` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  `Day` int NOT NULL DEFAULT '0',
  `PreviousDay` int DEFAULT NULL,
  `PreviousMonth` int DEFAULT NULL,
  `PreviousPeriodNumber` int DEFAULT NULL,
  `PreviousYear` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_GcsTemporary_GcsId` (`GcsId`),
  KEY `IX_GcsTemporary_MeasurementPointId` (`MeasurementPointId`),
  CONSTRAINT `FK_GcsTemporary_Gcs_GcsId` FOREIGN KEY (`GcsId`) REFERENCES `gcs` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_GcsTemporary_MeasurementPoints_MeasurementPointId` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1529148017221651 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hangfireaggregatedcounter`
--

DROP TABLE IF EXISTS `hangfireaggregatedcounter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hangfireaggregatedcounter` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Key` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Value` int NOT NULL,
  `ExpireAt` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_HangfireCounterAggregated_Key` (`Key`)
) ENGINE=InnoDB AUTO_INCREMENT=76225 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hangfirecounter`
--

DROP TABLE IF EXISTS `hangfirecounter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hangfirecounter` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Key` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Value` int NOT NULL,
  `ExpireAt` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_HangfireCounter_Key` (`Key`)
) ENGINE=InnoDB AUTO_INCREMENT=211128 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hangfiredistributedlock`
--

DROP TABLE IF EXISTS `hangfiredistributedlock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hangfiredistributedlock` (
  `Resource` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `CreatedAt` datetime(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hangfirehash`
--

DROP TABLE IF EXISTS `hangfirehash`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hangfirehash` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Key` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Field` varchar(40) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Value` longtext,
  `ExpireAt` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_HangfireHash_Key_Field` (`Key`,`Field`)
) ENGINE=InnoDB AUTO_INCREMENT=18180985 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hangfirejob`
--

DROP TABLE IF EXISTS `hangfirejob`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hangfirejob` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `StateId` int DEFAULT NULL,
  `StateName` varchar(20) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `InvocationData` longtext NOT NULL,
  `Arguments` longtext NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `ExpireAt` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_HangfireJob_StateName` (`StateName`),
  KEY `idx_expireat` (`ExpireAt`)
) ENGINE=InnoDB AUTO_INCREMENT=72840 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hangfirejobparameter`
--

DROP TABLE IF EXISTS `hangfirejobparameter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hangfirejobparameter` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `JobId` int NOT NULL,
  `Name` varchar(40) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Value` longtext,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_HangfireJobParameter_JobId_Name` (`JobId`,`Name`),
  KEY `FK_HangfireJobParameter_Job` (`JobId`),
  CONSTRAINT `FK_HangfireJobParameter_Job` FOREIGN KEY (`JobId`) REFERENCES `hangfirejob` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=112594 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hangfirejobqueue`
--

DROP TABLE IF EXISTS `hangfirejobqueue`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hangfirejobqueue` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `JobId` int NOT NULL,
  `FetchedAt` datetime(6) DEFAULT NULL,
  `Queue` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `FetchToken` varchar(36) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_HangfireJobQueue_QueueAndFetchedAt` (`Queue`,`FetchedAt`)
) ENGINE=InnoDB AUTO_INCREMENT=74905 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hangfirejobstate`
--

DROP TABLE IF EXISTS `hangfirejobstate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hangfirejobstate` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `JobId` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `Name` varchar(20) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Reason` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `Data` longtext,
  PRIMARY KEY (`Id`),
  KEY `FK_HangfireJobState_Job` (`JobId`),
  CONSTRAINT `FK_HangfireJobState_Job` FOREIGN KEY (`JobId`) REFERENCES `hangfirejob` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hangfirelist`
--

DROP TABLE IF EXISTS `hangfirelist`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hangfirelist` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Key` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Value` longtext,
  `ExpireAt` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hangfireserver`
--

DROP TABLE IF EXISTS `hangfireserver`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hangfireserver` (
  `Id` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Data` longtext NOT NULL,
  `LastHeartbeat` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hangfireset`
--

DROP TABLE IF EXISTS `hangfireset`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hangfireset` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Key` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Value` varchar(256) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Score` float NOT NULL,
  `ExpireAt` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_HangfireSet_Key_Value` (`Key`,`Value`)
) ENGINE=InnoDB AUTO_INCREMENT=56637088 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `hangfirestate`
--

DROP TABLE IF EXISTS `hangfirestate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hangfirestate` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `JobId` int NOT NULL,
  `Name` varchar(20) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Reason` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `Data` longtext,
  PRIMARY KEY (`Id`),
  KEY `FK_HangfireHangFire_State_Job` (`JobId`),
  CONSTRAINT `FK_HangfireHangFire_State_Job` FOREIGN KEY (`JobId`) REFERENCES `hangfirejob` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=226744 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `measurementpointalerts`
--

DROP TABLE IF EXISTS `measurementpointalerts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `measurementpointalerts` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `MeasurementPointId` bigint NOT NULL,
  `TagNameId` bigint NOT NULL,
  `EventName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Status` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedTime` datetime(6) NOT NULL,
  `Quanlity` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MeasurementPointAlerts_MeasurementPointId` (`MeasurementPointId`),
  KEY `IX_MeasurementPointAlerts_TagNameId` (`TagNameId`),
  KEY `idx_measurementpointalerts_CreatedTime` (`CreatedTime`),
  CONSTRAINT `FK_MeasurementPointAlerts_MeasurementPoints_MeasurementPointId` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_MeasurementPointAlerts_TagName_TagNameId` FOREIGN KEY (`TagNameId`) REFERENCES `tagname` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1531934460559362 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `measurementpointdailyenergyreports`
--

DROP TABLE IF EXISTS `measurementpointdailyenergyreports`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `measurementpointdailyenergyreports` (
  `Id` bigint NOT NULL,
  `ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiaoUnit` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiaoUnit` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiaoUnit` longtext,
  `ChiSoTichLuyDienNangTacDungAPTongKenhGiao` longtext,
  `ChiSoTichLuyDienNangTacDungAPTongKenhGiaoUnit` longtext,
  `ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao` longtext,
  `ChiSoTichLuyDienNangPhanKhangAQTongKenhGiaoUnit` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhanUnit` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhanUnit` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhanUnit` longtext,
  `ChiSoTichLuyDienNangTacDungAPTongKenhNhan` longtext,
  `ChiSoTichLuyDienNangTacDungAPTongKenhNhanUnit` longtext,
  `ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan` longtext,
  `ChiSoTichLuyDienNangPhanKhangAQTongKenhNhanUnit` longtext,
  `MeasurementPointId` bigint NOT NULL,
  `CreatedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `MeasurementPointDailyEnergyReports_CreatedTime_index` (`CreatedTime`),
  KEY `MeasurementPointDailyEnergyReports_MeasurementPointId_index` (`MeasurementPointId`),
  KEY `MeasurementPointHalfHourEnergyReports_CreatedTime_index` (`CreatedTime`),
  KEY `MeasurementPointHalfHourEnergyReports_MeasurementPointId_index` (`MeasurementPointId`),
  CONSTRAINT `MeasurementPointDailyEnergyReports_MeasurementPoints_Id_fk` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `measurementpointevents`
--

DROP TABLE IF EXISTS `measurementpointevents`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `measurementpointevents` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `MeasurementPointId` bigint NOT NULL,
  `TagNameId` bigint NOT NULL,
  `EventName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Status` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CreatedTime` datetime(6) NOT NULL,
  `Quanlity` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MeasurementPointEvents_MeasurementPointId` (`MeasurementPointId`),
  KEY `IX_MeasurementPointEvents_TagNameId` (`TagNameId`),
  KEY `idx_measurementpointevents_CreatedTime` (`CreatedTime`),
  CONSTRAINT `FK_MeasurementPointEvents_MeasurementPoints_MeasurementPointId` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_MeasurementPointEvents_TagName_TagNameId` FOREIGN KEY (`TagNameId`) REFERENCES `tagname` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1531934460559364 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `measurementpointhalfhourenergyreports`
--

DROP TABLE IF EXISTS `measurementpointhalfhourenergyreports`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `measurementpointhalfhourenergyreports` (
  `Id` bigint NOT NULL,
  `ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiaoUnit` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiaoUnit` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiaoUnit` longtext,
  `ChiSoTichLuyDienNangTacDungAPTongKenhGiao` longtext,
  `ChiSoTichLuyDienNangTacDungAPTongKenhGiaoUnit` longtext,
  `ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao` longtext,
  `ChiSoTichLuyDienNangPhanKhangAQTongKenhGiaoUnit` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhanUnit` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhanUnit` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan` longtext,
  `ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhanUnit` longtext,
  `ChiSoTichLuyDienNangTacDungAPTongKenhNhan` longtext,
  `ChiSoTichLuyDienNangTacDungAPTongKenhNhanUnit` longtext,
  `ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan` longtext,
  `ChiSoTichLuyDienNangPhanKhangAQTongKenhNhanUnit` longtext,
  `MeasurementPointId` bigint NOT NULL,
  `CreatedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `MeasurementPointHalfHourEnergyReports_MeasurementPoints_Id_fk` (`MeasurementPointId`),
  CONSTRAINT `MeasurementPointHalfHourEnergyReports_MeasurementPoints_Id_fk` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `measurementpointhalfhouroperatingreports`
--

DROP TABLE IF EXISTS `measurementpointhalfhouroperatingreports`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `measurementpointhalfhouroperatingreports` (
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `measurementpointmonitoring`
--

DROP TABLE IF EXISTS `measurementpointmonitoring`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `measurementpointmonitoring` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `MeasurementPointsId` bigint NOT NULL,
  `TagNameId` bigint DEFAULT NULL,
  `TagNameDescription` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `GroupId` bigint DEFAULT NULL,
  `CreatedTime` datetime(6) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `CreatedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Quanlity` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MeasurementPointMonitoring_MeasurementPointsId` (`MeasurementPointsId`),
  KEY `IX_MeasurementPointMonitoring_TagNameId` (`TagNameId`),
  KEY `idx_measurementpointmonitoring_CreatedTime` (`CreatedTime`)
) ENGINE=InnoDB AUTO_INCREMENT=1531938977562634 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `measurementpointmonitoringbydatetime`
--

DROP TABLE IF EXISTS `measurementpointmonitoringbydatetime`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `measurementpointmonitoringbydatetime` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `MeasurementPointId` bigint NOT NULL,
  `TagNameId` bigint DEFAULT NULL,
  `TagNameDescription` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Quanlity` tinyint(1) DEFAULT NULL,
  `GroupId` bigint DEFAULT NULL,
  `CreatedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MeasurementPointMonitoringByDateTime_MeasurementPointId` (`MeasurementPointId`),
  KEY `IX_MeasurementPointMonitoringByDateTime_TagNameId` (`TagNameId`),
  KEY `idx_measurementpointmonitoringbydatetime_CreatedTime` (`CreatedTime`),
  CONSTRAINT `FK_MeasurementPointMonitoringByDateTime_MeasurementPoints_Measu~` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_MeasurementPointMonitoringByDateTime_TagName_TagNameId` FOREIGN KEY (`TagNameId`) REFERENCES `tagname` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=1531938962432024 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `measurementpointmonitoringbystatus`
--

DROP TABLE IF EXISTS `measurementpointmonitoringbystatus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `measurementpointmonitoringbystatus` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `MeasurementPointId` bigint NOT NULL,
  `TagNameId` bigint DEFAULT NULL,
  `TagNameDescription` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Quanlity` tinyint(1) DEFAULT NULL,
  `GroupId` bigint DEFAULT NULL,
  `CreatedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MeasurementPointMonitoringByStatus_MeasurementPointId` (`MeasurementPointId`),
  KEY `IX_MeasurementPointMonitoringByStatus_TagNameId` (`TagNameId`),
  KEY `idx_measurementpointmonitoringbystatus_CreatedTime` (`CreatedTime`),
  CONSTRAINT `FK_MeasurementPointMonitoringByStatus_MeasurementPoints_Measure~` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_MeasurementPointMonitoringByStatus_TagName_TagNameId` FOREIGN KEY (`TagNameId`) REFERENCES `tagname` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=66759498 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `measurementpointmonitoringhourly`
--

DROP TABLE IF EXISTS `measurementpointmonitoringhourly`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `measurementpointmonitoringhourly` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `MeasurementPointId` bigint NOT NULL,
  `CreatedTime` datetime(6) DEFAULT NULL,
  `TrungBinhCongSuatTacDungTong3Pha` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TrungBinhDienApPhaAUnit` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TrungBinhDongDienPhaB` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TrungBinhCongSuatPhanKhangTong3PhaUnit` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TrungBinhDienApPhaA` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TrungBinhDongDienPhaAUnit` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TrungBinhDongDienPhaBUnit` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CongSuatPhanKhangTong3PhaCucDai` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TrungBinhDongDienPhaC` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CongSuatPhanKhangTong3PhaCucTieu` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TrungBinhDongDienPhaCUnit` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TrungBinhDienApPhaC` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TrungBinhDienApPhaBUnit` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TrungBinhDienApPhaCUnit` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TrungBinhDienApPhaB` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TrungBinhDongDienPhaA` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TrungBinhCongSuatPhanKhangTong3Pha` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CongSuatTacDungTong3PhaCucDai` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CongSuatTacDungTong3PhaCucTieu` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CongSuatTacDungTong3PhaUnit` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ChiSoTichLuyDienNangTacDungAPBieuGioBT` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ChiSoTichLuyDienNangTacDungAPBieuGioCD` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ChiSoTichLuyDienNangTacDungAPBieuGioTD` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ChiSoTichLuyDienNangTacDungAPTong` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_MeasurementPointMonitoringHourly_MeasurementPointId` (`MeasurementPointId`),
  KEY `MeasurementPointMonitoringHourly_CreatedTime_IDX` (`CreatedTime`) USING BTREE,
  CONSTRAINT `FK_MeasurementPointMonitoringHourly_MeasurementPoints_Measureme~` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1531925567389748 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `measurementpoints`
--

DROP TABLE IF EXISTS `measurementpoints`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `measurementpoints` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `Code` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Type` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Address` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CustomerId` bigint NOT NULL,
  `Email` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `GcsId` bigint DEFAULT NULL,
  `Status` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `MultiplierFactor` double NOT NULL,
  `CreatedTime` datetime(6) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `CreatedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  `ModifiedBy` bigint DEFAULT NULL,
  `ModifiedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Deleted` tinyint(1) NOT NULL,
  `DeletedTime` datetime(6) DEFAULT NULL,
  `DeletedBy` bigint DEFAULT NULL,
  `DeletedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `IsNewChange` tinyint(1) NOT NULL DEFAULT '0',
  `OldGcsId` bigint DEFAULT NULL,
  `Email2` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Email3` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_MeasurementPoints_Code` (`Code`),
  KEY `IX_MeasurementPoints_CustomerId` (`CustomerId`),
  KEY `IX_MeasurementPoints_GcsId` (`GcsId`),
  KEY `IX_MeasurementPoints_ModifiedBy` (`ModifiedBy`),
  CONSTRAINT `FK_MeasurementPoints_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_MeasurementPoints_Gcs_GcsId` FOREIGN KEY (`GcsId`) REFERENCES `gcs` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=1498482837635073 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `meterreplacements`
--

DROP TABLE IF EXISTS `meterreplacements`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `meterreplacements` (
  `Id` bigint NOT NULL,
  `MeasurementPointId` bigint NOT NULL,
  `ReplacementDate` datetime(6) NOT NULL,
  `Reason` varchar(500) DEFAULT NULL,
  `OldMeterSerial` varchar(100) NOT NULL,
  `OldMeterMultiplier` double NOT NULL DEFAULT '1',
  `OldBt` double NOT NULL DEFAULT '0',
  `OldCd` double NOT NULL DEFAULT '0',
  `OldTd` double NOT NULL DEFAULT '0',
  `OldSg` double NOT NULL DEFAULT '0',
  `OldVc` double NOT NULL DEFAULT '0',
  `OldPmax` double NOT NULL DEFAULT '0',
  `NewMeterSerial` varchar(100) NOT NULL,
  `NewMeterMultiplier` double NOT NULL DEFAULT '1',
  `NewBt` double NOT NULL DEFAULT '0',
  `NewCd` double NOT NULL DEFAULT '0',
  `NewTd` double NOT NULL DEFAULT '0',
  `NewSg` double NOT NULL DEFAULT '0',
  `NewVc` double NOT NULL DEFAULT '0',
  `NewPmax` double NOT NULL DEFAULT '0',
  `DocumentUrl` varchar(1000) DEFAULT NULL,
  `Status` varchar(50) NOT NULL DEFAULT 'APPROVED',
  `CreatedTime` datetime(6) NOT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `CreatedName` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MeterReplacements_MeasurementPointId` (`MeasurementPointId`),
  KEY `IX_MeterReplacements_ReplacementDate` (`ReplacementDate`),
  CONSTRAINT `FK_MeterReplacements_MeasurementPoints` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `notificationtemplates`
--

DROP TABLE IF EXISTS `notificationtemplates`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `notificationtemplates` (
  `Id` bigint NOT NULL,
  `Code` varchar(100) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `SubjectTemplate` varchar(500) NOT NULL,
  `BodyHtmlTemplate` longtext NOT NULL,
  `Description` varchar(1000) DEFAULT NULL,
  `IsDefault` tinyint(1) NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedTime` datetime(6) NOT NULL,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_NotificationTemplates_Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `periodicenergystatement`
--

DROP TABLE IF EXISTS `periodicenergystatement`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `periodicenergystatement` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `GcsId` bigint NOT NULL,
  `MeasurementPointId` bigint NOT NULL,
  `Month` int NOT NULL,
  `Year` int NOT NULL,
  `PeriodNumber` int NOT NULL,
  `Bt` double NOT NULL,
  `Cd` double NOT NULL,
  `Td` double NOT NULL,
  `Sg` double NOT NULL,
  `Vc` double NOT NULL,
  `Status` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `SentTime` datetime(6) DEFAULT NULL,
  `CreatedTime` datetime(6) NOT NULL,
  `Multiplier` double NOT NULL DEFAULT '0',
  `PreviousBt` double NOT NULL DEFAULT '0',
  `PreviousCd` double NOT NULL DEFAULT '0',
  `PreviousSg` double NOT NULL DEFAULT '0',
  `PreviousTd` double NOT NULL DEFAULT '0',
  `PreviousVc` double NOT NULL DEFAULT '0',
  `ConsumtionBt` double NOT NULL DEFAULT '0',
  `ConsumtionCd` double NOT NULL DEFAULT '0',
  `ConsumtionSg` double NOT NULL DEFAULT '0',
  `ConsumtionTd` double NOT NULL DEFAULT '0',
  `ConsumtionVc` double NOT NULL DEFAULT '0',
  `Day` int DEFAULT NULL,
  `PreviousDay` int DEFAULT NULL,
  `PreviousMonth` int DEFAULT NULL,
  `PreviousPeriodNumber` int DEFAULT NULL,
  `PreviousYear` int DEFAULT NULL,
  `SerialNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Pmax` double NOT NULL DEFAULT '0',
  `PreviousPmax` double NOT NULL DEFAULT '0',
  `ConsumtionPmax` double NOT NULL DEFAULT '0',
  `PmaxTime` datetime(6) DEFAULT NULL,
  `PreviousPmaxTime` datetime(6) DEFAULT NULL,
  `BillingTimestamp` datetime(6) DEFAULT NULL,
  `HasMeterReplacement` tinyint(1) NOT NULL DEFAULT '0',
  `MeterReplacementId` bigint DEFAULT NULL,
  `StatementPdfTemplateId` bigint DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_PeriodicEnergyStatement_GcsId` (`GcsId`),
  KEY `IX_PeriodicEnergyStatement_MeasurementPointId` (`MeasurementPointId`),
  CONSTRAINT `FK_PeriodicEnergyStatement_Gcs_GcsId` FOREIGN KEY (`GcsId`) REFERENCES `gcs` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_PeriodicEnergyStatement_MeasurementPoints_MeasurementPointId` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1530019139117059 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `notificationtemplates`
--

DROP TABLE IF EXISTS `notificationtemplates`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `notificationtemplates` (
  `Id` bigint NOT NULL,
  `Code` varchar(100) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `SubjectTemplate` varchar(500) NOT NULL,
  `BodyHtmlTemplate` longtext NOT NULL,
  `Description` varchar(1000) DEFAULT NULL,
  `IsDefault` tinyint(1) NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedTime` datetime(6) NOT NULL,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_NotificationTemplates_Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `meterreplacements`
--

DROP TABLE IF EXISTS `meterreplacements`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `meterreplacements` (
  `Id` bigint NOT NULL,
  `MeasurementPointId` bigint NOT NULL,
  `ReplacementDate` datetime(6) NOT NULL,
  `Reason` varchar(500) DEFAULT NULL,
  `OldMeterSerial` varchar(100) NOT NULL,
  `OldMeterMultiplier` double NOT NULL DEFAULT '1',
  `OldBt` double NOT NULL DEFAULT '0',
  `OldCd` double NOT NULL DEFAULT '0',
  `OldTd` double NOT NULL DEFAULT '0',
  `OldSg` double NOT NULL DEFAULT '0',
  `OldVc` double NOT NULL DEFAULT '0',
  `OldPmax` double NOT NULL DEFAULT '0',
  `NewMeterSerial` varchar(100) NOT NULL,
  `NewMeterMultiplier` double NOT NULL DEFAULT '1',
  `NewBt` double NOT NULL DEFAULT '0',
  `NewCd` double NOT NULL DEFAULT '0',
  `NewTd` double NOT NULL DEFAULT '0',
  `NewSg` double NOT NULL DEFAULT '0',
  `NewVc` double NOT NULL DEFAULT '0',
  `NewPmax` double NOT NULL DEFAULT '0',
  `DocumentUrl` varchar(1000) DEFAULT NULL,
  `Status` varchar(50) NOT NULL DEFAULT 'APPROVED',
  `CreatedTime` datetime(6) NOT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `CreatedName` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MeterReplacements_MeasurementPointId` (`MeasurementPointId`),
  KEY `IX_MeterReplacements_ReplacementDate` (`ReplacementDate`),
  CONSTRAINT `FK_MeterReplacements_MeasurementPoints` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `statementpdftemplates`
--

DROP TABLE IF EXISTS `statementpdftemplates`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `statementpdftemplates` (
  `Id` bigint NOT NULL,
  `Code` varchar(100) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `HeaderTitle` varchar(255) DEFAULT NULL,
  `SubHeaderTitle` varchar(255) DEFAULT NULL,
  `CompanyName` varchar(255) DEFAULT NULL,
  `CompanyAddress` varchar(500) DEFAULT NULL,
  `ContactPhone` varchar(255) DEFAULT NULL,
  `LogoBase64` longtext,
  `PaperSize` varchar(50) DEFAULT NULL,
  `DisplayColumnsJson` varchar(1000) DEFAULT NULL,
  `FooterNotes` varchar(2000) DEFAULT NULL,
  `RequireDualSignatures` tinyint(1) NOT NULL DEFAULT '1',
  `IsDefault` tinyint(1) NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedTime` datetime(6) NOT NULL,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_StatementPdfTemplates_Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `systemconfig`
--

DROP TABLE IF EXISTS `systemconfig`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `systemconfig` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `Key` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=1431228722995204 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `systememailconfigs`
--

DROP TABLE IF EXISTS `systememailconfigs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `systememailconfigs` (
  `Id` bigint NOT NULL,
  `ProviderType` varchar(50) NOT NULL,
  `SenderEmail` varchar(255) NOT NULL,
  `SenderDisplayName` varchar(255) DEFAULT NULL,
  `TenantId` varchar(100) DEFAULT NULL,
  `ClientId` varchar(100) DEFAULT NULL,
  `EncryptedClientSecret` varchar(1000) DEFAULT NULL,
  `SmtpHost` varchar(255) DEFAULT NULL,
  `SmtpPort` int DEFAULT NULL,
  `SecurityMode` varchar(50) DEFAULT NULL,
  `RequireAuth` tinyint(1) NOT NULL DEFAULT '1',
  `SmtpUsername` varchar(255) DEFAULT NULL,
  `EncryptedSmtpPassword` varchar(1000) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedTime` datetime(6) NOT NULL,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `tagname`
--

DROP TABLE IF EXISTS `tagname`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tagname` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `TagName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Unit` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `MeasurementPointId` bigint NOT NULL,
  `Status` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedTime` datetime(6) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `CreatedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  `ModifiedBy` bigint DEFAULT NULL,
  `ModifiedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Deleted` tinyint(1) NOT NULL,
  `DeletedTime` datetime(6) DEFAULT NULL,
  `DeletedBy` bigint DEFAULT NULL,
  `DeletedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CustomName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Type` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `WarningValue` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_TagName_MeasurementPointId` (`MeasurementPointId`),
  KEY `IX_TagName_ModifiedBy` (`ModifiedBy`),
  KEY `IX_TagName_Status` (`Status`),
  CONSTRAINT `FK_TagName_MeasurementPoints_MeasurementPointId` FOREIGN KEY (`MeasurementPointId`) REFERENCES `measurementpoints` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1500601705553944 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `usercustomers`
--

DROP TABLE IF EXISTS `usercustomers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usercustomers` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `CustomerId` bigint NOT NULL,
  `UserId` bigint NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_UserCustomers_CustomerId` (`CustomerId`),
  KEY `IX_UserCustomers_UserId` (`UserId`),
  CONSTRAINT `FK_UserCustomers_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_UserCustomers_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1442763684192257 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `UserName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Password` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `FullName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Email` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `PhoneNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Address` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CustomerCode` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Status` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CountLocked` int NOT NULL,
  `TimeLocked` datetime(6) NOT NULL,
  `IsLocked` tinyint(1) NOT NULL,
  `RoleName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CreatedTime` datetime(6) DEFAULT NULL,
  `CreatedBy` bigint DEFAULT NULL,
  `CreatedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  `ModifiedBy` bigint DEFAULT NULL,
  `ModifiedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Deleted` tinyint(1) NOT NULL,
  `DeletedTime` datetime(6) DEFAULT NULL,
  `DeletedBy` bigint DEFAULT NULL,
  `DeletedName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_Users_ModifiedBy` (`ModifiedBy`),
  KEY `IX_Users_Status` (`Status`)
) ENGINE=InnoDB AUTO_INCREMENT=1501273289547777 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `usertokens`
--

DROP TABLE IF EXISTS `usertokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usertokens` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `UserId` bigint NOT NULL,
  `AccessToken` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `RefreshToken` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Type` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ExpiredTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_UserTokens_UserId` (`UserId`),
  CONSTRAINT `FK_UserTokens_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1531747484344321 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping events for database 'amr'
--
/*!50106 SET @save_time_zone= @@TIME_ZONE */ ;
/*!50106 DROP EVENT IF EXISTS `evt_clean_measpointmonitoring` */;
DELIMITER ;;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;;
/*!50003 SET character_set_client  = utf8mb4 */ ;;
/*!50003 SET character_set_results = utf8mb4 */ ;;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;;
/*!50003 SET @saved_time_zone      = @@time_zone */ ;;
/*!50003 SET time_zone             = 'SYSTEM' */ ;;
/*!50106 CREATE*/ /*!50117 DEFINER=`root`@`localhost`*/ /*!50106 EVENT `evt_clean_measpointmonitoring` ON SCHEDULE EVERY 1 DAY STARTS '2025-03-11 00:30:00' ON COMPLETION NOT PRESERVE ENABLE DO CALL amr.sp_clean_measurementpointmonitoring() */ ;;
/*!50003 SET time_zone             = @saved_time_zone */ ;;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;;
/*!50003 SET character_set_client  = @saved_cs_client */ ;;
/*!50003 SET character_set_results = @saved_cs_results */ ;;
/*!50003 SET collation_connection  = @saved_col_connection */ ;;
/*!50106 DROP EVENT IF EXISTS `evt_clean_measpointmonitoringbystatus` */;;
DELIMITER ;;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;;
/*!50003 SET character_set_client  = utf8mb4 */ ;;
/*!50003 SET character_set_results = utf8mb4 */ ;;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;;
/*!50003 SET @saved_time_zone      = @@time_zone */ ;;
/*!50003 SET time_zone             = 'SYSTEM' */ ;;
/*!50106 CREATE*/ /*!50117 DEFINER=`root`@`localhost`*/ /*!50106 EVENT `evt_clean_measpointmonitoringbystatus` ON SCHEDULE EVERY 1 HOUR STARTS '2025-10-24 16:00:00' ON COMPLETION NOT PRESERVE ENABLE DO CALL sp_clean_measurementpointmonitoringbystatus() */ ;;
/*!50003 SET time_zone             = @saved_time_zone */ ;;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;;
/*!50003 SET character_set_client  = @saved_cs_client */ ;;
/*!50003 SET character_set_results = @saved_cs_results */ ;;
/*!50003 SET collation_connection  = @saved_col_connection */ ;;
DELIMITER ;
/*!50106 SET TIME_ZONE= @save_time_zone */ ;

--
-- Dumping routines for database 'amr'
--
/*!50003 DROP PROCEDURE IF EXISTS `batch_delete_old_records` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `batch_delete_old_records`()
BEGIN
    DECLARE deleted_count INT DEFAULT 200000;
    DECLARE total_deleted INT DEFAULT 0;
    DECLARE batch_number INT DEFAULT 0;
    
    -- Start the batch deletion loop
    WHILE deleted_count = 200000 DO
        SET batch_number = batch_number + 1;
        
        -- Delete 100,000 records at a time
        DELETE FROM amr.measurementpointmonitoringbystatus
        WHERE createdtime < CURDATE()
        LIMIT 200000;
        
        -- Get the number of rows deleted
        SET deleted_count = ROW_COUNT();
        SET total_deleted = total_deleted + deleted_count;
        
        -- Display progress
        SELECT 
            batch_number AS 'Batch #',
            deleted_count AS 'Deleted in this batch',
            total_deleted AS 'Total deleted',
            NOW() AS 'Timestamp';
        
        -- Optional: Add a 1-second delay to reduce load on the database
        DO SLEEP(1);
        
    END WHILE;
    
    -- Display final results
    SELECT 
        'COMPLETED' AS 'Status',
        total_deleted AS 'Total Records Deleted',
        batch_number AS 'Total Batches',
        NOW() AS 'Completed At';
        
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `clean_measurement_batch` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `clean_measurement_batch`()
BEGIN
  DECLARE rows_affected INT DEFAULT 1;

  WHILE rows_affected > 0 DO
    DELETE FROM amr.measurementpointmonitoring
    WHERE CreatedTime < '2025-10-19'
    LIMIT 100000;
    SET rows_affected = ROW_COUNT();
  END WHILE;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_clean_measurementpointmonitoring` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_clean_measurementpointmonitoring`()
BEGIN
    DECLARE rows_affected INT DEFAULT 0;
    DECLARE total_deleted INT DEFAULT 0;
    DECLARE batch_size INT DEFAULT 100000;
    DECLARE utc_offset INT DEFAULT 0;
    DECLARE expire_time DATETIME;

    SET utc_offset = TIMESTAMPDIFF(HOUR, UTC_TIMESTAMP(), NOW());
    SET expire_time = DATE_SUB(DATE(NOW()) - INTERVAL 1 DAY, INTERVAL utc_offset HOUR);

    REPEAT
        DELETE FROM amr.measurementpointmonitoring
        WHERE CreatedTime < expire_time
        LIMIT batch_size;

        SET rows_affected = ROW_COUNT();
        SET total_deleted = total_deleted + rows_affected;

        DO SLEEP(0.1);
    UNTIL rows_affected = 0 END REPEAT;

    SELECT total_deleted AS deleted_rows, expire_time AS expire_time;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_clean_measurementpointmonitoringbystatus` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_clean_measurementpointmonitoringbystatus`()
BEGIN
    DECLARE rows_affected INT DEFAULT 0;
    DECLARE total_deleted INT DEFAULT 0;
    DECLARE batch_size INT DEFAULT 100000;
    DECLARE utc_offset INT DEFAULT 0;
    DECLARE expire_time DATETIME;
    
    -- Calculate UTC offset
    SET utc_offset = TIMESTAMPDIFF(HOUR, UTC_TIMESTAMP(), NOW());
    
    -- Set expire time to 1 hour ago
    SET expire_time = DATE_SUB(NOW(), INTERVAL 1 HOUR);
    
    -- Delete records in batches
    REPEAT
        DELETE FROM amr.measurementpointmonitoringbystatus
        WHERE CreatedTime < expire_time
        LIMIT batch_size;
        
        SET rows_affected = ROW_COUNT();
        SET total_deleted = total_deleted + rows_affected;
        
        -- Sleep to reduce database load
        DO SLEEP(0.1);
    UNTIL rows_affected = 0 END REPEAT;
    
    -- Return summary
    SELECT total_deleted AS deleted_rows, expire_time AS expire_time;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-05 16:41:41

-- Khoi tao du lieu mac dinh cho bang systemconfig
INSERT INTO `amr`.`systemconfig` (`Id`, `Key`, `Value`) VALUES
  (1, 'LOCKLOGIN', '5'),
  (2, 'NUMBEROFINCORRECTENTRIES', '5'),
  (3, 'SYSTEMADMINISTRATIONFULLNAME', 'Administrator'),
  (4, 'SYSTEMADMINISTRATIONPHONE', '0901234567'),
  (5, 'SYSTEMADMINISTRATIONEMAIL', 'admin@mdms.vn'),
  (6, 'CUSTOMERCAREFULLNAME', 'CSKH'),
  (7, 'CUSTOMERCAREPHONE', '19001006'),
  (8, 'CUSTOMERCAREEMAIL', 'cskh@mdms.vn')
ON DUPLICATE KEY UPDATE `Value` = VALUES(`Value`);

-- Khoi tao cau hinh email mac dinh cho bang systememailconfigs
INSERT INTO `amr`.`systememailconfigs`
(`Id`, `ProviderType`, `SenderEmail`, `SenderDisplayName`, `TenantId`, `ClientId`, `EncryptedClientSecret`, `SmtpHost`, `SmtpPort`, `SecurityMode`, `RequireAuth`, `SmtpUsername`, `EncryptedSmtpPassword`, `IsActive`, `CreatedTime`)
VALUES
(1, 'MICROSOFT_GRAPH', 'noreply.hoadondientu.hp@vsip.com.vn', 'VSIP Hải Phòng - Phòng Cơ Điện', '5e5bd7f8-2adb-4595-956a-a03d68ea32a6', '6a86904e-3153-496f-8c1b-986ae398747a', '[AZURE_CLIENT_SECRET]', 'smtp.gmail.com', 587, 'StartTls', 1, '', '', 1, NOW())
ON DUPLICATE KEY UPDATE `ProviderType` = VALUES(`ProviderType`);

-- Khoi tao mau email mac dinh cho bang notificationtemplates
INSERT IGNORE INTO `amr`.`notificationtemplates` 
(`Id`, `Code`, `Name`, `SubjectTemplate`, `BodyHtmlTemplate`, `Description`, `IsDefault`, `IsActive`, `CreatedTime`)
VALUES
(1, 'STATEMENT_NOTIFICATION', 'Phiếu chỉ số', 
 'THÔNG BÁO CHỈ SỐ ĐIỆN HÀNG THÁNG', 
 '<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Thông Báo Chỉ Số & Sản Lượng Điện Năng</title></head><body style=\"margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,\'Segoe UI\',Roboto,Helvetica,Arial,sans-serif;color:#334155;\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"background-color:#f5f7fc;padding:24px 10px;width:100%;max-width:100%;\"><tr><td align=\"center\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"max-width:640px;width:100%;background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;\"><tr><td style=\"background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:24px 30px;text-align:center;\"><h2 style=\"margin:0;color:#ffffff;font-size:20px;font-weight:700;\">{{CompanyName}}</h2><p style=\"margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;\">HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM (MDMS)</p><p style=\"margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;\">THÔNG BÁO CHỈ SỐ & SẢN LƯỢNG ĐIỆN NĂNG</p></td></tr><tr><td style=\"padding:24px 30px 10px 30px;\"><p style=\"margin:0 0 14px 0;font-size:15px;color:#0f172a;\">Kính gửi Quý khách hàng: <strong>{{CustomerName}}</strong>,</p><p style=\"margin:0 0 16px 0;font-size:13.5px;line-height:1.6;color:#475569;\">Chúng tôi xin trân trọng gửi tới Quý khách hàng bảng kê chỉ số công tơ và sản lượng điện năng tiêu thụ trong kỳ ghi chỉ số: {{Period}}.</p><table width=\"100%\" cellpadding=\"10\" cellspacing=\"0\" border=\"0\" style=\"background-color:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;margin-bottom:18px;font-size:13.5px;\"><tr><td width=\"22%\" style=\"color:#64748b;font-weight:500;\">Tên điểm đo:</td><td style=\"font-weight:700;color:#0f172a;\">{{PointName}}</td></tr></table></td></tr><tr><td style=\"padding:0 30px 15px 30px;\"><table width=\"100%\" cellpadding=\"8\" cellspacing=\"0\" border=\"1\" style=\"border-collapse:collapse;border-color:#e2e8f0;font-size:13px;\"><tr style=\"background-color:#f1f5f9;color:#0f172a;font-weight:600;text-align:center;\"><td width=\"32%\">Biểu giá</td><td width=\"22%\">Chỉ số cũ</td><td width=\"22%\">Chỉ số mới</td><td width=\"24%\">Sản lượng (kWh)</td></tr><tr><td style=\"padding-left:12px;\">Bình thường (BT)</td><td align=\"right\" style=\"color:#475569;\">{{PreviousBt}}</td><td align=\"right\" style=\"color:#475569;\">{{Bt}}</td><td align=\"right\" style=\"font-weight:700;color:#0f172a;\">{{ConsumtionBt}}</td></tr><tr><td style=\"padding-left:12px;\">Cao điểm (CD)</td><td align=\"right\" style=\"color:#475569;\">{{PreviousCd}}</td><td align=\"right\" style=\"color:#475569;\">{{Cd}}</td><td align=\"right\" style=\"font-weight:700;color:#0f172a;\">{{ConsumtionCd}}</td></tr><tr><td style=\"padding-left:12px;\">Thấp điểm (TD)</td><td align=\"right\" style=\"color:#475569;\">{{PreviousTd}}</td><td align=\"right\" style=\"color:#475569;\">{{Td}}</td><td align=\"right\" style=\"font-weight:700;color:#0f172a;\">{{ConsumtionTd}}</td></tr><tr style=\"background-color:#fef2f2;font-weight:700;\"><td style=\"padding-left:12px;color:#991b1b;\">Tổng điện năng hữu công (SG)</td><td align=\"right\" style=\"color:#991b1b;\">{{PreviousSg}}</td><td align=\"right\" style=\"color:#991b1b;\">{{Sg}}</td><td align=\"right\" style=\"color:#b22222;font-size:14px;\">{{ConsumtionTotal}}</td></tr><tr><td style=\"padding-left:12px;color:#64748b;\">Tổng điện năng phản kháng (VC)</td><td align=\"right\" style=\"color:#64748b;\">{{PreviousVc}}</td><td align=\"right\" style=\"color:#64748b;\">{{Vc}}</td><td align=\"right\" style=\"font-weight:600;color:#64748b;\">{{ConsumtionVc}}</td></tr></table></td></tr><tr><td style=\"padding:0 30px 15px 30px;\"><table width=\"100%\" cellpadding=\"8\" cellspacing=\"0\" border=\"0\" style=\"background-color:#eff6ff;border:1px solid #bfdbfe;border-radius:6px;font-size:13px;color:#1e40af;\"><tr><td width=\"55%\" style=\"vertical-align:top;\">• Công suất cực đại <strong>Pmax</strong>: <strong>{{Pmax}}</strong><br>• Thời điểm ghi nhận hồi: <strong>{{PmaxTime}}</strong></td><td width=\"45%\" style=\"vertical-align:top;\">• Hệ số công suất <strong>Cos &phi;</strong>: <strong>{{CosPhi}}</strong></td></tr></table></td></tr><tr><td style=\"padding:0 30px 15px 30px;\"><div style=\"background-color:#f8fafc;border-left:4px solid #b22222;padding:12px 16px;border-radius:4px;font-size:13px;color:#334155;line-height:1.6;\">📎 Chi tiết Phiếu chỉ số điện hàng tháng được đính kèm <strong>PhieuBaoSo_{{SerialNumber}}.pdf</strong></div></td></tr><tr><td style=\"padding:0 30px 24px 30px;\"><div style=\"background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;\">Mọi thắc mắc hoặc yêu cầu đối soát số liệu, Quý khách vui lòng liên hệ: {{CareFullname}}<br>📞 Điện thoại: {{ContactPhone}}<br>✉️ Email: {{CareEmail}}</div><p style=\"margin:16px 0 0 0;font-size:13.5px;color:#334155;\">Trân trọng cảm ơn sự hợp tác của Quý khách hàng!</p></td></tr><tr><td style=\"background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;\">Email tự động phát hành từ HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM (MDMS) - {{CompanyName}}. Vui lòng không phản hồi trực tiếp vào địa chỉ thư này.</td></tr></table></td></tr></table></body></html>', 
 'Mẫu email thông báo sản lượng điện hàng tháng gửi khách hàng đính kèm file PDF', 1, 1, NOW()),

(2, 'FORGOT_PASSWORD', 'Đặt lại mật khẩu', 
 '[MDMS] Yêu cầu đặt lại mật khẩu', 
 '<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Yêu Cầu Đặt Lại Mật Khẩu</title></head><body style=\"margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,\'Segoe UI\',Roboto,Helvetica,Arial,sans-serif;color:#334155;\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"padding:40px 15px;\"><tr><td align=\"center\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"max-width:580px;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;\"><tr><td style=\"background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:28px 32px;text-align:center;\"><h2 style=\"margin:0;color:#ffffff;font-size:20px;font-weight:700;\">{{CompanyName}}</h2><p style=\"margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;\">HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM ĐIỆN TỬ (MDMS)</p><p style=\"margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;\">YÊU CẦU ĐẶT LẠI MẬT KHẨU</p></td></tr><tr><td style=\"padding:32px 32px 20px 32px;\"><p style=\"margin:0 0 14px 0;font-size:15px;color:#0f172a;\">Kính gửi <strong>{{Username}}</strong>,</p><p style=\"margin:0 0 20px 0;font-size:14px;line-height:1.7;color:#475569;\">Chúng tôi nhận được yêu cầu thiết lập lại mật khẩu đăng nhập cho tài khoản liên kết với địa chỉ email này trên hệ thống MDMS. Để hoàn tất quy trình, Quý vị vui lòng nhấp vào nút thao tác bên dưới:</p><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"margin:24px 0;\"><tr><td align=\"center\"><a href=\"{{ResetLink}}\" target=\"_blank\" style=\"display:inline-block;background-color:#b22222;color:#ffffff;font-size:14px;font-weight:600;text-decoration:none;padding:13px 36px;border-radius:6px;box-shadow:0 2px 8px rgba(178,34,34,0.3);\">Thiết Lập Lại Mật Khẩu</a></td></tr></table><div style=\"background-color:#fffbeb;border-left:4px solid #f59e0b;padding:12px 16px;margin-bottom:20px;border-radius:4px;\"><p style=\"margin:0;font-size:12.5px;color:#92400e;line-height:1.5;\">⏱️ <strong>Lưu ý bảo mật:</strong> Liên kết đặt lại mật khẩu có hiệu lực trong vòng <strong>10 phút</strong> kể từ khi phát hành. Nếu Quý vị không gửi yêu cầu này, vui lòng bỏ qua thư hoặc thông báo ngay cho quản trị viên.</p></div><div style=\"background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;\">Nếu Quý vị cần hỗ trợ kỹ thuật, vui lòng liên hệ: {{CareFullname}}<br>📞 Hotline: {{SupportPhone}}<br>✉️ Email: {{SystemEmail}}</div></td></tr><tr><td style=\"background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;\">Email tự động phát hành từ Hệ thống Quản trị Đo đếm Điện tử MDMS.</td></tr></table></td></tr></table></body></html>', 
 'Mẫu email gửi link yêu cầu thiết lập lại mật khẩu người dùng', 1, 1, NOW()),

(3, 'USER_CREDENTIAL', 'Cấp mới tài khoản & mật khẩu', 
 '[MDMS] Thông tin tài khoản - {{FullName}}', 
 '<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Thông Tin Cấp Tài Khoản</title></head><body style=\"margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,\'Segoe UI\',Roboto,Helvetica,Arial,sans-serif;color:#334155;\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"padding:40px 15px;\"><tr><td align=\"center\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"max-width:600px;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;\"><tr><td style=\"background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:28px 32px;text-align:center;\"><h2 style=\"margin:0;color:#ffffff;font-size:20px;font-weight:700;\">{{CompanyName}}</h2><p style=\"margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;\">HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM ĐIỆN TỬ (MDMS)</p><p style=\"margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;\">THÔNG TIN TÀI KHOẢN</p></td></tr><tr><td style=\"padding:32px 32px 20px 32px;\"><p style=\"margin:0 0 14px 0;font-size:15px;color:#0f172a;\">Kính gửi Quý người dùng: <strong>{{FullName}}</strong>,</p><p style=\"margin:0 0 20px 0;font-size:14px;line-height:1.7;color:#475569;\">Tài khoản truy cập Hệ thống Quản trị Dữ liệu Đo đếm Điện tử (MDMS) của Quý vị đã được cấp thành công. Thông tin tài khoản đăng nhập chi tiết như sau:</p><table width=\"100%\" cellpadding=\"12\" cellspacing=\"0\" border=\"0\" style=\"background-color:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;margin-bottom:24px;font-size:14px;\"><tr><td width=\"35%\" style=\"color:#64748b;font-weight:500;\">Tên đăng nhập:</td><td width=\"65%\" style=\"color:#0f172a;font-weight:700;font-family:monospace;font-size:15px;\">{{Username}}</td></tr><tr><td style=\"color:#64748b;font-weight:500;\">Mật khẩu khởi tạo:</td><td style=\"color:#b22222;font-weight:700;font-family:monospace;font-size:15px;\">{{Password}}</td></tr></table><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"margin:24px 0;\"><tr><td align=\"center\"><a href=\"{{LoginUrl}}\" target=\"_blank\" style=\"display:inline-block;background-color:#b22222;color:#ffffff;font-size:14px;font-weight:600;text-decoration:none;padding:13px 36px;border-radius:6px;box-shadow:0 2px 8px rgba(178,34,34,0.3);\">Đăng Nhập</a></td></tr></table><div style=\"background-color:#fffbeb;border-left:4px solid #f59e0b;padding:12px 16px;margin-bottom:20px;border-radius:4px;\"><p style=\"margin:0;font-size:13px;color:#92400e;line-height:1.5;\">⚠️ <strong>Khuyến cáo an toàn:</strong> Vì lý do bảo mật thông tin, Quý vị vui lòng thực hiện đổi mật khẩu ngay trong lần đăng nhập đầu tiên và tuyệt đối không chia sẻ mật khẩu này cho bất kỳ ai.</p></div><div style=\"background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;\">Nếu Quý vị cần hỗ trợ kỹ thuật, vui lòng liên hệ: {{CareFullname}}<br>📞 Hotline: {{SupportPhone}}<br>✉️ Email: {{SystemEmail}}</div></td></tr><tr><td style=\"background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;\">Email tự động phát hành từ Hệ thống Quản trị Đo đếm Điện tử MDMS.</td></tr></table></td></tr></table></body></html>', 
 'Mẫu email gửi thông tin tên đăng nhập và mật khẩu khi tài khoản được cấp mới', 1, 1, NOW());

-- Khoi tao mau phieu bao PDF mac dinh cho bang statementpdftemplates
INSERT IGNORE INTO `amr`.`statementpdftemplates` 
(`Id`, `Code`, `Name`, `HeaderTitle`, `SubHeaderTitle`, `CompanyName`, `CompanyAddress`, `ContactPhone`, `PaperSize`, `EngineType`, `DisplayColumnsJson`, `FooterNotes`, `RequireDualSignatures`, `IsDefault`, `IsActive`, `CreatedTime`)
VALUES
(1, 'A5_VSIP_STANDARD', 'Mẫu phiếu báo A5 Ngang - Chuẩn VSIP Hải Phòng', 
 'MONTHLY POWER METER READING', 'PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG', 
 'VSIP HAI PHONG CO., LTD', 'VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam', 'Tel: 84-225-3959868   Fax: 84-225-3959886', 
 'A5_LANDSCAPE', 'QUEST_PDF', '{"columns":["BT","CD","TD","SG","VC","COSPHI","REMARKS","PMAX","MULTIPLIER"],"documentCode":"M&E-1/01 PR04","copyLabel":"(M&E Copy – Liên cho Phòng Cơ Điện)","partyAName":"Party A (VSIP Hai Phong Co., Ltd)\\nBên A (Công ty TNHH VSIP Hải Phòng)","partyBName":"Party B (Tenant)\\nBên B (Khách hàng)","headerLeftMode":"LOGO","headerCenterMode":"LOGO","headerRightMode":"DECOR","footerLeftMode":"LOGO","footerCenterMode":"LOGO","footerRightMode":"LOGO"}', 
 'Quý khách vui lòng kiểm tra và phản hồi số liệu trong vòng 03 ngày làm việc kể từ ngày nhận phiếu báo.', 
 1, 1, 1, NOW()),

(2, 'A4_PORTRAIT_STANDARD', 'Mẫu phiếu báo A4 Đứng - Chuẩn Kế toán & Lưu trữ', 
 'MONTHLY POWER METER READING', 'PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG', 
 'VSIP HAI PHONG CO., LTD', 'VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam', 'Tel: 84-225-3959868   Fax: 84-225-3959886', 
 'A4_PORTRAIT', 'QUEST_PDF', '{"columns":["BT","CD","TD","SG","VC","COSPHI","REMARKS","PMAX","MULTIPLIER"],"documentCode":"M&E-1/01 PR04","copyLabel":"(M&E Copy – Liên cho Phòng Cơ Điện)","partyAName":"Party A (VSIP Hai Phong Co., Ltd)\\nBên A (Công ty TNHH VSIP Hải Phòng)","partyBName":"Party B (Tenant)\\nBên B (Khách hàng)","headerLeftMode":"LOGO","headerCenterMode":"LOGO","headerRightMode":"DECOR","footerLeftMode":"LOGO","footerCenterMode":"LOGO","footerRightMode":"LOGO"}', 
 'Phiếu báo này được kết xuất tự động từ hệ thống đo đếm từ xa AMR/MDMS. Mọi yêu cầu đối soát số liệu kỹ thuật xin liên hệ Bộ phận Vận hành Đo đếm.', 
 1, 0, 1, NOW());

