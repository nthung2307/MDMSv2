using System.Diagnostics.CodeAnalysis;

namespace Application.Common
{
    [ExcludeFromCodeCoverage]
    public static class Constants
    {
        public static class ApiVersion
        {
            public const string V1_0 = "1.0";
        }

        public static class Oauth
        {
            public const string GrantType = "grant_type";
            public const string ClientId = "client_id";
            public const string RedirectUri = "redirect_uri";
            public const string Code = "code";
            public const string AuthorizationCode = "authorization_code";
        }

        public static class MimeTypes
        {
            public static class Text
            {
                public const string Plain = "text/plain";
                public const string Html = "text/html";
                public const string Xml = "text/xml";
                public const string RichText = "text/richtext";
            }

            public static class Application
            {
                public const string Soap = "application/soap+xml";
                public const string Octet = "application/octet-stream";
                public const string Rtf = "application/rtf";
                public const string Pdf = "application/pdf";
                public const string Zip = "application/zip";
                public const string Json = "application/json";
                public const string Xml = "application/xml";
            }

            public static class Image
            {
                public const string Gif = "image/gif";
                public const string Tiff = "image/tiff";
                public const string Jpeg = "image/jpeg";
            }
        }

        /// <summary>
        /// For ActionLog usage. Each action should be under ObjectName|ActionName format
        /// </summary>
        public static class TableName
        {
            public const string RolePermissions = "RolePermissions";
            public const string Roles = "Roles";
            public const string UserRoles = "UserRoles";
            public const string Users = "Users";
            public const string Customers = "Customers";
            public const string UserCustomers = "UserCustomers";
            public const string Gcs = "Gcs";
            public const string GcsTemporary = "GcsTemporary";
            public const string MeasurementPointMonitoring = "MeasurementPointMonitoring";
            public const string MeasurementPointMonitoringByDateTime = "MeasurementPointMonitoringByDateTime";
            public const string MeasurementPointMonitoringByStatus = "MeasurementPointMonitoringByStatus";
            public const string ArchiveDataMeasurementPointMonitoring = "ArchiveDataMeasurementPointMonitoring";
            public const string MeasurementPointMonitoringHourly = "MeasurementPointMonitoringHourly";
            public const string MeasurementPointEvents = "MeasurementPointEvents";
            public const string GcsCutoffCycle = "GcsCutoffCycle";
            public const string GcsMonthlyDetail = "GcsMonthlyDetail";
            public const string MeasurementPoints = "MeasurementPoints";
            public const string UserTokens = "UserTokens";
            public const string TagName = "TagName";
            public const string SystemConfig = "SystemConfig";
            public const string MeasurementPointAlert = "MeasurementPointAlerts";
            public const string PeriodicEnergyStatement = "PeriodicEnergyStatement";
            public const string MeasurementPointHalfHourEnergyReport = "MeasurementPointHalfHourEnergyReports";
            public const string MeasurementPointDailyEnergyReport = "MeasurementPointDailyEnergyReports";
            public const string MeasurementPointHalfHourOperatingReport = "MeasurementPointHalfHourOperatingReports";
            public const string SystemEmailConfigs = "SystemEmailConfigs";
            public const string NotificationTemplates = "NotificationTemplates";
            public const string StatementPdfTemplates = "StatementPdfTemplates";
            public const string MeterReplacements = "MeterReplacements";
        }

        public static class EmailProvider
        {
            public const string MicrosoftGraph = "MICROSOFT_GRAPH";
            public const string CustomSmtp = "CUSTOM_SMTP";
            public const string Gmail = "GMAIL";
        }

        public static class NotificationTemplateCode
        {
            public const string UserCredential = "USER_CREDENTIAL";
            public const string ForgotPassword = "FORGOT_PASSWORD";
            public const string StatementNotification = "STATEMENT_NOTIFICATION";
        }

        public static class SystemConfig
        {
            public const string LockLogin = "LOCKLOGIN";
            public const string NumberOfIncorrectEntries = "NUMBEROFINCORRECTENTRIES";
            public const string SystemAdministrationFullname = "SYSTEMADMINISTRATIONFULLNAME";
            public const string SystemAdministrationEmail = "SYSTEMADMINISTRATIONEMAIL";
            public const string SystemAdministrationPhone = "SYSTEMADMINISTRATIONPHONE";
            public const string CustomerCareFullname = "CUSTOMERCAREFULLNAME";
            public const string CustomerCareEmail = "CUSTOMERCAREEMAIL";
            public const string CustomerCarePhone = "CUSTOMERCAREPHONE";

            // Thông tin công ty / đơn vị cung cấp điện
            public const string CompanyName = "COMPANY_NAME";
            public const string CompanyTaxCode = "COMPANY_TAX_CODE";
            public const string CompanyPhone = "COMPANY_PHONE";
            public const string CompanyFax = "COMPANY_FAX";
            public const string CompanyEmail = "COMPANY_EMAIL";
            public const string CompanyWebsite = "COMPANY_WEBSITE";
            public const string CompanyProvince = "COMPANY_PROVINCE";
            public const string CompanyDistrict = "COMPANY_DISTRICT";
            public const string CompanyWard = "COMPANY_WARD";
            public const string CompanyAddressDetail = "COMPANY_ADDRESS_DETAIL";
            public const string CompanyCountry = "COMPANY_COUNTRY";
            public const string CompanyAddress = "COMPANY_ADDRESS";

            // Đường dẫn công khai của hệ thống (Cloudflare Tunnel, Domain hoặc WAN IP)
            public const string PublicBaseUrl = "PUBLIC_BASE_URL";

            // Cấu hình bảo mật Kepware IoT Gateway
            public const string IoT_ApiKey = "IOT_API_KEY";
            public const string IoT_AllowedIps = "IOT_ALLOWED_IPS";
            public const string IoT_RequireApiKey = "IOT_REQUIRE_API_KEY";
        }
    }
}