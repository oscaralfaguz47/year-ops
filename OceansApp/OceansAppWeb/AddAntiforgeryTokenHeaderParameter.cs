using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OceansAppWeb
{
    public class AddAntiforgeryTokenHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            // Only add the token header parameter to POST, PUT, DELETE requests
            if (context.ApiDescription.HttpMethod == "POST" ||
                context.ApiDescription.HttpMethod == "PUT" ||
                context.ApiDescription.HttpMethod == "DELETE")
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "RequestVerificationToken",
                    In = ParameterLocation.Header,
                    Description = "Antiforgery token",
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    }
                });
            }
        }
    }
}
