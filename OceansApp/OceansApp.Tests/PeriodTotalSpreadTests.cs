using OceansApp.DataAccess.Services;
using Xunit;

namespace OceansApp.Tests
{
    /// <summary>
    /// Tests for the pure period-total spread helper. Cover the invariant the feature exists to
    /// protect — entered total == filed total — across the default, override, half-hour, and
    /// degenerate (tiny total) cases, plus rejection of non-positive inputs. See docs/adr/0003.
    /// </summary>
    public class PeriodTotalSpreadTests
    {
        [Theory]
        [InlineData(150, 21)]   // override, doesn't divide evenly
        [InlineData(167.5, 21)] // half-hour total
        [InlineData(168, 21)]   // default 21 × 8
        [InlineData(80, 10)]
        [InlineData(100, 3)]    // rounds up -> negative remainder folded onto last day
        [InlineData(0.5, 21)]   // tiny total
        public void Spread_SumsToExactlyTheEnteredTotal(decimal total, int days)
        {
            var spread = PeriodTotalSpread.Spread(total, days);

            Assert.Equal(days, spread.Count);
            Assert.Equal(total, spread.Sum());
        }

        [Fact]
        public void Spread_DefaultCase_IsFlatEightPerDay_NoRemainderDay()
        {
            // total = N × 8 divides exactly: every day is 8.00 and no day differs.
            var spread = PeriodTotalSpread.Spread(21 * 8m, 21);

            Assert.All(spread, q => Assert.Equal(8.00m, q));
        }

        [Fact]
        public void Spread_Override_DistributesRemainderAcrossTrailingDays()
        {
            // 15000 cents / 21 = 714 base (7.14), remainder 6 -> the last 6 days carry 7.15.
            // Max 1-cent spread between days, and the remainder sits on the tail. Sum is exact.
            var spread = PeriodTotalSpread.Spread(150m, 21);

            Assert.All(spread.Take(15), q => Assert.Equal(7.14m, q));
            Assert.All(spread.Skip(15), q => Assert.Equal(7.15m, q));
            Assert.Equal(150m, spread.Sum());
        }

        [Fact]
        public void Spread_HalfHourTotal_StaysExact()
        {
            // 16750 cents / 21 = 797 base (7.97), remainder 13 -> the last 13 days carry 7.98.
            var spread = PeriodTotalSpread.Spread(167.5m, 21);

            Assert.All(spread.Take(8), q => Assert.Equal(7.97m, q));
            Assert.All(spread.Skip(8), q => Assert.Equal(7.98m, q));
            Assert.Equal(167.5m, spread.Sum());
        }

        [Fact]
        public void Spread_TinyTotal_DropsZeroDays_NonZeroOnTrailingDays()
        {
            // 5 cents over 21 days: base 0, remainder 5 -> last 5 days = 0.01, first 16 = 0.00.
            // The leading 0.00 days are the ones the caller drops; sum stays exact.
            var spread = PeriodTotalSpread.Spread(0.05m, 21);

            Assert.Equal(0.05m, spread.Sum());
            Assert.Equal(16, spread.Count(q => q == 0m));
            Assert.All(spread.Skip(16), q => Assert.Equal(0.01m, q));
        }

        [Fact]
        public void Spread_NeverProducesNegativeQuantities()
        {
            // The folded remainder must never drive a day below zero across realistic inputs.
            for (var days = 1; days <= 23; days++)
            {
                for (decimal total = 0.5m; total <= 400m; total += 0.5m)
                {
                    var spread = PeriodTotalSpread.Spread(total, days);
                    Assert.All(spread, q => Assert.True(q >= 0, $"negative qty for total={total}, days={days}"));
                    Assert.Equal(total, spread.Sum());
                }
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Spread_NonPositiveWorkableDays_Throws(int days)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PeriodTotalSpread.Spread(100m, days));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Spread_NonPositiveTotal_Throws(decimal total)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PeriodTotalSpread.Spread(total, 21));
        }

        [Theory]
        [InlineData(0.004)]  // positive but rounds to 0 cents
        [InlineData(0.001)]
        public void Spread_PositiveButSubCentTotal_Throws(decimal total)
        {
            // Must not silently spread to all-zero days (which would file an empty submission).
            Assert.Throws<ArgumentOutOfRangeException>(() => PeriodTotalSpread.Spread(total, 21));
        }
    }
}
