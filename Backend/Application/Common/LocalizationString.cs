using System.Diagnostics.CodeAnalysis;

namespace Application.Common
{
    /// <summary>
    /// To define all localized strings in the system
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class LocalizationString
    {
        public static class Common
        {
            public const string Success = "Success";
            public const string Error = "Error";
            public const string DataValidationError = "Data validation error";
            public const string UnknownFieldName = "General";
            public const string NotFound = "Item is not found. It may be deleted or not existed in the system";
            public const string CommitFailed = "Cannot finish you action at this time, try again";
            public const string ViewListFailed = "Cannot finish you action at this time, try again";
            public const string InvalidRecaptcha = "Captcha is not valid";
            public const string ItemNotFound = "Item is not found, deactivated or deleted";
            public const string EmptyField = "{PropertyName} is empty";
            public const string NullField = "{PropertyName} is null";
            public const string IncorrectFormatField = "{PropertyName} ({PropertyValue}) format is not correct";
            public const string MaxLengthField = "The length of {PropertyName} must be {MaxLength} characters or fewer. You entered {TotalLength} characters";
            public const string MaxTotalValueField = "The total value of Mfg,Sqe,Engineering must be 100% or fewer";
            public const string MinLengthField = "The length of {PropertyName} must be at least {MaxLength} characters. You entered {TotalLength} characters";
            public const string DuplicatedItem = "Item is duplicated";
            public const string DuplicatedField = "{PropertyName} is duplicated";
            public const string DisabledFeature = "Feature is disabled";
            public const string NotValidEnumValue = "{PropertyName} value ({PropertyValue}) is not valid";
            public const string NotEmpty = "{PropertyName} value ({PropertyValue}) is required";
            public const string MaxResults = "The maximum allowed value of MaxResults is 50";
            public const string EqualYearValue = "Model Year and ProductionDate must have same year value";
        }
        
    }
}