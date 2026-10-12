using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Application.Common.ValueObjects;

#pragma warning disable 8618

// ReSharper disable All

namespace Application.Common.Responses
{
    [ExcludeFromCodeCoverage]
    public class FailureResponse
    {
        [DefaultValue(202)] public int Status { get; set; } = 202;

        /// <summary>
        /// Error Object
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [DefaultValue("")]
        public IEnumerable<ErrorItem> Errors { get; set; }

        /// <summary>
        /// Initialize FailureResponse
        /// </summary>
        /// <param name="errors">Error Array</param>
        public FailureResponse(IEnumerable<ErrorItem> errors)
        {
            Errors = errors;
        }
    }
}