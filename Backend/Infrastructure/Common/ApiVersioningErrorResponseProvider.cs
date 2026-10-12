using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

// ReSharper disable All

namespace VineHunter.Infrastructure.Common
{
    /// <summary>
    /// To handle error while requesting API with error on API Versioning.
    /// Possible error codes including:
    ///     - ApiVersionUnspecified: An API version is required, but was not specified
    ///     - UnsupportedApiVersion: The specified API version is not supported
    ///     - InvalidApiVersion: An API version was specified, but it is invalid
    ///     - AmbiguousApiVersion: An API version was specified multiple times with different values
    /// </summary>
    public class ApiVersioningErrorResponseProvider : DefaultErrorResponseProvider
    {
        /// <summary>
        /// Create response message
        /// </summary>
        /// <param name="context">Current error context</param>
        /// <returns>HTTP code 406</returns>
        public override IActionResult CreateResponse(ErrorResponseContext context)
        {
            var errorResponse = new ApiVersioningErrorResponse()
            {
                Status = HttpStatusCode.NotAcceptable,
                ErrorCode = context.ErrorCode,
                Message = context.Message
            };
            return new ObjectResult(errorResponse) {StatusCode = (int) HttpStatusCode.NotAcceptable};
        }
    }

    public class ApiVersioningErrorResponse
    {
        public HttpStatusCode Status { get; set; } = HttpStatusCode.NotAcceptable;
        public string? ErrorCode { get; set; }
        public string? Message { get; set; }
    }
}