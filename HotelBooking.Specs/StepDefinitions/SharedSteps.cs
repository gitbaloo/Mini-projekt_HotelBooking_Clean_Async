using HotelBooking.Core;
using HotelBooking.Specs.Support;
using TechTalk.SpecFlow;

namespace HotelBooking.Specs.StepDefinitions
{
    /// <summary>
    /// Step definitions that are shared across feature files (Background tables, etc.).
    /// </summary>
    [Binding]
    public sealed class SharedSteps
    {
        private readonly BookingTestContext _ctx;

        public SharedSteps(BookingTestContext ctx) => _ctx = ctx;

        // ── Background: rooms ──────────────────────────────────────────────────

        [Given(@"the hotel has the following rooms:")]
        public void GivenTheHotelHasTheFollowingRooms(Table table)
        {
            _ctx.Rooms = table.Rows
                .Select(row => new Room
                {
                    Id = int.Parse(row["Id"]),
                    Description = row["Description"]
                })
                .ToList();
        }

        [Given(@"the hotel has no rooms")]
        public void GivenTheHotelHasNoRooms()
        {
            _ctx.Rooms = new List<Room>();
        }

        // ── Background: bookings ───────────────────────────────────────────────

        [Given(@"the following active bookings exist:")]
        public void GivenTheFollowingActiveBookingsExist(Table table)
        {
            _ctx.Bookings = table.Rows
                .Select(row => new Booking
                {
                    Id = int.Parse(row["Id"]),
                    StartDate = BookingTestContext.ParseDate(row["StartDate"]),
                    EndDate = BookingTestContext.ParseDate(row["EndDate"]),
                    IsActive = true,
                    CustomerId = int.Parse(row["CustomerId"]),
                    RoomId = int.Parse(row["RoomId"])
                })
                .ToList();

            // Rebuild the SUT now that both rooms and bookings are ready.
            _ctx.BuildBookingManager();
        }

        [Given(@"there are no bookings in the system")]
        public void GivenThereAreNoBookingsInTheSystem()
        {
            _ctx.Bookings = new List<Booking>();
            _ctx.BuildBookingManager();
        }

        // ── Common assertion ───────────────────────────────────────────────────

        [Then(@"an ArgumentException should be thrown")]
        public void ThenAnArgumentExceptionShouldBeThrown()
        {
            Assert.NotNull(_ctx.ThrownException);
            Assert.IsType<ArgumentException>(_ctx.ThrownException);
        }
    }
}
