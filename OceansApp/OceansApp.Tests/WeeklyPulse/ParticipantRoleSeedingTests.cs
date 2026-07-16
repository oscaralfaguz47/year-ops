using OceansApp.DataAccess.DbInitializer;
using OceansApp.Utility.ConstantData.Claims.WeeklyPulse;

namespace OceansApp.Tests.WeeklyPulse
{
    // WP-CR3: the 'Weekly Pulse Participant' role must carry ONLY the Participate
    // claim (never Administer), and re-seeding must not duplicate the claim.
    public class ParticipantRoleSeedingTests
    {
        private static readonly (string ClaimType, string ClaimValue) Participate =
            (WeeklyPulseClaimsCD.Participate_ClaimType, WeeklyPulseClaimsCD.Participate_ClaimValue);

        private static readonly (string ClaimType, string ClaimValue) Administer =
            (WeeklyPulseClaimsCD.Administer_ClaimType, WeeklyPulseClaimsCD.Administer_ClaimValue);

        [Fact]
        public void ParticipantRole_GrantsParticipate_ButNotAdminister()
        {
            var desired = DbInitializer.WeeklyPulseParticipantRoleClaims;

            Assert.Contains(Participate, desired);
            Assert.DoesNotContain(Administer, desired);
        }

        [Fact]
        public void BuildRoleClaimsToInsert_OnFreshRole_InsertsParticipateClaim()
        {
            var existing = new HashSet<(string, string)>();

            var toInsert = DbInitializer.BuildRoleClaimsToInsert(
                "role-participant",
                DbInitializer.WeeklyPulseParticipantRoleClaims,
                existing,
                createdBy: "master-user",
                creationDate: new DateTime(2026, 7, 8));

            var single = Assert.Single(toInsert);
            Assert.Equal(WeeklyPulseClaimsCD.Participate_ClaimType, single.ClaimType);
            Assert.Equal(WeeklyPulseClaimsCD.Participate_ClaimValue, single.ClaimValue);
            Assert.Equal("role-participant", single.RoleId);
        }

        [Fact]
        public void BuildRoleClaimsToInsert_WhenClaimAlreadyPresent_IsIdempotent()
        {
            var existing = new HashSet<(string, string)> { Participate };

            var toInsert = DbInitializer.BuildRoleClaimsToInsert(
                "role-participant",
                DbInitializer.WeeklyPulseParticipantRoleClaims,
                existing,
                createdBy: "master-user",
                creationDate: new DateTime(2026, 7, 8));

            Assert.Empty(toInsert);
        }
    }
}
