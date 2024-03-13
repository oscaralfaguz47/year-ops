
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections;

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
            if (stringToValidate != null)
            {
                if (stringToValidate != "")
                {
                    if (stringToValidate.Trim() != "")
                    {
                        if (stringToValidate.Trim().Length > maxCharacterNum)
                        {
                            modelState.AddModelError(field, $"The {fieldName} must be between 1 and {maxCharacterNum} characters.");
                        }
                    }
                }
            }
        }
        public void ValidateRequiredFieldStringValue(string field, string fieldName, string? stringToValidate, ModelStateDictionary modelState)
        {
            if (stringToValidate != null)
            {
                if (stringToValidate.Trim() == "")
                {
                    modelState.AddModelError(field, $"The {fieldName} is required.");
                }
            }
            else
            {
                modelState.AddModelError(field, $"The {fieldName} is required.");
            }
        }
        public void ValidateRequiredFieldAnyValue(string field, string fieldName, object valueToValidate, ModelStateDictionary modelState)
        {
            if (valueToValidate == null)
            {
                modelState.AddModelError(field, $"The {fieldName} is required.");
            }else if (valueToValidate is string && string.IsNullOrWhiteSpace(valueToValidate as string))
            {
                modelState.AddModelError(field, $"The {fieldName} is required.");
            }
        }
        public void ValidateRequiredFieldBooleanType(string field, string fieldName, object valueToValidate, ModelStateDictionary modelState)
        {
            if (valueToValidate == null)
            {
                modelState.AddModelError(field, $"The {fieldName} is required.");
            }
            else if (valueToValidate is not bool)
            {
                modelState.AddModelError(field, $"The {fieldName} value should be a boolean.");
            }
        }
        public void ValidateRequiredFieldIntType(string field, string fieldName, object? valueToValidate, ModelStateDictionary modelState)
        {
            if (valueToValidate == null)
            {
                modelState.AddModelError(field, $"The {fieldName} is required.");
            }
            else if (!(valueToValidate is int))
            {
                modelState.AddModelError(field, $"The {fieldName} value should be an int.");
            }
        }

        public void ValidateRequiredFieldNumberValue(string field, string fieldName, object numToValidate, ModelStateDictionary modelState)
        {
            if (numToValidate == null)
            {
                modelState.AddModelError(field, $"The {fieldName} is required.");
            }
            else
            {
                if (!(numToValidate is int || numToValidate is decimal || numToValidate is float))
                {
                    modelState.AddModelError(field, $"The {fieldName} should be a number.");
                }
            }
        }
        public void ValidateEmail(string field, string fieldName, string email, ModelStateDictionary modelState)
        {
            if (email != null)
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

        public void ValidateDateValidFormat(string field, string fieldName, object? dateValue, ModelStateDictionary modelState)
        {
            if (dateValue != null)
            {
                var validationResult = ValidateDateFormat(dateValue.ToString(), fieldName);

                if (!validationResult.Result)
                {
                    modelState.AddModelError(field, validationResult.ResultFalseMessage);
                }
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
        public void ValidateNotEmptyArray(string field, string fieldName, object arrayToValidate, ModelStateDictionary modelState)
        {
            if (arrayToValidate != null)
            {
                if (arrayToValidate is IEnumerable array)
                {
                    if (!array.Cast<object>().Any())
                    {
                        modelState.AddModelError(field, $"The {fieldName} is required.");
                    }
                }
            }
        }

        //PRIVATE
        private class ValidateResponse
        {
            public bool Result { get; set; }
            public string ResultFalseMessage { get; set; }
        }

        private ValidateResponse ValidateDateFormat(object dateString, string field)
        {
            ValidateResponse response = new ValidateResponse();

            if (dateString != null)
            {
                if (string.IsNullOrEmpty(dateString.ToString()))
                {
                    response.Result = true;
                    return response;
                }
                bool isValidDate = DateTime.TryParse(dateString.ToString(), out DateTime fechaConvertida);

                if (isValidDate)
                {
                    response.Result = true;
                }
                else
                {
                    response.Result = false;
                    response.ResultFalseMessage = $"The {field} is not a valid Date.";
                }
            }
            else
            {
                response.Result = false;
                response.ResultFalseMessage = $"The {field} is required to add a valid date.";
            }
            return response;
        }
    }
}
