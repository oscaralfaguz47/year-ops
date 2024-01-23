
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.RegularExpressions;

namespace OceansApp.Utility.SharedMethods.InputValidations
{
    public class ValidateInputs
    {

        public void ValidateRequiredAndStringLength(string field, string fieldName, string stringToValidate, int maxCharacterNum, ModelStateDictionary modelState)
        {
            if (string.IsNullOrEmpty(stringToValidate.Trim()) || stringToValidate.Trim().Length > maxCharacterNum)
            {
                modelState.AddModelError(field, $"The {fieldName} must be between 1 and {maxCharacterNum} characters.");
            }
        }

        public void ValidateNotRequiredAndStringLength(string field, string fieldName, string stringToValidate, int maxCharacterNum, ModelStateDictionary modelState)
        {
            if (stringToValidate.Trim() != "" && stringToValidate.Trim() != null)
            {
                if (stringToValidate.Trim().Length > maxCharacterNum)
                {
                    modelState.AddModelError(field, $"The {fieldName} must be between 1 and {maxCharacterNum} characters.");
                }
            }
        }
        public void ValidateRequiredFieldStringValue(string field, string fieldName, string? stringToValidate, ModelStateDictionary modelState)
        {
            if (stringToValidate == null || stringToValidate.Trim() == "")
            {
                    modelState.AddModelError(field, $"The {fieldName} is required.");
            }
        }
        public void ValidateRequiredFieldNumberValue(string field, string fieldName, decimal? numToValidate, ModelStateDictionary modelState)
        {
            if (numToValidate == null)
            {
                modelState.AddModelError(field, $"The {fieldName} is required.");
            }
        }
        public void ValidateEmail(string field, string fieldName, string email, ModelStateDictionary modelState)
        {
            if (email.Trim() != null && email.Trim() != "")
            {
                ValidateData validateData = new();
                if (!validateData.IsValidEmail(email.Trim()))
                {
                    modelState.AddModelError(field, $"The {fieldName} is not a valid email.");
                }
            }
        }
        public void ValidateListOfEmails(string field, string fieldName, List<string> listOfEmails, ModelStateDictionary modelState)
        {
            var count = 0;
            foreach (var email in listOfEmails)
            {
                count++;
                if (email.Trim() != null && email.Trim() != "")
                {
                    ValidateData validateData = new();
                    if (!validateData.IsValidEmail(email.Trim()))
                    {
                        modelState.AddModelError(field, $"The email #{count} for {fieldName} is not a valid email. You are putting: ({email}). Please correct it!");
                    }
                }
            }
        }
        public void ValidateNotRequiredEmail(string field, string fieldName, string email, ModelStateDictionary modelState)
        {
            if (email.Trim() != null && email.Trim() != "")
            {
                modelState.AddModelError(field, $"The {fieldName} is not a valid email.");
            }
        }

        public void ValidateDateValidFormat(string field, string fieldName, string dateValue, ModelStateDictionary modelState)
        {
            var validationResult = ValidateDateFormat(dateValue, fieldName);

            if (!validationResult.Result)
            {
                modelState.AddModelError(field, validationResult.ResultFalseMessage);
            }
        }

        public void ValidateNoNegativeNumber(string field, string fieldName, decimal numToValidate, ModelStateDictionary modelState)
        {
            if (numToValidate < 0)
            {
                modelState.AddModelError(field, $"The {fieldName} can not be a negative number.");
            }
        }
        public void ValidateMinAndMaxLenthNumber(string field, string fieldName, decimal? numToValidate, decimal minNum, decimal maxNum, ModelStateDictionary modelState)
        {
            if (numToValidate != null)
            {
                if (numToValidate < minNum || numToValidate > maxNum)
                {
                    modelState.AddModelError(field, $"The {fieldName} must be between {minNum} and {maxNum}.");
                }
            }
        }

        //PRIVATE
        private class ValidateResponse
        {
            public bool Result { get; set; }
            public string ResultFalseMessage { get; set; }
        }

        private ValidateResponse ValidateDateFormat(string dateString, string field)
        {
            ValidateResponse response = new ValidateResponse();

            if (string.IsNullOrEmpty(dateString))
            {
                response.Result = true;
                return response;
            }
            bool isValidDate = DateTime.TryParse(dateString, out DateTime fechaConvertida);

            if (isValidDate)
            {
                response.Result = true;
            }
            else
            {
                response.Result = false;
                response.ResultFalseMessage = $"The {field} is not a valid Date.";
            }
            return response;
        }
    }
}
