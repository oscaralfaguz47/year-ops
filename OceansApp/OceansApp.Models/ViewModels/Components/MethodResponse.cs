
namespace OceansApp.Models.ViewModels.Components
{
    public class MethodResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? MessageType { get; set; } //Saving Error, Validation Error, Exception Error, No Exists Error
    }
}
