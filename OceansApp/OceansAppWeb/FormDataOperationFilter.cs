using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OceansAppWeb
{
    public class FormDataOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var formParams = context.MethodInfo.GetParameters()
                .Where(p => p.GetCustomAttributes(typeof(FromFormAttribute), false).Any())
                .ToArray();

            if (formParams.Length > 0)
            {
                var schemaProperties = new Dictionary<string, OpenApiSchema>();

                foreach (var param in formParams)
                {
                    var paramType = param.ParameterType;
                    OpenApiSchema schema;

                    if (paramType == typeof(int) || paramType == typeof(int?))
                    {
                        schema = new OpenApiSchema { Type = "integer", Format = "int32" };
                    }
                    else if (paramType == typeof(long) || paramType == typeof(long?))
                    {
                        schema = new OpenApiSchema { Type = "integer", Format = "int64" };
                    }
                    else if (paramType == typeof(string))
                    {
                        schema = new OpenApiSchema { Type = "string" };
                    }
                    else if (paramType == typeof(bool) || paramType == typeof(bool?))
                    {
                        schema = new OpenApiSchema { Type = "boolean" };
                    }
                    else
                    {
                        schema = context.SchemaGenerator.GenerateSchema(paramType, context.SchemaRepository);
                    }

                    schemaProperties[param.Name] = schema;
                }

                operation.RequestBody = new OpenApiRequestBody
                {
                    Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = schemaProperties,
                            Required = schemaProperties.Keys.ToHashSet()
                        }
                    }
                }
                };
            }
        }
    }
}
