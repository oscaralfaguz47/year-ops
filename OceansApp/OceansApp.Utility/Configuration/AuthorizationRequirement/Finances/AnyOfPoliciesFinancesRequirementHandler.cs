
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace OceansApp.Utility.Configuration.AuthorizationRequirement.Finances
{
    public class AnyOfPoliciesFinancesRequirementHandler : AuthorizationHandler<AnyOfPoliciesFinancesRequirement>
    {
        private readonly IServiceProvider _serviceProvider;

        public AnyOfPoliciesFinancesRequirementHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AnyOfPoliciesFinancesRequirement requirement)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var authorizationService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

                var policies = new List<string>
            {
                "AccessToAccountsReceivable",
                "AccessToManageConsultantPaymentsDebitsAndCredits",
                "AccessToManageTheBasicsOfPaymentSheets"
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
