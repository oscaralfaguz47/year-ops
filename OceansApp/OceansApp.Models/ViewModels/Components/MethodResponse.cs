
namespace OceansApp.Models.ViewModels.Components
{
    public class MethodResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? MessageType { get; set; } //Saving Error, Validation Error, Exception Error, No Exists Error, Not Found, Duplication
        public int? IdCreatedElement { get; set; }

        public MethodResponse()
        {
        }
        private MethodResponse(bool success, string message, string messageType, int? idCreatedElement = null)
        {
            Success = success;
            Message = message;
            MessageType = messageType;
            IdCreatedElement = idCreatedElement;
        }
        public static MethodResponse CreateSuccessResponse(string? message = null, int? idCreatedElement = null)
        {
            return new MethodResponse(true, message, null, idCreatedElement);
        }

        public static MethodResponse CreateFailureExceptionResponse(string? message = null)
        {
            return new MethodResponse(false, message, "Exception Error");
        }
        public static MethodResponse CreateFailureValidationResponse(string message)
        {
            return new MethodResponse(false, message, "Validation Error");
        }
        public static MethodResponse CreateFailureNotFoundResponse(string message)
        {
            return new MethodResponse(false, message, "Not Found");
        }
    }
}
