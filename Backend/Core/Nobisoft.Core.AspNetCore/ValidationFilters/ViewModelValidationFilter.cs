using Microsoft.AspNetCore.Mvc.Filters;
using Nobisoft.Core.ErrorModels;
using Nobisoft.Core.Exceptions;

namespace Nobisoft.Core.AspNetCore.ValidationFilters
{
    /// <summary>
    /// To generate ViewModel error friendly with user once it raises errors
    /// </summary>
    public class ViewModelValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Before controller execution, binding process
            if (!context.ModelState.IsValid)
            {
                var errorsInModelState = context.ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(e => e.Key, e => e.Value.Errors.Select(x => x.ErrorMessage)).ToArray();
                var errorResponses = new List<ErrorModel>();
                foreach (var (key, value) in errorsInModelState)
                {
                    errorResponses.AddRange(value.Select(subError => new ErrorModel()
                        {FieldName = key, Error = subError}));
                }

                throw new BadRequestException().WithData("Errors", errorResponses);
            }

            // If you can go here, it means there is no model state error then your request will be processed by the controller
            await next();
        }
    }
}