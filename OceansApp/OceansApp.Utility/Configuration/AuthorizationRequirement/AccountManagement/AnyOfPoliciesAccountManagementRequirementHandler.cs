using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace OceansApp.Utility.Configuration.AuthorizationRequirement.AccountManagement
{
    public class AnyOfPoliciesAccountManagementRequirementHandler : AuthorizationHandler<AnyOfPoliciesAccountManagementRequirement>
    {
        private readonly IServiceProvider _serviceProvider;

        public AnyOfPoliciesAccountManagementRequirementHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AnyOfPoliciesAccountManagementRequirement requirement)
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
