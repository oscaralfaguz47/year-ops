using Microsoft.Data.SqlClient;

namespace OceansAppWeb
{
    public class DatabaseExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DatabaseExceptionMiddleware> _logger;

        public DatabaseExceptionMiddleware(RequestDelegate next, ILogger<DatabaseExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (SqlException ex) when (ex.Number == -2 || ex.Number == 40613)
            {
                _logger.LogError(ex, "Database connection failed.");
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("Database is starting up, please try again in a moment.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("An unexpected error occurred. Please try again later.");
            }
        }
    }
}
