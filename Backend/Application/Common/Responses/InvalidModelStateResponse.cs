using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable All

namespace Application.Common.Responses
{
    [ExcludeFromCodeCoverage]
    public class InvalidModelStateResponse
    {
        [DefaultValue(400)] public int Status { get; set; } = 400;

        /// <summary>
        /// Error Object
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [DefaultValue("")]
        public object Errors { get; set; }

        /// <summary>
        /// Initialize InvalidModelStateResponse
        /// </summary>
        /// <param name="errors">Error Object</param>
        public InvalidModelStateResponse(object errors)
        {
            Errors = errors;
        }
    }
}