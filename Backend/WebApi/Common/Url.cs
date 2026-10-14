namespace WebApi.Common;

public static class Url
{
    public static class Areas
    {
        public const string Users = "Users";
        public const string Auth = "Auth";
        public const string Customers = "Customers";
        public const string IoT = "IoT";
        public const string MeasurementPoints = "MeasurementPoints";
        public const string MeasurementPointMonitoring = "MeasurementPointMonitoring";
        public const string Gcs = "Gcs";
        public const string GcsCutOffCycle = "GcsCutOffCycle";
        public const string GcsMonthlyDetail = "GcsMonthlyDetail";
        public const string TagName = "TagName";
        public const string Report = "Report";
        public const string SystemConfig = "SystemConfig";
        public const string PeriodicEnergyStatement = "PeriodicEnergyStatement";
    }

    public static class Default
    {
        public static class Account
        {
            public const string Create = "User/Create-User";
            public const string Update = "User/Update-User";
            public const string Delete = "User/Delete-User/{id}";
            public const string ChangeStatus = "User/Change-Status";
            public const string List = "User/Get-List-Users";
            public const string View = "User/Get-User-Detail";
            public const string ExportPdf = "User/Export-Pdf-User";
            public const string ExportCsv = "User/Export-Csv-User";
        }

        public static class Customer
        {
            public const string Create = "Customers/Create-Customer";
            public const string Update = "Customers/Update-Customer";
            public const string Delete = "Customers/Delete-Customer/{id}";
            public const string ChangeStatus = "Customers/Change-Status";
            public const string List = "Customers/Get-List-Customers";
            public const string ListForReport = "Customers/Get-List-Customers-For-Report";
            public const string View = "Customers/Get-Customer-Detail";
            public const string ExportPdf = "Customers/Export-Pdf-Customers";
            public const string ExportCsv = "Customers/Export-Csv-Customers";
            public const string ImportCsv = "Customers/Import-Csv";
        }

        public static class Report
        {
            public const string LoadChart = "Report/Load-Chart";
            public const string VoltageChart = "Report/Voltage-Chart";
            public const string PowerChart = "Report/Power-Chart";
            public const string ConsumptionOutputChart = "Report/Consumption-Output-Chart";
            public const string FinalizedOutputElectricityChart = "Report/Finalized-Output-Electricity-Chart";
            public const string ListFinalizedOutputElectricity = "Report/List-Finalized-Output-Electricity";
            public const string HalfHourFinalizedEnergyReport = "Report/Half-Hour-Finalized-Energy-Report";
            public const string DailyFinalizedEnergyReport = "Report/Daily-Finalized-Energy-Report";
            public const string ExportCsvHalfHourFinalizedEnergy = "Half-Hour-Finalized-Energy/Export-Csv-Half-Hour-Finalized-Energy";
            public const string ExportPdfHalfHourFinalizedEnergy = "Half-Hour-Finalized-Energy/Export-Pdf-Half-Hour-Finalized-Energy";
            public const string ExportCsvDailyFinalizedEnergy = "Daily-Finalized-Energy/Export-Csv-Daily-Finalized-Energy";
            public const string ExportPdfDailyFinalizedEnergy = "Daily-Finalized-Energy/Export-Pdf-Daily-Finalized-Energy";
        }

        public static class Auth
        {
            public const string SignIn = "Auth/Sign-In";
            public const string RefreshToken = "Auth/Refresh-Token";
            public const string ChangePassword = "Auth/Change-Password";
            public const string ForgotPassword = "Auth/Forgot-Password";
            public const string ConfirmForgotPassword = "Auth/Confirm-Forgot-Password";
            public const string ChangeInformationAccount = "Auth/Change-Information-Account";
            public const string CheckTokenForgotPassword = "Auth/Check-Token-Forgot-Password";
        }

        public static class IoT
        {
            public const string MeasurementPointMonitoring = "IoT/Get-Data-Measurement-Point-Monitoring";
            public const string MeasurementPointMonitoringByDateTime = "IoT/Get-DateTime-Measurement-Point";
            public const string MeasurementPointAlert = "IoT/Get-Data-Measurement-Point-Alert";
            public const string GetDataGcs = "IoT/Get-Data-Gcs";
            public const string GetDataTest = "IoT/test";
        }

