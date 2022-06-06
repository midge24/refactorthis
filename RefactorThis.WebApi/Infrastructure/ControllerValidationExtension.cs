using Microsoft.AspNetCore.Mvc;
using RefactorThis.Helpers;
using System.ComponentModel.DataAnnotations;

namespace RefactorThis.WebApi.Infrastructure
{
    public static class ControllerValidationExtension
    {
		/// <summary>
		/// This function will allow the controller to test its model and to be unit testable
		/// it will also attempt at assigning the ValidationResults to the object being validated 
		/// if this has not been done already.
		/// </summary>
		/// <typeparam name="TViewModel"></typeparam>
		/// <typeparam name="TController"></typeparam>
		/// <param name="controller"></param>
		/// <param name="viewModelToValidate"></param>
		/// <returns></returns>
		public static bool ValidateViewModel<TViewModel, TController>(this TController controller, TViewModel viewModelToValidate) where TViewModel : ValidationExtensions
			where TController : ControllerBase
		{
			var validationResults = new List<ValidationResult>();
			object viewModelToValidateReplacement = new DefaultValidatableObject();
			bool valid;
			ValidationContext validationContext;

			if (viewModelToValidate is not null && viewModelToValidate.IsValidated())
			{
				valid = viewModelToValidate.IsValid();

				foreach (var validationResult in viewModelToValidate.GetValidationResults())
				{
					controller.ModelState.AddModelError(validationResult.MemberNames.FirstOrDefault() ?? string.Empty, validationResult.ErrorMessage!);
				}
			}
			else
			{
				validationContext = new(viewModelToValidate ?? viewModelToValidateReplacement, null, null);
				valid = Validator.TryValidateObject(viewModelToValidate ?? viewModelToValidateReplacement, validationContext, validationResults, true);

				//Those are the previous validation results saved with the object, if there are none then the object is most likely to have been validated here.
				var objectValidatedDictionary = ((RefactorThis.Helpers.ValidationExtensions)validationContext.ObjectInstance).GetValidationResults();

				//Assign the validation results if there are ValidationResults but non associated with the object
				if (validationResults.Any() && !objectValidatedDictionary.Any())
					((RefactorThis.Helpers.ValidationExtensions)validationContext.ObjectInstance).SetValidationResults(validationResults);

				foreach (var validationResult in validationResults)
				{
					controller.ModelState.AddModelError(validationResult.MemberNames.FirstOrDefault() ?? string.Empty, validationResult.ErrorMessage!);
				}
			}

			return valid;
		}

	}
}
