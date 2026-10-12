using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nobisoft.Core.Constants;

public static class Validator
{
    public static class FieldLength
    {
        public const int SingleLineOfText = 255;
        public const string SingleLineOfTextError = "The length of {PropertyName} must be {MaxLength} characters or fewer. You entered {TotalLength} characters.";
        public const string NotEmptyError = "{PropertyName} must not be empty.";
        public const int Description = 1000;
        public const string DescriptionError = "The length of {PropertyName} must be {MaxLength} characters or fewer. You entered {TotalLength} characters.";
        public const int Url = 1000;
        public const string UrlError = "The length of {PropertyName} must be {MaxLength} characters or fewer. You entered {TotalLength} characters.";
        public const int Token = 2000;
        public const string TokenError = "The length of {PropertyName} must be {MaxLength} characters or fewer. You entered {TotalLength} characters.";
        public const string NotValidStatus = "{PropertyName} must be in the defined status list";
        public const int TextMinLength = 3;
        public const int TextMaxLength = 255;
        public const int MiddleTextLength = 50;
        public const int UrlMaxLength = 1000;
        public const int DescriptionMaxLength = 1000;
        public const int RecaptchaMaxLength = 1000;
        public const int FirebaseTokenMaxLength = 700;
        public const int HashedPassword = 2000;
        public const int OtpLength = 6;
    }
}

public static class Serializer
{
    public static readonly JsonSerializerOptions? JsonSerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };
}

public static class Title
{
    public const string Reviewer = "Reviewer";
    public const string QeDirector = "QE Director";
    public const string QeManager = "QE Manager";
    public const string DirectorOfGlobalManufacturingQuality = "Director of Global Manufacturing Quality";
}

public static class Notification
{
    public static class Type
    {
        public const string ApprovedCccCodeByQeManager = "APPROVED_CCC_CODE_BY_QE_MANAGER";
        public const string ApprovedCccCodeByReviewer = "APPROVED_CCC_CODE_BY_REVIEWER";
        public const string ApprovedCccCodeByDirector = "APPROVED_CCC_CODE_BY_DIRECTOR";
        
        public const string RejectedCccCodeByQeManager = "REJECTED_CCC_CODE_BY_QE_MANAGER";
        public const string RejectedCccCodeByReviewer = "REJECTED_CCC_CODE_BY_REVIEWER";
        public const string RejectedCccCodeByDirector = "REJECTED_CCC_CODE_BY_DIRECTOR";
        
        public const string ProposeNewCccCode = "PROPOSE_NEW_CCC_CODE";
        public const string ProposeUpdateDescriptionCccCode = "PROPOSE_UPDATE_DESCRIPTION_CCC_CODE";
        public const string ProposeUpdateValueCccCode = "PROPOSE_UPDATE_VALUE_CCC_CODE";
        public const string ProposeDeactivateCccCode = "PROPOSE_DEACTIVATE_CCC_CODE";
        public const string ProposeReactivateCccCode = "PROPOSE_REACTIVATE_CCC_CODE";
        public const string ProposeTransferCccCode = "PROPOSE_TRANSFER_CCC_CODE";
        
        public const string ProposeNewMasterData = "PROPOSE_NEW_MASTER_DATA";
        public const string ProposeUpdateMasterData = "PROPOSE_NEW_MASTER_DATA";
        public const string ProposeDeactivateMasterData = "PROPOSE_NEW_MASTER_DATA";
        public const string ProposeActivateMasterData = "PROPOSE_NEW_MASTER_DATA";
        
        public const string ApprovedMasterDataByDirector = "APPROVED_MASTER_DATA_BY_DIRECTOR";
        public const string RejectedMasterDataByDirector = "REJECTED_MASTER_DATA_BY_DIRECTOR";
        
        public const string ProposeUpdateConfigurationRule = "PROPOSE_UPDATE_CONFIGURATION_RULE";
        public const string ApprovedConfigurationRuleByDirector = "APPROVED_CONFIGURATION_RULE_BY_DIRECTOR";
        public const string RejectedConfigurationRuleByDirector = "REJECTED_CONFIGURATION_RULE_BY_DIRECTOR";
        public const string ProposeHandshake = "PROPOSE_HANDSHAKE";
        public const string ApprovedHandshake = "APPROVED_HANDSHAKE";
        public const string RejectedHandshake = "REJECTED_HANDSHAKE";
        public const string BinningProcess = "BINNING_PROCESS";
        public const string NotificationFirstTime = "NOTIFICATION_FIRST_TIME";
        public const string NotificationSecondTime = "NOTIFICATION_SECOND_TIME";
    }
}

public static class MasterData
{
    public static class Type
    {
        public const string Factory = "FACTORY";
        public const string Market = "MARKET";
        public const string Model = "MODEL";
        public const string ModelYear = "MODEL_YEAR";
        public const string Part = "PART";
        public const string PositionFr = "POSITION_FR";
        public const string PositionLr = "POSITION_LR";
        public const string PositionWhere = "POSITION_WHERE";
        public const string PositionWhich = "POSITION_WHICH";
        public const string Severity = "SEVERITY";
        public const string Variant = "VARIANT";
    }
}

public static class Workflow
{
    public static class Status
    {
        public const string New = "NEW";
        public const string Approved = "APPROVED";
        public const string Rejected = "REJECTED";
    }
    
    public static class Action
    {
        public const string Create = "CREATE";
        public const string Update = "UPDATE";
        public const string Delete = "DELETE";
        public const string Deactivate = "DEACTIVATE";
        public const string Activate = "ACTIVATE";
    }
}

public static class ConfigurationRule
{
    public static class Parameter
    {
        public const string Mis = "MIS";
        public const string ContributeWeight = "CONTRIBUTE_WEIGHT";
        public const string LockTime = "LOCK_TIME";
        public const string BinningTime = "BINNING_TIME";
    }
    
    public static class Type
    {
        // MIS
        public const string MisMes = "MIS_MES";
        public const string MisDms = "MIS_DMS";
        
        // Contribute Weight
     
        public const string ContributeWeightSeverity = "CONTRIBUTE_WEIGHT_SEVERITY";
        public const string ContributeWeightWarranty = "CONTRIBUTE_WEIGHT_WARRANTY";
        public const string ContributeWeightAtPlant = "CONTRIBUTE_WEIGHT_AT_PLANT";
        public const string ContributeWeightVoc = "CONTRIBUTE_WEIGHT_VOC";
        
        // Lock Time
        public const string LockTimeBinning = "LOCK_TIME_BINNING";
        public const string LockTimeReBinning = "LOCK_TIME_REBINNING";
        public const string LockTimeLinkToJira = "LOCK_TIME_LINK_TO_JIRA";
        
        // Binning Time (for Binning Process)
        public const string BinningTimeBinning = "BINNING_TIME_BINNING";
        public const string BinningTimeReminder = "BINNING_TIME_REMINDER";

    }
}