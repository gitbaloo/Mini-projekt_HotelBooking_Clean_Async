using HotelBooking.Core;
using HotelBooking.Specs.Support;
using TechTalk.SpecFlow;

namespace HotelBooking.Specs.StepDefinitions
{
    /// <summary>
    /// Step definitions that are shared across feature files (Background tables, etc.).
    /// </summary>
    [Binding]
    public sealed class SharedSteps(BookingTestContext context)
    {
        private readonly BookingTestContext _context = context;

        // ── Background: rooms ──────────────────────────────────────────────────

        [Given(@"the hotel has the following rooms:")]
        public void GivenTheHotelHasTheFollowingRooms(Table table)
        {
            _context.Rooms = [.. table.Rows
                .Select(row => new Room
                {
                    Id = int.Parse(row["Id"]),
                    Description = row["Description"]
                })];
        }

        [Given(@"the hotel has no rooms")]
        public void GivenTheHotelHasNoRooms()
        {
            _context.Rooms = [];
            _context.BuildBookingManager();
        }

        // ── Background: bookings ───────────────────────────────────────────────

        [Given(@"the following active bookings exist:")]
        public void GivenTheFollowingActiveBookingsExist(Table table)
        {
            _context.Bookings = [.. table.Rows
                .Select(row => new Booking
                {
                    Id = int.Parse(row["Id"]),
                    StartDate = BookingTestContext.ParseDate(row["StartDate"]),
                    EndDate = BookingTestContext.ParseDate(row["EndDate"]),
                    IsActive = true,
                    CustomerId = int.Parse(row["CustomerId"]),
                    RoomId = int.Parse(row["RoomId"])
                })];

            // Rebuild the SUT now that both rooms and bookings are ready.
            _context.BuildBookingManager();
        }

        [Given(@"there are no bookings in the system")]
        public void GivenThereAreNoBookingsInTheSystem()
        {
            _context.Bookings = [];
            _context.BuildBookingManager();
        }

        // ── BB-01: missing start date ──────────────────────────────────────────

        [Given(@"a customer has accidentally entered ""NULL"" by leaving out start date")]
        public void GivenACustomerHasLeftOutStartDate()
        {
            // DateTime.MinValue is far in the past — FindAvailableRoom throws ArgumentException
            _context.LastBooking = new Booking
            {
                StartDate = DateTime.MinValue,
                EndDate = DateTime.Today.AddDays(2),
                CustomerId = 1
            };

            if (_context.BookingManager == null)
                _context.BuildBookingManager();
        }

        // ── BB-02: missing end date ────────────────────────────────────────────

        [Given(@"a customer has accidentally entered ""NULL"" by leaving out end date")]
        public void GivenACustomerHasLeftOutEndDate()
        {
            // DateTime.MinValue as end date is before start — FindAvailableRoom throws ArgumentException
            _context.LastBooking = new Booking
            {
                StartDate = DateTime.Today.AddDays(2),
                EndDate = DateTime.MinValue,
                CustomerId = 1
            };

            if (_context.BookingManager == null)
                _context.BuildBookingManager();
        }

        // ── Common assertions ──────────────────────────────────────────────────

        // Used by Scenario Outline steps (BB-03, BB-06)
        [Then(@"an ArgumentException should be thrown")]
        public void ThenAnArgumentExceptionShouldBeThrown()
        {
            Assert.NotNull(_context.ThrownException);
            Assert.IsType<ArgumentException>(_context.ThrownException);
        }

        // Used by plain Scenario steps (BB-01, BB-02)
        [Then(@"an ArgumentException is thrown")]
        public void ThenAnArgumentExceptionIsThrown()
        {
            Assert.NotNull(_context.ThrownException);
            Assert.IsType<ArgumentException>(_context.ThrownException);
        }
    }
}
