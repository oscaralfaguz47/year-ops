
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace OceansApp.Utility.Configuration.AuthorizationRequirement.Recruiting
{
    public class AnyOfPoliciesRecruitingRequirementHandler : AuthorizationHandler<AnyOfPoliciesRecruitingRequirement>
    {
        private readonly IServiceProvider _serviceProvider;

        public AnyOfPoliciesRecruitingRequirementHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AnyOfPoliciesRecruitingRequirement requirement)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var authorizationService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

                var policies = new List<string>
            {
                "AccessToManageInterviews"
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
