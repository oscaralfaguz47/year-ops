
namespace OceansApp.Utility.ConstantData
{
    public class BenefitsCD
    {
        public const string Balance_Program = "1BalanceProgram";
        public const string Bonusly = "2Bonusly";
        public const string Oceans_Challenge = "3OceansChallenge";
        public const string VTO = "4VTO";

        // Fixed VTO entitlement: 1 voluntary day per year for every employee (ADR 0003 — a
        // configurable version — was withdrawn). Kept here so the value lives in one place.
        public const int VtoAnnualDays = 1;
    }
}
