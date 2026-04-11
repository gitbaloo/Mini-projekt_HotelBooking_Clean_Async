using HotelBooking.Core;
using HotelBooking.Specs.Support;
using TechTalk.SpecFlow;

namespace HotelBooking.Specs.StepDefinitions
{
    /// <summary>
    /// Step definitions specific to the "Create Booking" feature.
    /// </summary>
    [Binding]
    public sealed class CreateBookingSteps
    {
        private readonly BookingTestContext _context;

        public CreateBookingSteps(BookingTestContext context) => _context = context;

        // ── Given ──────────────────────────────────────────────────────────────

        [Given(@"a customer wants to book from ""(.*)"" to ""(.*)""")]
        public void GivenACustomerWantsToBookFromTo(string startToken, string endToken)
        {
            _context.LastBooking = new Booking
            {
                StartDate = BookingTestContext.ParseDate(startToken),
                EndDate = BookingTestContext.ParseDate(endToken),
                CustomerId = 1
            };

            // If the SUT hasn't been built yet (e.g. "no rooms" scenario sets rooms
            // before calling this step), build it now.
            if (_context.BookingManager == null)
                _context.BuildBookingManager();
        }

        // ── When ───────────────────────────────────────────────────────────────

        [When(@"the booking is submitted")]
        public async Task WhenTheBookingIsSubmitted()
        {
            try
            {
                _context.CreateBookingResult = await _context.BookingManager.CreateBooking(_context.LastBooking!);
            }
            catch (Exception ex)
            {
                _context.ThrownException = ex;
            }
        }

        // ── Then ───────────────────────────────────────────────────────────────

        [Then(@"the booking should be created successfully")]
        public void ThenTheBookingShouldBeCreatedSuccessfully()
        {
            Assert.Null(_context.ThrownException);
            Assert.True(_context.CreateBookingResult,
                "Expected CreateBooking to return true, but it returned false.");
        }

        [Then(@"the booking should not be created")]
        public void ThenTheBookingShouldNotBeCreated()
        {
            // A booking is "not created" either when CreateBooking returns false,
            // OR when invalid input causes an ArgumentException (e.g. BB-04: start date is today).
            if (_context.ThrownException != null)
            {
                Assert.IsType<ArgumentException>(_context.ThrownException);
                return;
            }

            Assert.False(_context.CreateBookingResult,
                "Expected CreateBooking to return false, but it returned true.");
        }

        [Then(@"the booking should be marked as active")]
        public void ThenTheBookingShouldBeMarkedAsActive()
        {
            Assert.True(_context.LastBooking!.IsActive,
                "Expected the booking's IsActive flag to be true.");
        }

        [Then(@"the booking should not be marked as active")]
        public void ThenTheBookingShouldNotBeMarkedAsActive()
        {
            Assert.False(_context.LastBooking!.IsActive,
                "Expected the booking's IsActive flag to be false after rejection.");
        }

        [Then(@"the booking should be assigned a valid room")]
        public void ThenTheBookingShouldBeAssignedAValidRoom()
        {
            Assert.Contains(_context.Rooms, r => r.Id == _context.LastBooking!.RoomId);

            // The assigned room must not have any active booking that overlaps the new booking's period.
            var overlaps = _context.Bookings.Where(b =>
                b.IsActive &&
                b.RoomId == _context.LastBooking!.RoomId &&
                !(b.EndDate < _context.LastBooking.StartDate || b.StartDate > _context.LastBooking.EndDate));

            Assert.Empty(overlaps);
        }

        [Then(@"no room should be assigned to the booking")]
        public void ThenNoRoomShouldBeAssignedToTheBooking()
        {
            // RoomId stays at its default (0) when CreateBooking returns false
            Assert.Equal(0, _context.LastBooking!.RoomId);
        }
    }
}
