using Azure.Storage.Queues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.TimeOff;
using Xunit;

namespace OceansApp.Tests
{
    // Backstop coverage for VTO-1.1: the submit-validation path must hard-block an
    // over-allowance VTO request (no TimeOffRequest row created) while still accepting
    // an in-allowance one (lands at "Waiting to be approved"). The yearly allowance is
    // the fixed CONSTANT of 1 day/year (ADR 0003 withdrawn — no config).
    public class VtoSubmitValidationTests
    {
        private const int ConsultantId = 1;
        private const string UserId = "user-1";

        private static ApplicationDbContext NewContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ApplicationDbContext(options);
        }

        private static void Seed(ApplicationDbContext db, bool withExistingVtoDay)
        {
            db.TRANSACTION_STATUSES.Add(new TransactionStatus { TransactionStatusId = 1, Name = "Approved" });
            db.TRANSACTION_STATUSES.Add(new TransactionStatus { TransactionStatusId = 2, Name = "Waiting to be approved" });

            var category = new ApplicationUserCategory { UserCategoryId = 1, Name = "Consultant" };
            db.UserCategories.Add(category);

            var user = new ApplicationUser
            {
                Id = UserId,
                Name = "Test",
                LastName = "Consultant",
                Email = "test.consultant@oceanscode.com",
                UserCategoryId = category.UserCategoryId,
                ApplicationUserCategory = category
            };
            db.AspNetUsers.Add(user);

            db.CONSULTANT_DETAILS.Add(new ConsultantDetail
            {
                ConsultantId = ConsultantId,
                UserId = UserId,
                ApplicationUser = user,
                IdCountry = "CR",
                StartDate = new DateTime(2020, 1, 1),
                IsEligibleForPaidTimeOff = false,
                ConsultantHolidayId = null
            });

            if (withExistingVtoDay)
            {
                // One VTO day already used this year (pending counts against the balance) -> VtoAvailable = 0.
                db.TIME_OFF_REQUESTS.Add(new TimeOffRequest
                {
                    ConsultantId = ConsultantId,
                    TimeOffType = "VTO",
                    StartDate = DateTime.UtcNow.Date,
                    EndDate = DateTime.UtcNow.Date,
                    BusinessDays = 1,
                    TransactionStatusId = 2, // Waiting to be approved
                    CreationDate = DateTime.UtcNow,
                    UserCreatedBy = UserId
                });
            }

            db.SaveChanges();
        }

        private static TimeOffRequestRepository NewRepository(ApplicationDbContext db)
        {
            var config = new ConfigurationBuilder().Build();
            var queue = new Lazy<QueueClient>(() => null!); // email sends are best-effort (try/catch) — never reached on the reject path.
            return new TimeOffRequestRepository(db, config, queue);
        }

        private static DateTime NextWeekday()
        {
            var d = DateTime.UtcNow.Date.AddDays(1);
            while (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                d = d.AddDays(1);
            return d;
        }

        [Fact]
        public async Task SubmitRequest_OverAllowanceVto_IsRejected_AndCreatesNoRow()
        {
            using var db = NewContext(nameof(SubmitRequest_OverAllowanceVto_IsRejected_AndCreatesNoRow));
            Seed(db, withExistingVtoDay: true); // VtoAvailable = 0

            var day = NextWeekday();
            var data = new SubmitTimeOffRequestVM { TimeOffType = "VTO", StartDate = day, EndDate = day };

            var response = await NewRepository(db).SubmitRequestAsync(UserId, ConsultantId, data, "https://localhost");

            Assert.False(response.Success);
            // No NEW TimeOffRequest row created — only the single pre-seeded one remains.
            Assert.Equal(1, await db.TIME_OFF_REQUESTS.CountAsync());
        }

        [Fact]
        public async Task SubmitRequest_InAllowanceVto_IsAccepted_AsWaitingToBeApproved()
        {
            using var db = NewContext(nameof(SubmitRequest_InAllowanceVto_IsAccepted_AsWaitingToBeApproved));
            Seed(db, withExistingVtoDay: false); // VtoAvailable = 1

            var day = NextWeekday();
            var data = new SubmitTimeOffRequestVM { TimeOffType = "VTO", StartDate = day, EndDate = day };

            var response = await NewRepository(db).SubmitRequestAsync(UserId, ConsultantId, data, "https://localhost");

            Assert.True(response.Success);

            var created = await db.TIME_OFF_REQUESTS.Include(r => r.TransactionStatus).SingleAsync();
            Assert.Equal("VTO", created.TimeOffType);
            Assert.Equal(2, created.TransactionStatusId); // Waiting to be approved
        }
    }
}
