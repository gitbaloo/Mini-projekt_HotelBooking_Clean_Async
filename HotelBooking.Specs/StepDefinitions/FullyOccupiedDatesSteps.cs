using HotelBooking.Specs.Support;
using TechTalk.SpecFlow;

namespace HotelBooking.Specs.StepDefinitions
{
    /// <summary>
    /// Step definitions specific to the "Fully Occupied Dates" feature.
    /// </summary>
    [Binding]
    public sealed class FullyOccupiedDatesSteps(BookingTestContext context)
    {
        private readonly BookingTestContext _context = context;

        // ── Given ──────────────────────────────────────────────────────────────

        [Given(@"I query occupied dates from ""(.*)"" to ""(.*)""")]
        public void GivenIQueryOccupiedDatesFromTo(string startToken, string endToken)
        {
            _context.OccupiedQueryStart = BookingTestContext.ParseDate(startToken);
            _context.OccupiedQueryEnd = BookingTestContext.ParseDate(endToken);

            // Build the SUT if it hasn't been built yet
            // (happens when "there are no bookings" step runs before this step).
            if (_context.BookingManager == null)
                _context.BuildBookingManager();
        }

        // ── When ───────────────────────────────────────────────────────────────

        [When(@"I request the fully occupied dates")]
        public async Task WhenIRequestTheFullyOccupiedDates()
        {
            try
            {
                _context.OccupiedDatesResult = await _context.BookingManager
                    .GetFullyOccupiedDates(_context.OccupiedQueryStart, _context.OccupiedQueryEnd);
            }
            catch (Exception ex)
            {
                _context.ThrownException = ex;
            }
        }

        // ── Then ───────────────────────────────────────────────────────────────

        [Then(@"the result should contain exactly (\d+) dates")]
        public void ThenTheResultShouldContainExactlyDates(int expectedCount)
        {
            Assert.Null(_context.ThrownException);
            Assert.NotNull(_context.OccupiedDatesResult);
            Assert.Equal(expectedCount, _context.OccupiedDatesResult!.Count);
        }

        [Then(@"all dates from ""(.*)"" to ""(.*)"" should be in the result")]
        public void ThenAllDatesFromToShouldBeInTheResult(string startToken, string endToken)
        {
            var start = BookingTestContext.ParseDate(startToken);
            var end = BookingTestContext.ParseDate(endToken);

            for (var d = start; d <= end; d = d.AddDays(1))
            {
                Assert.Contains(d, _context.OccupiedDatesResult!);
            }
        }
    }
}
