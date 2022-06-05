using Microsoft.AspNetCore.Mvc;
using RefactorThis.Helpers;
using RefactorThis.Persistence.Models;

namespace RefactorThis.WebApi.Infrastructure
{
    /// <summary>
    /// Extension of the controller base class to allow more flexibility with the validation
    /// </summary>
    public class ControllerBaseExtended: ControllerBase
    {
        private readonly ILogger _logger;

        public ControllerBaseExtended(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// This wrapper is here to use the same calling name as the function below, nothing else.
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        protected ActionResult SendBadRequest(object requestDto )
        {
            return BadRequest(requestDto);
        }

        /// <summary>
        /// This function will allow to use the ValidationProblemDetails that are normally returned when the HTTP400 errors are automatically thrown.
        /// This allows the front end to use the same way to display the validation errors.
        /// If the object doesn't have any validation errors then it will force a revalidation.
        /// </summary>
        /// <param name="requestDto"></param>        
        /// <returns></returns>
        protected IActionResult SendBadRequest(ValidationExtensions requestDto)
        {
            if (requestDto is null)
                requestDto = new DefaultValidatableObject();

            var err = new List<ReLeasedValidationResult>();

            var valRes = requestDto?.GetValidationResults();

            if (valRes?.Count > 0)
                err.AddRange(valRes);
            else
                err.AddRange(requestDto.Validate(_logger));

            var dic2 = new Dictionary<string, string[]>();

            err.ForEach(x =>
            {
                foreach (var member in x.MemberNames)
                {
                    if (dic2.ContainsKey(member))
                    {
                        var val = dic2[member];
                        var updatedVal = val.Append(x.ErrorMessage);
                        dic2[member] = updatedVal.ToArray();
                    }
                    else
                        dic2.TryAdd(member, new string[] { x.ErrorMessage });
                }
            });

            ValidationProblemDetails det = new(dic2);

            return ValidationProblem(det);
        }
    }
}

