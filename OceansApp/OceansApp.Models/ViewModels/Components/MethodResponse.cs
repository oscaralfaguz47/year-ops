
namespace OceansApp.Models.ViewModels.Components
{
    public class MethodResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? MessageType { get; set; } //Saving Error, Validation Error, Exception Error, No Exists Error, Not Found, Duplication
        public int? IdCreatedElement { get; set; }
        public string? FieldName { get; set; }
        public List<CreatedElement>? CreatedElementsList { get; set; }
        public List<string> StringsList { get; set; }
        public MethodResponse()
        {
        }
        public class CreatedElement
        {
            public int? IdElement { get; set; }
            public string? ElementType { get; set; }
        }

        private MethodResponse(bool success, string message, string messageType, int? idCreatedElement = null, 
            List<CreatedElement>? createdElementsList = null, List<string>? stringsList = null, string ? fieldName = null)
        {
            Success = success;
            Message = message;
            MessageType = messageType;
            IdCreatedElement = idCreatedElement;
            CreatedElementsList = createdElementsList;
            StringsList = stringsList;
            FieldName = fieldName;
        }
        public static MethodResponse CreateSuccessResponse(string? message = null, int? idCreatedElement = null, List<CreatedElement>? createdElementsList = null)
        {
            return new MethodResponse(true, message, null, idCreatedElement, createdElementsList);
        }
        public static MethodResponse CreateSuccessResponseStringsList(string? message = null, List<string>? stringsList = null)
        {
            return new MethodResponse(true, message, null, null, null, stringsList);
        }

        public static MethodResponse CreateFailureExceptionResponse(string? message = null)
        {
            return new MethodResponse(false, message, "Exception Error");
        }
        public static MethodResponse CreateFailureValidationResponse(string message, string? fieldName = null)
        {
            return new MethodResponse(false, message, "Validation Error", null, null, null, fieldName);
        }
        public static MethodResponse CreateFailureNotFoundResponse(string message)
        {
            return new MethodResponse(false, message, "Not Found");
        }
    }
}
