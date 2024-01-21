
namespace OceansAppWeb
{
    public class RedirectToDashboardMiddleware
    {
        private readonly RequestDelegate _next;

        public RedirectToDashboardMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/")
            {
                context.Response.Redirect("/Home/Dashboard");
            }
            else
            {
                await _next(context);
            }
        }
    }
}
