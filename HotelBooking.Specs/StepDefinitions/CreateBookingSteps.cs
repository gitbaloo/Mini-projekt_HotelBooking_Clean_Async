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
        private readonly BookingTestContext _ctx;

        public CreateBookingSteps(BookingTestContext ctx) => _ctx = ctx;

        // ── Given ──────────────────────────────────────────────────────────────

        [Given(@"a customer wants to book from ""(.*)"" to ""(.*)""")]
        public void GivenACustomerWantsToBookFromTo(string startToken, string endToken)
        {
            _ctx.LastBooking = new Booking
            {
                StartDate = BookingTestContext.ParseDate(startToken),
                EndDate = BookingTestContext.ParseDate(endToken),
                CustomerId = 1
            };

            // If the SUT hasn't been built yet (e.g. "no rooms" scenario sets rooms
            // before calling this step), build it now.
            if (_ctx.BookingManager == null)
                _ctx.BuildBookingManager();
        }

        // ── When ───────────────────────────────────────────────────────────────

        [When(@"the booking is submitted")]
        public async Task WhenTheBookingIsSubmitted()
        {
            try
            {
                _ctx.CreateBookingResult = await _ctx.BookingManager.CreateBooking(_ctx.LastBooking!);
            }
            catch (Exception ex)
            {
                _ctx.ThrownException = ex;
            }
        }

        // ── Then ───────────────────────────────────────────────────────────────

        [Then(@"the booking should be created successfully")]
        public void ThenTheBookingShouldBeCreatedSuccessfully()
        {
            Assert.Null(_ctx.ThrownException);
            Assert.True(_ctx.CreateBookingResult,
                "Expected CreateBooking to return true, but it returned false.");
        }

        [Then(@"the booking should not be created")]
        public void ThenTheBookingShouldNotBeCreated()
        {
            Assert.Null(_ctx.ThrownException);
            Assert.False(_ctx.CreateBookingResult,
                "Expected CreateBooking to return false, but it returned true.");
        }

        [Then(@"the booking should be marked as active")]
        public void ThenTheBookingShouldBeMarkedAsActive()
        {
            Assert.True(_ctx.LastBooking!.IsActive,
                "Expected the booking's IsActive flag to be true.");
        }

        [Then(@"the booking should not be marked as active")]
        public void ThenTheBookingShouldNotBeMarkedAsActive()
        {
            Assert.False(_ctx.LastBooking!.IsActive,
                "Expected the booking's IsActive flag to be false after rejection.");
        }

        [Then(@"the booking should be assigned a valid room")]
        public void ThenTheBookingShouldBeAssignedAValidRoom()
        {
            Assert.Contains(_ctx.Rooms, r => r.Id == _ctx.LastBooking!.RoomId);

            // The assigned room must not have any active booking that overlaps the new booking's period.
            var overlaps = _ctx.Bookings.Where(b =>
                b.IsActive &&
                b.RoomId == _ctx.LastBooking!.RoomId &&
                !(b.EndDate < _ctx.LastBooking.StartDate || b.StartDate > _ctx.LastBooking.EndDate));

            Assert.Empty(overlaps);
        }

        [Then(@"no room should be assigned to the booking")]
        public void ThenNoRoomShouldBeAssignedToTheBooking()
        {
            // RoomId stays at its default (0) when CreateBooking returns false
            Assert.Equal(0, _ctx.LastBooking!.RoomId);
        }
    }
}
