using HotelBooking.Specs.Support;
using TechTalk.SpecFlow;

namespace HotelBooking.Specs.StepDefinitions
{
    /// <summary>
    /// Step definitions specific to the "Fully Occupied Dates" feature.
    /// </summary>
    [Binding]
    public sealed class FullyOccupiedDatesSteps
    {
        private readonly BookingTestContext _ctx;

        public FullyOccupiedDatesSteps(BookingTestContext ctx) => _ctx = ctx;

        // ── Given ──────────────────────────────────────────────────────────────

        [Given(@"I query occupied dates from ""(.*)"" to ""(.*)""")]
        public void GivenIQueryOccupiedDatesFromTo(string startToken, string endToken)
        {
            _ctx.OccupiedQueryStart = BookingTestContext.ParseDate(startToken);
            _ctx.OccupiedQueryEnd = BookingTestContext.ParseDate(endToken);

            // Build the SUT if it hasn't been built yet
            // (happens when "there are no bookings" step runs before this step).
            if (_ctx.BookingManager == null)
                _ctx.BuildBookingManager();
        }

        // ── When ───────────────────────────────────────────────────────────────

        [When(@"I request the fully occupied dates")]
        public async Task WhenIRequestTheFullyOccupiedDates()
        {
            try
            {
                _ctx.OccupiedDatesResult = await _ctx.BookingManager
                    .GetFullyOccupiedDates(_ctx.OccupiedQueryStart, _ctx.OccupiedQueryEnd);
            }
            catch (Exception ex)
            {
                _ctx.ThrownException = ex;
            }
        }

        // ── Then ───────────────────────────────────────────────────────────────

        [Then(@"the result should contain exactly (\d+) dates")]
        public void ThenTheResultShouldContainExactlyDates(int expectedCount)
        {
            Assert.Null(_ctx.ThrownException);
            Assert.NotNull(_ctx.OccupiedDatesResult);
            Assert.Equal(expectedCount, _ctx.OccupiedDatesResult!.Count);
        }

        [Then(@"all dates from ""(.*)"" to ""(.*)"" should be in the result")]
        public void ThenAllDatesFromToShouldBeInTheResult(string startToken, string endToken)
        {
            var start = BookingTestContext.ParseDate(startToken);
            var end = BookingTestContext.ParseDate(endToken);

            for (var d = start; d <= end; d = d.AddDays(1))
            {
                Assert.Contains(d, _ctx.OccupiedDatesResult!);
            }
        }
    }
}