        public static class MeasurementPoints
        {
            public const string Create = "Measurement-Points/Create-Measurement-Point";
            public const string Update = "Measurement-Points/Update-Measurement-Point";
            public const string ChangeStatus = "Measurement-Points/Change-Status-Measurement-Point";
            public const string Delete = "Measurement-Points/Delete-Measurement-Point/{id}";
            public const string List = "Measurement-Points/Get-List-Measurement-Points";
            public const string ListForReport = "Measurement-Points/Get-List-Measurement-Points-For-Report";
            public const string View = "Measurement-Points/Get-Measurement-Point";
            public const string ExportPdf = "Measurement-Points/Export-Pdf-Measurement-Points";
            public const string ExportCsv = "Measurement-Points/Export-Csv-Measurement-Points";
        }

        public static class MeasurementPointMonitoring
        {
            public const string List = "Measurement-Point-Monitoring/Get-List-Measurement-Point-Monitoring";
            public const string ExportPdfMeasurementPointMonitoring = "Measurement-Point-Monitoring/Export-Pdf-Measurement-Point-Monitoring";
            public const string ExportCsvMeasurementPointMonitoring = "Measurement-Point-Monitoring/Export-Csv-Measurement-Point-Monitoring";
            public const string ListEvent = "Measurement-Point-Monitoring/Get-List-Measurement-Point-Event";
            public const string ExportPdfMeasurementPointEvent = "Measurement-Point-Monitoring/Export-Pdf-Measurement-Point-Event";
            public const string ExportCsvMeasurementPointEvent = "Measurement-Point-Monitoring/Export-Csv-Measurement-Point-Event";
            public const string ExportPdfMeasurementPointAlert = "Measurement-Point-Monitoring/Export-Pdf-Measurement-Point-Alert";
            public const string ExportCsvMeasurementPointAlert = "Measurement-Point-Monitoring/Export-Csv-Measurement-Point-Alert";
            public const string ListAlert = "Measurement-Point-Monitoring/Get-List-Measurement-Point-Alert";
            public const string View = "Measurement-Point-Monitoring/Get-Measurement-Point-Monitoring";
            public const string CountAlert = "Measurement-Point-Monitoring/Cout-Alert";
            public const string DeleteManyRecordsMeasurementPointMonitoring = "Measurement-Point-Monitoring/DeleteManyRecordsMeasurementPointMonitoring";
        }

        public static class Gcs
        {
            public const string Create = "Gcs/Create-Gcs";
            public const string Update = "Gcs/Update-Gcs";
            public const string ChangeStatus = "Gcs/Change-Status-Gcs";
            public const string Delete = "Gcs/Delete-Gcs/{id}";
            public const string FinalMeterReading = "Gcs/Final-Meter-Reading";
            public const string UpdateGcsTemporary = "Gcs/Update-Gcs-Temporary";
            public const string List = "Gcs/Get-List-Gcs";
            public const string View = "Gcs/Get-Gcs";
            public const string ExportPdf = "Gcs/Export-Pdf-Gcs";
            public const string ExportCsv = "Gcs/Export-Csv-Gcs";
            public const string FinalExportPdf = "Gcs/Export-Pdf-Gcs-Final";
            public const string FinalExportCsv = "Gcs/Export-Csv-Gcs-Final";
        }

        public static class GcsCutOffCycle
        {
            public const string List = "Gcs-CutOff-Cycle/Get-List-Gcs-CutOff-Cycle";
            public const string ListYear = "Gcs-CutOff-Cycle/Get-List-Year-Gcs-CutOff-Cycle";
        }


        public static class GcsMonthlyDetail
        {
            public const string List = "Gcs-Monthly-Detail/Get-List-Gcs-Monthly-Detail";
            public const string ExportPdf = "Gcs-Monthly-Detail/Export-Pdf-Gcs-Monthly-Detail";
            public const string ExportCsv = "Gcs-Monthly-Detail/Export-Csv-Gcs-Monthly-Detail";
        }

        public static class PeriodicEnergyStatement
        {
            public const string List = "Periodic-Energy-Statement/Get-List-Periodic-Energy-Statement";
            public const string SendMail = "Periodic-Energy-Statement/Send-Mail";
            public const string SendAllMail = "Periodic-Energy-Statement/Send-All-Mail";
            public const string CheckStatusSendMail = "Periodic-Energy-Statement/Check-Status-Send-Mail";
        }

        public static class TagName
        {
            public const string Create = "Tag-Name/Create-Tag-Name";
            public const string Update = "Tag-Name/Update-Tag-Name";
            public const string Delete = "Tag-Name/Delete-Tag-Name/{id}";
            public const string List = "Tag-Name/Get-List-Tag-Name";
            public const string View = "Tag-Name/Get-Tag-Name";
        }

        public static class SystemConfig
        {
            public const string Create = "System-Config/Create-Update-System-Config";
            public const string List = "System-Config/Get-List-System-Config";
        }
    }
}