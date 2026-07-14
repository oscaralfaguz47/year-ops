using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Covers the pure To-Do surfacing rules: the Dashboard shows all non-Done to-dos,
    /// while the Review surfaces non-Done with Blocked loud and Done dropped. No
    /// DbContext/HttpContext — operates on a derived state.
    /// </summary>
    public class ToDoSurfacingTests
    {
        // Dashboard: surfaces until Done.
        [Theory]
        [InlineData(ToDoStatus.Open, true)]
        [InlineData(ToDoStatus.Blocked, true)]
        [InlineData(ToDoStatus.Done, false)]
        public void ShowsOnDashboard_AllNonDone(ToDoStatus state, bool expected)
        {
            Assert.Equal(expected, ReviewSurfacingService.ToDoShowsOnDashboard(state));
        }

        // Review: Done dropped (Hidden), Blocked loud, Open quiet.
        [Theory]
        [InlineData(ToDoStatus.Open, ToDoSurfacing.Quiet)]
        [InlineData(ToDoStatus.Blocked, ToDoSurfacing.Loud)]
        [InlineData(ToDoStatus.Done, ToDoSurfacing.Hidden)]
        public void SurfaceToDo_BlockedLoud_DoneDropped(ToDoStatus state, ToDoSurfacing expected)
        {
            Assert.Equal(expected, ReviewSurfacingService.SurfaceToDo(state));
        }

        [Fact]
        public void SurfaceToDo_BlockedIsLoud_NotQuiet()
        {
            // The acceptance criterion calls out Blocked surfacing "loud".
            Assert.Equal(ToDoSurfacing.Loud, ReviewSurfacingService.SurfaceToDo(ToDoStatus.Blocked));
        }
    }
}
