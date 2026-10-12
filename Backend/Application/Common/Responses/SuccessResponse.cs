using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Application.Common;

// ReSharper disable All

// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable 8618
#pragma warning disable 1591
namespace Application.Common.Responses.Common.Responses
{
    /// <summary>
    /// To handle all responses of Succeeded cases
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SuccessResponse
    {
        [DefaultValue(200)] public int Status { get; set; } = 200;

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Data if any, it may be an array or an object
        /// </summary>
        [DefaultValue("")]
        public object Data { get; set; }

        /// <summary>
        /// Initialize SuccessResponse
        /// </summary>
        /// <param name="message">Response message</param>
        /// <param name="data"> An array or an object</param>
        public SuccessResponse(string message = LocalizationString.Common.Success, object data = null!)
        {
            Message = message;
            Data = data;
        }

        public SuccessResponse()
        {
            Message = LocalizationString.Common.Success;
            Data = new { };
        }
    }
}