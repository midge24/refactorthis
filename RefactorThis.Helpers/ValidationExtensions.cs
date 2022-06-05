using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RefactorThis.Persistence.Models;

namespace RefactorThis.Helpers
{
    public abstract class ValidationExtensions: IValidatableObject
    {
        protected List<ReLeasedValidationResult> ValidationResults { get; set; }
        protected bool Validated { get; set; } = false;

        public IEnumerable<ReLeasedValidationResult> GetResultsIfAlreadyValidated()
        {
            if (Validated) return ValidationResults;

            return Validate();
        }

        public bool IsValidated() => Validated;

        public void SetValidated()
        {
            Validated = true;
        }

        /// <summary>
        /// This function will return the validation results associated with the current object.
        /// </summary>
        /// <returns></returns>
        public List<ReLeasedValidationResult> GetValidationResults()
        {
            if (ValidationResults is null)
                ValidationResults = new List<ReLeasedValidationResult>();

            return ValidationResults;
        }

        public void SetValidationResults(List<ValidationResult> input)
        {
            if (input is null) input = new List<ValidationResult>();
            ValidationResults = input.Select(x => new ReLeasedValidationResult(x.ErrorMessage, x.MemberNames)).ToList();
        }

        public bool IsValid()
        {
            return Validated && !ValidationResults.Any();
        }

        /// <summary>
        /// Returns the validation errors
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ReLeasedValidationResult> Validate()
        {
            ValidationResults = (List<ReLeasedValidationResult>)Validate(new ValidationContext(this));
            Validated = true;

            return ValidationResults.AsEnumerable();
        }

        /// <summary>
        /// Returns the validation errors and log them using the mentioned logger
        /// </summary>
        /// <param name="_logger"></param>
        /// <returns></returns>
        public IEnumerable<ReLeasedValidationResult> Validate(ILogger _logger = null)
        {
            ValidationResults = (List<ReLeasedValidationResult>)Validate(new ValidationContext(this));
            Validated = true;

            if (_logger is not null)
            {
                foreach (var validationError in ValidationResults)
                {
                    var errorMessage = string.IsNullOrEmpty(validationError.ErrorCode) ? $"Error: {validationError.ErrorMessage}" : $"Error Code: {validationError.ErrorCode}, Error: {validationError.ErrorMessage}";

                    _logger.LogError(errorMessage, validationError.MemberNames);
                }
            }

            return ValidationResults.AsEnumerable();
        }

        public abstract IEnumerable<ValidationResult> Validate(ValidationContext context);
    }

    public class DefaultValidatableObject : ValidationExtensions
    {
        public override IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            ValidationResults = new();
            ValidationResults.Add(new ReLeasedValidationResult("Request is null and cannot be validated", new[] { "Request" }));
            Validated = true;

            return ValidationResults;
        }
    }
}

