using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace OceansApp.Utility.Configuration.AuthorizationRequirement.AdminCenter
{
    public class AnyOfPoliciesAdminCenterRequirementHandler : AuthorizationHandler<AnyOfPoliciesAdminCenterRequirement>
    {
        private readonly IServiceProvider _serviceProvider;

        public AnyOfPoliciesAdminCenterRequirementHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AnyOfPoliciesAdminCenterRequirement requirement)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var authorizationService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

                // Lista de políticas a verificar
                var policies = new List<string>
            {
                "AccessToUpdateDataFromSoftlandSection",
                "AccessToUserAdministration",
                "AccessToUserRolesAndPermissions",
                "AccessToConsultantPositions"
            };

                foreach (var policy in policies)
                {
                    var policyResult = await authorizationService.AuthorizeAsync(context.User, policy);
                    if (policyResult.Succeeded)
                    {
                        context.Succeed(requirement); // Si alguna política se cumple, se concede acceso
                        return;
                    }
                }
            }
        }
    }

}
