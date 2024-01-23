using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace OceansApp.Utility.Configuration.AuthorizationRequirement.ProjectManagement
{
    public class AnyOfPoliciesProjectManagementRequirementHandler : AuthorizationHandler<AnyOfPoliciesProjectManagementRequirement>
    {
        private readonly IServiceProvider _serviceProvider;

        public AnyOfPoliciesProjectManagementRequirementHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AnyOfPoliciesProjectManagementRequirement requirement)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var authorizationService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

                var policies = new List<string>
            {
                "AccessToClientsPage",
                "AccessToProjectsPage"
            };

                foreach (var policy in policies)
                {
                    var policyResult = await authorizationService.AuthorizeAsync(context.User, policy);
                    if (policyResult.Succeeded)
                    {
                        context.Succeed(requirement);
                        return;
                    }
                }
            }
        }
    }
}
