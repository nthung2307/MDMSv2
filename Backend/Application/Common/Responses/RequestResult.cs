using System.Diagnostics.CodeAnalysis;
using Application.Common.ValueObjects;
using MediatR;
using Nobisoft.Core.Exceptions;
using Nobisoft.Core.ValueObjects;

// ReSharper disable All
#pragma warning disable 8625

namespace Application.Common.Responses.Common.Responses
{
    /// <summary>
    /// To generate result for all action in this system.
    /// Result contains:
    ///     1. Succeeded flag: true/false
    ///     2. Errors list: List of errors if any in case of failure
    ///     3. Data object: Data object if action need to pass data back to caller
    /// </summary>
    /// <typeparam name="TResultDataType"></typeparam>
    [ExcludeFromCodeCoverage]
    public class RequestResult<TResultDataType> : IRequest<Unit>
    {
        internal RequestResult(bool success, IEnumerable<ErrorItem> errors, TResultDataType? data)
        {
            Success = success;
            Errors = errors.ToArray();
            Data = data;
        }

        public TResultDataType? Data { get; private set; }
        public bool Success { get; private set; }
        public IEnumerable<ErrorItem> Errors { get; private set; }

        /// <summary>
        /// Handle success
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static RequestResult<TResultDataType> Succeed(TResultDataType? data)
        {
            return new RequestResult<TResultDataType>(true, new ErrorItem[] { }, data);
        }

        /// <summary>
        /// Handle fail while processing request
        /// </summary>
        /// <param name="errors"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static RequestResult<TResultDataType> Fail(IEnumerable<ErrorItem> errors, TResultDataType? data = default)
        {
            return new RequestResult<TResultDataType>(false, errors, data);
        }

        /// <summary>
        /// Handle forbiden error
        /// </summary>
        /// <returns></returns>
        public static RequestResult<TResultDataType> Forbid(string message, Tracker tracker)
        {
            throw new UnauthorizedException(nameof(RequestResult<TResultDataType>), nameof(UnauthorizedException), message, tracker);
            //return new RequestResult<TResultDataType>(false, errors, data, true);
        }
    }
}