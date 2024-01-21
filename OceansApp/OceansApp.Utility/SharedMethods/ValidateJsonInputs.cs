
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;

namespace OceansApp.Utility.SharedMethods
{
    public class ValidateJsonInputs
    {
        public class ValidateResponse
        {
            public bool Result { get; set; }
            public string ResultFalseMessage { get; set; }
        }

        public ValidateResponse ValidateDateFormat(string dateString, string field)
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

        public void ValidateAndAddModelError(string dateValue, string fieldName, ModelStateDictionary modelState)
        {
            var validationResult = ValidateDateFormat(dateValue, fieldName);

            if (!validationResult.Result)
            {
                modelState.AddModelError(fieldName, validationResult.ResultFalseMessage);
            }
        }

    }
}
